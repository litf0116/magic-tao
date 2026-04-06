# PWA 推送通知系统架构

## 系统架构图

```
┌─────────────────────────────────────────────────────────────┐
│                        客户端层                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   PC Web     │  │  H5 (UniApp) │  │  小程序/App   │     │
│  │  Vue 3 + SW  │  │  Vue 3 + SW  │  │  (待定)      │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                           ↕ HTTPS
┌─────────────────────────────────────────────────────────────┐
│                      应用服务层                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          Backend API (.NET 8 + ABP)                   │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐    │  │
│  │  │ 订阅管理   │  │ 推送服务   │  │ 消息队列   │    │  │
│  │  │ API       │  │ Service    │  │ (可选)     │    │  │
│  │  └────────────┘  └────────────┘  └────────────┘    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↕
┌─────────────────────────────────────────────────────────────┐
│                      数据存储层                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  MySQL DB    │  │    Redis     │  │   文件存储   │     │
│  │ (订阅数据)   │  │  (缓存/队列) │  │  (VAPID密钥) │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                           ↕
┌─────────────────────────────────────────────────────────────┐
│                      推送服务层                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   FCM        │  │  Web Push    │  │  OneSignal   │     │
│  │ (Google)     │  │  Services    │  │  (第三方)    │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

## 核心组件设计

### 1. 前端组件

#### Service Worker
- **职责**: 处理推送事件、显示通知、处理点击事件
- **技术**: Workbox / 原生 Service Worker API
- **文件**: `sw.js` 或通过 Vite PWA Plugin 生成

#### 推送订阅管理
- **职责**: 请求权限、创建订阅、管理订阅状态
- **技术**: Web Push API
- **实现**: Vue Composable (`usePushNotification`)

### 2. 后端组件

#### 订阅管理服务
```csharp
public interface IPushSubscriptionService
{
    Task SaveSubscriptionAsync(PushSubscriptionDto subscription);
    Task RemoveSubscriptionAsync(string endpoint);
    Task<IEnumerable<PushSubscriptionDto>> GetSubscriptionsAsync();
}
```

#### 推送发送服务
```csharp
public interface IPushNotificationService
{
    Task SendNotificationAsync(PushSubscription subscription, PushMessage message);
    Task SendBatchNotificationsAsync(IEnumerable<PushSubscription> subscriptions, PushMessage message);
}
```

#### VAPID 密钥管理
```csharp
public interface IVapidKeyService
{
    string GetPublicKey();
    string GetPrivateKey();
    void GenerateKeys();
}
```

### 3. 数据模型

#### 推送订阅实体
```csharp
public class PushSubscription : Entity<Guid>
{
    public string Endpoint { get; set; }
    public string P256DH { get; set; }
    public string Auth { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
```

#### 推送消息实体
```csharp
public class PushMessage
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Icon { get; set; }
    public string Image { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public List<NotificationAction> Actions { get; set; }
}
```

## 技术选型

### 推送服务对比

| 服务 | 优势 | 劣势 | 适用场景 |
|------|------|------|---------|
| **FCM** | 免费、稳定、跨平台 | 需要 Google 账号 | 需要移动端支持的项目 |
| **Web Push (自建)** | 完全控制、无依赖 | 需要维护推送服务 | 对隐私要求高的项目 |
| **OneSignal** | 简单易用、免费额度大 | 依赖第三方服务 | 快速集成、中小型项目 |

### .NET 库选型

#### 推荐库: `WebPush.AspNetCore`
- **NuGet**: `Lib.AspNetCore.WebPush`
- **优点**: 
  - 支持最新 Web Push 协议
  - 内置 VAPID 支持
  - ASP.NET Core 集成
  - 活跃维护

**替代方案**:
- `WebPush` (基础库)
- `PushSharp` (多平台支持，但更新较慢)

## 数据流设计

### 订阅流程
```
用户访问 → 请求权限 → 生成订阅对象 → 发送到后端 → 存储到数据库
```

### 推送流程
```
触发事件 → 后端查询订阅 → 推送服务加密 → 发送到推送服务器 → 
推送到客户端 → Service Worker 接收 → 显示通知
```

## 扩展性设计

### 水平扩展
- 无状态推送服务
- 订阅数据分片
- 消息队列缓冲

### 性能优化
- 批量推送
- 异步处理
- 缓存订阅数据
- 连接池管理

---

**状态**: 架构设计初稿  
**更新时间**: 2026-03-07