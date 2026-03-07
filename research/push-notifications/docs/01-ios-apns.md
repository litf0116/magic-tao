# iOS APNs 技术文档

## 📱 APNs 概述

**Apple Push Notification Service (APNs)** 是苹果官方提供的推送通知服务，用于向 iOS、iPadOS、macOS、watchOS 和 tvOS 设备发送远程通知。

### 核心特性

- **可靠性强**: 官方服务，稳定可靠
- **安全性高**: 使用 TLS 加密通信
- **支持丰富**: 支持静默推送、后台更新、富媒体通知
- **免费使用**: 对开发者免费开放

## 🔑 APNs 架构

```
┌─────────────┐         TLS          ┌─────────────┐
│   Provider  │  ─────────────────>  │    APNs     │
│  (后端服务)  │    HTTP/2 (JSON)    │   (Apple)   │
└─────────────┘                      └─────────────┘
                                              │
                                              ▼
                                       ┌─────────────┐
                                       │   Device    │
                                       │  (用户设备)  │
                                       └─────────────┘
```

### 工作流程

1. **设备注册**: App 向 APNs 请求 Device Token
2. **Token 上传**: App 将 Device Token 上传到后端
3. **推送发送**: 后端使用 Provider Certificate + Device Token 向 APNs 发送推送
4. **消息传递**: APNs 将消息推送到设备
5. **通知展示**: 系统展示通知给用户

## 📋 APNs 配置

### 1. 生成推送证书

#### 方式 A：PKCS#12 (.p12) 证书（已弃用）

```bash
# 在 Apple Developer 创建推送证书
# 下载 .cer 文件
# 导出为 .p12 格式
```

#### 方式 B：APNs Auth Key（推荐）

1. 在 Apple Developer 创建 "Keys"
2. 选择 APNs 类型
3. 下载 `.p8` 文件（只能下载一次！）
4. 记录 Key ID 和 Team ID

**推荐使用 APNs Auth Key 的原因**：
- 无需定期更新证书
- 支持多个 App 共用一个 Key
- 管理更简单

### 2. 配置 App 能力

在 Xcode 中启用 Push Notifications capability：
- `Signing & Capabilities` → `+ Capability` → `Push Notifications`

### 3. 代码配置

#### Swift (iOS 端)

```swift
import UserNotifications

// 注册推送通知
func registerForPushNotifications() {
    let center = UNUserNotificationCenter.current()
    center.delegate = self
    
    // 请求权限
    center.requestAuthorization(options: [.alert, .sound, .badge]) { granted, error in
        if granted {
            DispatchQueue.main.async {
                UIApplication.shared.registerForRemoteNotifications()
            }
        }
    }
}

// 获取 Device Token
func application(_ application: UIApplication, 
                 didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
    let token = deviceToken.map { String(format: "%02.2hhx", $0) }.joined()
    print("Device Token: \(token)")
    
    // 上传到后端
    uploadDeviceToken(token: token)
}

// 处理推送失败
func application(_ application: UIApplication, 
                 didFailToRegisterForRemoteNotificationsWithError error: Error) {
    print("Failed to register: \(error)")
}

// 处理收到的推送
func userNotificationCenter(_ center: UNUserNotificationCenter, 
                           didReceive response: UNNotificationResponse, 
                           withCompletionHandler completionHandler: @escaping () -> Void) {
    let userInfo = response.notification.request.content.userInfo
    print("Received notification: \(userInfo)")
    completionHandler()
}
```

## 🔧 后端集成 (.NET)

### 使用 dotAPNS 库

#### 安装 NuGet 包

```bash
dotnet add package DotAPNS
```

#### 配置

```csharp
// appsettings.json
{
  "Apns": {
    "KeyId": "YOUR_KEY_ID",
    "TeamId": "YOUR_TEAM_ID",
    "BundleId": "com.yourcompany.yourapp",
    "PrivateKeyPath": "./certs/AuthKey_YOUR_KEY_ID.p8",
    "UseSandbox": false
  }
}
```

#### 实现

```csharp
using DotAPNS;
using DotAPNS.Args;

public class ApnsNotificationService : ITransientDependency
{
    private readonly ApnsClient _apnsClient;
    private readonly IConfiguration _configuration;
    
    public ApnsNotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var apnsConfig = new ApnsConfig(
            _configuration["Apns:KeyId"],
            _configuration["Apns:TeamId"],
            _configuration["Apns:BundleId"],
            _configuration["Apns:PrivateKeyPath"],
            useSandbox: bool.Parse(_configuration["Apns:UseSandbox"])
        );
        
        _apnsClient = new ApnsClient(apnsConfig);
    }
    
    public async Task SendPushAsync(string deviceToken, string title, string body, 
                                   Dictionary<string, object> customData = null)
    {
        var payload = new ApnsPayload
        {
            Aps = new Aps
            {
                Alert = new ApsAlert
                {
                    Title = title,
                    Body = body
                },
                Sound = "default",
                Badge = 1
            },
            Custom = customData ?? new Dictionary<string, object>()
        };
        
        var notification = new ApnsNotification(deviceToken, payload);
        var response = await _apnsClient.SendAsync(notification);
        
        if (!response.IsSuccess)
        {
            throw new Exception($"APNs push failed: {response.Reason}");
        }
    }
    
    public async Task SendSilentPushAsync(string deviceToken, 
                                         Dictionary<string, object> customData)
    {
        var payload = new ApnsPayload
        {
            Aps = new Aps
            {
                ContentAvailable = 1
            },
            Custom = customData
        };
        
        var notification = new ApnsNotification(deviceToken, payload);
        var response = await _apnsClient.SendAsync(notification);
        
        if (!response.IsSuccess)
        {
            throw new Exception($"APNs silent push failed: {response.Reason}");
        }
    }
}
```

