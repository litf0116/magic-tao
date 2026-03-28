import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';
import '../models/chat_message_model.dart';
import '../models/chat_list_item_model.dart';

class ChatRepository {
  final ApiClient _apiClient = ApiClient();

  Future<bool> sendChannelMessage({
    required String channel,
    required String message,
    ChatMessageType? type,
  }) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.sendChannelMsg,
        data: {
          'channel': channel,
          'message': message,
          'type': type != null ? _chatMessageTypeToString(type) : 'Text',
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to send channel message: ${e.message}');
    }
  }

  Future<bool> sendDirectMessage({
    required int toUserId,
    required String message,
    ChatMessageType? type,
  }) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.sendMsg,
        data: {
          'to': toUserId,
          'message': message,
          'type': type != null ? _chatMessageTypeToString(type) : 'Text',
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to send direct message: ${e.message}');
    }
  }

  Future<bool> preConnect() async {
    try {
      await _apiClient.dio.post(ApiEndpoints.preConnect);
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to pre-connect: ${e.message}');
    }
  }

  Future<List<ChatMessage>?> getOfflineMessages() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.offline);
      if (response.data != null && response.data['messages'] != null) {
        return (response.data['messages'] as List)
            .map((json) => ChatMessage.fromJson(json))
            .toList();
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get offline messages: ${e.message}');
    }
  }

  Future<List<String>?> getChannels() async {
    try {
      final response = await _apiClient.dio.post(ApiEndpoints.getChannels);
      if (response.data != null && response.data['channels'] != null) {
        return (response.data['channels'] as List)
            .map((e) => e.toString())
            .toList();
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get channels: ${e.message}');
    }
  }

  Future<bool> backoutMessage(String messageId) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.backout,
        data: {'messageId': messageId},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to backout message: ${e.message}');
    }
  }

  Future<bool> leaveChannel(String channel) async {
    try {
      await _apiClient.dio.get(
        ApiEndpoints.leaveChannel,
        queryParameters: {'channel': channel},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to leave channel: ${e.message}');
    }
  }

  Future<bool> subscribeChannel(String channel) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.subChannel,
        data: {'channel': channel},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to subscribe to channel: ${e.message}');
    }
  }

  Future<bool> deleteChannel(String channel) async {
    try {
      await _apiClient.dio.get(
        ApiEndpoints.delChannel,
        queryParameters: {'channel': channel},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to delete channel: ${e.message}');
    }
  }

  Future<bool> banUser({
    required int userId,
    required String channel,
    required int durationMinutes,
  }) async {
    try {
      await _apiClient.dio.post(
        ApiEndpoints.banUser,
        data: {
          'userId': userId,
          'channel': channel,
          'duration': durationMinutes,
        },
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to ban user: ${e.message}');
    }
  }

  Future<List<ChatListItem>?> getChatList() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getChatList);
      if (response.data != null && response.data['items'] != null) {
        return (response.data['items'] as List)
            .map((json) => ChatListItem.fromJson(json))
            .toList();
      }
      return [];
    } on DioException catch (e) {
      throw Exception('Failed to get chat list: ${e.message}');
    }
  }

  Future<bool> deleteChatList(int chatId) async {
    try {
      await _apiClient.dio.get(
        ApiEndpoints.deleteChatList,
        queryParameters: {'id': chatId},
      );
      return true;
    } on DioException catch (e) {
      throw Exception('Failed to delete chat list: ${e.message}');
    }
  }

  static String _chatMessageTypeToString(ChatMessageType type) {
    switch (type) {
      case ChatMessageType.text:
        return 'Text';
      case ChatMessageType.image:
        return 'Image';
      case ChatMessageType.file:
        return 'File';
      case ChatMessageType.receipt:
        return 'Receipt';
      case ChatMessageType.welcome:
        return 'Welcome';
      case ChatMessageType.goodbye:
        return 'Goodbye';
      case ChatMessageType.banUser:
        return 'BanUser';
      case ChatMessageType.backout:
        return 'Backout';
      case ChatMessageType.auctionStart:
        return 'AuctionStart';
      case ChatMessageType.auctionBid:
        return 'AuctionBid';
      case ChatMessageType.auctionEnd:
        return 'AuctionEnd';
      case ChatMessageType.auctionDeal:
        return 'AuctionDeal';
      case ChatMessageType.error:
        return 'Error';
      case ChatMessageType.kasecStatusChanged:
        return 'KasecStatusChanged';
    }
  }
}
