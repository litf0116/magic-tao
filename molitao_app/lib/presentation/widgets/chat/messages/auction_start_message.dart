import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 开始秒杀消息组件
/// - 边框: 2px solid #ef4444
/// - 背景: #fff5f5
/// - 右上角标签: "开始秒杀"
/// - 显示: 商品名称、起拍价、描述
class AuctionStartMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const AuctionStartMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    String name = '';
    double startPrice = 0.0;
    String description = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        name = payload['name'] ?? '';
        startPrice = (payload['startPrice'] ?? 0).toDouble();
        description = payload['description'] ?? '';
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          border: Border.all(color: const Color(0xffEF4444), width: 2),
          color: const Color(0xffFFF5F5),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Stack(
          children: [
            // 右上角标签
            Positioned(
              top: 0,
              right: 0,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: const BoxDecoration(
                  color: Color(0xffEF4444),
                  borderRadius: BorderRadius.only(
                    topLeft: Radius.circular(4),
                    bottomLeft: Radius.circular(4),
                  ),
                ),
                child: const Text(
                  '开始秒杀',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
            // 内容区域
            Padding(
              padding: const EdgeInsets.only(top: 20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '商品名称: $name',
                    style: const TextStyle(fontSize: 14, color: Colors.black87),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '起拍价: ￥${startPrice.toStringAsFixed(2)}',
                    style: const TextStyle(fontSize: 14, color: Colors.black87),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    description,
                    style: const TextStyle(fontSize: 14, color: Colors.black87),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
