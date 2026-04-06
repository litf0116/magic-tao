import 'package:dio/dio.dart';
import '../services/storage_service.dart';
import '../services/navigation_service.dart';

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

      if (data.containsKey('success') && data.containsKey('result')) {
        if (data['success'] == true) {
          response.data = _normalizeResult(data['result']);
        } else if (data['error'] != null) {
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
      response.data = {'items': response.data};
    }

    super.onResponse(response, handler);
  }

  dynamic _normalizeResult(dynamic result) {
    if (result is List) {
      return {'items': result};
    }
    return result;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      await StorageService().clearToken();
      NavigationService.instance.navigateToLogin();
    }
    super.onError(err, handler);
  }
}
