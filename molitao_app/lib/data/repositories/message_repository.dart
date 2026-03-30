import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/chat_message_model.dart';

class MessageRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<ChatMessage>?> getPrivateHistory({
    required int userId,
    int? lastTime,
    int size = 20,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getPrivateHistory,
        queryParameters: {'id': userId, 'lastTime': lastTime, 'size': size},
      );

      // 拦截器已解包 result
      if (response.data != null) {
        if (response.data is List) {
          return (response.data as List)
              .map((json) => ChatMessage.fromJson(json))
              .toList();
        } else if (response.data['items'] != null) {
          return (response.data['items'] as List)
              .map((json) => ChatMessage.fromJson(json))
              .toList();
        }
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get private history: ${e.message}');
    }
  }

  Future<List<ChatMessage>?> getChannelHistory({
    required String channel,
    int? lastTime,
    int size = 20,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getChanHistory,
        queryParameters: {'chan': channel, 'lastTime': lastTime, 'size': size},
      );

      // 拦截器已解包 result
      if (response.data != null) {
        if (response.data is List) {
          return (response.data as List)
              .map((json) => ChatMessage.fromJson(json))
              .toList();
        } else if (response.data['items'] != null) {
          return (response.data['items'] as List)
              .map((json) => ChatMessage.fromJson(json))
              .toList();
        }
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get channel history: ${e.message}');
    }
  }

  Future<int?> getChannelLastId(String channel) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getChanLastId,
        queryParameters: {'chan': channel},
      );

      // 拦截器已解包 result
      return response.data?['lastId'] ?? response.data;
    } on DioException catch (e) {
      throw Exception('Failed to get channel last ID: ${e.message}');
    }
  }

  Future<int?> getPrivateLastId(int userId) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getPrivateLastId,
        queryParameters: {'id': userId},
      );

      // 拦截器已解包 result
      return response.data?['lastId'] ?? response.data;
    } on DioException catch (e) {
      throw Exception('Failed to get private last ID: ${e.message}');
    }
  }
}
