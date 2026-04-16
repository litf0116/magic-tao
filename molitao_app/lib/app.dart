import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'data/services/navigation_service.dart';
import 'data/services/push_service.dart';
import 'presentation/widgets/push_notification_banner.dart';

class App extends ConsumerStatefulWidget {
  App({super.key});

  @override
  ConsumerState<App> createState() => _AppState();
}

class _AppState extends ConsumerState<App> {
  StreamSubscription<PushMessage>? _pushClickSubscription;

  @override
  void initState() {
    super.initState();
    _listenToPushClicks();
  }

  void _listenToPushClicks() {
    _pushClickSubscription = PushService().onClick.listen((message) {
      debugPrint('[App] 推送点击: ${message.extras}');

      WidgetsBinding.instance.addPostFrameCallback((_) {
        _handlePushNavigation(message);
      });
    });
  }

  void _handlePushNavigation(PushMessage message) {
    final router = NavigationService.instance.router;
    if (router == null) return;

    final type = message.type;
    final auctionItemId = message.auctionItemId;
    final path = message.path;

    if (path != null && path.isNotEmpty) {
      router.push(path);
    } else if (auctionItemId != null && auctionItemId.isNotEmpty) {
      router.push('/chat/auction');
    } else if (type == 'auction') {
      router.push('/chat/auction');
    } else if (type == 'chat' && message.extras['chatId'] != null) {
      final chatId = message.extras['chatId'];
      final chatType = message.extras['chatType'] ?? 'private';
      final chatName = message.extras['chatName'] ?? '聊天';

      if (chatType == 'group') {
        router.push(
          '/chat/group/$chatId?name=${Uri.encodeComponent(chatName)}',
        );
      } else {
        router.push(
          '/chat/private/$chatId?name=${Uri.encodeComponent(chatName)}',
        );
      }
    }
  }

  @override
  void dispose() {
    _pushClickSubscription?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);

    NavigationService.instance.setRouter(router);

    return PushNotificationBanner(
      child: MaterialApp.router(
        title: '魔力淘',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.lightTheme,
        routerConfig: router,
      ),
    );
  }
}
