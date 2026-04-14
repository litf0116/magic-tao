import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 欢迎消息组件
/// - 居中显示
/// - 灰色文字，12px
/// - 显示用户名欢迎信息
class WelcomeMessage extends StatelessWidget {
  final ChatMessage message;

  const WelcomeMessage({Key? key, required this.message}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    String fromName = message.fromName ?? '';
    String msg = message.msg ?? '';

    // 如果消息为空，则构建默认欢迎消息
    String displayMsg = msg.isEmpty ? '$fromName 加入群聊' : msg;

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
