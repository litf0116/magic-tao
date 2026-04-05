# .NET 后端集成方案

## 🏗️ 架构设计

### 推送系统架构

```
┌─────────────────────────────────────────────────────────────┐
│                     应用层 (Application Layer)               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ 拍卖服务      │  │ 用户服务      │  │ 通知服务      │      │
│  │              │  │              │  │              │      │
│  │ - 创建拍卖    │  │ - 用户注册    │  │ - 发送通知    │      │
│  │ - 出价处理    │  │ - Token管理  │  │ - 消息模板    │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
└─────────┼──────────────────┼──────────────────┼────────────┘
          │                  │                  │
┌─────────▼──────────────────▼──────────────────▼────────────┐
│                   领域层 (Domain Layer)                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           PushNotificationDomainService              │  │
│  │                                                       │  │
│  │ - 统一推送接口                                        │  │
│  │ - 消息模板管理                                        │  │
│  │ - 目标用户选择                                        │  │
│  │ - 推送队列管理                                        │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
          │
┌─────────▼──────────────────────────────────────────────────┐
│              基础设施层 (Infrastructure Layer)              │
│  ┌──────────────┐           ┌──────────────┐              │
│  │ ApnsProvider │           │ FcmProvider  │              │
│  │              │           │              │              │
│  │ - APNs SDK   │           │ - Firebase   │              │
│  │ - HTTP/2     │           │   Admin SDK  │              │
│  │ - Token验证  │           │ - Token管理  │              │
│  └──────────────┘           └──────────────┘              │
│          │                           │                    │
│          └───────────┬───────────────┘                    │
┌──────────────────────▼──────────────────────────────────────┐
│                    外部服务 (External Services)            │
│  ┌──────────────┐           ┌──────────────┐              │
│  │ Apple APNs   │           │ Google FCM    │              │
│  │              │           │              │              │
│  │ - iOS推送    │           │ - Android推送│              │
│  │ - 静默推送    │           │ - 主题订阅    │              │
│  │ - 富媒体通知  │           │ - 数据消息    │              │
│  └──────────────┘           └──────────────┘              │
└────────────────────────────────────────────────────────────┘
```

## 📦 依赖安装

### NuGet 包

```bash
# APNs
dotnet add package DotAPNS

# FCM
dotnet add package FirebaseAdmin

# ABP Framework
dotnet add package Volo.Abp.Ddd.Domain
dotnet add package Volo.Abp.BackgroundJobs
dotnet add package Volo.Abp.Caching
```

## 🔧 配置管理

### appsettings.json

```json
{
  "PushNotification": {
    "Enabled": true,
    "Providers": {
      "Apns": {
        "Enabled": true,
        "KeyId": "YOUR_KEY_ID",
        "TeamId": "YOUR_TEAM_ID",
        "BundleId": "com.molitao.app",
        "PrivateKeyPath": "./certs/AuthKey_YOUR_KEY_ID.p8",
        "UseSandbox": false
      },
      "Fcm": {
        "Enabled": true,
        "ProjectId": "your-project-id",
        "ServiceAccountPath": "./certs/firebase-service-account.json"
      }
    },
    "Settings": {
      "MaxRetries": 3,
      "RetryDelaySeconds": 5,
      "BatchSize": 500,
      "MessageTtlMinutes": 60
    }
  }
}
```

### 配置选项类

```csharp
namespace TtWork.Project.PushNotifications.Options
{
    public class PushNotificationOptions
    {
        public bool Enabled { get; set; }
        public ApnsOptions Apns { get; set; }
        public FcmOptions Fcm { get; set; }
        public PushSettings Settings { get; set; }
    }

    public class ApnsOptions
    {
        public bool Enabled { get; set; }
        public string KeyId { get; set; }
        public string TeamId { get; set; }
        public string BundleId { get; set; }
        public string PrivateKeyPath { get; set; }
        public bool UseSandbox { get; set; }
    }

    public class FcmOptions
    {
        public bool Enabled { get; set; }
        public string ProjectId { get; set; }
        public string ServiceAccountPath { get; set; }
    }

    public class PushSettings
    {
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 500;
        public int MessageTtlMinutes { get; set; } = 60;
    }
}
```

## 🎯 领域层实现

### 推送接口定义

