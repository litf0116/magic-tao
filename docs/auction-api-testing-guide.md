# 拍卖系统 API 测试文档

## 一、文档概述

本文档记录拍卖系统 API 的功能测试用例、测试结果和业务规则验证情况。

**测试时间**: 2026-04-05
**测试环境**: localhost:12580
**测试账号**: feifei (普通用户)、用户14 (管理员)

---

## 二、权限系统

### 2.1 权限定义

| 权限名称 | 值 | 说明 |
|---------|-----|------|
| `Pages.Chat.Manager` | 聊天室管理 | 拍卖师/管理员权限 |
| `Pages.Auction` | 竞拍权限 | 所有登录用户 |
| `Pages.Auction.Manager` | 竞拍管理 | 竞拍管理功能 |
| `Pages.Administration` | 系统管理 | 管理员权限 |

### 2.2 权限分配

| 角色 | ChatManager | Administration | 说明 |
|------|-------------|---------------|------|
| 竞拍用户 | ❌ | ❌ | 普通用户，仅可出价 |
| 聊天室管理 | ✅ | ❌ | 拍卖操作权限 |
| 拍卖师 | ✅ | ❌ | 拍卖操作权限 |
| Admin | ✅ | ✅ | 全部权限 |

### 2.3 数据库权限配置

```sql
-- 给用户添加 ChatManager 权限
INSERT INTO AbpPermissions (TenantId, Name, IsGranted, Discriminator, UserId, CreationTime)
VALUES (1, 'Pages.Chat.Manager', 1, 'UserPermissionSetting', {userId}, NOW());
```

---

## 三、拍卖状态流转

### 3.1 状态枚举

| 状态值 | 名称 | 说明 |
|-------|------|------|
| 0 | 草稿 | 未上架 |
| 1 | 上架 | 已上架待拍卖 |
| 2 | 拍卖中 | 正在拍卖 |
| 4 | 已成交 | 拍卖成交 |
| 8 | 交易成功 | 交易完成 |
| 16 | 卖家失约 | 卖家违约 |
| 32 | 买家失约 | 买家违约 |
| 128 | 交易关闭 | 交易关闭 |

### 3.2 状态流转图

```
创建拍品 → 上架(1) → 拍卖中(2) → 已成交(4)
                ↑                    ↓
              流拍                可重复结束
            (恢复上架)            (状态不变)
```

---

## 四、API 接口清单

### 4.1 认证接口

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/TokenAuth/Authenticate` | POST | 公开 | 用户登录 |
| `/api/TokenAuth/GenerateTokenForUser` | POST | 本地 | 为指定用户生成Token |

### 4.2 拍品管理接口

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/services/app/AuctionItem/Create` | POST | ChatManager | 创建拍品 |
| `/api/services/app/AuctionItem/Update` | PUT | ChatManager | 更新拍品 |
| `/api/services/app/AuctionItem/Delete` | DELETE | ChatManager | 删除拍品 |
| `/api/services/app/AuctionItem/GetDetail` | GET | 登录用户 | 获取拍品详情 |
| `/api/services/app/AuctionItem/GetAll` | GET | 登录用户 | 获取拍品列表 |
| `/api/services/app/AuctionItem/GetPublicListAnonymous` | GET | 公开 | 匿名获取公开列表 |

### 4.3 拍卖操作接口

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/services/app/AuctionItem/StartAuction` | GET | ChatManager | 开始拍卖 |
| `/api/services/app/AuctionItem/EndAuction` | GET | ChatManager | 结束拍卖 |
| `/api/services/app/AuctionItem/Bid` | POST | 登录用户 | 出价 |
| `/api/services/app/AuctionItem/GetCurrentAuctionItem` | GET | 登录用户 | 获取当前拍卖品 |

### 4.4 卡秒功能接口

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/services/app/AuctionItem/SetKasecStatus` | POST | ChatManager | 设置卡秒状态 |
| `/api/services/app/AuctionItem/GetKasecStatus` | GET | 公开 | 获取卡秒状态 |

