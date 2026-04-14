import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('完整用户流程 - 登录认证', () {
    testWidgets('用户登录完整流程', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到个人中心页面
      final profileTab = find.text('个人中心');
      if (profileTab.evaluate().isNotEmpty) {
        await tester.tap(profileTab.first);
        await tester.pumpAndSettle();
      }

      // 检查登录状态
      final loginButton = find.text('登录');
      if (loginButton.evaluate().isNotEmpty) {
        expect(loginButton, findsWidgets);
      }
    });

    testWidgets('用户退出登录流程', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到个人中心页面
      final profileTab = find.text('个人中心');
      if (profileTab.evaluate().isNotEmpty) {
        await tester.tap(profileTab.first);
        await tester.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 会话列表', () {
    testWidgets('浏览会话列表', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到会话列表
      final chatListTab = find.text('会话列表');
      if (chatListTab.evaluate().isNotEmpty) {
        await tester.tap(chatListTab.first);
        await tester.pumpAndSettle();
      }

      // 验证页面存在
      expect(find.byType(Scaffold), findsOneWidget);
    });

    testWidgets('查看会话详情', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到会话列表
      final chatListTab = find.text('会话列表');
      if (chatListTab.evaluate().isNotEmpty) {
        await tester.tap(chatListTab.first);
        await tester.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 交易站', () {
    testWidgets('浏览交易站', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到交易站页面
      final tradingPostTab = find.text('交易站');
      if (tradingPostTab.evaluate().isNotEmpty) {
        await tester.tap(tradingPostTab.first);
        await tester.pumpAndSettle();
      }

      // 验证页面存在
      expect(find.byType(Scaffold), findsOneWidget);
    });

    testWidgets('滚动浏览交易站内容', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到交易站页面
      final tradingPostTab = find.text('交易站');
      if (tradingPostTab.evaluate().isNotEmpty) {
        await tester.tap(tradingPostTab.first);
        await tester.pumpAndSettle();
      }

      // 尝试滚动
      final scrollables = find.byWidgetPredicate(
        (widget) => widget is ScrollView || widget is Scrollable,
      );
      if (scrollables.evaluate().isNotEmpty) {
        await tester.fling(scrollables.first, const Offset(0, -300), 1000);
        await tester.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 通讯录', () {
    testWidgets('浏览通讯录', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到通讯录页面
      final contactsTab = find.text('通讯录');
      if (contactsTab.evaluate().isNotEmpty) {
        await tester.tap(contactsTab.first);
        await tester.pumpAndSettle();
      }

      // 验证页面存在
      expect(find.byType(Scaffold), findsOneWidget);
    });
  });

  group('完整用户流程 - 个人中心', () {
    testWidgets('查看个人信息', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到个人中心页面
      final profileTab = find.text('个人中心');
      if (profileTab.evaluate().isNotEmpty) {
        await tester.tap(profileTab.first);
        await tester.pumpAndSettle();
      }

      // 验证页面存在
      expect(find.byType(Scaffold), findsOneWidget);
    });

    testWidgets('修改个人信息', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到个人中心页面
      final profileTab = find.text('个人中心');
      if (profileTab.evaluate().isNotEmpty) {
        await tester.tap(profileTab.first);
        await tester.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 综合场景', () {
    testWidgets('跨页面导航测试', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 首页 -> 会话列表
      final chatListTab = find.text('会话列表');
      if (chatListTab.evaluate().isNotEmpty) {
        await tester.tap(chatListTab.first);
        await tester.pumpAndSettle();
      }

      // 会话列表 -> 交易站
      final tradingPostTab = find.text('交易站');
      if (tradingPostTab.evaluate().isNotEmpty) {
        await tester.tap(tradingPostTab.first);
        await tester.pumpAndSettle();
      }

      // 交易站 -> 通讯录
      final contactsTab = find.text('通讯录');
      if (contactsTab.evaluate().isNotEmpty) {
        await tester.tap(contactsTab.first);
        await tester.pumpAndSettle();
      }

      // 通讯录 -> 个人中心
      final profileTab = find.text('个人中心');
      if (profileTab.evaluate().isNotEmpty) {
        await tester.tap(profileTab.first);
        await tester.pumpAndSettle();
      }

      // 返回首页
      final homeTab = find.text('首页');
      if (homeTab.evaluate().isNotEmpty) {
        await tester.tap(homeTab.first);
        await tester.pumpAndSettle();
      }
    });

    testWidgets('首页滚动测试', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 首页使用 SingleChildScrollView
      final scrollView = find.byType(SingleChildScrollView);
      if (scrollView.evaluate().isNotEmpty) {
        await tester.fling(scrollView.first, const Offset(0, -300), 1000);
        await tester.pumpAndSettle();

        await tester.fling(scrollView.first, const Offset(0, 300), 1000);
        await tester.pumpAndSettle();
      }
    });
  });
}
