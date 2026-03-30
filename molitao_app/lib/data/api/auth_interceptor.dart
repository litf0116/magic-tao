import 'package:dio/dio.dart';
import '../services/storage_service.dart';

class AuthInterceptor extends Interceptor {
  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
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
    if (response.data is Map<String, dynamic>) {
      final data = response.data as Map<String, dynamic>;

      // ABP 标准响应格式: { success, result, error }
      if (data.containsKey('success') && data.containsKey('result')) {
        if (data['success'] == true) {
          // 成功：解包 result，统一数组格式
          response.data = _normalizeResult(data['result']);
        } else if (data['error'] != null) {
          // 失败：转换为异常
          final error = data['error'] as Map<String, dynamic>;
          throw DioException(
            requestOptions: response.requestOptions,
            response: response,
            type: DioExceptionType.badResponse,
            message: error['details'] ?? error['message'] ?? '请求失败',
          );
        }
      }
    } else if (response.data is List) {
      // 直接数组：统一包装
      response.data = {'items': response.data};
    }

    super.onResponse(response, handler);
  }

  /// 标准化结果：数组包装为 { items: [...] }
  dynamic _normalizeResult(dynamic result) {
    if (result is List) {
      return {'items': result};
    }
    return result;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.response?.statusCode == 401) {
      // TODO: 跳转登录页
    }
    super.onError(err, handler);
  }
}
