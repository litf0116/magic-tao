import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 图片消息组件
/// - 宽高 150x150
/// - 圆角 6px
/// - 点击放大预览
class ImageMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const ImageMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    // 从payload中获取图片URL和其他信息
    String imageUrl = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        imageUrl = payload['url'] ?? '';
        // 可以使用width和height变量，但暂时只使用固定尺寸
        // double width = (payload['width'] ?? 150).toDouble();
        // double height = (payload['height'] ?? 150).toDouble();
      } else if (message.payload is String) {
        // 如果payload是字符串，尝试解析为JSON
        try {
          // 这里我们简单地假设字符串就是URL
          imageUrl = message.payload;
        } catch (e) {
          // 解析失败则忽略
        }
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 150,
        height: 150,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(6),
          image: DecorationImage(
            image: NetworkImage(imageUrl),
            fit: BoxFit.cover,
          ),
        ),
      ),
    );
  }
}
