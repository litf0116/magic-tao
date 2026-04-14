import 'package:flutter/material.dart';
import '../../../data/models/chat_message_model.dart';
import 'message_bubble.dart';

/// 文本消息组件
///
/// 显示文本内容，支持：
/// - 自动换行
/// - 长按复制
/// - URL 链接识别（待实现）
class TextMessage extends StatelessWidget {
  final ChatMessage message;
  final bool isSelf;
  final VoidCallback? onTap;
  final VoidCallback? onLongPress;

  const TextMessage({
    super.key,
    required this.message,
    this.isSelf = false,
    this.onTap,
    this.onLongPress,
  });

  @override
  Widget build(BuildContext context) {
    final textColor = isSelf ? Colors.white : Colors.black87;

    return MessageBubble(
      position: isSelf ? MessagePosition.right : MessagePosition.left,
      onTap: onTap,
      onLongPress: onLongPress,
      child: SelectableText(
        message.msg ?? '',
        style: TextStyle(fontSize: 15, color: textColor, height: 1.4),
        // 可选：支持选择文本
        onTap: onTap,
        contextMenuBuilder: (context, editableTextState) {
          // 长按显示复制菜单
          return AdaptiveTextSelectionToolbar.editableText(
            editableTextState: editableTextState,
          );
        },
      ),
    );
  }
}

/// 带头像的文本消息
class TextMessageWithAvatar extends StatelessWidget {
  final ChatMessage message;
  final bool isSelf;
  final VoidCallback? onAvatarTap;
  final VoidCallback? onMessageTap;
  final VoidCallback? onMessageLongPress;

  const TextMessageWithAvatar({
    super.key,
    required this.message,
    this.isSelf = false,
    this.onAvatarTap,
    this.onMessageTap,
    this.onMessageLongPress,
  });

  @override
  Widget build(BuildContext context) {
    return MessageWithAvatar(
      avatarUrl: _getAvatarUrl(),
      userName: message.fromName,
      isSelf: isSelf,
      onAvatarTap: onAvatarTap,
      onMessageTap: onMessageTap,
      onMessageLongPress: onMessageLongPress,
      child: TextMessage(
        message: message,
        isSelf: isSelf,
        onTap: onMessageTap,
        onLongPress: onMessageLongPress,
      ),
    );
  }

  String? _getAvatarUrl() {
    if (message.avatar == null) return null;
    final avatar = message.avatar!;
    if (avatar.startsWith('http')) return avatar;
    return 'https://image.molitao.top/$avatar';
  }
}
