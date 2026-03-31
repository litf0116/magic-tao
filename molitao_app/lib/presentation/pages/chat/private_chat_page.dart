import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/private_chat_provider.dart';
import '../../providers/user_provider.dart';
import '../../../data/models/chat_message_model.dart';
import '../../widgets/chat/messages/message_widget.dart';
import '../../widgets/chat/chat_input_area.dart';

/// 私聊页面
class PrivateChatPage extends ConsumerStatefulWidget {
  final int friendId;
  final String friendName;
  final String? friendAvatar;

  const PrivateChatPage({
    super.key,
    required this.friendId,
    required this.friendName,
    this.friendAvatar,
  });

  @override
  ConsumerState<PrivateChatPage> createState() => _PrivateChatPageState();
}

class _PrivateChatPageState extends ConsumerState<PrivateChatPage> {
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
    // 当滚动到顶部时，加载更多历史消息
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 100) {
      ref
          .read(
            privateChatProvider((
              friendId: widget.friendId,
              friendName: widget.friendName,
              friendAvatar: widget.friendAvatar,
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
      privateChatProvider((
        friendId: widget.friendId,
        friendName: widget.friendName,
        friendAvatar: widget.friendAvatar,
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
          privateChatProvider((
            friendId: widget.friendId,
            friendName: widget.friendName,
            friendAvatar: widget.friendAvatar,
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
      privateChatProvider((
        friendId: widget.friendId,
        friendName: widget.friendName,
        friendAvatar: widget.friendAvatar,
      )),
    );

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.friendName),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.more_vert),
            onPressed: () {
              // TODO: 显示好友详情
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
          Icon(
            Icons.chat_bubble_outline,
            size: 64,
            color: Colors.grey.shade400,
          ),
          const SizedBox(height: 16),
          Text(
            '暂无消息',
            style: TextStyle(fontSize: 16, color: Colors.grey.shade500),
          ),
          const SizedBox(height: 8),
          Text(
            '发送第一条消息开始聊天',
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
                // 用户名
                if (!isSelf)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 4),
                    child: Text(
                      message.fromName ?? widget.friendName,
                      style: const TextStyle(
                        fontSize: 12,
                        color: Color(0xFF999999),
                      ),
                    ),
                  ),
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
    final avatarUrl = message.from == _getCurrentUserId()
        ? null
        : widget.friendAvatar;

    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        color: const Color(0xFFF4835A),
        shape: BoxShape.circle,
        image: avatarUrl != null
            ? DecorationImage(image: NetworkImage(avatarUrl), fit: BoxFit.cover)
            : null,
      ),
      child: avatarUrl == null
          ? Center(
              child: Text(
                _getAvatarText(message.fromName ?? '我'),
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
              ),
            )
          : null,
    );
  }

  void _handleMessageTap(ChatMessage message) {
    debugPrint('消息被点击: ${message.id}, 类型: ${message.type}');
  }

  String _getAvatarText(String name) {
    if (name.isEmpty) return '用';
    return name.length > 2 ? name.substring(0, 2) : name;
  }
}
