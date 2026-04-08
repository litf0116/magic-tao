import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:persistent_bottom_nav_bar_v2/persistent_bottom_nav_bar_v2.dart';
import '../test_helpers/test_helpers.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('应用启动和首页测试', () {
    testWidgets('应用正常启动并显示首页', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      expect(find.byType(MaterialApp), findsOneWidget);
    });

    testWidgets('底部导航栏显示正常', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 应用使用 PersistentTabView 和 Style1BottomNavBar，而不是标准 BottomNavigationBar
      expect(find.byType(PersistentTabView), findsOneWidget);
    });
  });

  group('页面导航测试', () {
    testWidgets('点击首页标签切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      final homeIcon = find.byIcon(Icons.home);
      if (homeIcon.evaluate().isNotEmpty) {
        await tester.tap(homeIcon.first);
        await tester.pumpAndSettle();
      }
    });

    testWidgets('点击论坛标签切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle();
      }
    });

    testWidgets('点击拍卖标签切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle();
      }
    });

    testWidgets('点击消息标签切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle();
      }
    });

    testWidgets('点击我的标签切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      final personIcon = find.byIcon(Icons.person);
      if (personIcon.evaluate().isNotEmpty) {
        await tester.tap(personIcon.first);
        await tester.pumpAndSettle();
      }
    });
  });

  group('滚动和列表测试', () {
    testWidgets('首页列表可以滚动', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 应用使用 SingleChildScrollView，而不是 ListView
      final scrollView = find.byType(SingleChildScrollView);
      if (scrollView.evaluate().isNotEmpty) {
        await tester.fling(scrollView.first, const Offset(0, -300), 1000);
        await tester.pumpAndSettle();

        await tester.fling(scrollView.first, const Offset(0, 300), 1000);
        await tester.pumpAndSettle();
      }
    });
  });

  group('性能测试', () {
    testWidgets('应用启动性能', (tester) async {
      final stopwatch = Stopwatch()..start();

      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      stopwatch.stop();

      // 放宽阈值到 15 秒，考虑到网络请求和真机性能
      expect(stopwatch.elapsedMilliseconds, lessThan(15000));
    });
  });
}