```csharp
using Volo.Abp.Domain.Services;

namespace TtWork.Project.PushNotifications
{
    public interface IPushNotificationDomainService : IDomainService
    {
        /// <summary>
        /// 发送单个用户推送
        /// </summary>
        Task SendToUserAsync(Guid userId, string title, string body, 
                           Dictionary<string, object> data = null);

        /// <summary>
        /// 批量发送用户推送
        /// </summary>
        Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body,
                             Dictionary<string, object> data = null);

        /// <summary>
        /// 发送推送到指定设备
        /// </summary>
        Task SendToDeviceAsync(string deviceToken, DevicePlatform platform, 
                              string title, string body,
                              Dictionary<string, object> data = null);

        /// <summary>
        /// 发送主题推送
        /// </summary>
        Task SendToTopicAsync(string topic, string title, string body,
                             Dictionary<string, object> data = null);

        /// <summary>
        /// 发送静默推送（iOS）或数据消息（Android）
        /// </summary>
        Task SendSilentAsync(string deviceToken, DevicePlatform platform,
                            Dictionary<string, object> data);
    }

    public enum DevicePlatform
    {
        iOS,
        Android
    }
}
```

### 推送领域服务实现

```csharp
using System.Threading;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Uow;
using TtWork.Project.PushNotifications.Options;
using TtWork.Project.PushNotifications.Providers;
using TtWork.Project.Users;

namespace TtWork.Project.PushNotifications
{
    public class PushNotificationDomainService : DomainService, IPushNotificationDomainService
    {
        private readonly PushNotificationOptions _options;
        private readonly IApnsProvider _apnsProvider;
        private readonly IFcmProvider _fcmProvider;
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;
        private readonly IDistributedCache<PushRetryCacheItem> _retryCache;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IGuidGenerator _guidGenerator;

        public PushNotificationDomainService(
            IOptions<PushNotificationOptions> options,
            IApnsProvider apnsProvider,
            IFcmProvider fcmProvider,
            IUserDeviceTokenRepository deviceTokenRepository,
            IDistributedCache<PushRetryCacheItem> retryCache,
            IBackgroundJobManager backgroundJobManager,
            IGuidGenerator guidGenerator)
        {
            _options = options.Value;
            _apnsProvider = apnsProvider;
            _fcmProvider = fcmProvider;
            _deviceTokenRepository = deviceTokenRepository;
            _retryCache = retryCache;
            _backgroundJobManager = backgroundJobManager;
            _guidGenerator = guidGenerator;
        }

        public async Task SendToUserAsync(Guid userId, string title, string body,
                                         Dictionary<string, object> data = null)
        {
            if (!_options.Enabled)
            {
                Logger.LogWarning("Push notification is disabled");
                return;
            }

            var deviceTokens = await _deviceTokenRepository.GetActiveTokensByUserAsync(userId);
            
            foreach (var token in deviceTokens)
            {
                await SendToDeviceAsync(token.DeviceToken, token.Platform, title, body, data);
            }
        }

        public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body,
                                         Dictionary<string, object> data = null)
        {
            if (!_options.Enabled)
            {
                Logger.LogWarning("Push notification is disabled");
                return;
            }

            var deviceTokens = await _deviceTokenRepository.GetActiveTokensByUsersAsync(userIds);
            
            var iOSDevices = deviceTokens.Where(t => t.Platform == DevicePlatform.iOS).ToList();
            var androidDevices = deviceTokens.Where(t => t.Platform == DevicePlatform.Android).ToList();

            if (iOSDevices.Any() && _options.Apns.Enabled)
            {
                await _apnsProvider.SendBatchAsync(iOSDevices.Select(t => t.DeviceToken), 
                                                   title, body, data);
            }

            if (androidDevices.Any() && _options.Fcm.Enabled)
            {
                await _fcmProvider.SendBatchAsync(androidDevices.Select(t => t.DeviceToken), 
                                                 title, body, data);
            }
        }

        public async Task SendToDeviceAsync(string deviceToken, DevicePlatform platform,
                                          string title, string body,
                                          Dictionary<string, object> data = null)
        {
            if (!_options.Enabled)
            {
                Logger.LogWarning("Push notification is disabled");
                return;
            }

            try
            {
                switch (platform)
                {
                    case DevicePlatform.iOS:
                        if (!_options.Apns.Enabled)
                        {
                            Logger.LogWarning("APNs provider is disabled");
                            return;
                        }
                        await _apnsProvider.SendAsync(deviceToken, title, body, data);
                        break;

                    case DevicePlatform.Android:
                        if (!_options.Fcm.Enabled)
                        {
                            Logger.LogWarning("FCM provider is disabled");
                            return;
                        }
                        await _fcmProvider.SendAsync(deviceToken, title, body, data);
                        break;

                    default:
                        throw new NotSupportedException($"Platform {platform} is not supported");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send push notification to {DeviceToken}", deviceToken);
                
                // 加入重试队列
                await RetryPushAsync(deviceToken, platform, title, body, data);
            }
        }

        public async Task SendToTopicAsync(string topic, string title, string body,
                                          Dictionary<string, object> data = null)
        {
            if (!_options.Enabled)
            {
                Logger.LogWarning("Push notification is disabled");
                return;
            }

            // FCM 支持主题推送
            if (_options.Fcm.Enabled)
            {
                await _fcmProvider.SendToTopicAsync(topic, title, body, data);
            }
        }

        public async Task SendSilentAsync(string deviceToken, DevicePlatform platform,
                                         Dictionary<string, object> data)
        {
            if (!_options.Enabled)
            {
                Logger.LogWarning("Push notification is disabled");
                return;
            }

            try
            {
                switch (platform)
                {
                    case DevicePlatform.iOS:
                        if (!_options.Apns.Enabled)
                        {
                            Logger.LogWarning("APNs provider is disabled");
                            return;
                        }
                        await _apnsProvider.SendSilentAsync(deviceToken, data);
                        break;

                    case DevicePlatform.Android:
                        if (!_options.Fcm.Enabled)
                        {
                            Logger.LogWarning("FCM provider is disabled");
                            return;
                        }
                        await _fcmProvider.SendDataMessageAsync(deviceToken, data);
                        break;

                    default:
                        throw new NotSupportedException($"Platform {platform} is not supported");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send silent push to {DeviceToken}", deviceToken);
                await RetryPushAsync(deviceToken, platform, null, null, data, isSilent: true);
            }
        }

        private async Task RetryPushAsync(string deviceToken, DevicePlatform platform,
                                         string title, string body,
                                         Dictionary<string, object> data,
                                         bool isSilent = false)
        {
            var retryKey = $"push_retry:{deviceToken}:{Clock.Now.Ticks}";
            var cacheItem = await _retryCache.GetAsync(retryKey);

            if (cacheItem == null)
            {
                cacheItem = new PushRetryCacheItem
                {
                    DeviceToken = deviceToken,
                    Platform = platform,
                    Title = title,
                    Body = body,
                    Data = data,
                    IsSilent = isSilent,
                    RetryCount = 0,
                    CreatedAt = Clock.Now
                };

                await _retryCache.SetAsync(retryKey, cacheItem, 
                    TimeSpan.FromMinutes(_options.Settings.MessageTtlMinutes));
            }

            if (cacheItem.RetryCount < _options.Settings.MaxRetries)
            {
                cacheItem.RetryCount++;
                await _retryCache.SetAsync(retryKey, cacheItem,
                    TimeSpan.FromMinutes(_options.Settings.MessageTtlMinutes));

                // 延迟重试
                _backgroundJobManager.Enqueue<PushRetryJob, PushRetryArgs>(
                    new PushRetryArgs
                    {
                        DeviceToken = deviceToken,
                        Platform = platform,
                        Title = title,
                        Body = body,
                        Data = data,
                        IsSilent = isSilent,
                        RetryCount = cacheItem.RetryCount
                    },
                    BackgroundJobPriority.High,
                    delay: TimeSpan.FromSeconds(_options.Settings.RetryDelaySeconds * cacheItem.RetryCount)
                );
            }
            else
            {
                Logger.LogWarning("Max retries reached for device token: {DeviceToken}", deviceToken);
            }
        }
    }

    public class PushRetryCacheItem
    {
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public bool IsSilent { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PushRetryArgs
    {
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public bool IsSilent { get; set; }
        public int RetryCount { get; set; }
    }
}
```

