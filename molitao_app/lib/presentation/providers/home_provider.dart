import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../data/api/api_client.dart';
import '../../data/api/api_endpoints.dart';
import '../../data/models/cms_article_model.dart';
import '../../data/models/advertising_space_model.dart';

// Home state
class HomeState {
  final List<CmsArticle> articles;
  final List<AdvertisingSpace> advertisingSpaces;
  final bool isLoading;
  final String? errorMessage;

  HomeState({
    required this.articles,
    required this.advertisingSpaces,
    required this.isLoading,
    this.errorMessage,
  });

  HomeState copyWith({
    List<CmsArticle>? articles,
    List<AdvertisingSpace>? advertisingSpaces,
    bool? isLoading,
    String? errorMessage,
  }) {
    return HomeState(
      articles: articles ?? this.articles,
      advertisingSpaces: advertisingSpaces ?? this.advertisingSpaces,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage ?? this.errorMessage,
    );
  }

  factory HomeState.initial() {
    return HomeState(articles: [], advertisingSpaces: [], isLoading: false);
  }
}

// Home notifier
class HomeNotifier extends StateNotifier<HomeState> {
  final Ref _ref;

  HomeNotifier(this._ref) : super(HomeState.initial());

  Future<void> loadHomeData() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      // Load articles and advertising spaces concurrently
      final articlesFuture = _loadArticles();
      final advertisingSpacesFuture = _loadAdvertisingSpaces();

      await Future.wait([articlesFuture, advertisingSpacesFuture]);

      state = state.copyWith(isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  Future<void> _loadArticles() async {
    try {
      final response = await ApiClient().dio.get(
        ApiEndpoints.getAllPublicCmsArticles,
        queryParameters: {'pid': 1},
      );

      if (response.data != null && response.data['items'] != null) {
        final articles = (response.data['items'] as List)
            .map((json) => CmsArticle.fromJson(json))
            .toList();

        state = state.copyWith(articles: articles);
      }
    } catch (e) {
      // Handle error silently or log if needed
      print('Error loading articles: $e');
    }
  }

  Future<void> _loadAdvertisingSpaces() async {
    try {
      final response = await ApiClient().dio.get(
        '${ApiEndpoints.getAdvertisingSpaceTypeList}1',
      );

      if (response.data != null && response.data['items'] != null) {
        final advertisingSpaces = (response.data['items'] as List)
            .map((json) => AdvertisingSpace.fromJson(json))
            .toList();

        state = state.copyWith(advertisingSpaces: advertisingSpaces);
      } else if (response.data != null && response.data is List) {
        final advertisingSpaces = (response.data as List)
            .map((json) => AdvertisingSpace.fromJson(json))
            .toList();

        state = state.copyWith(advertisingSpaces: advertisingSpaces);
      }
    } catch (e) {
      // Handle error silently or log if needed
      print('Error loading advertising spaces: $e');
    }
  }

  Future<void> refreshHomeData() async {
    await loadHomeData();
  }
}

// Home provider
final homeProvider = StateNotifierProvider<HomeNotifier, HomeState>((ref) {
  return HomeNotifier(ref);
});
