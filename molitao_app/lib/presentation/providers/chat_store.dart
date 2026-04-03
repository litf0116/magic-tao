import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/chat_message_model.dart';
import '../../data/models/chat_list_item_model.dart';
import '../../data/services/websocket_service.dart';
import '../../data/services/storage_service.dart';
import '../../data/services/sound_service.dart';
import '../../data/repositories/chat_repository.dart';
import 'user_provider.dart';

/// 聊天状态 - 与 UniApp chatStore 保持一致
class ChatState {
  /// 聊天列表
  final List<ChatListItem> chatList;

  /// 聊天消息映射 - key 是聊天 ID
  /// 与 UniApp chatMap 一致
  final Map<String, List<ChatMessage>> chatMap;

  /// 当前聊天
  final ChatListItem currentChat;

  /// WebSocket ID
  final int websocketId;

  /// 是否已连接
  final bool isConnected;

  /// 未读消息数
  final int unreadCount;

  const ChatState({
    this.chatList = const [],
    this.chatMap = const {},
    this.currentChat = const ChatListItem(
      id: -1,
      name: 'auction',
      type: ChatListItemType.group,
      unread: 0,
      order: 99,
    ),
    this.websocketId = 0,
    this.isConnected = false,
    this.unreadCount = 0,
  });

  ChatState copyWith({
    List<ChatListItem>? chatList,
    Map<String, List<ChatMessage>>? chatMap,
    ChatListItem? currentChat,
    int? websocketId,
    bool? isConnected,
    int? unreadCount,
  }) {
    return ChatState(
      chatList: chatList ?? this.chatList,
      chatMap: chatMap ?? this.chatMap,
      currentChat: currentChat ?? this.currentChat,
      websocketId: websocketId ?? this.websocketId,
      isConnected: isConnected ?? this.isConnected,
      unreadCount: unreadCount ?? this.unreadCount,
    );
  }
}

/// 聊天 Store - 与 UniApp chatStore 保持一致
class ChatStore extends StateNotifier<ChatState> {
  final Ref _ref;
  final WebSocketService _webSocketService = WebSocketService();
  final ChatRepository _chatRepository = ChatRepository();
  final SoundService _soundService = SoundService();
  StreamSubscription<Map<String, dynamic>>? _messageSubscription;

  ChatStore(this._ref) : super(const ChatState()) {
    _initialize();
  }

  /// 初始化
  Future<void> _initialize() async {
    // 加载聊天列表
    await getChatList();
  }

  /// 连接 WebSocket - 与 UniApp connectServer 一致
  Future<void> connectServer({bool reconnect = false}) async {
    if (state.isConnected && !reconnect) {
      return;
    }

    final storageService = StorageService();
    final token = await storageService.getToken();

    if (token == null || token.isEmpty) {
      print('[ChatStore] 无 token，跳过连接');
      return;
    }

    // 调用 pre-connect 获取 websocketId
    try {
      final preConnectResult = await _chatRepository.preConnect();
      if (preConnectResult != null) {
        final websocketId = preConnectResult['websocketId'] as int?;
        if (websocketId != null) {
          state = state.copyWith(websocketId: websocketId);
        }
      }
    } catch (e) {
      print('[ChatStore] pre-connect 失败: $e');
    }

    // 连接 WebSocket
    await _webSocketService.connect(token: token);

    // 监听消息
    _messageSubscription?.cancel();
    _messageSubscription = _webSocketService.messageStream.listen(
      _onMessage,
      onError: (error) {
        print('[ChatStore] WebSocket 错误: $error');
        state = state.copyWith(isConnected: false);
      },
    );

    state = state.copyWith(isConnected: _webSocketService.isConnected);
  }

  /// 断开连接
  Future<void> disconnect() async {
    await _webSocketService.disconnect();
    _messageSubscription?.cancel();
    state = state.copyWith(isConnected: false);
  }

  /// 处理收到的消息 - 与 UniApp onmessage 一致
  void _onMessage(Map<String, dynamic> msgData) {
    print('[ChatStore] 收到原始消息: $msgData');

    final msg = ChatMessage.fromJson(msgData);
    print(
      '[ChatStore] 解析后消息: type=${msg.type}, msg=${msg.msg}, fromName=${msg.fromName}, avatar=${msg.avatar}',
    );

    // 处理回执消息
    if (msg.receipt != null) {
      if (msg.receipt == '用户不在线' || msg.receipt == '发送成功') {
        return;
      }
    }

    // 播放消息声音（与 UniApp App.vue 一致）
    _playMessageSound(msg);

    // 处理群聊消息（有 chan 字段）
    if (msg.chan != null && msg.chan!.isNotEmpty) {
      _handleChannelMessage(msg);
      return;
    }

    // 处理私聊消息（有 from 字段但没有 chan）
    if (msg.from != null) {
      _handlePrivateMessage(msg);
    }
  }

