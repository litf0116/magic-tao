import "package:flutter/material.dart";

/// 拉黑确认对话框
/// 
/// 显示确认拉黑用户的对话框，返回用户确认结果
/// 
/// 使用方式:
/// ```dart
/// final confirmed = await showBlockConfirmDialog(context);
/// if (confirmed) {
///   // 执行拉黑操作
/// }
/// ```
Future<bool> showBlockConfirmDialog(final BuildContext context) async {
  final result = await showDialog<bool>(
    context: context,
    builder: (final context) => AlertDialog(
      title: const Text("确认拉黑"),
      content: const Text(
        "确定拉黑该用户吗？拉黑后对方将无法给您发送消息",
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(false),
          child: const Text("取消"),
        ),
        TextButton(
          onPressed: () => Navigator.of(context).pop(true),
          child: const Text(
            "确认拉黑",
            style: TextStyle(color: Colors.red),
          ),
        ),
      ],
    ),
  );
  return result ?? false;
}