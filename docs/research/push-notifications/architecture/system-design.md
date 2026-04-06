# 推送通知系统架构设计

## 🏗️ 系统架构图

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              用户层 (User Layer)                         │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐      │
│  │   iOS App        │  │  Android App     │  │   Web App       │      │
│  │                  │  │                  │  │                  │      │
│  │ - UniApp 框架    │  │ - UniApp 框架    │  │ - Vue 3         │      │
│  │ - 推送 SDK       │  │ - 推送 SDK       │  │ - Web Push API  │      │
│  │ - 权限管理       │  │ - 权限管理       │  │ - Service Worker│      │
│  └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘      │
│           │                     │                     │                │
└───────────┼─────────────────────┼─────────────────────┼────────────────┘
            │                     │                     │
┌───────────▼─────────────────────▼─────────────────────▼────────────────┐
│                           网络层 (Network Layer)                        │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │              HTTPS / HTTP/2 (TLS 加密)                       │      │
│  └─────────────────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────────────────┘
            │
┌───────────▼──────────────────────────────────────────────────────────┐
│                        应用层 (Application Layer)                      │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │                   API Gateway (Ocelot)                       │      │
│  │  - 请求路由                                                   │      │
│  │  - 身份验证                                                   │      │
│  │  - 限流                                                       │      │
│  └──────────────┬──────────────────────────────────────────────┘      │
│                 │                                                     │
│  ┌──────────────┴──────────────────────────────────────────────┐      │
│  │         Push Notification Controller (REST API)            │      │
│  │  - POST /api/push/send                                      │      │
│  │  - POST /api/push/device-token/register                     │      │
│  │  - POST /api/push/device-token/unregister                   │      │
│  └──────────────┬──────────────────────────────────────────────┘      │
└─────────────────┼─────────────────────────────────────────────────────┘
                  │
┌─────────────────▼─────────────────────────────────────────────────────┐
│                        领域层 (Domain Layer)                           │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │          PushNotificationDomainService                      │      │
│  │  - 统一推送接口                                              │      │
│  │  - 消息模板管理                                              │      │
│  │  - 目标用户选择                                              │      │
│  │  - 推送队列管理                                              │      │
│  └──────────────┬──────────────────────────────────────────────┘      │
│                 │                                                     │
│  ┌──────────────┴──────────────────────────────────────────────┐      │
│  │              Business Logic Services                        │      │
│  │  - AuctionBiddingService (拍卖出价)                         │      │
│  │  - AuctionManagementService (拍卖管理)                      │      │
│  │  - UserNotificationService (用户通知)                       │      │
│  └─────────────────────────────────────────────────────────────┘      │
└─────────────────┼─────────────────────────────────────────────────────┘
                  │
┌─────────────────▼─────────────────────────────────────────────────────┐
│                      基础设施层 (Infrastructure Layer)                 │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │              Push Notification Providers                    │      │
│  │  ┌────────────────┐  ┌────────────────┐                    │      │
│  │  │  ApnsProvider  │  │  FcmProvider   │                    │      │
│  │  │                │  │                │                    │      │
│  │  │ - dotAPNS SDK  │  │ - Firebase     │                    │      │
│  │  │ - HTTP/2       │  │   Admin SDK   │                    │      │
│  │  │ - Token 验证   │  │ - Token 管理  │                    │      │
│  │  └────────┬───────┘  └────────┬───────┘                    │      │
│  └───────────┼──────────────────┼──────────────────────────────┘      │
│              │                  │                                   │
│  ┌───────────▼──────────────────▼──────────────────────────────┐      │
│  │              Background Job Manager                         │      │
│  │  - 推送重试                                                 │      │
│  │  - 延迟推送                                                 │      │
│  │  - 批量推送                                                 │      │
│  └───────────┬─────────────────────────────────────────────────┘      │
│              │                                                      │
│  ┌───────────▼─────────────────────────────────────────────────┐      │
│  │              Cache Manager                                  │      │
│  │  - Device Token 缓存                                        │      │
│  │  - 推送重试缓存                                             │      │
│  │  - 消息模板缓存                                             │      │
│  └───────────┬─────────────────────────────────────────────────┘      │
│              │                                                      │
│  ┌───────────▼─────────────────────────────────────────────────┐      │
│  │              Repository Layer                                │      │
│  │  - UserDeviceTokenRepository                                │      │
│  │  - PushLogRepository                                        │      │
│  │  - PushMetricsRepository                                    │      │
│  └─────────────────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────────────────┘
                  │
┌─────────────────▼─────────────────────────────────────────────────────┐
│                        数据层 (Data Layer)                             │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │                    Database (MySQL)                         │      │
│  │  - UserDeviceTokens (设备 Token)                            │      │
│  │  - PushLogs (推送日志)                                       │      │
│  │  - PushMetrics (推送指标)                                    │      │
│  └─────────────────────────────────────────────────────────────┘      │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │                  Distributed Cache                           │      │
│  │  - Device Token 缓存                                        │      │
│  │  - 推送重试队列                                             │      │
│  └─────────────────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────────────────┘
                  │
