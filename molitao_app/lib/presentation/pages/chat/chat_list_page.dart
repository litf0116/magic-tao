import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../data/models/chat_list_item_model.dart' as model;
import '../../providers/chat_provider.dart';
import 'package:intl/intl.dart';

class ChatListPage extends ConsumerWidget {
  const ChatListPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final chatState = ref.watch(chatProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('会话')),
      body: chatState.isLoading
          ? const Center(child: CircularProgressIndicator())
          : chatState.chatList.isEmpty
          ? const Center(child: Text('当前没有会话'))
          : ListView.builder(
              itemCount: chatState.chatList.length,
              itemBuilder: (context, index) {
                // Need to convert from provider model to data model
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
                return _buildChatItem(context, chatItem);
              },
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

  Widget _buildChatItem(BuildContext context, model.ChatListItem chatItem) {
    return GestureDetector(
      onTap: () => _navigateToChatDetail(context, chatItem),
      onLongPress: () => _showContextMenu(context, chatItem),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
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
                        color: Colors.red,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      constraints: const BoxConstraints(
                        minWidth: 18,
                        minHeight: 18,
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
            const SizedBox(width: 12),
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
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      if (chatItem.time != null)
                        Text(
                          _formatTime(chatItem.time!),
                          style: const TextStyle(
                            fontSize: 12,
                            color: Colors.grey,
                          ),
                        ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    chatItem.lastMsg ?? '',
                    style: const TextStyle(fontSize: 14, color: Colors.grey),
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

  Widget _buildAvatar(model.ChatListItem chatItem) {
    if (chatItem.id != null) {
      // Special handling for system channels
      if (chatItem.id == -10) {
        // Announcement
        return Container(
          width: 50,
          height: 50,
          decoration: const BoxDecoration(
            color: Colors.green,
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
          width: 50,
          height: 50,
          decoration: const BoxDecoration(
            color: Colors.green,
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
          width: 50,
          height: 50,
          decoration: const BoxDecoration(
            color: Colors.green,
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
          width: 50,
          height: 50,
          decoration: const BoxDecoration(
            color: Colors.green,
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
          radius: 25,
          backgroundImage: NetworkImage(chatItem.avatar!),
        );
      }
    }

    // For group chats without avatar, use random color avatar
    if (chatItem.type == model.ChatListItemType.group) {
      return Container(
        width: 50,
        height: 50,
        decoration: BoxDecoration(
          color: _getRandomColor(chatItem.name),
          shape: BoxShape.circle,
        ),
        child: const Center(
          child: Icon(Icons.group, color: Colors.white, size: 24),
        ),
      );
    }

    // Default avatar for users without image
    return const CircleAvatar(
      radius: 25,
      backgroundColor: Colors.grey,
      child: Icon(Icons.person, color: Colors.white, size: 24),
    );
  }

  Color _getRandomColor(String name) {
    // Generate a deterministic color based on the name
    int hash = name.hashCode;
    int r = (hash >> 16) % 255;
    int g = (hash >> 8) % 255;
    int b = hash % 255;
    return Color.fromRGBO(r, g, b, 1.0);
  }

  String _getChannelDisplayName(model.ChatListItem chatItem) {
    if (chatItem.id != null) {
      if (chatItem.id == -10) return '公告';
      if (chatItem.id == -11) return '新手群';
      if (chatItem.id == 0) return '大厅';
      if (chatItem.id == -1) return '秒杀';
    }
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
    if (chatItem.id != null) {
      if (chatItem.id == -1) {
        // Auction
        // Navigate to auction chat page
        Navigator.pushNamed(context, '/chat/auction');
      } else if (chatItem.type == model.ChatListItemType.user) {
        // Navigate to private chat page
        Navigator.pushNamed(context, '/chat/private/${chatItem.id}');
      } else {
        // Navigate to group chat page
        Navigator.pushNamed(context, '/chat/group/${chatItem.id}');
      }
    } else {
      // Default to private chat if no ID
      Navigator.pushNamed(context, '/chat/private/${chatItem.id ?? 0}');
    }
  }

  void _showContextMenu(BuildContext context, model.ChatListItem chatItem) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('操作'),
          content: const Text('确定要删除这个会话吗？'),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop(); // Close dialog
              },
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                // Close dialog and delete conversation
                Navigator.of(context).pop();
                // TODO: Implement delete conversation functionality
              },
              child: const Text('删除'),
            ),
          ],
        );
      },
    );
  }
}
