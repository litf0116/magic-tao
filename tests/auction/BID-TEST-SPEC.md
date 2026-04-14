# 竞拍流程测试规范

## 1. 测试概述

### 1.1 测试目标
验证拍卖竞拍功能的完整性，包括：
- 竞拍出价 (Bid)
- 竞拍商品详情查询 (GetDetail)
- 竞拍商品列表查询 (GetAll / GetPublicListAnonymous)
- 竞拍开始 (StartAuction) - 管理员权限
- 竞拍结束 (EndAuction) - 管理员权限
- 保证金检查
- 竞拍资格验证
- 加价阶梯规则验证

### 1.2 测试范围

| 模块 | 测试内容 | 测试类型 |
|------|---------|---------|
| 拍卖 API | 出价接口 | API 测试 |
| 业务逻辑 | 价格更新、资格检查 | 业务测试 |
| 数据库 | 竞拍记录、商品状态 | 数据验证 |

### 1.3 测试环境

```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
Redis: 127.0.0.1:6379
```

## 2. 测试账号

### 2.1 竞拍者账号
```
用户名: feifei
密码: 123456
用户ID: 7509
显示名: 粑粑&贝伊果我
租户ID: 1
```

### 2.2 管理员账号
```
用户名: admin
密码: 123456
权限: Pages.ChatManager (竞拍管理权限)
```

### 2.3 登录获取Token

**登录API**:
```bash
POST http://localhost:12580/api/TokenAuth/Authenticate
Content-Type: application/json

{
  "userNameOrEmailAddress": "feifei",
  "password": "123456"
}
```

**响应示例**:
```json
{
  "result": {
    "accessToken": "eyJhbGci...",
    "expireInSeconds": 604800,
    "userId": 7509,
    "refreshToken": "eyJhbGci..."
  },
  "success": true
}
```

**使用Token**:
```bash
Authorization: Bearer {accessToken}
```

## 3. 竞拍API接口 (实测验证)

### 3.1 获取竞拍商品列表 (匿名)

```bash
GET http://localhost:12580/api/AuctionItem/GetPublicListAnonymous
# 无需认证，任何人都可访问
```

**注意**: `/api/services/app/AuctionItem/GetAll` 需要登录认证

### 3.2 获取竞拍商品详情

```bash
GET http://localhost:12580/api/AuctionItem/GetDetail?id={auctionItemId}
Authorization: Bearer {token}
```

**响应字段**:
- `id`: 商品ID (long)
- `name`: 商品名称
- `status`: 拍卖状态 (字符串: "待拍卖" / "拍卖中" / "已成交" 等)
- `startingPrice`: 起拍价 (int)
- `currentPrice`: 当前价 (int?)
- `currentPriceUserId`: 当前最高出价用户ID (long?)
- `currentPriceUserName`: 当前最高出价用户名
- `dealUserId`: 成交用户ID (long?)
- `dealUserName`: 成交用户名
- `finalPrice`: 成交价 (int?)
- `toUserMsg`: 成交提示消息

### 3.3 竞拍出价 (核心接口) ✅ 已实测

```bash
POST http://localhost:12580/api/services/app/AuctionItem/Bid
Authorization: Bearer {token}
Content-Type: application/json

{
  "auctionItemId": 4100,
  "bidPrice": 10,
  "bidUserName": "feifei",
  "bidUserAvatar": "http://image.molitao.top/molitao/2026-04-02/upload_xxx.jpg"
}
```

**⚠️ 重要发现**: `bidUserName` 和 `bidUserAvatar` 字段**不能为空**，必须传递有效值。
- `bidUserName`: 可以传用户名
- `bidUserAvatar`: 可以传头像URL，也可以传空字符串 `""` (但字段必须存在)

**请求参数 (BidHistoryCreateDto)**:
- `auctionItemId` (long): 竞拍商品ID **[必填]**
- `bidPrice` (int): 出价金额 **[必填]**
- `bidUserName` (string): 出价用户名 **[必填，不能为空]**
- `bidUserAvatar` (string): 出价用户头像 **[必填，不能为空]**

