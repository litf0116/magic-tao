# 推送通知数据流程设计

## 🔄 完整推送流程

### 1. 设备注册流程

```
┌─────────┐
│   App   │
└────┬────┘
     │
     │ 1. 用户登录 App
     │
     ▼
┌─────────────────┐
│ 检查推送权限    │
└────────┬────────┘
         │
         │ 2. 无权限 -> 申请权限
         │
         ▼
┌─────────────────┐
│ 用户授权/拒绝   │
└────────┬────────┘
         │
         │ 3. 已授权
         │
         ▼
┌─────────────────┐
│ 获取 Device Token│
│ (iOS/Android)   │
└────────┬────────┘
         │
         │ 4. Token: xxxxx
         │
         ▼
┌─────────────────┐
│ 上传到后端 API  │
│ POST /api/push/ │
│ device-token/   │
│ register        │
└────────┬────────┘
         │
         │ 5. 存储到数据库
         │
         ▼
┌─────────────────┐
│  UserDeviceToken│
│  Table          │
│  - UserId       │
│  - DeviceToken  │
│  - Platform     │
│  - IsActive     │
└─────────────────┘
```

### 2. 推送发送流程

```
┌─────────────────┐
│ 业务事件触发     │
│ (如出价成功)     │
└────────┬────────┘
         │
         │ 1. 触发推送
         │
         ▼
┌─────────────────┐
│ 调用推送服务    │
│ PushNotification│
│ DomainService   │
└────────┬────────┘
         │
         │ 2. 获取目标用户
         │
         ▼
┌─────────────────┐
│ 查询设备 Token  │
│ UserDeviceToken  │
│ Repository      │
└────────┬────────┘
         │
         │ 3. Token 列表
         │
         ▼
┌─────────────────┐
│ 构建推送消息    │
│ - 标题           │
│ - 内容           │
│ - 自定义数据     │
└────────┬────────┘
         │
         │ 4. 消息对象
         │
         ▼
┌─────────────────┐
│ 根据平台选择    │
│ Provider        │
└────┬────────┬───┘
     │        │
     │ 5a     │ 5b
     ▼        ▼
┌─────────┐ ┌──────────┐
│APNs     │ │  FCM     │
│Provider │ │ Provider │
└────┬────┘ └─────┬────┘
     │            │
     │ 6a         │ 6b
     ▼            ▼
┌─────────┐  ┌──────────┐
│APNs     │  │  FCM     │
│Server   │  │  Server  │
└────┬────┘  └─────┬────┘
     │            │
     │ 7a         │ 7b
     ▼            ▼
┌─────────┐  ┌──────────┐
│iOS 设备 │  │Android   │
│         │  │设备      │
└─────────┘  └──────────┘
     │            │
     │ 8          │ 8
     ▼            ▼
┌─────────────────────┐
│ 用户收到推送        │
│ - 显示通知          │
│ - 或处理数据消息    │
└─────────────────────┘
```

### 3. 推送失败重试流程

```
┌─────────────────┐
│ 推送发送失败    │
└────────┬────────┘
         │
         │ 1. 捕获异常
         │
         ▼
┌─────────────────┐
│ 判断错误类型    │
└────┬────────┬───┘
     │        │
     │ Token  │ 其他错误
     │ 无效   │
     ▼        ▼
┌─────────┐ ┌─────────────┐
│ 标记     │ │ 加入重试队列 │
│ Token   │ │ - 设备 Token │
│ 失效   │ │ - 平台       │
│         │ │ - 消息内容   │
└─────────┘ │ - 重试次数   │
            └──────┬──────┘
                   │
                   │ 2. 延迟重试
                   │
                   ▼
          ┌─────────────────┐
          │ 后台任务执行    │
          │ BackgroundJob   │
          └────────┬────────┘
                   │
                   │ 3. 重新发送
                   │
                   ▼
          ┌─────────────────┐
          │ 检查重试次数    │
          └────────┬────────┘
                   │
         ┌─────────┴─────────┐
         │                   │
         │ < Max Retries     │ >= Max Retries
         ▼                   ▼
  ┌──────────────┐   ┌──────────────┐
  │ 继续重试     │   │ 标记为永久   │
  │ (指数退避)   │   │ 失败         │
  └──────────────┘   └──────────────┘
```

