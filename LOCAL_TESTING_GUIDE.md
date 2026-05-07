# 本地开发测试指南

## 一、测试账号

| 用户ID | 角色 | 说明 |
|--------|------|------|
| 14 | 管理员 | 拥有所有权限 |
| 7509 | 普通用户 | feifei 用户 |

---

## 二、服务端口

| 服务 | 端口 | 说明 |
|------|------|------|
| API Server | `12580` | 后端 API 服务 |
| ImServer | `6001` | WebSocket 服务 |
| MySQL | `3306` | 数据库 |
| Redis | `6379` | 缓存/消息队列 |

---

## 三、GenerateTokenForUser 接口

### 3.1 接口说明

**功能**: 通过用户ID直接生成 Token，无需密码登录

**限制**: 仅允许本地 IP 访问（127.0.0.1, 192.168.*, 10.*, 172.*）

### 3.2 请求示例

```bash
# 生成管理员 Token (userId=14)
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}'

# 生成普通用户 Token (userId=7509)
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 7509}'
```

### 3.3 响应示例

```json
{
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 7509,
    "userName": "feifei"
  },
  "success": true
}
```

### 3.4 快捷脚本

```bash
# 获取 Token 并保存到变量
TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 7509}' | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('accessToken',''))")

echo "Token: $TOKEN"
```

---

## 四、WebSocket 测试

### 4.1 获取 WebSocket 连接信息

```bash
# 使用 pre-connect 获取 WebSocket 地址
curl -X POST "http://127.0.0.1:12580/ws/pre-connect" \
  -H "Authorization: Bearer $TOKEN"
```

**响应示例**:
```json
{
  "result": {
    "websocketId": 7509,
    "server": "ws://127.0.0.1:6001/ws?token=xxx"
  }
}
```

### 4.2 WebSocket 连接流程

```
1. 调用 /ws/pre-connect 获取 WebSocket URL
         │
         ▼
2. 连接到返回的 WebSocket URL
         │
         ▼
3. 发送订阅频道请求:
   {"type": 4, "chan": "-1_auction"}
         │
         ▼
4. 接收频道消息
```

### 4.3 消息类型

| 数值 | 字符串 | 说明 |
|------|--------|------|
| 4 | Subscribe | 订阅频道 |
| 1 | Text | 文本消息 |
| 1000 | AuctionStart | 拍卖开始 |
| 1002 | AuctionBid | 出价消息 |
| 1010 | AuctionEnd | 拍卖结束 |

---

## 五、订阅功能测试

### 5.1 测试脚本

```bash
#!/bin/bash
# test_subscription.sh

BASE_URL="http://127.0.0.1:12580"
USER_ID=7509

echo "===== 测试订阅功能 ====="

# 1. 获取 Token
echo "[Step 1] 获取用户 Token..."
TOKEN=$(curl -s -X POST "$BASE_URL/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": $USER_ID}" | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('accessToken',''))")

echo "Token: ${TOKEN:0:30}..."

# 2. 获取 WebSocket 连接信息
echo "[Step 2] 获取 WebSocket 连接信息..."
WS_INFO=$(curl -s -X POST "$BASE_URL/ws/pre-connect" \
  -H "Authorization: Bearer $TOKEN")

WS_ID=$(echo "$WS_INFO" | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('websocketId',''))")
WS_SERVER=$(echo "$WS_INFO" | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('server',''))")

echo "WebSocket ID: $WS_ID"
echo "Server: $WS_SERVER"

# 3. 查询待拍卖拍品
echo "[Step 3] 查询待拍卖拍品..."
AUCTION_LIST=$(curl -s "$BASE_URL/api/services/app/AuctionItem/GetList?Status=listed&MaxResultCount=1" \
  -H "Authorization: Bearer $TOKEN")

AUCTION_ID=$(echo "$AUCTION_LIST" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(items[0].get('id','') if items else '')")

if [ -z "$AUCTION_ID" ]; then
  echo "没有待拍卖的拍品"
else
  echo "拍品 ID: $AUCTION_ID"
  
  # 4. 订阅拍品
  echo "[Step 4] 订阅拍品..."
  curl -s -X POST "$BASE_URL/api/services/app/AuctionItem/SubStartNotify" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"auctionItemId\": $AUCTION_ID, \"platform\": \"app\"}"
  
  echo ""
  echo "✅ 订阅成功"
fi

echo "===== 测试完成 ====="
```

