import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/group_chat_provider.dart';
import '../../providers/user_provider.dart';
import '../../../data/models/chat_message_model.dart';
import '../../widgets/chat/messages/message_widget.dart';
import '../../widgets/chat/chat_input_area.dart';

/// 群聊页面
class GroupChatPage extends ConsumerStatefulWidget {
  final String channel;
  final int channelId;
  final String channelName;

  const GroupChatPage({
    super.key,
    required this.channel,
    required this.channelId,
    required this.channelName,
  });

  @override
  ConsumerState<GroupChatPage> createState() => _GroupChatPageState();
}

class _GroupChatPageState extends ConsumerState<GroupChatPage> {
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.removeListener(_onScroll);
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 100) {
      ref
          .read(
            groupChatProvider((
              channel: widget.channel,
              channelId: widget.channelId,
              channelName: widget.channelName,
            )).notifier,
          )
          .loadHistoryMessages();
    }
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    }
  }

  Future<void> _onSendText(String text) async {
    final notifier = ref.read(
      groupChatProvider((
        channel: widget.channel,
        channelId: widget.channelId,
        channelName: widget.channelName,
      )).notifier,
    );

    await notifier.sendTextMessage(text);
    _scrollToBottom();
  }

  Future<void> _onPickImage() async {
    try {
      final XFile? image = await _imagePicker.pickImage(
        source: ImageSource.gallery,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 85,
      );

      if (image != null) {
        final notifier = ref.read(
          groupChatProvider((
            channel: widget.channel,
            channelId: widget.channelId,
            channelName: widget.channelName,
          )).notifier,
        );

        await notifier.sendImageMessage(image.path);
        _scrollToBottom();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('选择图片失败: $e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final chatState = ref.watch(
      groupChatProvider((
        channel: widget.channel,
        channelId: widget.channelId,
        channelName: widget.channelName,
      )),
    );

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.channelName),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.people),
            onPressed: () {
              // TODO: 显示群成员列表
            },
          ),
          IconButton(
            icon: const Icon(Icons.more_vert),
            onPressed: () {
              // TODO: 显示群设置
            },
          ),
        ],
      ),
      body: Column(
        children: [
          // 消息列表
          Expanded(
            child: Container(
              color: const Color(0xFFFAF1F0),
              child: chatState.isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : chatState.messages.isEmpty
                  ? _buildEmptyState()
                  : _buildMessageList(chatState.messages),
            ),
          ),

          // 输入区域
          ChatInputArea(onSendText: _onSendText, onSelectImage: _onPickImage),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.group, size: 64, color: Colors.grey.shade400),
          const SizedBox(height: 16),
          Text(
            '暂无消息',
            style: TextStyle(fontSize: 16, color: Colors.grey.shade500),
          ),
          const SizedBox(height: 8),
          Text(
            '发送第一条消息开始群聊',
            style: TextStyle(fontSize: 14, color: Colors.grey.shade400),
          ),
        ],
      ),
    );
  }

  Widget _buildMessageList(List<ChatMessage> messages) {
    return ListView.builder(
      controller: _scrollController,
      padding: const EdgeInsets.all(16),
      itemCount: messages.length,
      itemBuilder: (context, index) {
        final message = messages[index];
        return _buildMessageItem(message);
      },
    );
  }

  Widget _buildMessageItem(ChatMessage message) {
    // 判断是否是自己发送的消息
    final isSelf = message.from != null && message.from == _getCurrentUserId();

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

  int? _getCurrentUserId() {
    final userState = ref.read(userProvider);
    return userState.user?.id;
  }

  Widget _buildAvatar(ChatMessage message) {
    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        color: _getAvatarColor(message.from ?? 0),
        shape: BoxShape.circle,
      ),
      child: Center(
        child: Text(
          _getAvatarText(message.fromName ?? '用户'),
          style: const TextStyle(
            color: Colors.white,
            fontSize: 12,
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
              color: _getTagColor(message.tagClass),
              borderRadius: BorderRadius.circular(4),
            ),
            child: Text(
              message.fromTag ?? '管理',
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

  Color _getTagColor(String? tagClass) {
    switch (tagClass) {
      case 'tag_AuctionManager':
        return const Color(0xFFFF5722);
      case 'tag_Admin':
        return const Color(0xFFE91E63);
      default:
        return const Color(0xFFF4835A);
    }
  }
}
