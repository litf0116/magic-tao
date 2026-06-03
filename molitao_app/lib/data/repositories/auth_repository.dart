import 'dart:convert';
import 'dart:io' show Platform;
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart' show kDebugMode, debugPrint;
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_model.dart';
import '../services/storage_service.dart';

/// 调试日志开关：仅在 Debug 模式下打印
void _debugLog(String message) {
  if (kDebugMode) debugPrint(message);
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
          'platform': _detectPlatform(), // 平台标识（Android/iOS/Other）
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
      await _storageService.clearToken();
      await _storageService.clearUserData();
      return true;
    }
  }

  /// 手机号验证码重置密码
  Future<bool> phoneResetPassword(
      String phoneNumber, String code, String newPassword) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.phoneResetPassword,
        data: {
          'phoneNumber': phoneNumber,
          'code': code,
          'newPassword': newPassword,
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception('重置密码失败: ${e.message}');
    }
  }

  /// 注销账号
  Future<bool> deleteAccount(String password) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.deleteAccount,
        data: {'password': password},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('注销失败: ${e.message}');
    }
  }

  /// 修改密码（当前登录用户）
  Future<bool> changePassword(String currentPassword, String newPassword) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.changePassword,
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        },
      );
      return true;
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null && errorMessage.toString().isNotEmpty) {
        throw Exception(errorMessage);
      }
      throw Exception('修改密码失败: ${e.message}');
    }
  }

  /// 发送短信验证码
  Future<bool> sendSmsCode(String phoneNumber, {String purpose = 'login'}) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.sendSmsCode,
        data: {'phoneNumber': phoneNumber, 'purpose': purpose},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('发送验证码失败: ${e.message}');
    }
  }

  /// 手机号验证码登录
  Future<LoginResult> phoneAuthenticate(String phoneNumber, String code) async {
    try {
      _debugLog('[AuthRepository] ===== 手机号验证码登录请求 =====');
      _debugLog('[AuthRepository] phoneNumber: $phoneNumber');

      final response = await _apiClient.dio.post(
        ApiEndpoints.phoneAuthenticate,
        data: {'phoneNumber': phoneNumber, 'code': code},
      );

      _debugLog('[AuthRepository] 手机号登录响应: ${jsonEncode(response.data)}');

      if (response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final accessToken = data['accessToken'] as String?;

        if (accessToken != null) {
          await _storageService.setToken(accessToken);
        }

        UserDto? user;
        List<String>? roles;
        if (accessToken != null) {
          try {
            _debugLog('[AuthRepository] 开始获取用户信息...');
            final userInfo = await getCurrentLoginInformations();
            if (userInfo != null) {
              user = userInfo['user'] != null
                  ? UserDto.fromJson(userInfo['user'] as Map<String, dynamic>)
                  : null;
              roles = (userInfo['roles'] as List<dynamic>?)
                  ?.map((e) => e.toString())
                  .toList();
            }
          } catch (e) {
            _debugLog('[AuthRepository] 获取用户信息失败: $e');
          }
        }

        return LoginResult(accessToken: accessToken, user: user, roles: roles);
      }
      return const LoginResult();
    } on DioException catch (e) {
      throw Exception('手机号登录失败: ${e.message}');
    }
  }

  /// 获取当前用户的微信 OpenId
  /// 返回 null 表示用户没有通过微信 App 登录
  Future<String?> getMyWechatOpenId() async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getMyWechatOpenId,
      );
      if (response.data != null) {
        return response.data as String?;
      }
      return null;
    } on DioException catch (e) {
      _debugLog('[AuthRepository] 获取微信 OpenId 失败: ${e.message}');
      return null;
    }
  }

  /// 检测当前运行平台（用于微信登录等需要区分平台的接口）
  String _detectPlatform() {
    if (Platform.isAndroid) return 'android';
    if (Platform.isIOS) return 'ios';
    return 'other';
  }
}