### 4.5 出价历史接口

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/services/app/BidHistory/GetAll` | GET | Administration | 获取出价历史(管理员) |
| `/api/services/app/BidHistory/GetMyBidHistory` | GET | 登录用户 | 获取我的出价历史 |

---

## 五、已测试用例

### 5.1 权限验证测试

| 用例 | 用户 | 预期结果 | 实际结果 | 状态 |
|------|------|---------|---------|------|
| 创建拍品 | feifei (普通) | 权限不足 | 权限不足 | ✅ 通过 |
| 创建拍品 | 用户14 (管理员) | 成功创建 | 成功创建 | ✅ 通过 |
| 开始拍卖 | feifei (普通) | 权限不足 | 权限不足 | ✅ 通过 |
| 开始拍卖 | 用户14 (管理员) | 成功开始 | 成功开始 | ✅ 通过 |
| 结束拍卖 | feifei (普通) | 权限不足 | 权限不足 | ✅ 通过 |
| 结束拍卖 | 用户14 (管理员) | 成功结束 | 成功结束 | ✅ 通过 |

### 5.2 业务规则测试

| 用例 | 场景 | 预期结果 | 实际结果 | 状态 |
|------|------|---------|---------|------|
| 最低加价 | 出价低于当前价 | 错误提示 | 错误提示 | ✅ 通过 |
| 最低加价 | 满足加价规则 | 成功出价 | 成功出价 | ✅ 通过 |
| 单商品拍卖 | 同一时间多商品 | 只允许一个 | 只允许一个 | ✅ 通过 |
| 流拍 | 无出价结束拍卖 | 状态恢复上架 | 状态恢复上架 | ✅ 通过 |
| 重复结束 | 已成交商品再结束 | 状态不变 | 状态不变 | ⚠️ 注意 |

### 5.3 最低加价规则

| 价格区间 | 加价规则 | 测试验证 |
|---------|---------|---------|
| 0-1000 (含) | 5R一加 | ✅ 100→105 成功 |
| 1000-2000 | 10R一加 | ✅ 110→120 成功 |
| 2000-5000 | 20R一加 | ✅ 测试通过 |
| 5000-10000 | 50R一加 | ✅ 测试通过 |
| 10000+ | 100R一加 | ✅ 10000→10100 成功 |

### 5.4 卡秒功能测试

| 用例 | 操作 | 结果 | 状态 |
|------|------|------|------|
| 设置卡秒 | 开启卡秒 | 成功 | ✅ 通过 |
| 获取卡秒 | 查看状态 | true | ✅ 通过 |
| 卡秒出价 | 3倍加价 | 成功 | ✅ 通过 |
| 关闭卡秒 | 关闭状态 | false | ✅ 通过 |
| 正常出价 | 恢复正常加价 | 成功 | ✅ 通过 |

### 5.5 出价历史测试

| 用例 | API | 结果 | 状态 |
|------|-----|------|------|
| 查看我的出价 | GetMyBidHistory | 正常返回 | ✅ 通过 |
| 管理员查看所有 | GetAll (Pid) | 正常返回 | ✅ 通过 |
| 普通用户查看所有 | GetAll | 权限不足 | ✅ 通过 |

### 5.6 异常场景测试

| 用例 | 未登录访问 | 预期结果 | 状态 |
|------|-----------|---------|------|
| 公开API | GetPublicListAnonymous | 正常访问 | ✅ 通过 |
| 授权API | GetAll | 拒绝访问 | ✅ 通过 |
| 出价API | Bid | 拒绝访问 | ✅ 通过 |

---

## 六、API 调用示例

### 6.1 登录获取 Token

```bash
# 普通用户登录
curl -X POST http://localhost:12580/api/TokenAuth/Authenticate \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"feifei","password":"123456"}'

# 响应
{
  "result": {
    "accessToken": "eyJ...",
    "userId": 7509,
    "userName": "feifei"
  },
  "success": true
}
```

### 6.2 为指定用户生成 Token (本地调用)

```bash
curl -X POST http://localhost:12580/api/TokenAuth/GenerateTokenForUser \
  -H "Content-Type: application/json" \
  -d '{"userId":14}'
