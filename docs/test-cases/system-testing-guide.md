# 系统测试辅助文档

## 概述

本文档提供魔力淘后端 API 的系统测试基础功能说明，包括 Token 获取、测试账号、常用接口调用等。

## 测试环境

| 配置 | 值 |
|------|-----|
| 本地后端 | `http://localhost:12580` |
| 数据库 | MySQL `www_molitao_top` (root/root) |
| Redis | `127.0.0.1:6379` |

---

## 基础功能

### 1. 获取用户 Token

通过 `GenerateTokenForUser` 接口可根据用户 ID 获取认证 Token。

**接口**: `POST /api/TokenAuth/GenerateTokenForUser`

**限制**: 仅允许本地 IP 访问（127.0.0.1, 192.168.x.x, 10.x.x.x, 172.x.x.x）

**请求参数**:
```json
{
  "userId": 14
}
```

**响应结果**:
```json
{
  "success": true,
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "encryptedAccessToken": "xxx",
    "expireInSeconds": 604800,
    "refreshToken": "xxx",
    "refreshTokenExpireInSeconds": 604800,
    "userId": 14,
    "userName": "oFzSV6st7nn8ZeoTEQqbveyjfMAU"
  }
}
```

**快速获取 Token 的方式**:

```bash
# 使用 python 提取 Token
TOKEN=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")

# 或使用 jq (需安装)
TOKEN=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | jq -r '.result.accessToken')
```

---

### 2. 生成密码哈希

为指定密码生成哈希值（用于数据库更新）。

**接口**: `GET /api/TokenAuth/GenerateHashedPassword`

**参数**: `plainPassword` (可选，默认 "123456")

**使用示例**:
```bash
# 生成新密码哈希
curl -X GET "http://localhost:12580/api/TokenAuth/GenerateHashedPassword?plainPassword=your_password"
```

---

## 测试账号

### 预设用户

| 用户ID | 用户名 | 昵称 | 角色 | 密码 | 说明 |
|--------|--------|------|------|------|------|
| 14 | oFzSV6st7nn8ZeoTEQqbveyjfMAU | 【魔力淘】老淡 | 管理员/拍卖师 | - | Admin 角色，可操作拍卖 |
| 7509 | feifei | 飞飞后端审核测试2 | 普通用户 | 123456 | 测试用户 |

### 查询用户信息

```bash
mysql -uroot -proot -e "SELECT Id, UserName, Name, IsActive FROM www_molitao_top.abpusers WHERE Id IN (14, 7509);"
```

---

## 消息发送接口

### 发送私人消息

**接口**: `POST /ws/send-msg`

**请求头**: `Authorization: Bearer {TOKEN}`

**请求参数**:
```json
{
  "from": 7509,
  "to": 14,
  "message": {
    "type": "Text",
    "msg": "测试消息内容"
  },
  "isReceipt": false
}
```

**消息类型 (type)**:
| 类型值 | 名称 | 说明 |
|--------|------|------|
| 1 | Text | 文本消息 |
| 1002 | AuctionBid | 出价消息 |
| 1010 | AuctionEnd | 拍卖结束 |
| 1011 | AuctionDeal | 拍卖成交 |

**使用示例**:
```bash
# 获取 Token
TOKEN=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 7509}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")

# 发送消息
curl -s -X POST "http://localhost:12580/ws/send-msg" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": 7509,
    "to": 14,
    "message": {
      "type": "Text",
      "msg": "测试消息"
    },
    "isReceipt": false
  }'
```

**响应示例**:
```json
{
  "success": true,
  "result": {
    "code": 0,
    "data": {
      "id": "606f375e-3f17-48c0-9fce-6b6b72e3713a",
      "from": 7509,
      "to": 14,
      "message": {
        "id": "606f375e-3f17-48c0-9fce-6b6b72e3713a",
        "type": "Text",
        "fromName": "飞飞后端审核测试2",
        "to": 14,
        "time": 1777035492466,
        "msg": "测试消息",
        "sequenceNumber": 18
      }
    }
  }
}
```

