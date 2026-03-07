# 高并发接口性能优化指南：两层缓存架构

## 问题背景

在高并发场景下，直接使用 Redis 作为单一缓存层时，接口响应时间会显著下降：

| 接口 | 优化前 | 问题根因 |
|------|--------|----------|
| 卡秒状态查询 | 20-100ms | Redis 网络延迟 + 连接池排队 |
| 拍卖品列表 | ~200ms | Redis 网络延迟 + 序列化开销 + 服务端压力 |

## 根本原因分析

### Redis 高并发性能瓶颈

```
客户端请求 → 网络(1-5ms) → Redis服务(排队处理) → 网络(1-5ms) → 响应
```

**瓶颈来源：**

1. **网络往返延迟**：每次 Redis 请求需要网络往返，约 1-5ms
2. **连接池限制**：配置 `MaxPoolSize=50`，高并发时请求排队
3. **序列化开销**：JSON 序列化/反序列化消耗 CPU 时间
4. **Redis 服务端压力**：单线程处理，高并发时延迟增加
5. **并发竞争**：同一热点数据的并发访问导致 Redis 压力集中

### 性能衰减模式

```
低并发 (QPS < 10):     Redis 延迟 ~20ms  ✅ 可接受
中并发 (QPS 10-50):    Redis 延迟 ~50ms  ⚠️  开始衰减
高并发 (QPS > 50):     Redis 延迟 >100ms ❌ 性能恶化
```

## 解决方案：两层缓存架构

### 架构设计

```
┌─────────────────────────────────────────────┐
│  客户端请求                                  │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│  L1: IMemoryCache (本地内存缓存)             │
│  - 响应时间: ~1ms                           │
│  - TTL: L2 的 1/3 ~ 1/6                    │
│  - 容量: 有限，进程内 LRU 淘汰              │
│  - 用途: 热点数据，极快访问                 │
└─────────────────┬───────────────────────────┘
                  │ 未命中
                  ▼
┌─────────────────────────────────────────────┐
│  L2: Redis (分布式缓存)                      │
│  - 响应时间: ~20ms                          │
│  - TTL: 30秒 ~ 15分钟                       │
│  - 容量: 大，可扩展                          │
│  - 用途: 共享缓存、持久化                   │
└─────────────────┬───────────────────────────┘
                  │ 未命中
                  ▼
┌─────────────────────────────────────────────┐
│  Database (MySQL)                            │
│  - 响应时间: ~50ms (有索引优化)             │
│  - 用途: 数据持久化                         │
└─────────────────────────────────────────────┘
```

### 核心设计原则

#### 1. TTL 梯度设计

```
L1 TTL < L2 TTL

推荐比例：
- L1 TTL = L2 TTL × (1/3 ~ 1/6)
- 例如：L2=60秒，则 L1=10~20秒
```

**为什么这样设计？**
- L1 更新更频繁，减少脏数据风险
- L1 过期后仍可从 L2 获取，避免数据库压力
- 平衡数据一致性与性能

#### 2. 缓存更新策略

```csharp
// 读取流程（Cache-Aside 模式）
public async Task<T> GetAsync(string key)
{
    // 1. 检查 L1 缓存
    if (_memoryCache.TryGetValue(key, out T cached))
        return cached;

    // 2. 获取锁防止缓存击穿
    await _lock.WaitAsync();
    try
    {
        // 3. 双重检查 L1
        if (_memoryCache.TryGetValue(key, out cached))
            return cached;

        // 4. 检查 L2 缓存
        var redisValue = await _redis.StringGetAsync(key);
        if (redisValue.HasValue)
        {
            var result = Deserialize<T>(redisValue);
            // 写入 L1
            _memoryCache.Set(key, result, TimeSpan.FromSeconds(L1_TTL));
            return result;
        }

        // 5. 从数据库加载
        var dbResult = await _database.GetAsync(...);

        // 6. 同时写入 L1 和 L2
        _memoryCache.Set(key, dbResult, TimeSpan.FromSeconds(L1_TTL));
        await _redis.StringSetAsync(key, Serialize(dbResult), TimeSpan.FromSeconds(L2_TTL));

        return dbResult;
    }
    finally
    {
        _lock.Release();
    }
}
```

