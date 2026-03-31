import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/chat_message_model.dart';
import '../../data/repositories/chat_repository.dart';
import '../../data/services/storage_service.dart';
import '../../data/services/upload_service.dart';
import 'chat_provider.dart' show webSocketServiceProvider;

/// 群聊状态
class GroupChatState {
  final List<ChatMessage> messages;
  final bool isLoading;
  final bool isLoadingMore;
  final bool hasMoreHistory;
  final bool isConnected;
  final bool isJoined;
  final String? error;

  const GroupChatState({
    this.messages = const [],
    this.isLoading = false,
    this.isLoadingMore = false,
    this.hasMoreHistory = true,
    this.isConnected = false,
    this.isJoined = false,
    this.error,
  });

  GroupChatState copyWith({
    List<ChatMessage>? messages,
    bool? isLoading,
    bool? isLoadingMore,
    bool? hasMoreHistory,
    bool? isConnected,
    bool? isJoined,
    String? error,
  }) {
    return GroupChatState(
      messages: messages ?? this.messages,
      isLoading: isLoading ?? this.isLoading,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      hasMoreHistory: hasMoreHistory ?? this.hasMoreHistory,
      isConnected: isConnected ?? this.isConnected,
      isJoined: isJoined ?? this.isJoined,
      error: error,
    );
  }
}

/// 群聊 Notifier
class GroupChatNotifier extends StateNotifier<GroupChatState> {
  final Ref _ref;
  final String _channel;
  final int _channelId;
  final String _channelName;
  final ChatRepository _chatRepository = ChatRepository();
  final UploadService _uploadService = UploadService();
  StreamSubscription<Map<String, dynamic>>? _messageSubscription;

  GroupChatNotifier(
    this._ref,
    this._channel,
    this._channelId,
    this._channelName,
  ) : super(const GroupChatState()) {
    _initialize();
  }

  Future<void> _initialize() async {
    state = state.copyWith(isLoading: true);

    // 连接 WebSocket
    await _connectWebSocket();

    // 加入频道
    await _joinChannel();

    // 加载历史消息
    await loadHistoryMessages();
  }

  Future<void> _connectWebSocket() async {
    final webSocketService = _ref.read(webSocketServiceProvider);

    if (!webSocketService.isConnected) {
      // 获取 token
      final storageService = StorageService();
      final token = await storageService.getToken();

      if (token == null || token.isEmpty) {
        print('[GroupChat] 无 token，跳过 WebSocket 连接');
        return;
      }

      await webSocketService.connect(token: token);
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

  Future<void> _joinChannel() async {
    try {
      await _chatRepository.subscribeChannel(_channel);
      state = state.copyWith(isJoined: true);
    } catch (e) {
      state = state.copyWith(error: '加入频道失败: $e');
    }
  }

  void _handleIncomingMessage(Map<String, dynamic> message) {
    print('[GroupChat] 收到消息: $message');

    // 检查是否有 receipt（回执消息）
    final receipt = message['receipt'] as String?;
    if (receipt != null) {
      if (receipt == '用户不在线') {
        print('[GroupChat] 用户不在线');
        return;
      } else if (receipt == '发送成功') {
        print('[GroupChat] 发送成功');
        return;
      }
    }

    // 解析消息（与 UniApp onmessage 保持一致）
    final chatMessage = ChatMessage.fromJson(message);

    // 处理群聊消息（有 chan 字段）
    if (chatMessage.chan != null && chatMessage.chan!.isNotEmpty) {
      // 检查消息是否属于当前频道
      if (chatMessage.chan == _channel) {
        _addMessage(chatMessage);
      }
      return;
    }

    // 处理私聊消息（有 from 字段但没有 chan）
    if (chatMessage.from != null && chatMessage.type != null) {
      // 私聊消息不在群聊 provider 中处理
      return;
    }
  }

  void _addMessage(ChatMessage message) {
    final messages = [...state.messages, message];
    state = state.copyWith(messages: messages);
  }

  /// 加载历史消息
  Future<void> loadHistoryMessages({bool force = false}) async {
    if (state.isLoadingMore || !state.hasMoreHistory) return;

    state = state.copyWith(isLoadingMore: true);

    try {
      // TODO: 从 API 获取历史消息
      // final history = await _chatRepository.getChannelHistory(_channel, ...);

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
        chan: _channel,
        from: 0,
        fromName: '',
        msg: text,
        time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      );

      _addMessage(localMessage);

      // 发送到频道
      final success = await _chatRepository.sendChannelMessage(
        channel: _channel,
        message: text,
        type: ChatMessageType.text,
      );

      if (success) {
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
  Future<bool> sendImageMessage(String localPath) async {
    if (localPath.isEmpty) return false;

    try {
      // 创建本地消息（显示正在上传）
      final localMessage = ChatMessage(
        id: 'local_${DateTime.now().millisecondsSinceEpoch}',
        type: ChatMessageType.image,
        status: ChatMessageStatus.sending,
        chan: _channel,
        from: 0,
        fromName: '',
        msg: localPath, // 先使用本地路径
        payload: {'url': localPath},
        time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
      );

      _addMessage(localMessage);

      // 1. 上传图片到服务器
      final imageUrl = await _uploadService.uploadImage(localPath);
      if (imageUrl == null) {
        _updateMessageStatus(localMessage.id!, ChatMessageStatus.fail);
        state = state.copyWith(error: '图片上传失败');
        return false;
      }

      // 2. 更新本地消息的图片URL
      _updateMessageContent(localMessage.id!, imageUrl);

      // 3. 发送消息到频道
      final success = await _chatRepository.sendChannelMessage(
        channel: _channel,
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

  void _updateMessageContent(String messageId, String newContent) {
    final messages = state.messages.map((msg) {
      if (msg.id == messageId) {
        return ChatMessage(
          id: msg.id,
          type: msg.type,
          status: msg.status,
          chan: msg.chan,
          from: msg.from,
          fromName: msg.fromName,
          fromAdmin: msg.fromAdmin,
          fromTag: msg.fromTag,
          tagClass: msg.tagClass,
          avatar: msg.avatar,
          to: msg.to,
          time: msg.time,
          msg: newContent,
          payload: {'url': newContent},
          receipt: msg.receipt,
          sequenceNumber: msg.sequenceNumber,
        );
      }
      return msg;
    }).toList();

    state = state.copyWith(messages: messages);
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

  /// 离开频道
  Future<void> leaveChannel() async {
    try {
      await _chatRepository.leaveChannel(_channel);
      state = state.copyWith(isJoined: false);
    } catch (e) {
      // 静默失败
    }
  }

  /// 获取频道信息
  String get channel => _channel;
  int get channelId => _channelId;
  String get channelName => _channelName;

  @override
  void dispose() {
    _messageSubscription?.cancel();
    leaveChannel();
    super.dispose();
  }
}

/// 群聊 Provider 工厂
final groupChatProvider =
    StateNotifierProvider.family<
      GroupChatNotifier,
      GroupChatState,
      ({String channel, int channelId, String channelName})
    >((ref, params) {
      return GroupChatNotifier(
        ref,
        params.channel,
        params.channelId,
        params.channelName,
      );
    });
