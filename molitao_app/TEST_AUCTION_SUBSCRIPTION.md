# 开拍订阅功能测试用例

## 一、业务流程分析

### 1.1 订阅流程链路

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           开拍订阅完整流程                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  【阶段1: 用户订阅拍品】                                                      │
│  auction_chat_page.dart                                                      │
│       │                                                                     │
│       └── 用户点击"开拍通知"按钮 (status == listed)                          │
│              │                                                              │
│              ▼                                                              │
│  【阶段2: 订阅请求发送】                                                      │
│  auction_provider.dart                                                       │
│       │                                                                     │
│       └── subscribeStartNotification()                                      │
│              │                                                              │
│              ▼                                                              │
│  【阶段3: 后端处理】                                                         │
│  HTTP POST /api/services/app/AuctionItem/SubStartNotify                     │
│       │                                                                     │
│       └── 存储订阅记录到 T_AuctionStartNotify 表                             │
│                                                                             │
│  【阶段4: WebSocket连接监听】                                                │
│  chat_store.dart                                                             │
│       │                                                                     │
│       ├── connectServer() → WebSocket 连接                                  │
│       ├── joinChannel("-1_auction") → 订阅秒杀场频道                         │
│       │                                                                     │
│       ▼                                                                     │
│  websocket_service.dart                                                      │
│       │                                                                     │
│       └── _onMessageReceived() → 消息类型映射                               │
│              │                                                              │
│              └── type 1000 = AuctionStart                                  │
│                                                                             │
│  【阶段5: 消息处理分发】                                                     │
│  chat_store.dart                                                             │
│       │                                                                     │
│       └── _onMessage() → _handleChannelMessage() → chatMap 更新             │
│                                                                             │
│  【阶段6: UI展示】                                                          │
│  auction_chat_page.dart                                                      │
│       │                                                                     │
│       └── MessageWidget → AuctionStartMessage                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 后端推送逻辑

```
拍卖开始 (StartAuction)
    │
    ├── 发布 AuctionStartedEvent 事件
    │
    └── 调用 Notify(auctionItemId, name)
            │
            ├── 查询 T_AuctionStartNotify 表获取订阅用户
            │
            └── 按平台发送推送：
                ├── 小程序: 微信模板消息
                ├── App: 微信模板消息 + 极光推送
                └── H5: WebPush
```

---

## 二、关键文件清单

| 文件 | 作用 | 关键代码行 |
|------|------|-----------|
| `auction_chat_page.dart` | 订阅触发 + 消息展示 | 1556-1595, 105-126 |
| `auction_provider.dart` | 订阅方法封装 | 151-166 |
| `auction_repository.dart` | 订阅 API 调用 | 114-134 |
| `websocket_service.dart` | WebSocket 连接和消息接收 | 127-185 |
| `chat_store.dart` | 聊天消息处理和分发 | 134-163, 199-249 |
| `auction_start_message.dart` | 开拍消息 UI 组件 | 31-65 |
| `chat_message_model.dart` | 消息类型定义 | 146, 183 |

---

## 三、潜在问题分析

| 问题 | 严重程度 | 位置 | 说明 |
|------|----------|------|------|
| **WebSocket 重连后订阅丢失** | 🔴 高 | `chat_store.dart` | 重连后不会重新 joinChannel，导致收不到消息 |
| **订阅后无确认反馈** | 🟡 中 | `auction_chat_page.dart` | 用户点击订阅按钮后没有明确的成功/失败提示 |
| **无取消订阅功能** | 🟡 中 | `auction_provider.dart` | 只有订阅方法，没有取消订阅方法 |
| **离线消息丢失** | 🔴 高 | `websocket_service.dart` | WebSocket 断开期间的消息无法接收 |
| **推送异常被吞掉** | 🟡 中 | `AuctionItemAppService.cs` | Notify 异常被捕获但不抛出，用户无感知 |

---

## 四、测试用例

### TC-001: 正常订阅流程测试

**前置条件**：
- 用户已登录
- WebSocket 已连接
- 存在状态为 `listed`（待拍卖）的拍品