```

### 6.3 创建拍品

```bash
curl -X POST http://localhost:12580/api/services/app/AuctionItem/Create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "测试拍品",
    "imageUrl": "https://example.com/image.jpg",
    "description": "拍品描述",
    "startingPrice": 100,
    "sellerInfo": "卖家信息",
    "status": 1
  }'
```

### 6.4 开始拍卖

```bash
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/StartAuction?id=17408" \
  -H "Authorization: Bearer $TOKEN"
```

### 6.5 出价

```bash
curl -X POST http://localhost:12580/api/services/app/AuctionItem/Bid \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "auctionItemId": 17408,
    "bidPrice": 105
  }'
```

### 6.6 结束拍卖

```bash
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/EndAuction?id=17408" \
  -H "Authorization: Bearer $TOKEN"
```

### 6.7 设置卡秒状态

```bash
curl -X POST http://localhost:12580/api/services/app/AuctionItem/SetKasecStatus \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "auctionItemId": 17408,
    "isKasec": true
  }'
```

### 6.8 获取卡秒状态

```bash
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/GetKasecStatus?auctionItemId=17408"
```

### 6.9 获取我的出价历史

```bash
curl -X GET "http://localhost:12580/api/services/app/BidHistory/GetMyBidHistory?MaxResultCount=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 6.10 按状态筛选拍品

```bash
# 上架状态
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/GetAll?Status=1&MaxResultCount=10" \
  -H "Authorization: Bearer $TOKEN"

# 拍卖中状态
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/GetAll?Status=2&MaxResultCount=10" \
  -H "Authorization: Bearer $TOKEN"

# 已成交状态
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/GetAll?Status=4&MaxResultCount=10" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 七、完整拍卖流程测试

### 7.1 流程概述

```
管理员创建拍品 → 上架状态 → 开始拍卖 → 用户出价 → 结束拍卖 → 成交
```

### 7.2 测试步骤

```bash
# 1. 管理员登录
ADMIN_TOKEN=$(curl -s -X POST http://localhost:12580/api/TokenAuth/GenerateTokenForUser \
  -H "Content-Type: application/json" \
  -d '{"userId":14}' | jq -r '.result.accessToken')

# 2. 创建拍品
curl -X POST http://localhost:12580/api/services/app/AuctionItem/Create \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"测试","imageUrl":"x.jpg","startingPrice":100,"sellerInfo":"卖家","status":1}'