**成功响应**:
```json
{
  "result": {
    "id": 4100,
    "name": "商品名称",
    "currentPrice": 10,
    "currentPriceUserId": 7509,
    "currentPriceUserName": "粑粑&贝伊果我",
    "status": "拍卖中"
  },
  "success": true
}
```

**失败响应 (出价过低)**:
```json
{
  "success": false,
  "error": {
    "message": "出价必须大于最低加价：\n1000以内（含），5R一加\n1000~2000，10R一加\n2000~5000，20R一加\n5000~1W，50R一加\n1W以上，100R一加"
  }
}
```

**出价业务流程**:
1. 检查用户竞拍资格 (通过 `IBidEligibilityService.CheckBidEligibilityAsync`)
2. 检查商品是否存在且状态为"拍卖中"
3. 获取内存锁 (SemaphoreSlim)，防止并发出价
4. 创建 BidHistory 记录
5. 更新商品当前价和出价用户 (`find.SetBid()`)
6. 发送竞拍消息到频道 `-1_auction`
7. 清除相关缓存
8. 发布 `BidPlacedEvent` 事件

### 3.4 开始竞拍 (管理员) ✅ 已实测

```bash
GET http://localhost:12580/api/services/app/AuctionItem/StartAuction?id={auctionItemId}
Authorization: Bearer {adminToken}
```

**权限要求**: `Pages.ChatManager`

**业务规则**:
- 同一时间只能有一个商品在拍卖中
- 已成交商品不能再次拍卖
- 商品状态从"上架(Status=1)"变为"拍卖中(Status=2)"

### 3.5 结束竞拍 (管理员) ✅ 已实测

```bash
GET http://localhost:12580/api/services/app/AuctionItem/EndAuction?id={auctionItemId}
Authorization: Bearer {adminToken}
```

**权限要求**: `Pages.ChatManager`

**业务规则**:
- 无人出价: 商品回退到待拍卖状态 (`find.Back()`)
- 有出价: 设置为已成交状态 (`find.SetDeal()`)，计算用户群聊等级
- 状态变为"已成交(Status=4)"

## 4. 竞拍资格检查

### 4.1 加价阶梯规则 (实测验证)

| 当前价格范围 | 最小加价幅度 |
|------------|------------|
| 0 ~ 1,000 | +5 元 |
| 1,000 ~ 2,000 | +10 元 |
| 2,000 ~ 5,000 | +20 元 |
| 5,000 ~ 10,000 | +50 元 |
| 10,000 以上 | +100 元 |

**卡秒状态**: 三倍加价幅度

### 4.2 竞拍资格验证服务
系统使用 `IBidEligibilityService.CheckBidEligibilityAsync()` 进行验证：
- 检查用户是否存在
- 检查用户保证金是否充足
- 检查出价金额是否高于当前价 + 最小加价
- 检查商品状态是否为"拍卖中"
- 检查用户名格式 (不能是"玩家 xxxxx")
- 检查禁言状态

## 5. 竞拍状态流转 (实测验证)

### 5.1 商品状态枚举 (AuctionStatusEnum)

| 状态值 | 含义 | 说明 |
|-------|------|------|
| 1 | 上架/待拍卖 | 可以启动拍卖 |
| 2 | 拍卖中 | 可以出价 |
| 4 | 已成交 | 拍卖结束，有成交用户 |

### 5.2 出价流程
```
用户发起出价
    ↓
检查竞拍资格 (保证金、出价金额、禁言状态)
    ↓
获取内存锁 (SemaphoreSlim, 10秒超时)
    ↓
创建BidHistory记录
    ↓
更新商品当前价 (SetBid)
    ↓
发送竞拍消息到频道 -1_auction
    ↓
清除缓存 (详情、列表、当前商品)
    ↓
发布BidPlacedEvent事件
    ↓
返回更新后的商品信息
```

