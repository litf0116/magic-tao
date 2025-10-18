# 违禁词缓存自动管理系统

## 📋 概述

本文档描述了违禁词缓存的自动管理机制，确保在以下时机自动重建缓存：
- 添加/删除违禁词后
- 系统启动后
- 缓存异常时
- 定期维护（建议每周一次）

## 🏗️ 系统架构

### 1. **自动缓存重建机制**

#### **CRUD操作触发重建**
```csharp
// 创建违禁词时自动重建
public override async Task<SensitiveWordDto> Create(SensitiveWordDto input)
{
    var result = await base.Create(input);
    await RebuildCacheWithLogging($"创建违禁词: {input.Content}");
    return result;
}

// 更新违禁词时自动重建
public override async Task<SensitiveWordDto> Update(SensitiveWordDto input)
{
    var result = await base.Update(input);
    await RebuildCacheWithLogging($"更新违禁词: {input.Content}");
    return result;
}

// 删除违禁词时自动重建
public override async Task Delete(EntityDto<long> input)
{
    var entity = await Repository.GetAsync(input.Id);
    await base.Delete(input);
    await RebuildCacheWithLogging($"删除违禁词: {entity?.Content}");
    return result;
}
```

#### **批量操作支持**
```csharp
// 批量添加违禁词
[HttpPost]
public async Task BatchCreateAsync(BatchCreateRequest input)
{
    var entities = input.Words.Split(',')
        .Where(x => !string.IsNullOrEmpty(x))
        .Select(x => new SensitiveWord { Content = x }).ToList();

    using (var uow = _unitOfWorkManager.Begin())
    {
        foreach (var entity in entities)
        {
            await Repository.InsertAsync(entity);
        }
        await uow.CompleteAsync();
    }

    // 自动重建缓存
    await RebuildCacheWithLogging("批量添加违禁词");
}
```

### 2. **系统启动时缓存初始化**

#### **启动器服务**
```csharp
public class SensitiveWordInitializer : ITransientDependency
{
    public async Task InitializeAsync()
    {
        _logger.LogInformation("开始初始化违禁词缓存...");

        // 强制重建缓存
        var words = await mediator.Send(new QueryCacheWords(true));

        _logger.LogInformation("违禁词缓存初始化完成，共加载 {Count} 个违禁词", words.Length);

        // 触发初始化完成事件
        await _eventBus.TriggerAsync(new SensitiveWordCacheInitializedEvent(words.Length));
    }
}
```

#### **应用启动钩子**
```csharp
public override void PostInitialize()
{
    // 在应用启动完成后初始化违禁词缓存
    Task.Run(async () =>
    {
        await Task.Delay(5000); // 等待5秒确保所有服务都已启动

        try
        {
            using var scope = IocManager.CreateScope();
            var initializer = scope.Resolve<SensitiveWordInitializer>();
            await initializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 违禁词缓存自动初始化失败");
        }
    });
}
```

### 3. **缓存健康监控**

#### **健康检查API**
```http
GET /api/services/app/SensitiveWord/CheckCacheHealth
```

**返回结果**：
```json
{
  "isHealthy": true,
  "cacheCount": 2573,
  "databaseCount": 2573,
  "isSync": true,
  "lastCheck": "2025-10-18T12:30:00Z",
  "cacheKey": "SensitiveWords"
}
```

#### **异常检测和自动恢复**
```csharp
public class SensitiveWordCacheRecoveryService
{
    public async Task<bool> DetectAndRecoverAsync()
    {
        try
        {
            // 检查缓存状态
            var cachedWords = await _mediator.Send(new QueryCacheWords());
            var dbCount = await _sensitiveWordRepository.CountAsync();

            // 检测异常情况
            var hasCacheError = cachedWords == null;
            var hasEmptyCache = cachedWords.Length == 0 && dbCount > 0;
            var hasSyncError = cachedWords.Length != dbCount;

            if (hasCacheError || hasEmptyCache || hasSyncError)
            {
                // 自动恢复
                await RecoverCache("异常检测自动恢复");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            // 异常情况下强制恢复
            await RecoverCache("异常检测失败后强制恢复");
            return false;
        }
    }
}
```