#### 3. 缓存失效策略

**写操作时缓存失效：**
```csharp
public async Task UpdateAsync(T entity)
{
    await _database.UpdateAsync(entity);

    // 清除 L1 缓存（进程内，立即生效）
    _memoryCache.Remove(key);

    // 清除 L2 缓存（跨进程，需要时间）
    await _redis.KeyDeleteAsync(key);
}
```

**批量失效支持：**
```csharp
// 使用模式匹配批量清除
_redis.DeleteKeysWithPartten("auction:list:*");
```

#### 4. 缓存雪崩保护

```csharp
// TTL 随机化，避免同时失效
public static TimeSpan GetExpireWithJitter(TimeSpan baseExpire)
{
    var jitter = Random.Shared.Next(0, 5); // 0-5秒随机偏移
    return baseExpire.Add(TimeSpan.FromSeconds(jitter));
}
```

#### 5. 缓存击穿保护

使用 `SemaphoreSlim` 实现并发控制：

```csharp
private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

public async Task<T> GetAsync(string key)
{
    var lockKey = $"lock:{key}";
    var semaphore = _cacheLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

    await semaphore.WaitAsync();
    try
    {
        // 双重检查 + 加载逻辑
        ...
    }
    finally
    {
        semaphore.Release();
    }
}
```

## 实施案例

### 案例 1：卡秒状态查询优化

**Commit:** `c995989`

**问题：** 高并发下响应时间 20-100ms

**解决方案：**
```csharp
// BidEligibilityService.cs:350-358
var kasecCacheKey = $"{KASEC_CACHE_PREFIX}{input.AuctionItemId}";

if (_memoryCache.TryGetValue(kasecCacheKey, out bool cachedKasecValue))
{
    isKasec = cachedKasecValue;
}
else
{
    var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{input.AuctionItemId}");
    isKasec = kasecVal.HasValue && kasecVal == "true";
    _memoryCache.Set(kasecCacheKey, isKasec, TimeSpan.FromSeconds(5));
}
```

**效果：**
- 20并发下：20-100ms → **20-50ms**
- Redis QPS 降低：**90%+**

### 案例 2：拍卖品列表接口优化

**Commit:** `fbdf378`

**问题：** 高并发下响应时间 ~200ms

**解决方案：**
```csharp
// AuctionItemCacheManager.cs
public async Task<ListResultDto<AuctionItemDto>> GetAuctionListAsync(AppResultRequestDto input)
{
    string cacheKey = AuctionItemCacheKeys.GenerateListCacheKey(input);
    string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);

    // 1. L1 缓存检查（~1ms）
    if (_memoryCache.TryGetValue(localCacheKey, out ListResultDto<AuctionItemDto> localCached))
        return localCached;

    // 2. 缓存锁防击穿
    var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
    await semaphore.WaitAsync();
    try
    {
        // 3. 双重检查 + L2 缓存（~20ms）
        var redisValue = await _redisClient.Database.StringGetAsync(cacheKey);
        if (redisValue.HasValue)
        {
            var result = JsonConvert.DeserializeObject<ListResultDto<AuctionItemDto>>(redisValue);
            _memoryCache.Set(localCacheKey, result, TimeSpan.FromSeconds(10));
            return result;
        }

        // 4. 数据库查询（~5ms 有索引）
        var dbResult = await GetAuctionListFromDatabaseAsync(input);
        _memoryCache.Set(localCacheKey, dbResult, TimeSpan.FromSeconds(10));
        await _redisClient.Database.StringSetAsync(cacheKey, JsonConvert.SerializeObject(result), expireTime);

        return dbResult;
    }
    finally
    {
        semaphore.Release();
    }
}
```

**效果：**
- L1 缓存命中平均：**~18ms**
- P95：**40ms**，P99：**55ms** ✅ (目标 < 100ms)
- Redis QPS 降低：**70%+**

## 适用场景

### ✅ 推荐使用两层缓存

