# 用户消息通知权限检查与引导方案

## 问题分析

用户收不到推送消息可能的原因：
1. **系统通知权限未开启** - iOS/Android 系统层面的通知权限被关闭
2. **App 内推送开关关闭** - 用户在 App 设置中关闭了推送
3. **极光推送未初始化** - Registration ID 未获取
4. **用户未订阅拍品** - 没有调用订阅 API

## 现有实现

### 已有功能
| 组件 | 文件 | 功能 |
|------|------|------|
| NotificationPermissionService | `data/services/notification_permission_service.dart` | 检查/请求系统通知权限 |
| 订阅时权限检查 | `auction_chat_page.dart:1831-1835` | 订阅前检查权限，未开启则弹窗引导 |
| 设置页面开关 | `settings_page.dart:148-159` | 推送通知开关（仅保存本地状态） |

### 存在问题
1. **设置页开关未关联系统权限** - 开关状态与实际系统权限不同步
2. **缺少权限状态展示** - 用户不知道当前系统权限是否开启
3. **缺少 App 启动检查** - 首次安装后未主动请求权限
4. **关闭开关无提示** - 用户关闭开关后无法引导去系统设置

## 解决方案

### 方案一：设置页面增强（推荐）

#### 1. 增强推送通知开关逻辑

**修改文件：** `settings_page.dart`

```dart
import '../../../data/services/notification_permission_service.dart';

class _SettingsPageState extends ConsumerState<SettingsPage> {
  bool _pushNotificationEnabled = true;
  bool _systemPermissionGranted = false;  // 新增：系统权限状态
  final _permissionService = NotificationPermissionService();

  @override
  void initState() {
    super.initState();
    _loadSettings();
    _checkSystemPermission();  // 新增：检查系统权限
    _calculateCacheSize();
    _loadAppVersion();
  }

  /// 检查系统通知权限
  Future<void> _checkSystemPermission() async {
    final granted = await _permissionService.checkPermission();
    if (mounted) {
      setState(() {
        _systemPermissionGranted = granted;
        // 同步系统权限状态到开关（如果系统权限关闭，强制关闭 App 内开关）
        if (!granted) {
          _pushNotificationEnabled = false;
        }
      });
    }
  }

  /// 处理推送开关变化
  Future<void> _handlePushNotificationChange(bool value) async {
    if (value) {
      // 用户想开启推送
      if (!_systemPermissionGranted) {
        // 系统权限未开启，引导去设置
        await _permissionService.showPermissionDialog(context);
        // 返回后重新检查权限
        await _checkSystemPermission();
        return;
      }
    }

    // 更新 App 内开关状态
    setState(() {
      _pushNotificationEnabled = value;
    });
    await _savePushNotificationSetting(value);
  }
}
```

#### 2. 增强 UI 展示

