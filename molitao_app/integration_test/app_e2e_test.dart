import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:patrol/patrol.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  patrolTest('完整用户流程 - 启动应用并浏览首页', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    await $('首页').tap();
    expect($('首页'), findsOneWidget);
  });

  patrolTest('完整用户流程 - 导航到论坛页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final forumTab = $('论坛');
    await forumTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到拍卖页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final auctionTab = $('拍卖');
    await auctionTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到消息页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final messageTab = $('消息');
    await messageTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到我的页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final profileTab = $('我的');
    await profileTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 滚动列表', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    await $.tester.fling(find.byType(ListView), Offset(0, -300), 1000);
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 返回首页', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final homeTab = $('首页');
    await homeTab.tap();
    await $.pumpAndSettle();
    expect($('首页'), findsOneWidget);
  });
}
