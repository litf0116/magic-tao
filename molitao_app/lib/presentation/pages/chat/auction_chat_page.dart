import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

import '../../../data/models/auction_item_model.dart';
import '../../../data/models/chat_message_model.dart';
import '../../providers/auction_provider.dart';
import '../../providers/chat_store.dart';
import '../../providers/user_provider.dart';
import '../../widgets/chat/chat_input_area.dart';
import '../../widgets/chat/messages/message_widget.dart';

/// 拍卖聊天页面（秒杀场）
/// 与 UniApp auction.vue 保持一致
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

    // 初始化动画
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

    // 初始化 - 与 UniApp onload 一致
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      // 加载拍卖列表 (今日榜单)
      ref.read(auctionProvider.notifier).loadAuctions();

      // 连接 WebSocket
      await ref.read(chatStoreProvider.notifier).connectServer();

      // 设置当前聊天
      ref
          .read(chatStoreProvider.notifier)
          .setCurrentChatId(_channelId, name: _channelName);

      // 加入频道
      await ref.read(chatStoreProvider.notifier).joinChannel(_channel);

      // 加载历史消息
      await ref.read(chatStoreProvider.notifier).getGroupHistory(_channel);

      // 滚动到底部
      _scrollToBottom();
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
      final messages = ref.read(currentChatMessagesProvider);
      final lastTime = messages.isNotEmpty ? messages.last.time : null;
      if (lastTime != null) {
        ref
            .read(chatStoreProvider.notifier)
            .getGroupHistory(_channel, lastTime: lastTime);
      }
    }
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      // 等待下一帧渲染完成后再滚动，确保消息已添加到列表
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (_scrollController.hasClients) {
          _scrollController.animateTo(
            _scrollController.position.maxScrollExtent,
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeOut,
          );
        }
      });
    }
  }

  Future<void> _onSendText(String text) async {
    await ref
        .read(chatStoreProvider.notifier)
        .sendChannelMsg(
          channel: _channel,
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
        // TODO: 上传图片后发送
        await ref
            .read(chatStoreProvider.notifier)
            .sendChannelMsg(
              channel: _channel,
              message: image.path,
              type: ChatMessageType.image,
            );
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
        content: TextField(
          controller: priceController,
          keyboardType: TextInputType.number,
          decoration: InputDecoration(
            hintText: auctionState.isKasec
                ? '卡秒模式-需三倍加价(最低$minPrice R)'
                : '请输入出价金额(最低$minPrice R)',
            suffixText: 'R',
          ),
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

              Navigator.pop(context);

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
    final messages = ref.watch(currentChatMessagesProvider);
    final chatState = ref.watch(chatStoreProvider);
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
              _buildAnnouncementBar(),
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
          _buildRightSideButtons(onAuctionItem),
          if (_showUnreadNotification)
            _buildNewMessageButton(chatState.unreadCount),
          if (_showAuctionList) _buildAuctionListPanel(auctionState),
        ],
      ),
    );
  }

  Widget _buildRightSideButtons(dynamic onAuctionItem) {
    return Positioned(
      right: 0,
      top: 100,
      child: Column(
        children: [
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
          if (onAuctionItem != null && onAuctionItem.id != null) ...[
            GestureDetector(
              onTap: () => _showAuctionDetail(onAuctionItem),
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                decoration: BoxDecoration(
                  color: const Color(0xFFFF7144),
                  borderRadius: BorderRadius.circular(8),
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
                ),
                child: const Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      '秒杀中',
                      style: TextStyle(color: Colors.white, fontSize: 10),
                    ),
                    Text(
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
      child: const Row(
        children: [
          Icon(Icons.campaign, color: Colors.white, size: 16),
          SizedBox(width: 8),
          Expanded(
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
                  onTap: () => _onMessageTap(message),
                ),
              ],
            ),
          ),
          if (isSelf) ...[const SizedBox(width: 10), _buildAvatar(message)],
        ],
      ),
    );
  }

  /// 处理消息点击事件
  void _onMessageTap(ChatMessage message) {
    // 拍卖相关消息点击后显示拍品详情
    if (message.type == ChatMessageType.auctionStart ||
        message.type == ChatMessageType.auctionBid ||
        message.type == ChatMessageType.auctionEnd ||
        message.type == ChatMessageType.auctionDeal) {
      _showAuctionDetailFromMessage(message);
    }
  }

  /// 从消息中提取拍品信息并显示详情
  void _showAuctionDetailFromMessage(ChatMessage message) {
    final payload = message.payload;
    if (payload == null) return;

    // payload 已经在 ChatMessage.fromJson 中解析为 Map
    if (payload is Map<String, dynamic>) {
      // 使用 AuctionItemDto.fromJson 解析拍品信息
      final item = AuctionItemDto.fromJson(payload);
      _showAuctionDetail(item);
    }
  }

  int? _getCurrentUserId() {
    final userState = ref.read(userProvider);
    return userState.user?.id;
  }

  Widget _buildAvatar(ChatMessage message) {
    // 优先使用消息中的 avatar 字段
    String? avatarUrl;
    if (message.avatar != null) {
      final avatar = message.avatar!;
      avatarUrl = avatar.startsWith('http')
          ? avatar
          : 'https://image.molitao.top/$avatar';
    }

    if (avatarUrl == null || avatarUrl.isEmpty) {
      // 显示默认头像（颜色块 + 文字）
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

  void _showAuctionDetail(AuctionItemDto item) {
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
            mainAxisSize: MainAxisSize.min,
            children: [
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
              const SizedBox(height: 12),
              // 状态标签和开拍通知按钮
              Row(
                children: [
                  _buildStatusBadge(item.status),
                  const Spacer(),
                  // 待拍卖状态显示开拍通知按钮
                  if (item.status == AuctionStatusEnum.listed)
                    ElevatedButton(
                      onPressed: () => _subscribeNotification(item.id),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF4CAF50),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 8,
                        ),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: const Text('开拍通知'),
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  /// 构建状态标签
  Widget _buildStatusBadge(AuctionStatusEnum? status) {
    String text;
    Color color;

    switch (status) {
      case AuctionStatusEnum.auctioning:
        text = '拍卖中';
        color = const Color(0xFF4CAF50);
        break;
      case AuctionStatusEnum.listed:
        text = '待拍卖';
        color = const Color(0xFF999999);
        break;
      case AuctionStatusEnum.sold:
        text = '已成交';
        color = const Color(0xFF4CAF50);
        break;
      default:
        text = '未知状态';
        color = const Color(0xFF999999);
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        text,
        style: const TextStyle(color: Colors.white, fontSize: 12),
      ),
    );
  }

  /// 订阅开拍通知
  Future<void> _subscribeNotification(int? auctionItemId) async {
    if (auctionItemId == null) return;

    final success = await ref
        .read(auctionProvider.notifier)
        .subscribeStartNotification(auctionItemId);

    if (mounted) {
      Navigator.pop(context); // 关闭弹窗
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(success ? '订阅成功，秒杀开始时将推送通知' : '订阅失败，请重试'),
          backgroundColor: success ? const Color(0xFF4CAF50) : Colors.red,
        ),
      );
    }
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

  Widget _buildNewMessageButton(int unreadCount) {
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
            ),
            child: Text(
              '$unreadCount条新消息',
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
              onTap: () {},
              child: SlideTransition(
                position: _auctionListAnimation,
                child: Container(
                  width: 280,
                  height: double.infinity,
                  color: Colors.white,
                  child: Column(
                    children: [
                      // Tab header
                      Container(
                        height: 48,
                        margin: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.grey.shade300),
                        ),
                        child: Row(
                          children: [
                            Expanded(
                              child: GestureDetector(
                                onTap: () {
                                  ref
                                      .read(auctionProvider.notifier)
                                      .setActiveAuctionTab(1);
                                },
                                child: Container(
                                  decoration: BoxDecoration(
                                    color: auctionState.activeAuctionTab == 1
                                        ? const Color(
                                            0xFFF4835A,
                                          ) // Active tab background
                                        : Colors
                                              .white, // Inactive tab background
                                    borderRadius: BorderRadius.horizontal(
                                      left: Radius.circular(8),
                                    ),
                                    border: Border.all(
                                      color: Colors.grey.shade300,
                                      width: auctionState.activeAuctionTab == 1
                                          ? 0
                                          : 1,
                                    ),
                                  ),
                                  child: Center(
                                    child: Text(
                                      '今日榜单',
                                      style: TextStyle(
                                        color:
                                            auctionState.activeAuctionTab == 1
                                            ? Colors
                                                  .white // Active tab text
                                            : const Color(
                                                0xFF666666,
                                              ), // Inactive tab text
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                            ),
                            Expanded(
                              child: GestureDetector(
                                onTap: () {
                                  ref
                                      .read(auctionProvider.notifier)
                                      .setActiveAuctionTab(2);
                                },
                                child: Container(
                                  decoration: BoxDecoration(
                                    color: auctionState.activeAuctionTab == 2
                                        ? const Color(
                                            0xFFF4835A,
                                          ) // Active tab background
                                        : Colors
                                              .white, // Inactive tab background
                                    borderRadius: BorderRadius.horizontal(
                                      right: Radius.circular(8),
                                    ),
                                    border: Border.all(
                                      color: Colors.grey.shade300,
                                      width: auctionState.activeAuctionTab == 2
                                          ? 0
                                          : 1,
                                    ),
                                  ),
                                  child: Center(
                                    child: Text(
                                      '昨日成交',
                                      style: TextStyle(
                                        color:
                                            auctionState.activeAuctionTab == 2
                                            ? Colors
                                                  .white // Active tab text
                                            : const Color(
                                                0xFF666666,
                                              ), // Inactive tab text
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      // Tab content area
                      Expanded(
                        child: (() {
                          if (auctionState.activeAuctionTab == 1) {
                            // Tab 1: 今日榜单 (Today's list - listed and auctioning items)
                            final todayList = auctionState.todayList;

                            if (auctionState.isLoading && todayList.isEmpty) {
                              return const Center(
                                child: CircularProgressIndicator(),
                              );
                            }

                            if (todayList.isEmpty) {
                              return const Center(child: Text('暂无拍品'));
                            }

                            return ListView.builder(
                              padding: const EdgeInsets.all(8),
                              itemCount: todayList.length,
                              itemBuilder: (context, index) {
                                final item = todayList[index];
                                final isAuctioning =
                                    item.status == AuctionStatusEnum.auctioning;

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
                                                    color: Colors.grey.shade200,
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
                                        color: isAuctioning
                                            ? const Color(
                                                0xFF4CAF50,
                                              ) // Green for auctioning
                                            : Colors.grey, // Gray for listed
                                        borderRadius: BorderRadius.circular(4),
                                      ),
                                      child: Text(
                                        isAuctioning ? '拍卖中' : '待拍',
                                        style: const TextStyle(
                                          color: Colors.white,
                                          fontSize: 12,
                                        ),
                                      ),
                                    ),
                                    onTap: () => _showAuctionDetail(item),
                                  ),
                                );
                              },
                            );
                          } else {
                            // Tab 2: 昨日成交 (Yesterday's list - sold items)
                            if (auctionState.isLoading &&
                                auctionState.yesterdayList.isEmpty) {
                              return const Center(
                                child: CircularProgressIndicator(),
                              );
                            }

                            if (auctionState.yesterdayList.isEmpty) {
                              return const Center(child: Text('暂无成交'));
                            }

                            return ListView.builder(
                              padding: const EdgeInsets.all(8),
                              itemCount: auctionState.yesterdayList.length,
                              itemBuilder: (context, index) {
                                final item = auctionState.yesterdayList[index];

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
                                                    color: Colors.grey.shade200,
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
                                      '¥${item.finalPrice ?? item.currentPrice ?? item.startingPrice ?? 0}',
                                    ),
                                    trailing: Container(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 8,
                                        vertical: 4,
                                      ),
                                      decoration: BoxDecoration(
                                        color: const Color(
                                          0xFF4CAF50,
                                        ), // Green for sold
                                        borderRadius: BorderRadius.circular(4),
                                      ),
                                      child: Text(
                                        '已成交',
                                        style: const TextStyle(
                                          color: Colors.white,
                                          fontSize: 12,
                                        ),
                                      ),
                                    ),
                                    onTap: () => _showAuctionDetail(item),
                                  ),
                                );
                              },
                            );
                          }
                        })(),
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
