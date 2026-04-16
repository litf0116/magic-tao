import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/chat_emoji_model.dart';

/// 收藏表情仓库
class ChatEmojiRepository {
  final Dio _dio = ApiClient().dio;

  /// 获取用户所有收藏的表情
  Future<List<ChatEmojiDto>> getAll() async {
    try {
      final response = await _dio.get(ApiEndpoints.getAllChatEmoji);
      if (response.data != null && response.data is Map) {
        final items = response.data['items'] as List?;
        if (items != null) {
          return items
              .map(
                (item) => ChatEmojiDto.fromJson(item as Map<String, dynamic>),
              )
              .toList();
        }
      }
      return [];
    } catch (e) {
      debugPrint('[ChatEmojiRepository] 获取收藏表情失败: $e');
      return [];
    }
  }

  /// 添加收藏表情
  Future<ChatEmojiDto?> create(String url) async {
    try {
      final response = await _dio.post(
        ApiEndpoints.createChatEmoji,
        data: {'url': url},
      );
      if (response.data != null) {
        return ChatEmojiDto.fromJson(response.data as Map<String, dynamic>);
      }
      return null;
    } catch (e) {
      debugPrint('[ChatEmojiRepository] 添加收藏表情失败: $e');
      rethrow;
    }
  }

  /// 删除收藏表情
  Future<bool> delete(int id) async {
    try {
      await _dio.delete('${ApiEndpoints.deleteChatEmoji}?id=$id');
      return true;
    } catch (e) {
      debugPrint('[ChatEmojiRepository] 删除收藏表情失败: $e');
      return false;
    }
  }
}
