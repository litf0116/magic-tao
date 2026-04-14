import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../data/models/chat_message_model.dart';
import 'message_bubble.dart';

/// 图片消息组件
///
/// 显示图片内容，支持：
/// - 自适应尺寸
/// - 点击全屏预览
/// - 加载占位符
/// - 错误处理
class ImageMessage extends StatelessWidget {
  final ChatMessage message;
  final bool isSelf;
  final VoidCallback? onTap;
  final VoidCallback? onLongPress;
  final Function(String)? onImageTap;

  // 图片最大尺寸限制
  static const double maxWidth = 200;
  static const double maxHeight = 200;
  static const double minWidth = 100;
  static const double minHeight = 100;

  const ImageMessage({
    super.key,
    required this.message,
    this.isSelf = false,
    this.onTap,
    this.onLongPress,
    this.onImageTap,
  });

  @override
  Widget build(BuildContext context) {
    final imageUrl = _getImageUrl();
    final imageSize = _getImageSize();

    return GestureDetector(
      onTap: () {
        if (onImageTap != null && imageUrl != null) {
          onImageTap!(imageUrl);
        }
        onTap?.call();
      },
      onLongPress: onLongPress,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(8),
        child: Container(
          constraints: BoxConstraints(
            maxWidth: maxWidth,
            maxHeight: maxHeight,
            minWidth: minWidth,
            minHeight: minHeight,
          ),
          width: imageSize.width,
          height: imageSize.height,
          child: imageUrl != null
              ? CachedNetworkImage(
                  imageUrl: imageUrl,
                  fit: BoxFit.cover,
                  placeholder: (context, url) => Container(
                    color: Colors.grey.shade200,
                    child: const Center(
                      child: SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                    ),
                  ),
                  errorWidget: (context, url, error) => Container(
                    color: Colors.grey.shade200,
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.broken_image,
                          color: Colors.grey.shade400,
                          size: 32,
                        ),
                        const SizedBox(height: 4),
                        Text(
                          '加载失败',
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey.shade500,
                          ),
                        ),
                      ],
                    ),
                  ),
                )
              : Container(
                  color: Colors.grey.shade200,
                  child: Icon(
                    Icons.image,
                    color: Colors.grey.shade400,
                    size: 40,
                  ),
                ),
        ),
      ),
    );
  }

  /// 获取图片 URL
  String? _getImageUrl() {
    try {
      final payload = _parsePayload();
      if (payload == null) {
        print('[ImageMessage] _getImageUrl: payload is null');
        return null;
      }

      String? url = payload['url'] as String?;
      if (url == null) {
        print('[ImageMessage] _getImageUrl: url is null, payload=$payload');
        return null;
      }

      // 清理 URL
      url = url.trim();
      print('[ImageMessage] _getImageUrl: original url=$url');

      // 处理 file:// 协议（无效的本地文件协议）
      if (url.startsWith('file://')) {
        // 提取路径部分，假设是相对路径
        url = url.replaceFirst('file://', '');
        // 如果不是以 / 开头，添加 /
        if (!url.startsWith('/')) {
          url = '/$url';
        }
      }

      // 处理绝对路径（以 / 开头）
      if (url.startsWith('/')) {
        url = 'https://image.molitao.top$url';
      } else if (!url.startsWith('http://') && !url.startsWith('https://')) {
        // 处理相对路径
        url = 'https://image.molitao.top/$url';
      }

      // 添加缩略图参数
      final result = '$url!w300';
      print('[ImageMessage] _getImageUrl: final url=$result');
      return result;
    } catch (e) {
      print('[ImageMessage] _getImageUrl: exception=$e');
      return null;
    }
  }

  /// 获取原始图片 URL（用于预览）
  String? getOriginalImageUrl() {
    try {
      final payload = _parsePayload();
      if (payload == null) return null;

      String? url = payload['url'] as String?;
      if (url == null) return null;

      // 清理 URL
      url = url.trim();

      // 处理 file:// 协议（无效的本地文件协议）
      if (url.startsWith('file://')) {
        // 提取路径部分，假设是相对路径
        url = url.replaceFirst('file://', '');
        // 如果不是以 / 开头，添加 /
        if (!url.startsWith('/')) {
          url = '/$url';
        }
      }

      // 处理绝对路径（以 / 开头）
      if (url.startsWith('/')) {
        url = 'https://image.molitao.top$url';
      } else if (!url.startsWith('http://') && !url.startsWith('https://')) {
        // 处理相对路径
        url = 'https://image.molitao.top/$url';
      }
      return url;
    } catch (e) {
      return null;
    }
  }

  /// 解析 payload
  Map<String, dynamic>? _parsePayload() {
    print(
      '[ImageMessage] _parsePayload: message.payload type=${message.payload.runtimeType}, value=${message.payload}',
    );

    if (message.payload == null) return null;

    if (message.payload is Map<String, dynamic>) {
      print('[ImageMessage] _parsePayload: payload is Map');
      return message.payload as Map<String, dynamic>;
    }

    if (message.payload is String) {
      try {
        print(
          '[ImageMessage] _parsePayload: payload is String, original=${message.payload}',
        );
        // 尝试解析 JSON 字符串
        final decoded = Uri.decodeComponent(message.payload as String);
        print('[ImageMessage] _parsePayload: decoded=$decoded');
        // 简单处理，实际应用中可能需要 json.decode
        return {'url': decoded};
      } catch (e) {
        print('[ImageMessage] _parsePayload: parse error=$e');
        return null;
      }
    }

    print('[ImageMessage] _parsePayload: unknown payload type');
    return null;
  }

  /// 计算图片显示尺寸
  Size _getImageSize() {
    try {
      final payload = _parsePayload();
      if (payload == null) return const Size(minWidth, minHeight);

      final width = (payload['width'] as num?)?.toDouble() ?? 0;
      final height = (payload['height'] as num?)?.toDouble() ?? 0;

      if (width <= 0 || height <= 0) {
        return const Size(minWidth, minHeight);
      }

      // 计算缩放比例
      double displayWidth = width;
      double displayHeight = height;

      // 如果图片太大，按比例缩小
      if (width > maxWidth || height > maxHeight) {
        final widthRatio = maxWidth / width;
        final heightRatio = maxHeight / height;
        final ratio = widthRatio < heightRatio ? widthRatio : heightRatio;
        displayWidth = width * ratio;
        displayHeight = height * ratio;
      }

      // 确保最小尺寸
      if (displayWidth < minWidth) displayWidth = minWidth;
      if (displayHeight < minHeight) displayHeight = minHeight;

      return Size(displayWidth, displayHeight);
    } catch (e) {
      return const Size(minWidth, minHeight);
    }
  }
}