## 🔌 Provider 接口与实现

### APNs Provider

```csharp
using DotAPNS;
using DotAPNS.Args;

namespace TtWork.Project.PushNotifications.Providers
{
    public interface IApnsProvider : ITransientDependency
    {
        Task SendAsync(string deviceToken, string title, string body, 
                      Dictionary<string, object> data = null);
        Task SendSilentAsync(string deviceToken, Dictionary<string, object> data);
        Task SendBatchAsync(IEnumerable<string> deviceTokens, string title, string body,
                           Dictionary<string, object> data = null);
    }

    public class ApnsProvider : IApnsProvider
    {
        private readonly ApnsClient _apnsClient;
        private readonly ILogger<ApnsProvider> _logger;
        private readonly PushNotificationOptions _options;

        public ApnsProvider(
            IOptions<PushNotificationOptions> options,
            ILogger<ApnsProvider> logger)
        {
            _options = options.Value;
            _logger = logger;

            var apnsConfig = new ApnsConfig(
                _options.Apns.KeyId,
                _options.Apns.TeamId,
                _options.Apns.BundleId,
                _options.Apns.PrivateKeyPath,
                useSandbox: _options.Apns.UseSandbox
            );

            _apnsClient = new ApnsClient(apnsConfig);
        }

        public async Task SendAsync(string deviceToken, string title, string body,
                                  Dictionary<string, object> data = null)
        {
            var payload = CreatePayload(title, body, data);
            var notification = new ApnsNotification(deviceToken, payload);

            _logger.LogInformation("Sending APNs push to {DeviceToken}", deviceToken);

            var response = await _apnsClient.SendAsync(notification);

            if (!response.IsSuccess)
            {
                _logger.LogError("APNs push failed: {Reason}", response.Reason);
                throw new ApnsException(response.Reason);
            }

            _logger.LogInformation("APNs push sent successfully to {DeviceToken}", deviceToken);
        }

        public async Task SendSilentAsync(string deviceToken, Dictionary<string, object> data)
        {
            var payload = CreateSilentPayload(data);
            var notification = new ApnsNotification(deviceToken, payload);

            _logger.LogInformation("Sending APNs silent push to {DeviceToken}", deviceToken);

            var response = await _apnsClient.SendAsync(notification);

            if (!response.IsSuccess)
            {
                _logger.LogError("APNs silent push failed: {Reason}", response.Reason);
                throw new ApnsException(response.Reason);
            }

            _logger.LogInformation("APNs silent push sent successfully to {DeviceToken}", deviceToken);
        }

        public async Task SendBatchAsync(IEnumerable<string> deviceTokens, string title, string body,
                                        Dictionary<string, object> data = null)
        {
            var tasks = deviceTokens.Select(token => SendAsync(token, title, body, data));
            await Task.WhenAll(tasks);
        }

        private ApnsPayload CreatePayload(string title, string body, Dictionary<string, object> data)
        {
            return new ApnsPayload
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
                Custom = data ?? new Dictionary<string, object>()
            };
        }

        private ApnsPayload CreateSilentPayload(Dictionary<string, object> data)
        {
            return new ApnsPayload
            {
                Aps = new Aps
                {
                    ContentAvailable = 1
                },
                Custom = data ?? new Dictionary<string, object>()
            };
        }
    }

    public class ApnsException : Exception
    {
        public ApnsException(string reason) : base($"APNs error: {reason}")
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}
```

