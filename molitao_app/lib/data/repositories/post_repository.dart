import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/post_model.dart';
import '../models/announce_model.dart';

class PostRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<PostModel>> getPostList({
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

      // 拦截器已统一格式为 { items: [...] }
      final items = response.data['items'] as List? ?? [];
      return items.map((json) => PostModel.fromJson(json)).toList();
    } on DioException catch (e) {
      throw Exception('Failed to get post list: ${e.message}');
    }
  }

  Future<CmsArticleDto?> getLatestBulletin() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getLatestBulletin);
      if (response.data != null) {
        return CmsArticleDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get latest bulletin: ${e.message}');
    }
  }

  Future<List<CmsArticleDto>> getCategoryList() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getCategoryList);

      // 拦截器已统一格式为 { items: [...] }
      final items = response.data['items'] as List? ?? [];
      return items.map((json) => CmsArticleDto.fromJson(json)).toList();
    } on DioException catch (e) {
      throw Exception('Failed to get category list: ${e.message}');
    }
  }

  Future<PostModel?> getPostDetail(int postId) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.getPostDetail}$postId',
      );
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
      if (response.data != null) {
        return PostModel.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to update post: ${e.message}');
    }
  }
}