## 📊 数据流转详解

### 用户设备 Token 管理

#### Token 注册

```csharp
// 前端
const deviceToken = await getPushToken()
await registerDeviceToken(deviceToken)

// API
POST /api/push/device-token/register
{
  "deviceToken": "xxxxx",
  "platform": "iOS",
  "deviceInfo": "{\"model\":\"iPhone 14\",\"os\":\"iOS 16\"}"
}

// 后端
public async Task RegisterTokenAsync(RegisterDeviceTokenDto input)
{
    // 检查是否已存在
    var existingToken = await _tokenRepository.FindByDeviceTokenAsync(input.DeviceToken);
    
    if (existingToken != null)
    {
        // 如果是同一用户，更新使用时间
        if (existingToken.UserId == CurrentUser.Id)
        {
            existingToken.UpdateLastUsed();
            await _tokenRepository.UpdateAsync(existingToken);
            return;
        }
        
        // 如果是不同用户，标记旧 Token 为无效
        existingToken.MarkAsInactive();
        await _tokenRepository.UpdateAsync(existingToken);
    }
    
    // 创建新 Token
    var newToken = new UserDeviceToken(
        GuidGenerator.Create(),
        CurrentUser.Id.Value,
        input.DeviceToken,
        input.Platform,
        input.DeviceInfo
    );
    
    await _tokenRepository.InsertAsync(newToken);
}
```

#### Token 查询

```csharp
// 批量查询用户 Token
public async Task<List<UserDeviceToken>> GetActiveTokensByUsersAsync(
    IEnumerable<Guid> userIds)
{
    var cacheKey = $"user_tokens:{string.Join(',', userIds)}";
    var cached = await _cache.GetAsync<List<UserDeviceToken>>(cacheKey);
    
    if (cached != null)
    {
        return cached;
    }
    
    var tokens = await _tokenRepository
        .GetQueryable()
        .Where(t => userIds.Contains(t.UserId) && t.IsActive)
        .ToListAsync();
    
    await _cache.SetAsync(cacheKey, tokens, TimeSpan.FromMinutes(30));
    
    return tokens;
}
```

#### Token 清理

```csharp
// 定期清理无效 Token
public class InvalidTokenCleanupJob : AsyncBackgroundJob<object>
{
    public override async Task ExecuteAsync(object args)
    {
        var expiredTokens = await _tokenRepository
            .GetQueryable()
            .Where(t => !t.IsActive)
            .Where(t => t.LastModificationTime < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
        
        foreach (var token in expiredTokens)
        {
            await _tokenRepository.DeleteAsync(token);
        }
        
        Logger.LogInformation($"Cleaned up {expiredTokens.Count} invalid tokens");
    }
}
```

### 推送消息构建

#### 消息模板

```csharp
// 拍卖出价通知模板
public class BidPlacedMessageTemplate
{
    public async Task<object> BuildMessage(
        Guid auctionId, 
        Guid itemId, 
        decimal amount)
    {
        return new
        {
            title = "出价成功",
            body = $"您的出价 ¥{amount:N0} 已成功提交",
            data = new
            {
                type = "bid_placed",
                auctionId = auctionId.ToString(),
                itemId = itemId.ToString(),
                amount = amount,
                timestamp = DateTime.UtcNow.ToString("o")
            }
        };
    }
}
```

#### 消息发送

