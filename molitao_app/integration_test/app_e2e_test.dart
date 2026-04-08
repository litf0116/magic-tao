import 'package:flutter_test/flutter_test.dart';
import 'package:patrol/patrol.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';

// Patrol 测试 - 不要使用 IntegrationTestWidgetsFlutterBinding
// 使用命令: patrol test -d 827af65d0722 integration_test/app_e2e_test.dart

void main() {
  patrolTest('完整用户流程 - 启动应用并浏览首页', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    await $('首页').tap();
    expect($('首页'), findsOneWidget);
  });

  patrolTest('完整用户流程 - 导航到会话列表页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final chatListTab = $('会话列表');
    await chatListTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到交易站页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final tradingPostTab = $('交易站');
    await tradingPostTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到通讯录页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final contactsTab = $('通讯录');
    await contactsTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 导航到个人中心页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final profileTab = $('个人中心');
    await profileTab.tap();
    await $.pumpAndSettle();
  });

  patrolTest('完整用户流程 - 滚动列表', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    // 首页使用 SingleChildScrollView，不是 ListView
    final scrollView = find.byType(SingleChildScrollView);
    if (scrollView.evaluate().isNotEmpty) {
      await $.tester.fling(scrollView.first, const Offset(0, -300), 1000);
      await $.pumpAndSettle();
    }
  });

  patrolTest('完整用户流程 - 返回首页', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));
    final homeTab = $('首页');
    await homeTab.tap();
    await $.pumpAndSettle();
    expect($('首页'), findsOneWidget);
  });
}
