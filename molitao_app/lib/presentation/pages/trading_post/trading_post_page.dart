import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_html/flutter_html.dart';
import 'package:molitao_app/presentation/providers/trading_post_provider.dart';
import 'package:molitao_app/data/models/post_model.dart';
import 'package:go_router/go_router.dart';

class TradingPostPage extends ConsumerStatefulWidget {
  const TradingPostPage({super.key});

  @override
  ConsumerState<TradingPostPage> createState() => _TradingPostPageState();
}

class _TradingPostPageState extends ConsumerState<TradingPostPage> {
  bool _initialized = false;

  @override
  void initState() {
    super.initState();
    // Initialize data only once when the widget is first created
    Future.microtask(() {
      ref.read(tradingPostProvider.notifier).initialize();
    });
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(tradingPostProvider, (previous, current) {
      if (current.errorMessage != null) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(current.errorMessage!)));
      }
    });

    final state = ref.watch(tradingPostProvider);
    final notifier = ref.read(tradingPostProvider.notifier);

    debugPrint('[TradingPostPage] Building page - posts: ${state.posts.length}, isLoading: ${state.isLoading}, error: ${state.errorMessage}');

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '交易站',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a), // Primary color #f4835a
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: () => notifier.refresh(),
        child: Stack(
          children: [
            CustomScrollView(
              slivers: [
                // Category filter tabs (horizontal scroll)
                SliverToBoxAdapter(
                  child: Container(
                    height: 50,
                    margin: const EdgeInsets.symmetric(vertical: 8),
                    child: ListView.builder(
                      scrollDirection: Axis.horizontal,
                      itemCount:
                          state.categories.length + 1, // +1 for "全部" (All)
                      itemBuilder: (context, index) {
                        if (index == 0) {
                          return _buildCategoryChip(
                            context,
                            '全部',
                            state.selectedCategoryId == null,
                            () => notifier.selectCategory(null),
                          );
                        }
                        final category = state.categories[index - 1];
                        return _buildCategoryChip(
                          context,
                          category.title ?? '',
                          state.selectedCategoryId == category.categoryId,
                          () => notifier.selectCategory(category.categoryId),
                        );
                      },
                    ),
                  ),
                ),

                // Notice bar
                if (state.bulletin != null)
                  SliverToBoxAdapter(
                    child: Container(
                      margin: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      padding: const EdgeInsets.all(12),
                      constraints: const BoxConstraints(maxHeight: 100), // 限制最大高度
                      decoration: BoxDecoration(
                        color: Colors.orange[50],
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.orange.shade200),
                      ),
                      child: Row(
                        children: [
                          Image.asset(
                            'assets/images/notice.png',
                            width: 20,
                            height: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: SingleChildScrollView( // 添加滚动支持
                              child: Html(
                                data: state.bulletin!.content ?? '',
                                style: {
                                  'body': Style(
                                    fontSize: FontSize(14),
                                    color: Colors.orange,
                                    margin: Margins.zero,
                                    padding: HtmlPaddings.zero,
                                  ),
                                  'p': Style(
                                    margin: Margins.zero,
                                    padding: HtmlPaddings.zero,
                                  ),
                                },
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                // Search box and hot words
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 8,
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Search input
                        Container(
                          decoration: BoxDecoration(
                            color: Colors.grey[100],
                            borderRadius: BorderRadius.circular(24),
                            border: Border.all(color: Colors.grey.shade300),
                          ),
                          child: Row(
                            children: [
                              const Padding(
                                padding: EdgeInsets.symmetric(horizontal: 12),
                                child: Icon(Icons.search, color: Colors.grey),
                              ),
                              Expanded(
                                child: TextField(
                                  controller: TextEditingController(
                                    text: state.searchKeywords,
                                  ),
                                  onChanged: (value) {
                                    notifier.setSearchKeywords(value);
                                  },
                                  onSubmitted: (_) {
                                    notifier.searchPosts();
                                  },
                                  decoration: const InputDecoration(
                                    hintText: '请输入关键词',
                                    border: InputBorder.none,
                                    contentPadding: EdgeInsets.symmetric(
                                      vertical: 12,
                                    ),
                                  ),
                                ),
                              ),
                              if (state.searchKeywords.isNotEmpty)
                                IconButton(
                                  icon: const Icon(Icons.clear, size: 18),
                                  onPressed: () {
                                    notifier.setSearchKeywords('');
                                    notifier.searchPosts();
                                  },
                                ),
                            ],
                          ),
                        ),

                        // Hot words chips
                        if (state.hotWords.isNotEmpty)
                          Padding(
                            padding: const EdgeInsets.only(top: 12),
                            child: Wrap(
                              spacing: 8,
                              runSpacing: 8,
                              children: state.hotWords
                                  .map(
                                    (word) => GestureDetector(
                                      onTap: () =>
                                          notifier.switchToHotWord(word),
                                      child: Container(
                                        padding: const EdgeInsets.symmetric(
                                          horizontal: 8,
                                          vertical: 4,
                                        ),
                                        decoration: BoxDecoration(
                                          color:
                                              state.selectedHotWordId == word.id
                                              ? const Color(0xff007aff)
                                              : const Color(0xffe2e2e2),
                                          borderRadius: BorderRadius.circular(
                                            15,
                                          ),
                                        ),
                                        child: Text(
                                          word.title ?? '',
                                          style: TextStyle(
                                            fontSize: 12,
                                            color:
                                                state.selectedHotWordId ==
                                                    word.id
                                                ? Colors.white
                                                : const Color(0xff666666),
                                          ),
                                        ),
                                      ),
                                    ),
                                  )
                                  .toList(),
                            ),
                          ),
                      ],
                    ),
                  ),
                ),

                // Pinned posts section
                if (state.pinnedPosts.isNotEmpty)
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            '置顶帖子',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 8),
                          ...state.pinnedPosts.map(
                            (post) => _buildPostItem(post, context),
                          ),
                        ],
                      ),
                    ),
                  ),

                // "Post" button
                SliverToBoxAdapter(
                  child: Container(
                    margin: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 16,
                    ),
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () {
                        context.push('/trading-post/add');
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(
                          0xff007aff,
                        ), // Blue color like UniApp
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: const Text(
                        '我要发帖',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                    ),
                  ),
                ),

                // Normal posts
                SliverList(
                  delegate: SliverChildBuilderDelegate(
                    (context, index) {
                      if (index >= state.posts.length) {
                        // Show loading indicator at the end if loading more
                        if (state.isLoadingMore) {
                          return const Padding(
                            padding: EdgeInsets.all(16),
                            child: Center(child: CircularProgressIndicator()),
                          );
                        }
                        return Container(); // No more items
                      }

                      return _buildPostItem(state.posts[index], context);
                    },
                    childCount: state.hasMore && state.isLoadingMore
                        ? state.posts.length +
                              1 // +1 for loading indicator
                        : state.posts.length,
                  ),
                ),

                // Load more indicator
                if (state.isLoadingMore && !state.hasMore)
                  const SliverToBoxAdapter(
                    child: Padding(
                      padding: EdgeInsets.all(16),
                      child: Center(child: Text('没有更多数据')),
                    ),
                  ),

                // Bottom spacer to prevent content being hidden by tabbar
                const SliverToBoxAdapter(child: SizedBox(height: 80)),
              ],
            ),

            // Loading overlay
            if (state.isLoading)
              Container(
                color: Colors.white.withValues(alpha: 0.7),
                child: const Center(child: CircularProgressIndicator()),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildCategoryChip(
    BuildContext context,
    String label,
    bool isSelected,
    VoidCallback onTap,
  ) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 15, vertical: 6),
        margin: const EdgeInsets.symmetric(horizontal: 5),
        decoration: BoxDecoration(
          color: isSelected ? const Color(0xff007aff) : const Color(0xfff5f5f5),
          borderRadius: BorderRadius.circular(15),
        ),
        child: Center(
          child: Text(
            label,
            style: TextStyle(
              color: isSelected ? Colors.white : const Color(0xff666666),
              fontSize: 14,
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildPostItem(PostModel post, BuildContext context) {
    return InkWell(
      onTap: () =>
          context.push('/trading-post/detail/${post.postId ?? post.id}'),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: const BoxDecoration(
          border: Border(
            bottom: BorderSide(color: Color(0xffdadada), width: 0.5),
          ),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            // Left side: title and meta
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Post title
                  Text(
                    post.title ?? '',
                    style: const TextStyle(
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                      color: Color(0xff333333),
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 6),
                  // Post meta info
                  Row(
                    children: [
                      if (post.categoryName != null)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 6,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: const Color(0xffe6f7ff),
                            borderRadius: BorderRadius.circular(3),
                          ),
                          child: Text(
                            post.categoryName!,
                            style: const TextStyle(
                              fontSize: 12,
                              color: Color(0xff1890ff),
                            ),
                          ),
                        ),
                      const SizedBox(width: 8),
                      if (post.userName != null)
                        Expanded(
                          child: Text(
                            post.userName!,
                            style: const TextStyle(
                              fontSize: 12,
                              color: Color(0xff999999),
                            ),
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      if (post.creationTime != null)
                        Text(
                          _formatDateTime(post.creationTime!),
                          style: const TextStyle(
                            fontSize: 12,
                            color: Color(0xff999999),
                          ),
                        ),
                    ],
                  ),
                ],
              ),
            ),
            // Right side: avatar
            if (post.userAvatar != null)
              Padding(
                padding: const EdgeInsets.only(left: 12),
                child: ClipOval(
                  child: Image.network(
                    post.userAvatar!,
                    width: 30,
                    height: 30,
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) => Container(
                      width: 30,
                      height: 30,
                      color: Colors.grey[300],
                      child: const Icon(
                        Icons.person,
                        size: 18,
                        color: Colors.grey,
                      ),
                    ),
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }

  String _formatDateTime(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inDays > 0) {
      return '${difference.inDays}天前';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}小时前';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}分钟前';
    } else {
      return '刚刚';
    }
  }
}
