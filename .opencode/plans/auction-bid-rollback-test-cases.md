# 拍卖报价撤回功能测试用例

## 1. 文档信息

- **创建日期**: 2026-05-05
- **版本**: v1.0
- **功能模块**: 拍卖报价撤回
- **修复问题**: 撤回报价后价格回退不正确导致用户无法正常报价

## 2. 测试环境

- **后端服务**: http://localhost:12580
- **数据库**: MySQL (www_molitao_top)
- **Redis**: 127.0.0.1:6379
- **测试用户**: userId=14

## 3. 功能说明

### 3.1 业务场景

拍卖报价撤回功能允许管理员撤销用户的出价记录。撤销后：
- 该出价记录标记为 `IsRollBack=true`
- 拍卖品价格回退到上一个有效出价
- 用户可以基于回退后的价格重新报价

### 3.2 修复内容

**问题**: 撤回出价后，查询最新价格时没有过滤 `IsRollBack=true` 的记录，导致价格回退不正确，用户无法正常报价。

**修复**: 在所有查询 BidHistory 的地方添加 `.Where(w => !w.IsRollBack)` 过滤条件。

**影响范围**:
- `AuctionItemCacheManager.cs`: 3处
- `AuctionItemAppService.cs`: 2处

---

## 4. API 接口列表

| 接口 | 方法 | 路径 | 说明 |
|------|------|------|------|
| 获取拍卖品详情 | GET | /api/AuctionItem/GetDetail?id={id} | 获取拍卖品信息及出价历史 |
| 用户报价 | POST | /api/services/app/AuctionItem/Bid | 用户对拍卖品出价 |
| 管理员撤回 | POST | /api/services/app/Websocket/backout | 撤回出价消息 |
| 查询出价历史 | GET | /api/services/app/BidHistory/GetAll | 查询拍卖品出价历史 |
| 生成用户Token | POST | /api/TokenAuth/GenerateTokenForUser | 管理员为指定用户生成token |

---

## 5. 测试用例

### 5.1 基础功能测试

#### TC-001: 正常报价流程

**前置条件**:
- 拍卖品状态为"拍卖中"
- 用户已登录
- 用户有报价资格

**测试步骤**:
```bash
# 1. 获取拍卖品详情
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer 用户token"

# 记录当前价格: currentPrice

# 2. 用户报价（当前价格+20）
curl -X POST "http://localhost:12580/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer 用户token" \
  -H "Content-Type: application/json" \
  -d '{
    "auctionItemId": 拍卖品ID,
    "bidPrice": 新价格,
    "bidUserId": 用户ID
  }'

# 3. 验证报价成功
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer 用户token"
```

**预期结果**:
- ✅ 报价接口返回 `success: true`
- ✅ 拍卖品 `currentPrice` 更新为新报价
- ✅ 出价历史中新增一条记录，`IsRollBack=false`

**实际结果**: _待填写_

---

#### TC-002: 管理员撤回报价

**前置条件**:
- 已完成 TC-001
- 有管理员权限或使用管理员token

**测试步骤**:
```bash
# 1. 查询最新出价消息ID
curl -X GET "http://localhost:12580/api/services/app/Message/GetAll?auctionItemId=拍卖品ID&maxResultCount=1" \
  -H "Authorization: Bearer 管理员token"

# 记录消息ID: messageId

# 2. 管理员撤回出价
curl -X POST "http://localhost:12580/api/services/app/Websocket/backout" \
  -H "Authorization: Bearer 管理员token" \
  -H "Content-Type: application/json" \
  -d '{"id": "消息ID"}'

# 3. 验证价格回退
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer 用户token"
```

**预期结果**:
- ✅ 撤回接口返回 `success: true`
- ✅ 拍卖品 `currentPrice` 回退到上一个有效报价
- ✅ 出价历史中该记录 `IsRollBack=true`
- ✅ 缓存已刷新

**实际结果**: _待填写_

---

#### TC-003: 撤回后重新报价（核心测试）

**前置条件**:
- 已完成 TC-002
- 价格已回退

**测试步骤**:
```bash
# 假设价格流程：起拍价100 → 用户A报价120 → 撤回 → 回退到100

# 1. 用户重新报价110（介于回退价和撤回价之间）
curl -X POST "http://localhost:12580/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer 用户token" \
  -H "Content-Type: application/json" \
  -d '{
    "auctionItemId": 拍卖品ID,
    "bidPrice": 110,
    "bidUserId": 用户ID
  }'

# 2. 验证报价成功
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer 用户token"
```

