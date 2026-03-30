import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/private_chat_provider.dart';
import '../../widgets/chat/message_bubble.dart';
import '../../../core/widgets/chat_input_bar.dart';
import '../../../data/models/chat_message_model.dart';

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
  bool _showMorePanel = false;

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
      // 加载更多消息
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
        // TODO: 上传图片到服务器，获取 URL
        // 这里暂时使用本地路径
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

    setState(() {
      _showMorePanel = false;
    });
  }

  Future<void> _onTakePhoto() async {
    try {
      final XFile? image = await _imagePicker.pickImage(
        source: ImageSource.camera,
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
        ).showSnackBar(SnackBar(content: Text('拍照失败: $e')));
      }
    }

    setState(() {
      _showMorePanel = false;
    });
  }

  void _toggleMorePanel() {
    setState(() {
      _showMorePanel = !_showMorePanel;
    });
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
        backgroundColor: const Color(0xFFf4835a),
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
              color: const Color(0xFFfaf1f0),
              child: chatState.isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : chatState.messages.isEmpty
                  ? _buildEmptyState()
                  : _buildMessageList(chatState.messages),
            ),
          ),

          // 更多功能面板
          if (_showMorePanel)
            MoreActionsPanel(
              onImagePick: _onPickImage,
              onCameraPick: _onTakePhoto,
            ),

          // 输入栏
          ChatInputBar(
            onSendText: _onSendText,
            onMoreTap: _toggleMorePanel,
            placeholder: '发送消息',
          ),
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
      padding: const EdgeInsets.symmetric(vertical: 8),
      itemCount: messages.length,
      itemBuilder: (context, index) {
        final message = messages[index];
        return _buildMessageItem(message);
      },
    );
  }

  Widget _buildMessageItem(ChatMessage message) {
    // 判断是否是自己发送的消息
    // TODO: 从用户状态获取当前用户 ID
    final isSelf =
        message.status == ChatMessageStatus.sending ||
        message.status == ChatMessageStatus.success;

    // 根据消息类型渲染不同的消息组件
    switch (message.type) {
      case ChatMessageType.text:
        return _buildTextMessage(message, isSelf);
      case ChatMessageType.image:
        return _buildImageMessage(message, isSelf);
      case ChatMessageType.welcome:
        return SystemMessageBubble(text: '${message.fromName} 加入了聊天');
      case ChatMessageType.banUser:
        return SystemMessageBubble(text: message.msg ?? '用户已被禁言');
      case ChatMessageType.backout:
        return const SystemMessageBubble(text: '消息已撤回');
      default:
        return _buildTextMessage(message, isSelf);
    }
  }

  Widget _buildTextMessage(ChatMessage message, bool isSelf) {
    return MessageWithAvatar(
      isSelf: isSelf,
      avatarUrl: isSelf ? null : widget.friendAvatar,
      userName: isSelf ? null : message.fromName,
      child: Text(
        message.msg ?? '',
        style: TextStyle(
          fontSize: 15,
          color: isSelf ? Colors.white : Colors.black87,
        ),
      ),
    );
  }

  Widget _buildImageMessage(ChatMessage message, bool isSelf) {
    final imageUrl =
        message.msg ?? (message.payload is Map ? message.payload['url'] : null);

    return MessageWithAvatar(
      isSelf: isSelf,
      avatarUrl: isSelf ? null : widget.friendAvatar,
      userName: isSelf ? null : message.fromName,
      child: GestureDetector(
        onTap: () {
          // TODO: 查看大图
          if (imageUrl != null) {
            _showImagePreview(imageUrl);
          }
        },
        child: ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: Image.network(
            imageUrl ?? '',
            width: 200,
            fit: BoxFit.cover,
            errorBuilder: (context, error, stackTrace) {
              return Container(
                width: 200,
                height: 150,
                color: Colors.grey.shade300,
                child: const Center(
                  child: Icon(Icons.broken_image, color: Colors.grey),
                ),
              );
            },
            loadingBuilder: (context, child, loadingProgress) {
              if (loadingProgress == null) return child;
              return Container(
                width: 200,
                height: 150,
                color: Colors.grey.shade200,
                child: Center(
                  child: CircularProgressIndicator(
                    value: loadingProgress.expectedTotalBytes != null
                        ? loadingProgress.cumulativeBytesLoaded /
                              loadingProgress.expectedTotalBytes!
                        : null,
                  ),
                ),
              );
            },
          ),
        ),
      ),
    );
  }

  void _showImagePreview(String imageUrl) {
    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.transparent,
        insetPadding: EdgeInsets.zero,
        child: Stack(
          fit: StackFit.expand,
          children: [
            GestureDetector(
              onTap: () => Navigator.of(context).pop(),
              child: InteractiveViewer(
                child: Image.network(imageUrl, fit: BoxFit.contain),
              ),
            ),
            Positioned(
              top: MediaQuery.of(context).padding.top + 8,
              right: 8,
              child: IconButton(
                icon: const Icon(Icons.close, color: Colors.white),
                onPressed: () => Navigator.of(context).pop(),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