┌─────────────────▼─────────────────────────────────────────────────────┐
│                      外部服务层 (External Services)                   │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │                 Apple Push Notification Service              │      │
│  │  - iOS 推送                                                 │      │
│  │  - 静默推送                                                 │      │
│  │  - 富媒体通知                                               │      │
│  └─────────────────────────────────────────────────────────────┘      │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐      │
│  │                 Firebase Cloud Messaging                    │      │
│  │  - Android 推送                                             │      │
│  │  - 主题订阅                                                 │      │
│  │  - 数据消息                                                 │      │
│  └─────────────────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────────────────┘
```

## 🎯 核心设计原则

### 1. 单一职责原则 (SRP)

每个组件只负责一个功能：

- **ApnsProvider**: 专注于 APNs 推送
- **FcmProvider**: 专注于 FCM 推送
- **PushNotificationDomainService**: 专注于业务逻辑
- **BackgroundJobManager**: 专注于后台任务

### 2. 开闭原则 (OCP)

对扩展开放，对修改关闭：

- 通过接口定义推送提供者，易于添加新的推送服务
- 消息模板可以扩展，无需修改核心代码

### 3. 依赖倒置原则 (DIP)

高层模块不依赖低层模块，都依赖于抽象：

- `IPushNotificationDomainService` 依赖 `IApnsProvider` 和 `IFcmProvider` 接口
- 通过依赖注入管理依赖关系

### 4. 接口隔离原则 (ISP)

客户端不应该依赖它不需要的接口：

- `IApnsProvider` 只定义 APNs 相关方法
- `IFcmProvider` 只定义 FCM 相关方法

## 📊 数据模型设计

### UserDeviceToken 实体

```csharp
public class UserDeviceToken : AuditedAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; }
    public DevicePlatform Platform { get; set; }
    public string DeviceInfo { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
```

### PushLog 实体

```csharp
public class PushLog : AuditedAggregateRoot<Guid>
{
    public string Platform { get; set; }
    public string DeviceToken { get; set; }
    public string MessageType { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string CustomData { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public int RetryCount { get; set; }
}
```

### PushMetrics 实体

```csharp
public class PushMetrics : AuditedAggregateRoot<Guid>
{
    public string Platform { get; set; }
    public string MessageType { get; set; }
    public int SentCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public DateTime Date { get; set; }
}
```

## 🔐 安全设计

### 1. 密钥管理

- ✅ APNs Auth Key 存储在安全位置（如 Azure Key Vault）
- ✅ Firebase Service Account 文件加密存储
- ✅ 定期轮换密钥

### 2. 通信安全

- ✅ 使用 HTTPS/TLS 加密通信
- ✅ API 认证使用 JWT Token
- ✅ 敏感数据加密传输

### 3. 数据安全

- ✅ Device Token 加密存储
- ✅ 推送内容脱敏记录日志
- ✅ 定期清理过期数据

### 4. 访问控制

- ✅ 基于角色的访问控制 (RBAC)
- ✅ API 限流防止滥用
- ✅ 输入验证和输出编码

## 🚀 性能优化

### 1. 缓存策略

```csharp
// Device Token 缓存
public class DeviceTokenCache
{
    private readonly IDistributedCache<List<UserDeviceToken>> _cache;
    
    public async Task<List<UserDeviceToken>> GetTokensAsync(Guid userId)
    {
        var cacheKey = $"user_tokens:{userId}";
        return await _cache.GetAsync(cacheKey) ?? 
               await LoadFromDatabase(userId);
    }
}
```

### 2. 批量推送

```csharp
// 批量发送优化
public async Task SendBatchAsync(IEnumerable<string> tokens, string title, string body)
{
    const int batchSize = 500;
    var batches = tokens
        .Select((token, index) => new { token, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.token));

    await Task.WhenAll(batches.Select(batch => 
        SendSingleBatchAsync(batch, title, body)
    ));
}
```

### 3. 连接池

- ✅ APNs HTTP/2 连接复用
- ✅ FCM 连接池管理
- ✅ 数据库连接池优化

### 4. 异步处理

```csharp
// 异步发送推送
public async Task SendPushAsync(string deviceToken, string title, string body)
{
    await Task.Run(async () =>
    {
        await _provider.SendAsync(deviceToken, title, body);
    });
}
```

## 📈 可扩展性设计

### 1. 水平扩展

```
┌─────────────────────────────────────────────────────────────┐
│                    Load Balancer (Nginx)                    │
└────────────┬────────────────────────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
┌───▼────┐     ┌────▼────┐
│ Node 1 │     │ Node 2  │
└────────┘     └─────────┘
    │                 │
┌───▼────┐     ┌────▼────┐
│ Redis  │     │ Redis   │
│ Cache  │     │ Cache   │
└────────┘     └─────────┘
    │                 │
┌───▼─────────────────▼────┐
│      MySQL Cluster       │
└──────────────────────────┘
```

### 2. 消息队列

```csharp
// 使用消息队列解耦
public class PushQueue
{
    private readonly IMessageQueue _queue;
    
    public async Task EnqueueAsync(PushMessage message)
    {
        await _queue.PublishAsync("push_queue", message);
    }
    
    public async Task ProcessAsync()
    {
        await _queue.SubscribeAsync("push_queue", async message =>
        {
            await _pushService.SendAsync(message);
        });
    }
}
```

### 3. 微服务拆分

```
┌─────────────────────────────────────────────────────────────┐
│                        API Gateway                           │
└────────────┬────────────────────────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
┌───▼────────┐  ┌────▼────────┐
│  Auction   │  │    Push     │
│  Service   │  │  Service    │
│            │  │             │
│ - 拍卖管理  │  │ - 推送服务  │
│ - 出价处理  │  │ - Token管理 │
└────────────┘  └─────────────┘
```

## 🔍 监控与日志

### 1. 监控指标

```csharp
// 推送监控指标
public class PushMonitoringService
{
    private readonly IMetrics _metrics;
    
    public void RecordPush(string platform, bool success)
    {
        _metrics.Counter("push.total", new { platform }).Increment();
        _metrics.Counter("push.success", new { platform }).Increment(success ? 1 : 0);
        _metrics.Gauge("push.active_connections", GetActiveConnections());
    }
}
```

### 2. 日志记录

```csharp
// 结构化日志
public class PushLogger
{
    public void LogPush(PushContext context)
    {
        _logger.LogInformation("Push sent: {Platform}, {DeviceToken}, {MessageType}", 
            context.Platform, 
            context.DeviceToken, 
            context.MessageType);
    }
}
```

### 3. 告警机制

```csharp
// 告警规则
public class PushAlertingService
{
    public async Task CheckAlertsAsync()
    {
        var failureRate = await GetFailureRateAsync();
        
        if (failureRate > 0.05) // 失败率超过 5%
        {
            await SendAlertAsync($"Push failure rate too high: {failureRate:P2}");
        }
    }
}
```

## 🛡️ 容错与恢复

### 1. 重试机制

```csharp
// 指数退避重试
public async Task SendWithRetryAsync(string deviceToken, string title, string body)
{
    var retryCount = 0;
    var maxRetries = 3;
    var delay = TimeSpan.FromSeconds(1);
    
    while (retryCount < maxRetries)
    {
        try
        {
            await _provider.SendAsync(deviceToken, title, body);
            return;
        }
        catch (Exception ex) when (retryCount < maxRetries)
        {
            retryCount++;
            await Task.Delay(delay);
            delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        }
    }
    
    throw new Exception($"Push failed after {maxRetries} retries");
}
```

### 2. 熔断机制

```csharp
// 熔断器
public class PushCircuitBreaker
{
    private readonly CircuitBreaker _circuitBreaker;
    
    public async Task SendAsync(string deviceToken, string title, string body)
    {
        await _circuitBreaker.ExecuteAsync(async () =>
        {
            await _provider.SendAsync(deviceToken, title, body);
        });
    }
}
```

### 3. 降级策略

```csharp
// 服务降级
public class PushFallbackService
{
    public async Task SendWithFallbackAsync(string deviceToken, string title, string body)
    {
        try
        {
            await _provider.SendAsync(deviceToken, title, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push failed, using fallback");
            
            // 降级策略：使用短信通知
            await _smsService.SendAsync(deviceToken, title, body);
        }
    }
}
```

## 📋 部署架构

### 生产环境部署

```
┌─────────────────────────────────────────────────────────────┐
│                      CDN (CloudFlare)                        │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│              Load Balancer (Nginx)                          │
│  - SSL 终止                                                  │
│  - 负载均衡                                                  │
│  - 健康检查                                                  │
└────┬──────────────────────┬──────────────────────┬──────────┘
     │                      │                      │
┌────▼────────┐      ┌─────▼───────┐      ┌──────▼──────┐
│   App 1     │      │    App 2    │      │    App 3    │
│   (Docker)  │      │   (Docker)  │      │   (Docker)  │
└────┬────────┘      └─────┬───────┘      └──────┬──────┘
     │                      │                      │
     └──────────────────────┴──────────────────────┘
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                    Redis Cluster                              │
│  - Device Token 缓存                                          │
│  - 推送重试队列                                               │
└───────────────────────────┬──────────────────────────────────┘
                            │
┌───────────────────────────▼──────────────────────────────────┐
│                   MySQL Master-Slave                          │
│  - 主库负责写操作                                              │
│  - 从库负责读操作                                              │
└───────────────────────────────────────────────────────────────┘
```

### 容器化部署

```yaml
# docker-compose.yml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=...
    depends_on:
      - mysql
      - redis

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=...
    volumes:
      - mysql_data:/var/lib/mysql

  redis:
    image: redis:7
    volumes:
      - redis_data:/data

volumes:
  mysql_data:
  redis_data:
```

## 🔗 参考资料

- [ABP Framework Architecture](https://docs.abp.io/en/abp/latest/Architecture)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microservices Patterns](https://microservices.io/patterns/)
