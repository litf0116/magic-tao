import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/user_deposit_log_model.dart';

class DepositLogRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<UserDepositLogDto>> getMyDepositLogs({
    int? skipCount = 0,
    int? maxResultCount = 20,
    String? sorting,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUserDepositLog,
        queryParameters: {
          'SkipCount': skipCount,
          'MaxResultCount': maxResultCount,
          'Sorting': sorting ?? 'creationTime desc',
        },
      );

      final items = response.data['items'] as List? ?? [];
      return items.map((json) => UserDepositLogDto.fromJson(json)).toList();
    } on DioException catch (e) {
      throw Exception('Failed to get deposit logs: ${e.message}');
    }
  }
}
