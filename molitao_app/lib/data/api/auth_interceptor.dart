import 'package:dio/dio.dart';
import '../services/storage_service.dart';

class AuthInterceptor extends Interceptor {
  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    // Add headers to every request
    final token = await StorageService().getToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    options.headers['Abp.Tenantid'] = '1';
    options.headers['Content-Type'] = 'application/json';
    options.headers['AppName'] = 'flutter';

    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    // 自动解包 ABP 标准响应格式: { success: true, result: T }
    // 将 response.data 从 { success, result, error } 转换为 result
    if (response.data is Map<String, dynamic>) {
      final data = response.data as Map<String, dynamic>;

      // 检查是否是 ABP 标准响应格式
      if (data.containsKey('success') && data.containsKey('result')) {
        if (data['success'] == true) {
          // 成功响应：解包 result
          response.data = data['result'];
        } else if (data.containsKey('error') && data['error'] != null) {
          // 失败响应：转换为 DioException
          final error = data['error'] as Map<String, dynamic>;
          final message = error['message'] ?? '请求失败';
          final details = error['details'] ?? '';

          throw DioException(
            requestOptions: response.requestOptions,
            response: response,
            type: DioExceptionType.badResponse,
            message: details.isNotEmpty ? '$message: $details' : message,
          );
        }
      }
    }

    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    // 处理 401 未授权
    if (err.response?.statusCode == 401) {
      // TODO: 跳转到登录页或清除 token
      // StorageService().clearToken();
    }

    super.onError(err, handler);
  }
}
