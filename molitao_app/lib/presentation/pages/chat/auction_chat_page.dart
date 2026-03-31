import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/group_chat_provider.dart';
import '../../providers/user_provider.dart';
import '../../../data/models/chat_message_model.dart';
import '../../widgets/chat/messages/message_widget.dart';
import '../../widgets/chat/chat_input_area.dart';

/// 拍卖聊天页面（秒杀场）
///
/// 这是一个特殊的群聊页面，channel 固定为 '-1_auction'
/// 包含拍卖特有的功能：出价、查看拍品列表等
class AuctionChatPage extends ConsumerStatefulWidget {
  const AuctionChatPage({super.key});

  @override
  ConsumerState<AuctionChatPage> createState() => _AuctionChatPageState();
}

class _AuctionChatPageState extends ConsumerState<AuctionChatPage> {
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();
  bool _showAuctionList = false;

  static const String _channel = '-1_auction';
  static const int _channelId = -1;
  static const String _channelName = '秒杀场';

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
              channel: _channel,
              channelId: _channelId,
              channelName: _channelName,
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
        channel: _channel,
        channelId: _channelId,
        channelName: _channelName,
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
            channel: _channel,
            channelId: _channelId,
            channelName: _channelName,
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

  void _showBidDialog() {
    final TextEditingController priceController = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('出价'),
        content: TextField(
          controller: priceController,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(
            hintText: '请输入出价金额',
            suffixText: 'R',
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () {
              final price = int.tryParse(priceController.text);
              if (price != null && price >= 5) {
                Navigator.pop(context);
                _submitBid(price);
              } else {
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('最低出价为5R')));
              }
            },
            child: const Text('确定'),
          ),
        ],
      ),
    );
  }

  void _submitBid(int price) {
    // TODO: 调用出价 API
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text('出价成功: $price R')));
  }

  @override
  Widget build(BuildContext context) {
    final chatState = ref.watch(
      groupChatProvider((
        channel: _channel,
        channelId: _channelId,
        channelName: _channelName,
      )),
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text(_channelName),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.list),
            onPressed: () {
              setState(() {
                _showAuctionList = !_showAuctionList;
              });
            },
          ),
        ],
      ),
      body: Stack(
        children: [
          Column(
            children: [
              // 公告栏
              _buildAnnouncementBar(),

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
              ChatInputArea(
                onSendText: _onSendText,
                onSelectImage: _onPickImage,
              ),
            ],
          ),

          // 拍卖商品列表侧边栏
          if (_showAuctionList) _buildAuctionListPanel(),

          // 快速出价按钮
          _buildQuickBidButton(),

          // 新消息提醒按钮
          _buildNewMessageButton(),
        ],
      ),
    );
  }

  Widget _buildAnnouncementBar() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      color: const Color(0xFFFF7144),
      child: Row(
        children: [
          const Icon(Icons.campaign, color: Colors.white, size: 16),
          const SizedBox(width: 8),
          const Expanded(
            child: Text(
              '欢迎来到秒杀场！点击右上角列表查看拍品',
              style: TextStyle(color: Colors.white, fontSize: 13),
              overflow: TextOverflow.ellipsis,
            ),
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
          Icon(Icons.gavel, size: 64, color: Colors.grey.shade400),
          const SizedBox(height: 16),
          Text(
            '秒杀场暂无消息',
            style: TextStyle(fontSize: 16, color: Colors.grey.shade500),
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

  Widget _buildAuctionListPanel() {
    return Positioned(
      right: 0,
      top: 0,
      bottom: 0,
      width: 280,
      child: GestureDetector(
        onTap: () {
          setState(() {
            _showAuctionList = false;
          });
        },
        child: Container(
          color: Colors.black54,
          child: Align(
            alignment: Alignment.centerRight,
            child: GestureDetector(
              onTap: () {},
              child: Container(
                width: 260,
                color: Colors.white,
                child: Column(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(16),
                      color: const Color(0xFFF4835A),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text(
                            '秒杀榜',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.close, color: Colors.white),
                            onPressed: () {
                              setState(() {
                                _showAuctionList = false;
                              });
                            },
                          ),
                        ],
                      ),
                    ),
                    Expanded(
                      child: ListView.builder(
                        padding: const EdgeInsets.all(8),
                        itemCount: 3,
                        itemBuilder: (context, index) {
                          return Card(
                            margin: const EdgeInsets.symmetric(vertical: 4),
                            child: ListTile(
                              leading: Container(
                                width: 50,
                                height: 50,
                                color: Colors.grey.shade200,
                                child: const Icon(Icons.image),
                              ),
                              title: Text('拍品 ${index + 1}'),
                              subtitle: Text('¥${(index + 1) * 100}'),
                              trailing: Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 8,
                                  vertical: 4,
                                ),
                                decoration: BoxDecoration(
                                  color: index == 0
                                      ? const Color(0xFF4CAF50)
                                      : Colors.grey,
                                  borderRadius: BorderRadius.circular(4),
                                ),
                                child: Text(
                                  index == 0 ? '拍卖中' : '待拍',
                                  style: const TextStyle(
                                    color: Colors.white,
                                    fontSize: 12,
                                  ),
                                ),
                              ),
                              onTap: () {
                                // TODO: 显示拍品详情
                              },
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildQuickBidButton() {
    return Positioned(
      right: 10,
      bottom: 150,
      child: GestureDetector(
        onTap: _showBidDialog,
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
          decoration: BoxDecoration(
            color: const Color(0xFFFF4D4F),
            borderRadius: BorderRadius.circular(10),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.15),
                blurRadius: 8,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                '秒杀中',
                style: TextStyle(color: Colors.white, fontSize: 10),
              ),
              const Text(
                '出价',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 14,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildNewMessageButton() {
    return Positioned(
      right: 10,
      top: 100,
      child: GestureDetector(
        onTap: () {
          // TODO: 滚动到最新消息
          _scrollToBottom();
        },
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          decoration: BoxDecoration(
            color: const Color(0xFFF4835A),
            borderRadius: const BorderRadius.only(
              topLeft: Radius.circular(8),
              bottomLeft: Radius.circular(8),
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.1),
                blurRadius: 4,
                offset: const Offset(0, 2),
              ),
            ],
          ),
          child: const Text(
            '新消息 3',
            style: TextStyle(color: Colors.white, fontSize: 12),
          ),
        ),
      ),
    );
  }
}