## 📤 推送消息格式

### 标准 JSON 格式

```json
{
  "aps": {
    "alert": {
      "title": "拍卖出价提醒",
      "body": "您关注的拍品刚刚有新出价！"
    },
    "sound": "default",
    "badge": 1,
    "category": "BID_UPDATE"
  },
  "auctionId": "12345",
  "itemId": "67890",
  "bidAmount": 10000
}
```

### 字段说明

| 字段 | 类型 | 说明 | 必需 |
|------|------|------|------|
| `aps` | Object | Apple 定义的标准字段 | ✅ |
| `aps.alert` | Object/String | 提示内容 | ✅ |
| `aps.sound` | String | 提示音（默认 "default"） | ❌ |
| `aps.badge` | Number | 应用角标数 | ❌ |
| `aps.content-available` | Number | 静默推送（设为 1） | ❌ |
| `aps.category` | String | 通知分类（用于可交互通知） | ❌ |
| `custom` | Object | 自定义数据 | ❌ |

### 富媒体通知

```json
{
  "aps": {
    "alert": {
      "title": "新消息",
      "body": "张三发送了一条消息"
    },
    "mutable-content": 1,
    "sound": "default"
  },
  "image-url": "https://example.com/image.jpg",
  "message-type": "text"
}
```

## 🔒 安全最佳实践

### 1. 密钥管理

- ✅ 使用 APNs Auth Key (.p8) 代替证书
- ✅ 将密钥文件存储在安全位置（如 Azure Key Vault）
- ✅ 不要将密钥提交到版本控制
- ✅ 限制密钥访问权限

### 2. Device Token 管理

- ✅ 使用 HTTPS 传输 Device Token
- ✅ 在后端验证 Token 格式
- ✅ 定期清理无效 Token
- ✅ 处理 Token 更新场景

### 3. 消息内容安全

- ✅ 不要在推送中包含敏感信息
- ✅ 使用自定义字段传递业务数据
- ✅ 后端验证消息接收者权限

## 📊 性能优化

### 1. 批量发送

```csharp
public async Task SendBatchPushAsync(IEnumerable<string> deviceTokens, 
                                    string title, string body)
{
    var tasks = deviceTokens.Select(token => 
        SendPushAsync(token, title, body)
    );
    
    await Task.WhenAll(tasks);
}
```

### 2. 连接复用

dotAPNS 内部自动管理 HTTP/2 连接池，无需手动优化。

### 3. 错误处理

```csharp
public async Task SendPushWithErrorHandling(string deviceToken, 
                                           string title, string body)
{
    try
    {
        await SendPushAsync(deviceToken, title, body);
    }
    catch (ApnsException ex)
    {
        // 处理 APNs 特定错误
        switch (ex.Reason)
        {
            case "Unregistered":
                // Token 无效，从数据库移除
                await RemoveDeviceTokenAsync(deviceToken);
                break;
            case "BadDeviceToken":
                // Token 格式错误
                await MarkDeviceTokenInvalidAsync(deviceToken);
                break;
            default:
                // 其他错误
                LogError(ex);
                break;
        }
    }
}
```

## 🎯 高级功能

### 1. 静默推送

```csharp
// 唤醒 App 并在后台更新数据
await SendSilentPushAsync(deviceToken, new Dictionary<string, object>
{
    { "type", "data_update" },
    { "timestamp", DateTime.UtcNow.ToString("o") }
});
```

### 2. 可交互通知

```swift
// iOS 端定义 Action
let acceptAction = UNNotificationAction(
    identifier: "ACCEPT_BID",
    title: "接受",
    options: .foreground
)

let declineAction = UNNotificationAction(
    identifier: "DECLINE_BID",
    title: "拒绝",
    options: .destructive
)

let category = UNNotificationCategory(
    identifier: "BID_INVITATION",
    actions: [acceptAction, declineAction],
    intentIdentifiers: []
)

UNUserNotificationCenter.current().setNotificationCategories([category])
```

### 3. 后台更新

```json
{
  "aps": {
    "content-available": 1
  },
  "update-type": "auction_status"
}
```

## 📈 监控与日志

### 推送成功率监控

```csharp
public class PushMetricsService : ITransientDependency
{
    private readonly IMetrics _metrics;
    
    public void RecordPushSuccess(string platform)
    {
        _metrics.Counter("push.success", new
        {
            platform = platform
        }).Increment();
    }
    
    public void RecordPushFailure(string platform, string reason)
    {
        _metrics.Counter("push.failure", new
        {
            platform = platform,
            reason = reason
        }).Increment();
    }
}
```

### 日志记录

```csharp
_logger.LogInformation("Sending push notification to {DeviceToken}", deviceToken);
_logger.LogDebug("Push payload: {Payload}", JsonSerializer.Serialize(payload));
```

## 🔗 参考资料

- [Apple Developer - Local and Remote Notification Programming](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server)
- [Apple Developer - Generating a Remote Notification](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server/generating_a_remote_notification)
- [APNs Provider API](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server/sending_notification_requests_to_apns)
- [dotAPNS GitHub](https://github.com/alexalok/dotAPNS)
