# Android FCM 技术文档

## 📱 FCM 概述

**Firebase Cloud Messaging (FCM)** 是 Google 提供的跨平台消息传递解决方案，用于向 Android、iOS 和 Web 应用发送消息。它是 Google Cloud Messaging (GCM) 的继任者。

### 核心特性

- **跨平台**: 支持 Android、iOS、Web
- **高可靠性**: Google 基础设施，全球覆盖
- **免费使用**: 对开发者免费开放
- **丰富功能**: 支持消息优先级、主题订阅、数据消息等
- **易于集成**: 提供 SDK 和 REST API

## 🔑 FCM 架构

```
┌─────────────┐         HTTP           ┌─────────────┐
│   Provider  │  ──────────────────>  │    FCM      │
│  (后端服务)  │    HTTPS/HTTP/2       │  (Google)   │
└─────────────┘                      └─────────────┘
                                              │
                                              ▼
                                       ┌─────────────┐
                                       │   Device    │
                                       │  (用户设备)  │
                                       └─────────────┘
```

### 工作流程

1. **设备注册**: App 向 FCM 生成 Registration Token
2. **Token 上传**: App 将 Registration Token 上传到后端
3. **推送发送**: 后端使用 Server API Key + Registration Token 向 FCM 发送推送
4. **消息传递**: FCM 将消息推送到设备
5. **通知展示**: App 展示通知给用户（通知消息）或接收数据（数据消息）

## 📋 FCM 配置

### 1. 创建 Firebase 项目

