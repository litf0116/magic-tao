import 'package:flutter/material.dart';
import 'package:molitao_app/data/models/chat_message_model.dart';
import 'package:molitao_app/presentation/widgets/chat/messages/message_widget.dart';
import 'package:molitao_app/presentation/widgets/chat/chat_input_area.dart';

/// 聊天消息预览页面
/// 用于测试和预览各种消息类型的 UI 效果
class ChatMessagePreviewPage extends StatefulWidget {
  const ChatMessagePreviewPage({Key? key}) : super(key: key);

  @override
  State<ChatMessagePreviewPage> createState() => _ChatMessagePreviewPageState();
}

class _ChatMessagePreviewPageState extends State<ChatMessagePreviewPage> {
  final List<ChatMessage> _messages = List.from(mockMessages);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('消息组件预览'),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
      ),
      body: Column(
        children: [
          // 消息列表
          Expanded(
            child: Container(
              color: const Color(0xFFFAF1F0),
              child: ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: _messages.length,
                itemBuilder: (context, index) {
                  final message = _messages[index];
                  return _buildMessageItem(message);
                },
              ),
            ),
          ),

          // 输入区域
          ChatInputArea(
            onSendText: _handleSendText,
            onSelectImage: _handleSelectImage,
            onSelectEmoji: _handleSelectEmoji,
          ),
        ],
      ),
    );
  }

  void _handleSendText(String text) {
    // 添加新消息
    final newMessage = ChatMessage(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      type: ChatMessageType.text,
      from: 1001,
      fromName: '我',
      msg: text,
      time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
    );

    setState(() {
      _messages.add(newMessage);
    });
  }

  void _handleSelectImage() {
    // 模拟选择图片
    final newMessage = ChatMessage(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      type: ChatMessageType.image,
      from: 1001,
      fromName: '我',
      msg: 'https://picsum.photos/300/300',
      payload: {'url': 'https://picsum.photos/300/300'},
      time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
    );

    setState(() {
      _messages.add(newMessage);
    });

    ScaffoldMessenger.of(
      context,
    ).showSnackBar(const SnackBar(content: Text('图片选择功能待实现')));
  }

  void _handleSelectEmoji(String emojiCode) {
    debugPrint('选择了表情: $emojiCode');
  }

  Widget _buildMessageItem(ChatMessage message) {
    // 判断是否是自己发送的消息
    final isSelf = message.from == 1001;

    // 系统消息和欢迎消息居中显示
    if (message.type == ChatMessageType.welcome ||
        message.type == ChatMessageType.banUser ||
        message.type == ChatMessageType.backout) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: 8),
        child: MessageWidget(message: message),
      );
    }

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: isSelf
            ? MainAxisAlignment.end
            : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 头像（非自己消息显示在左边）
          if (!isSelf) ...[_buildAvatar(message), const SizedBox(width: 10)],
          // 消息内容
          Flexible(
            child: Column(
              crossAxisAlignment: isSelf
                  ? CrossAxisAlignment.end
                  : CrossAxisAlignment.start,
              children: [
                // 用户名和标签
                if (!isSelf) _buildUserName(message),
                const SizedBox(height: 4),
                // 消息组件
                MessageWidget(
                  message: message,
                  onTap: () => _handleMessageTap(message),
                ),
              ],
            ),
          ),
          // 头像（自己消息显示在右边）
          if (isSelf) ...[const SizedBox(width: 10), _buildAvatar(message)],
        ],
      ),
    );
  }

  Widget _buildAvatar(ChatMessage message) {
    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        color: _getAvatarColor(message.from ?? 0),
        shape: BoxShape.circle,
      ),
      child: Center(
        child: Text(
          _getAvatarText(message.fromName ?? '用户'),
          style: const TextStyle(
            color: Colors.white,
            fontSize: 14,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
    );
  }

  Widget _buildUserName(ChatMessage message) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        if (message.fromAdmin == true) ...[
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
            decoration: BoxDecoration(
              color: Colors.red,
              borderRadius: BorderRadius.circular(4),
            ),
            child: const Text(
              '主持',
              style: TextStyle(
                color: Colors.white,
                fontSize: 10,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 6),
        ],
        if (message.fromTag != null) ...[
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
            decoration: BoxDecoration(
              color: Colors.orange,
              borderRadius: BorderRadius.circular(4),
            ),
            child: Text(
              message.fromTag!,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 10,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 6),
        ],
        Text(
          message.fromName ?? '未知用户',
          style: const TextStyle(fontSize: 12, color: Color(0xFF999999)),
        ),
      ],
    );
  }

  void _handleMessageTap(ChatMessage message) {
    debugPrint('消息被点击: ${message.id}, 类型: ${message.type}');
  }

  Color _getAvatarColor(int userId) {
    final colors = [
      const Color(0xFFF4835A),
      const Color(0xFF1890FF),
      const Color(0xFFFF4D4F),
      const Color(0xFF722ED1),
      const Color(0xFF52C41A),
      const Color(0xFFFA8C16),
    ];
    return colors[userId % colors.length];
  }

  String _getAvatarText(String name) {
    if (name.isEmpty) return '用';
    return name.length > 2 ? name.substring(0, 2) : name;
  }
}

