# Android 通知权限功能回退指南

## 回退策略

### 保留的安全修改

以下修改建议**保留**，不会影响现有功能：

#### 1. AndroidManifest.xml 的权限声明

```xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
```

**原因**:
- ✅ Android 13+ 正常使用
- ✅ Android 12 及以下自动忽略
- ✅ 符合 Google Play 要求
- ✅ 不影响现有功能

---

### 可选回退的修改

如果需要完全回退到之前版本，执行以下操作：

#### 方法一：Git 回退

```bash
# 1. 查看修改文件
git status

# 2. 回退 Flutter 代码修改
git checkout HEAD~1 -- lib/data/services/notification_permission_service.dart
git checkout HEAD~1 -- lib/presentation/pages/settings/settings_page.dart

# 3. 重新构建
flutter clean
flutter pub get
flutter build apk --release
```

#### 方法二：手动回退

##### 回退 notification_permission_service.dart

将文件内容恢复为：

```dart
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';

class NotificationPermissionService {
  static final NotificationPermissionService _instance =
      NotificationPermissionService._internal();
  factory NotificationPermissionService() => _instance;
  NotificationPermissionService._internal();

  Future<bool> checkPermission() async {
    final status = await Permission.notification.status;
    return status.isGranted;
  }

  Future<bool> requestPermission() async {
    final status = await Permission.notification.request();
    return status.isGranted;
  }

  Future<void> showPermissionDialog(BuildContext context) async {
    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: const Text('开启通知权限'),
        content: const Text(
          '您还没有开启通知权限，订阅后将无法收到开拍提醒。\n\n请在设置中开启通知权限，以便及时收到拍品开拍提醒。',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('暂不开启'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('去开启'),
          ),
        ],
      ),
    );

    if (result == true) {
      await openAppSettings();
    }
  }
}
```

##### 回退 settings_page.dart

移除以下修改：

1. **移除 import**:
```dart
// 删除这行
import '../../../data/services/notification_permission_service.dart';
```

2. **移除 with WidgetsBindingObserver**:
```dart
// 从
class _SettingsPageState extends ConsumerState<SettingsPage>
    with WidgetsBindingObserver {

// 改回
class _SettingsPageState extends ConsumerState<SettingsPage> {
```

3. **移除新增变量**:
```dart
// 删除这两行
NotificationPermissionState _systemPermissionState =
    NotificationPermissionState.unknown;
final _permissionService = NotificationPermissionService();
```

4. **移除 initState 中的新增代码**:
```dart
// 删除
WidgetsBinding.instance.addObserver(this);
_checkSystemPermission();
```

5. **移除新增的方法**:
```dart
// 删除以下方法
@override
void dispose() { ... }

@override
void didChangeAppLifecycleState(AppLifecycleState state) { ... }

Future<void> _checkSystemPermission() async { ... }

Future<void> _handlePushNotificationChange(bool value) async { ... }

Widget _buildNotificationSwitchTile() { ... }
```

6. **恢复原有的 _buildSwitchTile**:
```dart
Widget _buildSwitchTile({
  required IconData icon,
  required String title,
  required bool value,
  required ValueChanged<bool> onChanged,
}) {
  return Padding(
    padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
    child: Row(
      children: [
        Container(
          width: 40,
          height: 40,
          decoration: BoxDecoration(
            color: const Color(0xfff6f6f6),
            borderRadius: BorderRadius.circular(8.0),
          ),
          child: Icon(icon, size: 24, color: const Color(0xff1a1a1a)),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Text(
            title,
            style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
          ),
        ),
        Switch(
          value: value,
          onChanged: onChanged,
          activeThumbColor: const Color(0xfff4835a),
        ),
      ],
    ),
  );
}
```

7. **恢复消息通知区域**:
```dart
_buildSectionCard(
  title: '消息通知',
  children: [
    _buildSwitchTile(
      icon: Icons.notifications_outlined,
      title: '推送通知',
      value: _pushNotificationEnabled,
      onChanged: (value) {
        setState(() {
          _pushNotificationEnabled = value;
        });
        _savePushNotificationSetting(value);
      },
    ),
  ],
),
```

---

## 验证回退成功

### 检查清单

- [ ] App 能正常启动
- [ ] 设置页面正常显示
- [ ] 推送开关可以正常切换（仅本地状态）
- [ ] 订阅功能正常（仍然会检查权限并弹窗）
- [ ] 无崩溃、无异常日志

### 测试命令

```bash
# 1. 清理构建
flutter clean

# 2. 重新获取依赖
flutter pub get

# 3. 静态分析
flutter analyze

# 4. 构建 APK
flutter build apk --release

# 5. 安装到设备
adb install build/app/outputs/flutter-apk/app-release.apk
```

---

## 常见问题

### Q1: 回退后订阅功能是否正常？

**A**: 正常。订阅功能的权限检查使用的是 `NotificationPermissionService.checkPermission()` 和 `showPermissionDialog()`，这两个方法在回退版本中仍然存在且功能正常。

### Q2: 回退后极光推送是否正常？

**A**: 正常。极光推送的初始化和消息处理逻辑完全没有修改。

### Q3: 回退后 AndroidManifest.xml 的修改需要撤销吗？

**A**: 不需要。`POST_NOTIFICATIONS` 权限声明是安全的增强，建议保留。

### Q4: 如何判断是否需要回退？

**A**: 出现以下情况时考虑回退：
- 🔴 App 崩溃率显著上升
- 🔴 设置页面无法正常加载
- 🔴 权限检查导致 App 卡顿
- 🟠 大量用户反馈权限相关问题

---

## 联系方式

如有问题，请联系开发团队。

---

## 变更历史

| 日期 | 版本 | 变更内容 |
|------|------|---------|
| 2025-01-XX | v1.0 | 初始版本 - 新增 Android 通知权限检查和引导功能 |