### 5.3 结束流程
```
管理员结束竞拍 / 定时任务触发
    ↓
检查是否有出价
    ↓
无人出价 → 商品回退 (Back()) → 状态变为"上架"
    ↓
有出价 → 设置为已成交 (SetDeal()) → 状态变为"已成交"
    ↓
计算用户群聊等级
    ↓
发送竞拍结束消息
    ↓
发布AuctionEndedEvent事件
```

## 6. 数据库表结构 (实测验证)

### 6.1 T_AuctionItem (竞拍商品表)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | bigint | 主键 |
| Name | varchar(128) | 商品名称 |
| Status | int | 状态 (1=上架, 2=拍卖中, 4=已成交) |
| StartingPrice | int | 起拍价 |
| CurrentPrice | int? | 当前价 |
| CurrentPriceUserId | bigint? | 当前最高出价用户ID |
| CurrentPriceUserName | varchar(64) | 当前最高出价用户名 |
| FinalPrice | int? | 成交价 |
| DealUserId | bigint? | 成交用户ID |
| DealUserName | varchar(64) | 成交用户名 |
| DealTime | datetime? | 成交时间 |
| IsDeleted | tinyint(1) | 软删除标记 |

### 6.2 T_BidHistory (竞拍历史表)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | bigint | 主键 |
| AuctionItemId | bigint | 竞拍商品ID |
| BidPrice | int | 出价金额 |
| BidUserName | varchar(64) | 出价用户名 |
| BidUserAvatar | varchar(256) | 出价用户头像 |
| CreationTime | datetime | 出价时间 |
| CreatorUserId | bigint? | 出价用户ID |

### 6.3 AbpUsers (用户表 - 相关字段)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | bigint | 用户ID |
| UserName | varchar | 登录用户名 |
| Name | varchar | 显示名 |
| DepositBalance | decimal | 保证金余额 |
| HeadImgUrl | varchar | 头像URL |

## 7. 测试用例 (实测通过)

### 7.1 完整竞拍流程测试 ✅ 已通过

**测试时间**: 2026-04-04

**测试步骤**:
1. 管理员登录 → 获取 admin token
2. 管理员启动拍卖 (商品ID: 4100) → ✅ 状态变为"拍卖中"
3. 竞拍者登录 → 获取 feifei token
4. feifei 首次出价 (10元) → ✅ 当前价=10, 用户ID=7509
5. feifei 再次出价 (20元) → ✅ 当前价=20
6. feifei 低价出价 (5元) → ✅ 被拒绝，提示加价规则
7. 查询商品详情 → ✅ 验证当前价=20
8. 管理员结束竞拍 → ✅ 状态变为"已成交", 成交价=20
9. 数据库验证 → ✅ Status=4, DealUserId=7509, FinalPrice=20

**API 调用记录**:
```bash
# Step 1: 管理员登录
curl -X POST "http://localhost:12580/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"admin","password":"123456"}'

# Step 2: 启动拍卖
curl "http://localhost:12580/api/services/app/AuctionItem/StartAuction?id=4100" \
  -H "Authorization: Bearer {adminToken}"

# Step 3: 竞拍者登录
curl -X POST "http://localhost:12580/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"feifei","password":"123456"}'

# Step 4: 出价
curl -X POST "http://localhost:12580/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer {feifeiToken}" \
  -H "Content-Type: application/json" \
  -d '{"auctionItemId":4100,"bidPrice":10,"bidUserName":"feifei","bidUserAvatar":"{头像URL}"}'

# Step 5: 查询详情
curl "http://localhost:12580/api/AuctionItem/GetDetail?id=4100" \
  -H "Authorization: Bearer {feifeiToken}"

# Step 6: 结束竞拍
curl "http://localhost:12580/api/services/app/AuctionItem/EndAuction?id=4100" \
  -H "Authorization: Bearer {adminToken}"
```

