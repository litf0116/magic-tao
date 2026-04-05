# PWA 推送通知安全注意事项

## 一、CSRF/XSRF 防护 (关键)

### 1.1 问题说明

根据 MDN 官方警告：

> When implementing PushManager subscriptions, it is vitally important that you protect against CSRF/XSRF issues in your app.

### 1.2 防护方案

#### 方案一：Anti-Forgery Token

```csharp
// 后端 - 启用 CSRF 保护
[ValidateAntiForgeryToken]
[HttpPost("api/web-push/subscribe")]
public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto)
{
    // 订阅逻辑
}
```

```javascript
// 前端 - 发送 CSRF Token
async function subscribeToPush() {
    const csrfToken = document.querySelector('meta[name="csrf-token"]').content;
    
    await fetch('/api/web-push/subscribe', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-Token': csrfToken
        },
        body: JSON.stringify(subscription),
        credentials: 'same-origin'
    });
}
```

#### 方案二：SameSite Cookie

```csharp
// 后端 - Cookie 配置
options.Cookie.SameSite = SameSiteMode.Strict;
options.Cookie.Secure = true;
options.Cookie.HttpOnly = true;
```

#### 方案三：自定义 Header

```javascript
// 前端
await fetch('/api/web-push/subscribe', {
    method: 'POST',
    headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'X-Request-Context': 'web-push-subscribe'
    }
});
```

---

## 二、VAPID 密钥安全

### 2.1 密钥生成

```csharp
using System.Security.Cryptography;

public class VapidKeyGenerator
{
    public static (string PublicKey, string PrivateKey) GenerateKeys()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var privateKey = ecdsa.ExportParameters(true).D;
        var publicKey = ecdsa.ExportParameters(false).Q.X
            .Concat(ecdsa.ExportParameters(false).Q.Y)
            .ToArray();
        
        return (
            Base64UrlEncode(publicKey),
            Base64UrlEncode(privateKey)
        );
    }
    
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
```

### 2.2 密钥存储

| 环境 | 推荐方案 | 风险等级 |
|------|---------|---------|
| **生产环境** | Azure Key Vault / AWS Secrets Manager | 🟢 低风险 |
| **测试环境** | 环境变量 | 🟡 中风险 |
| **开发环境** | appsettings.Development.json | 🟡 中风险 |
| **代码仓库** | ❌ 禁止 | 🔴 高风险 |

### 2.3 密钥轮换策略

```csharp
public class VapidKeyRotationService
{
    // 密钥轮换周期：6-12 个月
    private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(180);
    
    public async Task<VapidDetails> GetActiveKeysAsync()
    {
        var keys = await _cache.GetAsync<VapidDetails>("vapid:current");
        
        if (keys == null || ShouldRotateKeys(keys))
        {
            keys = await RotateKeysAsync();
        }
        
        return keys;
    }
    
    private async Task<VapidDetails> RotateKeysAsync()
    {
        // 1. 生成新密钥
        var newKeys = VapidKeyGenerator.GenerateKeys();
        
        // 2. 保留旧密钥 30 天过渡期
        var oldKeys = await _cache.GetAsync<VapidDetails>("vapid:current");
        if (oldKeys != null)
        {
            await _cache.SetAsync(
                $"vapid:legacy:{DateTime.UtcNow.Ticks}", 
                oldKeys, 
                TimeSpan.FromDays(30)
            );
        }
        
        // 3. 设置新密钥
        var vapidDetails = new VapidDetails
        {
            Subject = "mailto:admin@yourdomain.com",
            PublicKey = newKeys.PublicKey,
            PrivateKey = newKeys.PrivateKey,
            GeneratedAt = DateTime.UtcNow
        };
        
        await _cache.SetAsync("vapid:current", vapidDetails, TimeSpan.FromDays(180));
        
        return vapidDetails;
    }
}
```

---

## 三、消息加密

### 3.1 自动加密

Web Push 协议（RFC 8291）要求所有消息必须加密。使用 `Lib.Net.Http.WebPush` 库会自动处理：

```csharp
// 消息自动加密
var payload = JsonSerializer.Serialize(new
{
    title = "通知标题",
    body = "通知内容",
    data = new { sensitive = "敏感信息" }
});

// 库会自动加密 payload
await _pushClient.RequestPushMessageDeliveryAsync(
    subscription,
    new PushMessage(payload),
    vapidAuthentication
);
```

### 3.2 加密特性

- **算法**: ECDH (P-256) + AES-128-GCM
- **端到端加密**: 推送服务无法读取内容
- **客户端解密**: 浏览器使用订阅密钥解密

---

## 四、订阅端点保护

### 4.1 问题说明

W3C 规范警告：

> The endpoint URL needs to be kept secret, or other applications might be able to send push messages to your application.

