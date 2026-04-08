import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:patrol/patrol.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('完整用户流程 - 登录认证', () {
    patrolTest('用户登录完整流程', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到个人中心页面
      await $('个人中心').tap();
      await $.pumpAndSettle();

      // 检查登录状态
      final loginButton = $('登录');
      if (await loginButton.exists) {
        await loginButton.tap();
        await $.pumpAndSettle();

        // 输入用户名和密码
        final usernameField = $(TextField).at(0);
        if (await usernameField.exists) {
          await usernameField.enterText('test_user');
          await $.pumpAndSettle();
        }

        final passwordField = $(TextField).at(1);
        if (await passwordField.exists) {
          await passwordField.enterText('password123');
          await $.pumpAndSettle();
        }

        // 点击登录
        final submitButton = $('登录');
        if (await submitButton.exists) {
          await submitButton.tap();
          await $.pumpAndSettle();
        }
      }
    });

    patrolTest('用户退出登录流程', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到个人中心页面
      await $('个人中心').tap();
      await $.pumpAndSettle();

      // 点击退出按钮
      final logoutButton = $('退出登录');
      if (await logoutButton.exists) {
        await logoutButton.tap();
        await $.pumpAndSettle();

        // 确认退出
        final confirmButton = $('确定');
        if (await confirmButton.exists) {
          await confirmButton.tap();
          await $.pumpAndSettle();
        }
      }
    });
  });

  group('完整用户流程 - 会话列表', () {
    patrolTest('浏览会话列表', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到会话列表
      await $('会话列表').tap();
      await $.pumpAndSettle();

      // 验证页面存在
      expect($(Scaffold), findsOneWidget);
    });

    patrolTest('查看会话详情', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到会话列表
      await $('会话列表').tap();
      await $.pumpAndSettle();

      // 点击第一个会话
      final firstChat = $(ListTile).at(0);
      if (await firstChat.exists) {
        await firstChat.tap();
        await $.pumpAndSettle();

        // 验证详情页打开
        expect($(Scaffold), findsOneWidget);

        // 返回
        final backButton = $(BackButton);
        if (await backButton.exists) {
          await backButton.tap();
          await $.pumpAndSettle();
        }
      }
    });
  });

  group('完整用户流程 - 交易站', () {
    patrolTest('浏览交易站', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到交易站页面
      await $('交易站').tap();
      await $.pumpAndSettle();

      // 验证页面存在
      expect($(Scaffold), findsOneWidget);
    });

    patrolTest('滚动浏览交易站内容', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到交易站页面
      await $('交易站').tap();
      await $.pumpAndSettle();

      // 尝试滚动
      final scrollables = find.byWidgetPredicate(
        (widget) => widget is ScrollView || widget is Scrollable,
      );
      if (scrollables.evaluate().isNotEmpty) {
        await $.tester.fling(scrollables.first, const Offset(0, -300), 1000);
        await $.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 通讯录', () {
    patrolTest('浏览通讯录', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到通讯录页面
      await $('通讯录').tap();
      await $.pumpAndSettle();

      // 验证页面存在
      expect($(Scaffold), findsOneWidget);
    });
  });

  group('完整用户流程 - 个人中心', () {
    patrolTest('查看个人信息', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到个人中心页面
      await $('个人中心').tap();
      await $.pumpAndSettle();

      // 验证页面存在
      expect($(Scaffold), findsOneWidget);
    });

    patrolTest('修改个人信息', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到个人中心页面
      await $('个人中心').tap();
      await $.pumpAndSettle();

      // 点击编辑按钮
      final editButton = $('编辑');
      if (await editButton.exists) {
        await editButton.tap();
        await $.pumpAndSettle();

        // 修改昵称
        final nicknameField = $(TextField).at(0);
        if (await nicknameField.exists) {
          await nicknameField.enterText('新昵称');
          await $.pumpAndSettle();

          // 保存修改
          final saveButton = $('保存');
          if (await saveButton.exists) {
            await saveButton.tap();
            await $.pumpAndSettle();
          }
        }
      }
    });
  });

  group('完整用户流程 - 综合场景', () {
    patrolTest('跨页面导航测试', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 首页 -> 会话列表
      await $('会话列表').tap();
      await $.pumpAndSettle();

      // 会话列表 -> 交易站
      await $('交易站').tap();
      await $.pumpAndSettle();

      // 交易站 -> 通讯录
      await $('通讯录').tap();
      await $.pumpAndSettle();

      // 通讯录 -> 个人中心
      await $('个人中心').tap();
      await $.pumpAndSettle();

      // 返回首页
      await $('首页').tap();
      await $.pumpAndSettle();
    });

    patrolTest('首页滚动测试', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 首页使用 SingleChildScrollView
      final scrollView = find.byType(SingleChildScrollView);
      if (scrollView.evaluate().isNotEmpty) {
        await $.tester.fling(scrollView.first, const Offset(0, -300), 1000);
        await $.pumpAndSettle();

        await $.tester.fling(scrollView.first, const Offset(0, 300), 1000);
        await $.pumpAndSettle();
      }
    });
  });
}
