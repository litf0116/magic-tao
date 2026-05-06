# Android 通知权限支持方案

## Android 版本差异

### 版本分布（中国区 2024）

| Android 版本 | API Level | 市场占比 | 通知权限处理 |
|-------------|-----------|---------|-------------|
| Android 14 | 34 | ~15% | 运行时权限 |
| Android 13 | 33 | ~25% | 运行时权限 |
| Android 12 | 31-32 | ~30% | 安装时授予 |
| Android 11 | 30 | ~20% | 安装时授予 |
| Android 10 及以下 | ≤29 | ~10% | 安装时授予 |

**关键分水岭**: Android 13 (API 33)

---

## Android 13+ (API 33+) 处理方案

### 1. 权限声明

**文件**: `android/app/src/main/AndroidManifest.xml`

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:tools="http://schemas.android.com/tools">
    
    <!-- 现有权限 -->
    <uses-permission android:name="android.permission.INTERNET"/>
    <uses-permission android:name="android.permission.CAMERA"/>
    <!-- ... -->
    
    <!-- ✅ 新增：Android 13+ 通知权限 -->
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
    
    <application ...>
        <!-- ... -->
    </application>
</manifest>
```

**说明**:
- `POST_NOTIFICATIONS` 是 Android 13 引入的新权限
- 即使不声明，`permission_handler` 也能正常工作，但声明后更规范
- 旧版本 Android 会自动忽略此权限声明

### 2. 运行时权限请求

**Flutter 层实现** (已存在 `NotificationPermissionService`):

```dart
import 'package:permission_handler/permission_handler.dart';

class NotificationPermissionService {
  /// 检查通知权限状态
  Future<bool> checkPermission() async {
    final status = await Permission.notification.status;
    return status.isGranted;
  }

  /// 获取详细权限状态
  Future<PermissionStatus> getPermissionStatus() async {
    return await Permission.notification.status;
  }

  /// 请求通知权限
  Future<bool> requestPermission() async {
    final status = await Permission.notification.request();
    return status.isGranted;
  }

  /// 检查是否永久拒绝
  Future<bool> isPermanentlyDenied() async {
    return await Permission.notification.isPermanentlyDenied;
  }

  /// 打开系统设置
  Future<bool> openSettings() async {
    return await openAppSettings();
  }
}
```

### 3. 权限状态处理流程

```
┌─────────────────────────────────────────────────────────┐
│                  Android 13+ 权限流程                     │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │  检查权限状态          │
              │  Permission.status    │
              └───────────────────────┘
                          │
            ┌─────────────┼─────────────┐
            │             │             │
            ▼             ▼             ▼
      isGranted      isDenied   isPermanentlyDenied
      (已授权)        (被拒绝)      (永久拒绝)
            │             │             │
            ▼             ▼             ▼
      正常使用      再次请求      引导去设置
                         │
                         ▼
                  用户选择授权?
                    /        \
                  是          否
                  /            \
                 ▼              ▼
            isGranted    isPermanentlyDenied
                              │
                              ▼
                         引导去设置
```

---

## Android 12 及以下 (API ≤32) 处理方案

### 权限特点

1. **安装时自动授予** - App 安装后默认拥有通知权限
2. **用户可手动关闭** - 在系统设置中可以关闭 App 的通知
3. **检查方法不同** - 使用 `NotificationManagerCompat.areNotificationsEnabled()`

### 兼容性处理

**`permission_handler` 已自动处理版本差异**:

```dart
// permission_handler 内部实现逻辑（简化版）
Future<PermissionStatus> status() async {
  if (Build.VERSION.SDK_INT >= 33) {
    // Android 13+: 检查 POST_NOTIFICATIONS 权限
    return checkSelfPermission(POST_NOTIFICATIONS);
  } else {
    // Android 12-: 检查 NotificationManager.areNotificationsEnabled()
    return NotificationManagerCompat.areNotificationsEnabled();
  }
}
```

**我们的代码无需关心版本差异**:

```dart
// ✅ 正确：permission_handler 自动处理版本差异
final hasPermission = await Permission.notification.status.isGranted;

// ❌ 错误：不需要手动判断版本
if (Platform.isAndroid && androidVersion >= 33) {
  // ...
}
```

---

## 完整实现方案

### 1. 更新 AndroidManifest.xml

**需要添加的内容**:

```xml
<!-- Android 13+ 通知权限 -->
<uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
```

**完整示例**:

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:tools="http://schemas.android.com/tools">
    
    <!-- 网络权限 -->
    <uses-permission android:name="android.permission.INTERNET"/>
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE"/>
    <uses-permission android:name="android.permission.ACCESS_WIFI_STATE"/>
    
    <!-- 硬件权限 -->
    <uses-permission android:name="android.permission.CAMERA"/>
    <uses-feature android:name="android.hardware.camera" android:required="false"/>
    <uses-feature android:name="android.hardware.camera.autofocus" android:required="false"/>
    
    <!-- 通知相关 -->
    <uses-permission android:name="android.permission.VIBRATE"/>
    <uses-permission android:name="android.permission.RECEIVE_USER_PRESENT"/>
    
    <!-- ✅ Android 13+ 通知权限 -->
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
    
    <application ...>
        <!-- 极光推送配置 -->
        <meta-data
            android:name="JPUSH_CHANNEL"
            android:value="developer-default"/>
        <meta-data
            android:name="JPUSH_APPKEY"
            android:value="4e91398522bb1286f6452efb"/>
        
        <!-- ... -->
    </application>
</manifest>
```