/// Mock 消息数据
/// 包含所有消息类型的示例
final List<ChatMessage> mockMessages = [
  // 欢迎消息
  ChatMessage(
    id: '1',
    type: ChatMessageType.welcome,
    fromName: '系统',
    msg: '欢迎来到秒杀场，请注意遵守交易规则',
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 文本消息（带表情）
  ChatMessage(
    id: '2',
    type: ChatMessageType.text,
    from: 1002,
    fromName: '张三',
    msg: '这个看起来不错[微笑]，准备出价了[奋斗]',
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 开始秒杀消息
  ChatMessage(
    id: '3',
    type: ChatMessageType.auctionStart,
    from: 1003,
    fromName: '秒杀主持',
    fromAdmin: true,
    fromTag: '主持',
    payload: {
      'id': 12345,
      'name': '175级神天兵满修号',
      'description': '起拍价: 50魔力值\n装备齐全，技能全满',
    },
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 出价消息
  ChatMessage(
    id: '4',
    type: ChatMessageType.auctionBid,
    from: 1004,
    fromName: '李四',
    payload: {'id': 12345, 'name': '175级神天兵满修号', 'currentPrice': 68.0},
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 出价消息2
  ChatMessage(
    id: '5',
    type: ChatMessageType.auctionBid,
    from: 1005,
    fromName: '王五',
    payload: {'id': 12345, 'name': '175级神天兵满修号', 'currentPrice': 85.0},
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 卡秒状态消息 - 开启
  ChatMessage(
    id: '6',
    type: ChatMessageType.kasecStatusChanged,
    from: 1003,
    fromName: '秒杀主持',
    fromAdmin: true,
    fromTag: '主持',
    msg: '秒杀主持已开启卡秒模式，需三倍加价！',
    payload: {'isKasec': true},
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 秒杀结束消息
  ChatMessage(
    id: '7',
    type: ChatMessageType.auctionEnd,
    from: 1003,
    fromName: '秒杀主持',
    fromAdmin: true,
    fromTag: '主持',
    payload: {
      'id': 12345,
      'name': '175级神天兵满修号',
      'dealUserName': '王五',
      'finalPrice': 85.0,
      'status': '已成交',
      'dealTime': DateTime.now().toIso8601String(),
    },
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 成交通知消息（自己中标）
  ChatMessage(
    id: '8',
    type: ChatMessageType.auctionDeal,
    from: 1001,
    fromName: '王五（我）',
    payload: {
      'id': 12345,
      'name': '175级神天兵满修号',
      'finalPrice': 85.0,
      'dealTime': DateTime.now().toIso8601String(),
    },
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 系统消息 - 禁言
  ChatMessage(
    id: '9',
    type: ChatMessageType.banUser,
    fromName: '系统',
    msg: '用户 张三 已被禁言 60 分钟',
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 文本消息（自己发送）
  ChatMessage(
    id: '10',
    type: ChatMessageType.text,
    from: 1001,
    fromName: '我',
    msg: '收到，我会联系的[OK]',
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 图片消息
  ChatMessage(
    id: '11',
    type: ChatMessageType.image,
    from: 1002,
    fromName: '张三',
    msg: 'https://picsum.photos/300/300',
    payload: {'url': 'https://picsum.photos/300/300'},
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),

  // 卡秒状态消息 - 关闭
  ChatMessage(
    id: '12',
    type: ChatMessageType.kasecStatusChanged,
    from: 1003,
    fromName: '秒杀主持',
    fromAdmin: true,
    fromTag: '主持',
    msg: '秒杀主持已关闭卡秒模式，恢复正常加价',
    payload: {'isKasec': false},
    time: DateTime.now().millisecondsSinceEpoch ~/ 1000,
  ),
];