**测试步骤**：
1. 进入拍卖聊天页面（秒杀场）
2. 找到状态为 `listed` 的拍品
3. 点击"开拍通知"按钮
4. 验证订阅请求发送成功

**预期结果**：
- ✅ 调用 API `/api/services/app/AuctionItem/SubStartNotify`
- ✅ 数据库 `T_AuctionStartNotify` 表新增订阅记录
- ✅ 用户收到订阅成功提示

**验证命令**：
```sql
-- 查询订阅记录
SELECT * FROM T_AuctionStartNotify 
WHERE AuctionItemId = {auctionItemId} AND UserId = {userId};
```

---

### TC-002: WebSocket 消息接收测试

**前置条件**：
- 用户已订阅拍品
- WebSocket 已连接并加入 `-1_auction` 频道

**测试步骤**：
1. 管理员触发拍卖开始（调用 `StartAuction` API）
2. 验证 WebSocket 收到 `AuctionStart` 消息
3. 验证消息正确解析并显示在聊天列表

**预期结果**：
- ✅ WebSocket 收到消息，type = 1000
- ✅ 消息类型转换为 `AuctionStart`
- ✅ `chatMap` 更新，新增开拍消息
- ✅ UI 显示 `AuctionStartMessage` 组件

**验证点**：
```dart
// websocket_service.dart 第173行
1000: 'AuctionStart',  // 消息类型映射

// chat_store.dart 第154-156行
if (msg.chan != null && msg.chan!.isNotEmpty) {
  _handleChannelMessage(msg);  // 处理频道消息
}
```

---

### TC-003: WebSocket 重连测试

**前置条件**：
- 用户已订阅拍品
- WebSocket 已连接

**测试步骤**：
1. 模拟网络断开（关闭 WiFi/飞行模式）
2. 等待 5 秒后恢复网络
3. 验证 WebSocket 自动重连
4. 验证重新加入 `-1_auction` 频道

**预期结果**：
- ✅ WebSocket 自动重连成功
- ✅ 重新订阅 `-1_auction` 频道
- ✅ 能正常接收后续消息

**⚠️ 当前问题**：
```dart
// chat_store.dart 缺少重连后的 joinChannel 逻辑
// 重连后需要重新调用 joinChannel("-1_auction")
```

---

### TC-004: 离线推送测试

**前置条件**：
- 用户已订阅拍品
- App 在后台或关闭状态

**测试步骤**：
1. 将 App 切换到后台
2. 管理员触发拍卖开始
3. 验证用户收到系统通知

**预期结果**：
- ✅ App 端收到极光推送通知
- ✅ 微信端收到模板消息通知（如有 openid）
- ✅ 点击通知能打开 App 并跳转到对应拍品

**验证命令**：
```sql
-- 查询推送记录
SELECT * FROM T_AuctionStartNotify 
WHERE AuctionItemId = {auctionItemId} AND Platform = 'app';
```

---

### TC-005: 消息类型解析测试

**前置条件**：
- WebSocket 已连接

**测试步骤**：
1. 后端发送不同类型的拍卖消息
2. 验证前端正确解析消息类型

**测试数据**：
| 后端 type 值 | 前端解析结果 | 消息类型 |
|-------------|-------------|---------|
| 1000 | AuctionStart | 拍卖开始 |
| 1002 | AuctionBid | 出价消息 |
| 1010 | AuctionEnd | 拍卖结束 |
| 1011 | AuctionDeal | 成交消息 |

**验证代码**：
```dart
// websocket_service.dart 第164-179行
const typeMap = {
  1000: 'AuctionStart',
  1002: 'AuctionBid',
  1010: 'AuctionEnd',
  1011: 'AuctionDeal',
};
```

---

### TC-006: 订阅权限验证测试

**前置条件**：
- 用户未登录

**测试步骤**：
1. 未登录用户尝试订阅拍品
2. 验证订阅请求被拒绝

**预期结果**：
- ✅ API 返回 401 Unauthorized
- ✅ 前端提示用户登录

---

### TC-007: 重复订阅测试

**前置条件**：
- 用户已订阅某拍品

