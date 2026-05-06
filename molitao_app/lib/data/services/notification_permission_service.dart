import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';
import '../../core/theme/app_colors.dart';

/// 通知权限状态枚举
enum NotificationPermissionState {
  granted, // 已授权
  denied, // 被拒绝（可再次请求）
  permanentlyDenied, // 永久拒绝（需去设置）
  restricted, // 受限（家长控制等）
  limited, // 有限授权（iOS）
  unknown, // 未知状态
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

  /// 检查权限是否已授权（简化版 - 兼容旧代码）
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
    // 检查 context 是否有效
    if (!context.mounted) return false;
    
    final state = await getPermissionState();

    String dialogTitle = title ?? '开启通知权限';
    String dialogMessage;

    if (state == NotificationPermissionState.permanentlyDenied) {
      dialogMessage = message ??
          '您已禁用通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在系统设置中找到"通知"选项并开启。';
    } else {
      dialogMessage = message ??
          '您还没有开启通知权限，订阅后将无法收到开拍提醒。\n\n'
          '请在设置中开启通知权限，以便及时收到拍品开拍提醒。';
    }

    // 再次检查 context 是否有效（await 之后）
    if (!context.mounted) return false;
    
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
              style: TextStyle(color: AppColors.primary),
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
      if (!context.mounted) return false;
      return await showPermissionDialog(context);
    }

    // 3. 尝试请求权限
    final newState = await requestPermission();

    if (newState == NotificationPermissionState.granted) {
      return true; // 用户同意授权
    }

    // 4. 用户拒绝，引导去设置
    if (newState == NotificationPermissionState.permanentlyDenied) {
      if (!context.mounted) return false;
      return await showPermissionDialog(context);
    }

    return false;
  }
}
