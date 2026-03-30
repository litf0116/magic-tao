import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/group_chat_provider.dart';
import '../../widgets/chat/message_bubble.dart';
import '../../../core/widgets/chat_input_bar.dart';
import '../../../data/models/chat_message_model.dart';

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
      groupChatProvider((
        channel: widget.channel,
        channelId: widget.channelId,
        channelName: widget.channelName,
      )),
    );

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.channelName),
        backgroundColor: const Color(0xFFf4835a),
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
        return SystemMessageBubble(text: '${message.fromName} 加入了群聊');
      case ChatMessageType.banUser:
        return SystemMessageBubble(text: message.msg ?? '用户已被禁言');
      case ChatMessageType.backout:
        return const SystemMessageBubble(text: '消息已撤回');
      case ChatMessageType.auctionStart:
        return _buildAuctionMessage(message, '拍卖开始', const Color(0xFF4CAF50));
      case ChatMessageType.auctionBid:
        return _buildAuctionMessage(message, '出价', const Color(0xFFFF9800));
      case ChatMessageType.auctionEnd:
        return _buildAuctionMessage(message, '拍卖结束', const Color(0xFFF44336));
      case ChatMessageType.auctionDeal:
        return _buildAuctionMessage(message, '成交', const Color(0xFF2196F3));
      case ChatMessageType.kasecStatusChanged:
        return _buildKasecStatusMessage(message);
      default:
        return _buildTextMessage(message, isSelf);
    }
  }

  Widget _buildTextMessage(ChatMessage message, bool isSelf) {
    return MessageWithAvatar(
      isSelf: isSelf,
      avatarUrl: message.avatar,
      userName: isSelf ? null : message.fromName,
      onAvatarTap: () {
        // TODO: 查看用户资料
      },
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 管理员标签
          if (!isSelf && message.fromAdmin == true && message.fromTag != null)
            Container(
              margin: const EdgeInsets.only(bottom: 4),
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: _getTagColor(message.tagClass),
                borderRadius: BorderRadius.circular(4),
              ),
              child: Text(
                message.fromTag!,
                style: const TextStyle(fontSize: 10, color: Colors.white),
              ),
            ),
          // 消息内容
          Text(
            message.msg ?? '',
            style: TextStyle(
              fontSize: 15,
              color: isSelf ? Colors.white : Colors.black87,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildImageMessage(ChatMessage message, bool isSelf) {
    final imageUrl =
        message.msg ?? (message.payload is Map ? message.payload['url'] : null);

    return MessageWithAvatar(
      isSelf: isSelf,
      avatarUrl: message.avatar,
      userName: isSelf ? null : message.fromName,
      onAvatarTap: () {
        // TODO: 查看用户资料
      },
      child: GestureDetector(
        onTap: () {
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

  Widget _buildAuctionMessage(ChatMessage message, String title, Color color) {
    return MessageWithAvatar(
      isSelf: false,
      avatarUrl: message.avatar,
      userName: message.fromName,
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: color, width: 2),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.gavel, color: color, size: 16),
                const SizedBox(width: 4),
                Text(
                  title,
                  style: TextStyle(color: color, fontWeight: FontWeight.bold),
                ),
              ],
            ),
            const SizedBox(height: 8),
            if (message.payload != null)
              Text(
                _getAuctionDescription(message.payload),
                style: const TextStyle(fontSize: 14),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildKasecStatusMessage(ChatMessage message) {
    return SystemMessageBubble(text: message.msg ?? '卡秒状态已变更');
  }

  String _getAuctionDescription(dynamic payload) {
    if (payload is Map) {
      final name = payload['name'] ?? payload['Name'] ?? '';
      final price = payload['price'] ?? payload['Price'] ?? '';
      if (name.isNotEmpty && price.isNotEmpty) {
        return '$name - ¥$price';
      }
      return name.toString();
    }
    return '';
  }

  Color _getTagColor(String? tagClass) {
    switch (tagClass) {
      case 'tag_AuctionManager':
        return const Color(0xFFFF5722);
      case 'tag_Admin':
        return const Color(0xFFE91E63);
      default:
        return const Color(0xFFf4835a);
    }
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