### FCM Provider

```csharp
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace TtWork.Project.PushNotifications.Providers
{
    public interface IFcmProvider : ITransientDependency
    {
        Task SendAsync(string registrationToken, string title, string body,
                      Dictionary<string, object> data = null);
        Task SendDataMessageAsync(string registrationToken, Dictionary<string, object> data);
        Task SendBatchAsync(IEnumerable<string> registrationTokens, string title, string body,
                           Dictionary<string, object> data = null);
        Task SendToTopicAsync(string topic, string title, string body,
                             Dictionary<string, object> data = null);
    }

    public class FcmProvider : IFcmProvider
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<FcmProvider> _logger;
        private readonly PushNotificationOptions _options;

        public FcmProvider(
            IOptions<PushNotificationOptions> options,
            ILogger<FcmProvider> logger)
        {
            _options = options.Value;
            _logger = logger;

            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromFile(_options.Fcm.ServiceAccountPath);

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _options.Fcm.ProjectId
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task SendAsync(string registrationToken, string title, string body,
                                  Dictionary<string, object> data = null)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = ConvertDataDictionary(data),
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

            _logger.LogInformation("Sending FCM push to {RegistrationToken}", registrationToken);

            var response = await _messaging.SendAsync(message);

            if (string.IsNullOrEmpty(response))
            {
                _logger.LogError("FCM push failed");
                throw new FcmException("Failed to send FCM message");
            }

            _logger.LogInformation("FCM push sent successfully to {RegistrationToken}", registrationToken);
        }

        public async Task SendDataMessageAsync(string registrationToken, Dictionary<string, object> data)
        {
            var message = new Message
            {
                Token = registrationToken,
                Data = ConvertDataDictionary(data),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Uptime = 3600
                }
            };

            _logger.LogInformation("Sending FCM data message to {RegistrationToken}", registrationToken);

            var response = await _messaging.SendAsync(message);

            if (string.IsNullOrEmpty(response))
            {
                _logger.LogError("FCM data message failed");
                throw new FcmException("Failed to send FCM data message");
            }

            _logger.LogInformation("FCM data message sent successfully to {RegistrationToken}", registrationToken);
        }

        public async Task SendBatchAsync(IEnumerable<string> registrationTokens, string title, string body,
                                        Dictionary<string, object> data = null)
        {
            const int batchSize = 500;
            var batches = registrationTokens
                .Select((token, index) => new { token, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.token));

            foreach (var batch in batches)
            {
                var message = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = ConvertDataDictionary(data),
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

                _logger.LogInformation("Sending FCM batch push to {Count} devices", batch.Count());

                var response = await _messaging.SendMulticastAsync(message);

                if (response.FailureCount > 0)
                {
                    var failedTokens = new List<string>();
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            failedTokens.Add(batch.ElementAt(i));
                            _logger.LogWarning("FCM push failed for {Token}: {Error}", 
                                batch.ElementAt(i), response.Responses[i].Exception.Message);
                        }
                    }

                    // 移除无效 Token
                    await RemoveInvalidTokensAsync(failedTokens);
                }

                _logger.LogInformation("FCM batch push sent: {SuccessCount} success, {FailureCount} failed",
                    response.SuccessCount, response.FailureCount);
            }
        }

        public async Task SendToTopicAsync(string topic, string title, string body,
                                          Dictionary<string, object> data = null)
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = ConvertDataDictionary(data),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            _logger.LogInformation("Sending FCM topic push to {Topic}", topic);

            var response = await _messaging.SendAsync(message);

            if (string.IsNullOrEmpty(response))
            {
                _logger.LogError("FCM topic push failed");
                throw new FcmException("Failed to send FCM topic message");
            }

            _logger.LogInformation("FCM topic push sent successfully to {Topic}", topic);
        }

        private Dictionary<string, string> ConvertDataDictionary(Dictionary<string, object> data)
        {
            return data?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.ToString()
            ) ?? new Dictionary<string, string>();
        }

        private async Task RemoveInvalidTokensAsync(List<string> tokens)
        {
            // 从数据库移除无效 Token
            // ...
        }
    }

    public class FcmException : Exception
    {
        public FcmException(string message) : base(message)
        {
        }
    }
}
```

