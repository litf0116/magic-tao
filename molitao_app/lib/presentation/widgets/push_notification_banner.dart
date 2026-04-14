import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/current_route_provider.dart';
import '../../data/services/push_service.dart';

/// 推送通知横幅组件
class PushNotificationBanner extends ConsumerStatefulWidget {
  final Widget child;

  const PushNotificationBanner({super.key, required this.child});

  @override
  ConsumerState<PushNotificationBanner> createState() =>
      _PushNotificationBannerState();
}

class _PushNotificationBannerState
    extends ConsumerState<PushNotificationBanner> {
  StreamSubscription<PushMessage>? _messageSubscription;
  Timer? _dismissTimer;

  // 当前显示的通知
  PushMessage? _currentMessage;
  bool _isVisible = false;

  @override
  void initState() {
    super.initState();
    _listenToPushMessages();
  }

  void _listenToPushMessages() {
    _messageSubscription = PushService().onMessage.listen((message) {
      _showNotification(message);
    });
  }

  void _showNotification(PushMessage message) {
    // 检查当前页面是否应该屏蔽
    final routeNotifier = ref.read(currentRouteProvider.notifier);
    if (routeNotifier.shouldSuppressBanner()) {
      debugPrint('[PushBanner] 当前页面屏蔽横幅: ${routeNotifier.currentLocation}');
      return;
    }

    debugPrint('[PushBanner] 显示通知: ${message.title} - ${message.content}');

    setState(() {
      _currentMessage = message;
      _isVisible = true;
    });

    // 5秒后自动消失
    _dismissTimer?.cancel();
    _dismissTimer = Timer(const Duration(seconds: 5), () {
      _dismiss();
    });
  }

  void _dismiss() {
    if (mounted) {
      setState(() {
        _isVisible = false;
      });
    }
    _dismissTimer?.cancel();
  }

  void _onTapDetail() {
    final message = _currentMessage;
    if (message == null) return;

    debugPrint('[PushBanner] 点击查看详情: ${message.extras}');

    // 根据类型跳转
    final type = message.type;
    final auctionItemId = message.auctionItemId;

    if (auctionItemId != null && auctionItemId.isNotEmpty) {
      // 跳转到拍品详情页
      // TODO: 实现跳转逻辑
      debugPrint('[PushBanner] 跳转拍品: $auctionItemId');
    }

    _dismiss();
  }

  @override
  void dispose() {
    _messageSubscription?.cancel();
    _dismissTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      textDirection: TextDirection.ltr,
      children: [
        widget.child,
        // 顶部横幅
        AnimatedPositioned(
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
          top: _isVisible ? 0 : -120,
          left: 0,
          right: 0,
          child: SafeArea(
            bottom: false,
            child: AnimatedOpacity(
              duration: const Duration(milliseconds: 200),
              opacity: _isVisible ? 1.0 : 0.0,
              child: _buildBanner(),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildBanner() {
    final message = _currentMessage;
    if (message == null) return const SizedBox.shrink();

    return Directionality(
      textDirection: TextDirection.ltr,
      child: Container(
        margin: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 10,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: Row(
              children: [
                // 图标
                Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: const Color(0xFFF4835A).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: const Icon(
                    Icons.notifications_active,
                    color: Color(0xFFF4835A),
                    size: 20,
                  ),
                ),
                const SizedBox(width: 12),
                // 内容
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        message.title,
                        style: const TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                          color: Color(0xFF333333),
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        message.content,
                        style: const TextStyle(
                          fontSize: 12,
                          color: Color(0xFF666666),
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                // 查看详情按钮
                TextButton(
                  onPressed: _onTapDetail,
                  style: TextButton.styleFrom(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 6,
                    ),
                    minimumSize: Size.zero,
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                  ),
                  child: const Text(
                    '查看',
                    style: TextStyle(
                      fontSize: 12,
                      color: Color(0xFFF4835A),
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
                // 关闭按钮
                IconButton(
                  onPressed: _dismiss,
                  icon: const Icon(Icons.close, size: 18),
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(
                    minWidth: 24,
                    minHeight: 24,
                  ),
                  color: const Color(0xFF999999),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
