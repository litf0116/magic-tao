import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_balance_log_model.dart';

class BalanceLogRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<UserBalanceLogDto>> getMyBalanceLogs({
    int? skipCount = 0,
    int? maxResultCount = 20,
    String? sorting,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUserBalanceLog,
        queryParameters: {
          'SkipCount': skipCount,
          'MaxResultCount': maxResultCount,
          'Sorting': sorting ?? 'creationTime desc',
        },
      );

      final items = response.data['items'] as List? ?? [];
      return items.map((json) => UserBalanceLogDto.fromJson(json)).toList();
    } on DioException catch (e) {
      throw Exception('Failed to get balance logs: ${e.message}');
    }
  }
}
