# 拍卖成功消息展示功能测试说明

## 测试目标

验证拍卖成功后的消息展示功能，确保以下功能正常工作：

1. **拍卖成功 channel 消息**：拍卖成功后发送 `AuctionEnd` 类型消息到 `-1_auction` 频道
2. **成交用户私信**：向成交用户发送 `AuctionDeal` 类型私信（自动编码为 `AuctionEnd`）
3. **流拍消息**：无出价时发送流拍消息到 `-1_auction` 频道

## 测试前准备

### 1. 确保后端服务运行

```bash
cd backend
dotnet run --project src/TtWork.Project.Web.Host
```

### 2. 准备测试数据

确保数据库中存在以下数据：

- 测试用户（用户名: `admin`，密码: `123qwe`）
- 至少一个测试拍卖商品（或让测试脚本自动创建）

### 3. 安装依赖

确保系统已安装：
- `curl` - HTTP 请求工具
- `python3` - JSON 格式化输出（可选）

## 测试脚本说明

### 主测试脚本

**文件**: `backend/test-scripts/test-auction-end-message.sh`

**测试流程**：

1. **登录认证**：使用 admin 账号登录，获取 access token
2. **获取用户信息**：获取当前登录用户的 ID
3. **获取拍卖商品列表**：查询可用的拍卖商品，如果不存在则自动创建
4. **开始拍卖**：调用 `StartAuction` API 开始拍卖
5. **模拟出价**：调用 `Bid` API 进行出价
6. **结束拍卖**：调用 `EndAuction` API 手动结束拍卖
7. **验证 channel 消息**：获取消息列表，验证 `AuctionEnd` 消息是否正确发送
8. **验证私聊频道**：如果成交，检查是否创建了与成交用户的私聊频道

### 运行测试脚本

```bash
cd backend/test-scripts
./test-auction-end-message.sh
```

### 预期输出

脚本执行过程中会显示详细的测试步骤和结果：

- ✓ 绿色：测试步骤成功
- ✗ 红色：测试步骤失败
- 黄色：章节标题

## 测试场景

### 场景1：拍卖成功（有出价）

**步骤**：
1. 开始拍卖
2. 用户A出价 100 元
3. 用户B出价 150 元
4. 结束拍卖

**预期结果**：

#### 消息1：Channel 广播消息
- **类型**: `AuctionEnd` (1010)
- **频道**: `-1_auction`
- **发送者**: 拍卖师（有管理员标签）
- **消息内容**: `恭喜 {成交用户名} 以 ￥{成交价格} 拍得 {商品名}`
- **接收者**: 所有订阅了拍卖频道的用户

#### 消息2：成交用户私信
- **原始类型**: `AuctionDeal` (1011)
- **编码后类型**: `AuctionEnd` (1010)
- **接收者**: 成交用户
- **消息内容**: `恭喜您,您拍得了{商品名},成交价:{价格},\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待`

### 场景2：流拍（无出价）

**步骤**：
1. 开始拍卖
2. 直接结束拍卖（无人出价）

**预期结果**：

#### Channel 消息
- **类型**: `AuctionEnd` (1010)
- **频道**: `-1_auction`
- **发送者**: 拍卖师（有管理员标签）
- **消息内容**: `拍卖结束，无人出价，商品已回退`

## 手动测试命令

### 1. 登录获取 token

```bash
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/Authenticate" \
    -H "Content-Type: application/json" \
    -d '{
        "userNameOrEmailAddress": "admin",
        "password": "123qwe"
    }' | python3 -m json.tool
```

### 2. 获取拍卖商品列表

```bash
TOKEN="your-access-token-here"

curl -X GET "http://127.0.0.1:12580/api/services/app/AuctionItem/GetPublicList" \
    -H "Authorization: Bearer $TOKEN" \
    -H "AppVersion: 20260224@1.1.21" | python3 -m json.tool
```

### 3. 开始拍卖

```bash
AUCTION_ID=1001

curl -X GET "http://127.0.0.1:12580/api/services/app/AuctionItem/StartAuction?id=$AUCTION_ID" \
    -H "Authorization: Bearer $TOKEN" \
    -H "AppVersion: 20260224@1.1.21" | python3 -m json.tool
```

### 4. 出价

```bash
curl -X POST "http://127.0.0.1:12580/api/services/app/AuctionItem/Bid" \
    -H "Authorization: Bearer $TOKEN" \
    -H "AppVersion: 20260224@1.1.21" \
    -H "Content-Type: application/json" \
    -d '{
        "auctionItemId": 1001,
        "bidPrice": 150
    }' | python3 -m json.tool
```

### 5. 结束拍卖

