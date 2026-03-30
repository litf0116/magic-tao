import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';

class UserRepository {
  final ApiClient _apiClient = ApiClient();

  Future<UserDto?> getCurrentUser() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getUser);
      if (response.data != null) {
        return UserDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get current user: ${e.message}');
    }
  }

  Future<UserDto?> updateUser(UserDto user) async {
    try {
      final response = await _apiClient.dio.put(
        ApiEndpoints.updateUser,
        data: user.toJson(),
      );
      if (response.data != null) {
        return UserDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to update user: ${e.message}');
    }
  }

  Future<bool> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.changePassword,
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
          'newPasswordConfirm': newPassword,
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to change password: ${e.message}');
    }
  }

  Future<bool> disablePasswordLogin() async {
    try {
      await _apiClient.dio.post(ApiEndpoints.disablePasswordLogin);
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to disable password login: ${e.message}');
    }
  }

  Future<UserDto?> getCurrentLoginInformation() async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getCurrentLoginInformation,
      );
      if (response.data != null) {
        // 响应可能是 { user: {...} } 或 { currentUser: {...} } 或直接用户对象
        if (response.data['user'] != null) {
          return UserDto.fromJson(response.data['user']);
        } else if (response.data['currentUser'] != null) {
          return UserDto.fromJson(response.data['currentUser']);
        } else if (response.data['id'] != null) {
          return UserDto.fromJson(response.data);
        }
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get current login information: ${e.message}');
    }
  }
}
