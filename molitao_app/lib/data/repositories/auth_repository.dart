import 'dart:convert';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';
import '../services/storage_service.dart';

/// 调试日志开关
const bool _kDebugLog = true;

void _debugLog(String message) {
  if (_kDebugLog) print(message);
}

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

      // 打印原始返回信息
      _debugLog('[AuthRepository] ===== 原始响应 =====');
      _debugLog('[AuthRepository] statusCode: ${response.statusCode}');
      _debugLog('[AuthRepository] data type: ${response.data.runtimeType}');
      _debugLog('[AuthRepository] raw data: ${jsonEncode(response.data)}');
      _debugLog('[AuthRepository] ===================');

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;

        // Save token to storage
        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        // 登录成功后获取用户信息
        UserDto? user;
        List<String>? roles;
        if (accessToken != null) {
          try {
            _debugLog('[AuthRepository] 开始获取用户信息...');
            final userInfo = await getCurrentLoginInformations();
            _debugLog('[AuthRepository] 用户信息响应: $userInfo');
            if (userInfo != null) {
              user = userInfo['user'] != null
                  ? UserDto.fromJson(userInfo['user'] as Map<String, dynamic>)
                  : null;
              roles = (userInfo['roles'] as List<dynamic>?)
                  ?.map((e) => e.toString())
                  .toList();
              _debugLog(
                '[AuthRepository] 解析后 user: id=${user?.id}, userName=${user?.userName}, fullName=${user?.fullName}',
              );
            }
          } catch (e) {
            _debugLog('[AuthRepository] 获取用户信息失败: $e');
          }
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

      _debugLog('[AuthRepository] 微信小程序登录响应: ${jsonEncode(response.data)}');

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
      _debugLog('[AuthRepository] ===== 微信App登录请求 =====');
      _debugLog('[AuthRepository] authCode: $code');
      _debugLog(
        '[AuthRepository] endpoint: ${ApiEndpoints.authenticateWeixinApp}',
      );

      final response = await _apiClient.dio.post(
        ApiEndpoints.authenticateWeixinApp,
        data: {
          'authCode': code, // ✅ 修复：使用 authCode 而不是 code
          'platform': 'android', // 平台标识
        },
      );

      _debugLog('[AuthRepository] 微信App登录响应: ${jsonEncode(response.data)}');

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;

        // 保存 token
        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        // 获取用户信息（参考账号密码登录流程）
        UserDto? user;
        List<String>? roles;
        if (accessToken != null) {
          try {
            _debugLog('[AuthRepository] 微信登录成功，开始获取用户信息...');
            final userInfo = await getCurrentLoginInformations();
            _debugLog('[AuthRepository] 用户信息响应: $userInfo');
            if (userInfo != null) {
              user = userInfo['user'] != null
                  ? UserDto.fromJson(userInfo['user'] as Map<String, dynamic>)
                  : null;
              roles = (userInfo['roles'] as List<dynamic>?)
                  ?.map((e) => e.toString())
                  .toList();
              _debugLog(
                '[AuthRepository] 解析后 user: id=${user?.id}, userName=${user?.userName}, fullName=${user?.fullName}',
              );
            }
          } catch (e) {
            _debugLog('[AuthRepository] 获取用户信息失败: $e');
          }
        }

        return LoginResult(accessToken: accessToken, user: user, roles: roles);
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
      _debugLog(
        '[AuthRepository] getCurrentLoginInformations 响应: ${jsonEncode(response.data)}',
      );
      return response.data as Map<String, dynamic>?;
    } on DioException catch (e) {
      _debugLog(
        '[AuthRepository] getCurrentLoginInformations 失败: ${e.message}',
      );
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