# 3. 开始拍卖
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/StartAuction?id=17408" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# 4. 用户登录
FEIFEI_TOKEN=$(curl -s -X POST http://localhost:12580/api/TokenAuth/Authenticate \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"feifei","password":"123456"}' | jq -r '.result.accessToken')

# 5. 用户出价
curl -X POST http://localhost:12580/api/services/app/AuctionItem/Bid \
  -H "Authorization: Bearer $FEIFEI_TOKEN" \
  -d '{"auctionItemId":17408,"bidPrice":105}'

# 6. 结束拍卖
curl -X GET "http://localhost:12580/api/services/app/AuctionItem/EndAuction?id=17408" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

---

## 八、业务规则说明

### 8.1 同一时间只能有一个拍卖中的商品

当有商品处于"拍卖中"状态时，无法开始另一个商品的拍卖。

### 8.2 最低加价规则

出价必须满足最低加价要求：
- 1000以内（含）：5R一加
- 1000~2000：10R一加
- 2000~5000：20R一加
- 5000~10000：50R一加
- 10000以上：100R一加

### 8.3 卡秒规则

- 开启卡秒后，出价需要3倍最低加价
- 卡秒状态存储在内存缓存中，30分钟后自动失效
- 每单结束后自动关闭卡秒

### 8.4 流拍规则

无人出价时结束拍卖，状态恢复为"上架"，不是"已成交"。

---

## 九、补充测试用例（Pending → 正式用例）

### 9.1 订阅开拍通知功能

| 用例 | 接口 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A01 | `AuctionStartNotify` | 1. 用户订阅拍品开拍通知 | 成功创建订阅记录 | ⬜ 待测 |
| TC-A02 | `GetMySubscribes` | 2. 获取我的订阅列表 | 返回订阅的拍品 | ⬜ 待测 |
| TC-A03 | `CancelSubscribe` | 3. 取消订阅 | 记录被删除 | ⬜ 待测 |

### 9.2 拍卖状态流转测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A04 | 草稿状态开始拍卖 | 1. 创建草稿状态的拍品 | 不能开始拍卖 | ⬜ 待测 |
| TC-A05 | 拍卖中状态再次开始 | 1. 已有拍卖中商品，再 StartAuction | 抛出"已存在拍卖的商品" | ⬜ 待测 |
| TC-A06 | 流拍后恢复上架 | 1. 无人出价结束拍卖 | 状态恢复为"上架" | ⬜ 待测 |
| TC-A07 | 成交后结束 | 1. 有出价时结束拍卖 | 状态变为"已成交" | ⬜ 待测 |
| TC-A08 | 已成交商品再次结束 | 1. 调用 EndAuction | 状态保持"已成交"不变 | ⬜ 待测 |
| TC-A09 | 卖家违约 | 1. 成交后标记卖家违约 | 状态变为"卖家失约" | ⬜ 待测 |
| TC-A10 | 买家违约 | 1. 成交后标记买家违约 | 状态变为"买家失约" | ⬜ 待测 |
| TC-A11 | 交易成功 | 1. 双方履约后标记成功 | 状态变为"交易成功" | ⬜ 待测 |
| TC-A12 | 交易关闭 | 1. 管理员关闭交易 | 状态变为"交易关闭" | ⬜ 待测 |

### 9.3 价格规则测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A13 | 边界：最低加价+1 | 1. 当前价100，出价106 | 成功（101-1000区间加价5） | ⬜ 待测 |
| TC-A14 | 边界：低于最低加价 | 1. 当前价100，出价103 | 失败，提示加价不足 | ⬜ 待测 |
| TC-A15 | 跨价格区间 | 1. 当前价980，出价1000 | 成功（5R一加） | ⬜ 待测 |
| TC-A16 | 价格区间临界1000 | 1. 当前价1000，出价1005 | 失败（1000-2000需10R） | ⬜ 待测 |
| TC-A17 | 价格区间临界2000 | 1. 当前价2000，出价2015 | 失败（2000-5000需20R） | ⬜ 待测 |
| TC-A18 | 价格区间临界5000 | 1. 当前价5000，出价5040 | 失败（5000-10000需50R） | ⬜ 待测 |
| TC-A19 | 价格区间临界10000 | 1. 当前价10000，出价10090 | 失败（10000+需100R） | ⬜ 待测 |

### 9.4 卡秒功能扩展测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A20 | 卡秒状态过期 | 1. 开启卡秒，等待30分钟后 | 卡秒状态自动失效 | ⬜ 待测 |
| TC-A21 | 卡秒关闭后恢复 | 1. 开启卡秒→出价→关闭卡秒→出价 | 恢复正常加价规则 | ⬜ 待测 |
| TC-A22 | 卡秒中出价低于3倍 | 1. 当前价100，开启卡秒，出价115 | 失败（需3*5=15） | ⬜ 待测 |
| TC-A23 | 卡秒中出价正好3倍 | 1. 当前价100，开启卡秒，出价115 | 成功 | ⬜ 待测 |
| TC-A24 | 卡秒中出价超过3倍 | 1. 当前价100，开启卡秒，出价120 | 成功 | ⬜ 待测 |

### 9.5 缓存一致性测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A25 | 创建拍品后缓存更新 | 1. 创建拍品 | 列表缓存被清除 | ⬜ 待测 |
| TC-A26 | 更新拍品后缓存更新 | 1. 更新拍品 | 详情缓存被清除并重新预热 | ⬜ 待测 |
| TC-A27 | 删除拍品后缓存更新 | 1. 删除拍品 | 所有相关缓存被清除 | ⬜ 待测 |
| TC-A28 | 出价后缓存更新 | 1. 用户出价 | 详情缓存和当前拍卖缓存更新 | ⬜ 待测 |

### 9.6 高并发测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A29 | 多用户同时出价 | 1. 10个用户同时对同一商品出价 | 只有最高价成功，其他失败 | ⬜ 待测 |
| TC-A30 | 防止重复出价 | 1. 同一用户快速点击2次 | 只有一次成功 | ⬜ 待测 |
| TC-A31 | 结束拍卖时并发 | 1. 有人在出价的同时结束拍卖 | 正确处理竞态条件 | ⬜ 待测 |

### 9.7 WebSocket 消息推送测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A32 | 新出价消息推送 | 1. 用户出价 | 所有在线用户收到新出价消息 | ⬜ 待测 |
| TC-A33 | 拍卖开始消息推送 | 1. 管理员开始拍卖 | 所有订阅用户收到通知 | ⬜ 待测 |
| TC-A34 | 拍卖结束消息推送 | 1. 管理员结束拍卖 | 所有在线用户收到结束消息 | ⬜ 待测 |
| TC-A35 | 卡秒状态变更推送 | 1. 开启/关闭卡秒 | 所有在线用户收到状态变更消息 | ⬜ 待测 |
| TC-A36 | 成交消息推送 | 1. 拍卖成交 | 成交用户收到私信通知 | ⬜ 待测 |

### 9.8 微信订阅消息测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A37 | 小程序订阅开拍通知 | 1. 小程序用户订阅 | 记录openid和platform | ⬜ 待测 |
| TC-A38 | App订阅开拍通知 | 1. App用户订阅 | 记录极光RegId | ⬜ 待测 |
| TC-A39 | 开拍时微信推送 | 1. 拍品开始拍卖 | 已订阅用户收到微信服务通知 | ⬜ 待测 |
| TC-A40 | 成交时微信推送 | 1. 拍品成交 | 成交用户收到微信服务通知 | ⬜ 待测 |

### 9.9 边界场景测试

| 用例 | 场景 | 步骤 | 预期结果 | 状态 |
|------|------|------|---------|------|
| TC-A41 | 管理员自己竞拍 | 1. 拍卖师参与自己商品的竞拍 | 允许（未做限制） | ⬜ 待测 |
| TC-A42 | 卖家参与竞拍 | 1. 卖家竞拍自己的商品 | 允许（未做限制） | ⬜ 待测 |
| TC-A43 | 出价低于当前最高价 | 1. 当前价500，出价400 | 失败 | ⬜ 待测 |
| TC-A44 | 出价等于当前最高价 | 1. 当前价500，出价500 | 失败 | ⬜ 待测 |
| TC-A45 | 成交后查看详情 | 1. 拍卖结束后查看详情 | 正常返回成交信息 | ⬜ 待测 |
| TC-A46 | 匿名用户访问公开接口 | 1. 不带Token调用GetPublicListAnonymous | 正常返回 | ⬜ 待测 |
| TC-A47 | 匿名用户访问授权接口 | 1. 不带Token调用GetAll | 返回401 | ⬜ 待测 |

---

## 十、测试执行记录

---

## 十、相关文档

| 文档 | 说明 |
|------|------|
| [auction_kasec_feature.md](./auction_kasec_feature.md) | 卡秒功能需求文档 |
| [auction_bid_logic_update_plan.md](./auction_bid_logic_update_plan.md) | 出价逻辑更新计划 |
| [auction_message_interaction.md](./auction_message_interaction.md) | 拍卖消息交互机制 |
| [auction_status_unification.md](./auction_status_unification.md) | 拍卖状态统一规范 |
| [BID_ELIGIBILITY_SERVICE_USAGE.md](./BID_ELIGIBILITY_SERVICE_USAGE.md) | 出价资格服务 |

---

**最后更新**: 2026-04-05
