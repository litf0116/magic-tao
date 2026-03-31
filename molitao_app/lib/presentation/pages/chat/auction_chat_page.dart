import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../providers/group_chat_provider.dart';
import '../../providers/user_provider.dart';
import '../../providers/auction_provider.dart';
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

class _AuctionChatPageState extends ConsumerState<AuctionChatPage>
    with TickerProviderStateMixin {
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();

  // UI 状态
  bool _showAuctionList = false;
  bool _showUnreadNotification = false;
  int _unreadCount = 0;

  // 动画控制器
  late AnimationController _auctionListAnimationController;
  late AnimationController _unreadAnimationController;
  late Animation<Offset> _auctionListAnimation;
  late Animation<Offset> _unreadAnimation;

  static const String _channel = '-1_auction';
  static const int _channelId = -1;
  static const String _channelName = '秒杀场';

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);

    // 初始化拍卖列表动画
    _auctionListAnimationController = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );
    _auctionListAnimation =
        Tween<Offset>(begin: const Offset(1.0, 0.0), end: Offset.zero).animate(
          CurvedAnimation(
            parent: _auctionListAnimationController,
            curve: Curves.easeOut,
          ),
        );

    // 初始化未读消息动画
    _unreadAnimationController = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );
    _unreadAnimation =
        Tween<Offset>(begin: const Offset(1.0, 0.0), end: Offset.zero).animate(
          CurvedAnimation(
            parent: _unreadAnimationController,
            curve: Curves.easeOut,
          ),
        );

    // 加载拍卖列表
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(auctionProvider.notifier).loadAuctions();
    });
  }

  @override
  void dispose() {
    _scrollController.removeListener(_onScroll);
    _scrollController.dispose();
    _auctionListAnimationController.dispose();
    _unreadAnimationController.dispose();
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

  /// 显示新消息通知
  void _showUnreadNotificationWithCount(int count) {
    if (count <= 0) return;

    setState(() {
      _unreadCount = count;
      _showUnreadNotification = true;
    });

    _unreadAnimationController.forward();

    // 3秒后自动隐藏
    Future.delayed(const Duration(seconds: 3), () {
      if (mounted) {
        _unreadAnimationController.reverse().then((_) {
          if (mounted) {
            setState(() {
              _showUnreadNotification = false;
            });
          }
        });
      }
    });
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
    final auctionState = ref.read(auctionProvider);
    final onAuctionItem = auctionState.onAuctionItem;

    if (onAuctionItem == null) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('没有正在秒杀的商品')));
      return;
    }

    final currentPrice =
        onAuctionItem.currentPrice ?? onAuctionItem.startingPrice ?? 0;
    final minPrice = auctionState.isKasec
        ? (currentPrice * 3).ceil()
        : (currentPrice + 5).ceil();

    final TextEditingController priceController = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(auctionState.isKasec ? '卡秒出价' : '出价'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: priceController,
              keyboardType: TextInputType.number,
              decoration: InputDecoration(
                hintText: auctionState.isKasec
                    ? '卡秒模式-需三倍加价(最低$minPrice R)'
                    : '请输入出价金额(最低$minPrice R)',
                suffixText: 'R',
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () async {
              final price = int.tryParse(priceController.text);
              if (price == null) {
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('请输入数字')));
                return;
              }

              if (price < 5) {
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('最低出价为5R')));
                return;
              }

              if (auctionState.isKasec && price < minPrice) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text('卡秒模式需要三倍加价，最低出价为$minPrice R')),
                );
                return;
              }

              Navigator.pop(context);

              // 调用出价 API
              final success = await ref
                  .read(auctionProvider.notifier)
                  .bid(onAuctionItem.id!, price.toDouble());

              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(success ? '出价成功: $price R' : '出价失败，请重试'),
                  ),
                );
              }
            },
            child: const Text('确定'),
          ),
        ],
      ),
    );
  }

  void _toggleAuctionList() {
    setState(() {
      _showAuctionList = !_showAuctionList;
    });

    if (_showAuctionList) {
      _auctionListAnimationController.forward();
    } else {
      _auctionListAnimationController.reverse();
    }
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
    final auctionState = ref.watch(auctionProvider);
    final onAuctionItem = auctionState.onAuctionItem;

    return Scaffold(
      appBar: AppBar(
        title: const Text(_channelName),
        backgroundColor: const Color(0xFFF4835A),
        foregroundColor: Colors.white,
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

          // 右侧垂直按钮条
          _buildRightSideButtons(onAuctionItem),

          // 新消息提醒按钮
          if (_showUnreadNotification) _buildNewMessageButton(),

          // 拍卖商品列表侧边栏
          if (_showAuctionList) _buildAuctionListPanel(auctionState),
        ],
      ),
    );
  }

  /// 右侧垂直按钮条（秒杀榜 + 出价按钮）
  Widget _buildRightSideButtons(dynamic onAuctionItem) {
    return Positioned(
      right: 0,
      top: 100,
      child: Column(
        children: [
          // 秒杀榜按钮
          GestureDetector(
            onTap: _toggleAuctionList,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 12),
              decoration: const BoxDecoration(
                color: Color(0xFFFF7144),
                borderRadius: BorderRadius.only(
                  topLeft: Radius.circular(8),
                  bottomLeft: Radius.circular(8),
                ),
                boxShadow: [
                  BoxShadow(
                    color: Color(0x1A000000),
                    blurRadius: 4,
                    offset: Offset(0, 2),
                  ),
                ],
              ),
              child: const Text(
                '秒杀榜',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 14,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),

          const SizedBox(height: 12),

          // 出价按钮区域（只有正在拍卖时才显示）
          if (onAuctionItem != null && onAuctionItem.id != null) ...[
            // 拍品详情按钮
            GestureDetector(
              onTap: () {
                _showAuctionDetail(onAuctionItem);
              },
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                decoration: BoxDecoration(
                  color: const Color(0xFFFF7144),
                  borderRadius: BorderRadius.circular(8),
                  boxShadow: const [
                    BoxShadow(
                      color: Color(0x1A000000),
                      blurRadius: 4,
                      offset: Offset(0, 2),
                    ),
                  ],
                ),
                child: const Text(
                  '拍品详情',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 12,
                    decoration: TextDecoration.underline,
                  ),
                ),
              ),
            ),

            const SizedBox(height: 8),

            // 出价按钮
            GestureDetector(
              onTap: _showBidDialog,
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 12,
                  vertical: 12,
                ),
                decoration: BoxDecoration(
                  color: const Color(0xFFFF4D4F),
                  borderRadius: BorderRadius.circular(10),
                  boxShadow: const [
                    BoxShadow(
                      color: Color(0x26000000),
                      blurRadius: 8,
                      offset: Offset(0, 4),
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
                        decoration: TextDecoration.underline,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
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
              '欢迎来到秒杀场！点击右侧"秒杀榜"查看拍品',
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
                if (!isSelf) _buildUserName(message),
                const SizedBox(height: 4),
                MessageWidget(
                  message: message,
                  onTap: () => _handleMessageTap(message),
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

  void _showAuctionDetail(dynamic item) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.6,
        minChildSize: 0.3,
        maxChildSize: 0.9,
        expand: false,
        builder: (context, scrollController) => Container(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // 标题
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Text(
                      item.name ?? '拍品详情',
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFFFF7144),
                      ),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(context),
                  ),
                ],
              ),

              const SizedBox(height: 16),

              // 图片
              if (item.imageUrl != null)
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    item.imageUrl!,
                    width: double.infinity,
                    height: 200,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                      height: 200,
                      color: Colors.grey.shade200,
                      child: const Center(
                        child: Icon(Icons.image, size: 48, color: Colors.grey),
                      ),
                    ),
                  ),
                ),

              const SizedBox(height: 16),

              // 价格信息
              Row(
                children: [
                  const Text('当前价格: ', style: TextStyle(fontSize: 14)),
                  Text(
                    '¥${item.currentPrice ?? item.startingPrice ?? 0}',
                    style: const TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFFFF4D4F),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 8),

              // 状态
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 12,
                  vertical: 6,
                ),
                decoration: BoxDecoration(
                  color: const Color(0xFF4CAF50),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: const Text(
                  '拍卖中',
                  style: TextStyle(color: Colors.white, fontSize: 12),
                ),
              ),
            ],
          ),
        ),
      ),
    );
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

  Widget _buildNewMessageButton() {
    return Positioned(
      right: 60,
      top: 60,
      child: SlideTransition(
        position: _unreadAnimation,
        child: GestureDetector(
          onTap: _scrollToBottom,
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            decoration: const BoxDecoration(
              color: Color(0xFFF4835A),
              borderRadius: BorderRadius.only(
                topLeft: Radius.circular(8),
                bottomLeft: Radius.circular(8),
              ),
              boxShadow: [
                BoxShadow(
                  color: Color(0x1A000000),
                  blurRadius: 4,
                  offset: Offset(0, 2),
                ),
              ],
            ),
            child: Text(
              '$_unreadCount条新消息',
              style: const TextStyle(color: Colors.white, fontSize: 12),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildAuctionListPanel(dynamic auctionState) {
    return Positioned.fill(
      child: GestureDetector(
        onTap: _toggleAuctionList,
        child: Container(
          color: Colors.black54,
          child: Align(
            alignment: Alignment.centerRight,
            child: GestureDetector(
              onTap: () {}, // 阻止点击穿透
              child: SlideTransition(
                position: _auctionListAnimation,
                child: Container(
                  width: 280,
                  height: double.infinity,
                  color: Colors.white,
                  child: Column(
                    children: [
                      // 标题栏
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
                              icon: const Icon(
                                Icons.close,
                                color: Colors.white,
                              ),
                              onPressed: _toggleAuctionList,
                            ),
                          ],
                        ),
                      ),

                      // 列表
                      Expanded(
                        child: auctionState.isLoading
                            ? const Center(child: CircularProgressIndicator())
                            : auctionState.auctionList.isEmpty
                            ? const Center(child: Text('暂无拍品'))
                            : ListView.builder(
                                padding: const EdgeInsets.all(8),
                                itemCount: auctionState.auctionList.length,
                                itemBuilder: (context, index) {
                                  final item = auctionState.auctionList[index];
                                  final isActive =
                                      item.status?.name == 'auctioning';

                                  return Card(
                                    margin: const EdgeInsets.symmetric(
                                      vertical: 4,
                                    ),
                                    child: ListTile(
                                      leading: ClipRRect(
                                        borderRadius: BorderRadius.circular(4),
                                        child: item.imageUrl != null
                                            ? Image.network(
                                                item.imageUrl!,
                                                width: 50,
                                                height: 50,
                                                fit: BoxFit.cover,
                                                errorBuilder: (_, __, ___) =>
                                                    Container(
                                                      width: 50,
                                                      height: 50,
                                                      color:
                                                          Colors.grey.shade200,
                                                      child: const Icon(
                                                        Icons.image,
                                                        size: 24,
                                                      ),
                                                    ),
                                              )
                                            : Container(
                                                width: 50,
                                                height: 50,
                                                color: Colors.grey.shade200,
                                                child: const Icon(
                                                  Icons.image,
                                                  size: 24,
                                                ),
                                              ),
                                      ),
                                      title: Text(
                                        item.name ?? '未知拍品',
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      subtitle: Text(
                                        '¥${item.currentPrice ?? item.startingPrice ?? 0}',
                                      ),
                                      trailing: Container(
                                        padding: const EdgeInsets.symmetric(
                                          horizontal: 8,
                                          vertical: 4,
                                        ),
                                        decoration: BoxDecoration(
                                          color: isActive
                                              ? const Color(0xFF4CAF50)
                                              : Colors.grey,
                                          borderRadius: BorderRadius.circular(
                                            4,
                                          ),
                                        ),
                                        child: Text(
                                          isActive ? '拍卖中' : '待拍',
                                          style: const TextStyle(
                                            color: Colors.white,
                                            fontSize: 12,
                                          ),
                                        ),
                                      ),
                                      onTap: () {
                                        _showAuctionDetail(item);
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
      ),
    );
  }
}