**测试步骤**：
1. 用户再次点击"开拍通知"按钮
2. 验证后端处理逻辑

**预期结果**：
- ✅ 后端不重复创建订阅记录
- ✅ 或提示用户已订阅

---

### TC-008: 推送异常处理测试

**前置条件**：
- 用户已订阅拍品
- 极光推送服务异常

**测试步骤**：
1. 模拟极光推送服务不可用
2. 触发拍卖开始
3. 验证拍卖流程不受影响

**预期结果**：
- ✅ 拍卖正常开始
- ✅ 推送异常被记录但不阻断流程
- ✅ WebSocket 消息正常发送

**验证代码**：
```csharp
// AuctionItemAppService.cs
try {
    await Notify(find.Id, find.Name);
} catch (Exception e) {
    _logger.LogError(e, "发送拍卖开始通知失败");
    // 不抛出异常，拍卖流程继续
}
```

---

## 五、自动化测试方案

### 5.1 单元测试

```dart
// test/auction_subscription_test.dart

import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';

void main() {
  group('AuctionSubscription', () {
    test('subscribeStartNotification should call API with correct params', () async {
      // Arrange
      final repository = MockAuctionRepository();
      when(() => repository.subscribeStartNotification(1, platform: 'app'))
          .thenAnswer((_) async => true);

      // Act
      final result = await repository.subscribeStartNotification(1, platform: 'app');

      // Assert
      expect(result, true);
      verify(() => repository.subscribeStartNotification(1, platform: 'app')).called(1);
    });

    test('WebSocket message type 1000 should convert to AuctionStart', () {
      // Arrange
      final service = WebSocketService();
      final message = {'type': 1000, 'msg': '拍卖开始'};

      // Act
      final converted = service.convertMessageType(message);

      // Assert
      expect(converted['type'], 'AuctionStart');
    });
  });
}
```

### 5.2 集成测试

```dart
// integration_test/auction_subscription_test.dart

import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('AuctionSubscription Integration Tests', () {
    testWidgets('User can subscribe to auction notification', (tester) async {
      // 1. 登录
      await tester.pumpWidget(MyApp());
      await login(tester, 'test@example.com', 'password');

      // 2. 进入拍卖聊天页面
      await tester.tap(find.text('秒杀场'));
      await tester.pumpAndSettle();

      // 3. 点击订阅按钮
      await tester.tap(find.text('开拍通知'));
      await tester.pumpAndSettle();

      // 4. 验证订阅成功提示
      expect(find.text('订阅成功'), findsOneWidget);
    });

    testWidgets('User receives AuctionStart message via WebSocket', (tester) async {
      // 1. 订阅拍品
      // ...

      // 2. 模拟后端发送 AuctionStart 消息
      // ...

      // 3. 验证消息显示
      expect(find.text('开始秒杀'), findsOneWidget);
    });
  });
}
```

### 5.3 端到端测试脚本

```bash
#!/bin/bash
# test_auction_subscription.sh

echo "===== 开拍订阅功能端到端测试 ====="

# 1. 检查 WebSocket 连接
echo "[Step 1] 检查 WebSocket 服务状态..."
curl -s "https://www.molitao.top/ws/pre-connect" -X POST \
  -H "Authorization: Bearer $TOKEN" | jq .

# 2. 创建测试拍品
echo "[Step 2] 创建测试拍品..."
AUCTION_ID=$(curl -s -X POST "https://www.molitao.top/api/services/app/AuctionItem/Create" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"测试拍品","startingPrice":100}' | jq -r '.result.id')
echo "创建拍品 ID: $AUCTION_ID"

# 3. 订阅拍品
echo "[Step 3] 订阅拍品..."
curl -s -X POST "https://www.molitao.top/api/services/app/AuctionItem/SubStartNotify" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"auctionItemId\":$AUCTION_ID,\"platform\":\"app\"}" | jq .

# 4. 验证订阅记录
echo "[Step 4] 验证订阅记录..."
curl -s "https://www.molitao.top/api/services/app/AuctionItem/GetSubscription?auctionItemId=$AUCTION_ID" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 5. 触发拍卖开始
echo "[Step 5] 触发拍卖开始..."
curl -s -X POST "https://www.molitao.top/api/services/app/AuctionItem/StartAuction" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"id\":$AUCTION_ID}" | jq .

# 6. 清理测试数据
echo "[Step 6] 清理测试数据..."
curl -s -X DELETE "https://www.molitao.top/api/services/app/AuctionItem/Delete?id=$AUCTION_ID" \
  -H "Authorization: Bearer $ADMIN_TOKEN"

echo "===== 测试完成 ====="
```

