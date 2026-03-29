import 'chat_message_model.dart';

enum ChatListItemType { group, user, system }

class ChatListItem {
  final int? id;
  final String name;
  final ChatListItemType type;
  final int? time;
  final String? avatar;
  final String? lastMsg;
  final int unread;
  final int order;
  final ChatMessage? msg;

  const ChatListItem({
    this.id,
    required this.name,
    required this.type,
    this.time,
    this.avatar,
    this.lastMsg,
    required this.unread,
    required this.order,
    this.msg,
  });

  factory ChatListItem.fromJson(Map<String, dynamic> json) {
    return ChatListItem(
      id: json['id'],
      name: json['name'] ?? '',
      type: _parseChatListItemType(json['type']),
      time: json['time'],
      avatar: json['avatar'],
      lastMsg: json['lastMsg'],
      unread: json['unread'] ?? 0,
      order: json['order'] ?? 0,
      msg: json['msg'] != null ? ChatMessage.fromJson(json['msg']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'type': _chatListItemTypeToString(type),
      'time': time,
      'avatar': avatar,
      'lastMsg': lastMsg,
      'unread': unread,
      'order': order,
      'msg': msg?.toJson(),
    };
  }

  static ChatListItemType _parseChatListItemType(dynamic value) {
    if (value == null) return ChatListItemType.user; // default
    if (value is int) {
      switch (value) {
        case 0:
          return ChatListItemType.group;
        case 1:
          return ChatListItemType.user;
        case 2:
          return ChatListItemType.system;
        default:
          return ChatListItemType.user;
      }
    } else if (value is String) {
      final stringValue = value.toLowerCase();
      switch (stringValue) {
        case 'group':
          return ChatListItemType.group;
        case 'user':
          return ChatListItemType.user;
        case 'system':
          return ChatListItemType.system;
        default:
          return ChatListItemType.user;
      }
    }
    return ChatListItemType.user;
  }

  static String _chatListItemTypeToString(ChatListItemType type) {
    switch (type) {
      case ChatListItemType.group:
        return 'group';
      case ChatListItemType.user:
        return 'user';
      case ChatListItemType.system:
        return 'system';
    }
  }
}
