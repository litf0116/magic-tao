import 'dart:convert';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/qr_code_model.dart';

/// 调试日志开关
const bool _kDebugLog = true;

void _debugLog(String message) {
  // ignore: avoid_print
  if (_kDebugLog) print(message);
}

/// 二维码扫码登录仓库
class QrCodeRepository {
  final ApiClient _apiClient = ApiClient();

  /// 扫码获取用户信息
  /// 
  /// [code] 二维码code
  /// 返回用户信息（已脱敏手机号）
  Future<QrCodeUserInfo?> getUserInfoByCode(String code) async {
    try {
      _debugLog('[QrCodeRepository] 获取用户信息, code: $code');
      
      final response = await _apiClient.dio.get(
        '${ApiEndpoints.qrCodeUserInfo}/$code',
      );

      _debugLog('[QrCodeRepository] 响应: ${jsonEncode(response.data)}');

      if (response.data != null) {
        return QrCodeUserInfo.fromJson(response.data as Map<String, dynamic>);
      }
      return null;
    } on DioException catch (e) {
      _debugLog('[QrCodeRepository] 获取用户信息失败: ${e.message}');
      throw Exception('获取用户信息失败: ${e.message}');
    }
  }

  /// 确认登录
  /// 
  /// [code] 二维码code
  /// 返回登录结果，包含 token 和用户信息
  Future<QrCodeLoginResult?> confirmLogin(String code) async {
    try {
      _debugLog('[QrCodeRepository] 确认登录, code: $code');
      
      final response = await _apiClient.dio.post(
        ApiEndpoints.qrCodeConfirm,
        data: {'code': code},
      );

      _debugLog('[QrCodeRepository] 确认登录响应: ${jsonEncode(response.data)}');

      if (response.data != null) {
        return QrCodeLoginResult.fromJson(response.data as Map<String, dynamic>);
      }
      return null;
    } on DioException catch (e) {
      _debugLog('[QrCodeRepository] 确认登录失败: ${e.message}');
      throw Exception('确认登录失败: ${e.message}');
    }
  }
}
