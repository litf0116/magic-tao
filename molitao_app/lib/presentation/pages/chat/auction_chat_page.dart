import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/group_chat_provider.dart';
import '../../widgets/chat/message_bubble.dart';
import '../../../core/widgets/chat_input_bar.dart';
import '../../../data/models/chat_message_model.dart';

/// 拍卖聊天页面
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
  bool _showMorePanel = false;
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
        backgroundColor: const Color(0xFFf4835a),
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

          // 拍卖商品列表侧边栏
          if (_showAuctionList) _buildAuctionListPanel(),

          // 快速出价按钮
          _buildQuickBidButton(),
        ],
      ),
    );
  }

  Widget _buildAnnouncementBar() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      color: const Color(0xFFf4835a),
      child: const Text(
        '欢迎来到秒杀场！点击右上角列表查看拍品',
        style: TextStyle(color: Colors.white, fontSize: 13),
        overflow: TextOverflow.ellipsis,
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
      padding: const EdgeInsets.symmetric(vertical: 8),
      itemCount: messages.length,
      itemBuilder: (context, index) {
        final message = messages[index];
        return _buildMessageItem(message);
      },
    );
  }

  Widget _buildMessageItem(ChatMessage message) {
    final isSelf =
        message.status == ChatMessageStatus.sending ||
        message.status == ChatMessageStatus.success;

    switch (message.type) {
      case ChatMessageType.text:
        return _buildTextMessage(message, isSelf);
      case ChatMessageType.image:
        return _buildImageMessage(message, isSelf);
      case ChatMessageType.welcome:
        return SystemMessageBubble(text: '${message.fromName} 加入了秒杀场');
      case ChatMessageType.banUser:
        return SystemMessageBubble(text: message.msg ?? '用户已被禁言');
      case ChatMessageType.backout:
        return const SystemMessageBubble(text: '消息已撤回');
      case ChatMessageType.auctionStart:
        return _buildAuctionStartMessage(message);
      case ChatMessageType.auctionBid:
        return _buildAuctionBidMessage(message);
      case ChatMessageType.auctionEnd:
        return _buildAuctionEndMessage(message);
      case ChatMessageType.auctionDeal:
        return _buildAuctionDealMessage(message);
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
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (!isSelf && message.fromAdmin == true && message.fromTag != null)
            Container(
              margin: const EdgeInsets.only(bottom: 4),
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: const Color(0xFFf4835a),
                borderRadius: BorderRadius.circular(4),
              ),
              child: Text(
                message.fromTag!,
                style: const TextStyle(fontSize: 10, color: Colors.white),
              ),
            ),
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
          ),
        ),
      ),
    );
  }

  Widget _buildAuctionStartMessage(ChatMessage message) {
    return MessageWithAvatar(
      isSelf: false,
      avatarUrl: message.avatar,
      userName: message.fromName,
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: const Color(0xFF4CAF50), width: 2),
        ),
        child: Row(
          children: [
            const Icon(Icons.gavel, color: Color(0xFF4CAF50), size: 20),
            const SizedBox(width: 8),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    '拍卖开始',
                    style: TextStyle(
                      color: Color(0xFF4CAF50),
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  if (message.payload != null)
                    Text(
                      _getAuctionName(message.payload),
                      style: const TextStyle(fontSize: 14),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAuctionBidMessage(ChatMessage message) {
    final payload = message.payload;
    String bidInfo = '';
    if (payload is Map) {
      final name = payload['name'] ?? '';
      final price = payload['currentPrice'] ?? payload['price'] ?? '';
      bidInfo = price.isNotEmpty ? '$name - ¥$price' : name;
    }

    return MessageWithAvatar(
      isSelf: false,
      avatarUrl: message.avatar,
      userName: message.fromName,
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: const Color(0xFFFF9800), width: 2),
        ),
        child: Row(
          children: [
            const Icon(Icons.attach_money, color: Color(0xFFFF9800), size: 20),
            const SizedBox(width: 8),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    '出价',
                    style: TextStyle(
                      color: Color(0xFFFF9800),
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  if (bidInfo.isNotEmpty)
                    Text(bidInfo, style: const TextStyle(fontSize: 14)),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAuctionEndMessage(ChatMessage message) {
    return MessageWithAvatar(
      isSelf: false,
      avatarUrl: message.avatar,
      userName: message.fromName,
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: const Color(0xFFF44336), width: 2),
        ),
        child: const Row(
          children: [
            Icon(Icons.stop_circle, color: Color(0xFFF44336), size: 20),
            SizedBox(width: 8),
            Text(
              '拍卖结束',
              style: TextStyle(
                color: Color(0xFFF44336),
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAuctionDealMessage(ChatMessage message) {
    return MessageWithAvatar(
      isSelf: false,
      avatarUrl: message.avatar,
      userName: message.fromName,
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: const Color(0xFF2196F3), width: 2),
        ),
        child: Row(
          children: [
            const Icon(Icons.check_circle, color: Color(0xFF2196F3), size: 20),
            const SizedBox(width: 8),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    '成交',
                    style: TextStyle(
                      color: Color(0xFF2196F3),
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  if (message.payload != null)
                    Text(
                      _getAuctionDescription(message.payload),
                      style: const TextStyle(fontSize: 14),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildKasecStatusMessage(ChatMessage message) {
    return SystemMessageBubble(text: message.msg ?? '卡秒状态已变更');
  }

  String _getAuctionName(dynamic payload) {
    if (payload is Map) {
      return payload['name'] ?? payload['Name'] ?? '';
    }
    return '';
  }

  String _getAuctionDescription(dynamic payload) {
    if (payload is Map) {
      final name = payload['name'] ?? payload['Name'] ?? '';
      final price = payload['finalPrice'] ?? payload['price'] ?? '';
      if (name.isNotEmpty && price.isNotEmpty) {
        return '$name - ¥$price';
      }
      return name.toString();
    }
    return '';
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
                      color: const Color(0xFFf4835a),
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
      right: 8,
      bottom: 200,
      child: Column(
        children: [
          FloatingActionButton(
            heroTag: 'bid',
            mini: true,
            backgroundColor: const Color(0xFFf44336),
            onPressed: _showBidDialog,
            child: const Icon(Icons.gavel, color: Colors.white),
          ),
          const SizedBox(height: 4),
          const Text(
            '出价',
            style: TextStyle(
              fontSize: 12,
              color: Color(0xFFf44336),
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
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