```dart
Widget _buildSwitchTile({
  required IconData icon,
  required String title,
  required bool value,
  required ValueChanged<bool> onChanged,
  String? subtitle,  // 新增：副标题
  bool showWarning = false,  // 新增：是否显示警告
}) {
  return Padding(
    padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
    child: Row(
      children: [
        Container(
          width: 40,
          height: 40,
          decoration: BoxDecoration(
            color: showWarning 
                ? Colors.orange.withValues(alpha: 0.1)
                : const Color(0xfff6f6f6),
            borderRadius: BorderRadius.circular(8.0),
          ),
          child: Icon(
            icon, 
            size: 24, 
            color: showWarning ? Colors.orange : const Color(0xff1a1a1a),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
              ),
              if (subtitle != null) ...[
                const SizedBox(height: 2),
                Text(
                  subtitle,
                  style: TextStyle(
                    fontSize: 12,
                    color: showWarning ? Colors.orange : const Color(0xff999999),
                  ),
                ),
              ],
            ],
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

#### 3. 消息通知设置区域更新

```dart
_buildSectionCard(
  title: '消息通知',
  children: [
    _buildSwitchTile(
      icon: Icons.notifications_outlined,
      title: '推送通知',
      subtitle: _systemPermissionGranted 
          ? null 
          : '系统通知权限未开启，点击去设置',
      showWarning: !_systemPermissionGranted,
      value: _pushNotificationEnabled && _systemPermissionGranted,
      onChanged: _handlePushNotificationChange,
    ),
  ],
),
```

### 方案二：App 启动时主动请求权限

**修改文件：** `main.dart` 或 `app_initializer.dart`

```dart
Future<void> _requestNotificationPermissionIfNeeded() async {
  final permissionService = NotificationPermissionService();
  final hasPermission = await permissionService.checkPermission();
  
  if (!hasPermission) {
    // 首次启动时主动请求权限
    final prefs = await SharedPreferences.getInstance();
    final hasRequested = prefs.getBool('notification_permission_requested') ?? false;
    
    if (!hasRequested) {
      await permissionService.requestPermission();
      await prefs.setBool('notification_permission_requested', true);
    }
  }
}
```

### 方案三：增强 NotificationPermissionService

**修改文件：** `notification_permission_service.dart`

```dart
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';

class NotificationPermissionService {
  static final NotificationPermissionService _instance =
      NotificationPermissionService._internal();
  factory NotificationPermissionService() => _instance;
  NotificationPermissionService._internal();

  /// 检查通知权限状态
  Future<bool> checkPermission() async {
    final status = await Permission.notification.status;
    return status.isGranted;
  }

  /// 获取权限状态详细信息
  Future<PermissionStatus> getPermissionStatus() async {
    return await Permission.notification.status;
  }

  /// 请求通知权限
  Future<bool> requestPermission() async {
    final status = await Permission.notification.request();
    return status.isGranted;
  }

  /// 打开系统设置
  Future<bool> openSettings() async {
    return await openAppSettings();
  }

  /// 显示权限引导对话框（增强版）
  Future<bool> showPermissionDialog(
    BuildContext context, {
    String? title,
    String? message,
    String? confirmText,
    String? cancelText,
  }) async {
    final status = await getPermissionStatus();
    
    String dialogMessage;
    if (status.isPermanentlyDenied) {
      dialogMessage = 
          '您已禁用通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在系统设置中找到"通知"选项并开启。';
    } else {
      dialogMessage = 
          '您还没有开启通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在设置中开启通知权限，以便及时收到拍品开拍提醒。';
    }

    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: Text(title ?? '开启通知权限'),
        content: Text(message ?? dialogMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text(cancelText ?? '暂不开启'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: Text(confirmText ?? '去开启'),
          ),
        ],
      ),
    );

    if (result == true) {
      return await openSettings();
    }
    return false;
  }

  /// 检查权限并在需要时引导用户
  Future<bool> checkAndRequestPermission(BuildContext context) async {
    final hasPermission = await checkPermission();
    if (hasPermission) return true;

    final status = await getPermissionStatus();
    
    // 如果是永久拒绝，直接引导去设置
    if (status.isPermanentlyDenied) {
      return await showPermissionDialog(context);
    }

    // 否则先尝试请求权限
    final granted = await requestPermission();
    if (!granted) {
      return await showPermissionDialog(context);
    }
    return granted;
  }
}
```

### 方案四：订阅入口检查增强

**修改文件：** `auction_chat_page.dart`

```dart
Future<void> _handleSubscribe(AuctionItem auctionItem) async {
  final permissionService = NotificationPermissionService();
  
  // 增强检查：检查并请求权限
  final hasPermission = await permissionService.checkAndRequestPermission(context);
  
  if (!hasPermission) {
    // 用户拒绝了权限，但仍然允许订阅（只是收不到推送）
    showToast('订阅成功，但您未开启通知权限，无法收到开拍提醒');
  }
  
  // 继续订阅逻辑...
  await _subscribeAuction(auctionItem.id);
}
```

## 最佳实践建议

### 1. 权限请求时机

| 时机 | 行为 | 原因 |
|------|------|------|
| App 首次启动 | 主动请求权限 | 用户首次使用时最可能同意 |
| 订阅拍品时 | 检查并引导 | 相关性高，用户有明确需求 |
| 设置页面打开 | 检查并展示状态 | 让用户了解当前状态 |
| 用户开启开关时 | 检查并引导 | 用户主动操作，意愿强 |

### 2. UI/UX 设计原则

1. **状态可见** - 让用户清楚知道当前权限状态
2. **引导清晰** - 明确告诉用户为什么需要权限
3. **操作便捷** - 一键跳转到系统设置
4. **不强制** - 允许用户拒绝权限但继续使用其他功能

### 3. 权限状态处理

```dart
PermissionStatus status = await Permission.notification.status;

