import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/login_binding_model.dart';

class LoginBindingService {
  final ApiClient _apiClient = ApiClient();

  Future<List<LoginBindingDto>> getLoginBindings() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getLoginBindings);
      if (response.data != null) {
        final result = LoginBindingListResultDto.fromJson(response.data);
        return result.items ?? [];
      }
      return [];
    } on DioException catch (e) {
      throw Exception('获取登录绑定失败: ${e.message}');
    }
  }

  Future<bool> bindPhone(String phoneNumber, String code) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.bindPhone,
        data: {'phoneNumber': phoneNumber, 'code': code},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('绑定手机号失败: ${e.message}');
    }
  }

  Future<bool> unbindLogin(String loginProvider) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.unbindLogin,
        data: {'loginProvider': loginProvider},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('解绑失败: ${e.message}');
    }
  }
}
