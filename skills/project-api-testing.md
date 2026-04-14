# Project API Testing Skill

## 概述

这是一个用于项目 API 自动化测试的技能，主要功能包括：

1. 通过 `GenerateTokenForUser` 接口获取指定用户的认证 token
2. 提供拍卖相关接口的快速测试
3. 支持预设测试用户（拍卖师 ID: 14，测试用户 ID: 7509）

## 核心功能

### 获取用户 Token

**接口**: `POST /api/TokenAuth/GenerateTokenForUser`

**限制**: 仅允许本地 IP 访问（127.0.0.1, 192.168.x.x, 10.x.x.x, 172.x.x.x）

**请求参数**:
```json
{
  "userId": 14  // 用户 ID
}
```

**响应结果**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "encryptedAccessToken": "xxx",
  "expireInSeconds": 86400,
  "refreshToken": "xxx",
  "refreshTokenExpireInSeconds": 604800,
  "userId": 14,
  "userName": "拍卖师"
}
```

### 拍卖相关接口

1. **获取拍卖商品列表**
   - 接口: `GET /api/services/app/AuctionItem/GetPublicList`
   - 参数: `SkipCount`, `MaxResultCount`

2. **开始拍卖**
   - 接口: `GET /api/services/app/AuctionItem/StartAuction?id={auctionItemId}`
   - 权限: 拍卖师

3. **出价**
   - 接口: `POST /api/services/app/AuctionItem/Bid`
   - 参数: `auctionItemId`, `bidPrice`

4. **结束拍卖**
   - 接口: `GET /api/services/app/AuctionItem/EndAuction?id={auctionItemId}`
   - 权限: 拍卖师

5. **获取消息历史**
   - 接口: `POST /ws/get-history`
   - 参数: `chan`, `lastMessageId`, `limit`

## 预设用户

| 用户类型 | 用户 ID | 用户名 | 密码 | 说明 |
|---------|---------|--------|------|------|
| 管理员/拍卖师 | 14 | oFzSV6st7nn8ZeoTEQqbveyjfMAU | (系统生成) | 有权限开始/结束拍卖 |
| 测试用户 | 7509 | feifei | 123456 | 用于出价和测试 |

### 更新测试用户密码

用户 7509 的密码已更新为 `123456`，使用以下 SQL：

```sql
UPDATE t_users 
SET Password = 'AQAAAAIAAYagAAAAEELvIS7IF2FX8osRxav+DfM8eAosC/ra0xZqxbzSsyzJmWb0NBs7L4HxxELQtQx1zg=='
WHERE Id = 7509;
```

### 生成新密码哈希

如需为其他密码生成哈希值：

```bash
curl -X GET "http://127.0.0.1:12580/api/TokenAuth/GenerateHashedPassword?plainPassword=your_password"
```

## 使用示例

### 1. 获取管理员 Token 并开始拍卖

```bash
# 获取管理员 token (ADMIN_TOKEN)
ADMIN_TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
    -H "Content-Type: application/json" \
    -d '{"userId": 14}' | jq -r '.result.accessToken')

# 开始拍卖
curl -X GET "http://127.0.0.1:12580/api/services/app/AuctionItem/StartAuction?id=1001" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "AppVersion: 20260224@1.1.21" | jq
```

### 2. 获取测试用户 Token 并出价

```bash
# 获取测试用户 token (USER_TOKEN)
USER_TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
    -H "Content-Type: application/json" \
    -d '{"userId": 7509}' | jq -r '.result.accessToken')

# 出价
curl -X POST "http://127.0.0.1:12580/api/services/app/AuctionItem/Bid" \
    -H "Authorization: Bearer $USER_TOKEN" \
    -H "AppVersion: 20260224@1.1.21" \
    -H "Content-Type: application/json" \
    -d '{"auctionItemId": 1001, "bidPrice": 150}' | jq
```

### 3. 结束拍卖并验证消息

```bash
# 获取管理员 token (ADMIN_TOKEN)
ADMIN_TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
    -H "Content-Type: application/json" \
    -d '{"userId": 14}' | jq -r '.result.accessToken')

# 结束拍卖
curl -X GET "http://127.0.0.1:12580/api/services/app/AuctionItem/EndAuction?id=1001" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "AppVersion: 20260224@1.1.21" | jq

# 获取拍卖频道消息
curl -X POST "http://127.0.0.1:12580/ws/get-history" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "AppVersion: 20260224@1.1.21" \
    -H "Content-Type: application/json" \
    -d '{"chan": "-1_auction", "lastMessageId": null, "limit": 20}' | jq
```

## 消息类型说明

### 拍卖相关消息类型

| 类型值 | 名称 | 说明 |
|--------|------|------|
| 1010 | AuctionEnd | 拍卖结束消息（频道广播） |
| 1011 | AuctionDeal | 拍卖成交通知（私聊，自动编码为 AuctionEnd） |
| 1002 | AuctionBid | 出价消息 |
| 1000 | AuctionStart | 拍卖开始 |
| 2000 | KasecStatusChanged | 卡秒状态变化 |

### Channel 信息

- **拍卖频道 ID**: `-1_auction`
- **系统频道类型**: System
- **订阅方式**: 通过 `POST /ws/sub-channel` 订阅

## 测试场景

### 场景1：正常拍卖流程

1. 使用管理员 ID (14) 获取 ADMIN_TOKEN
2. 获取拍卖商品列表
3. 开始拍卖
4. 使用测试用户 ID (7509) 获取 USER_TOKEN 并出价
5. 使用管理员 ADMIN_TOKEN 结束拍卖
6. 验证 channel 消息

### 场景2：流拍流程

1. 使用管理员 ID (14) 获取 ADMIN_TOKEN
2. 开始拍卖
3. 直接结束拍卖（无人出价）
4. 验证流拍消息

## 注意事项

1. **IP 限制**: `GenerateTokenForUser` 接口仅允许本地 IP 访问
2. **版本控制**: 所有请求需携带 `AppVersion` 请求头
3. **Token 过期**: 访问令牌有效期为 24 小时
4. **权限控制**: 开始/结束拍卖需要拍卖师权限

## 依赖工具

- `curl` - HTTP 请求工具
- `jq` - JSON 处理工具（可选，用于格式化输出）

## 故障排查

### 无法获取 token

检查：
1. 后端服务是否运行
2. 请求是否来自本地 IP
3. 用户 ID 是否有效

### 接口调用失败

检查：
1. Token 是否有效
2. 是否包含 `AppVersion` 请求头
3. 用户是否有相应权限

### 消息未显示

检查：
1. 用户是否订阅了拍卖频道
2. WebSocket 连接是否正常
3. 后端日志是否有错误