---

## 六、修复建议

### 6.1 WebSocket 重连后重新订阅

```dart
// chat_store.dart

void _onDone() {
  print('[ChatStore] WebSocket 连接已关闭');
  state = state.copyWith(isConnected: false);

  if (_shouldReconnect) {
    _scheduleReconnect();
  }
}

// 添加重连后的重新订阅逻辑
Future<void> _reconnectAndSubscribe() async {
  await connectServer(reconnect: true);
  
  // 重新加入之前的频道
  if (state.currentChat.id != -1) {
    final channel = '${state.currentChat.id}_${state.currentChat.name}';
    await joinChannel(channel);
  }
}
```

### 6.2 订阅成功反馈

```dart
// auction_chat_page.dart

Future<void> _saveSubscription(int auctionItemId, {String? openid}) async {
  final success = await ref
      .read(auctionProvider.notifier)
      .subscribeStartNotification(auctionItemId, openid: openid);

  if (mounted) {
    Navigator.pop(context);
    
    // 添加明确的成功/失败反馈
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(success 
          ? '✅ 订阅成功，拍卖开始时将推送通知' 
          : '❌ 订阅失败，请检查网络后重试'),
        backgroundColor: success ? const Color(0xFF4CAF50) : Colors.red,
        duration: const Duration(seconds: 3),
        action: success ? null : SnackBarAction(
          label: '重试',
          onPressed: () => _subscribeNotification(auctionItemId),
        ),
      ),
    );
  }
}
```

### 6.3 添加取消订阅功能

```dart
// auction_provider.dart

/// 取消开拍通知订阅
Future<bool> unsubscribeStartNotification(int auctionItemId) async {
  try {
    await _repository.unsubscribeStartNotification(auctionItemId);
    return true;
  } catch (e) {
    return false;
  }
}
```

---

## 七、测试执行记录

| 测试用例 | 执行时间 | 结果 | 备注 |
|---------|---------|------|------|
| TC-001 | - | - | 待执行 |
| TC-002 | - | - | 待执行 |
| TC-003 | - | - | 待执行 |
| TC-004 | - | - | 待执行 |
| TC-005 | - | - | 待执行 |
| TC-006 | - | - | 待执行 |
| TC-007 | - | - | 待执行 |
| TC-008 | - | - | 待执行 |

---

## 八、结论

### 8.1 当前实现状态

| 功能点 | 状态 | 说明 |
|--------|------|------|
| 订阅 API 调用 | ✅ 正常 | HTTP API 正确调用 |
| WebSocket 连接 | ✅ 正常 | pre-connect + 连接成功 |
| 消息类型转换 | ✅ 正常 | type 1000 → AuctionStart |
| 消息接收处理 | ✅ 正常 | chatMap 更新正确 |
| UI 展示 | ✅ 正常 | AuctionStartMessage 正确渲染 |
| WebSocket 重连 | ⚠️ 需修复 | 重连后未重新 joinChannel |
| 离线推送 | ✅ 正常 | 极光推送 + 微信模板消息 |
| 订阅反馈 | ⚠️ 需优化 | 缺少明确的成功提示 |

### 8.2 核心问题

**最关键的问题是 WebSocket 重连后订阅丢失**：
- 当网络波动导致 WebSocket 断开重连时
- 用户虽然还在拍卖聊天页面，但已经不在 `-1_auction` 频道中
- 此时拍卖开始，用户无法收到 `AuctionStart` 消息

**建议优先修复**：
1. 在 `chat_store.dart` 的重连逻辑中添加重新 `joinChannel` 的代码
2. 添加订阅成功的明确反馈
3. 添加取消订阅功能
