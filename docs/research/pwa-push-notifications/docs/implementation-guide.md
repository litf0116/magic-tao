# PWA 推送通知实现指南

## 概述

本文档整理 PWA 推送通知的完整实现方案，包括前端、后端和服务端的集成。

## 核心技术栈

### 前端
- Vue 3 + TypeScript
- Vite PWA Plugin
- Service Worker API
- Web Push API

### 后端
- C# .NET 8
- ABP Framework
- Web Push 库 (待选型)
- Entity Framework Core

### 推送服务
- 选项 1: Firebase Cloud Messaging (FCM)
- 选项 2: Web Push 协议 (自建)
- 选项 3: OneSignal 等第三方服务

## 实现步骤

### 第一阶段：基础架构搭建

#### 1. 前端 Service Worker 设置

```typescript
// 待补充：Service Worker 注册代码
```

#### 2. VAPID 密钥生成

```bash
# 待补充：VAPID 密钥生成命令
```

#### 3. 后端推送服务集成

```csharp
// 待补充：.NET Web Push 集成代码
```

### 第二阶段：订阅管理

#### 1. 前端订阅流程

```typescript
// 待补充：订阅请求代码
```

#### 2. 后端订阅存储

```csharp
// 待补充：订阅实体和存储代码
```

### 第三阶段：推送发送

#### 1. 后端推送 API

```csharp
// 待补充：推送发送 API 代码
```

#### 2. Service Worker 接收处理

```typescript
// 待补充：推送事件处理代码
```

## 关键概念

### VAPID (Voluntary Application Server Identification)

- **作用**: 标识应用服务器身份
- **生成**: 需要生成公钥和私钥对
- **存储**: 私钥保存在服务器，公钥提供给前端

### Push Subscription

- **endpoint**: 推送服务的唯一 URL
- **keys**: 加密密钥 (p256dh, auth)
- **存储**: 需要在后端持久化存储

### Service Worker

- **注册**: 在主线程中注册
- **生命周期**: install, activate, fetch, push
- **作用**: 处理推送事件和显示通知

## 浏览器兼容性

| 浏览器 | 支持情况 | 备注 |
|--------|---------|------|
| Chrome | ✅ 完全支持 | 需要 HTTPS |
| Firefox | ✅ 完全支持 | 需要 HTTPS |
| Safari | ✅ 支持 (macOS 13+) | 需要 HTTPS |
| Edge | ✅ 完全支持 | 需要 HTTPS |
| 微信浏览器 | ❌ 不支持 | 限制较多 |

## 注意事项

1. **HTTPS 要求**: Web Push 必须在 HTTPS 环境下运行
2. **用户授权**: 需要明确请求用户授权
3. **订阅过期**: 订阅可能会过期，需要定期检查
4. **推送服务**: 不同浏览器使用不同的推送服务
5. **电池优化**: 移动端需要考虑电池消耗

---

**状态**: 待补充详细内容  
**更新时间**: 2026-03-07