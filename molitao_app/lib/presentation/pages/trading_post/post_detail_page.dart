import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/post_provider.dart';
import '../../../data/models/post_model.dart';

/// 帖子详情页
class PostDetailPage extends ConsumerWidget {
  final int postId;

  const PostDetailPage({super.key, required this.postId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(postDetailProvider(postId));

    return Scaffold(
      appBar: AppBar(
        title: const Text('帖子详情'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
        actions: state.post != null
            ? [
                PopupMenuButton<String>(
                  onSelected: (value) =>
                      _handleMenuAction(context, ref, value, state.post!),
                  itemBuilder: (context) => [
                    const PopupMenuItem(
                      value: 'edit',
                      child: ListTile(
                        leading: Icon(Icons.edit),
                        title: Text('修改'),
                      ),
                    ),
                    const PopupMenuItem(
                      value: 'delete',
                      child: ListTile(
                        leading: Icon(Icons.delete, color: Colors.red),
                        title: Text('删除', style: TextStyle(color: Colors.red)),
                      ),
                    ),
                  ],
                ),
              ]
            : null,
      ),
      body: state.isLoading
          ? const Center(child: CircularProgressIndicator())
          : state.error != null
          ? _buildErrorState(state.error!)
          : state.post != null
          ? _buildContent(context, ref, state.post!)
          : const Center(child: Text('帖子不存在')),
    );
  }

  Widget _buildErrorState(String error) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.error_outline, size: 48, color: Colors.grey.shade400),
          const SizedBox(height: 16),
          Text(error, style: TextStyle(color: Colors.grey.shade600)),
        ],
      ),
    );
  }

  Widget _buildContent(BuildContext context, WidgetRef ref, PostModel post) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 标题区域
          _buildTitleSection(post),

          // 联系方式
          _buildContactSection(post),

          // 标签区域
          if (post.categoryName != null && post.categoryName!.isNotEmpty)
            _buildTagsSection(post.categoryName!),

          // 内容区域
          _buildContentSection(context, post),

          // 操作按钮
          _buildActionButtons(context, ref, post),
        ],
      ),
    );
  }

  Widget _buildTitleSection(PostModel post) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            post.title ?? '',
            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              // 头像
              Container(
                width: 32,
                height: 32,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.grey.shade300,
                  image: post.userAvatar != null
                      ? DecorationImage(
                          image: NetworkImage(post.userAvatar!),
                          fit: BoxFit.cover,
                        )
                      : null,
                ),
                child: post.userAvatar == null
                    ? const Icon(Icons.person, size: 20, color: Colors.grey)
                    : null,
              ),
              const SizedBox(width: 8),
              // 用户名
              Text(
                post.userName ?? '匿名',
                style: TextStyle(fontSize: 14, color: Colors.grey.shade700),
              ),
              const SizedBox(width: 16),
              // 时间
              Text(
                _formatTime(post.creationTime),
                style: TextStyle(fontSize: 12, color: Colors.grey.shade500),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildContactSection(PostModel post) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.wechat, size: 18, color: Colors.green.shade600),
              const SizedBox(width: 8),
              Text(
                '微信：${post.wechat ?? '—'}',
                style: const TextStyle(fontSize: 14),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Icon(Icons.chat, size: 18, color: Colors.blue.shade600),
              const SizedBox(width: 8),
              Text(
                'QQ：${post.qq ?? '—'}',
                style: const TextStyle(fontSize: 14),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildTagsSection(String categoryName) {
    final tags = categoryName.split(',').where((t) => t.isNotEmpty).toList();
    final colors = [
      const Color(0xFFFF6B6B),
      const Color(0xFF4ECDC4),
      const Color(0xFF45B7D1),
      const Color(0xFF96CEB4),
      const Color(0xFFFF9800),
      const Color(0xFF9B59B6),
    ];

    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Wrap(
        spacing: 8,
        runSpacing: 8,
        children: tags.asMap().entries.map((entry) {
          final color = colors[entry.key % colors.length];
          return Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              border: Border.all(color: color),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Text(
              entry.value,
              style: TextStyle(color: color, fontSize: 12),
            ),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildContentSection(BuildContext context, PostModel post) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: GestureDetector(
        onTap: () => _showImagePreview(context, post.content ?? ''),
        child: _buildRichText(post.content ?? ''),
      ),
    );
  }

  Widget _buildRichText(String content) {
    // 简化处理：直接显示文本
    // Flutter 没有内置的 HTML 渲染，需要使用 flutter_html 包
    // 这里先做简单处理，提取纯文本显示
    final textContent = _stripHtmlTags(content);
    return Text(textContent, style: const TextStyle(fontSize: 15, height: 1.6));
  }

  String _stripHtmlTags(String html) {
    final regExp = RegExp(r'<[^>]*>');
    return html.replaceAll(regExp, ' ').replaceAll('&nbsp;', ' ');
  }

  Widget _buildActionButtons(
    BuildContext context,
    WidgetRef ref,
    PostModel post,
  ) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(top: 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () => _sendMessage(context, post),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF007AFF),
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
              child: const Text('点击留言'),
            ),
          ),
        ],
      ),
    );
  }

  void _handleMenuAction(
    BuildContext context,
    WidgetRef ref,
    String action,
    PostModel post,
  ) {
    switch (action) {
      case 'edit':
        Navigator.of(
          context,
        ).pushNamed('/trading-post/add', arguments: {'postId': post.postId});
        break;
      case 'delete':
        _showDeleteConfirmDialog(context, ref, post);
        break;
    }
  }

  void _showDeleteConfirmDialog(
    BuildContext context,
    WidgetRef ref,
    PostModel post,
  ) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('确认删除'),
        content: const Text('确定要删除这篇帖子吗？'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () async {
              Navigator.pop(context);
              final success = await ref
                  .read(postDetailProvider(postId).notifier)
                  .deletePost(post.postId ?? postId);
              if (success && context.mounted) {
                Navigator.pop(context);
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('删除成功')));
              }
            },
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('删除'),
          ),
        ],
      ),
    );
  }

  void _sendMessage(BuildContext context, PostModel post) {
    // 跳转到私聊页面
    Navigator.of(context).pushNamed(
      '/chat/private',
      arguments: {
        'friendId': post.lastModifierUserId,
        'friendName': post.userName,
        'friendAvatar': post.userAvatar,
      },
    );
  }

  void _showImagePreview(BuildContext context, String content) {
    // 从 HTML 中提取图片 URL
    final regExp = RegExp(r'<img.+?src="(.+?)".*?>');
    final matches = regExp.allMatches(content);
    final images = matches.map((m) => m.group(1)).whereType<String>().toList();

    if (images.isEmpty) return;

    // 显示图片预览
    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.transparent,
        insetPadding: EdgeInsets.zero,
        child: Stack(
          fit: StackFit.expand,
          children: [
            PageView.builder(
              itemCount: images.length,
              itemBuilder: (context, index) {
                return GestureDetector(
                  onTap: () => Navigator.pop(context),
                  child: InteractiveViewer(
                    child: Image.network(images[index], fit: BoxFit.contain),
                  ),
                );
              },
            ),
            Positioned(
              top: MediaQuery.of(context).padding.top + 8,
              right: 8,
              child: IconButton(
                icon: const Icon(Icons.close, color: Colors.white),
                onPressed: () => Navigator.pop(context),
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _formatTime(DateTime? time) {
    if (time == null) return '';
    final now = DateTime.now();
    final diff = now.difference(time);

    if (diff.inDays > 365) {
      return '${time.year}-${time.month.toString().padLeft(2, '0')}-${time.day.toString().padLeft(2, '0')}';
    } else if (diff.inDays > 0) {
      return '${diff.inDays}天前';
    } else if (diff.inHours > 0) {
      return '${diff.inHours}小时前';
    } else if (diff.inMinutes > 0) {
      return '${diff.inMinutes}分钟前';
    } else {
      return '刚刚';
    }
  }
}