```csharp
// 统一推送接口
public async Task SendPushAsync(
    string deviceToken, 
    DevicePlatform platform, 
    object message)
{
    try
    {
        switch (platform)
        {
            case DevicePlatform.iOS:
                await _apnsProvider.SendAsync(deviceToken, message);
                break;
                
            case DevicePlatform.Android:
                await _fcmProvider.SendAsync(deviceToken, message);
                break;
        }
        
        // 记录成功日志
        await _logRepository.InsertAsync(new PushLog
        {
            Platform = platform.ToString(),
            DeviceToken = deviceToken,
            MessageType = message.data?.type,
            Title = message.title,
            Body = message.body,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        // 记录失败日志
        await _logRepository.InsertAsync(new PushLog
        {
            Platform = platform.ToString(),
            DeviceToken = deviceToken,
            MessageType = message.data?.type,
            Title = message.title,
            Body = message.body,
            IsSuccess = false,
            ErrorMessage = ex.Message,
            SentAt = DateTime.UtcNow
        });
        
        // 加入重试队列
        await _retryQueue.EnqueueAsync(new PushRetryItem
        {
            DeviceToken = deviceToken,
            Platform = platform,
            Message = message,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        });
        
        throw;
    }
}
```

### 推送重试机制

#### 重试队列

```csharp
// 推送重试队列
public class PushRetryQueue
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    
    public async Task EnqueueAsync(PushRetryItem item)
    {
        var delay = TimeSpan.FromSeconds(
            Math.Pow(2, item.RetryCount) * 5); // 指数退避
        
        _backgroundJobManager.Enqueue<PushRetryJob, PushRetryArgs>(
            new PushRetryArgs
            {
                DeviceToken = item.DeviceToken,
                Platform = item.Platform,
                Message = item.Message,
                RetryCount = item.RetryCount + 1
            },
            BackgroundJobPriority.Normal,
            delay: delay
        );
    }
}
```

#### 重试任务

```csharp
// 推送重试任务
public class PushRetryJob : AsyncBackgroundJob<PushRetryArgs>
{
    private readonly IPushNotificationDomainService _pushService;
    
    public override async Task ExecuteAsync(PushRetryArgs args)
    {
        try
        {
            await _pushService.SendPushAsync(
                args.DeviceToken,
                args.Platform,
                args.Message
            );
        }
        catch (Exception ex)
        {
            if (args.RetryCount >= MaxRetries)
            {
                // 达到最大重试次数，标记为永久失败
                Logger.LogError($"Push permanently failed: {args.DeviceToken}");
                await MarkDeviceTokenAsInvalidAsync(args.DeviceToken);
            }
            else
            {
                // 继续重试
                await _retryQueue.EnqueueAsync(new PushRetryItem
                {
                    DeviceToken = args.DeviceToken,
                    Platform = args.Platform,
                    Message = args.Message,
                    RetryCount = args.RetryCount,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
```

## 📈 监控数据流

### 推送指标收集

```csharp
// 推送指标收集
public class PushMetricsCollector
{
    public async Task CollectMetricsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        // 收集今天的推送数据
        var logs = await _logRepository
            .GetQueryable()
            .Where(l => l.SentAt >= today && l.SentAt < today.AddDays(1))
            .ToListAsync();
        
        var metrics = logs
            .GroupBy(l => new { l.Platform, l.MessageType })
            .Select(g => new PushMetrics
            {
                Platform = g.Key.Platform,
                MessageType = g.Key.MessageType,
                SentCount = g.Count(),
                SuccessCount = g.Count(l => l.IsSuccess),
                FailureCount = g.Count(l => !l.IsSuccess),
                SuccessRate = (double)g.Count(l => l.IsSuccess) / g.Count(),
                Date = today
            })
            .ToList();
        
        // 保存到数据库
        foreach (var metric in metrics)
        {
            await _metricsRepository.InsertAsync(metric);
        }
        
        // 发送监控数据到监控系统
        foreach (var metric in metrics)
        {
            _monitoringService.RecordMetric("push.sent", metric.SentCount, new
            {
                platform = metric.Platform,
                message_type = metric.MessageType
            });
            
            _monitoringService.RecordMetric("push.success_rate", metric.SuccessRate, new
            {
                platform = metric.Platform,
                message_type = metric.MessageType
            });
        }
    }
}
```

### 实时监控