1. 访问 [Firebase Console](https://console.firebase.google.com/)
2. 创建新项目
3. 添加 Android 应用
4. 下载 `google-services.json` 文件
5. 添加到 App 的 `app/` 目录

### 2. 配置项目依赖

#### Gradle 配置

```gradle
// 项目根目录 build.gradle
buildscript {
    dependencies {
        classpath 'com.google.gms:google-services:4.4.2'
    }
}

// app/build.gradle
plugins {
    id 'com.google.gms.google-services'
}

dependencies {
    implementation 'com.google.firebase:firebase-messaging:24.1.0'
}
```

### 3. 代码配置

#### Kotlin (Android 端)

```kotlin
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage

// 自定义 FCM Service
class MyFirebaseMessagingService : FirebaseMessagingService() {
    
    override fun onMessageReceived(remoteMessage: RemoteMessage) {
        // 处理接收到的消息
        handleNotification(remoteMessage)
    }
    
    override fun onNewToken(token: String) {
        // 获取新的 Registration Token
        Log.d("FCM", "New token: $token")
        
        // 上传到后端
        sendRegistrationToServer(token)
    }
    
    private fun handleNotification(remoteMessage: RemoteMessage) {
        val notification = remoteMessage.notification
        val data = remoteMessage.data
        
        if (notification != null) {
            // 通知消息
            showNotification(
                notification.title ?: "通知",
                notification.body ?: "",
                data
            )
        } else if (data.isNotEmpty()) {
            // 数据消息
            handleDataMessage(data)
        }
    }
    
    private fun showNotification(title: String, body: String, data: Map<String, String>) {
        val channelId = "default_channel"
        val notificationId = System.currentTimeMillis().toInt()
        
        val builder = NotificationCompat.Builder(this, channelId)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle(title)
            .setContentText(body)
            .setAutoCancel(true)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
        
        // 添加点击 Intent
        val intent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            data.forEach { (key, value) ->
                putExtra(key, value)
            }
        }
        
        val pendingIntent = PendingIntent.getActivity(
            this, 0, intent, PendingIntent.FLAG_IMMUTABLE
        )
        
        builder.setContentIntent(pendingIntent)
        
        // 创建通知渠道（Android 8.0+）
        createNotificationChannel(channelId)
        
        val manager = NotificationManagerCompat.from(this)
        manager.notify(notificationId, builder.build())
    }
    
    private fun createNotificationChannel(channelId: String) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val name = "默认通知渠道"
            val descriptionText = "应用默认通知"
            val importance = NotificationManager.IMPORTANCE_HIGH
            
            val channel = NotificationChannel(
                channelId, name, importance
            ).apply {
                description = descriptionText
            }
            
            val manager = getSystemService(NotificationManager::class.java)
            manager.createNotificationChannel(channel)
        }
    }
    
    private fun sendRegistrationToServer(token: String) {
        // 上传到后端 API
        lifecycleScope.launch {
            try {
                val response = RetrofitClient.apiService.registerDevice(token)
                if (response.isSuccessful) {
                    Log.d("FCM", "Token registered successfully")
                }
            } catch (e: Exception) {
                Log.e("FCM", "Failed to register token", e)
            }
        }
    }
}

// 获取 Token
private fun getFirebaseToken() {
    FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
        if (task.isSuccessful) {
            val token = task.result
            Log.d("FCM", "Firebase token: $token")
            
            // 上传到后端
            sendRegistrationToServer(token)
        }
    }
}
```

#### AndroidManifest.xml

```xml
<manifest>
    <application>
        <!-- FCM Service -->
        <service
            android:name=".MyFirebaseMessagingService"
            android:exported="false">
            <intent-filter>
                <action android:name="com.google.firebase.MESSAGING_EVENT" />
            </intent-filter>
        </service>
    </application>
    
    <!-- 通知权限（Android 13+）-->
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
</manifest>
```

## 🔧 后端集成 (.NET)

### 使用 Firebase Admin SDK

#### 安装 NuGet 包

```bash
dotnet add package FirebaseAdmin
```

#### 配置

```csharp
// appsettings.json
{
  "Firebase": {
    "ProjectId": "your-project-id",
    "ServiceAccountPath": "./certs/firebase-service-account.json"
  }
}
```

#### 实现

```csharp
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

public class FcmNotificationService : ITransientDependency
{
    private readonly FirebaseMessaging _messaging;
    private readonly IConfiguration _configuration;
    
    public FcmNotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // 初始化 Firebase App
        if (FirebaseApp.DefaultInstance == null)
        {
            var credential = GoogleCredential.FromFile(
                _configuration["Firebase:ServiceAccountPath"]
            );
            
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }
        
        _messaging = FirebaseMessaging.DefaultInstance;
    }
    
    public async Task SendPushAsync(string registrationToken, string title, 
                                   string body, Dictionary<string, string> data = null)
    {
        var message = new Message
        {
            Token = registrationToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "default_channel",
                    Sound = "default",
                    NotificationCount = 1
                }
            }
        };
        
        var response = await _messaging.SendAsync(message);
        
        if (string.IsNullOrEmpty(response))
        {
            throw new Exception("FCM push failed");
        }
    }
    
    public async Task SendDataMessageAsync(string registrationToken, 
                                          Dictionary<string, string> data)
    {
        var message = new Message
        {
            Token = registrationToken,
            Data = data,
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Uptime = 3600  // 消息 TTL（秒）
            }
        };
        
        var response = await _messaging.SendAsync(message);
        
        if (string.IsNullOrEmpty(response))
        {
            throw new Exception("FCM data message failed");
        }
    }
    
    public async Task SendMulticastAsync(IEnumerable<string> registrationTokens, 
                                        string title, string body)
    {
        var message = new MulticastMessage
        {
            Tokens = registrationTokens.ToList(),
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "default_channel",
                    Sound = "default"
                }
            }
        };
        
        var response = await _messaging.SendMulticastAsync(message);
        
        if (response.FailureCount > 0)
        {
            var failedTokens = new List<string>();
            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    failedTokens.Add(registrationTokens.ElementAt(i));
                }
            }
            
            // 处理失败的 Token
            await RemoveInvalidTokensAsync(failedTokens);
        }
    }
    
    public async Task SendTopicAsync(string topic, string title, string body)
    {
        var message = new Message
        {
            Topic = topic,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High
            }
        };
        
        var response = await _messaging.SendAsync(message);
        
        if (string.IsNullOrEmpty(response))
        {
            throw new Exception("FCM topic message failed");
        }
    }
    
    private async Task RemoveInvalidTokensAsync(List<string> tokens)
    {
        // 从数据库移除无效 Token
        // ...
    }
}
```

## 📤 推送消息格式

### 通知消息

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "拍卖出价提醒",
      "body": "您关注的拍品刚刚有新出价！"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "default_channel",
        "sound": "default",
        "notification_count": 1
      }
    }
  }
}
```

### 数据消息

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "data": {
      "type": "auction_update",
      "auction_id": "12345",
      "item_id": "67890",
      "current_bid": "10000"
    },
    "android": {
      "priority": "high",
      "ttl": "3600s"
    }
  }
}
```

### 主题消息

```json
{
  "message": {
    "topic": "auction_updates",
    "notification": {
      "title": "新拍品上线",
      "body": "刚刚发布了新的拍品！"
    }
  }
}
```

## 🔒 安全最佳实践

### 1. Service Account 管理

- ✅ 使用 Firebase Console 生成的 Service Account
- ✅ 将密钥文件存储在安全位置（如 Azure Key Vault）
- ✅ 限制 Service Account 的权限
- ✅ 定期轮换密钥

### 2. Registration Token 管理

- ✅ 使用 HTTPS 传输 Token
- ✅ 在后端验证 Token 格式
- ✅ 定期清理无效 Token
- ✅ 处理 Token 刷新

### 3. 消息验证

- ✅ 后端验证消息发送权限
- ✅ 不在消息中包含敏感信息
- ✅ 使用数据字段传递业务数据

### 4. API 密钥保护

- ✅ 不要将 API 密钥暴露在客户端
- ✅ 使用 Server API Key（不要使用 Web API Key）
- ✅ 限制 API 密钥的使用范围

## 📊 性能优化

### 1. 批量发送

