# Android 通知权限功能测试用例

## 测试环境要求

### 设备要求
- Android 13+ 设备（API 33+）- 测试运行时权限
- Android 12 及以下设备（API ≤32）- 测试兼容性

### 测试账号
- 管理员账号: userId=14
- 普通用户账号: userId=7509

---

## 测试前置条件

1. ✅ App 已安装且为最新版本
2. ✅ 用户已登录
3. ✅ 后端服务正常运行
4. ✅ 极光推送已配置 AppKey: `4e91398522bb1286f6452efb`

---

## 功能测试用例

### TC-01: 设置页面启动时检查权限状态

**测试目的**: 验证设置页面能正确显示当前系统通知权限状态

**前置条件**: 
- App 已启动
- 用户已登录

**测试步骤**:
1. 打开设置页面
2. 观察"消息通知"区域

**预期结果**:
- ✅ 如果系统通知权限已开启：开关显示为开启状态，无警告提示
- ✅ 如果系统通知权限未开启：开关显示为关闭状态，显示橙色警告和提示文字
- ✅ 如果权限被永久拒绝：显示"通知权限已被禁用，点击去设置开启"

**验证方法**:
```bash
# 检查日志输出
adb logcat | grep "NotificationPermission"
```

---

### TC-02: 用户点击开关开启通知权限（Android 13+）

**测试目的**: 验证用户首次请求通知权限的流程

**前置条件**:
- Android 13+ 设备
- 系统通知权限未开启
- App 未请求过通知权限

**测试步骤**:
1. 打开设置页面
2. 点击"推送通知"开关
3. 系统弹出权限请求对话框
4. 选择"允许"

**预期结果**:
- ✅ 系统弹出权限请求对话框
- ✅ 用户选择"允许"后，开关变为开启状态
- ✅ 警告提示消失
- ✅ 后台日志显示权限授予成功

**验证方法**:
```bash
# 检查权限状态
adb shell dumpsys notification | grep "Notification"

# 检查 App 日志
adb logcat | grep -E "NotificationPermission|Permission"
```

---

### TC-03: 用户拒绝权限后再次请求

**测试目的**: 验证用户拒绝权限后可以再次请求

**前置条件**:
- Android 13+ 设备
- 用户已拒绝过通知权限（但未勾选"不再询问"）

**测试步骤**:
1. 打开设置页面
2. 点击"推送通知"开关
3. 系统弹出权限请求对话框
4. 选择"拒绝"
5. 再次点击开关

**预期结果**:
- ✅ 第一次拒绝后，开关保持关闭状态
- ✅ 再次点击时，系统仍然弹出权限请求对话框
- ✅ 用户可以选择"允许"或"拒绝"

---

### TC-04: 权限永久拒绝后的引导

**测试目的**: 验证用户永久拒绝权限后的引导流程

**前置条件**:
- Android 13+ 设备
- 用户已勾选"不再询问"并拒绝权限

**测试步骤**:
1. 打开设置页面
2. 点击"推送通知"开关

**预期结果**:
- ✅ 弹出引导对话框，提示"您已禁用通知权限..."
- ✅ 对话框有两个按钮："暂不开启"和"去开启"
- ✅ 点击"去开启"跳转到系统设置页面
- ✅ 显示橙色警告："通知权限已被禁用，点击去设置开启"

**验证方法**:
```bash
# 检查权限状态
adb shell dumpsys package com.molitao.app | grep "POST_NOTIFICATIONS"
```

---

### TC-05: 从系统设置返回后状态刷新

**测试目的**: 验证用户从系统设置返回后，App 能正确刷新权限状态

**前置条件**:
- 用户已进入系统设置页面

**测试步骤**:
1. 在系统设置中开启通知权限
2. 返回 App（通过返回键或手势）
3. 观察设置页面

**预期结果**:
- ✅ 设置页面的推送通知开关自动变为开启状态
- ✅ 警告提示消失
- ✅ 日志显示 `didChangeAppLifecycleState: resumed`
- ✅ 日志显示权限检查成功

**验证方法**:
```bash
# 监听生命周期变化
adb logcat | grep "AppLifecycleState"
```

---

### TC-06: Android 12 及以下兼容性测试

**测试目的**: 验证 Android 12 及以下设备的权限检查兼容性

**前置条件**:
- Android 12 或更低版本设备

**测试步骤**:
1. 安装 App
2. 打开设置页面
3. 观察推送通知开关状态

**预期结果**:
- ✅ 默认情况下，开关显示为开启状态（安装时自动授权）
- ✅ 如果用户在系统设置中关闭通知，开关显示为关闭状态
- ✅ 点击开关时，引导用户去系统设置

