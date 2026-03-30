import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 文本消息组件
/// - 白色背景，圆角 6px
/// - padding: 8px 12px
/// - 字体 14px，行高 1.5
/// - 支持换行
class TextMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const TextMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(6),
        ),
        child: Text(
          message.msg ?? '',
          style: const TextStyle(fontSize: 14, height: 1.5),
        ),
      ),
    );
  }
}
