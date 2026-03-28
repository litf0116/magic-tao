import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';
import '../services/storage_service.dart';

class AuthRepository {
  final ApiClient _apiClient = ApiClient();
  final StorageService _storageService = StorageService();

  Future<UserDto?> login(String username, String password) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.authenticate,
        data: {'userName': username, 'password': password},
      );

      if (response.data != null) {
        final userDto = UserDto.fromJson(response.data);

        // Save token to storage
        if (userDto.id != null) {
          await _storageService.setToken(userDto.id.toString());
        }

        return userDto;
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Login failed: ${e.message}');
    }
  }

  Future<UserDto?> weixinMiniLogin(String code) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.weixinMiniAuthenticate,
        data: {'code': code},
      );

      if (response.data != null) {
        final userDto = UserDto.fromJson(response.data);
        return userDto;
      }
      return null;
    } on DioException catch (e) {
      throw Exception('WeChat Mini Program login failed: ${e.message}');
    }
  }

  Future<UserDto?> weixinAppLogin(String code) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.authenticateWeixinApp,
        data: {'code': code},
      );

      if (response.data != null) {
        final userDto = UserDto.fromJson(response.data);
        return userDto;
      }
      return null;
    } on DioException catch (e) {
      throw Exception('WeChat App login failed: ${e.message}');
    }
  }

  Future<bool> logout() async {
    try {
      await _apiClient.dio.get(ApiEndpoints.logout);
      await _storageService.clearToken();
      return true;
    } on DioException catch (e) {
      throw Exception('Logout failed: ${e.message}');
    }
  }

  Future<String?> getQrLoginUrl(String state) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.pubQrLogin}?state=$state',
      );
      return response.data?['qrUrl'];
    } on DioException catch (e) {
      throw Exception('QR Login URL failed: ${e.message}');
    }
  }

  Future<UserDto?> getQrToken(String key) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.qrToken}?key=$key',
      );

      if (response.data != null) {
        final userDto = UserDto.fromJson(response.data);
        return userDto;
      }
      return null;
    } on DioException catch (e) {
      throw Exception('QR Token retrieval failed: ${e.message}');
    }
  }
}