## 💾 数据模型

### UserDeviceToken 实体

```csharp
using Volo.Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Users
{
    public class UserDeviceToken : AuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }

        protected UserDeviceToken()
        {
        }

        public UserDeviceToken(
            Guid id,
            Guid userId,
            string deviceToken,
            DevicePlatform platform,
            string deviceInfo)
            : base(id)
        {
            UserId = userId;
            DeviceToken = deviceToken;
            Platform = platform;
            DeviceInfo = deviceInfo;
            IsActive = true;
            LastUsedAt = Clock.Now;
        }

        public void MarkAsInactive()
        {
            IsActive = false;
        }

        public void UpdateLastUsed()
        {
            LastUsedAt = Clock.Now;
        }
    }

    public enum DevicePlatform
    {
        iOS,
        Android
    }
}
```

### Repository 接口

```csharp
using Volo.Abp.Domain.Repositories;

namespace TtWork.Project.Users
{
    public interface IUserDeviceTokenRepository : IRepository<UserDeviceToken, Guid>
    {
        Task<List<UserDeviceToken>> GetActiveTokensByUserAsync(Guid userId);
        Task<List<UserDeviceToken>> GetActiveTokensByUsersAsync(IEnumerable<Guid> userIds);
        Task<UserDeviceToken> FindByDeviceTokenAsync(string deviceToken);
    }
}
```

