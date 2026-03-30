import 'package:flutter/material.dart';
import 'package:molitao_app/utils/emoji_decoder.dart';

/// 表情选择器组件
class EmojiPicker extends StatelessWidget {
  /// 表情选中回调
  final void Function(String emojiCode) onEmojiSelected;

  /// 是否显示收藏标签（预留）
  final bool showFavoriteTab;

  const EmojiPicker({
    Key? key,
    required this.onEmojiSelected,
    this.showFavoriteTab = false,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.white,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // 标签栏
          _buildTabBar(),

          // 表情网格
          Expanded(child: _buildEmojiGrid()),
        ],
      ),
    );
  }

  Widget _buildTabBar() {
    return Container(
      height: 36,
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: Color(0xFFECECEC), width: 1)),
      ),
      child: Row(
        children: [
          _buildTab('系统', true),
          if (showFavoriteTab) _buildTab('收藏', false),
        ],
      ),
    );
  }

  Widget _buildTab(String label, bool isActive) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: BoxDecoration(
        border: isActive
            ? const Border(
                bottom: BorderSide(color: Color(0xFFF4835A), width: 2),
              )
            : null,
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 14,
          color: isActive ? const Color(0xFFF4835A) : const Color(0xFF666666),
          fontWeight: isActive ? FontWeight.w500 : FontWeight.normal,
        ),
      ),
    );
  }

  Widget _buildEmojiGrid() {
    final emojiCodes = EmojiDecoder.emojiCodes;

    return GridView.builder(
      padding: const EdgeInsets.all(8),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 8,
        mainAxisSpacing: 8,
        crossAxisSpacing: 8,
      ),
      itemCount: emojiCodes.length,
      itemBuilder: (context, index) {
        final code = emojiCodes[index];
        final url = EmojiDecoder.getEmojiUrl(code);

        return GestureDetector(
          onTap: () => onEmojiSelected(code),
          child: Container(
            decoration: BoxDecoration(
              color: Colors.grey.shade50,
              borderRadius: BorderRadius.circular(4),
            ),
            child: url != null
                ? ClipRRect(
                    borderRadius: BorderRadius.circular(4),
                    child: Image.network(
                      url,
                      width: 28,
                      height: 28,
                      fit: BoxFit.contain,
                      errorBuilder: (context, error, stackTrace) {
                        return Center(
                          child: Text(
                            code.replaceAll('[', '').replaceAll(']', ''),
                            style: const TextStyle(fontSize: 10),
                          ),
                        );
                      },
                    ),
                  )
                : Center(
                    child: Text(code, style: const TextStyle(fontSize: 10)),
                  ),
          ),
        );
      },
    );
  }
}
