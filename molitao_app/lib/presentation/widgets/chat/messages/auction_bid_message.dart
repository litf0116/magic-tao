import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 出价消息组件
/// - 边框: 2px solid #ff7144
/// - 背景: #ffb673
/// - 右上角标签: "出价"
/// - 显示: 商品名称、当前出价（大号白色字体）
class AuctionBidMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const AuctionBidMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    String name = '';
    double currentPrice = 0.0;

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        name = payload['name'] ?? '';
        currentPrice = (payload['currentPrice'] ?? 0).toDouble();
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          border: Border.all(color: const Color(0xffFF7144), width: 2),
          color: const Color(0xffFFB673),
          borderRadius: BorderRadius.circular(12),
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
                  color: Color(0xffFF7144),
                  borderRadius: BorderRadius.only(
                    topRight: Radius.circular(10),
                    bottomLeft: Radius.circular(8),
                  ),
                ),
                child: const Text(
                  '出价',
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
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '商品名称: $name',
                    style: const TextStyle(fontSize: 14, color: Colors.black87),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '当前出价：￥${currentPrice.toStringAsFixed(2)}',
                    style: const TextStyle(
                      fontSize: 24,
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                    ),
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
