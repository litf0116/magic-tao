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
    await Future.delayed(const Duration(seconds: 2));
    try {
      final packageInfo = await PackageInfo.fromPlatform();
      final buildNumber = int.tryParse(packageInfo.buildNumber) ?? 1;

      final response = await ApiClient().dio.get(
        ApiEndpoints.checkUpdate,
        queryParameters: {
          'platform': Platform.isIOS ? 'ios' : 'android',
          'currentVersionCode': buildNumber,
        },
      );

      if (response.data != null && response.data['success'] == true) {
        final result = response.data['result'];
        if (result != null && result['hasUpdate'] == true) {
          if (mounted) {
            _showUpdateNotification(result);
          }
        }
      }
    } catch (e) {
      debugPrint('[App] 检查更新失败: $e');
    }
  }

  void _showUpdateNotification(Map<String, dynamic> updateInfo) {
    final latestVersion = updateInfo['latestVersionName'] ?? '';
    final description = updateInfo['description'] ?? '发现新版本';
    final downloadUrl = updateInfo['downloadUrl'];

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '发现新版本 v$latestVersion',
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
            if (description.isNotEmpty)
              Text(
                description,
                style: const TextStyle(fontSize: 12),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
          ],
        ),
        backgroundColor: const Color(0xfff4835a),
        duration: const Duration(seconds: 5),
        action: SnackBarAction(
          label: '更新',
          textColor: Colors.white,
          onPressed: () {
            if (downloadUrl != null) {
              // Navigate to update or open download URL
            }
          },
        ),
      ),
    );
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