### Repository 实现

```csharp
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TtWork.Project.EntityFrameworkCore;

namespace TtWork.Project.Users
{
    public class EfCoreUserDeviceTokenRepository : EfCoreRepository<TtWorkDbContext, UserDeviceToken, Guid>,
        IUserDeviceTokenRepository
    {
        public EfCoreUserDeviceTokenRepository(
            IDbContextProvider<TtWorkDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<UserDeviceToken>> GetActiveTokensByUserAsync(Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();
        }

        public async Task<List<UserDeviceToken>> GetActiveTokensByUsersAsync(IEnumerable<Guid> userIds)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(t => userIds.Contains(t.UserId) && t.IsActive)
                .ToListAsync();
        }

        public async Task<UserDeviceToken> FindByDeviceTokenAsync(string deviceToken)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(t => t.DeviceToken == deviceToken)
                .FirstOrDefaultAsync();
        }
    }
}
```

## 📨 应用服务层

### PushNotificationAppService

```csharp
using Volo.Abp.Application.Services;

namespace TtWork.Project.PushNotifications
{
    [RemoteService(false)]
    public class PushNotificationAppService : ApplicationService
    {
        private readonly IPushNotificationDomainService _pushService;

        public PushNotificationAppService(IPushNotificationDomainService pushService)
        {
            _pushService = pushService;
        }

        public async Task SendToUserAsync(SendToUserDto input)
        {
            await _pushService.SendToUserAsync(
                input.UserId,
                input.Title,
                input.Body,
                input.Data
            );
        }

        public async Task SendToUsersAsync(SendToUsersDto input)
        {
            await _pushService.SendToUsersAsync(
                input.UserIds,
                input.Title,
                input.Body,
                input.Data
            );
        }

        public async Task SendToDeviceAsync(SendToDeviceDto input)
        {
            await _pushService.SendToDeviceAsync(
                input.DeviceToken,
                input.Platform,
                input.Title,
                input.Body,
                input.Data
            );
        }

        public async Task SendToTopicAsync(SendToTopicDto input)
        {
            await _pushService.SendToTopicAsync(
                input.Topic,
                input.Title,
                input.Body,
                input.Data
            );
        }

        public async Task SendSilentAsync(SendSilentDto input)
        {
            await _pushService.SendSilentAsync(
                input.DeviceToken,
                input.Platform,
                input.Data
            );
        }
    }

    public class SendToUserDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class SendToUsersDto
    {
        public List<Guid> UserIds { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class SendToDeviceDto
    {
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class SendToTopicDto
    {
        public string Topic { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class SendSilentDto
    {
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
```

### UserDeviceTokenAppService

```csharp
using Volo.Abp.Application.Services;
using TtWork.Project.Users;

namespace TtWork.Project.PushNotifications
{
    public class UserDeviceTokenAppService : ApplicationService
    {
        private readonly IUserDeviceTokenRepository _tokenRepository;
        private readonly IPushNotificationDomainService _pushService;

        public UserDeviceTokenAppService(
            IUserDeviceTokenRepository tokenRepository,
            IPushNotificationDomainService pushService)
        {
            _tokenRepository = tokenRepository;
            _pushService = pushService;
        }

        public async Task RegisterTokenAsync(RegisterDeviceTokenDto input)
        {
            var existingToken = await _tokenRepository.FindByDeviceTokenAsync(input.DeviceToken);

            if (existingToken != null)
            {
                if (existingToken.UserId != CurrentUser.Id)
                {
                    // Token 已被其他用户使用，标记为无效
                    existingToken.MarkAsInactive();
                    await _tokenRepository.UpdateAsync(existingToken);
                }
                else
                {
                    // 同一用户的 Token，更新使用时间
                    existingToken.UpdateLastUsed();
                    await _tokenRepository.UpdateAsync(existingToken);
                    return;
                }
            }

            var newToken = new UserDeviceToken(
                GuidGenerator.Create(),
                CurrentUser.Id.Value,
                input.DeviceToken,
                input.Platform,
                input.DeviceInfo
            );

            await _tokenRepository.InsertAsync(newToken);
        }

        public async Task UnregisterTokenAsync(UnregisterDeviceTokenDto input)
        {
            var token = await _tokenRepository.FindByDeviceTokenAsync(input.DeviceToken);

            if (token != null && token.UserId == CurrentUser.Id)
            {
                token.MarkAsInactive();
                await _tokenRepository.UpdateAsync(token);
            }
        }
    }

    public class RegisterDeviceTokenDto
    {
        public string DeviceToken { get; set; }
        public DevicePlatform Platform { get; set; }
        public string DeviceInfo { get; set; }
    }

    public class UnregisterDeviceTokenDto
    {
        public string DeviceToken { get; set; }
    }
}
```

