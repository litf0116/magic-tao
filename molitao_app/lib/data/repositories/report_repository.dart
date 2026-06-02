import "package:dio/dio.dart";
import 'package:molitao_app/data/api/api_client.dart';
import 'package:molitao_app/data/api/api_endpoints.dart';

class ReportRepository {
  final ApiClient _apiClient = ApiClient();

  /// Create a user report
  Future<bool> createReport({
    required int messageId,
    required int reportedUserId,
    required String chan,
    required String reason,
    String? evidence,
  }) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.userReport,
        data: {
          "messageId": messageId,
          "reportedUserId": reportedUserId,
          "chan": chan,
          "reason": reason,
          if (evidence != null) "evidence": evidence,
        },
      );
      return true;
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to create report: ${e.message}");
    }
  }

  /// Get my report history with pagination
  Future<List<Map<String, dynamic>>> getMyReports({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      var response = await _apiClient.dio.get(
        ApiEndpoints.userReport,
        queryParameters: {"page": page, "pageSize": pageSize},
      );
      var items = response.data["items"] as List? ?? [];
      return items.cast<Map<String, dynamic>>();
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to get my reports: ${e.message}");
    }
  }
}