/// 带头像的图片消息
class ImageMessageWithAvatar extends StatelessWidget {
  final ChatMessage message;
  final bool isSelf;
  final VoidCallback? onAvatarTap;
  final VoidCallback? onMessageTap;
  final VoidCallback? onMessageLongPress;
  final Function(String)? onImageTap;

  const ImageMessageWithAvatar({
    super.key,
    required this.message,
    this.isSelf = false,
    this.onAvatarTap,
    this.onMessageTap,
    this.onMessageLongPress,
    this.onImageTap,
  });

  @override
  Widget build(BuildContext context) {
    final imageMessage = ImageMessage(
      message: message,
      isSelf: isSelf,
      onTap: onMessageTap,
      onLongPress: onMessageLongPress,
      onImageTap: onImageTap,
    );

    if (isSelf) {
      return MessageBubble(
        position: MessagePosition.right,
        padding: EdgeInsets.zero,
        onTap: onMessageTap,
        onLongPress: onMessageLongPress,
        child: imageMessage,
      );
    }

    return MessageWithAvatar(
      avatarUrl: _getAvatarUrl(),
      userName: message.fromName,
      isSelf: isSelf,
      onAvatarTap: onAvatarTap,
      child: imageMessage,
    );
  }

  String? _getAvatarUrl() {
    if (message.avatar == null) return null;
    final avatar = message.avatar!;
    if (avatar.startsWith('http')) return avatar;
    return 'https://image.molitao.top/$avatar';
  }
}
