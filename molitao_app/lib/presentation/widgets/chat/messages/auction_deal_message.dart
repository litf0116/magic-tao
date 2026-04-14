import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 成交通知消息组件（发给中标用户）
/// - 边框: 2px solid #22c55e
/// - 右上角标签: "交易通知"
/// - 显示: 恭喜文字、商品信息框、成交价、成交时间、提示文字
class AuctionDealMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const AuctionDealMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    String dealUserName = '';
    double finalPrice = 0.0;
    String name = '';
    String dealTime = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        dealUserName = payload['dealUserName'] ?? '';
        finalPrice = (payload['finalPrice'] ?? 0).toDouble();
        name = payload['name'] ?? '';
        dealTime = payload['dealTime'] ?? '';
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        clipBehavior: Clip.none,
        decoration: BoxDecoration(
          border: Border.all(color: const Color(0xff22C55E), width: 2),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Stack(
          clipBehavior: Clip.none,
          children: [
            // 右上角标签
            Positioned(
              top: -2,
              right: -2,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: const BoxDecoration(
                  color: Color(0xff22C55E),
                  borderRadius: BorderRadius.only(
                    topRight: Radius.circular(8),
                    bottomLeft: Radius.circular(8),
                  ),
                ),
                child: const Text(
                  '交易通知',
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
                  // 恭喜文字
                  Container(
                    padding: const EdgeInsets.all(8),
                    child: RichText(
                      text: TextSpan(
                        style: const TextStyle(
                          color: Color(0xff16A34A),
                          fontSize: 14,
                        ),
                        children: [
                          const TextSpan(text: '恭喜 '),
                          TextSpan(
                            text: dealUserName,
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                          const TextSpan(text: ' 最终以 '),
                          TextSpan(
                            text: '￥${finalPrice.toStringAsFixed(2)}',
                            style: const TextStyle(
                              fontSize: 18,
                              color: Color(0xffEF4444),
                            ),
                          ),
                          const TextSpan(text: ' 秒得商品'),
                        ],
                      ),
                    ),
                  ),
                  // 商品信息框
                  Container(
                    padding: const EdgeInsets.all(16),
                    margin: const EdgeInsets.symmetric(vertical: 8),
                    decoration: BoxDecoration(
                      border: Border.all(
                        color: const Color(0xff22C55E),
                        width: 2.5,
                      ),
                      borderRadius: BorderRadius.circular(12),
                      color: const Color(0xffBBF7D0),
                      boxShadow: [
                        BoxShadow(
                          color: const Color(0x3322C55E), // 20% opacity
                          blurRadius: 8,
                          offset: const Offset(0, 2),
                        ),
                      ],
                    ),
                    child: Text(
                      name,
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  // 成交价
                  Padding(
                    padding: const EdgeInsets.only(left: 12),
                    child: Text(
                      '成交价: ￥${finalPrice.toStringAsFixed(2)}',
                      style: const TextStyle(
                        fontSize: 14,
                        color: Colors.black87,
                      ),
                    ),
                  ),
                  // 成交时间
                  Padding(
                    padding: const EdgeInsets.only(left: 12, top: 4),
                    child: Text(
                      '成交时间: $dealTime',
                      style: const TextStyle(fontSize: 12, color: Colors.grey),
                    ),
                  ),
                  const SizedBox(height: 8),
                  // 提示文字
                  const Text(
                    '双方私聊秒杀主持确认交易!\n认准星标小心冒充\n有请下一件拍品',
                    style: TextStyle(fontSize: 12, color: Colors.grey),
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
