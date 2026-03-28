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
}