  /// 播放消息声音 - 与 UniApp App.vue 一致
  void _playMessageSound(ChatMessage msg) {
    final userState = _ref.read(userProvider);
    final currentUserId = userState.user?.id;

    // 如果是自己发送的消息，不播放声音
    if (msg.from != null && msg.from == currentUserId) {
      return;
    }

    // Welcome 消息（用户进入群聊）- 非 lobby 和 auction 频道
    if (msg.type == ChatMessageType.welcome) {
      final chan = msg.chan ?? '';
      if (chan != '0_lobby' && chan != '-1_auction') {
        _soundService.playWelcomeSound();
      }
      return;
    }

    // 私聊消息（Text/Image）
    if ((msg.type == ChatMessageType.text ||
            msg.type == ChatMessageType.image) &&
        msg.chan == null) {
      _soundService.playMessageSound();
      return;
    }

    // 拍卖成交消息
    if (msg.type == ChatMessageType.auctionEnd) {
      _soundService.playAuctionEndSound();
      return;
    }
  }

  /// 处理群聊消息 - 与 UniApp chatStore.onmessage 一致
  void _handleChannelMessage(ChatMessage msg) {
    final t = msg.chan!.split('_');
    int? id = int.tryParse(t[0]);
    if (id == null) return;

    // 转换 ID（与 UniApp 一致）
    if (id > 0) id = -id;

    final key = '$id';
    final name = t.length > 1 ? t[1] : '';

    // 更新 chatList
    final existingItem = state.chatList.firstWhere(
      (item) => item.id == id && item.id != state.currentChat.id,
      orElse: () => const ChatListItem(
        id: 0,
        name: '',
        type: ChatListItemType.group,
        unread: 0,
        order: 0,
      ),
    );

    final updatedChatList = [...state.chatList];
    final existingIndex = updatedChatList.indexWhere((item) => item.id == id);

    final newItem = ChatListItem(
      id: id,
      name: name,
      type: ChatListItemType.group,
      time: msg.time,
      lastMsg: msg.msg,
      avatar: msg.avatar,
      unread: existingItem.id != 0 ? existingItem.unread + 1 : 0,
      order: id == 0
          ? 100
          : id == -1
          ? 99
          : 0,
    );

    if (existingIndex >= 0) {
      updatedChatList[existingIndex] = newItem;
    } else {
      updatedChatList.insert(0, newItem);
    }

    // 更新 chatMap
    final updatedChatMap = Map<String, List<ChatMessage>>.from(state.chatMap);
    final existingMessages = updatedChatMap[key] ?? [];
    final updatedMessages = [...existingMessages, msg];

    // 限制消息数量（与 UniApp 一致，最多 800 条）
    if (updatedMessages.length > 800) {
      updatedMessages.removeRange(0, updatedMessages.length - 750);
    }

    updatedChatMap[key] = updatedMessages;

    // 更新未读数
    final totalUnread = updatedChatList.fold(
      0,
      (sum, item) => sum + item.unread,
    );

    state = state.copyWith(
      chatList: updatedChatList,
      chatMap: updatedChatMap,
      unreadCount: totalUnread,
    );
  }