**验证方法**:
```bash
# 检查通知是否启用
adb shell cmd notification allow_listener com.molitao.app
```

---

### TC-07: 订阅拍品时的权限检查

**测试目的**: 验证订阅拍品时能正确检查并引导权限

**前置条件**:
- 系统通知权限未开启
- 用户在拍卖详情页

**测试步骤**:
1. 进入拍卖详情页
2. 点击"订阅开拍提醒"按钮

**预期结果**:
- ✅ 弹出权限引导对话框
- ✅ 对话框提示需要开启通知权限
- ✅ 用户可选择"暂不开启"或"去开启"
- ✅ 选择"去开启"跳转到系统设置

**相关文件**: `lib/presentation/pages/chat/auction_chat_page.dart:1831-1835`

---

### TC-08: 推送功能不影响现有功能

**测试目的**: 验证新增权限功能不影响 App 其他功能

**前置条件**:
- App 已启动
- 用户已登录

**测试步骤**:
1. 测试登录/注册功能
2. 测试浏览拍品列表
3. 测试进入拍卖直播间
4. 测试出价功能
5. 测试个人中心其他设置项

**预期结果**:
- ✅ 所有现有功能正常工作
- ✅ 无崩溃、无卡顿
- ✅ 无异常日志输出

---

## 回归测试检查清单

### 核心功能回归

- [ ] 用户登录/注册正常
- [ ] 拍品列表加载正常
- [ ] 拍卖详情页显示正常
- [ ] WebSocket 连接正常
- [ ] 出价功能正常
- [ ] 订阅功能正常
- [ ] 极光推送接收正常
- [ ] 设置页面其他选项正常

### 性能回归

- [ ] App 启动速度无明显变化
- [ ] 设置页面加载速度正常
- [ ] 权限检查不阻塞 UI 线程

---

## 测试日志收集

### 关键日志点

```bash
# 监听权限相关日志
adb logcat | grep -E "NotificationPermission|Permission|Settings"

# 监听生命周期
adb logcat | grep "AppLifecycleState"

# 监听极光推送
adb logcat | grep "JPush|Push"
```

### 日志示例

**权限检查成功**:
```
I/flutter: [NotificationPermission] 检查权限状态: granted
I/flutter: [Settings] 系统权限状态: NotificationPermissionState.granted
```

**权限请求流程**:
```
I/flutter: [Settings] 用户点击推送开关，当前状态: false
I/flutter: [NotificationPermission] 请求权限...
I/flutter: [NotificationPermission] 权限授予成功
I/flutter: [Settings] 权限状态更新: granted
```

**从设置返回**:
```
I/flutter: didChangeAppLifecycleState: resumed
I/flutter: [Settings] 重新检查权限状态
I/flutter: [NotificationPermission] 检查权限状态: granted
```

---

## 缺陷报告模板

### 缺陷 ID: BUG-XX

**标题**: [简要描述问题]

**严重程度**: 🔴 致命 / 🟠 严重 / 🟡 一般 / 🟢 轻微

**重现步骤**:
1. 
2. 
3. 

**预期结果**: 

**实际结果**: 

**设备信息**:
- 设备型号: 
- Android 版本: 
- App 版本: 

**日志/截图**: 

---

## 回退方案

### 快速回退到上一版本

如果发现严重问题需要回退：

```bash
# 1. 回退代码变更
git checkout HEAD~1 -- lib/data/services/notification_permission_service.dart
git checkout HEAD~1 -- lib/presentation/pages/settings/settings_page.dart

# 2. 重新构建
flutter clean && flutter pub get && flutter build apk

# 3. 安装旧版本
adb install app-release.apk
```

### 保留 AndroidManifest.xml 的权限声明

**AndroidManifest.xml 的修改是安全的增强，建议保留**:
```xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
```

这个权限声明：
- ✅ Android 13+ 设备正常使用
- ✅ Android 12 及以下设备自动忽略
- ✅ 不影响任何现有功能
- ✅ 符合 Google Play 要求

---

## 测试通过标准

### 必须通过项

- [ ] TC-01: 设置页面正确显示权限状态
- [ ] TC-02: Android 13+ 权限请求流程正常
- [ ] TC-05: 从设置返回后状态刷新正常
- [ ] TC-06: Android 12 及以下兼容性正常
- [ ] TC-08: 现有功能不受影响

### 性能指标

- 权限检查耗时 < 500ms
- 页面加载耗时无明显增加
- 无内存泄漏

### 稳定性指标

- 无崩溃
- 无 ANR（Application Not Responding）
- 无异常日志
