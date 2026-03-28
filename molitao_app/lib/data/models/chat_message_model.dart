enum ChatMessageStatus { sending, fail, success }

enum ChatMessageType {
  text,
  image,
  file,
  receipt,
  welcome,
  goodbye,
  banUser,
  backout,
  auctionStart,
  auctionBid,
  auctionEnd,
  auctionDeal,
  error,
  kasecStatusChanged,
}

class ChatMessage {
  final String? id;
  final ChatMessageType? type;
  final ChatMessageStatus? status;
  final String? chan;
  final int? from;
  final String? fromName;
  final bool? fromAdmin;
  final String? fromTag;
  final String? tagClass;
  final String? avatar;
  final int? to;
  final int? time;
  final String? msg;
  final dynamic payload;
  final String? receipt;
  final int? sequenceNumber;

  const ChatMessage({
    this.id,
    this.type,
    this.status,
    this.chan,
    this.from,
    this.fromName,
    this.fromAdmin,
    this.fromTag,
    this.tagClass,
    this.avatar,
    this.to,
    this.time,
    this.msg,
    this.payload,
    this.receipt,
    this.sequenceNumber,
  });

  factory ChatMessage.fromJson(Map<String, dynamic> json) {
    return ChatMessage(
      id: json['id'],
      type: _parseChatMessageType(json['type']),
      status: _parseChatMessageStatus(json['status']),
      chan: json['chan'],
      from: json['from'],
      fromName: json['fromName'],
      fromAdmin: json['fromAdmin'],
      fromTag: json['fromTag'],
      tagClass: json['tagClass'],
      avatar: json['avatar'],
      to: json['to'],
      time: json['time'],
      msg: json['msg'],
      payload: json['payload'],
      receipt: json['receipt'],
      sequenceNumber: json['sequenceNumber'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'type': _chatMessageTypeToString(type),
      'status': _chatMessageStatusToString(status),
      'chan': chan,
      'from': from,
      'fromName': fromName,
      'fromAdmin': fromAdmin,
      'fromTag': fromTag,
      'tagClass': tagClass,
      'avatar': avatar,
      'to': to,
      'time': time,
      'msg': msg,
      'payload': payload,
      'receipt': receipt,
      'sequenceNumber': sequenceNumber,
    };
  }

  static ChatMessageType? _parseChatMessageType(dynamic value) {
    if (value == null) return null;
    final stringValue = value.toString().toLowerCase();
    switch (stringValue) {
      case 'text':
        return ChatMessageType.text;
      case 'image':
        return ChatMessageType.image;
      case 'file':
        return ChatMessageType.file;
      case 'receipt':
        return ChatMessageType.receipt;
      case 'welcome':
        return ChatMessageType.welcome;
      case 'goodbye':
        return ChatMessageType.goodbye;
      case 'banuser':
        return ChatMessageType.banUser;
      case 'backout':
        return ChatMessageType.backout;
      case 'auctionstart':
        return ChatMessageType.auctionStart;
      case 'auctionbid':
        return ChatMessageType.auctionBid;
      case 'auctionend':
        return ChatMessageType.auctionEnd;
      case 'auctiondeal':
        return ChatMessageType.auctionDeal;
      case 'error':
        return ChatMessageType.error;
      case 'kasecstatuschanged':
        return ChatMessageType.kasecStatusChanged;
      default:
        return null;
    }
  }

  static String? _chatMessageTypeToString(ChatMessageType? type) {
    if (type == null) return null;
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

  static ChatMessageStatus? _parseChatMessageStatus(dynamic value) {
    if (value == null) return null;
    final stringValue = value.toString().toLowerCase();
    switch (stringValue) {
      case 'sending':
        return ChatMessageStatus.sending;
      case 'fail':
        return ChatMessageStatus.fail;
      case 'success':
        return ChatMessageStatus.success;
      default:
        return null;
    }
  }

  static String? _chatMessageStatusToString(ChatMessageStatus? status) {
    if (status == null) return null;
    switch (status) {
      case ChatMessageStatus.sending:
        return 'Sending';
      case ChatMessageStatus.fail:
        return 'Fail';
      case ChatMessageStatus.success:
        return 'Success';
    }
  }
}
