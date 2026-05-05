import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/auction_item_model.dart';
import '../models/bid_history_model.dart';
import '../models/list_result.dart';

class AuctionRepository {
  final ApiClient _apiClient = ApiClient();

  Future<ListResult<AuctionItemDto>> getPublicAuctionList({
    int? skipCount = 0,
    int? maxResultCount = 10,
    int? status, // Add this
  }) async {
    try {
      final queryParameters = {
        'SkipCount': skipCount,
        'MaxResultCount': maxResultCount,
      };

      // Add status parameter if provided
      if (status != null) {
        queryParameters['Status'] = status;
      }

      final response = await _apiClient.dio.get(
        ApiEndpoints.getPublicAuctionList,
        queryParameters: queryParameters,
      );

      return ListResult<AuctionItemDto>.fromJson(
        response.data as Map<String, dynamic>,
        (json) => AuctionItemDto.fromJson(json),
      );
    } on DioException catch (e) {
      throw Exception('Failed to get public auction list: ${e.message}');
    }
  }

  Future<AuctionItemDto?> startAuction(int auctionItemId) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.startAuction,
        queryParameters: {'id': auctionItemId},
      );

      if (response.data != null) {
        return AuctionItemDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to start auction: ${e.message}');
    }
  }

  Future<AuctionItemDto?> endAuction(int auctionItemId) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.endAuction,
        queryParameters: {'id': auctionItemId},
      );

      if (response.data != null) {
        return AuctionItemDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to end auction: ${e.message}');
    }
  }

  Future<BidHistoryCreateDto?> placeBid({
    required int auctionItemId,
    required double bidPrice,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.bid,
        data: {'auctionItemId': auctionItemId, 'bidPrice': bidPrice.toInt()},
      );

      if (response.data != null) {
        return BidHistoryCreateDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to place bid: ${e.message}');
    }
  }

  Future<ListResult<AuctionItemDto>> getMySuccessList({
    int? skipCount = 0,
    int? maxResultCount = 10,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getMySuccessList,
        queryParameters: {
          'SkipCount': skipCount,
          'MaxResultCount': maxResultCount,
        },
      );

      return ListResult<AuctionItemDto>.fromJson(
        response.data as Map<String, dynamic>,
        (json) => AuctionItemDto.fromJson(json),
      );
    } on DioException catch (e) {
      throw Exception('Failed to get my success list: ${e.message}');
    }
  }

  Future<bool> subscribeStartNotification(
    int auctionItemId, {
    String platform = 'app',
    String? openid,
  }) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.subStartNotify,
        data: {
          'auctionItemId': auctionItemId,
          'platform': platform,
          if (openid != null) 'openid': openid,
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception(
        'Failed to subscribe to start notification: ${e.message}',
      );
    }
  }

  Future<ListResult<AuctionItemDto>> getAuctionMidList({
    int? skipCount = 0,
    int? maxResultCount = 10,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.getAuctionMidList,
        data: {'SkipCount': skipCount, 'MaxResultCount': maxResultCount},
      );

      return ListResult<AuctionItemDto>.fromJson(
        response.data as Map<String, dynamic>,
        (json) => AuctionItemDto.fromJson(json),
      );
    } on DioException catch (e) {
      throw Exception('Failed to get auction mid list: ${e.message}');
    }
  }

  Future<String?> getKasecStatus(int auctionItemId) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getKasecStatus,
        queryParameters: {'auctionItemId': auctionItemId},
      );

      return response.data?['status'];
    } on DioException catch (e) {
      throw Exception('Failed to get Kasec status: ${e.message}');
    }
  }

  Future<AuctionItemDto?> getAuctionDetail(int auctionItemId) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.getAuctionDetail,
        queryParameters: {'id': auctionItemId},
      );

      if (response.data != null) {
        return AuctionItemDto.fromJson(response.data);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('Failed to get auction detail: ${e.message}');
    }
  }
}
