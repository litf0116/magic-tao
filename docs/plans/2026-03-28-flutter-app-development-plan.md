# 魔力淘 Flutter App 开发计划

> **项目名称**: molitao_app  
> **目标**: 将现有 UniApp 项目迁移到 Flutter，实现 Android/iOS 原生 App  
> **团队**: 有 Flutter 开发经验  
> **预计工期**: 2-3 周  
> **创建时间**: 2026-03-28

---

## 一、项目概述

### 1.1 App 定位
- **名称**: 魔力淘
- **类型**: 综合拍卖交易平台
- **核心功能**: 拍卖、聊天、交易站、用户中心

### 1.2 技术架构

```
┌─────────────────────────────────────────────────┐
│                    Flutter App                    │
├─────────────────────────────────────────────────┤
│  Presentation Layer (UI)                        │
│  ├── Pages (首页、聊天、交易站、个人中心)         │
│  ├── Widgets (组件库)                           │
│  └── State Management (Riverpod)                │
├─────────────────────────────────────────────────┤
│  Domain Layer (业务逻辑)                        │
│  ├── Entities (实体模型)                        │
│  ├── Repositories (仓库接口)                    │
│  └── Use Cases (用例)                           │
├─────────────────────────────────────────────────┤
│  Data Layer (数据层)                            │
│  ├── API Service (HTTP 客户端)                  │
│  ├── Models (数据模型)                          │
│  ├── WebSocket Service (实时通信)               │
│  └── Local Storage (本地缓存)                   │
├─────────────────────────────────────────────────┤
│  Core (核心功能)                                │
│  ├── Utils (工具类)                             │
│  ├── Constants (常量)                           │
│  └── Theme (主题配置)                           │
└─────────────────────────────────────────────────┘
```

### 1.3 技术栈

| 组件 | 技术选型 | 说明 |
|---|---|---|
| 框架 | Flutter 3.x | 跨平台原生框架 |
| 语言 | Dart 3.x | 类型安全语言 |
| 状态管理 | Riverpod | 响应式状态管理 |
| 网络请求 | Dio | HTTP 客户端 |
| WebSocket | web_socket_channel | 实时通信 |
| 路由 | GoRouter | 声明式路由 |
| 本地存储 | SharedPreferences / Hive | 持久化存储 |
| UI 组件 | Material Design 3 | 官方设计规范 |
| 图片加载 | cached_network_image | 图片缓存 |
| 消息推送 | Firebase Messaging / 极光推送 | 推送服务 |

---

## 二、功能模块规划

### 2.1 底部导航 (TabBar)

| 序号 | Tab | 图标 | 说明 |
|---|---|---|---|
| 1 | 首页 | tab1.png | 拍卖列表、公告 |
| 2 | 会话列表 | tab2.png | 聊天会话管理 |
| 3 | 交易站 | tab3.png | 买卖帖子发布 |
| 4 | 通讯录 | tab3.png | 好友列表 |
| 5 | 个人中心 | tab4.png | 用户信息、设置 |

### 2.2 页面列表

#### 首页模块
- [x] `HomePage` - 首页（拍卖列表）
- [x] `AuctionDetailPage` - 拍卖详情页
- [x] `AnnouncePage` - 公告列表页

#### 会话模块
- [x] `ChatListPage` - 会话列表
- [x] `GroupChatPage` - 群聊页面
- [x] `PrivateChatPage` - 私聊页面
- [x] `AuctionChatPage` - 秒杀场聊天

#### 交易站模块
- [x] `TradingPostPage` - 交易站首页
- [x] `PostDetailPage` - 帖子详情
- [x] `AddPostPage` - 发布帖子

#### 通讯录模块
- [x] `ContactsPage` - 联系人列表
- [x] `UserProfilePage` - 用户资料页

#### 个人中心模块
- [x] `MyPage` - 个人中心
- [x] `LoginPage` - 登录页面
- [x] `UserInfoPage` - 用户信息编辑
- [x] `BalanceLogPage` - 余额明细
- [x] `DepositLogPage` - 魔力值明细
- [x] `AuctionSuccessPage` - 已成交列表

---

## 三、API 迁移计划

### 3.1 现有 API 结构 (api.ts)

```typescript
// API 基础配置
const host = 'https://www.molitao.top'

// 主要模块
1. TokenAuth - 认证模块
2. User - 用户模块
3. ws - WebSocket 模块
4. auctionItem - 拍卖模块
5. chatEmoji - 表情模块
6. client - 客户端模块
7. post - 交易站帖子模块
8. message - 消息模块
```

### 3.2 Flutter API 对应

