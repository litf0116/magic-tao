import 'dart:async';
import 'dart:io';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_html/flutter_html.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';

import '../../../data/models/auction_item_model.dart';
import '../../../data/models/chat_message_model.dart';
import '../../../data/repositories/chat_repository.dart';
import '../../../data/repositories/friend_repository.dart';
import '../../../data/repositories/user_repository.dart';
import '../../../data/services/notification_permission_service.dart';
import '../../../data/services/upload_service.dart';
import '../../providers/auction_provider.dart';
import '../../providers/chat_emoji_store.dart';
import '../../providers/chat_store.dart';
import '../../providers/user_provider.dart';
import '../../widgets/chat/chat_input_area.dart';
import '../../widgets/chat/messages/message_widget.dart';
import '../../widgets/common/user_profile_dialog.dart';

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
  final UploadService _uploadService = UploadService();

  // UI 状态
  bool _showAuctionList = false;
  bool _showUnreadNotification = false;
  bool _isUploadingImage = false;
  bool _isLoadingMessages = false;

  // 历史消息加载状态
  bool _isLoadingHistory = false;
  bool _hasMoreHistory = true;

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

    WidgetsBinding.instance.addPostFrameCallback((_) async {
      ref.read(auctionProvider.notifier).loadAuctions();

      await ref.read(chatStoreProvider.notifier).connectServer();

      ref
          .read(chatStoreProvider.notifier)
          .setCurrentChatId(_channelId, name: _channelName);

      await ref.read(chatStoreProvider.notifier).joinChannel(_channel);

      setState(() => _isLoadingMessages = true);
      await ref.read(chatStoreProvider.notifier).getGroupHistory(_channel);
      setState(() => _isLoadingMessages = false);

      // 滚动到底部
      _scrollToBottom();

      // 监听消息变化，自动滚动到底部（只在初始化时注册一次）
      ref.listenManual<List<ChatMessage>>(currentChatMessagesProvider, (
        previous,
        next,
      ) {
        print('[AuctionChat] ========== ref.listen 触发 (initState) ==========');
        print('[AuctionChat] previous.length = ${previous?.length}');
        print('[AuctionChat] next.length = ${next.length}');
        print('[AuctionChat] _isLoadingHistory = $_isLoadingHistory');

        // 消息数量增加且不在加载历史消息，说明有新消息
        if (next.length > previous!.length && !_isLoadingHistory) {
          print('[AuctionChat] ✅ 检测到新消息，准备滚动到底部');
          // 直接调用 _scrollToBottom，内部已有 addPostFrameCallback
          _scrollToBottom();
        } else {
          print('[AuctionChat] ❌ 不满足滚动条件');
        }
      });
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
    // 检测顶部加载历史消息
    if (_scrollController.position.pixels <= 0 &&
        !_isLoadingHistory &&
        _hasMoreHistory) {
      final messages = ref.read(currentChatMessagesProvider);
      if (messages.isNotEmpty) {
        final firstTime = messages.first.time;
        if (firstTime != null) {
          setState(() {
            _isLoadingHistory = true;
          });

          ref
              .read(chatStoreProvider.notifier)
              .getGroupHistory(_channel, lastTime: firstTime)
              .then((_) {
                if (mounted) {
                  setState(() {
                    _isLoadingHistory = false;
                    // 如果返回的消息少于预期，说明没有更多历史消息
                  });
                }
              })
              .catchError((error) {
                if (mounted) {
                  setState(() {
                    _isLoadingHistory = false;
                  });
                }
              });
        }
      }
    }
  }

  void _scrollToBottom() {
    print('[AuctionChat] _scrollToBottom() 被调用');
    print(
      '[AuctionChat] _scrollController.hasClients = ${_scrollController.hasClients}',
    );

    if (!_scrollController.hasClients) {
      print('[AuctionChat] ❌ _scrollController.hasClients = false，跳过滚动');
      return;
    }

    try {
      // 使用 addPostFrameCallback 确保在当前帧渲染完成后执行
      WidgetsBinding.instance.addPostFrameCallback((_) {
        print('[AuctionChat] addPostFrameCallback 触发');

        try {
          if (!_scrollController.hasClients) {
            print('[AuctionChat] ❌ callback: hasClients = false，无法滚动');
            return;
          }

          final currentPixels = _scrollController.position.pixels;
          final extent = _scrollController.position.maxScrollExtent;
          final viewportDimension =
              _scrollController.position.viewportDimension;

          print('[AuctionChat] ========== 滚动位置信息 ==========');
          print('[AuctionChat] 当前位置 pixels = $currentPixels');
          print('[AuctionChat] 最大滚动位置 maxScrollExtent = $extent');
          print('[AuctionChat] 视口高度 viewportDimension = $viewportDimension');
          print('[AuctionChat] 是否需要滚动 = ${currentPixels < extent}');
          print('[AuctionChat] =====================================');

          _scrollController.animateTo(
            extent,
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeOut,
          );
          print('[AuctionChat] ✅ 已调用 animateTo 滚动到 $extent');
        } catch (e, stackTrace) {
          print('[AuctionChat] ❌ callback 执行异常: $e');
          print('[AuctionChat] StackTrace: $stackTrace');
        }
      });
      print('[AuctionChat] addPostFrameCallback 已注册');
    } catch (e, stackTrace) {
      print('[AuctionChat] ❌ 注册 addPostFrameCallback 异常: $e');
      print('[AuctionChat] StackTrace: $stackTrace');
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
        // 获取当前用户 ID
        final userId = _getCurrentUserId();

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
                .sendChannelMsg(
                  channel: _channel,
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
        ).showSnackBar(SnackBar(content: Text('选择图片失败：$e')));
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
        title: Text(
          _channelName,
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
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
                showFavoriteTab: true,
                onSelectFavoriteEmoji: _onSelectFavoriteEmoji,
              ),
            ],
          ),
          _buildRightSideButtons(onAuctionItem),
          if (_showUnreadNotification)
            _buildNewMessageButton(chatState.unreadCount),
          if (_showAuctionList) _buildAuctionListPanel(auctionState),
          // 加载遮罩
          if (_isUploadingImage) _buildLoadingOverlay(),
          if (_isLoadingMessages) _buildMessageLoadingOverlay(),
        ],
      ),
    );
  }

  Widget _buildMessageLoadingOverlay() {
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
                '正在加载消息...',
                style: TextStyle(fontSize: 14, color: Colors.grey.shade700),
              ),
            ],
          ),
        ),
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
      // 底部预留输入框高度（56px）+ 额外空间，避免消息被遮挡
      padding: const EdgeInsets.only(left: 16, right: 16, top: 16, bottom: 16),
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
          if (!isSelf) ...[
            _buildAvatar(message, onTap: () => _onAvatarTap(message)),
            const SizedBox(width: 10),
          ],
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
                  onLongPress: () => _onMessageLongPress(message),
                ),
              ],
            ),
          ),
          if (isSelf) ...[
            const SizedBox(width: 10),
            _buildAvatar(message, onTap: () => _onAvatarTap(message)),
          ],
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

  /// 处理头像点击事件
  void _onAvatarTap(ChatMessage message) {
    _showMessageActionSheet(message, isAvatarTap: true);
  }

  /// 处理消息长按事件
  void _onMessageLongPress(ChatMessage message) {
    _showMessageActionSheet(message, isAvatarTap: false);
  }

  /// 显示消息操作菜单
  void _showMessageActionSheet(
    ChatMessage message, {
    bool isAvatarTap = false,
  }) {
    final currentUserId = _getCurrentUserId();
    final isSelf = message.from == currentUserId;
    final isImage = message.type == ChatMessageType.image;
    final isAdmin = ref.read(userProvider.notifier).isAdmin;
    final senderIsAdmin = message.fromAdmin ?? false;

    // 关闭键盘
    FocusScope.of(context).unfocus();

    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // 图片消息：收藏至表情（与 UniApp 一致，所有图片消息都可收藏）
            if (isImage) ...[
              ListTile(
                leading: const Icon(Icons.favorite_border),
                title: const Text('收藏至表情'),
                onTap: () {
                  Navigator.pop(context);
                  _addToFavorites(message);
                },
              ),
              const Divider(height: 1),
            ],
            // 撤销（仅自己的消息）
            if (isSelf) ...[
              ListTile(
                leading: const Icon(Icons.undo),
                title: const Text('撤销'),
                onTap: () {
                  Navigator.pop(context);
                  _backoutMessage(message);
                },
              ),
              const Divider(height: 1),
            ],
            // 加为好友（不是自己的消息）
            if (!isSelf) ...[
              ListTile(
                leading: const Icon(Icons.person_add),
                title: const Text('加为好友'),
                onTap: () {
                  Navigator.pop(context);
                  _addFriend(message);
                },
              ),
              const Divider(height: 1),
            ],
            // 私聊（管理员或发送者是管理员，且不是自己）
            if ((isAdmin || senderIsAdmin) && !isSelf) ...[
              ListTile(
                leading: const Icon(Icons.chat_bubble_outline),
                title: const Text('私聊'),
                onTap: () {
                  Navigator.pop(context);
                  _startPrivateChat(message);
                },
              ),
              const Divider(height: 1),
            ],
            // 查看资料（不是自己的消息）
            if (!isSelf) ...[
              ListTile(
                leading: const Icon(Icons.person),
                title: const Text('查看资料'),
                onTap: () {
                  Navigator.pop(context);
                  _viewUserInfo(message);
                },
              ),
              const Divider(height: 1),
            ],
            // 取消
            ListTile(
              leading: const Icon(Icons.close),
              title: const Text('取消'),
              onTap: () => Navigator.pop(context),
            ),
          ],
        ),
      ),
    );
  }

  /// 显示顶部 SnackBar 消息
  void _showTopSnackBar(
    BuildContext context,
    String message, {
    bool isError = false,
  }) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message, style: const TextStyle(color: Colors.white)),
        backgroundColor: isError
            ? const Color(0xFFF44336)
            : const Color(0xFF4CAF50),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        margin: EdgeInsets.only(
          left: 16,
          right: 16,
          top: MediaQuery.of(context).padding.top + 50,
          bottom: MediaQuery.of(context).padding.bottom + 80,
        ),
        duration: Duration(seconds: isError ? 3 : 2),
      ),
    );
  }

  /// 添加好友
  Future<void> _addFriend(ChatMessage message) async {
    if (message.from == null) return;

    try {
      final friendRepository = FriendRepository();
      await friendRepository.addFriend(message.from!);
      if (mounted) {
        _showTopSnackBar(context, '已发送好友申请');
      }
    } catch (e) {
      if (mounted) {
        final errorText = e.toString().replaceAll('Exception: ', '');
        _showTopSnackBar(context, errorText, isError: true);
      }
    }
  }

  /// 发起私聊
  void _startPrivateChat(ChatMessage message) {
    if (message.from == null) return;
    final friendId = message.from!;
    final friendName = message.fromName ?? '用户';
    final friendAvatar = message.avatar;
    context.push(
      '/chat/private/$friendId?name=$friendName${friendAvatar != null ? '&avatar=$friendAvatar' : ''}',
    );
  }

  /// 查看用户资料
  Future<void> _viewUserInfo(ChatMessage message) async {
    if (message.from == null) return;

    // 显示加载中
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => const Center(child: CircularProgressIndicator()),
    );

    try {
      final userRepository = UserRepository();
      final user = await userRepository.getUserById(message.from!);

      if (!mounted) return;

      // 关闭加载弹窗
      Navigator.pop(context);

      if (user != null) {
        UserProfileDialog.show(context, user);
      } else {
        _showTopSnackBar(context, '获取用户信息失败', isError: true);
      }
    } catch (e) {
      if (!mounted) return;
      Navigator.pop(context);
      final errorText = e.toString().replaceAll('Exception: ', '');
      _showTopSnackBar(context, errorText, isError: true);
    }
  }

  /// 撤回消息
  Future<void> _backoutMessage(ChatMessage message) async {
    if (message.id == null) return;

    try {
      final chatRepository = ChatRepository();
      await chatRepository.backoutMessage(message.id!);
      // 从本地消息列表移除
      ref.read(chatStoreProvider.notifier).removeMessage(message.id!);
      if (mounted) {
        _showTopSnackBar(context, '已撤回');
      }
    } catch (e) {
      if (mounted) {
        final errorText = e.toString().replaceAll('Exception: ', '');
        _showTopSnackBar(context, errorText, isError: true);
      }
    }
  }

  /// 收藏至表情（图片消息）
  Future<void> _addToFavorites(ChatMessage message) async {
    final payload = message.payload;
    if (payload == null) return;

    // 获取图片 URL
    String? imageUrl;
    if (payload is Map<String, dynamic>) {
      imageUrl = payload['url'] as String?;
    } else if (payload is String) {
      imageUrl = payload;
    }

    if (imageUrl == null || imageUrl.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('无法获取图片地址')));
      }
      return;
    }

    try {
      // 调用收藏表情 API
      final success = await ref
          .read(userEmojiProvider.notifier)
          .addToEmoji(imageUrl);

      if (mounted) {
        if (success) {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(const SnackBar(content: Text('已添加到收藏表情')));
        } else {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(const SnackBar(content: Text('添加失败，请重试')));
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('添加失败: $e')));
      }
    }
  }

  /// 选择收藏表情发送（发送图片消息）
  void _onSelectFavoriteEmoji(dynamic emoji) {
    final url = emoji.url;
    if (url == null || url.isEmpty) return;

    // 构建 payload，与 UniApp 保持一致
    final payload = {'url': url, 'width': 200, 'height': 200};

    // 发送图片消息
    ref
        .read(chatStoreProvider.notifier)
        .sendChannelMsg(
          channel: _channel,
          message: url,
          type: ChatMessageType.image,
          payload: payload,
        );

    // 滚动到底部
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _scrollToBottom();
    });
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

  Widget _buildAvatar(ChatMessage message, {VoidCallback? onTap}) {
    // 优先使用消息中的 avatar 字段
    String? avatarUrl;
    if (message.avatar != null) {
      final avatar = message.avatar!;
      avatarUrl = avatar.startsWith('http')
          ? avatar
          : 'https://image.molitao.top/$avatar';
    }

    Widget avatarWidget;

    if (avatarUrl == null || avatarUrl.isEmpty) {
      // 显示默认头像（颜色块 + 文字）
      avatarWidget = Container(
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
    } else {
      // 显示网络头像
      avatarWidget = Container(
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

    return GestureDetector(onTap: onTap, child: avatarWidget);
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
    // 提取 description 中的图片 URL 列表（用于预览）
    final imageUrls = _extractImageUrls(item.description);

    // 关闭键盘，防止关闭 modal 后键盘弹出
    FocusScope.of(context).unfocus();

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
        builder: (context, scrollController) => SingleChildScrollView(
          controller: scrollController,
          child: Container(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                // 标题栏
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
                // 开拍通知按钮（待拍卖状态）
                if (item.status == AuctionStatusEnum.listed)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: ElevatedButton(
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
                  ),
                // 商品内容区域
                _buildAuctionContent(item, imageUrls),
              ],
            ),
          ),
        ),
      ),
    );
  }

  /// 从 description HTML 中提取图片 URL 列表
  List<String> _extractImageUrls(String? description) {
    if (description == null || description.isEmpty) return [];

    final urls = <String>[];

    // 提取 data-url 属性
    final dataUrlRegex = RegExp(
      r'<img[^>]+data-url=["\x27"]([^"\x27"]+)["\x27"]',
    );
    final dataUrlMatches = dataUrlRegex.allMatches(description);
    for (final match in dataUrlMatches) {
      urls.add(match.group(1)!);
    }

    // 如果没有 data-url，提取 src 属性
    if (urls.isEmpty) {
      final srcRegex = RegExp(r'<img[^>]+src=["\x27"]([^"\x27"]+)["\x27"]');
      final srcMatches = srcRegex.allMatches(description);
      for (final match in srcMatches) {
        final url = match.group(1)!;
        // 移除缩略图参数 !w300
        final cleanUrl = url.replaceAll(RegExp(r'!w300$'), '');
        urls.add(cleanUrl);
      }
    }

    return urls;
  }

  /// 转换图片 URL 为完整地址
  String? _convertImageUrl(String? url) {
    if (url == null || url.isEmpty) return null;

    // 清理 URL
    url = url.trim();

    // 处理 file:// 协议（无效的本地文件协议）
    if (url.startsWith('file://')) {
      // 提取路径部分，假设是相对路径
      url = url.replaceFirst('file://', '');
      if (!url.startsWith('/')) {
        url = '/$url';
      }
    }

    // 处理绝对路径（以 / 开头）
    if (url.startsWith('/')) {
      return 'https://image.molitao.top$url';
    } else if (!url.startsWith('http://') && !url.startsWith('https://')) {
      // 处理相对路径
      return 'https://image.molitao.top/$url';
    }

    return url;
  }

  /// 构建拍品内容（参考 UniApp getStartContent）
  Widget _buildAuctionContent(AuctionItemDto item, List<String> imageUrls) {
    final description = item.description;

    // 如果没有 description，显示 imageUrl
    if (description == null || description.trim().isEmpty) {
      final imageUrl = _convertImageUrl(item.imageUrl);
      if (imageUrl != null && imageUrl.isNotEmpty) {
        return GestureDetector(
          onTap: () => _previewImages([imageUrl]),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: CachedNetworkImage(
              imageUrl: imageUrl,
              width: double.infinity,
              height: 200,
              fit: BoxFit.cover,
              placeholder: (_, __) => Container(
                height: 200,
                color: Colors.grey.shade200,
                child: const Center(
                  child: SizedBox(
                    width: 24,
                    height: 24,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  ),
                ),
              ),
              errorWidget: (_, __, ___) => Container(
                height: 200,
                color: Colors.grey.shade200,
                child: const Center(
                  child: Icon(Icons.image, size: 48, color: Colors.grey),
                ),
              ),
            ),
          ),
        );
      }
      return const SizedBox.shrink();
    }

    // 渲染 description HTML
    return GestureDetector(
      onTap: () {
        // 点击内容区域预览图片
        if (imageUrls.isNotEmpty) {
          _previewImages(imageUrls);
        }
      },
      child: Html(
        data: description,
        style: {
          "body": Style(margin: Margins.zero, padding: HtmlPaddings.zero),
          "img": Style(width: Width(double.infinity)),
          "div": Style(fontSize: FontSize(14), color: Colors.black87),
          "span": Style(fontSize: FontSize(14), color: Colors.black87),
        },
        extensions: [
          TagExtension(
            tagsToExtend: {"img"},
            builder: (extensionContext) {
              // 优先使用 data-url，其次使用 src
              final dataUrl = extensionContext.attributes['data-url'];
              final src = extensionContext.attributes['src'];
              String? imageUrl = dataUrl ?? src;

              if (imageUrl == null) return const SizedBox.shrink();

              // 清理 URL
              imageUrl = imageUrl.trim();

              // 移除缩略图参数 !w300
              imageUrl = imageUrl.replaceAll(RegExp(r'!w300$'), '');

              // 处理 file:// 协议（无效的本地文件协议）
              if (imageUrl.startsWith('file://')) {
                // 提取路径部分，假设是相对路径
                imageUrl = imageUrl.replaceFirst('file://', '');
                if (!imageUrl.startsWith('/')) {
                  imageUrl = '/$imageUrl';
                }
              }

              // 处理绝对路径（以 / 开头）
              if (imageUrl.startsWith('/')) {
                imageUrl = 'https://image.molitao.top$imageUrl';
              } else if (!imageUrl.startsWith('http://') &&
                  !imageUrl.startsWith('https://')) {
                // 处理相对路径
                imageUrl = 'https://image.molitao.top/$imageUrl';
              }

              return GestureDetector(
                onTap: () {
                  if (imageUrls.isNotEmpty) {
                    _previewImages(imageUrls);
                  } else if (imageUrl != null) {
                    _previewImages([imageUrl]);
                  }
                },
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: CachedNetworkImage(
                    imageUrl: imageUrl,
                    width: double.infinity,
                    fit: BoxFit.cover,
                    placeholder: (_, __) => Container(
                      height: 150,
                      color: Colors.grey.shade200,
                      child: const Center(
                        child: SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        ),
                      ),
                    ),
                    errorWidget: (_, __, ___) => const SizedBox.shrink(),
                  ),
                ),
              );
            },
          ),
        ],
      ),
    );
  }

  /// 预览图片
  void _previewImages(List<String> urls) {
    if (urls.isEmpty) return;

    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) =>
            _ImagePreviewPage(imageUrls: urls, initialIndex: 0),
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
  /// 订阅开拍通知（简化版，仅系统订阅，不调用微信订阅消息）
  Future<void> _subscribeNotification(int? auctionItemId) async {
    debugPrint('[订阅通知] 开始订阅, auctionItemId=$auctionItemId');
    if (auctionItemId == null) {
      debugPrint('[订阅通知] auctionItemId 为空，返回');
      return;
    }

    final permissionService = NotificationPermissionService();
    final hasPermission = await permissionService.checkPermission();
    if (!hasPermission) {
      debugPrint('[订阅通知] 没有通知权限，弹窗提示');
      await permissionService.showPermissionDialog(context);
      return;
    }

    // 直接调用后端订阅接口
    await _saveSubscription(auctionItemId);
  }

  Future<void> _saveSubscription(int auctionItemId, {String? openid}) async {
    final success = await ref
        .read(auctionProvider.notifier)
        .subscribeStartNotification(auctionItemId, openid: openid);

    if (mounted) {
      Navigator.pop(context);
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

                                return Container(
                                  margin: const EdgeInsets.symmetric(
                                    vertical: 4,
                                  ),
                                  decoration: BoxDecoration(
                                    color: const Color(0xFFF5F5F5),
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                  child: ListTile(
                                    leading: ClipRRect(
                                      borderRadius: BorderRadius.circular(4),
                                      child:
                                          item.imageUrl != null &&
                                              item.imageUrl!.isNotEmpty
                                          ? CachedNetworkImage(
                                              imageUrl: item.imageUrl!,
                                              width: 50,
                                              height: 50,
                                              fit: BoxFit.cover,
                                              placeholder: (_, __) => Container(
                                                width: 50,
                                                height: 50,
                                                color: Colors.grey.shade200,
                                                child: const SizedBox(
                                                  width: 16,
                                                  height: 16,
                                                  child:
                                                      CircularProgressIndicator(
                                                        strokeWidth: 2,
                                                      ),
                                                ),
                                              ),
                                              errorWidget: (_, __, ___) =>
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
                                    subtitle: item.currentPrice != null
                                        ? Text('¥${item.currentPrice}')
                                        : null,
                                    trailing: Container(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 8,
                                        vertical: 4,
                                      ),
                                      decoration: BoxDecoration(
                                        color: isAuctioning
                                            ? const Color(0xFF4CAF50)
                                            : Colors.grey,
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

                                return Container(
                                  margin: const EdgeInsets.symmetric(
                                    vertical: 4,
                                  ),
                                  decoration: BoxDecoration(
                                    color: const Color(0xFFF5F5F5),
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                  child: ListTile(
                                    leading: ClipRRect(
                                      borderRadius: BorderRadius.circular(4),
                                      child:
                                          item.imageUrl != null &&
                                              item.imageUrl!.isNotEmpty
                                          ? CachedNetworkImage(
                                              imageUrl: item.imageUrl!,
                                              width: 50,
                                              height: 50,
                                              fit: BoxFit.cover,
                                              placeholder: (_, __) => Container(
                                                width: 50,
                                                height: 50,
                                                color: Colors.grey.shade200,
                                                child: const SizedBox(
                                                  width: 16,
                                                  height: 16,
                                                  child:
                                                      CircularProgressIndicator(
                                                        strokeWidth: 2,
                                                      ),
                                                ),
                                              ),
                                              errorWidget: (_, __, ___) =>
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
                                    subtitle:
                                        (item.finalPrice ??
                                                item.currentPrice) !=
                                            null
                                        ? Text(
                                            '¥${item.finalPrice ?? item.currentPrice}',
                                          )
                                        : null,
                                    trailing: Container(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 8,
                                        vertical: 4,
                                      ),
                                      decoration: BoxDecoration(
                                        color: const Color(0xFF4CAF50),
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

/// 图片预览页面
class _ImagePreviewPage extends StatefulWidget {
  final List<String> imageUrls;
  final int initialIndex;

  const _ImagePreviewPage({required this.imageUrls, this.initialIndex = 0});

  @override
  State<_ImagePreviewPage> createState() => _ImagePreviewPageState();
}

class _ImagePreviewPageState extends State<_ImagePreviewPage> {
  late PageController _pageController;
  late int _currentIndex;

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex;
    _pageController = PageController(initialPage: widget.initialIndex);
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        backgroundColor: Colors.black,
        foregroundColor: Colors.white,
        title: Text(
          '${_currentIndex + 1} / ${widget.imageUrls.length}',
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
      ),
      body: PageView.builder(
        controller: _pageController,
        itemCount: widget.imageUrls.length,
        onPageChanged: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
        itemBuilder: (context, index) {
          return InteractiveViewer(
            child: Center(
              child: CachedNetworkImage(
                imageUrl: widget.imageUrls[index],
                fit: BoxFit.contain,
                placeholder: (_, __) => const Center(
                  child: CircularProgressIndicator(
                    color: Colors.white,
                    strokeWidth: 2,
                  ),
                ),
                errorWidget: (_, __, ___) => const Center(
                  child: Icon(
                    Icons.broken_image,
                    color: Colors.white,
                    size: 64,
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