```bash
curl -X GET "http://127.0.0.1:12580/api/services/app/AuctionItem/EndAuction?id=1001" \
    -H "Authorization: Bearer $TOKEN" \
    -H "AppVersion: 20260224@1.1.21" | python3 -m json.tool
```

### 6. 获取消息列表

```bash
curl -X POST "http://127.0.0.1:12580/ws/get-history" \
    -H "Authorization: Bearer $TOKEN" \
    -H "AppVersion: 20260224@1.1.21" \
    -H "Content-Type: application/json" \
    -d '{
        "chan": "-1_auction",
        "lastMessageId": null,
        "limit": 20
    }' | python3 -m json.tool
```

## 消息类型说明

### ChatMessageType 枚举

| 类型值 | 名称 | 说明 |
|--------|------|------|
| 1010 | AuctionEnd | 拍卖结束消息（频道广播） |
| 1011 | AuctionDeal | 拍卖成交通知（私聊，会自动编码为 AuctionEnd） |
| 1002 | AuctionBid | 出价消息 |
| 1000 | AuctionStart | 拍卖开始 |
| 2000 | KasecStatusChanged | 卡秒状态变化 |

### 消息编码机制

系统会自动对某些消息类型进行编码：

- **AuctionDeal → AuctionEnd**: 成交消息通过私聊发送时，会自动编码为 `AuctionEnd` 类型，并在 payload 中保留原始信息
- **KasecStatusChanged → AuctionBid**: 卡秒状态消息会编码为 `AuctionBid` 类型

## 验证点

### 后端验证

1. **查看日志**
   ```bash
   tail -f backend/src/TtWork.Project.Web.Host/Logs/Logs.txt
   ```

   查找关键日志：
   - `拍卖成功消息发送成功`
   - `拍卖成交私信发送成功`
   - `流拍消息发送成功`

2. **检查数据库**
   ```sql
   -- 查询拍卖频道的消息
   SELECT TOP 20 *
   FROM t_message
   WHERE Chan = '-1_auction'
   ORDER BY Time DESC;

   -- 查询私聊消息
   SELECT TOP 20 *
   FROM t_message
   WHERE Type = 1010 AND Chan IS NULL
   ORDER BY Time DESC;
   ```

### 前端验证

1. **WebSocket 连接**
   - 打开浏览器开发者工具
   - 切换到 Network 标签页
   - 查找 WS 连接（ws://127.0.0.1:12580/ws）
   - 查看收到的消息

2. **消息展示**
   - 登录 PC 前端：http://127.0.0.1:12581
   - 进入拍卖频道
   - 查看是否显示拍卖成功消息
   - 如果是成交用户，查看私聊列表

## 故障排查

### 问题1：消息未发送到 channel

**可能原因**：
- WebSocket 连接未建立
- 用户未订阅拍卖频道
- 版本控制导致频道隐藏

**解决方法**：
1. 检查后端日志是否有错误
2. 验证 WebSocket 连接状态
3. 检查版本号设置

### 问题2：成交用户未收到私信

**可能原因**：
- 用户 ID 不匹配
- 聊天删除记录未清除

**解决方法**：
1. 检查 `DealUserId` 字段值
2. 查看 `ChatListDelete` 表

### 问题3：管理员标签未显示

**可能原因**：
- 用户角色配置不正确
- 消息发送选项未设置

**解决方法**：
1. 检查用户角色（应有 `AuctionManager`）
2. 验证 `AddAdminTag` 选项

## 相关文件

### 后端代码

- 拍卖业务逻辑：`backend/src/TtWork.Project/Applications/Auctions/AuctionItemAppService.cs`
  - `EndAuction` (行 846-1090)
  - `Callback` (行 311-579)

- 消息发送服务：`backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs`
  - `SendAuctionMessageAsync` (行 311-348)
  - `EncodeAuctionDealMessage` (行 704-735)

- 消息类型定义：`backend/FreeIM/FreeIM/TokenEvent.cs`

### 前端代码

- 聊天状态管理：`pc/src/stores/chatStore.ts`
- 群聊页面：`pc/src/views/chat/groupChat.vue`

## 版本控制说明

拍卖频道的显示受版本控制影响：

- **已发布版本** (当前版本 ≤ 稳定版本)：显示拍卖频道
- **审核中版本** (当前版本 > 稳定版本)：隐藏拍卖频道

测试时请确保版本号设置正确，版本号格式：`YYYYMMDD@主.次.补`

示例：
- 当前版本：`20260224@1.1.21`
- 稳定版本：`20260224@1.1.21` → 显示拍卖频道
- 稳定版本：`20260224@1.1.20` → 隐藏拍卖频道

## 联系方式

如有问题，请查看：
- 后端日志文件
- 浏览器控制台错误
- GitHub Issues