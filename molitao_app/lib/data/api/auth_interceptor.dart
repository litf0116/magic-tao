import 'package:dio/dio.dart';
import '../services/storage_service.dart';
import '../services/navigation_service.dart';
import 'api_client.dart';

class AuthInterceptor extends Interceptor {
  static bool _isRefreshing = false;
  static final List<void Function(String)> _refreshSubscribers = [];

  void _subscribeTokenRefresh(void Function(String) callback) {
    _refreshSubscribers.add(callback);
  }

  void _onTokenRefreshed(String token) {
    for (final callback in _refreshSubscribers) {
      callback(token);
    }
    _refreshSubscribers.clear();
  }

  Future<String?> _refreshAccessToken() async {
    final refreshToken = await StorageService().getRefreshToken();
    if (refreshToken == null) return null;

    try {
      final response = await ApiClient().dio.post(
        '/api/TokenAuth/RefreshToken',
        data: {'refreshToken': refreshToken},
        options: Options(
          headers: {
            'Content-Type': 'application/json',
            'Abp.Tenantid': '1',
          },
        ),
      );

      if (response.data != null && response.data['accessToken'] != null) {
        final accessToken = response.data['accessToken'] as String;
        final expireInSeconds = response.data['expireInSeconds'] as int? ?? 604800;
        
        await StorageService().setToken(accessToken);
        await StorageService().setTokenExpireTime(expireInSeconds);
        
        return accessToken;
      }
    } catch (e) {
      print('[AuthInterceptor] Refresh token failed: $e');
      await StorageService().clearAllTokens();
      NavigationService.instance.navigateToLogin();
    }
    return null;
  }

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

    // Token 自动续期
    final shouldRefresh = await StorageService().isTokenExpiringSoon(3600);
    final hasRefreshToken = await StorageService().getRefreshToken() != null;
    
    if (shouldRefresh && hasRefreshToken && !_isRefreshing) {
      _isRefreshing = true;
      final newToken = await _refreshAccessToken();
      _isRefreshing = false;
      
      if (newToken != null) {
        options.headers['Authorization'] = 'Bearer $newToken';
        _onTokenRefreshed(newToken);
      }
    }

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
      final refreshToken = await StorageService().getRefreshToken();
      
      if (refreshToken != null && !_isRefreshing) {
        _isRefreshing = true;
        final newToken = await _refreshAccessToken();
        _isRefreshing = false;
        
        if (newToken != null) {
          // 重试原请求
          err.requestOptions.headers['Authorization'] = 'Bearer $newToken';
          try {
            final response = await ApiClient().dio.fetch(err.requestOptions);
            handler.resolve(response);
            return;
          } catch (e) {
            await StorageService().clearAllTokens();
            NavigationService.instance.navigateToLogin();
          }
        }
      } else if (_isRefreshing) {
        // 如果正在刷新，订阅刷新完成后重试
        _subscribeTokenRefresh((token) async {
          err.requestOptions.headers['Authorization'] = 'Bearer $token';
          try {
            final response = await ApiClient().dio.fetch(err.requestOptions);
            handler.resolve(response);
          } catch (e) {
            handler.next(err);
          }
        });
        return;
      } else {
        await StorageService().clearAllTokens();
        NavigationService.instance.navigateToLogin();
      }
    }
    super.onError(err, handler);
  }
}
