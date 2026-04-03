import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 卡秒状态消息组件
/// - 开启状态: 红色渐变背景，红色边框
/// - 关闭状态: 绿色渐变背景，绿色边框
/// - 显示闪电图标和状态文字
class KasecStatusMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const KasecStatusMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    bool isKasec = false;
    String statusText = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        isKasec = payload['isKasec'] ?? false;
        statusText = message.msg ?? '';
      } else if (message.payload is String) {
        try {
          // 如果payload是字符串，尝试解析为JSON
          // 这里我们简化处理，直接从message.msg获取文本
          statusText = message.msg ?? '';
        } catch (e) {
          statusText = message.msg ?? '';
        }
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        margin: const EdgeInsets.symmetric(vertical: 8),
        decoration: BoxDecoration(
          gradient: isKasec
              ? const LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFFFFF5F5), Color(0xFFFED7D7)],
                )
              : const LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFFF0FFF4), Color(0xFFC6F6D5)],
                ),
          border: Border.all(
            color: isKasec ? const Color(0xFFE53E3E) : const Color(0xFF38A169),
            width: 2,
          ),
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.1),
              spreadRadius: 0,
              blurRadius: 4,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          children: [
            // 闪电图标
            Icon(
              Icons.flash_on,
              size: 24,
              color: isKasec
                  ? const Color(0xFFE53E3E)
                  : const Color(0xFF38A169),
            ),
            const SizedBox(width: 12),
            // 状态文本
            Expanded(
              child: Text(
                statusText,
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: isKasec
                      ? const Color(0xFFC53030)
                      : const Color(0xFF2F855A),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