| UniApp API | Flutter 实现 |
|---|---|
| `api.tokenAuth` | `AuthService` |
| `api.user` | `UserRepository` |
| `api.ws` | `WebSocketService` |
| `api.auctionItem` | `AuctionRepository` |
| `api.chatEmoji` | `ChatEmojiRepository` |
| `api.post` | `PostRepository` |
| `api.message` | `MessageRepository` |

### 3.3 API 服务层设计

```dart
// lib/data/api/api_client.dart
class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  late Dio _dio;
  
  factory ApiClient() => _instance;
  
  ApiClient._internal() {
    _dio = Dio(BaseOptions(
      baseUrl: 'https://www.molitao.top',
      connectTimeout: Duration(seconds: 30),
      receiveTimeout: Duration(minutes: 5),
    ));
    
    // 添加拦截器
    _dio.interceptors.add(AuthInterceptor());
    _dio.interceptors.add(LogInterceptor());
  }
  
  // API 方法...
}
```

---

## 四、状态管理设计

### 4.1 Riverpod Provider 列表

| Provider | 类型 | 说明 |
|---|---|---|
| `userProvider` | StateNotifierProvider | 用户状态 |
| `chatProvider` | StateNotifierProvider | 聊天状态 |
| `auctionProvider` | StateNotifierProvider | 拍卖状态 |
| `themeProvider` | StateProvider | 主题状态 |
| `localeProvider` | StateProvider | 语言状态 |

### 4.2 状态类设计

```dart
// lib/presentation/providers/user_provider.dart
class UserState {
  final User? user;
  final String token;
  final bool isLogin;
  final bool isLoading;
  
  const UserState({
    this.user,
    this.token = '',
    this.isLogin = false,
    this.isLoading = false,
  });
  
  UserState copyWith({...}) => UserState(...);
}

class UserNotifier extends StateNotifier<UserState> {
  final UserRepository _repository;
  
  UserNotifier(this._repository) : super(UserState());
  
  Future<void> login(String phone, String code) async {
    state = state.copyWith(isLoading: true);
    try {
      final result = await _repository.login(phone, code);
      state = state.copyWith(user: result.user, token: result.token, isLogin: true);
    } catch (e) {
      // 错误处理
    } finally {
      state = state.copyWith(isLoading: false);
    }
  }
}
```

---

## 五、WebSocket 实时通信

### 5.1 SignalR 协议实现

现有项目使用 SignalR 协议进行 WebSocket 通信。Flutter 端需要实现：

```dart
// lib/data/services/websocket_service.dart
class WebSocketService {
  WebSocketChannel? _channel;
  String? _connectionId;
  Timer? _pingTimer;
  
  // 连接服务器
  Future<void> connect(String token) async {
    // 1. Negotiate 获取 connectionId
    final negotiateResponse = await _negotiate(token);
    _connectionId = negotiateResponse.connectionId;
    
    // 2. 建立 WebSocket 连接
    _channel = WebSocketChannel.connect(
      Uri.parse('wss://www.molitao.top/ws?connectionId=$_connectionId'),
    );
    
    // 3. 监听消息
    _channel!.stream.listen(_handleMessage);
    
    // 4. 启动 Ping 定时器
    _startPing();
  }
  
  // 发送消息
  Future<void> sendMessage(ChatMessage message) async {
    final payload = {
      'type': 1, // Invocation
      'target': 'SendMessage',
      'arguments': [message.toJson()],
    };
    _channel?.sink.add(jsonEncode(payload));
  }
  
  // 处理接收消息
  void _handleMessage(dynamic data) {
    final message = jsonDecode(data);
    // 解析 SignalR 协议消息
    // 触发相应事件
  }
}
```

---

## 六、开发阶段规划

### 阶段 1: 项目骨架搭建 (第 1-3 天)

#### Day 1: 项目初始化
- [ ] 创建 Flutter 项目 `molitao_app`
- [ ] 配置项目依赖 (pubspec.yaml)
- [ ] 设置目录结构
- [ ] 配置主题系统
- [ ] 配置路由系统 (GoRouter)

#### Day 2: 核心基础设施
- [ ] 实现 API 客户端 (Dio)
- [ ] 实现本地存储服务
- [ ] 实现用户认证拦截器
- [ ] 实现基础组件库

#### Day 3: 状态管理
- [ ] 实现用户状态管理 (Riverpod)
- [ ] 实现主题状态管理
- [ ] 实现语言状态管理
- [ ] 测试状态管理

### 阶段 2: 核心功能开发 (第 4-10 天)

#### Day 4-5: 登录与用户中心
- [ ] 实现登录页面
- [ ] 实现微信登录
- [ ] 实现手机号登录
- [ ] 实现个人中心页面
- [ ] 实现用户信息编辑

#### Day 6-7: 首页与拍卖
- [ ] 实现首页布局
- [ ] 实现拍卖列表
- [ ] 实现拍卖详情页
- [ ] 实现出价功能
- [ ] 实现公告列表

