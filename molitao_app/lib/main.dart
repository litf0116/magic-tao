import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'app.dart';
import 'data/services/wechat_service.dart';
import 'data/services/push_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final wechatService = WeChatService();
  final pushService = PushService();

  // 非阻塞初始化，让服务在后台初始化
  wechatService.initialize().then((_) {
    debugPrint('[Main] 微信初始化完成');
  });

  pushService.init().then((_) {
    debugPrint('[Main] 推送初始化完成');
  });

  runApp(ProviderScope(child: MyApp()));
}

class MyApp extends StatelessWidget {
  MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return App();
  }
}
