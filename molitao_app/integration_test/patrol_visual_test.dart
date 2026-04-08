import 'package:flutter_test/flutter_test.dart';
import 'package:patrol/patrol.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';

void main() {
  patrolTest('应用启动并显示首页', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    expect($('首页'), findsOneWidget);
  });

  patrolTest('导航到会话列表页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    await $('会话列表').tap();
    await $.pumpAndSettle();

    expect($(Scaffold), findsOneWidget);
  });

  patrolTest('导航到交易站页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    await $('交易站').tap();
    await $.pumpAndSettle();

    expect($(Scaffold), findsOneWidget);
  });

  patrolTest('导航到通讯录页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    await $('通讯录').tap();
    await $.pumpAndSettle();

    expect($(Scaffold), findsOneWidget);
  });

  patrolTest('导航到个人中心页面', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    await $('个人中心').tap();
    await $.pumpAndSettle();

    expect($(Scaffold), findsOneWidget);
  });

  patrolTest('首页滚动测试', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    final scrollView = find.byType(SingleChildScrollView);
    if (scrollView.evaluate().isNotEmpty) {
      await $.tester.fling(scrollView.first, const Offset(0, -300), 1000);
      await $.pumpAndSettle();

      await $.tester.fling(scrollView.first, const Offset(0, 300), 1000);
      await $.pumpAndSettle();
    }
  });

  patrolTest('跨页面导航完整流程', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    await $('会话列表').tap();
    await $.pumpAndSettle();

    await $('交易站').tap();
    await $.pumpAndSettle();

    await $('通讯录').tap();
    await $.pumpAndSettle();

    await $('个人中心').tap();
    await $.pumpAndSettle();

    await $('首页').tap();
    await $.pumpAndSettle();

    expect($('首页'), findsOneWidget);
  });
}
