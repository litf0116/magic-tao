import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import '../../data/services/websocket_service.dart';
import '../../data/api/api_client.dart';
import '../../data/api/api_endpoints.dart';
import '../../data/services/storage_service.dart';

// Chat list item type enum
enum ChatListItemType { group, user, system }

// Chat message type enum
enum ChatMessageType { text, image, file }

// Chat message status enum
enum ChatMessageStatus { sending, sent, delivered, read }

// Chat list item model
class ChatListItem {
  final int? id;
  final String name;
  final ChatListItemType type;
  final int? time;
  final String? avatar;
  final String? lastMsg;
  final int unread;
  final int order;

  ChatListItem({
    this.id,
    required this.name,
    required this.type,
    this.time,
    this.avatar,
    this.lastMsg,
    required this.unread,
    required this.order,
  });

  factory ChatListItem.fromJson(Map<String, dynamic> json) {
    return ChatListItem(
      id: json['id'],
      name: json['name'] ?? '',
      type: ChatListItemType.values[json['type'] ?? 1], // default to user
      time: json['time'],
      avatar: json['avatar'],
      lastMsg: json['lastMsg'],
      unread: json['unread'] ?? 0,
      order: json['order'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'type': type.index,
      'time': time,
      'avatar': avatar,
      'lastMsg': lastMsg,
      'unread': unread,
      'order': order,
    };
  }
}

// Chat message model
class ChatMessage {
  final String? id;
  final ChatMessageType? type;
  final ChatMessageStatus? status;
  final String? chan;
  final int? from;
  final String? fromName;
  final bool? fromAdmin;
  final String? msg;
  final int? time;
  final String? avatar;
  final dynamic payload;

  ChatMessage({
    this.id,
    this.type,
    this.status,
    this.chan,
    this.from,
    this.fromName,
    this.fromAdmin,
    this.msg,
    this.time,
    this.avatar,
    this.payload,
  });

  factory ChatMessage.fromJson(Map<String, dynamic> json) {
    return ChatMessage(
      id: json['id'],
      type: json['type'] != null ? ChatMessageType.values[json['type']] : null,
      status: json['status'] != null
          ? ChatMessageStatus.values[json['status']]
          : null,
      chan: json['chan'],
      from: json['from'],
      fromName: json['fromName'],
      fromAdmin: json['fromAdmin'],
      msg: json['msg'],
      time: json['time'],
      avatar: json['avatar'],
      payload: json['payload'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'type': type?.index,
      'status': status?.index,
      'chan': chan,
      'from': from,
      'fromName': fromName,
      'fromAdmin': fromAdmin,
      'msg': msg,
      'time': time,
      'avatar': avatar,
      'payload': payload,
    };
  }
}

// Chat state
class ChatState {
  final List<ChatListItem> chatList;
  final bool isWebSocketConnected;
  final int unreadCount;
  final bool isLoading;

  ChatState({
    required this.chatList,
    required this.isWebSocketConnected,
    required this.unreadCount,
    required this.isLoading,
  });

  ChatState copyWith({
    List<ChatListItem>? chatList,
    bool? isWebSocketConnected,
    int? unreadCount,
    bool? isLoading,
  }) {
    return ChatState(
      chatList: chatList ?? this.chatList,
      isWebSocketConnected: isWebSocketConnected ?? this.isWebSocketConnected,
      unreadCount: unreadCount ?? this.unreadCount,
      isLoading: isLoading ?? this.isLoading,
    );
  }

  factory ChatState.initial() {
    return ChatState(
      chatList: [],
      isWebSocketConnected: false,
      unreadCount: 0,
      isLoading: false,
    );
  }
}

// WebSocket service provider
final webSocketServiceProvider = Provider<WebSocketService>((ref) {
  return WebSocketService();
});

// Chat notifier
class ChatNotifier extends StateNotifier<ChatState> {
  final Ref _ref;
  StreamSubscription<Map<String, dynamic>>? _messageSubscription;

  ChatNotifier(this._ref) : super(ChatState.initial()) {
    _initializeWebSocket();
  }

  Future<void> _initializeWebSocket() async {
    final webSocketService = _ref.read(webSocketServiceProvider);

    // Listen to WebSocket messages
    _messageSubscription = webSocketService.messageStream.listen(
      (message) {
        _handleIncomingMessage(message);
      },
      onError: (error) {
        print('WebSocket error in chat: $error');
      },
    );
  }