```csharp
public async Task SendBatchPushAsync(IEnumerable<string> tokens, string title, string body)
{
    // FCM 支持一次最多 500 个 Token
    const int batchSize = 500;
    
    var batches = tokens
        .Select((token, index) => new { token, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.token));
    
    foreach (var batch in batches)
    {
        await SendMulticastAsync(batch, title, body);
    }
}
```

### 2. 消息优先级

```csharp
var message = new Message
{
    Token = registrationToken,
    Notification = new Notification
    {
        Title = "紧急消息",
        Body = "需要立即处理"
    },
    Android = new AndroidConfig
    {
        Priority = Priority.High  // 高优先级，立即发送
    }
};
```

### 3. 消息 TTL

```csharp
var message = new Message
{
    Token = registrationToken,
    Data = data,
    Android = new AndroidConfig
    {
        // 设置消息过期时间（秒）
        Uptime = 3600  // 1 小时
    }
};
```

### 4. 主题订阅优化

```kotlin
// 订阅主题
FirebaseMessaging.getInstance().subscribeToTopic("auction_updates")
    .addOnCompleteListener { task ->
        if (task.isSuccessful) {
            Log.d("FCM", "Subscribed to topic")
        }
    }

// 取消订阅
FirebaseMessaging.getInstance().unsubscribeFromTopic("auction_updates")
```

## 🎯 高级功能

### 1. 消息优先级

FCM 支持两种优先级：

| 优先级 | 说明 | 使用场景 |
|--------|------|----------|
| `High` | 立即发送，实时性要求高 | 即时消息、紧急通知 |
| `Normal` | 节省电量，批量发送 | 营销消息、非紧急通知 |

```csharp
.Android = new AndroidConfig
{
    Priority = Priority.High  // 或 Priority.Normal
}
```

### 2. 消息分类

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "消息",
      "body": "内容"
    },
    "android": {
      "notification": {
        "channel_id": "bids_channel"
      }
    }
  }
}
```

### 3. 消息目标

#### 单设备

```csharp
var message = new Message
{
    Token = "DEVICE_TOKEN"
};
```

#### 多设备

```csharp
var message = new MulticastMessage
{
    Tokens = new[] { "TOKEN1", "TOKEN2", "TOKEN3" }
};
```

#### 主题

```csharp
var message = new Message
{
    Topic = "auction_updates"
};
```

#### 条件

```csharp
var message = new Message
{
    Condition = "'auction_updates' in topics && 'high_priority' in topics"
};
```

### 4. A/B 测试

```kotlin
// 根据 Token 的最后一位数字分组
val token = FirebaseMessaging.getInstance().token.result
val group = if (token.last().isDigit()) "A" else "B"
```

```csharp
// 后端发送不同的消息
if (group == "A")
{
    await SendPushAsync(token, "版本 A 消息", "内容 A");
}
else
{
    await SendPushAsync(token, "版本 B 消息", "内容 B");
}
```

## 📈 监控与日志

### 推送监控

```csharp
public class FcmMetricsService : ITransientDependency
{
    private readonly IMetrics _metrics;
    
    public void RecordPushSuccess()
    {
        _metrics.Counter("fcm.push.success").Increment();
    }
    
    public void RecordPushFailure(string error)
    {
        _metrics.Counter("fcm.push.failure", new { error }).Increment();
    }
    
    public void RecordInvalidToken()
    {
        _metrics.Counter("fcm.invalid_token").Increment();
    }
}
```

### 日志记录

```csharp
_logger.LogInformation("Sending FCM push to {Token}", registrationToken);
_logger.LogDebug("FCM payload: {Payload}", JsonSerializer.Serialize(message));
```

### Firebase 控制台

- 查看消息发送统计
- 监控送达率
- 分析用户行为

## 🔔 Android 13 通知权限

### 请求权限

```kotlin
private fun requestNotificationPermission() {
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
        ActivityCompat.requestPermissions(
            this,
            arrayOf(Manifest.permission.POST_NOTIFICATIONS),
            REQUEST_NOTIFICATION_PERMISSION
        )
    }
}

override fun onRequestPermissionsResult(
    requestCode: Int,
    permissions: Array<String>,
    grantResults: IntArray
) {
    super.onRequestPermissionsResult(requestCode, permissions, grantResults)
    
    if (requestCode == REQUEST_NOTIFICATION_PERMISSION) {
        if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            // 权限已授予
        } else {
            // 权限被拒绝
            showPermissionDeniedDialog()
        }
    }
}
```

## 🔗 参考资料

- [Firebase Cloud Messaging](https://firebase.google.com/docs/cloud-messaging)
- [Set Up a Firebase Cloud Messaging Client App on Android](https://firebase.google.com/docs/cloud-messaging/android/client)
- [Send a Message to a Specific Device](https://firebase.google.com/docs/cloud-messaging/send-message)
- [Firebase Admin .NET SDK](https://firebase.google.com/docs/admin/setup)
- [Firebase Cloud Messaging REST API](https://firebase.google.com/docs/cloud-messaging/http-server-ref)
