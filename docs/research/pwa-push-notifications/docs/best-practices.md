# PWA 推送通知最佳实践

## 用户体验最佳实践

### 1. 权限请求时机

**✅ 推荐**:
- 在用户完成关键操作后请求（如完成注册、下单成功）
- 提供清晰的说明，告知用户为什么要授权
- 给用户选择的机会，不要强制授权

**❌ 避免**:
- 页面加载时立即弹出权限请求
- 在用户不了解应用功能时请求
- 频繁弹出权限请求打扰用户

### 2. 通知内容设计

**标题 (Title)**:
- 简洁明了，控制在 50 字符以内
- 包含关键信息或行动号召
- 使用友好的语气

**正文 (Body)**:
- 详细说明通知内容
- 控制在 150 字符以内
- 突出重要信息

**图标 (Icon)**:
- 使用应用 Logo (192x192 或 512x512)
- 保持与品牌一致
- 确保在小尺寸下清晰可见

**图片 (Image)**:
- 用于增强通知吸引力
- 推荐 16:9 比例
- 控制文件大小 (< 1MB)

### 3. 通知交互设计

**操作按钮 (Actions)**:
```javascript
{
  actions: [
    { action: 'view', title: '查看详情', icon: '/icons/view.png' },
    { action: 'dismiss', title: '忽略', icon: '/icons/dismiss.png' }
  ]
}
```

**点击行为**:
- 打开相关页面
- 携带上下文数据
- 关闭通知

## 技术实现最佳实践

### 1. Service Worker 生命周期管理

```javascript
// 注册 Service Worker
if ('serviceWorker' in navigator) {
  window.addEventListener('load', async () => {
    try {
      const registration = await navigator.serviceWorker.register('/sw.js');
      console.log('SW registered:', registration.scope);
    } catch (error) {
      console.error('SW registration failed:', error);
    }
  });
}
```

### 2. 订阅管理

**订阅创建**:
```javascript
async function subscribeUser() {
  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
  });
  
  // 发送到后端存储
  await fetch('/api/push/subscribe', {
    method: 'POST',
    body: JSON.stringify(subscription),
    headers: { 'Content-Type': 'application/json' }
  });
}
```

**订阅更新**:
- 定期检查订阅状态
- 处理订阅过期
- 重新订阅机制

**取消订阅**:
```javascript
async function unsubscribeUser() {
  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.getSubscription();
  
  if (subscription) {
    await subscription.unsubscribe();
    // 通知后端删除订阅
    await fetch('/api/push/unsubscribe', {
      method: 'POST',
      body: JSON.stringify({ endpoint: subscription.endpoint })
    });
  }
}
```

### 3. 推送事件处理

```javascript
self.addEventListener('push', event => {
  const data = event.data?.json() ?? {};
  
  const options = {
    body: data.body,
    icon: data.icon || '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    image: data.image,
    data: data.data,
    actions: data.actions,
    tag: data.tag, // 防止重复通知
    requireInteraction: data.requireInteraction || false,
    vibrate: [200, 100, 200] // 振动模式
  };
  
  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});
```

### 4. 通知点击处理

```javascript
self.addEventListener('notificationclick', event => {
  event.notification.close();
  
  const action = event.action;
  const data = event.notification.data;
  
  if (action === 'view') {
    // 打开相关页面
    event.waitUntil(
      clients.openWindow(data.url)
    );
  }
});
```

## 安全最佳实践

### 1. VAPID 密钥管理

**密钥生成**:
```bash
# 使用 web-push 库生成
npx web-push generate-vapid-keys
```

**密钥存储**:
- 私钥: 存储在服务器环境变量或密钥管理系统
- 公钥: 提供给前端，可公开

**密钥轮换**:
- 定期更换密钥（建议 6-12 个月）
- 平滑过渡：支持新旧密钥并存
- 通知用户重新订阅

### 2. 订阅数据安全

**存储加密**:
- 敏感字段加密存储（Auth, P256DH）
- 使用安全的加密算法（AES-256）

**访问控制**:
- 只有订阅所有者可以管理自己的订阅
- 管理员推送需要额外权限验证

**数据清理**:
- 定期清理过期订阅
- 删除无效的订阅数据

### 3. 推送内容安全

**内容验证**:
- 验证推送内容格式
- 过滤恶意内容
- 限制推送频率

**HTTPS 要求**:
- 所有通信必须使用 HTTPS
- 使用有效的 SSL 证书

## 性能优化

### 1. 前端优化

- Service Worker 缓存策略
- 延迟加载通知资源
- 避免阻塞主线程

### 2. 后端优化

- 批量推送（每批 1000 个订阅）
- 异步处理推送任务
- 使用消息队列缓冲
- 连接池管理

### 3. 数据库优化

- 订阅表索引优化（endpoint, userId）
- 定期归档历史记录
- 分库分表（大数据量）

## 错误处理

### 常见错误及处理

| 错误 | 原因 | 处理方式 |
|------|------|---------|
| 410 Gone | 订阅已失效 | 删除订阅，提示用户重新订阅 |
| 413 Payload Too Large | 推送内容过大 | 减少数据大小，使用 URL 引导 |
| 429 Too Many Requests | 推送频率过高 | 降低推送频率，添加重试逻辑 |
| 订阅失败 | 权限被拒绝 | 提供降级方案，引导用户开启权限 |

### 重试策略

```csharp
// 指数退避重试
var retryPolicy = Policy
    .Handle<PushServiceException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

## 监控与分析

### 关键指标

- 订阅成功率
- 推送送达率
- 用户点击率
- 订阅留存率
- 推送延迟

### 监控工具

- Application Insights (Azure)
- Prometheus + Grafana
- 自定义日志系统

---

**状态**: 最佳实践初稿  
**更新时间**: 2026-03-07