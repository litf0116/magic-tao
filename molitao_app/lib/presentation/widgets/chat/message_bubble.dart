import 'package:flutter/material.dart';

/// 消息位置枚举
enum MessagePosition { left, right, center }

/// 消息气泡基础组件
///
/// 用于包裹各类消息内容，提供统一的气泡样式
/// - 左侧：对方消息，白色背景
/// - 右侧：自己消息，橙色背景
/// - 居中：系统消息，灰色背景
class MessageBubble extends StatelessWidget {
  final Widget child;
  final MessagePosition position;
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final VoidCallback? onTap;
  final VoidCallback? onLongPress;

  const MessageBubble({
    super.key,
    required this.child,
    required this.position,
    this.padding,
    this.margin,
    this.onTap,
    this.onLongPress,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: margin ?? const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: Align(
        alignment: _getAlignment(),
        child: GestureDetector(
          onTap: onTap,
          onLongPress: onLongPress,
          child: Container(
            constraints: BoxConstraints(
              maxWidth: MediaQuery.of(context).size.width * 0.75,
            ),
            padding:
                padding ??
                const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: BoxDecoration(
              color: _getBackgroundColor(),
              borderRadius: _getBorderRadius(),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.05),
                  blurRadius: 4,
                  offset: const Offset(0, 1),
                ),
              ],
            ),
            child: child,
          ),
        ),
      ),
    );
  }

  Alignment _getAlignment() {
    switch (position) {
      case MessagePosition.left:
        return Alignment.centerLeft;
      case MessagePosition.right:
        return Alignment.centerRight;
      case MessagePosition.center:
        return Alignment.center;
    }
  }

  Color _getBackgroundColor() {
    switch (position) {
      case MessagePosition.left:
        return Colors.white;
      case MessagePosition.right:
        return const Color(0xFFf4835a); // 主题橙色
      case MessagePosition.center:
        return Colors.grey.shade200;
    }
  }

  BorderRadius _getBorderRadius() {
    switch (position) {
      case MessagePosition.left:
        return const BorderRadius.only(
          topLeft: Radius.circular(4),
          topRight: Radius.circular(12),
          bottomLeft: Radius.circular(12),
          bottomRight: Radius.circular(12),
        );
      case MessagePosition.right:
        return const BorderRadius.only(
          topLeft: Radius.circular(12),
          topRight: Radius.circular(4),
          bottomLeft: Radius.circular(12),
          bottomRight: Radius.circular(12),
        );
      case MessagePosition.center:
        return BorderRadius.circular(8);
    }
  }
}

/// 带头像的消息容器
///
/// 包含头像、用户名、消息内容的完整消息布局
class MessageWithAvatar extends StatelessWidget {
  final Widget child;
  final String? avatarUrl;
  final String? userName;
  final bool isSelf;
  final VoidCallback? onAvatarTap;
  final VoidCallback? onMessageTap;
  final VoidCallback? onMessageLongPress;
  final Widget? trailing;

  const MessageWithAvatar({
    super.key,
    required this.child,
    this.avatarUrl,
    this.userName,
    this.isSelf = false,
    this.onAvatarTap,
    this.onMessageTap,
    this.onMessageLongPress,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    if (isSelf) {
      // 自己的消息：靠右，不显示头像和用户名
      return MessageBubble(
        position: MessagePosition.right,
        onTap: onMessageTap,
        onLongPress: onMessageLongPress,
        child: child,
      );
    }

    // 对方的消息：靠左，显示头像和用户名
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 头像
          GestureDetector(onTap: onAvatarTap, child: _buildAvatar()),
          const SizedBox(width: 8),
          // 消息内容
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // 用户名
                if (userName != null)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 4),
                    child: Text(
                      userName!,
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.grey.shade600,
                      ),
                    ),
                  ),
                // 消息气泡
                MessageBubble(
                  position: MessagePosition.left,
                  onTap: onMessageTap,
                  onLongPress: onMessageLongPress,
                  child: child,
                ),
              ],
            ),
          ),
          // 尾部组件（如时间、状态等）
          if (trailing != null) trailing!,
        ],
      ),
    );
  }

  Widget _buildAvatar() {
    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: Colors.grey.shade300,
        image: avatarUrl != null
            ? DecorationImage(
                image: NetworkImage(avatarUrl!),
                fit: BoxFit.cover,
              )
            : null,
      ),
      child: avatarUrl == null
          ? Icon(Icons.person, size: 20, color: Colors.grey.shade500)
          : null,
    );
  }
}

/// 系统消息组件
class SystemMessageBubble extends StatelessWidget {
  final String text;

  const SystemMessageBubble({super.key, required this.text});

  @override
  Widget build(BuildContext context) {
    return MessageBubble(
      position: MessagePosition.center,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      child: Text(
        text,
        style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
        textAlign: TextAlign.center,
      ),
    );
  }
}
