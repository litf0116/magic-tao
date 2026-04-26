import 'dart:async';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'data/services/navigation_service.dart';
import 'data/services/push_service.dart';
import 'data/api/api_client.dart';
import 'data/api/api_endpoints.dart';
import 'presentation/widgets/push_notification_banner.dart';

final appUpdateInfoProvider = StateProvider<Map<String, dynamic>?>((ref) => null);

class App extends ConsumerStatefulWidget {
  const App({super.key});

  @override
  ConsumerState<App> createState() => _AppState();
}

class _AppState extends ConsumerState<App> {
  StreamSubscription<PushMessage>? _pushClickSubscription;

  @override
  void initState() {
    super.initState();
    _listenToPushClicks();
    _checkAppUpdate();
  }

  Future<void> _checkAppUpdate() async {
    if (!mounted) return;
    await Future.delayed(const Duration(seconds: 2));
    if (!mounted) return;
    try {
      final packageInfo = await PackageInfo.fromPlatform();
      debugPrint('[App] 当前版本: ${packageInfo.version}');

      final response = await ApiClient().dio.get(
        ApiEndpoints.checkUpdate,
        queryParameters: {
          'platform': Platform.isIOS ? 'ios' : 'android',
          'currentVersionCode': 0,
          'versionName': packageInfo.version,
        },
      );

      if (!mounted) return;
      final data = response.data;
      debugPrint('[App] 检查更新响应: $data');

      if (data != null && data['hasUpdate'] == true) {
        ref.read(appUpdateInfoProvider.notifier).state = data;
      }
    } catch (e) {
      debugPrint('[App] 检查更新失败: $e');
    }
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