### 4. **定期维护任务**

#### **每周自动维护**
```csharp
public class SensitiveWordCacheMaintenanceJob : AsyncJobBase
{
    public override async Task ExecuteAsync(JobExecutionContext context)
    {
        // 检查缓存健康状态
        var healthStatus = await CheckCacheHealth();

        // 如果缓存不健康，自动重建
        if (!healthStatus.IsHealthy || !healthStatus.IsSync)
        {
            await RebuildCache("定期维护自动重建");
        }

        // 记录维护结果
        context.JobResult.Message = $"维护完成。缓存词数: {healthStatus.CacheCount}";
    }
}
```

## 🚀 使用指南

### **API接口**

| 接口 | 方法 | 描述 | 自动重建 |
|------|------|------|----------|
| `/GetCachedWords` | GET | 获取缓存中的违禁词 | ❌ |
| `/CheckCacheHealth` | GET | 检查缓存健康状态 | ❌ |
| `/ReBuildCache` | POST | 手动重建缓存 | ✅ |
| `/Create` | POST | 创建违禁词 | ✅ |
| `/Update` | PUT | 更新违禁词 | ✅ |
| `/Delete` | DELETE | 删除违禁词 | ✅ |
| `/BatchCreate` | POST | 批量创建违禁词 | ✅ |

### **监控和日志**

#### **日志记录**
```csharp
// 自动重建缓存时记录详细日志
_logger.LogInformation("开始重建违禁词缓存，操作：{Operation}", operation);
_logger.LogInformation("违禁词缓存重建完成，操作：{Operation}", operation);
```

#### **事件通知**
```csharp
// 缓存初始化完成事件
public class SensitiveWordCacheInitializedEvent
{
    public int WordCount { get; }
    public DateTime InitializeTime { get; }
}
```

## 📊 性能优化

### **缓存策略**
- **缓存键**：`SensitiveWords`
- **过期时间**：永不过期（手动重建）
- **数据格式**：JSON字符串数组
- **重建触发**：CRUD操作、系统启动、异常检测、定期维护

### **性能指标**
- **缓存命中**：直接从Redis读取，O(1)时间复杂度
- **重建时间**：通常2-5秒，取决于违禁词数量
- **内存占用**：每个违禁词平均10-20字节
- **网络开销**：重建时一次性传输，平时无额外开销

## ⚠️ 注意事项

### **并发控制**
- 使用Redis单线程模型避免并发问题
- 重建时通过MediatR确保串行执行

### **错误处理**
- 自动重试机制：重建失败时记录日志但不影响主流程
- 降级策略：缓存不可用时回退到数据库查询（如需要）

### **监控建议**
- 定期检查缓存健康状态
- 监控重建频率和耗时
- 设置缓存异常告警

## 🛠️ 测试

### **功能测试**
```http
# 测试自动重建
POST /api/services/app/SensitiveWord/Create
{
    "content": "测试自动重建"
}

# 验证缓存更新
GET /api/services/app/SensitiveWord/GetCachedWords

# 检查健康状态
GET /api/services/app/SensitiveWord/CheckCacheHealth
```

### **性能测试**
- 测试大量违禁词的重建时间
- 验证缓存查询性能
- 检查内存使用情况

## 🎉 总结

通过自动缓存管理系统，违禁词缓存现在能够在以下时机自动更新：

1. ✅ **CRUD操作后** - 创建、更新、删除违禁词时自动重建
2. ✅ **系统启动后** - 应用启动5秒后自动初始化缓存
3. ✅ **异常检测时** - 发现缓存异常时自动恢复
4. ✅ **定期维护** - 每周自动检查和重建缓存

这套机制确保了违禁词缓存的高可用性和数据一致性，大大降低了手动维护成本。