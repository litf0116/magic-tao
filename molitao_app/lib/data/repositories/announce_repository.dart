import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/announce_model.dart';

class AnnounceRepository {
  final ApiClient _apiClient = ApiClient();

  Future<List<AnnounceDto>> getAllPublicAnnounces({int? categoryId}) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getAllPublicAnnounce,
        queryParameters: categoryId != null ? {'Pid': categoryId} : null,
      );

      final items = response.data['items'] as List? ?? [];
      return items.map((json) => AnnounceDto.fromJson(json)).toList();
    } on DioException catch (e) {
      throw Exception('Failed to get announces: ${e.message}');
    }
  }

  Future<AnnounceDto?> getLatestAnnounce({int? categoryId}) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getLatestAnnounce,
        queryParameters: categoryId != null ? {'id': categoryId} : null,
      );
      if (response.data != null) {
        return AnnounceDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get latest announce: ${e.message}');
    }
  }
}
