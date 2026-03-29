import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:molitao_app/data/repositories/post_repository.dart';
import 'package:molitao_app/data/models/post_model.dart';
import 'package:molitao_app/data/models/announce_model.dart';
import 'package:molitao_app/data/api/api_client.dart';

// State classes for different aspects of the trading post page
class TradingPostState {
  final List<PostModel> posts;
  final List<CmsArticleDto> categories;
  final CmsArticleDto? bulletin;
  final List<CmsArticleDto> hotWords;
  final List<PostModel> pinnedPosts;
  final bool isLoading;
  final bool isRefreshing;
  final bool isLoadingMore;
  final int currentPage;
  final bool hasMore;
  final String? errorMessage;
  final int? selectedCategoryId;
  final String searchKeywords;

  TradingPostState({
    this.posts = const [],
    this.categories = const [],
    this.bulletin,
    this.hotWords = const [],
    this.pinnedPosts = const [],
    this.isLoading = false,
    this.isRefreshing = false,
    this.isLoadingMore = false,
    this.currentPage = 1,
    this.hasMore = true,
    this.errorMessage,
    this.selectedCategoryId,
    this.searchKeywords = '',
  });

  TradingPostState copyWith({
    List<PostModel>? posts,
    List<CmsArticleDto>? categories,
    CmsArticleDto? bulletin,
    List<CmsArticleDto>? hotWords,
    List<PostModel>? pinnedPosts,
    bool? isLoading,
    bool? isRefreshing,
    bool? isLoadingMore,
    int? currentPage,
    bool? hasMore,
    String? errorMessage,
    int? selectedCategoryId,
    String? searchKeywords,
  }) {
    return TradingPostState(
      posts: posts ?? this.posts,
      categories: categories ?? this.categories,
      bulletin: bulletin ?? this.bulletin,
      hotWords: hotWords ?? this.hotWords,
      pinnedPosts: pinnedPosts ?? this.pinnedPosts,
      isLoading: isLoading ?? this.isLoading,
      isRefreshing: isRefreshing ?? this.isRefreshing,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      currentPage: currentPage ?? this.currentPage,
      hasMore: hasMore ?? this.hasMore,
      errorMessage: errorMessage ?? this.errorMessage,
      selectedCategoryId: selectedCategoryId ?? this.selectedCategoryId,
      searchKeywords: searchKeywords ?? this.searchKeywords,
    );
  }
}

// Provider for the trading post state
final tradingPostProvider =
    StateNotifierProvider<TradingPostNotifier, TradingPostState>(
      (ref) => TradingPostNotifier(),
    );

class TradingPostNotifier extends StateNotifier<TradingPostState> {
  final PostRepository _repository = PostRepository();
  final ApiClient _apiClient = ApiClient();

  TradingPostNotifier() : super(TradingPostState());

  Future<void> initialize() async {
    await loadInitialData();
  }

  Future<List<CmsArticleDto>> _loadCategories() async {
    try {
      final categories = await _repository.getCategoryList();
      return categories ?? [];
    } catch (e) {
      throw Exception('Failed to load categories');
    }
  }

  Future<CmsArticleDto?> _loadBulletin() async {
    try {
      final bulletin = await _repository.getLatestBulletin();
      return bulletin;
    } catch (e) {
      throw Exception('Failed to load bulletin');
    }
  }

  Future<List<CmsArticleDto>> _loadHotWords() async {
    try {
      // Using the actual hot words endpoint from ApiEndpoints
      final response = await _apiClient.dio.get('/api/HotWords/GetList');
      if (response.data != null && response.data['items'] != null) {
        return (response.data['items'] as List)
            .map((json) => CmsArticleDto.fromJson(json))
            .toList();
      }
      return [];
    } catch (e) {
      throw Exception('Failed to load hot words');
    }
  }

  Future<(List<PostModel>, bool)> _loadPosts({
    int page = 1,
    int? categoryId,
    String? search,
  }) async {
    try {
      final response = await _repository.getPostList(
        skipCount: (page - 1) * 10, // Assuming 10 items per page
        maxResultCount: 10,
        categoryId: categoryId,
      );

      // For now, we'll treat all posts as regular posts
      // In a real implementation, you'd separate pinned posts
      final posts = response ?? [];
      final hasMore =
          posts.length == 10; // If we got 10 items, there might be more

      return (posts, hasMore);
    } catch (e) {
      throw Exception('Failed to load posts');
    }
  }

  Future<void> loadInitialData() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      // Load all initial data concurrently
      final categories = await _loadCategories();
      final bulletin = await _loadBulletin();
      final hotWords = await _loadHotWords();
      final postsResult = await _loadPosts(
        page: 1,
        categoryId: state.selectedCategoryId,
        search: state.searchKeywords,
      );

      state = state.copyWith(
        isLoading: false,
        categories: categories,
        bulletin: bulletin,
        hotWords: hotWords,
        posts: postsResult.$1,
        hasMore: postsResult.$2,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  Future<void> refresh() async {
    state = state.copyWith(isRefreshing: true, errorMessage: null);

    try {
      final categories = await _loadCategories();
      final bulletin = await _loadBulletin();
      final hotWords = await _loadHotWords();
      final postsResult = await _loadPosts(
        page: 1,
        categoryId: state.selectedCategoryId,
        search: state.searchKeywords,
      );

      state = state.copyWith(
        isRefreshing: false,
        currentPage: 1,
        categories: categories,
        bulletin: bulletin,
        hotWords: hotWords,
        posts: postsResult.$1,
        hasMore: postsResult.$2,
      );
    } catch (e) {
      state = state.copyWith(isRefreshing: false, errorMessage: e.toString());
    }
  }

  Future<void> loadMore() async {
    if (state.isLoadingMore || !state.hasMore) return;

    state = state.copyWith(isLoadingMore: true);

    try {
      final nextPage = state.currentPage + 1;
      final result = await _loadPosts(
        page: nextPage,
        categoryId: state.selectedCategoryId,
        search: state.searchKeywords,
      );

      state = state.copyWith(
        currentPage: nextPage,
        posts: [...state.posts, ...result.$1],
        hasMore: result.$2,
        isLoadingMore: false,
      );
    } catch (e) {
      state = state.copyWith(isLoadingMore: false, errorMessage: e.toString());
    }
  }

  Future<void> selectCategory(int? categoryId) async {
    state = state.copyWith(selectedCategoryId: categoryId);
    await refresh();
  }

  Future<void> setSearchKeywords(String keywords) async {
    state = state.copyWith(searchKeywords: keywords);
  }

  Future<void> searchPosts() async {
    state = state.copyWith(currentPage: 1, errorMessage: null);

    try {
      final result = await _loadPosts(
        page: 1,
        categoryId: state.selectedCategoryId,
        search: state.searchKeywords,
      );

      state = state.copyWith(posts: result.$1, hasMore: result.$2);
    } catch (e) {
      state = state.copyWith(errorMessage: e.toString());
    }
  }

  Future<void> switchToHotWord(CmsArticleDto hotWord) async {
    state = state.copyWith(searchKeywords: hotWord.title ?? '');
    await searchPosts();
  }
}