  /// 处理私聊消息 - 与 UniApp chatStore.onmessage 一致
  void _handlePrivateMessage(ChatMessage msg) {
    if (msg.from == null) return;

    final key = '${msg.from}';

    // 更新 chatList
    final existingItem = state.chatList.firstWhere(
      (item) => item.id == msg.from && item.id != state.currentChat.id,
      orElse: () => const ChatListItem(
        id: 0,
        name: '',
        type: ChatListItemType.user,
        unread: 0,
        order: 0,
      ),
    );

    final updatedChatList = [...state.chatList];
    final existingIndex = updatedChatList.indexWhere(
      (item) => item.id == msg.from,
    );

    final newItem = ChatListItem(
      id: msg.from,
      name: msg.fromName ?? '',
      type: ChatListItemType.user,
      time: msg.time,
      lastMsg: msg.msg,
      avatar: msg.avatar,
      unread: existingItem.id != 0 ? existingItem.unread + 1 : 0,
      order: 0,
    );

    if (existingIndex >= 0) {
      updatedChatList[existingIndex] = newItem;
    } else {
      updatedChatList.insert(0, newItem);
    }

    // 更新 chatMap
    final updatedChatMap = Map<String, List<ChatMessage>>.from(state.chatMap);
    final existingMessages = updatedChatMap[key] ?? [];
    updatedChatMap[key] = [...existingMessages, msg];

    // 更新未读数
    final totalUnread = updatedChatList.fold(
      0,
      (sum, item) => sum + item.unread,
    );

    state = state.copyWith(
      chatList: updatedChatList,
      chatMap: updatedChatMap,
      unreadCount: totalUnread,
    );
  }

  /// 获取聊天列表 - 与 UniApp getChatList 一致
  Future<void> getChatList() async {
    try {
      final chatList = await _chatRepository.getChatList();
      final totalUnread = chatList.fold(0, (sum, item) => sum + item.unread);
      state = state.copyWith(chatList: chatList, unreadCount: totalUnread);
    } catch (e) {
      print('[ChatStore] 获取聊天列表失败: $e');
    }
  }

  /// 设置当前聊天 - 与 UniApp SetCurrentChatId 一致
  void setCurrentChatId(int id, {String name = '', bool isGroup = true}) {
    final item = state.chatList.firstWhere(
      (item) => item.id == id,
      orElse: () => ChatListItem(
        id: id,
        name: name,
        type: isGroup ? ChatListItemType.group : ChatListItemType.user,
        time: DateTime.now().millisecondsSinceEpoch,
        unread: 0,
        order: 0,
      ),
    );

    state = state.copyWith(currentChat: item);
  }

  /// 加入频道 - 与 UniApp joinChannel 一致
  Future<void> joinChannel(String channel) async {
    try {
      await _chatRepository.subscribeChannel(channel);
    } catch (e) {
      print('[ChatStore] 加入频道失败: $e');
    }
  }

  /// 离开频道 - 与 UniApp leaveChannel 一致
  Future<void> leaveChannel(String channel) async {
    try {
      await _chatRepository.leaveChannel(channel);
    } catch (e) {
      print('[ChatStore] 离开频道失败: $e');
    }
  }

  /// 发送频道消息 - 与 UniApp sendChannelMsg 一致
  Future<bool> sendChannelMsg({
    required String channel,
    required String message,
    ChatMessageType type = ChatMessageType.text,
    Map<String, dynamic>? payload,
  }) async {
    try {
      // 获取当前用户信息
      final userState = _ref.read(userProvider);
      final user = userState.user;

      await _chatRepository.sendChannelMessage(
        channel: channel,
        message: message,
        type: type,
        from: user?.id?.toInt() ?? state.websocketId,
        fromName: user?.fullName ?? user?.userName,
        avatar: user?.headImgUrl,
        payload: payload,
      );
      return true;
    } catch (e) {
      print('[ChatStore] 发送频道消息失败: $e');
      return false;
    }
  }

  /// 发送私聊消息 - 与 UniApp sendMsg 一致
  Future<bool> sendDirectMsg({
    required int toUserId,
    required String message,
    ChatMessageType type = ChatMessageType.text,
    Map<String, dynamic>? payload,
  }) async {
    try {
      // 获取当前用户信息
      final userState = _ref.read(userProvider);
      final user = userState.user;

      await _chatRepository.sendDirectMessage(
        toUserId: toUserId,
        message: message,
        type: type,
        from: user?.id?.toInt() ?? state.websocketId,
        fromName: user?.fullName ?? user?.userName,
        avatar: user?.headImgUrl,
        payload: payload,
      );
      return true;
    } catch (e) {
      print('[ChatStore] 发送私聊消息失败: $e');
      return false;
    }
  }

