# 拍卖"卡秒"功能需求开发文档

## 一、功能背景

为提升拍卖紧张氛围，支持拍卖师在拍卖过程中开启"卡秒"模式。开启后，用户需以三倍最低加价进行出价，并在聊天室高亮提示。每单结束后自动关闭卡秒，保证流程流畅。

---

## 二、功能需求

### 1. 前端需求

#### 1.1 UI 交互
- 在拍卖师操作区（结束竞拍按钮上方）新增"卡秒"按钮，支持开启/关闭。
- 按钮有两种状态（开启/关闭），状态切换有明显视觉区分。

#### 1.2 聊天室消息
- 开启卡秒时，自动向聊天室发送高亮消息："商品进入成交倒计时，卡秒出价需加够三倍一口价！"（红色边框）。

#### 1.3 出价弹窗提示
- 卡秒期间，用户点击出价，弹窗上方显示红色提示："您已卡秒出价，需加够三倍竞拍价才有效（最低出价：XXX）"。

#### 1.4 自动恢复
- 每单结束后，卡秒自动关闭，不影响下一单。
- 切换拍品时，卡秒自动关闭。

---

### 2. 前端逻辑

- 新增 isKasec 响应式变量，管理卡秒状态。
- 最低加价逻辑：卡秒期间最低加价为原区间的三倍。
- bid() 方法根据 isKasec 动态调整最低加价和弹窗提示。
- toggleKasec() 方法切换卡秒状态，并发送聊天室消息。
- 监听 onAuctionItem 变化和竞拍结束，自动关闭卡秒。

---

### 3. 后端需求

- 聊天室消息接口支持高亮/样式。
- 拍卖出价接口需校验卡秒期间的最低加价，防止绕过前端。
- 若需全员同步卡秒状态，后端可存储并推送卡秒状态（可选）。

---

### 4. 其他说明

- UI 提示需明显，避免用户误操作。
- 最低加价校验必须前后端一致，防止作弊。
- 若聊天室消息需全员可见，建议后端推送。

---

## 三、开发建议

- 前端：加按钮、状态、提示、逻辑切换。
- 后端：校验、消息推送、状态同步（如有需要）。
- 自动恢复：每单结束/切换自动关闭卡秒。

---

## 四、开发步骤与信息同步

1. **前端：卡秒按钮与状态管理**
   - 在 AuctionList.vue 管理员操作区新增"卡秒"按钮，点击切换 isKasec 状态。
   - 需要确认：卡秒状态是仅前端本地，还是需要全员同步？如需同步，请提供后端接口或事件推送机制。

2. **前端：卡秒消息发送**
   - 开启卡秒时，调用 chatStore.sendChannelMsg 发送高亮提示到拍卖群。
   - 需要确认：聊天室消息渲染组件是否支持 payload 扩展（如高亮、红色边框）？如不支持，请提供消息渲染相关代码片段。

3. **前端：最低加价逻辑调整**
   - 在 bid 方法中，根据 isKasec 动态调整最低加价。
   - 需要确认：拍卖区间加价的算法是否有特殊情况？（如有自定义区间、特殊商品等）

4. **前端：出价弹窗提示**
   - 卡秒期间，出价弹窗上方显示红色提示。
   - 需要确认：弹窗组件是否支持插入自定义 HTML 或样式？如不支持，请提供弹窗相关代码片段。

5. **自动恢复卡秒状态**
   - 在 end 方法和拍品切换时，自动关闭 isKasec。
   - 需要确认：拍品切换的触发点是否只在 AuctionList.vue 内部？如有全局切换，请说明切换逻辑。

6. **后端：最低加价校验**
   - 后端 bid 接口需校验卡秒期间的最低加价。
   - 需要确认：后端拍卖出价接口的代码实现，是否方便提供？如需协助设计校验逻辑，请提供接口实现片段。

7. **后端：聊天室消息高亮支持**
   - 如需后端推送高亮消息，需支持 payload 或消息类型扩展。
   - 需要确认：后端消息推送接口是否支持自定义 payload？如不支持，请说明接口约束。

8. **其他**
   - 如有多端（如 uniapp/h5/PC），请说明是否都需支持卡秒功能。
   - 如有特殊业务场景或权限控制，请提前说明。

> 在开发过程中，如有任何环节需要补充信息，请及时同步，确保开发顺利进行。

---

## 五、后端卡秒状态接口设计

### 1. 卡秒状态存储
- 使用 Redis，Key 设计为 `Auction:Kasec:{AuctionItemId}`，Value 为 bool（true=卡秒，false=正常）。

### 2. 接口设计

#### 2.1 设置卡秒状态（管理员）
- **接口地址**：`POST /api/AuctionItem/SetKasecStatus`
- **参数**：
  - `auctionItemId` (long)：拍品ID
  - `isKasec` (bool)：是否卡秒
- **权限**：仅拍卖师/管理员可调用
- **功能**：设置指定拍品的卡秒状态，写入 Redis

#### 2.2 获取卡秒状态（所有用户）
- **接口地址**：`GET /api/AuctionItem/GetKasecStatus?auctionItemId=xxx`
- **参数**：
  - `auctionItemId` (long)：拍品ID
- **返回**：
  - `isKasec` (bool)：当前卡秒状态
- **权限**：所有用户可调用
- **功能**：获取指定拍品的卡秒状态

### 3. 出价接口校验
- Bid 方法中，先查 Redis 获取当前拍品卡秒状态。
- 若卡秒，则最低加价为三倍，否则按原有区间加价。
- 校验逻辑与前端保持一致。

### 4. 推荐调用流程
- 管理员点击卡秒按钮时，前端调用 SetKasecStatus。
- 用户页面初始化/切换拍品时，前端调用 GetKasecStatus 获取最新状态。
- 出价时，后端自动校验卡秒状态，无需前端传参。
- 可选：卡秒状态变更时通过 WebSocket 广播，前端自动同步。

### 5. 示例代码片段（C#）

```csharp
// 设置卡秒状态
[HttpPost]
[AbpAuthorize(AppPermissions.Pages.ChatManager)]
public async Task SetKasecStatus(long auctionItemId, bool isKasec)
{
    await _redisClient.Database.StringSetAsync($"Auction:Kasec:{auctionItemId}", isKasec);
}

// 获取卡秒状态
[HttpGet]
public async Task<bool> GetKasecStatus(long auctionItemId)
{
    var val = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
    return val.HasValue && val == "true";
}

// Bid 校验
var isKasec = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{input.AuctionItemId}");
if (isKasec.HasValue && isKasec == "true")
{
    // 最低加价三倍
    minPrice = find.CurrentPrice.Value + ((minPrice - find.CurrentPrice.Value) * 3);
}
```

--- 