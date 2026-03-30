import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/chat_message_model.dart';
import '../../data/repositories/chat_repository.dart';
import '../../data/services/websocket_service.dart';

/// 私聊状态
class PrivateChatState {
  final List<ChatMessage> messages;
  final bool isLoading;
  final bool isLoadingMore;
  final bool hasMoreHistory;
  final bool isConnected;
  final String? error;

  const PrivateChatState({
    this.messages = const [],
    this.isLoading = false,
    this.isLoadingMore = false,
    this.hasMoreHistory = true,
    this.isConnected = false,
    this.error,
  });

  PrivateChatState copyWith({
    List<ChatMessage>? messages,
    bool? isLoading,
    bool? isLoadingMore,
    bool? hasMoreHistory,
    bool? isConnected,
    String? error,
  }) {
    return PrivateChatState(
      messages: messages ?? this.messages,
      isLoading: isLoading ?? this.isLoading,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      hasMoreHistory: hasMoreHistory ?? this.hasMoreHistory,
      isConnected: isConnected ?? this.isConnected,
      error: error,
    );
  }
}

/// 私聊 Notifier
class PrivateChatNotifier extends StateNotifier<PrivateChatState> {
  final Ref _ref;
  final int _friendId;
  final String _friendName;
  final String? _friendAvatar;
  final ChatRepository _chatRepository = ChatRepository();
  StreamSubscription<Map<String, dynamic>>? _messageSubscription;

  PrivateChatNotifier(
    this._ref,
    this._friendId,
    this._friendName,
    this._friendAvatar,
  ) : super(const PrivateChatState()) {
    _initialize();
  }

  Future<void> _initialize() async {
    state = state.copyWith(isLoading: true);

    // 连接 WebSocket
    await _connectWebSocket();

    // 加载历史消息
    await loadHistoryMessages();
  }

  Future<void> _connectWebSocket() async {
    final webSocketService = _ref.read(webSocketServiceProvider);

    if (!webSocketService.isConnected) {
      await webSocketService.connect();
    }

    // 监听消息
    _messageSubscription = webSocketService.messageStream.listen(
      _handleIncomingMessage,
      onError: (error) {
        state = state.copyWith(error: 'WebSocket 错误: $error');
      },
    );

    state = state.copyWith(isConnected: webSocketService.isConnected);
  }

  void _handleIncomingMessage(Map<String, dynamic> message) {
    final target = message['target'] as String?;

    if (target == 'ReceiveMessage') {
      final arguments = message['arguments'] as List<dynamic>?;
      if (arguments != null && arguments.isNotEmpty) {
        final msgData = arguments.first as Map<String, dynamic>;
        final chatMessage = ChatMessage.fromJson(msgData);

        // 只处理与当前好友相关的消息
        if (chatMessage.from == _friendId || chatMessage.to == _friendId) {
          _addMessage(chatMessage);
        }
      }
    }
  }

  void _addMessage(ChatMessage message) {
    final messages = [...state.messages, message];
    state = state.copyWith(messages: messages);
  }

  /// 加载历史消息
  Future<void> loadHistoryMessages({int? lastMessageTime}) async {
    if (state.isLoadingMore || !state.hasMoreHistory) return;

    state = state.copyWith(isLoadingMore: true);

    try {
      // TODO: 从 API 获取历史消息
      // 当前简化实现，无本地缓存

      state = state.copyWith(
        isLoading: false,
        isLoadingMore: false,
        hasMoreHistory: false,
      );
    } catch (e) {
      state = state.copyWith(isLoadingMore: false, error: '加载历史消息失败: $e');
    }
  }

  /// 发送文本消息
  Future<bool> sendTextMessage(String text) async {
    if (text.trim().isEmpty) return false;

    try {
      // 创建本地消息
      final localMessage = ChatMessage(
        id: 'local_${DateTime.now().millisecondsSinceEpoch}',
        type: ChatMessageType.text,
        status: ChatMessageStatus.sending,
        from: 0, // 当前用户 ID，需要从用户状态获取
        fromName: '', // 当前用户名
        to: _friendId,
        msg: text,
        time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      );

      // 先添加到本地列表
      _addMessage(localMessage);

      // 发送到服务器
      final success = await _chatRepository.sendDirectMessage(
        toUserId: _friendId,
        message: text,
        type: ChatMessageType.text,
      );

      if (success) {
        // 更新消息状态为成功
        _updateMessageStatus(localMessage.id!, ChatMessageStatus.success);
      } else {
        _updateMessageStatus(localMessage.id!, ChatMessageStatus.fail);
      }

      return success;
    } catch (e) {
      state = state.copyWith(error: '发送消息失败: $e');
      return false;
    }
  }

  /// 发送图片消息
  Future<bool> sendImageMessage(String imageUrl) async {
    if (imageUrl.isEmpty) return false;

    try {
      // 创建本地消息
      final localMessage = ChatMessage(
        id: 'local_${DateTime.now().millisecondsSinceEpoch}',
        type: ChatMessageType.image,
        status: ChatMessageStatus.sending,
        from: 0,
        fromName: '',
        to: _friendId,
        msg: imageUrl,
        payload: {'url': imageUrl},
        time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      );

      _addMessage(localMessage);

      // 发送到服务器
      final success = await _chatRepository.sendDirectMessage(
        toUserId: _friendId,
        message: imageUrl,
        type: ChatMessageType.image,
      );

      if (success) {
        _updateMessageStatus(localMessage.id!, ChatMessageStatus.success);
      } else {
        _updateMessageStatus(localMessage.id!, ChatMessageStatus.fail);
      }

      return success;
    } catch (e) {
      state = state.copyWith(error: '发送图片失败: $e');
      return false;
    }
  }

  void _updateMessageStatus(String messageId, ChatMessageStatus status) {
    final messages = state.messages.map((msg) {
      if (msg.id == messageId) {
        return ChatMessage(
          id: msg.id,
          type: msg.type,
          status: status,
          chan: msg.chan,
          from: msg.from,
          fromName: msg.fromName,
          fromAdmin: msg.fromAdmin,
          fromTag: msg.fromTag,
          tagClass: msg.tagClass,
          avatar: msg.avatar,
          to: msg.to,
          time: msg.time,
          msg: msg.msg,
          payload: msg.payload,
          receipt: msg.receipt,
          sequenceNumber: msg.sequenceNumber,
        );
      }
      return msg;
    }).toList();

    state = state.copyWith(messages: messages);
  }

  /// 获取好友信息
  int get friendId => _friendId;
  String get friendName => _friendName;
  String? get friendAvatar => _friendAvatar;

  @override
  void dispose() {
    _messageSubscription?.cancel();
    super.dispose();
  }
}

/// WebSocket 服务 Provider
final webSocketServiceProvider = Provider<WebSocketService>((ref) {
  return WebSocketService();
});

/// 私聊 Provider 工厂
/// 使用 family 创建带参数的 provider
final privateChatProvider =
    StateNotifierProvider.family<
      PrivateChatNotifier,
      PrivateChatState,
      ({int friendId, String friendName, String? friendAvatar})
    >((ref, params) {
      return PrivateChatNotifier(
        ref,
        params.friendId,
        params.friendName,
        params.friendAvatar,
      );
    });