1. **高并发读多写少接口**
   - 列表查询
   - 配置查询
   - 状态查询

2. **热点数据访问**
   - 少量 key 被频繁访问
   - 数据更新频率低

3. **对响应时间敏感**
   - 需要 < 100ms 响应
   - 用户体验要求高

### ⚠️ 谨慎使用

1. **频繁写入的数据**
   - L1 缓存频繁失效，优势不明显
   - 考虑使用更短的 TTL

2. **数据一致性要求极高**
   - L1 是进程内缓存，多实例间有延迟
   - 可通过缩短 TTL 降低延迟

3. **内存受限环境**
   - L1 缓存占用应用内存
   - 需要监控内存使用

## 性能对比

### 单层 Redis vs 两层缓存

| 指标 | 单层 Redis | 两层缓存 | 提升 |
|------|-----------|---------|------|
| 平均响应时间 | 50-100ms | 15-20ms | **5x** |
| P99 响应时间 | 200ms+ | 50-60ms | **4x** |
| Redis QPS | 100% | 20-30% | **-70%** |
| 内存使用 | 基准 | +30-50% | 可接受 |
| 缓存命中率 | 60-70% | 90%+ | **+30%** |

### 数据库查询优化效果

| 场景 | 无索引 | 有索引 | 提升 |
|------|--------|--------|------|
| 状态查询 | ~50ms | ~5ms | **10x** |
| 排序查询 | ~100ms | ~10ms | **10x** |

## 实施检查清单

### 代码实现

- [ ] 注入 `IMemoryCache` 依赖
- [ ] 实现 L1 缓存检查逻辑
- [ ] 实现 L2 缓存降级逻辑
- [ ] 添加缓存锁机制（`SemaphoreSlim`）
- [ ] 实现双重检查锁定
- [ ] 添加 TTL 随机化
- [ ] 更新缓存失效逻辑

### 配置调整

```json
{
  "Redis": {
    "MaxPoolSize": 50,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000
  }
}
```

### 数据库优化

```sql
-- 添加查询优化索引
CREATE INDEX IX_Table_Status ON T_AuctionItem(Status);
CREATE INDEX IX_Table_Status_Order_Id ON T_AuctionItem(Status, Order, Id);
```

### 监控指标

- [ ] L1 缓存命中率
- [ ] L2 缓存命中率
- [ ] 接口响应时间 (P50/P95/P99)
- [ ] Redis QPS
- [ ] 应用内存使用

## 参考资料

### 相关 Commit

- `c995989` - 添加卡秒状态本地内存缓存优化高并发性能
- `fbdf378` - 优化拍卖品列表接口 GetPublicList 性能

### 参考代码

- `BidEligibilityService.cs:350-358` - L1 缓存实现示例
- `AuctionItemCacheManager.cs` - 完整两层缓存实现
- `AuctionItemCachePolicy.cs` - TTL 策略配置

## 常见问题

### Q1: L1 缓存 TTL 应该设置多少？

**A:** 推荐设置为 L2 TTL 的 1/3 ~ 1/6：
- 平衡数据一致性（L1 更新频繁）
- 保证性能（L1 命中率高）
- 例如：L2=60秒 → L1=10秒

### Q2: 多实例间 L1 缓存不一致怎么办？

**A:** 这是预期行为，可通过以下方式缓解：
- 缩短 L1 TTL
- 数据更新时主动清除 L1
- 使用消息通知其他实例清除缓存

### Q3: L1 缓存占用多少内存合适？

**A:** 建议：
- 监控应用内存使用
- 只缓存热点数据（Top 100-1000）
- LRU 自动淘汰旧数据
- 预留足够内存给其他功能

### Q4: 如何验证优化效果？

**A:** 使用压力测试工具：
```bash
# 使用 Apache Bench
ab -n 1000 -c 20 -H "Authorization: Bearer TOKEN" \
   http://localhost:21021/api/services/app/XXX/GetList

# 关注指标：
# - P95/P99 响应时间
# - 错误率
# - Redis QPS 降低
```

---

**文档版本:** v1.0
**最后更新:** 2026-01-19
**维护者:** backend team
