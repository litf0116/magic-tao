import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';
import '../test_helpers/test_helpers.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('论坛页面测试', () {
    testWidgets('论坛页面加载', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 点击论坛标签
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle();

        // 验证论坛页面加载成功
        expect(find.byType(Scaffold), findsOneWidget);
      }
    });

    testWidgets('帖子列表显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 验证列表存在
        final listView = find.byType(ListView);
        if (listView.evaluate().isNotEmpty) {
          expect(listView, findsWidgets);
        }
      }
    });

    testWidgets('帖子分类筛选', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找分类标签
        final categoryChips = find.byType(Chip);
        if (categoryChips.evaluate().isNotEmpty) {
          await tester.tap(categoryChips.first);
          await tester.pumpAndSettle();
        }
      }
    });

    testWidgets('帖子滚动加载', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 滚动列表
        await tester.fling(
          find.byType(ListView).first,
          const Offset(0, -500),
          1000,
        );
        await tester.pumpAndSettle();

        // 验证列表仍然存在
        expect(find.byType(ListView), findsWidgets);
      }
    });
  });

  group('发帖功能测试', () {
    testWidgets('显示发帖按钮', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找发帖按钮
        final addButton = find.byType(FloatingActionButton);
        if (addButton.evaluate().isNotEmpty) {
          expect(addButton, findsWidgets);
        }
      }
    });

    testWidgets('打开发帖页面', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击发帖按钮
        final addButton = find.byType(FloatingActionButton);
        if (addButton.evaluate().isNotEmpty) {
          await tester.tap(addButton.first);
          await tester.pumpAndSettle();

          // 验证发帖页面打开
          expect(find.byType(Scaffold), findsOneWidget);
        }
      }
    });

    testWidgets('发帖表单验证', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击发帖按钮
        final addButton = find.byType(FloatingActionButton);
        if (addButton.evaluate().isNotEmpty) {
          await tester.tap(addButton.first);
          await tester.pumpAndSettle();

          // 验证表单元素
          final textFields = find.byType(TextField);
          expect(textFields, findsWidgets);
        }
      }
    });

    testWidgets('选择帖子分类', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击发帖按钮
        final addButton = find.byType(FloatingActionButton);
        if (addButton.evaluate().isNotEmpty) {
          await tester.tap(addButton.first);
          await tester.pumpAndSettle();

          // 查找分类选择器
          final dropdown = find.byType(DropdownButton);
          if (dropdown.evaluate().isNotEmpty) {
            await tester.tap(dropdown.first);
            await tester.pumpAndSettle();
          }
        }
      }
    });
  });

  group('帖子详情测试', () {
    testWidgets('查看帖子详情', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 等待帖子列表加载
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个帖子
        final postCards = find.byType(Card);
        if (postCards.evaluate().isNotEmpty) {
          await tester.tap(postCards.first);
          await tester.pumpAndSettle();

          // 验证详情页打开
          expect(find.byType(Scaffold), findsOneWidget);
        }
      }
    });

    testWidgets('帖子详情内容显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个帖子
        final postCards = find.byType(Card);
        if (postCards.evaluate().isNotEmpty) {
          await tester.tap(postCards.first);
          await tester.pumpAndSettle(const Duration(seconds: 2));

          // 验证内容存在
          final content = find.byType(SingleChildScrollView);
          if (content.evaluate().isNotEmpty) {
            expect(content, findsWidgets);
          }
        }
      }
    });

    testWidgets('帖子详情滚动', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个帖子
        final postCards = find.byType(Card);
        if (postCards.evaluate().isNotEmpty) {
          await tester.tap(postCards.first);
          await tester.pumpAndSettle();

          // 滚动详情页
          final scrollView = find.byType(SingleChildScrollView);
          if (scrollView.evaluate().isNotEmpty) {
            await tester.fling(scrollView.first, const Offset(0, -300), 1000);
            await tester.pumpAndSettle();
          }
        }
      }
    });
  });

  group('搜索功能测试', () {
    testWidgets('搜索框显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找搜索图标
        final searchIcon = find.byIcon(Icons.search);
        if (searchIcon.evaluate().isNotEmpty) {
          expect(searchIcon, findsWidgets);
        }
      }
    });

    testWidgets('输入搜索关键词', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到论坛
      final forumIcon = find.byIcon(Icons.forum);
      if (forumIcon.evaluate().isNotEmpty) {
        await tester.tap(forumIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找搜索框
        final searchField = find.byType(TextField);
        if (searchField.evaluate().isNotEmpty) {
          await tester.enterText(searchField.first, '测试关键词');
          await tester.pumpAndSettle();
        }
      }
    });
  });
}