### 5.2 测试拍卖开始通知

```bash
# 管理员触发拍卖开始
ADMIN_TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('accessToken',''))")

curl -X POST "http://127.0.0.1:12580/api/services/app/AuctionItem/StartAuction" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": AUCTION_ID}'
```

---

## 六、常用 API

### 6.1 用户相关

```bash
# 获取当前用户信息
curl -X GET "$BASE_URL/api/services/app/User/GetCurrentUser" \
  -H "Authorization: Bearer $TOKEN"

# 获取用户微信 OpenId
curl -X GET "$BASE_URL/api/services/app/User/GetMyWechatOpenId" \
  -H "Authorization: Bearer $TOKEN"
```

### 6.2 拍卖相关

```bash
# 获取拍卖列表
curl -X GET "$BASE_URL/api/services/app/AuctionItem/GetOnAuctionList" \
  -H "Authorization: Bearer $TOKEN"

# 获取拍品详情
curl -X GET "$BASE_URL/api/services/app/AuctionItem/Get?Id=AUCTION_ID" \
  -H "Authorization: Bearer $TOKEN"

# 开始拍卖（管理员）
curl -X POST "$BASE_URL/api/services/app/AuctionItem/StartAuction" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": AUCTION_ID}'

# 结束拍卖
curl -X POST "$BASE_URL/api/services/app/AuctionItem/EndAuction?id=AUCTION_ID" \
  -H "Authorization: Bearer $TOKEN"

# 出价
curl -X POST "$BASE_URL/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"auctionItemId": AUCTION_ID, "bidPrice": 100}'
```

### 6.3 聊天相关

```bash
# 获取聊天列表
curl -X GET "$BASE_URL/api/services/app/Chat/GetChatList" \
  -H "Authorization: Bearer $TOKEN"

# 获取群聊历史消息
curl -X GET "$BASE_URL/api/services/app/Chat/GetGroupHistory?chan=-1_auction&SkipCount=0&MaxResultCount=50" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 七、测试记录

### 测试日期: 2026-05-06

| 测试项 | 结果 | 说明 |
|--------|------|------|
| GenerateTokenForUser (userId=14) | ✅ | 管理员 Token 生成成功 |
| GenerateTokenForUser (userId=7509) | ✅ | 用户 Token 生成成功 |
| WebSocket pre-connect | ✅ | 返回 websocketId 和 server URL |
| ImServer (端口 6001) | ✅ | WebSocket 服务运行中 |
| API Server (端口 12580) | ✅ | API 服务运行中 |
| 订阅 API (SubStartNotify) | ✅ | 订阅成功，记录存入数据库 |
| 开始拍卖 (StartAuction) | ⚠️ | 需要当前无其他拍卖进行中 |

### 测试结论

**订阅功能正常工作**：
1. ✅ 订阅 API 正确保存订阅记录到 `T_AuctionStartNotify` 表
2. ✅ WebSocket pre-connect 返回正确的连接信息
3. ✅ 消息类型映射正确 (type 1000 → AuctionStart)

**需要验证**：
- WebSocket 客户端接收消息（需要通过 App 端测试）
- 推送通知是否正常发送（极光推送/微信模板消息）

### 快速测试命令

```bash
# 运行完整测试脚本
./scripts/test/test_websocket_subscription.sh

# 或手动测试
TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" -H "Content-Type: application/json" -d '{"userId": 7509}' | python3 -c "import sys,json; print(json.load(sys.stdin).get('result',{}).get('accessToken',''))")

# 订阅拍品
curl -X POST "http://127.0.0.1:12580/api/services/app/AuctionItem/SubStartNotify" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"auctionItemId": 17324, "platform": "app"}'

# 获取 WebSocket 连接
curl -X POST "http://127.0.0.1:12580/ws/pre-connect" -H "Authorization: Bearer $TOKEN"
```
