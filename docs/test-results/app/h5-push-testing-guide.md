# H5 PWA 推送功能测试指南

## 功能概述

本次更新实现了 H5 平台的 PWA 推送通知功能，使用 Web Push API 替代 App 端的 JPush。

## 已完成的功能

### 1. 核心服务实现

#### `src/utils/pushH5.ts` - H5 推送服务类
- ✅ `H5PushService` 类，管理推送订阅和权限
- ✅ `init()` - 初始化 Service Worker 和推送服务
- ✅ `requestPermission()` - 请求推送权限并订阅
- ✅ `unsubscribe()` - 取消推送订阅
- ✅ `getPermissionStatus()` - 获取当前权限状态
- ✅ VAPID 密钥配置（已生成真实密钥对）

#### `src/sw.js` - Service Worker
- ✅ 静态资源缓存策略
- ✅ API 请求缓存（network-first）
- ✅ Push 事件监听和处理
- ✅ Notification 点击事件处理
- ✅ Background sync 支持

### 2. UI 组件

#### `src/components/common/PushPermissionDialog.vue`
- ✅ 推送权限请求对话框
- ✅ 友好的功能说明（拍卖提醒、出价通知等）
- ✅ "立即开启" 和 "暂不开启" 按钮
- ✅ 优美的动画效果

#### `src/composables/usePushPermission.ts`
- ✅ `usePushPermission()` composable
- ✅ 推送权限状态管理
- ✅ 智能对话框显示逻辑
- ✅ 首次访问时自动提示（可配置）

### 3. 应用初始化

#### `index.html`
- ✅ Service Worker 注册代码
- ✅ 支持 HTTPS 和 localhost
- ✅ 错误处理和日志输出

#### `src/main.ts`
- ✅ 应用启动时初始化 h5PushService
- ✅ 使用条件编译隔离 H5 代码
- ✅ 错误捕获和处理

## 测试前准备

### 1. 启动开发服务器

```bash
cd molitao_uniapp
npm run dev:h5
```

服务器将运行在 `http://localhost:5173`（或其他端口）

### 2. 测试环境要求

- ✅ 现代浏览器（Chrome、Edge、Firefox、Safari）
- ✅ 必须使用 HTTPS 或 localhost
- ✅ 浏览器支持 Service Worker 和 Web Push API

### 3. 测试账号

使用以下账号进行测试：
- 管理员：`admin / 123456`
- 测试用户：`feifei / 123456`
- 测试用户：`oFzSV6st7nn8ZeoTEQqbveyjfMAU / 123456`

## 测试步骤

### 测试 1: Service Worker 注册

**步骤：**
1. 打开浏览器开发者工具（F12）
2. 切换到 Console 标签
3. 访问 H5 应用
4. 查看控制台输出

**预期结果：**
```
[SW] Service Worker 注册成功: https://localhost:5173/
[Main] H5 推送服务初始化完成
[H5Push] Service Worker 已就绪
[H5Push] 未找到订阅，等待用户授权
```

**验证点：**
- ✅ Application 标签中可以看到 Service Worker 已注册
- ✅ Service Worker 状态为 "activated"

---

### 测试 2: 推送权限请求

**步骤：**
1. 登录应用（使用测试账号）
2. 在设置或合适的位置调用 `usePushPermission().showPermissionDialog()`
3. 点击 "立即开启" 按钮

**预期结果：**
```
[H5Push] 请求通知权限...
[H5Push] 权限已授予
[H5Push] 订阅推送服务...
[H5Push] 订阅成功: https://fcm.googleapis.com/...
[H5Push] 发送订阅信息到后端...
```

**验证点：**
- ✅ 浏览器弹出权限请求提示
- ✅ 授权后显示 "推送通知已开启" 提示
- ✅ Application 标签中可以看到 "Push Subscription" 信息

---

### 测试 3: 推送订阅信息

**步骤：**
1. 打开开发者工具 Application 标签
2. 展开 Service Workers
3. 选择 Push Subscription

**预期结果：**
```
Endpoint: https://fcm.googleapis.com/...
Keys:
  - p256dh: BASE64_ENCODED_STRING
  - auth: BASE64_ENCODED_STRING
```

**验证点：**
- ✅ 订阅信息包含有效的 endpoint
- ✅ 包含 p256dh 和 auth 密钥
- ✅ 这些信息将发送到后端用于推送

---

### 测试 4: 离线缓存（PWA）