  /// 获取群聊历史消息 - 与 UniApp getGroupHistory 一致
  Future<List<ChatMessage>> getGroupHistory(
    String channel, {
    int? lastTime,
    bool reload = false,
  }) async {
    try {
      final t = channel.split('_');
      int? id = int.tryParse(t[0]);
      if (id == null) return [];

      if (id > 0) id = -id;
      final key = '$id';

      final result = await _chatRepository.getChannelHistory(
        channel: channel,
        lastTime: lastTime ?? DateTime.now().millisecondsSinceEpoch,
      );

      if (result.isNotEmpty) {
        final updatedChatMap = Map<String, List<ChatMessage>>.from(
          state.chatMap,
        );
        final existing = updatedChatMap[key] ?? [];

        // 后端返回 OrderByDescending (最新在前)，转为 ASC (最老在前，最新在底)
        final reversedResult = result.reversed.toList();

        if (existing.isNotEmpty && !reload) {
          // 合并历史消息
          final merged = [...reversedResult, ...existing];
          // 去重并按时间排序
          final uniqueMap = <String, ChatMessage>{};
          for (final msg in merged) {
            if (msg.id != null) {
              uniqueMap[msg.id!] = msg;
            }
          }
          final sorted = uniqueMap.values.toList()
            ..sort((a, b) => (a.time ?? 0).compareTo(b.time ?? 0));
          updatedChatMap[key] = sorted;
        } else {
          updatedChatMap[key] = reversedResult;
        }

        state = state.copyWith(chatMap: updatedChatMap);
      } else if (reload) {
        final updatedChatMap = Map<String, List<ChatMessage>>.from(
          state.chatMap,
        );
        if (!updatedChatMap.containsKey(key)) {
          updatedChatMap[key] = [];
        }
        state = state.copyWith(chatMap: updatedChatMap);
      }

      return result;
    } catch (e) {
      print('[ChatStore] 获取历史消息失败: $e');
      return [];
    }
  }

  /// 获取私聊历史消息 - 与 UniApp getPrivateHistory 一致
  Future<List<ChatMessage>> getPrivateHistory(
    int userId, {
    int? lastTime,
    bool reload = false,
  }) async {
    try {
      final key = '$userId';

      final result = await _chatRepository.getPrivateHistory(
        userId: userId,
        lastTime: lastTime ?? DateTime.now().millisecondsSinceEpoch,
      );

      if (result.isNotEmpty) {
        final updatedChatMap = Map<String, List<ChatMessage>>.from(
          state.chatMap,
        );
        final existing = updatedChatMap[key] ?? [];

        // 后端返回 OrderByDescending (最新在前)，转为 ASC (最老在前，最新在底)
        final reversedResult = result.reversed.toList();

        if (existing.isNotEmpty && !reload) {
          final merged = [...reversedResult, ...existing];
          final uniqueMap = <String, ChatMessage>{};
          for (final msg in merged) {
            if (msg.id != null) {
              uniqueMap[msg.id!] = msg;
            }
          }
          final sorted = uniqueMap.values.toList()
            ..sort((a, b) => (a.time ?? 0).compareTo(b.time ?? 0));
          updatedChatMap[key] = sorted;
        } else {
          updatedChatMap[key] = reversedResult;
        }

        state = state.copyWith(chatMap: updatedChatMap);
      }

      return result;
    } catch (e) {
      print('[ChatStore] 获取私聊历史消息失败: $e');
      return [];
    }
  }

  /// 标记已读
  void markAsRead(int chatId) {
    final updatedChatList = [...state.chatList];
    final index = updatedChatList.indexWhere((item) => item.id == chatId);

    if (index >= 0) {
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

  /// 移除消息（撤回）
  void removeMessage(String messageId) {
    final currentChatId = state.currentChat.id;
    final key = '$currentChatId';

    final updatedChatMap = Map<String, List<ChatMessage>>.from(state.chatMap);
    final messages = updatedChatMap[key];

    if (messages != null) {
      updatedChatMap[key] = messages
          .where((msg) => msg.id != messageId)
          .toList();
      state = state.copyWith(chatMap: updatedChatMap);
    }
  }

  @override
  void dispose() {
    _messageSubscription?.cancel();
    _webSocketService.dispose();
    super.dispose();
  }
}

/// ChatStore Provider
final chatStoreProvider = StateNotifierProvider<ChatStore, ChatState>((ref) {
  return ChatStore(ref);
});

/// 当前聊天消息 Provider
final currentChatMessagesProvider = Provider<List<ChatMessage>>((ref) {
  final chatState = ref.watch(chatStoreProvider);
  final currentChatId = chatState.currentChat.id;
  return chatState.chatMap['$currentChatId'] ?? [];
});

/// 当前拍卖商品 ID Provider（用于拍卖消息处理）
final currentAuctionItemIdProvider = StateProvider<int?>((ref) => null);
