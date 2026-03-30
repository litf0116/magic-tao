import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 系统消息组件
/// - 居中显示
/// - 灰色文字，12px
/// - 用于 BanUser、Backout 等系统通知
class SystemMessage extends StatelessWidget {
  final ChatMessage message;

  const SystemMessage({Key? key, required this.message}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    String msg = message.msg ?? '';

    // 根据消息类型生成不同的系统消息文本
    String displayMsg = msg;
    switch (message.type) {
      case ChatMessageType.banUser:
        displayMsg = msg.isNotEmpty ? msg : '用户已被禁言';
        break;
      case ChatMessageType.backout:
        displayMsg = msg.isNotEmpty ? msg : '用户已退出';
        break;
      default:
        displayMsg = msg;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Center(
        child: Text(
          displayMsg,
          style: const TextStyle(fontSize: 12, color: Colors.grey),
        ),
      ),
    );
  }
}