**步骤：**
1. 访问应用并等待 Service Worker 激活
2. 在开发者工具 Network 标签中勾选 "Offline"
3. 刷新页面

**预期结果：**
- ✅ 页面仍然可以正常显示
- ✅ 静态资源从 Service Worker 缓存加载
- ✅ Network 标签显示 "(from ServiceWorker)"

---

### 测试 5: 消息推送（需要后端支持）

**前提条件：**
- 后端需要实现 `/api/push/subscribe` 接口
- 后端需要使用 Web Push 协议发送消息

**步骤：**
1. 确保已订阅推送
2. 后端发送测试推送消息
3. 观察浏览器通知

**预期结果：**
- ✅ 浏览器显示推送通知
- ✅ 点击通知可以打开应用
- ✅ Service Worker 控制台显示推送接收日志

---

## 已知限制

### 1. 浏览器兼容性
- ✅ Chrome/Edge: 完全支持
- ✅ Firefox: 完全支持
- ⚠️ Safari: 部分 macOS/iOS 版本可能不支持

### 2. HTTPS 要求
- Web Push API 必须在 HTTPS 环境下使用
- 本地开发可以使用 localhost

### 3. 后端集成
- 当前实现已完成前端部分
- 后端需要实现：
  - `/api/push/subscribe` - 保存推送订阅信息
  - `/api/push/send` - 统一推送接口（支持 App 和 H5）
  - 使用 Web Push 库（如 web-push）发送 H5 推送

### 4. 后端 VAPID 配置

**生成的 VAPID 密钥对：**
```
Public Key: MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEwFIhGozygJ_yRTL6h3HQFXyCtD4xsJaZ-H9W2vu8ejKt3iWz4dvGdKnR1mnWHaT4msQmT4vblTr0_5H4Xmrp6g
Private Key: MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQglIpGnoJLAumDMTMT-bvhvi_iUrzfgani9WfaZlZjaWhRANCAATAUiEajPKAn_JFMvqHcdAVfIK0PjGwlpn4f1ba-7x6Mq3eJbPh28Z0qdHWadYdpPiaxCZPi9uVOvT_kfheaunq
```

**⚠️ 安全提醒：**
- 私钥必须安全存储在后端服务器
- 不要将私钥提交到前端代码或版本控制
- 使用环境变量或密钥管理服务存储私钥

---

## 下一步开发建议

### Phase 3: 后端推送集成

1. **创建后端推送 API**
   - `POST /api/push/subscribe` - 保存 H5 推送订阅
   - `POST /api/push/unsubscribe` - 删除 H5 推送订阅
   - `POST /api/push/send` - 统一推送接口

2. **实现多平台推送**
   - App: 使用 JPush
   - H5: 使用 Web Push
   - 小程序: 使用模板消息

3. **推送内容管理**
   - 拍卖开始提醒
   - 出价被超越通知
   - 拍卖结果通知
   - 系统消息

### Phase 4: 用户体验优化

1. **设置页面**
   - 添加推送开关
   - 推送类型偏好设置
   - 推送时间段设置

2. **推送历史**
   - 查看历史推送记录
   - 点击跳转到相关页面

3. **测试工具**
   - 开发环境下发送测试推送
   - 推送模板预览

---

## 提交记录

```
commit a159342 feat: 实现H5 PWA推送功能，完整支持Web Push API
- 修复 pushH5.ts TypeScript类型错误，使用ExtendedPushSubscription接口
- 生成VAPID密钥对，配置公钥和私钥
- 在index.html中添加Service Worker注册代码（支持HTTPS和localhost）
- 在main.ts中初始化h5PushService，使用条件编译隔离H5代码
- 创建PushPermissionDialog.vue组件，提供用户友好的权限请求UI
- 创建usePushPermission.ts composable，统一管理推送权限请求流程
- 添加完整的错误处理和用户反馈机制
```

---

## 相关文档

- [Web Push API - MDN](https://developer.mozilla.org/en-US/docs/Web/API/Push_API)
- [Service Worker - MDN](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Web Push Protocol - RFC 8030](https://tools.ietf.org/html/rfc8030)
- [VAPID Specification](https://datatracker.ietf.org/doc/html/draft-ietf-webpush-vapid-01)

---

## 技术支持

如有问题，请查看以下资源：
- 浏览器开发者工具 Console 日志
- Service Worker 详细日志
- Application 标签中的 Service Workers 和 Push Notifications 状态
