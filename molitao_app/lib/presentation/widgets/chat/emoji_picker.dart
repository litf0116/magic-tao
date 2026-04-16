import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/chat_emoji_store.dart';
import '../../../data/models/chat_emoji_model.dart' as model;
import '../../../utils/emoji_decoder.dart';

/// 表情选择器组件
class EmojiPicker extends ConsumerStatefulWidget {
  /// 表情选中回调（系统表情）
  final void Function(String emojiCode) onEmojiSelected;

  /// 收藏表情选中回调（收藏表情图片）
  final void Function(model.ChatEmojiDto emoji)? onFavoriteEmojiSelected;

  /// 是否显示收藏标签
  final bool showFavoriteTab;

  const EmojiPicker({
    Key? key,
    required this.onEmojiSelected,
    this.onFavoriteEmojiSelected,
    this.showFavoriteTab = false,
  }) : super(key: key);

  @override
  ConsumerState<EmojiPicker> createState() => _EmojiPickerState();
}

class _EmojiPickerState extends ConsumerState<EmojiPicker> {
  int _currentTabIndex = 0; // 0: 系统, 1: 收藏

  @override
  void initState() {
    super.initState();
    // 延迟初始化收藏表情列表
    if (widget.showFavoriteTab) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        ref.read(userEmojiProvider.notifier).ensureInitialized();
      });
    }
  }

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
          Expanded(child: _buildContent()),
        ],
      ),
    );
  }

  Widget _buildTabBar() {
    return Container(
      height: 40,
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: Color(0xFFECECEC), width: 1)),
      ),
      child: Row(
        children: [
          _buildTab('系统', 0),
          if (widget.showFavoriteTab) _buildTab('收藏', 1),
        ],
      ),
    );
  }

  Widget _buildTab(String label, int index) {
    final isActive = _currentTabIndex == index;
    return GestureDetector(
      onTap: () => setState(() => _currentTabIndex = index),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 10),
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
      ),
    );
  }

  Widget _buildContent() {
    if (_currentTabIndex == 0) {
      return _buildSystemEmojiGrid();
    } else {
      return _buildFavoriteEmojiGrid();
    }
  }

  /// 系统表情网格
  Widget _buildSystemEmojiGrid() {
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
          onTap: () => widget.onEmojiSelected(code),
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

  /// 收藏表情网格
  Widget _buildFavoriteEmojiGrid() {
    final userEmojiState = ref.watch(userEmojiProvider);

    if (userEmojiState.isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    final emojis = userEmojiState.userEmoji;

    if (emojis.isEmpty) {
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.favorite_border, size: 48, color: Colors.grey),
            SizedBox(height: 8),
            Text('暂无收藏表情', style: TextStyle(color: Colors.grey, fontSize: 14)),
            SizedBox(height: 4),
            Text(
              '长按聊天图片可收藏',
              style: TextStyle(color: Colors.grey, fontSize: 12),
            ),
          ],
        ),
      );
    }

    return GridView.builder(
      padding: const EdgeInsets.all(8),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 4,
        mainAxisSpacing: 8,
        crossAxisSpacing: 8,
      ),
      itemCount: emojis.length,
      itemBuilder: (context, index) {
        final emoji = emojis[index];
        return _buildFavoriteEmojiItem(emoji);
      },
    );
  }

  Widget _buildFavoriteEmojiItem(model.ChatEmojiDto emoji) {
    final imageUrl = emoji.url ?? '';
    final fullUrl = imageUrl.startsWith('http')
        ? imageUrl
        : 'https://image.molitao.top/$imageUrl';

    return GestureDetector(
      onTap: () {
        if (widget.onFavoriteEmojiSelected != null) {
          widget.onFavoriteEmojiSelected!(emoji);
        }
      },
      onLongPress: () => _showDeleteConfirmDialog(emoji),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.grey.shade50,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: Colors.grey.shade200),
        ),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: CachedNetworkImage(
            imageUrl: fullUrl,
            fit: BoxFit.cover,
            placeholder: (context, url) => Container(
              color: Colors.grey.shade100,
              child: const Center(
                child: SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                ),
              ),
            ),
            errorWidget: (context, url, error) => Container(
              color: Colors.grey.shade100,
              child: const Icon(Icons.broken_image, color: Colors.grey),
            ),
          ),
        ),
      ),
    );
  }

  /// 显示删除确认对话框
  void _showDeleteConfirmDialog(model.ChatEmojiDto emoji) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('提示'),
        content: const Text('确定删除该收藏表情吗？'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              _deleteEmoji(emoji);
            },
            child: const Text('删除', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }

  void _deleteEmoji(model.ChatEmojiDto emoji) {
    if (emoji.id != null) {
      ref.read(userEmojiProvider.notifier).removeEmoji(emoji.id!);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('已删除'), duration: Duration(seconds: 1)),
      );
    }
  }
}
