import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/post_model.dart';
import '../models/announce_model.dart';

class PostRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<PostModel>?> getPostList({
    int? skipCount = 0,
    int? maxResultCount = 10,
    int? categoryId,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getPostList,
        queryParameters: {
          'SkipCount': skipCount,
          'MaxResultCount': maxResultCount,
          'CategoryId': categoryId,
        },
      );

      // 拦截器已解包 result，response.data 可能是：
      // 1. { items: [...], totalCount, hasNextPages } (分页格式)
      // 2. [...] (直接数组，兼容旧格式)
      if (response.data != null) {
        if (response.data is List) {
          // 直接数组格式
          return (response.data as List)
              .map((json) => PostModel.fromJson(json))
              .toList();
        } else if (response.data['items'] != null) {
          // 分页格式
          return (response.data['items'] as List)
              .map((json) => PostModel.fromJson(json))
              .toList();
        }
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get post list: ${e.message}');
    }
  }

  Future<CmsArticleDto?> getLatestBulletin() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getLatestBulletin);
      // 拦截器已解包 result，response.data 就是单个对象
      if (response.data != null) {
        return CmsArticleDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get latest bulletin: ${e.message}');
    }
  }

  Future<List<CmsArticleDto>?> getCategoryList() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getCategoryList);

      // 拦截器已解包 result，response.data 可能是：
      // 1. [...] (直接数组)
      // 2. { items: [...] } (包装格式)
      if (response.data != null) {
        if (response.data is List) {
          // 直接数组格式 (GetCategoryList 返回这种格式)
          return (response.data as List)
              .map((json) => CmsArticleDto.fromJson(json))
              .toList();
        } else if (response.data['items'] != null) {
          // 包装格式
          return (response.data['items'] as List)
              .map((json) => CmsArticleDto.fromJson(json))
              .toList();
        }
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get category list: ${e.message}');
    }
  }

  Future<PostModel?> getPostDetail(int postId) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.getPostDetail}$postId',
      );
      // 拦截器已解包 result
      if (response.data != null) {
        return PostModel.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get post detail: ${e.message}');
    }
  }

  Future<bool> deletePost(int postId) async {
    try {
      await _apiClient.dio.get('${ApiEndpoints.deletePost}$postId');
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to delete post: ${e.message}');
    }
  }

  Future<PostModel?> createPost({
    required String title,
    required String content,
    String? imageUrl,
    int? categoryId,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.addPost,
        data: {
          'title': title,
          'content': content,
          'imageUrl': imageUrl,
          'categoryId': categoryId,
        },
      );

      // 拦截器已解包 result
      if (response.data != null) {
        return PostModel.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to create post: ${e.message}');
    }
  }

  Future<PostModel?> updatePost({
    required int id,
    required String title,
    required String content,
    String? imageUrl,
    int? categoryId,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.editPost,
        data: {
          'id': id,
          'title': title,
          'content': content,
          'imageUrl': imageUrl,
          'categoryId': categoryId,
        },
      );

      // 拦截器已解包 result
      if (response.data != null) {
        return PostModel.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to update post: ${e.message}');
    }
  }
}
