import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';

class FriendRepository {
  final ApiClient _apiClient = ApiClient();

  Future<bool> addFriend(int userId) async {
    try {
      await _apiClient.dio.get(
        ApiEndpoints.addFriend,
        queryParameters: {'id': userId},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to add friend: ${e.message}');
    }
  }

  Future<List<UserDtoBase>?> getUserFriends({
    int? userId,
    String? status,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUserFriends,
        queryParameters: {'id': userId, 'status': status},
      );

      if (response.data != null && response.data['items'] != null) {
        return (response.data['items'] as List)
            .map((json) => UserDtoBase.fromJson(json))
            .toList();
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get user friends: ${e.message}');
    }
  }

  Future<bool> agreeFriend({
    required int userId,
    required String status,
  }) async {
    try {
      await _apiClient.dio.get(
        ApiEndpoints.agreeFriend,
        queryParameters: {'id': userId, 'status': status},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to agree friend: ${e.message}');
    }
  }

  Future<int> getUserFriendCount() async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUserFriendCount,
      );
      return response.data?['count'] ?? 0;
    } on DioException catch (e) {
      throw Exception('Failed to get user friend count: ${e.message}');
    }
  }
}
