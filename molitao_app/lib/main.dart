import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:device_preview/device_preview.dart';
import 'app.dart';
import 'data/services/wechat_service.dart';
import 'data/services/push_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final wechatService = WeChatService();
  await wechatService.initialize();

  final pushService = PushService();
  await pushService.init();

  runApp(
    DevicePreview(
      enabled: true,
      builder: (context) => ProviderScope(child: MyApp()),
    ),
  );
}

class MyApp extends StatelessWidget {
  MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return App();
  }
}