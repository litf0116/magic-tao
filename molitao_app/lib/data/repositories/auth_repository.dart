import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';
import '../services/storage_service.dart';

/// 登录响应结果
class LoginResult {
  final String? accessToken;
  final UserDto? user;
  final List<String>? roles;

  const LoginResult({this.accessToken, this.user, this.roles});
}

class AuthRepository {
  final ApiClient _apiClient = ApiClient();
  final StorageService _storageService = StorageService();

  /// 账号密码登录
  Future<LoginResult> login(String username, String password) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.authenticate,
        data: {'userNameOrEmailAddress': username, 'password': password},
      );

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;
        final user = data['user'] != null
            ? UserDto.fromJson(data['user'] as Map<String, dynamic>)
            : null;
        final roles = (data['roles'] as List<dynamic>?)
            ?.map((e) => e.toString())
            .toList();

        // Save token to storage
        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        return LoginResult(accessToken: accessToken, user: user, roles: roles);
      }
      return const LoginResult();
    } on DioException catch (e) {
      throw Exception('登录失败: ${e.message}');
    }
  }

  /// 微信小程序登录
  Future<LoginResult> weixinMiniLogin(String code) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.weixinMiniAuthenticate,
        data: {'code': code},
      );

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;
        final user = data['user'] != null
            ? UserDto.fromJson(data['user'] as Map<String, dynamic>)
            : null;

        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        return LoginResult(accessToken: accessToken, user: user);
      }
      return const LoginResult();
    } on DioException catch (e) {
      throw Exception('微信小程序登录失败: ${e.message}');
    }
  }

  /// 微信 App 登录
  Future<LoginResult> weixinAppLogin(String code) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.authenticateWeixinApp,
        data: {'code': code},
      );

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;
        final user = data['user'] != null
            ? UserDto.fromJson(data['user'] as Map<String, dynamic>)
            : null;

        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        return LoginResult(accessToken: accessToken, user: user);
      }
      return const LoginResult();
    } on DioException catch (e) {
      throw Exception('微信 App 登录失败: ${e.message}');
    }
  }

  /// 获取当前登录用户信息
  Future<Map<String, dynamic>?> getCurrentLoginInformations() async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getCurrentLoginInformation,
      );
      return response.data as Map<String, dynamic>?;
    } on DioException catch (e) {
      throw Exception('获取用户信息失败: ${e.message}');
    }
  }

  Future<bool> logout() async {
    try {
      await _apiClient.dio.get(ApiEndpoints.logout);
      await _storageService.clearToken();
      await _storageService.clearUserData();
      return true;
    } on DioException {
      // 即使 API 调用失败，也清除本地数据
      await _storageService.clearToken();
      await _storageService.clearUserData();
      return true;
    }
  }

  Future<String?> getQrLoginUrl(String state) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.pubQrLogin}?state=$state',
      );
      return response.data?['qrUrl'];
    } on DioException catch (e) {
      throw Exception('获取二维码失败: ${e.message}');
    }
  }

  Future<LoginResult?> getQrToken(String key) async {
    try {
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.qrToken}?key=$key',
      );

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;
        final user = data['user'] != null
            ? UserDto.fromJson(data['user'] as Map<String, dynamic>)
            : null;

        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        return LoginResult(accessToken: accessToken, user: user);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('获取二维码登录结果失败: ${e.message}');
    }
  }
}