**预期结果**:
- ✅ 报价接口返回 `success: true`（修复前会失败）
- ✅ `currentPrice` 更新为110
- ✅ 出价历史中新增有效记录

**实际结果**: _待填写_

**关键验证点**: 
- 修复前：报价110时会提示"价格必须大于120"（错误行为）
- 修复后：报价110成功，因为当前价格是100（正确行为）

---

### 5.2 边界场景测试

#### TC-004: 连续多次撤回

**测试目的**: 验证连续撤回多个报价时价格正确回退

**测试步骤**:
```bash
# 价格流程：起拍价100 → 用户A报价110 → 用户B报价120 → 用户C报价130

# 1. 撤回用户C的报价130
# 预期：价格回退到120

# 2. 撤回用户B的报价120
# 预期：价格回退到110

# 3. 用户D报价115
# 预期：报价成功（修复前会失败，认为价格还是120）
```

**预期结果**:
- ✅ 每次撤回后价格正确回退到上一个有效报价
- ✅ 重新报价成功

**实际结果**: _待填写_

---

#### TC-005: 撤回第一条报价

**测试目的**: 验证撤回第一条报价后价格回退到起拍价

**测试步骤**:
```bash
# 价格流程：起拍价100 → 用户A报价110

# 1. 撤回用户A的报价110
# 预期：价格回退到起拍价100

# 2. 用户B报价105
# 预期：报价成功
```

**预期结果**:
- ✅ 价格回退到起拍价
- ✅ 可以基于起拍价重新报价

**实际结果**: _待填写_

---

#### TC-006: 撤回后原用户再次报价

**测试目的**: 验证被撤回的用户可以再次报价

**测试步骤**:
```bash
# 价格流程：起拍价100 → 用户A报价110 → 撤回 → 回退到100

# 1. 用户A再次报价115
curl -X POST "http://localhost:12580/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer 用户Atoken" \
  -H "Content-Type: application/json" \
  -d '{
    "auctionItemId": 拍卖品ID,
    "bidPrice": 115,
    "bidUserId": 用户A_ID
  }'
```

**预期结果**:
- ✅ 报价成功
- ✅ 用户A的最新有效报价是115

**实际结果**: _待填写_

---

### 5.3 数据一致性测试

#### TC-007: 数据库验证

**测试目的**: 验证数据库中 `IsRollBack` 标记正确

**测试步骤**:
```sql
-- 1. 查询拍卖品的所有出价历史
SELECT 
    Id, 
    AuctionItemId, 
    BidPrice, 
    BidUserName, 
    IsRollBack, 
    CreationTime 
FROM t_bid_history 
WHERE AuctionItemId = 拍卖品ID
ORDER BY CreationTime DESC;

-- 2. 验证被撤回的记录
-- IsRollBack = 1 表示已撤回
-- IsRollBack = 0 或 NULL 表示有效

-- 3. 统计有效出价数量
SELECT COUNT(*) as valid_bid_count
FROM t_bid_history
WHERE AuctionItemId = 拍卖品ID
  AND (IsRollBack = 0 OR IsRollBack IS NULL);

-- 4. 查询最新有效出价
SELECT TOP 1 *
FROM t_bid_history
WHERE AuctionItemId = 拍卖品ID
  AND (IsRollBack = 0 OR IsRollBack IS NULL)
ORDER BY CreationTime DESC;
```

**预期结果**:
- ✅ 被撤回的记录 `IsRollBack = 1`
- ✅ 有效记录 `IsRollBack = 0` 或 `NULL`
- ✅ 最新有效出价价格与 `currentPrice` 一致

**实际结果**: _待填写_

---

#### TC-008: 缓存一致性验证

**测试目的**: 验证撤回后缓存正确刷新

**测试步骤**:
```bash
# 1. 撤回前查看拍卖品详情
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer token"

# 记录 currentPrice

# 2. 执行撤回操作

# 3. 撤回后立即查看拍卖品详情
curl -X GET "http://localhost:12580/api/AuctionItem/GetDetail?id=拍卖品ID" \
  -H "Authorization: Bearer token"

# 验证 currentPrice 是否正确回退
```

**预期结果**:
- ✅ 缓存立即刷新
- ✅ `currentPrice` 正确回退
- ✅ 后续查询返回一致的价格

**实际结果**: _待填写_

---

### 5.4 并发场景测试

#### TC-009: 撤回时其他用户报价

**测试目的**: 验证撤回操作与其他用户报价的并发处理

