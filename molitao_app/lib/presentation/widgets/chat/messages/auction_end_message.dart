import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 秒杀结束消息组件
/// - 边框: 2px solid #ff9800
/// - 右上角标签: "成功秒杀"
/// - 显示: 中标用户、成交价、商品信息框、成交时间、提示文字
class AuctionEndMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const AuctionEndMessage({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    String status = '';
    String dealUserName = '';
    double finalPrice = 0.0;
    String name = '';
    String dealTime = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        status = payload['status'] ?? '';
        dealUserName = payload['dealUserName'] ?? '';
        finalPrice = (payload['finalPrice'] ?? 0).toDouble();
        name = payload['name'] ?? '';
        dealTime = payload['dealTime'] ?? '';
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          border: Border.all(color: const Color(0xffFF9800), width: 2),
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
                  color: Color(0xffFF9800),
                  borderRadius: BorderRadius.only(
                    topRight: Radius.circular(8),
                    bottomLeft: Radius.circular(8),
                  ),
                ),
                child: const Text(
                  '成功秒杀',
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
                  // 已成交状态
                  if (status == '已成交') ...[
                    Container(
                      padding: const EdgeInsets.all(8),
                      child: RichText(
                        text: TextSpan(
                          style: const TextStyle(
                            color: Colors.red,
                            fontSize: 14,
                          ),
                          children: [
                            const TextSpan(text: '恭喜 '),
                            TextSpan(
                              text: dealUserName,
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            const TextSpan(text: ' 最终以 '),
                            TextSpan(
                              text: '￥${finalPrice.toStringAsFixed(2)}',
                              style: const TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
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
                          color: const Color(0xffFF9800),
                          width: 2.5,
                        ),
                        borderRadius: BorderRadius.circular(12),
                        color: const Color(0xffFFB673),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0x33FF9800), // 20% opacity
                            blurRadius: 8,
                            offset: const Offset(0, 2),
                          ),
                        ],
                      ),
                      child: Text(
                        name,
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    // 成交时间
                    Padding(
                      padding: const EdgeInsets.only(left: 12),
                      child: Text(
                        dealTime,
                        style: const TextStyle(
                          fontSize: 12,
                          color: Colors.grey,
                        ),
                      ),
                    ),
                    const SizedBox(height: 8),
                    // 提示文字
                    const Text(
                      '双方私聊秒杀主持确认交易!\n认准星标小心冒充\n有请下一件拍品',
                      style: TextStyle(fontSize: 12, color: Colors.grey),
                    ),
                  ],
                  // 流拍状态
                  if (status == '上架') ...[
                    const Text(
                      '商品流拍',
                      style: TextStyle(fontSize: 14, color: Colors.black87),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
