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

  Future<List<UserDto>?> getAllUsers() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getAllUsers);
      if (response.data != null && response.data['items'] != null) {
        return (response.data['items'] as List)
            .map((json) => UserDto.fromJson(json))
            .toList();
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get all users: ${e.message}');
    }
  }

  Future<bool> canUsePasswordLogin() async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.canUsePasswordLogin,
      );
      return response.data?['result'] ?? false;
    } on DioException catch (e) {
      throw Exception('Failed to check password login: ${e.message}');
    }
  }

  Future<bool> enablePasswordLogin() async {
    try {
      await _apiClient.dio.post(ApiEndpoints.enablePasswordLogin);
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to enable password login: ${e.message}');
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
        // The response contains user info in a nested structure
        final userDto = response.data['currentUser'];
        if (userDto != null) {
          return UserDto.fromJson(userDto);
        }
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get current login information: ${e.message}');
    }
  }
}
