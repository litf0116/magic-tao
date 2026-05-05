import 'package:flutter/material.dart';

/// App底部弹窗配置
/// 遵循 iOS Human Interface Guidelines
class AppBottomSheetConfig {
  /// iOS HIG 标准圆角
  static const double borderRadius = 12.0;

  /// iOS HIG 拖动指示器尺寸
  static const double dragIndicatorWidth = 35.0;
  static const double dragIndicatorHeight = 5.0;
  static const double dragIndicatorRadius = 2.5;

  /// 拖动指示器距顶部间距
  static const double dragIndicatorTopMargin = 8.0;

  /// 遮罩颜色 (iOS 标准 rgba(0,0,0,0.4))
  static const Color modalBarrierColor = Color(0x66000000);

  /// 拖动指示器颜色 (iOS #E5E5EA)
  static const Color dragIndicatorColor = Color(0xFFE5E5EA);
}

/// iOS 风格底部弹窗包装组件
class AppBottomSheet extends StatelessWidget {
  final Widget child;
  final bool showDragIndicator;
  final double? borderRadius;
  final Color? backgroundColor;

  const AppBottomSheet({
    super.key,
    required this.child,
    this.showDragIndicator = true,
    this.borderRadius,
    this.backgroundColor,
  });

  @override
  Widget build(BuildContext context) {
    final radius = borderRadius ?? AppBottomSheetConfig.borderRadius;

    return Material(
      color: backgroundColor ?? Colors.white,
      borderRadius: BorderRadius.vertical(top: Radius.circular(radius)),
      clipBehavior: Clip.antiAlias,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (showDragIndicator) ...[
            SizedBox(height: AppBottomSheetConfig.dragIndicatorTopMargin),
            _buildDragIndicator(),
          ],
          Flexible(child: child),
        ],
      ),
    );
  }

  Widget _buildDragIndicator() {
    return Container(
      width: AppBottomSheetConfig.dragIndicatorWidth,
      height: AppBottomSheetConfig.dragIndicatorHeight,
      decoration: BoxDecoration(
        color: AppBottomSheetConfig.dragIndicatorColor,
        borderRadius: BorderRadius.circular(AppBottomSheetConfig.dragIndicatorRadius),
      ),
    );
  }
}

/// 底部弹窗 builder 工具函数
/// 使用方式: showAppBottomSheet(context, (context) => YourContent())
Future<T?> showAppBottomSheet<T>({
  required BuildContext context,
  required Widget Function(BuildContext context) builder,
  bool showDragIndicator = true,
  double? borderRadius,
  Color? backgroundColor,
  bool isScrollControlled = true,
}) {
  return showModalBottomSheet<T>(
    context: context,
    isScrollControlled: isScrollControlled,
    backgroundColor: Colors.transparent,
    barrierColor: AppBottomSheetConfig.modalBarrierColor,
    builder: (context) => AppBottomSheet(
      showDragIndicator: showDragIndicator,
      borderRadius: borderRadius,
      backgroundColor: backgroundColor,
      child: builder(context),
    ),
  );
}

/// 可滚动底部弹窗（用于 DraggableScrollableSheet 等可滚动内容）
/// 注意：builder 返回的 Widget 必须自己处理 SafeArea
Future<T?> showScrollableBottomSheet<T>({
  required BuildContext context,
  required Widget Function(BuildContext context) builder,
  double? borderRadius,
  Color? backgroundColor,
}) {
  final radius = borderRadius ?? AppBottomSheetConfig.borderRadius;
  return showModalBottomSheet<T>(
    context: context,
    isScrollControlled: true,
    backgroundColor: backgroundColor ?? Colors.white,
    barrierColor: AppBottomSheetConfig.modalBarrierColor,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(
        top: Radius.circular(radius),
      ),
    ),
    builder: builder,
  );
}

/// 通用操作列表底部弹窗
class AppActionSheet extends StatelessWidget {
  final List<ActionSheetItem> actions;
  final VoidCallback? onCancel;

  const AppActionSheet({
    super.key,
    required this.actions,
    this.onCancel,
  });

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          ...actions.map((action) => _buildActionItem(context, action)),
          if (onCancel != null) ...[
            const Divider(height: 1),
            ListTile(
              title: const Text(
                '取消',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w500,
                ),
              ),
              onTap: () {
                Navigator.pop(context);
                onCancel?.call();
              },
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildActionItem(BuildContext context, ActionSheetItem action) {
    return Column(
      children: [
        ListTile(
          leading: action.icon != null ? Icon(action.icon, color: action.iconColor) : null,
          title: Text(
            action.title,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w500,
              color: action.isDestructive ? Colors.red : null,
            ),
          ),
          onTap: () {
            Navigator.pop(context);
            action.onTap?.call();
          },
        ),
        if (action.showDivider) const Divider(height: 1),
      ],
    );
  }
}

/// 操作列表项
class ActionSheetItem {
  final String title;
  final VoidCallback? onTap;
  final IconData? icon;
  final Color? iconColor;
  final bool isDestructive;
  final bool showDivider;

  const ActionSheetItem({
    required this.title,
    this.onTap,
    this.icon,
    this.iconColor,
    this.isDestructive = false,
    this.showDivider = true,
  });
}