---

### 获取历史消息

**接口**: `POST /ws/get-history`

**请求参数**:
```json
{
  "chan": "-1_auction",
  "lastMessageId": null,
  "limit": 20
}
```

**使用示例**:
```bash
curl -s -X POST "http://localhost:12580/ws/get-history" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"chan": "-1_auction", "lastMessageId": null, "limit": 20}'
```

---

## 好友相关接口

### 添加好友

```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=7509" \
  -H "Authorization: Bearer $TOKEN_14"
```

### 获取好友申请数量

```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriendCount" \
  -H "Authorization: Bearer $TOKEN"
```

### 获取好友列表

```bash
# 待处理申请 (status=false)
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=7509&status=false" \
  -H "Authorization: Bearer $TOKEN"

# 已同意好友 (status=true)
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=7509&status=true" \
  -H "Authorization: Bearer $TOKEN"
```

### 同意/拒绝好友申请

```bash
# 同意
curl -s "http://localhost:12580/api/services/app/UserFriend/Agree?id=14&status=true" \
  -H "Authorization: Bearer $TOKEN_7509"

# 拒绝
curl -s "http://localhost:12580/api/services/app/UserFriend/Agree?id=14&status=false" \
  -H "Authorization: Bearer $TOKEN_7509"
```

---

## 数据库操作

### 连接数据库

```bash
mysql -h 127.0.0.1 -u root -proot -D www_molitao_top
```

### 常用查询

```sql
-- 查看用户信息
SELECT Id, UserName, Name, IsActive FROM abpusers WHERE Id IN (14, 7509);

-- 查看好友关系
SELECT * FROM T_UserFriend WHERE UserId IN (14, 7509) OR FriendId IN (14, 7509);

-- 查看消息记录
SELECT * FROM T_Message WHERE \`To\` = 14 OR \`From\` = 14 ORDER BY Time DESC LIMIT 10;

-- 清理测试数据
DELETE FROM T_UserFriend WHERE UserId IN (14, 7509) OR FriendId IN (14, 7509);
```

---

## 服务管理

### 启动本地后端服务

```bash
cd backend
dotnet run --project src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj --urls "http://localhost:12580"
```

### 检查服务状态

```bash
# 检查端口
lsof -i :12580

# 检查服务响应
curl -s -o /dev/null -w "%{http_code}" http://localhost:12580/
```

### 检查依赖服务

```bash
# MySQL
mysql -uroot -proot -e "SELECT 1"

# Redis
redis-cli ping
```

---

## 常见问题

### 1. 获取 Token 返回 404

确保使用正确的接口路径：
- 正确: `POST /api/TokenAuth/GenerateTokenForUser`
- 错误: `/api/services/app/Account/Login`

### 2. Token 获取成功但接口调用失败

检查：
- 请求头是否包含 `Authorization: Bearer {TOKEN}`
- `Content-Type: application/json`
- Token 是否过期（默认 7 天）

### 3. 消息发送成功但对方未收到

- 检查 WebSocket 连接是否正常
- 确认接收者是否在线
- 查看后端日志

### 4. 非好友发送消息被拒绝

**注意**: 好友检查逻辑位于 `WebsocketController.cs` 的 `SendMsg` 方法中。
如需禁用好友检查，请联系开发团队。

---

## 相关文件

| 文件 | 说明 |
|------|------|
| `docs/test-cases/friend-request-test-cases.md` | 好友功能测试用例 |
| `docs/friend-request-business-logic.md` | 好友申请业务逻辑 |
| `skills/project-api-testing.md` | 拍卖 API 测试指南 |
| `backend/src/TtWork.Project/Controllers/WebsocketController.cs` | 消息发送控制器 |
| `backend/src/TtWork.Project/Applications/UserFriendAppService.cs` | 好友服务 |

---

## 更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-04-24 | v1.0 | 初始文档，包含 Token 获取、测试账号、消息发送等基础功能 |
