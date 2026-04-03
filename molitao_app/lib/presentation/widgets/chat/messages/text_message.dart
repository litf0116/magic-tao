import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';
import 'package:molitao_app/utils/emoji_decoder.dart';

/// 文本消息组件
/// - 白色背景，圆角 6px
/// - padding: 8px 12px
/// - 字体 14px，行高 1.5
/// - 支持换行
/// - 支持表情解析
class TextMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const TextMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    final text = message.msg ?? '';

    // 调试输出
    print(
      '[TextMessage] msg=${message.msg}, type=${message.type}, from=${message.from}, fromName=${message.fromName}',
    );

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        constraints: const BoxConstraints(maxWidth: 280),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(6),
        ),
        child: _buildTextWithEmoji(text),
      ),
    );
  }

  /// 构建带表情的文本
  Widget _buildTextWithEmoji(String text) {
    // 检查是否包含表情
    if (!EmojiDecoder.containsEmoji(text)) {
      return Text(text, style: const TextStyle(fontSize: 14, height: 1.5));
    }

    // 解析文本片段
    final segments = EmojiDecoder.parseText(text);

    return RichText(
      text: TextSpan(
        children: segments.map((segment) {
          if (segment.type == TextSegmentType.emoji &&
              segment.emojiUrl != null) {
            // 表情图片
            return WidgetSpan(
              alignment: PlaceholderAlignment.middle,
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 1),
                child: Image.network(
                  segment.emojiUrl!,
                  width: 20,
                  height: 20,
                  fit: BoxFit.contain,
                  errorBuilder: (context, error, stackTrace) {
                    // 加载失败时显示表情代码
                    return Text(
                      segment.content,
                      style: const TextStyle(fontSize: 14),
                    );
                  },
                ),
              ),
            );
          } else {
            // 普通文本
            return TextSpan(
              text: segment.content,
              style: const TextStyle(
                fontSize: 14,
                height: 1.5,
                color: Colors.black,
              ),
            );
          }
        }).toList(),
      ),
    );
  }
}
