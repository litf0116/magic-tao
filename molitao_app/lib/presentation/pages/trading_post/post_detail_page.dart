import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_html/flutter_html.dart';
import 'package:url_launcher/url_launcher_string.dart';
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
        title: const Text(
          '帖子详情',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
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
      child: _buildRichText(context, post.content ?? ''),
    );
  }

  Widget _buildRichText(BuildContext context, String content) {
    return Html(
      data: content,
      style: {
        "body": Style(
          fontSize: FontSize(15),
          lineHeight: LineHeight(1.6),
          margin: Margins.zero,
          padding: HtmlPaddings.zero,
        ),
        "p": Style(margin: Margins.only(bottom: 8)),
        "img": Style(width: Width(double.infinity)),
      },
      onLinkTap: (url, attributes, element) {
        if (url != null) {
          _launchUrl(url);
        }
      },
      extensions: [
        TagExtension(
          tagsToExtend: {"img"},
          builder: (extensionContext) {
            final src = extensionContext.attributes['src'];
            if (src == null) return const SizedBox.shrink();

            return GestureDetector(
              onTap: () => _showImagePreview(context, src),
              child: Container(
                margin: const EdgeInsets.symmetric(vertical: 8),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    src,
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) => Container(
                      height: 200,
                      color: Colors.grey[200],
                      child: const Center(
                        child: Icon(Icons.broken_image, color: Colors.grey),
                      ),
                    ),
                  ),
                ),
              ),
            );
          },
        ),
      ],
    );
  }

  void _launchUrl(String url) async {
    try {
      if (await canLaunchUrlString(url)) {
        await launchUrlString(url);
      }
    } catch (e) {
      debugPrint('Failed to launch URL: $e');
    }
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

  void _showImagePreview(BuildContext context, String imageUrl) {
    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.transparent,
        insetPadding: EdgeInsets.zero,
        child: Stack(
          fit: StackFit.expand,
          children: [
            GestureDetector(
              onTap: () => Navigator.pop(context),
              child: InteractiveViewer(
                child: Image.network(imageUrl, fit: BoxFit.contain),
              ),
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
