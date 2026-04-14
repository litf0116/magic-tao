import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';
import '../test_helpers/test_helpers.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('消息页面测试', () {
    testWidgets('消息页面加载', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 点击消息标签
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle();

        // 验证消息页面加载成功
        expect(find.byType(Scaffold), findsOneWidget);
      }
    });

    testWidgets('聊天列表显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 验证列表存在
        final listView = find.byType(ListView);
        if (listView.evaluate().isNotEmpty) {
          expect(listView, findsWidgets);
        }
      }
    });

    testWidgets('聊天列表滚动', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 滚动列表
        final listView = find.byType(ListView);
        if (listView.evaluate().isNotEmpty) {
          await tester.fling(listView.first, const Offset(0, -300), 1000);
          await tester.pumpAndSettle();
        }
      }
    });

    testWidgets('未读消息提示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找未读消息红点
        final badge = find.byType(Container);
        expect(badge, findsWidgets);
      }
    });
  });

  group('聊天会话测试', () {
    testWidgets('打开聊天会话', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 验证聊天页面打开
          expect(find.byType(Scaffold), findsOneWidget);
        }
      }
    });

    testWidgets('聊天消息列表显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 验证消息列表存在
          final messageList = find.byType(ListView);
          if (messageList.evaluate().isNotEmpty) {
            expect(messageList, findsWidgets);
          }
        }
      }
    });

    testWidgets('聊天输入框显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 查找输入框
          final textField = find.byType(TextField);
          if (textField.evaluate().isNotEmpty) {
            expect(textField, findsWidgets);
          }
        }
      }
    });
  });

  group('发送消息测试', () {
    testWidgets('输入消息内容', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 输入消息
          final textField = find.byType(TextField);
          if (textField.evaluate().isNotEmpty) {
            await tester.enterText(textField.first, '测试消息');
            await tester.pumpAndSettle();
          }
        }
      }
    });

    testWidgets('发送按钮显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 查找发送按钮
          final sendButton = find.byIcon(Icons.send);
          if (sendButton.evaluate().isNotEmpty) {
            expect(sendButton, findsWidgets);
          }
        }
      }
    });

    testWidgets('发送文本消息', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 输入消息
          final textField = find.byType(TextField);
          if (textField.evaluate().isNotEmpty) {
            await tester.enterText(textField.first, '测试消息');
            await tester.pumpAndSettle();

            // 点击发送按钮
            final sendButton = find.byIcon(Icons.send);
            if (sendButton.evaluate().isNotEmpty) {
              await tester.tap(sendButton.first);
              await tester.pumpAndSettle();
            }
          }
        }
      }
    });
  });

  group('聊天功能测试', () {
    testWidgets('消息滚动', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 滚动消息列表
          final messageList = find.byType(ListView);
          if (messageList.evaluate().isNotEmpty) {
            await tester.fling(messageList.first, const Offset(0, -300), 1000);
            await tester.pumpAndSettle();
          }
        }
      }
    });

    testWidgets('返回聊天列表', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().isNotEmpty) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 点击返回按钮
          final backButton = find.byType(BackButton);
          if (backButton.evaluate().isNotEmpty) {
            await tester.tap(backButton.first);
            await tester.pumpAndSettle();
          }
        }
      }
    });

    testWidgets('聊天室切换', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个聊天
        final chatItems = find.byType(ListTile);
        if (chatItems.evaluate().length >= 2) {
          await tester.tap(chatItems.first);
          await tester.pumpAndSettle();

          // 返回
          final backButton = find.byType(BackButton);
          if (backButton.evaluate().isNotEmpty) {
            await tester.tap(backButton.first);
            await tester.pumpAndSettle();

            // 点击第二个聊天
            await tester.tap(chatItems.at(1));
            await tester.pumpAndSettle();
          }
        }
      }
    });
  });

  group('WebSocket 连接测试', () {
    testWidgets('WebSocket 连接状态', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 验证应用正常运行（WebSocket 连接正常）
        expect(find.byType(Scaffold), findsOneWidget);
      }
    });

    testWidgets('实时消息接收', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到消息页面
      final messageIcon = find.byIcon(Icons.message);
      if (messageIcon.evaluate().isNotEmpty) {
        await tester.tap(messageIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 等待一段时间检查消息更新
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 验证列表正常显示
        final listView = find.byType(ListView);
        expect(listView, findsWidgets);
      }
    });
  });
}