```csharp
// 实时推送监控
public class PushMonitoringService
{
    private readonly IHubContext<PushMonitoringHub> _hubContext;
    
    public async Task NotifyPushSentAsync(PushLog log)
    {
        await _hubContext.Clients.All.SendAsync("PushSent", new
        {
            platform = log.Platform,
            deviceToken = log.DeviceToken,
            messageType = log.MessageType,
            isSuccess = log.IsSuccess,
            timestamp = log.SentAt
        });
    }
    
    public async Task NotifyPushMetricsAsync(PushMetrics metrics)
    {
        await _hubContext.Clients.All.SendAsync("PushMetrics", new
        {
            platform = metrics.Platform,
            messageType = metrics.MessageType,
            sentCount = metrics.SentCount,
            successCount = metrics.SuccessCount,
            failureCount = metrics.FailureCount,
            successRate = metrics.SuccessRate,
            date = metrics.Date
        });
    }
}
```

## 🔍 日志数据流

### 结构化日志

```csharp
// 推送日志
public class PushLogger
{
    public void LogPush(PushContext context)
    {
        _logger.LogInformation("Push sent: {@Context}", context);
    }
    
    public void LogPushError(PushContext context, Exception ex)
    {
        _logger.LogError(ex, "Push failed: {@Context}", context);
    }
}

// 日志上下文
public class PushContext
{
    public string Platform { get; set; }
    public string DeviceToken { get; set; }
    public string MessageType { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, object> CustomData { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public int RetryCount { get; set; }
}
```

### 日志查询

```csharp
// 推送日志查询
public class PushLogQueryService
{
    public async Task<List<PushLog>> GetLogsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string platform = null,
        string messageType = null,
        bool? isSuccess = null)
    {
        var query = _logRepository.GetQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(l => l.SentAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(l => l.SentAt < endDate.Value);
        }
        
        if (!string.IsNullOrEmpty(platform))
        {
            query = query.Where(l => l.Platform == platform);
        }
        
        if (!string.IsNullOrEmpty(messageType))
        {
            query = query.Where(l => l.MessageType == messageType);
        }
        
        if (isSuccess.HasValue)
        {
            query = query.Where(l => l.IsSuccess == isSuccess.Value);
        }
        
        return await query.OrderByDescending(l => l.SentAt).ToListAsync();
    }
}
```

## 🎯 性能优化数据流

### 批量推送优化

```csharp
// 批量推送优化
public class BatchPushOptimizer
{
    public async Task SendBatchOptimizedAsync(
        IEnumerable<string> tokens, 
        object message)
    {
        const int batchSize = 500;
        const int parallelBatches = 5;
        
        var batches = tokens
            .Select((token, index) => new { token, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.token))
            .ToList();
        
        // 并行发送多个批次
        var semaphore = new SemaphoreSlim(parallelBatches);
        
        var tasks = batches.Select(async batch =>
        {
            await semaphore.WaitAsync();
            try
            {
                await SendSingleBatchAsync(batch, message);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        await Task.WhenAll(tasks);
    }
}
```

### 缓存优化

```csharp
// Token 缓存优化
public class DeviceTokenCache
{
    private readonly IDistributedCache<List<UserDeviceToken>> _cache;
    
    public async Task<List<UserDeviceToken>> GetTokensAsync(
        Guid userId)
    {
        var cacheKey = $"user_tokens:{userId}";
        var cached = await _cache.GetAsync<List<UserDeviceToken>>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }
        
        // 从数据库加载
        var tokens = await LoadFromDatabaseAsync(userId);
        
        // 缓存 30 分钟
        await _cache.SetAsync(
            cacheKey, 
            tokens, 
            TimeSpan.FromMinutes(30)
        );
        
        return tokens;
    }
    
    public async Task InvalidateCacheAsync(Guid userId)
    {
        var cacheKey = $"user_tokens:{userId}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

## 🔗 参考资料

- [Data Flow Diagram](https://www.ibm.com/docs/en/rational-soft-designer/9.5.1?topic=diagrams-data-flow)
- [Push Notification Best Practices](https://firebase.google.com/docs/cloud-messaging/concept-options)