### 4.2 防护措施

```csharp
// 后端 - 验证订阅所有权
[HttpPost("api/web-push/send")]
public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
{
    // 1. 验证用户身份
    if (!User.Identity?.IsAuthenticated ?? true)
    {
        return Unauthorized();
    }
    
    // 2. 验证订阅所有权
    var subscription = await _subscriptionRepository
        .FirstOrDefaultAsync(s => s.Endpoint == dto.Endpoint);
    
    if (subscription?.UserId != User.GetUserId())
    {
        return Forbid();
    }
    
    // 3. 速率限制
    if (!await _rateLimiter.TryAcquire(User.GetUserId()))
    {
        return StatusCode(429, "Too many requests");
    }
    
    // 4. 发送通知
    await _pushService.SendNotificationAsync(subscription, dto.Message);
    
    return Ok();
}
```

### 4.3 数据库加密

```csharp
// 敏感字段加密存储
public class WebPushSubscription
{
    public string Endpoint { get; set; }
    
    // 加密存储
    [ProtectedPersonalData]
    public string? P256dh { get; set; }
    
    [ProtectedPersonalData]
    public string? Auth { get; set; }
}
```

---

## 五、速率限制

### 5.1 防止滥用

```csharp
public class PushRateLimiter
{
    private readonly IDistributedCache _cache;
    
    // 每用户每小时最多 100 条
    private const int MaxNotifications = 100;
    private const int WindowSeconds = 3600;
    
    public async Task<bool> TryAcquire(Guid userId)
    {
        var key = $"push-rate:{userId}";
        var count = await _cache.GetStringAsync(key);
        
        var currentCount = string.IsNullOrEmpty(count) ? 0 : int.Parse(count);
        
        if (currentCount >= MaxNotifications)
        {
            return false;
        }
        
        await _cache.SetStringAsync(
            key, 
            (currentCount + 1).ToString(),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(WindowSeconds) 
            }
        );
        
        return true;
    }
}
```

---

## 六、隐私保护

### 6.1 GDPR 合规

#### 用户同意

```javascript
// 明确告知用户并获取同意
function showConsentDialog() {
    return new Promise((resolve) => {
        showDialog({
            title: '开启推送通知',
            content: `
                我们将向您发送以下类型的通知：
                • 新消息提醒
                • 重要更新通知
                • 账户活动提醒
                
                您可以随时在设置中关闭此功能。
            `,
            buttons: [
                { text: '开启', onClick: () => resolve(true) },
                { text: '暂不', onClick: () => resolve(false) }
            ]
        });
    });
}
```

#### 数据删除权

```csharp
// 实现"被遗忘权"
[HttpDelete("api/user/{userId}/push-subscriptions")]
public async Task<IActionResult> DeleteUserPushData(Guid userId)
{
    // 1. 删除所有订阅
    await _subscriptionRepository.DeleteAsync(s => s.UserId == userId);
    
    // 2. 发送静默推送使客户端订阅失效
    await DeactivateAllUserSubscriptions(userId);
    
    // 3. 记录审计日志
    await _auditLog.LogAsync(new AuditLogEntry
    {
        Action = "DeletePushData",
        UserId = userId,
        Timestamp = DateTime.UtcNow
    });
    
    return NoContent();
}
```

### 6.2 元数据隐私

W3C 规范指出：

> The push service is still exposed to the metadata of messages sent by an application server. This includes the timing, frequency, and size of messages.

**缓解措施**:

```csharp
// 消息填充 - 模糊实际内容大小
public string PadPayload(string payload, int targetSize = 1024)
{
    if (payload.Length >= targetSize) return payload;
    
    var padding = new string('X', targetSize - payload.Length);
    return JsonSerializer.Serialize(new
    {
        payload,
        _padding = padding
    });
}
```

---

## 七、安全检查清单

### 部署前检查

- [ ] 所有 API 端点启用 HTTPS
- [ ] CSRF 保护已启用
- [ ] VAPID 私钥安全存储（非代码仓库）
- [ ] 订阅端点验证所有权
- [ ] 速率限制已配置
- [ ] 敏感数据加密存储
- [ ] 用户同意流程符合 GDPR
- [ ] 实现数据删除功能

### 运行时监控

- [ ] 监控异常订阅增长
- [ ] 监控高频推送请求
- [ ] 监控失败率异常
- [ ] 定期审计访问日志
- [ ] 定期轮换 VAPID 密钥

---

**参考资源**:
- [OWASP CSRF 防护备忘单](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)
- [RFC 8291 - Web Push 加密](https://tools.ietf.org/html/rfc8291)
- [RFC 8292 - VAPID 认证](https://tools.ietf.org/html/rfc8292)
- [W3C Push API 安全考虑](https://www.w3.org/TR/push-api/#security-and-privacy-considerations)