### 2. 增强 NotificationPermissionService

**文件**: `lib/data/services/notification_permission_service.dart`

```dart
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';

/// 通知权限状态枚举
enum NotificationPermissionState {
  granted,           // 已授权
  denied,            // 被拒绝（可再次请求）
  permanentlyDenied, // 永久拒绝（需去设置）
  restricted,        // 受限（家长控制等）
  limited,           // 有限授权（iOS）
  unknown,           // 未知状态
}

class NotificationPermissionService {
  static final NotificationPermissionService _instance =
      NotificationPermissionService._internal();
  factory NotificationPermissionService() => _instance;
  NotificationPermissionService._internal();

  /// 检查通知权限状态（详细）
  Future<NotificationPermissionState> getPermissionState() async {
    try {
      final status = await Permission.notification.status;
      
      if (status.isGranted) {
        return NotificationPermissionState.granted;
      } else if (status.isPermanentlyDenied) {
        return NotificationPermissionState.permanentlyDenied;
      } else if (status.isDenied) {
        return NotificationPermissionState.denied;
      } else if (status.isRestricted) {
        return NotificationPermissionState.restricted;
      } else if (status.isLimited) {
        return NotificationPermissionState.limited;
      } else {
        return NotificationPermissionState.unknown;
      }
    } catch (e) {
      debugPrint('[NotificationPermission] 检查权限失败: $e');
      return NotificationPermissionState.unknown;
    }
  }

  /// 检查权限是否已授权（简化版）
  Future<bool> checkPermission() async {
    final state = await getPermissionState();
    return state == NotificationPermissionState.granted;
  }

  /// 请求通知权限
  Future<NotificationPermissionState> requestPermission() async {
    try {
      final status = await Permission.notification.request();
      
      if (status.isGranted) {
        return NotificationPermissionState.granted;
      } else if (status.isPermanentlyDenied) {
        return NotificationPermissionState.permanentlyDenied;
      } else {
        return NotificationPermissionState.denied;
      }
    } catch (e) {
      debugPrint('[NotificationPermission] 请求权限失败: $e');
      return NotificationPermissionState.unknown;
    }
  }

  /// 打开系统设置
  Future<bool> openSettings() async {
    try {
      return await openAppSettings();
    } catch (e) {
      debugPrint('[NotificationPermission] 打开设置失败: $e');
      return false;
    }
  }

  /// 显示权限引导对话框（增强版）
  Future<bool> showPermissionDialog(
    BuildContext context, {
    String? title,
    String? message,
    String? confirmText,
    String? cancelText,
  }) async {
    final state = await getPermissionState();
    
    String dialogTitle = title ?? '开启通知权限';
    String dialogMessage;
    
    if (state == NotificationPermissionState.permanentlyDenied) {
      dialogMessage = message ?? 
          '您已禁用通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在系统设置中找到"通知"选项并开启。';
    } else if (Platform.isAndroid) {
      dialogMessage = message ??
          '您还没有开启通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在设置中开启通知权限，以便及时收到拍品开拍提醒。';
    } else {
      dialogMessage = message ??
          '您还没有开启通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在设置中开启通知权限，以便及时收到拍品开拍提醒。';
    }

    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: Text(dialogTitle),
        content: Text(dialogMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text(cancelText ?? '暂不开启'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: Text(
              confirmText ?? '去开启',
              style: const TextStyle(color: Color(0xfff4835a)),
            ),
          ),
        ],
      ),
    );

    if (result == true) {
      return await openSettings();
    }
    return false;
  }

  /// 检查权限并在需要时引导用户（完整流程）
  Future<bool> checkAndRequestPermission(BuildContext context) async {
    // 1. 检查当前权限状态
    final state = await getPermissionState();
    
    if (state == NotificationPermissionState.granted) {
      return true; // 已有权限
    }
    
    // 2. 如果是永久拒绝，直接引导去设置
    if (state == NotificationPermissionState.permanentlyDenied) {
      return await showPermissionDialog(context);
    }
    
    // 3. 尝试请求权限
    final newState = await requestPermission();
    
    if (newState == NotificationPermissionState.granted) {
      return true; // 用户同意授权
    }
    
    // 4. 用户拒绝，引导去设置
    if (newState == NotificationPermissionState.permanentlyDenied) {
      return await showPermissionDialog(context);
    }
    
    return false;
  }
}
```

### 3. 设置页面集成

**文件**: `lib/presentation/pages/settings/settings_page.dart`

