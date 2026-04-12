import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'data/services/navigation_service.dart';
import 'presentation/widgets/push_notification_banner.dart';

class App extends ConsumerWidget {
  App({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
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
