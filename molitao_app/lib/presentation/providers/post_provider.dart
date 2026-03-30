import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/post_model.dart';
import '../../data/repositories/post_repository.dart';

/// 帖子详情状态
class PostDetailState {
  final PostModel? post;
  final bool isLoading;
  final String? error;

  const PostDetailState({this.post, this.isLoading = false, this.error});

  PostDetailState copyWith({PostModel? post, bool? isLoading, String? error}) {
    return PostDetailState(
      post: post ?? this.post,
      isLoading: isLoading ?? this.isLoading,
      error: error,
    );
  }
}

/// 帖子详情 Notifier
class PostDetailNotifier extends StateNotifier<PostDetailState> {
  final PostRepository _postRepository = PostRepository();

  PostDetailNotifier() : super(const PostDetailState());

  Future<void> loadPost(int postId) async {
    state = state.copyWith(isLoading: true);

    try {
      final post = await _postRepository.getPostDetail(postId);
      state = state.copyWith(post: post, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: '加载帖子失败: $e');
    }
  }

  Future<bool> deletePost(int postId) async {
    try {
      await _postRepository.deletePost(postId);
      return true;
    } catch (e) {
      state = state.copyWith(error: '删除帖子失败: $e');
      return false;
    }
  }
}

/// 帖子详情 Provider
final postDetailProvider =
    StateNotifierProvider.family<PostDetailNotifier, PostDetailState, int>((
      ref,
      postId,
    ) {
      final notifier = PostDetailNotifier();
      notifier.loadPost(postId);
      return notifier;
    });

/// 帖子列表状态
class PostListState {
  final List<PostModel> posts;
  final bool isLoading;
  final bool hasMore;
  final String? error;
  final int skipCount;

  const PostListState({
    this.posts = const [],
    this.isLoading = false,
    this.hasMore = true,
    this.error,
    this.skipCount = 0,
  });

  PostListState copyWith({
    List<PostModel>? posts,
    bool? isLoading,
    bool? hasMore,
    String? error,
    int? skipCount,
  }) {
    return PostListState(
      posts: posts ?? this.posts,
      isLoading: isLoading ?? this.isLoading,
      hasMore: hasMore ?? this.hasMore,
      error: error,
      skipCount: skipCount ?? this.skipCount,
    );
  }
}

/// 帖子列表 Notifier
class PostListNotifier extends StateNotifier<PostListState> {
  final PostRepository _postRepository = PostRepository();
  final int? categoryId;

  PostListNotifier({this.categoryId}) : super(const PostListState());

  Future<void> loadPosts({bool refresh = false}) async {
    if (state.isLoading) return;
    if (!refresh && !state.hasMore) return;

    state = state.copyWith(isLoading: true);

    try {
      final skipCount = refresh ? 0 : state.skipCount;
      final posts = await _postRepository.getPostList(
        skipCount: skipCount,
        maxResultCount: 10,
        categoryId: categoryId,
      );

      final newPosts = refresh ? posts : [...state.posts, ...?posts];
      final hasMore = (posts?.length ?? 0) >= 10;

      state = state.copyWith(
        posts: newPosts,
        isLoading: false,
        hasMore: hasMore,
        skipCount: skipCount + (posts?.length ?? 0),
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, error: '加载帖子列表失败: $e');
    }
  }

  Future<void> refresh() async {
    await loadPosts(refresh: true);
  }
}

/// 帖子列表 Provider
final postListProvider =
    StateNotifierProvider.family<PostListNotifier, PostListState, int?>((
      ref,
      categoryId,
    ) {
      final notifier = PostListNotifier(categoryId: categoryId);
      notifier.loadPosts();
      return notifier;
    });