**数据库验证**:
```sql
-- 验证竞拍历史
SELECT Id, AuctionItemId, BidPrice, BidUserName, CreationTime
FROM T_BidHistory WHERE AuctionItemId = 4100 ORDER BY CreationTime ASC;

-- 验证商品状态
SELECT Id, Name, Status, CurrentPrice, DealUserId, FinalPrice, DealTime
FROM T_AuctionItem WHERE Id = 4100;
```

**预期结果**: 
- 商品当前价正确更新
- 竞拍历史记录完整
- 结束后状态正确

---

### 7.2 出价低于最低加价测试 ✅ 已通过

**测试步骤**:
1. 当前价 = 20
2. 尝试出价 5 元

**预期结果**: 被拒绝，提示加价规则

**实际结果**: ✅ 被拒绝，错误消息: "出价必须大于最低加价：1000以内（含），5R一加..."

---

### 7.3 连续出价测试 ✅ 已通过

**测试步骤**:
1. feifei 出价 10 元 → 成功
2. feifei 再次出价 20 元 → 成功

**数据库验证**:
```sql
SELECT BidPrice, BidUserName, CreationTime
FROM T_BidHistory WHERE AuctionItemId = 4100 ORDER BY CreationTime ASC;
```

**实际结果**: ✅ 两条出价记录正确写入

---

## 8. 测试验证清单

### 8.1 功能验证

- [x] 管理员启动拍卖成功
- [x] 竞拍者出价成功
- [x] 价格更新正确
- [x] 竞拍历史记录生成
- [x] 出价低于最低加价被拒绝
- [x] 连续出价处理正确
- [x] 商品详情查询正确
- [x] 管理员结束竞拍成功
- [x] 成交状态正确写入数据库

### 8.2 数据验证

- [x] 商品 CurrentPrice 正确
- [x] 商品 CurrentPriceUserId 正确
- [x] 竞拍历史记录完整
- [x] 结束后 Status=4 (已成交)
- [x] DealUserId 和 FinalPrice 正确

### 8.3 待测试

- [ ] 保证金不足时出价被拒绝
- [ ] 非管理员尝试启动/结束拍卖被拒绝
- [ ] 已成交商品不能再次拍卖
- [ ] 同一时间只能有一个商品在拍卖中
- [ ] 卡秒模式测试
- [ ] 多用户并发出价测试

## 9. 测试清理

```sql
-- 删除测试产生的竞拍历史
DELETE FROM T_BidHistory WHERE AuctionItemId = 4100;

-- 恢复商品状态 (如果需要重新测试)
UPDATE T_AuctionItem SET Status = 1, CurrentPrice = NULL, CurrentPriceUserId = NULL, 
  CurrentPriceUserName = NULL, DealUserId = NULL, FinalPrice = NULL, DealTime = NULL
WHERE Id = 4100;
```

## 10. 实测经验总结

### 10.1 关键发现

1. **Bid 接口必填字段**: `bidUserName` 和 `bidUserAvatar` 不能为空，必须传递
2. **状态值**: Status=1(上架), 2(拍卖中), 4(已成交)
3. **加价规则**: 1000以内+5元，不是固定加价
4. **并发控制**: 使用内存锁 (SemaphoreSlim)，不是 Redis 分布式锁
5. **消息推送**: 出价后通过 SignalR 发送到 `-1_auction` 频道

### 10.2 常见错误

| 错误 | 原因 | 解决方法 |
|------|------|---------|
| `'Bid User Name' 不能为空` | 未传 bidUserName | 传递用户名 |
| `'Bid User Avatar' 不能为空` | 未传 bidUserAvatar | 传递头像URL |
| `获取APP失败` | 应用配置问题 | 检查 appsettings.json |
| `已存在拍卖的商品` | 已有商品在拍卖中 | 先结束当前拍卖 |

---

**最后更新**: 2026-04-04
**测试人员**: AI Agent
**测试状态**: 核心流程已通过，部分边界场景待测试
