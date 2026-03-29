import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:molitao_app/presentation/providers/trading_post_provider.dart';
import 'package:molitao_app/data/models/post_model.dart';
import 'package:molitao_app/data/models/announce_model.dart';
import 'package:go_router/go_router.dart';

class TradingPostPage extends ConsumerWidget {
  const TradingPostPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref.listen(tradingPostProvider, (previous, current) {
      if (current.errorMessage != null) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(current.errorMessage!)));
      }
    });

    final state = ref.watch(tradingPostProvider);
    final notifier = ref.read(tradingPostProvider.notifier);

    // Initialize data when the widget is first built
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (state.categories.isEmpty && !state.isLoading) {
        notifier.initialize();
      }
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('交易站'),
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
                          state.selectedCategoryId == category.id,
                          () => notifier.selectCategory(category.id),
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
                            child: Text(
                              state.bulletin!.content ?? '',
                              style: const TextStyle(
                                fontSize: 14,
                                color: Colors.orange,
                              ),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
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
                                          horizontal: 12,
                                          vertical: 6,
                                        ),
                                        decoration: BoxDecoration(
                                          color: Colors.grey[200],
                                          borderRadius: BorderRadius.circular(
                                            16,
                                          ),
                                        ),
                                        child: Text(
                                          word.title ?? '',
                                          style: const TextStyle(
                                            fontSize: 12,
                                            color: Colors.black87,
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
                          ...state.pinnedPosts
                              .map((post) => _buildPostItem(post, context))
                              .toList(),
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
                          0xfff4835a,
                        ), // Primary color
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
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
              ],
            ),

            // Loading overlay
            if (state.isLoading)
              Container(
                color: Colors.white.withOpacity(0.7),
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
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 4),
      child: ChoiceChip(
        label: Text(label),
        selected: isSelected,
        selectedColor: const Color(0xfff4835a), // Primary color
        backgroundColor: Colors.grey[200],
        onSelected: (_) => onTap(),
        labelStyle: TextStyle(
          color: isSelected ? Colors.white : Colors.black87,
          fontSize: 14,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
          side: BorderSide(
            color: isSelected ? const Color(0xfff4835a) : Colors.transparent,
          ),
        ),
      ),
    );
  }

  Widget _buildPostItem(PostModel post, BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 3,
            offset: const Offset(0, 1),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Post title
          Text(
            post.title ?? '',
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),

          const SizedBox(height: 8),

          // Post meta info
          Row(
            children: [
              if (post.categoryName != null)
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 8,
                    vertical: 4,
                  ),
                  decoration: BoxDecoration(
                    color: const Color(0xfff4835a).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(4),
                    border: Border.all(
                      color: const Color(0xfff4835a).withOpacity(0.3),
                    ),
                  ),
                  child: Text(
                    post.categoryName!,
                    style: const TextStyle(
                      fontSize: 12,
                      color: Color(0xfff4835a),
                    ),
                  ),
                ),

              const Spacer(),

              if (post.userName != null)
                Text(
                  post.userName!,
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),

              const SizedBox(width: 8),

              if (post.creationTime != null)
                Text(
                  _formatDateTime(post.creationTime!),
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),
            ],
          ),

          // Avatar
          if (post.imageUrl != null)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  post.imageUrl!,
                  height: 100,
                  width: double.infinity,
                  fit: BoxFit.cover,
                ),
              ),
            ),
        ],
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