#### Day 8-9: 聊天功能
- [ ] 实现 WebSocket 服务
- [ ] 实现会话列表
- [ ] 实现群聊页面
- [ ] 实现私聊页面
- [ ] 实现消息发送/接收
- [ ] 实现表情功能

#### Day 10: 交易站
- [ ] 实现交易站列表
- [ ] 实现帖子详情
- [ ] 实现发布帖子
- [ ] 实现图片上传

### 阶段 3: 完善与优化 (第 11-14 天)

#### Day 11: 通讯录与好友
- [ ] 实现通讯录页面
- [ ] 实现好友申请
- [ ] 实现用户资料页

#### Day 12: 钱包与支付
- [ ] 实现余额明细
- [ ] 实现魔力值明细
- [ ] 实现已成交列表
- [ ] 集成微信支付

#### Day 13: 消息推送
- [ ] 集成极光推送
- [ ] 实现推送通知处理
- [ ] 实现应用更新检查

#### Day 14: 测试与修复
- [ ] 功能测试
- [ ] 性能优化
- [ ] Bug 修复
- [ ] 发布准备

---

## 七、目录结构设计

```
molitao_app/
├── android/
├── ios/
├── lib/
│   ├── main.dart
│   ├── app.dart
│   │
│   ├── core/
│   │   ├── constants/
│   │   │   ├── api_constants.dart
│   │   │   ├── app_constants.dart
│   │   │   └── storage_keys.dart
│   │   ├── utils/
│   │   │   ├── date_utils.dart
│   │   │   ├── image_utils.dart
│   │   │   └── validation_utils.dart
│   │   ├── theme/
│   │   │   ├── app_theme.dart
│   │   │   ├── app_colors.dart
│   │   │   └── app_text_styles.dart
│   │   └── router/
│   │       └── app_router.dart
│   │
│   ├── data/
│   │   ├── api/
│   │   │   ├── api_client.dart
│   │   │   ├── auth_interceptor.dart
│   │   │   └── api_endpoints.dart
│   │   ├── models/
│   │   │   ├── user_model.dart
│   │   │   ├── chat_message_model.dart
│   │   │   ├── auction_item_model.dart
│   │   │   └── post_model.dart
│   │   ├── repositories/
│   │   │   ├── auth_repository.dart
│   │   │   ├── user_repository.dart
│   │   │   ├── chat_repository.dart
│   │   │   ├── auction_repository.dart
│   │   │   └── post_repository.dart
│   │   └── services/
│   │       ├── websocket_service.dart
│   │       ├── storage_service.dart
│   │       └── push_service.dart
│   │
│   ├── domain/
│   │   ├── entities/
│   │   │   ├── user.dart
│   │   │   ├── chat_message.dart
│   │   │   ├── auction_item.dart
│   │   │   └── post.dart
│   │   └── repositories/
│   │       └── i_auth_repository.dart
│   │
│   └── presentation/
│       ├── providers/
│       │   ├── user_provider.dart
│       │   ├── chat_provider.dart
│       │   └── auction_provider.dart
│       ├── pages/
│       │   ├── home/
│       │   │   ├── home_page.dart
│       │   │   └── auction_detail_page.dart
│       │   ├── chat/
│       │   │   ├── chat_list_page.dart
│       │   │   ├── group_chat_page.dart
│       │   │   └── private_chat_page.dart
│       │   ├── trading_post/
│       │   │   ├── trading_post_page.dart
│       │   │   ├── post_detail_page.dart
│       │   │   └── add_post_page.dart
│       │   ├── contacts/
│       │   │   └── contacts_page.dart
│       │   ├── profile/
│       │   │   ├── my_page.dart
│       │   │   ├── login_page.dart
│       │   │   └── user_info_page.dart
│       │   └── tabbar/
│       │       └── main_tab_page.dart
│       └── widgets/
│           ├── common/
│           │   ├── loading_widget.dart
│           │   ├── empty_widget.dart
│           │   └── error_widget.dart
│           ├── chat/
│           │   ├── message_bubble.dart
│           │   └── chat_input.dart
│           └── auction/
│               └── auction_card.dart
│
├── assets/
│   └── images/
│       ├── tab1.png
│       ├── tab1_b.png
│       ├── tab2.png
│       ├── tab2_b.png
│       └── ...
│
├── test/
│   ├── unit/
│   ├── widget/
│   └── integration/
│
├── pubspec.yaml
├── analysis_options.yaml
└── README.md
```

---

## 八、依赖配置

### pubspec.yaml

