import "package:dio/dio.dart";
import 'package:molitao_app/data/api/api_client.dart';
import 'package:molitao_app/data/api/api_endpoints.dart';

class BlockedUserRepository {
  final ApiClient _apiClient = ApiClient();

  Future<bool> blockUser(final int blockedUserId, {final String? reason}) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.blockedUser,
        data: {"blockedUserId": blockedUserId, "reason": reason},
      );
      return true;
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to block user: ${e.message}");
    }
  }

  Future<bool> unblockUser(final int id) async {
    try {
      await _apiClient.dio.delete(
        ApiEndpoints.blockedUser,
        queryParameters: {"id": id},
      );
      return true;
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to unblock user: ${e.message}");
    }
  }

  Future<List<BlockedUserDto>> getBlockedList() async {
    try {
      var response = await _apiClient.dio.get(
        ApiEndpoints.blockedUserGetAll,
      );
      var items = response.data["items"] as List? ?? [];
      return items.map((final json) => BlockedUserDto.fromJson(json)).toList();
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to get blocked list: ${e.message}");
    }
  }

  Future<bool> checkBlocked(final int userId) async {
    try {
      var response = await _apiClient.dio.get(
        ApiEndpoints.blockedUserCheck,
        queryParameters: {"blockedUserId": userId},
      );
      final result = response.data?["result"];
      if (result is Map) {
        return result["isBlocked"] as bool? ?? false;
      }
      return false;
    } on DioException catch (e) {
      var errorMessage = e.response?.data?["error"]?["message"];
      if (errorMessage != null) {
        throw Exception(errorMessage);
      }
      throw Exception("Failed to check blocked status: ${e.message}");
    }
  }
}

class BlockedUserDto {
  final int id;
  final int blockedUserId;
  final String? blockedUserName;
  final String? blockedUserAvatar;
  final String? reason;
  final DateTime creationTime;

  BlockedUserDto({
    required this.id,
    required this.blockedUserId,
    this.blockedUserName,
    this.blockedUserAvatar,
    this.reason,
    required this.creationTime,
  });

  factory BlockedUserDto.fromJson(final Map<String, dynamic> json) {
    return BlockedUserDto(
      id: (json["id"] as num?)?.toInt() ?? 0,
      blockedUserId: (json["blockedUserId"] as num?)?.toInt() ?? 0,
      blockedUserName: json["blockedUserName"] as String?,
      blockedUserAvatar: json["blockedUserAvatar"] as String?,
      reason: json["reason"] as String?,
      creationTime: _parseDateTime(json["creationTime"]),
    );
  }

  /// 安全解析时间字符串，后端未填充时返回 epoch
  static DateTime _parseDateTime(dynamic raw) {
    if (raw == null) return DateTime.fromMillisecondsSinceEpoch(0);
    if (raw is DateTime) return raw;
    final s = raw.toString();
    if (s.isEmpty || s.startsWith("0001-01-01")) {
      return DateTime.fromMillisecondsSinceEpoch(0);
    }
    return DateTime.tryParse(s) ?? DateTime.fromMillisecondsSinceEpoch(0);
  }
}
