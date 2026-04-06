import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/chat_store.dart';
import '../../providers/user_provider.dart';
import '../../../data/models/chat_message_model.dart';
import '../../../data/services/upload_service.dart';
import '../../widgets/chat/messages/message_widget.dart';
import '../../widgets/chat/chat_input_area.dart';

/// 私聊页面
/// 与 UniApp chatMain.vue 保持一致
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
  final UploadService _uploadService = UploadService();
  bool _isUploadingImage = false;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);

    // 初始化 - 与 UniApp onLoad 一致
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      // 连接 WebSocket
      await ref.read(chatStoreProvider.notifier).connectServer();

      // 设置当前聊天
      ref
          .read(chatStoreProvider.notifier)
          .setCurrentChatId(
            widget.friendId,
            name: widget.friendName,
            isGroup: false,
          );

      // 标记已读
      ref.read(chatStoreProvider.notifier).markAsRead(widget.friendId);

      // 加载历史消息
      await ref
          .read(chatStoreProvider.notifier)
          .getPrivateHistory(widget.friendId);

      // 滚动到底部
      _scrollToBottom();
    });
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
      final messages = ref.read(currentChatMessagesProvider);
      final lastTime = messages.isNotEmpty ? messages.last.time : null;
      if (lastTime != null) {
        ref
            .read(chatStoreProvider.notifier)
            .getPrivateHistory(widget.friendId, lastTime: lastTime);
      }
    }
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      // 第一次: 确保 UI rebuild 完成
      WidgetsBinding.instance.addPostFrameCallback((_) {
        // 第二次: rebuild 后的下一帧，列表已渲染完毕
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (_scrollController.hasClients &&
              _scrollController.position.maxScrollExtent > 0) {
            _scrollController.animateTo(
              _scrollController.position.maxScrollExtent,
              duration: const Duration(milliseconds: 300),
              curve: Curves.easeOut,
            );
          }
        });
      });
    }
  }

  Future<void> _onSendText(String text) async {
    await ref
        .read(chatStoreProvider.notifier)
        .sendDirectMsg(
          toUserId: widget.friendId,
          message: text,
          type: ChatMessageType.text,
        );
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
        // 获取当前用户 ID
        final userState = ref.read(userProvider);
        final userId = userState.user?.id;

        // 显示上传中状态
        setState(() {
          _isUploadingImage = true;
        });

        try {
          // 上传图片
          final imageUrl = await _uploadService.uploadImage(
            image.path,
            userId: userId?.toString(),
          );

          if (imageUrl != null) {
            // 获取图片尺寸
            final file = File(image.path);
            final bytes = await file.readAsBytes();
            final decodedImage = await decodeImageFromList(bytes);
            final width = decodedImage.width;
            final height = decodedImage.height;

            // 构建 payload，与 UniApp 保持一致
            final payload = {'url': imageUrl, 'width': width, 'height': height};

            // 发送图片消息
            await ref
                .read(chatStoreProvider.notifier)
                .sendDirectMsg(
                  toUserId: widget.friendId,
                  message: imageUrl,
                  type: ChatMessageType.image,
                  payload: payload,
                );
            _scrollToBottom();
          } else {
            if (mounted) {
              ScaffoldMessenger.of(
                context,
              ).showSnackBar(const SnackBar(content: Text('图片上传失败，请重试')));
            }
          }
        } finally {
          // 恢复上传状态
          if (mounted) {
            setState(() {
              _isUploadingImage = false;
            });
          }
        }
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isUploadingImage = false;
        });
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('选择图片失败: $e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final messages = ref.watch(currentChatMessagesProvider);

    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.friendName,
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
      ),
      body: Stack(
        children: [
          Column(
            children: [
              Expanded(
                child: Container(
                  color: const Color(0xFFFAF1F0),
                  child: messages.isEmpty
                      ? _buildEmptyState()
                      : _buildMessageList(messages),
                ),
              ),
              ChatInputArea(
                onSendText: _onSendText,
                onSelectImage: _onPickImage,
              ),
            ],
          ),
          // 加载遮罩
          if (_isUploadingImage) _buildLoadingOverlay(),
        ],
      ),
    );
  }

  Widget _buildLoadingOverlay() {
    return Container(
      color: Colors.black.withOpacity(0.3),
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const SizedBox(
                width: 32,
                height: 32,
                child: CircularProgressIndicator(
                  strokeWidth: 3,
                  valueColor: AlwaysStoppedAnimation<Color>(Color(0xFFF4835A)),
                ),
              ),
              const SizedBox(height: 16),
              Text(
                '正在发送图片...',
                style: TextStyle(fontSize: 14, color: Colors.grey.shade700),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.person, size: 64, color: Colors.grey.shade400),
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
    final isSelf = message.from != null && message.from == _getCurrentUserId();

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
          if (!isSelf) ...[_buildAvatar(message), const SizedBox(width: 10)],
          Flexible(
            child: Column(
              crossAxisAlignment: isSelf
                  ? CrossAxisAlignment.end
                  : CrossAxisAlignment.start,
              children: [
                MessageWidget(
                  message: message,
                  onTap: () => debugPrint('消息被点击: ${message.id}'),
                ),
              ],
            ),
          ),
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
    // 优先使用消息中的 avatar 字段
    String? avatarUrl;
    if (message.from != _getCurrentUserId() && message.avatar != null) {
      final avatar = message.avatar!;
      avatarUrl = avatar.startsWith('http')
          ? avatar
          : 'https://image.molitao.top/$avatar';
    }
    // 如果没有 message.avatar，使用传入的 friendAvatar 作为后备
    avatarUrl ??= widget.friendAvatar;

    if (avatarUrl == null || avatarUrl.isEmpty) {
      // 显示默认头像（文字）
      return Container(
        width: 36,
        height: 36,
        decoration: const BoxDecoration(
          color: Color(0xFFF4835A),
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

    // 显示网络头像
    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        image: DecorationImage(
          image: NetworkImage(avatarUrl),
          fit: BoxFit.cover,
        ),
      ),
    );
  }

  String _getAvatarText(String name) {
    if (name.isEmpty) return '用';
    return name.length > 2 ? name.substring(0, 2) : name;
  }
}
