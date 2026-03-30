import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../data/models/chat_list_item_model.dart' as model;
import '../../providers/chat_provider.dart';
import '../../providers/user_provider.dart';
import '../../mixins/auth_guard_mixin.dart';
import 'package:intl/intl.dart';

class ChatListPage extends ConsumerStatefulWidget {
  const ChatListPage({super.key});

  @override
  ConsumerState<ChatListPage> createState() => _ChatListPageState();
}

class _ChatListPageState extends ConsumerState<ChatListPage>
    with AuthGuardMixin {
  @override
  void initState() {
    super.initState();
    // AuthGuardMixin will check login status in initState
    // Load chat list and connect to WebSocket after auth check
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _initChat();
    });
  }

  Future<void> _initChat() async {
    print('========== ChatListPage._initChat() 被调用 ==========');

    // 检查是否已登录
    final userState = ref.read(userProvider);
    print('[ChatListPage] 用户登录状态: ${userState.isLoggedIn}');

    if (!userState.isLoggedIn) {
      print('[ChatListPage] 用户未登录，跳过 WebSocket 连接');
      return;
    }

    // 加载聊天列表
    print('[ChatListPage] 加载聊天列表...');
    await ref.read(chatProvider.notifier).loadChatList();

    // 连接 WebSocket（只在已登录时）
    print('[ChatListPage] 连接 WebSocket...');
    await ref.read(chatProvider.notifier).connectWebSocket();
  }

  @override
  Widget build(BuildContext context) {
    final chatState = ref.watch(chatProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('会话'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.read(chatProvider.notifier).loadChatList(),
        child: chatState.isLoading && chatState.chatList.isEmpty
            ? const Center(child: CircularProgressIndicator())
            : chatState.chatList.isEmpty
            ? ListView(
                children: const [
                  SizedBox(height: 200),
                  Center(child: Text('当前没有会话')),
                ],
              )
            : ListView.builder(
                itemCount: chatState.chatList.length,
                itemBuilder: (context, index) {
                  final providerItem = chatState.chatList[index];
                  final chatItem = model.ChatListItem(
                    id: providerItem.id,
                    name: providerItem.name,
                    type: _convertType(providerItem.type),
                    time: providerItem.time,
                    avatar: providerItem.avatar,
                    lastMsg: providerItem.lastMsg,
                    unread: providerItem.unread,
                    order: providerItem.order,
                  );
                  return _buildChatItem(context, ref, chatItem);
                },
              ),
      ),
    );
  }

  model.ChatListItemType _convertType(ChatListItemType providerType) {
    switch (providerType) {
      case ChatListItemType.group:
        return model.ChatListItemType.group;
      case ChatListItemType.user:
        return model.ChatListItemType.user;
      case ChatListItemType.system:
        return model.ChatListItemType.system;
    }
  }

  Widget _buildChatItem(
    BuildContext context,
    WidgetRef ref,
    model.ChatListItem chatItem,
  ) {
    return GestureDetector(
      onTap: () => _navigateToChatDetail(context, chatItem),
      onLongPress: () => _showContextMenu(context, ref, chatItem),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: const BoxDecoration(
          border: Border(bottom: BorderSide(color: Color(0xFFefefef))),
        ),
        child: Row(
          children: [
            Stack(
              children: [
                _buildAvatar(chatItem),
                if (chatItem.unread > 0)
                  Positioned(
                    right: 0,
                    top: 0,
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: const Color(0xFFee593c),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      constraints: const BoxConstraints(
                        minWidth: 20,
                        minHeight: 20,
                      ),
                      child: Text(
                        chatItem.unread > 99
                            ? '99+'
                            : chatItem.unread.toString(),
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 10,
                          fontWeight: FontWeight.bold,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          _getChannelDisplayName(chatItem),
                          style: TextStyle(
                            fontSize: 17,
                            fontWeight: _isSpecialChannel(chatItem.id)
                                ? FontWeight.bold
                                : FontWeight.normal,
                            color: _getChannelNameColor(chatItem),
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      if (chatItem.time != null)
                        Text(
                          _formatTime(chatItem.time!),
                          style: const TextStyle(
                            fontSize: 13,
                            color: Color(0xFFB3B3B3),
                          ),
                        ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    chatItem.lastMsg ?? '',
                    style: const TextStyle(
                      fontSize: 15,
                      color: Color(0xFFB3B3B3),
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  bool _isSpecialChannel(int? id) {
    return id != null && (id == -10 || id == -11 || id == -1 || id == 0);
  }

  Color _getChannelNameColor(model.ChatListItem chatItem) {
    if (chatItem.id == -10) return const Color(0xFF9333EA); // purple
    if (chatItem.id == -11) return const Color(0xFF3B82F6); // blue
    if (chatItem.id == 0) return const Color(0xFF2563EB); // blue
    if (chatItem.id == -1) return const Color(0xFF16A34A); // green
    return const Color(0xFF374151); // gray
  }

  Widget _buildAvatar(model.ChatListItem chatItem) {
    const double avatarSize = 50;

    if (chatItem.id != null) {
      // Special handling for system channels
      if (chatItem.id == -10) {
        // Announcement
        return Container(
          width: avatarSize,
          height: avatarSize,
          decoration: const BoxDecoration(
            color: Color(0xFF16A34A),
            shape: BoxShape.circle,
          ),
          child: const Center(
            child: Text(
              '系统',
              style: TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        );
      } else if (chatItem.id == -11) {
        // Newbie group
        return Container(
          width: avatarSize,
          height: avatarSize,
          decoration: const BoxDecoration(
            color: Color(0xFF16A34A),
            shape: BoxShape.circle,
          ),
          child: const Center(
            child: Text(
              '新手',
              style: TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        );
      } else if (chatItem.id == 0) {
        // Lobby
        return Container(
          width: avatarSize,
          height: avatarSize,
          decoration: const BoxDecoration(
            color: Color(0xFF16A34A),
            shape: BoxShape.circle,
          ),
          child: const Center(
            child: Text(
              '大厅',
              style: TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        );
      } else if (chatItem.id == -1) {
        // Auction
        return Container(
          width: avatarSize,
          height: avatarSize,
          decoration: const BoxDecoration(
            color: Color(0xFF16A34A),
            shape: BoxShape.circle,
          ),
          child: const Center(
            child: Text(
              '秒杀',
              style: TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        );
      } else if (chatItem.avatar != null && chatItem.avatar!.isNotEmpty) {
        // Regular user/group with avatar
        return CircleAvatar(
          radius: avatarSize / 2,
          backgroundImage: NetworkImage(chatItem.avatar!),
        );
      }
    }

    // For group chats without avatar, use random color avatar
    if (chatItem.type == model.ChatListItemType.group) {
      return Container(
        width: avatarSize,
        height: avatarSize,
        decoration: BoxDecoration(
          color: _getRandomColor(chatItem.name),
          shape: BoxShape.circle,
        ),
        child: Center(
          child: Text(
            chatItem.name.isNotEmpty
                ? chatItem.name.substring(
                    0,
                    chatItem.name.length > 2 ? 2 : chatItem.name.length,
                  )
                : '组队',
            style: const TextStyle(
              color: Colors.white,
              fontSize: 12,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
      );
    }

    // Default avatar for users without image
    return CircleAvatar(
      radius: avatarSize / 2,
      backgroundColor: Colors.grey,
      child: const Icon(Icons.person, color: Colors.white, size: 24),
    );
  }

  Color _getRandomColor(String name) {
    if (name.isEmpty) return Colors.black;
    int hash = 0;
    for (int i = 0; i < name.length; i++) {
      hash = name.codeUnitAt(i) + ((hash << 5) - hash);
    }
    int r = (hash >> 16) & 0xFF;
    int g = (hash >> 8) & 0xFF;
    int b = hash & 0xFF;
    return Color.fromRGBO(r, g, b, 1.0);
  }

  String _getChannelDisplayName(model.ChatListItem chatItem) {
    if (chatItem.id == -10) return '系统公告';
    if (chatItem.id == -11) return '新手版主群聊';
    if (chatItem.id == 0) return '勇者招募所';
    if (chatItem.id == -1) return '秒杀场';
    return chatItem.name;
  }

  String _formatTime(int timestamp) {
    final date = DateTime.fromMillisecondsSinceEpoch(timestamp);
    return DateFormat('MM-dd HH:mm').format(date);
  }

  void _navigateToChatDetail(
    BuildContext context,
    model.ChatListItem chatItem,
  ) {
    if (chatItem.id == null) return;

    // Mark as read
    ref.read(chatProvider.notifier).markAsRead(chatItem.id!);

    if (chatItem.id == -1) {
      // Auction
      context.push('/chat/auction');
    } else if (chatItem.id == -10) {
      // Announcement
      context.push('/chat/group/-10');
    } else if (chatItem.id == -11) {
      // Newbie group
      context.push('/chat/group/-11');
    } else if (chatItem.id == 0) {
      // Lobby
      context.push('/chat/group/0');
    } else if (chatItem.type == model.ChatListItemType.user) {
      // Private chat
      context.push('/chat/private/${chatItem.id}');
    } else {
      // Group chat
      context.push('/chat/group/${chatItem.id}');
    }
  }

  void _showContextMenu(
    BuildContext context,
    WidgetRef ref,
    model.ChatListItem chatItem,
  ) {
    showModalBottomSheet(
      context: context,
      builder: (BuildContext context) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ListTile(
                title: const Text('删除聊天', textAlign: TextAlign.center),
                onTap: () {
                  Navigator.pop(context);
                  _confirmDelete(context, ref, chatItem);
                },
              ),
            ],
          ),
        );
      },
    );
  }

  void _confirmDelete(
    BuildContext context,
    WidgetRef ref,
    model.ChatListItem chatItem,
  ) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('确定删除聊天记录吗？'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                // Delete conversation
                ref.read(chatProvider.notifier).deleteChat(chatItem.id ?? 0);
              },
              child: const Text('确定', style: TextStyle(color: Colors.red)),
            ),
          ],
        );
      },
    );
  }
}
