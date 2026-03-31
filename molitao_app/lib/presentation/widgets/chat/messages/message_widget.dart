import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/text_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/image_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/auction_bid_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/auction_start_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/auction_end_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/auction_deal_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/kasec_status_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/system_message.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/welcome_message.dart';

/// 消息组件工厂
/// 根据消息类型渲染不同的消息组件
class MessageWidget extends StatelessWidget {
  final ChatMessage message;
  final VoidCallback? onTap;

  const MessageWidget({Key? key, required this.message, this.onTap})
    : super(key: key);

  @override
  Widget build(BuildContext context) {
    // 调试输出
    print(
      '[MessageWidget] type=${message.type}, msg=${message.msg}, payload=${message.payload}',
    );

    switch (message.type) {
      case ChatMessageType.text:
        return TextMessage(message: message, onTap: onTap);
      case ChatMessageType.image:
        return ImageMessage(message: message, onTap: onTap);
      case ChatMessageType.auctionBid:
        return AuctionBidMessage(message: message, onTap: onTap);
      case ChatMessageType.auctionStart:
        return AuctionStartMessage(message: message, onTap: onTap);
      case ChatMessageType.auctionEnd:
        return AuctionEndMessage(message: message, onTap: onTap);
      case ChatMessageType.auctionDeal:
        return AuctionDealMessage(message: message, onTap: onTap);
      case ChatMessageType.kasecStatusChanged:
        return KasecStatusMessage(message: message, onTap: onTap);
      case ChatMessageType.receipt:
      case ChatMessageType.banUser:
      case ChatMessageType.backout:
        return SystemMessage(message: message);
      case ChatMessageType.welcome:
        return WelcomeMessage(message: message);
      case null:
      default:
        // 默认显示文本消息
        return TextMessage(
          message: ChatMessage(
            id: message.id,
            type: ChatMessageType.text,
            status: message.status,
            chan: message.chan,
            from: message.from,
            fromName: message.fromName,
            fromAdmin: message.fromAdmin,
            fromTag: message.fromTag,
            tagClass: message.tagClass,
            avatar: message.avatar,
            to: message.to,
            time: message.time,
            msg: message.msg ?? '未知消息类型',
            payload: message.payload,
            receipt: message.receipt,
            sequenceNumber: message.sequenceNumber,
          ),
          onTap: onTap,
        );
    }
  }
}
