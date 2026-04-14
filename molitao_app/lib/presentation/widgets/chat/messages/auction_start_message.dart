import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_html/flutter_html.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';

/// 开始秒杀消息组件
/// 参考 UniApp AuctionStartMessage.vue 实现
/// - 边框: 2px solid #ef4444
/// - 背景: #fff5f5
/// - 右上角标签: "开始秒杀"
/// - 显示: 商品名称、描述（HTML）
class AuctionStartMessage extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const AuctionStartMessage({super.key, required this.message, this.onTap});

  @override
  Widget build(BuildContext context) {
    String name = '';
    String description = '';

    if (message.payload != null) {
      if (message.payload is Map<String, dynamic>) {
        final payload = message.payload as Map<String, dynamic>;
        name = payload['name'] ?? '';
        description = payload['description'] ?? '';
      }
    }

    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.symmetric(vertical: 8),
        decoration: BoxDecoration(
          border: Border.all(color: const Color(0xffEF4444), width: 2),
          color: const Color(0xffFFF5F5),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Stack(
          clipBehavior: Clip.none,
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
                    topRight: Radius.circular(6),
                    bottomLeft: Radius.circular(8),
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
            Container(
              constraints: const BoxConstraints(minWidth: 200, maxWidth: 350),
              padding: const EdgeInsets.fromLTRB(16, 28, 16, 12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '商品名称: $name',
                    style: const TextStyle(
                      fontSize: 14,
                      color: Colors.black87,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                  const SizedBox(height: 8),
                  // 直接渲染 description HTML（包含图片和文本）
                  Html(
                    data: description,
                    style: {
                      "body": Style(
                        margin: Margins.zero,
                        padding: HtmlPaddings.zero,
                      ),
                      "img": Style(width: Width(double.infinity)),
                      "span": Style(
                        fontSize: FontSize(14),
                        color: Colors.black87,
                      ),
                      "div": Style(
                        fontSize: FontSize(14),
                        color: Colors.black87,
                      ),
                    },
                    extensions: [
                      TagExtension(
                        tagsToExtend: {"img"},
                        builder: (extensionContext) {
                          // 优先使用 data-url，其次使用 src
                          final dataUrl =
                              extensionContext.attributes['data-url'];
                          final src = extensionContext.attributes['src'];
                          String? imageUrl = dataUrl ?? src;

                          if (imageUrl == null) return const SizedBox.shrink();

                          // 清理 URL
                          imageUrl = imageUrl.trim();

                          // 处理 file:// 协议（无效的本地文件协议）
                          if (imageUrl.startsWith('file://')) {
                            // 提取路径部分，假设是相对路径
                            imageUrl = imageUrl.replaceFirst('file://', '');
                            if (!imageUrl.startsWith('/')) {
                              imageUrl = '/$imageUrl';
                            }
                          }

                          // 处理绝对路径（以 / 开头）
                          if (imageUrl.startsWith('/')) {
                            imageUrl = 'https://image.molitao.top$imageUrl';
                          } else if (!imageUrl.startsWith('http://') &&
                              !imageUrl.startsWith('https://')) {
                            // 处理相对路径
                            imageUrl = 'https://image.molitao.top/$imageUrl';
                          }

                          return ClipRRect(
                            borderRadius: BorderRadius.circular(4),
                            child: CachedNetworkImage(
                              imageUrl: imageUrl,
                              width: double.infinity,
                              fit: BoxFit.cover,
                              placeholder: (_, __) => Container(
                                height: 150,
                                color: Colors.grey.shade200,
                                child: const Center(
                                  child: SizedBox(
                                    width: 24,
                                    height: 24,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                    ),
                                  ),
                                ),
                              ),
                              errorWidget: (_, __, ___) =>
                                  const SizedBox.shrink(),
                            ),
                          );
                        },
                      ),
                    ],
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