  void _handleIncomingMessage(Map<String, dynamic> message) {
    final target = message['target'];

    if (target == 'ReceiveMessage' || target == 'ReceiveChannelMessage') {
      final arguments = message['arguments'] as List<dynamic>?;
      if (arguments != null && arguments.isNotEmpty) {
        final chatMessage = ChatMessage.fromJson(
          arguments.first as Map<String, dynamic>,
        );

        // Update chat list with new message
        final updatedChatList = [...state.chatList];
        final existingIndex = updatedChatList.indexWhere(
          (item) => item.id == chatMessage.from,
        );

        if (existingIndex != -1) {
          final existingItem = updatedChatList[existingIndex];
          updatedChatList[existingIndex] = existingItem.copyWith(
            lastMsg: chatMessage.msg,
            time: chatMessage.time,
            unread: existingItem.unread + 1,
          );
        } else {
          // Add new chat item if not exists
          updatedChatList.add(
            ChatListItem(
              id: chatMessage.from,
              name: chatMessage.fromName ?? 'Unknown',
              type: ChatListItemType.user,
              time: chatMessage.time,
              avatar: chatMessage.avatar,
              lastMsg: chatMessage.msg,
              unread: 1,
              order: DateTime.now().millisecondsSinceEpoch,
            ),
          );
        }

        // Calculate total unread count
        final totalUnread = updatedChatList.fold(
          0,
          (sum, item) => sum + item.unread,
        );

        state = state.copyWith(
          chatList: updatedChatList,
          unreadCount: totalUnread,
        );
      }
    }
  }

  Future<void> connectWebSocket() async {
    final webSocketService = _ref.read(webSocketServiceProvider);
    await webSocketService.connect();

    state = state.copyWith(isWebSocketConnected: webSocketService.isConnected);
  }

  Future<void> disconnectWebSocket() async {
    final webSocketService = _ref.read(webSocketServiceProvider);
    await webSocketService.disconnect();

    state = state.copyWith(isWebSocketConnected: false);
  }

  Future<void> loadChatList() async {
    state = state.copyWith(isLoading: true);

    try {
      final storageService = StorageService();
      final token = await storageService.getToken();

      if (token == null || token.isEmpty) {
        state = state.copyWith(chatList: [], isLoading: false);
        return;
      }

      final response = await ApiClient().dio.get(ApiEndpoints.getChatList);

      if (response.data != null && response.data is List) {
        final chatList = (response.data as List)
            .map((json) => ChatListItem.fromJson(json))
            .toList();

        // Sort by order descending
        chatList.sort((a, b) => b.order.compareTo(a.order));

        final totalUnread = chatList.fold(0, (sum, item) => sum + item.unread);

        state = state.copyWith(
          chatList: chatList,
          unreadCount: totalUnread,
          isLoading: false,
        );
      } else {
        state = state.copyWith(chatList: [], isLoading: false);
      }
    } catch (e) {
      print('Error loading chat list: $e');
      state = state.copyWith(chatList: [], isLoading: false);
    }
  }

  Future<void> deleteChat(int chatId) async {
    try {
      // Call API to delete chat
      await ApiClient().dio.delete('${ApiEndpoints.deleteChatList}/$chatId');

      // Remove from local state
      final updatedChatList = state.chatList
          .where((item) => item.id != chatId)
          .toList();
      final totalUnread = updatedChatList.fold(
        0,
        (sum, item) => sum + item.unread,
      );

      state = state.copyWith(
        chatList: updatedChatList,
        unreadCount: totalUnread,
      );
    } catch (e) {
      print('Error deleting chat: $e');
      // Still remove from local state even if API fails
      final updatedChatList = state.chatList
          .where((item) => item.id != chatId)
          .toList();
      final totalUnread = updatedChatList.fold(
        0,
        (sum, item) => sum + item.unread,
      );

      state = state.copyWith(
        chatList: updatedChatList,
        unreadCount: totalUnread,
      );
    }
  }

  Future<void> markAsRead(int chatId) async {
    final updatedChatList = [...state.chatList];
    final index = updatedChatList.indexWhere((item) => item.id == chatId);

    if (index != -1) {
      updatedChatList[index] = updatedChatList[index].copyWith(unread: 0);
      final totalUnread = updatedChatList.fold(
        0,
        (sum, item) => sum + item.unread,
      );

      state = state.copyWith(
        chatList: updatedChatList,
        unreadCount: totalUnread,
      );
    }
  }

  ChatListItem? getChatById(int chatId) {
    return state.chatList.firstWhere(
      (item) => item.id == chatId,
      orElse: () => state.chatList.first,
    );
  }

  @override
  void dispose() {
    _messageSubscription?.cancel();
    final webSocketService = _ref.read(webSocketServiceProvider);
    webSocketService.dispose();
    super.dispose();
  }
}

// Extension to allow copying ChatListItem with updated values
extension ChatListItemExtension on ChatListItem {
  ChatListItem copyWith({
    int? id,
    String? name,
    ChatListItemType? type,
    int? time,
    String? avatar,
    String? lastMsg,
    int? unread,
    int? order,
  }) {
    return ChatListItem(
      id: id ?? this.id,
      name: name ?? this.name,
      type: type ?? this.type,
      time: time ?? this.time,
      avatar: avatar ?? this.avatar,
      lastMsg: lastMsg ?? this.lastMsg,
      unread: unread ?? this.unread,
      order: order ?? this.order,
    );
  }
}

// Chat provider
final chatProvider = StateNotifierProvider<ChatNotifier, ChatState>((ref) {
  return ChatNotifier(ref);
});