```yaml
name: molitao_app
description: 魔力淘 - 在线拍卖交易平台
version: 1.0.0+1

environment:
  sdk: '>=3.0.0 <4.0.0'

dependencies:
  flutter:
    sdk: flutter
  
  # 状态管理
  flutter_riverpod: ^2.4.0
  
  # 网络请求
  dio: ^5.3.0
  
  # 路由
  go_router: ^12.0.0
  
  # 本地存储
  shared_preferences: ^2.2.0
  hive: ^2.2.3
  hive_flutter: ^1.1.0
  
  # UI 组件
  cached_network_image: ^3.3.0
  flutter_svg: ^2.0.7
  
  # 工具库
  intl: ^0.18.1
  json_annotation: ^4.8.1
  freezed_annotation: ^2.4.1
  
  # WebSocket
  web_socket_channel: ^2.4.0
  
  # 推送
  firebase_core: ^2.24.0
  firebase_messaging: ^14.7.0
  
  # 微信登录/支付
  fluwx: ^4.4.0

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^3.0.0
  build_runner: ^2.4.7
  json_serializable: ^6.7.1
  freezed: ^2.4.5
  hive_generator: ^2.0.1

flutter:
  uses-material-design: true
  assets:
    - assets/images/
```

---

## 九、关键实现要点

### 9.1 微信登录集成

```dart
// lib/data/services/wechat_service.dart
class WeChatService {
  static Future<void> init() async {
    await Fluwx().registerApi(
      appId: 'wx8178f2258942133d',
      universalLink: 'https://www.molitao.top/app/',
    );
  }
  
  static Future<WeChatAuthResponse> login() async {
    final result = await Fluwx.instance.authBy(
      which: NormalAuth(
        scope: 'snsapi_userinfo',
        state: 'wechat_sdk_demo_test',
      ),
    );
    return result;
  }
}
```

### 9.2 极光推送集成

```dart
// lib/data/services/push_service.dart
class PushService {
  static Future<void> init() async {
    // 初始化极光推送
    await JPush().setup(
      appKey: '4e91398522bb1286f6452efb',
      channel: 'developer-default',
    );
    
    // 监听推送消息
    JPush().addEventHandler(
      onReceiveNotification: (event) {
        // 处理通知
      },
      onOpenNotification: (event) {
        // 处理通知点击
      },
    );
  }
  
  static Future<String?> getRegistrationId() async {
    return await JPush().getRegistrationID();
  }
}
```

### 9.3 WebSocket 消息处理

```dart
// lib/data/services/websocket_service.dart
class WebSocketService {
  final _messageController = StreamController<ChatMessage>.broadcast();
  Stream<ChatMessage> get messages => _messageController.stream;
  
  void _handleMessage(Map<String, dynamic> data) {
    final type = data['type'] as int;
    
    switch (type) {
      case 1: // Invocation
        _handleInvocation(data);
        break;
      case 6: // Ping
        _sendPong();
        break;
      case 7: // Close
        _handleClose();
        break;
    }
  }
  
  void _handleInvocation(Map<String, dynamic> data) {
    final target = data['target'] as String;
    final arguments = data['arguments'] as List;
    
    switch (target) {
      case 'ReceiveMessage':
        final message = ChatMessage.fromJson(arguments.first);
        _messageController.add(message);
        break;
      case 'ReceiveChannelMessage':
        // 处理频道消息
        break;
    }
  }
}
```

---

## 十、测试计划

### 10.1 单元测试

- [ ] API 客户端测试
- [ ] 状态管理测试
- [ ] 数据模型测试
- [ ] 工具类测试

### 10.2 Widget 测试

- [ ] 页面组件测试
- [ ] 通用组件测试
- [ ] 交互测试

### 10.3 集成测试

- [ ] 登录流程测试
- [ ] 聊天流程测试
- [ ] 拍卖流程测试
- [ ] 支付流程测试

---

## 十一、发布计划

### Android 发布

1. 配置签名
2. 构建 Release APK/AAB
3. 上传 Google Play / 应用宝

### iOS 发布

1. 配置证书
2. 构建 IPA
3. 上传 App Store

---

## 十二、风险评估与应对

| 风险 | 影响 | 应对措施 |
|---|---|---|
| WebSocket 兼容性 | 高 | 提前验证 SignalR 协议实现 |
| 微信登录/支付集成 | 高 | 使用成熟插件 fluwx |
| 极光推送集成 | 中 | 官方 Flutter 插件 |
| UI 一致性 | 中 | 详细设计规范 + 截图对比 |
| 性能问题 | 中 | 性能监控 + 优化 |

---

## 十三、后续优化

- [ ] 暗黑模式支持
- [ ] 多语言支持
- [ ] 无障碍支持
- [ ] 性能监控
- [ ] 崩溃分析
- [ ] A/B 测试

---

**计划状态**: 📋 待执行  
**下一步**: 创建 Flutter 项目并开始阶段 1 开发