if (status.isGranted) {
  // ✅ 权限已开启
} else if (status.isDenied) {
  // ⚠️ 权限被拒绝，可以再次请求
} else if (status.isPermanentlyDenied) {
  // ❌ 权限被永久拒绝，只能去系统设置
} else if (status.isRestricted) {
  // 🔒 权限受限（如家长控制）
} else if (status.isLimited) {
  // ⚡ 权限受限（iOS 专属）
}
```

### 4. iOS/Android 差异处理

**iOS:**
- 首次请求时显示系统弹窗
- 用户拒绝后需要去系统设置
- 支持通知类型设置（横幅、声音、角标）

**Android:**
- 安装时默认授予权限（Android 12 及以下）
- Android 13+ 需要运行时请求
- 支持通知渠道管理

## 实施步骤

### 第一阶段：修复设置页面（优先级：高）
1. 修改 `settings_page.dart` 添加系统权限检查
2. 增强开关 UI 展示权限状态
3. 添加跳转系统设置功能

### 第二阶段：增强权限服务（优先级：中）
1. 扩展 `NotificationPermissionService` 功能
2. 添加永久拒绝状态处理
3. 优化引导对话框文案

### 第三阶段：优化用户体验（优先级：中）
1. App 启动时检查权限状态
2. 订阅时增强权限引导
3. 添加权限状态变更监听

### 第四阶段：数据统计（优先级：低）
1. 统计权限开启率
2. 统计用户拒绝权限后的行为
3. 优化引导策略

## 测试用例

### TC-1: 首次安装检查
**前置:** App 首次安装
**步骤:**
1. 打开设置页面
**预期:** 显示"系统通知权限未开启"警告

### TC-2: 订阅时权限检查
**前置:** 系统通知权限未开启
**步骤:**
1. 在拍卖详情页点击"订阅开拍提醒"
**预期:** 弹出权限引导对话框

### TC-3: 设置页面开关操作
**前置:** 系统通知权限未开启
**步骤:**
1. 打开设置页面
2. 点击推送通知开关
**预期:** 弹出权限引导对话框，引导去系统设置

### TC-4: 权限永久拒绝处理
**前置:** 用户在系统设置中永久拒绝通知权限
**步骤:**
1. 尝试开启推送通知开关
**预期:** 对话框提示"您已禁用通知权限"，引导去系统设置

### TC-5: 权限开启后状态同步
**前置:** 系统通知权限未开启
**步骤:**
1. 在设置页面点击开关
2. 在系统设置中开启权限
3. 返回 App
**预期:** 设置页面开关自动开启，警告消失

## 相关文件

| 文件 | 职责 |
|------|------|
| `data/services/notification_permission_service.dart` | 权限检查与引导服务 |
| `presentation/pages/settings/settings_page.dart` | 设置页面 UI |
| `presentation/pages/chat/auction_chat_page.dart` | 拍卖详情页（订阅入口） |
| `data/services/push_service.dart` | 极光推送服务 |
| `main.dart` | App 入口（启动时权限检查） |