## 🔄 后台任务

### PushRetryJob

```csharp
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Uow;

namespace TtWork.Project.PushNotifications.Jobs
{
    public class PushRetryJob : AsyncBackgroundJob<PushRetryArgs>
    {
        private readonly IPushNotificationDomainService _pushService;

        public PushRetryJob(IPushNotificationDomainService pushService)
        {
            _pushService = pushService;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(PushRetryArgs args)
        {
            if (args.IsSilent)
            {
                await _pushService.SendSilentAsync(
                    args.DeviceToken,
                    args.Platform,
                    args.Data
                );
            }
            else
            {
                await _pushService.SendToDeviceAsync(
                    args.DeviceToken,
                    args.Platform,
                    args.Title,
                    args.Body,
                    args.Data
                );
            }
        }
    }
}
```

## 📊 监控与日志

### 推送指标收集

```csharp
using Volo.Abp.BackgroundJobs;

namespace TtWork.Project.PushNotifications.Metrics
{
    public class PushMetricsService : ITransientDependency
    {
        private readonly ILogger<PushMetricsService> _logger;

        public PushMetricsService(ILogger<PushMetricsService> logger)
        {
            _logger = logger;
        }

        public void RecordPushSuccess(string platform)
        {
            _logger.LogInformation("Push sent successfully on {Platform}", platform);
            // 集成监控系统（如 Prometheus、Application Insights）
        }

        public void RecordPushFailure(string platform, string error)
        {
            _logger.LogError("Push failed on {Platform}: {Error}", platform, error);
        }

        public void RecordInvalidToken(string platform)
        {
            _logger.LogWarning("Invalid token on {Platform}", platform);
        }
    }
}
```

## 🚀 使用示例

### 拍卖出价通知

```csharp
// 在拍卖服务中调用
public class AuctionBiddingAppService : ApplicationService
{
    private readonly IPushNotificationDomainService _pushService;

    public async Task PlaceBidAsync(PlaceBidDto input)
    {
        // 处理出价逻辑
        // ...

        // 发送通知给出价用户
        await _pushService.SendToUserAsync(
            CurrentUser.Id.Value,
            "出价成功",
            $"您的出价 ¥{input.Amount} 已成功提交",
            new Dictionary<string, object>
            {
                { "type", "bid_placed" },
                { "auctionId", input.AuctionId },
                { "itemId", input.ItemId },
                { "amount", input.Amount }
            }
        );

        // 发送通知给其他关注者
        var followers = await GetAuctionFollowersAsync(input.AuctionId);
        if (followers.Any())
        {
            await _pushService.SendToUsersAsync(
                followers,
                "新出价提醒",
                $"您关注的拍品刚刚有新出价 ¥{input.Amount}",
                new Dictionary<string, object>
                {
                    { "type", "new_bid" },
                    { "auctionId", input.AuctionId },
                    { "itemId", input.ItemId },
                    { "amount", input.Amount }
                }
            );
        }
    }
}
```

### 拍卖结束通知

```csharp
public class AuctionManagementAppService : ApplicationService
{
    private readonly IPushNotificationDomainService _pushService;

    public async Task EndAuctionAsync(Guid auctionId)
    {
        // 结束拍卖逻辑
        // ...

        // 发送通知给所有参与者
        await _pushService.SendToTopicAsync(
            $"auction_{auctionId}",
            "拍卖结束",
            "拍卖已结束，请查看结果",
            new Dictionary<string, object>
            {
                { "type", "auction_ended" },
                { "auctionId", auctionId }
            }
        );
    }
}
```

## 🔗 参考资料

- [ABP Framework Documentation](https://docs.abp.io/)
- [dotAPNS GitHub](https://github.com/alexalok/dotAPNS)
- [Firebase Admin .NET SDK](https://firebase.google.com/docs/admin/setup)