**测试步骤**:
```bash
# 场景：用户A报价110 → 准备撤回 → 同时用户B报价120

# 1. 用户A报价110
# 2. 并发执行：
#    - 管理员撤回用户A的报价110
#    - 用户B报价120
# 3. 验证最终状态
```

**预期结果**:
- ✅ 撤回操作和报价操作都能成功
- ✅ 最终价格正确（取决于执行顺序）
- ✅ 数据一致性保持

**实际结果**: _待填写_

---

### 5.5 权限测试

#### TC-010: 非管理员撤回测试

**测试目的**: 验证权限控制

**测试步骤**:
```bash
# 普通用户尝试撤回自己的报价
curl -X POST "http://localhost:12580/api/services/app/Websocket/backout" \
  -H "Authorization: Bearer 普通用户token" \
  -H "Content-Type: application/json" \
  -d '{"id": "消息ID"}'
```

**预期结果**:
- ✅ 返回权限错误："无权操作"
- ✅ 报价记录未被标记为撤回

**实际结果**: _待填写_

---

## 6. 回归测试

### 6.1 影响范围确认

修复影响以下功能，需回归测试：

| 功能模块 | 测试重点 | 优先级 |
|---------|---------|--------|
| 获取拍卖品详情 | 出价历史正确性 | 高 |
| 用户报价 | 价格验证逻辑 | 高 |
| 拍卖结束 | 最终价格计算 | 高 |
| 拍卖列表展示 | 价格显示正确 | 中 |
| 缓存管理 | 缓存刷新及时 | 高 |

---

## 7. 测试数据准备

### 7.1 创建测试拍卖品

```bash
# 使用管理员账号创建测试拍卖品
TOKEN=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | jq -r '.result.accessToken')

# 创建拍卖品（需要填写完整参数）
curl -X POST "http://localhost:12580/api/services/app/AuctionItem/Create" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "撤回功能测试商品",
    "description": "测试拍卖报价撤回功能",
    "imageUrl": "https://picsum.photos/400/300",
    "startingPrice": 100,
    "status": 1
  }'
```

### 7.2 测试账号

- **管理员**: userId=14
- **测试用户A**: userId=XXX
- **测试用户B**: userId=XXX

---

## 8. 测试执行记录

### 8.1 测试执行摘要

| 测试轮次 | 执行日期 | 执行人 | 环境状态 | 通过率 | 备注 |
|---------|---------|--------|---------|--------|------|
| 第1轮 | YYYY-MM-DD | XXX | 测试环境 | X% | 待执行 |
| 第2轮 | YYYY-MM-DD | XXX | 生产环境 | X% | 待执行 |

### 8.2 缺陷记录

| 缺陷ID | 标题 | 严重程度 | 状态 | 关联用例 |
|--------|------|---------|------|---------|
| - | - | - | - | - |

---

## 9. 测试结论

### 9.1 功能验证

- [ ] 报价撤回功能正常
- [ ] 价格正确回退
- [ ] 重新报价成功
- [ ] 数据一致性保持
- [ ] 缓存正确刷新

### 9.2 性能验证

- [ ] 撤回操作响应时间 < 1秒
- [ ] 缓存刷新及时
- [ ] 无性能退化

### 9.3 兼容性验证

- [ ] PC端功能正常
- [ ] 小程序功能正常
- [ ] H5功能正常

### 9.4 最终结论

**测试状态**: 待测试

**修复验证**: 
- [ ] 问题已修复
- [ ] 问题部分修复
- [ ] 问题未修复
- [ ] 引入新问题

**上线建议**: 
- [ ] 可以上线
- [ ] 需要修复后上线
- [ ] 不建议上线

---

## 10. 附录

### 10.1 相关文件

**修复文件**:
- `backend/src/TtWork.Project/Services/Cache/AuctionItemCacheManager.cs`
- `backend/src/TtWork.Project/Applications/Auctions/AuctionItemAppService.cs`

**相关事件**:
- `backend/src/TtWork.Project/Events/Commands/RollBackAuctionEvent.cs`

### 10.2 参考资料

- [ABP Framework 文档](https://docs.abp.io/)
- [Entity Framework Core 查询过滤](https://learn.microsoft.com/en-us/ef/core/querying/)
- 项目内部文档：`/Users/mac/workspace/magic-tao/.sisyphus/plans/fix-auction-deal-performance.md`

---

**文档版本历史**:

| 版本 | 日期 | 修改人 | 修改内容 |
|------|------|--------|---------|
| v1.0 | 2026-05-05 | Sisyphus | 初始版本，创建完整测试用例 |