```dart
import '../../../data/services/notification_permission_service.dart';

class _SettingsPageState extends ConsumerState<SettingsPage> {
  bool _pushNotificationEnabled = true;
  NotificationPermissionState _systemPermissionState = 
      NotificationPermissionState.unknown;
  
  final _permissionService = NotificationPermissionService();

  @override
  void initState() {
    super.initState();
    _loadSettings();
    _checkSystemPermission();
    _calculateCacheSize();
    _loadAppVersion();
  }

  /// 检查系统通知权限状态
  Future<void> _checkSystemPermission() async {
    final state = await _permissionService.getPermissionState();
    if (mounted) {
      setState(() {
        _systemPermissionState = state;
        // 如果系统权限关闭，强制关闭 App 内开关
        if (state != NotificationPermissionState.granted) {
          _pushNotificationEnabled = false;
        }
      });
    }
  }

  /// 处理推送通知开关变化
  Future<void> _handlePushNotificationChange(bool value) async {
    if (value) {
      // 用户想开启推送
      if (_systemPermissionState != NotificationPermissionState.granted) {
        // 系统权限未开启，引导用户
        final granted = await _permissionService.checkAndRequestPermission(context);
        if (granted) {
          // 用户开启权限，返回后刷新状态
          await _checkSystemPermission();
          setState(() {
            _pushNotificationEnabled = true;
          });
          await _savePushNotificationSetting(true);
        }
        return;
      }
    }
    
    // 更新开关状态
    setState(() {
      _pushNotificationEnabled = value;
    });
    await _savePushNotificationSetting(value);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // ...
      body: Container(
        child: Column(
          children: [
            _buildSectionCard(
              title: '消息通知',
              children: [
                _buildNotificationSwitchTile(),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildNotificationSwitchTile() {
    final isGranted = _systemPermissionState == NotificationPermissionState.granted;
    final showWarning = !isGranted;
    
    String? subtitle;
    if (_systemPermissionState == NotificationPermissionState.permanentlyDenied) {
      subtitle = '通知权限已被禁用，点击去设置开启';
    } else if (_systemPermissionState == NotificationPermissionState.denied) {
      subtitle = '系统通知权限未开启，点击去开启';
    } else if (_systemPermissionState == NotificationPermissionState.unknown) {
      subtitle = '正在检查权限状态...';
    }

    return _buildSwitchTile(
      icon: Icons.notifications_outlined,
      title: '推送通知',
      subtitle: subtitle,
      showWarning: showWarning,
      value: _pushNotificationEnabled && isGranted,
      onChanged: _handlePushNotificationChange,
    );
  }
}
```

---

## 测试要点

### Android 13+ 测试

1. **首次请求权限**
   - App 首次请求通知权限时，系统弹出授权对话框
   - 用户选择"允许" → 权限授予成功
   - 用户选择"拒绝" → 可以再次请求

2. **永久拒绝处理**
   - 用户勾选"不再询问"并拒绝 → 状态为 `permanentlyDenied`
   - App 引导用户去系统设置手动开启

3. **从设置返回**
   - 使用 `WidgetsBindingObserver` 监听 `AppLifecycleState.resumed`
   - 用户从设置返回后，重新检查权限状态

### Android 12 及以下测试

1. **默认状态**
   - App 安装后默认拥有通知权限
   - 用户可在系统设置中关闭

2. **权限关闭后**
   - `Permission.notification.status` 返回 `denied`
   - 引导用户去系统设置开启

---

## 常见问题

### Q1: 为什么 Android 13+ 需要运行时权限？

**A**: Google 为了减少通知骚扰，在 Android 13 中引入了 `POST_NOTIFICATIONS` 运行时权限。用户必须显式授权才能发送通知。

### Q2: 不声明 POST_NOTIFICATIONS 会怎样？

**A**: `permission_handler` 仍然可以工作，但：
- Android 13+ 用户默认不会收到通知
- 需要运行时请求权限
- 建议显式声明以符合最佳实践

### Q3: 如何兼容所有 Android 版本？

**A**: 使用 `permission_handler` 包，它会自动处理版本差异：
- Android 13+: 检查 `POST_NOTIFICATIONS` 权限
- Android 12-: 使用 `NotificationManagerCompat.areNotificationsEnabled()`

### Q4: 用户从设置返回后如何刷新状态？

**A**: 使用 `WidgetsBindingObserver` 监听生命周期：

```dart
class _SettingsPageState extends ConsumerState<SettingsPage>
    with WidgetsBindingObserver {
  
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      // 用户从设置返回，重新检查权限
      _checkSystemPermission();
    }
  }
}
```

---

## 实施检查清单

- [ ] 添加 `POST_NOTIFICATIONS` 权限声明到 AndroidManifest.xml
- [ ] 增强 `NotificationPermissionService` 添加详细状态枚举
- [ ] 设置页面集成系统权限检查
- [ ] 添加权限状态展示 UI
- [ ] 测试 Android 13+ 权限请求流程
- [ ] 测试 Android 12 及以下兼容性
- [ ] 测试永久拒绝场景
- [ ] 测试从设置返回后状态刷新
