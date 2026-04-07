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

      // 导航到我的页面
      await $('我的').tap();
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

      // 导航到我的页面
      await $('我的').tap();
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

  group('完整用户流程 - 论坛功能', () {
    patrolTest('浏览帖子列表', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到论坛
      await $('论坛').tap();
      await $.pumpAndSettle();

      // 滚动浏览帖子
      await $.tester.fling(find.byType(ListView).first, Offset(0, -500), 1000);
      await $.pumpAndSettle();

      // 验证列表存在
      expect($(ListView), findsWidgets);
    });

    patrolTest('查看帖子详情', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到论坛
      await $('论坛').tap();
      await $.pumpAndSettle();

      // 点击第一个帖子
      final firstPost = $(Card).at(0);
      if (await firstPost.exists) {
        await firstPost.tap();
        await $.pumpAndSettle();

        // 验证详情页打开
        expect($(Scaffold), findsOneWidget);

        // 返回
        await $(BackButton).tap();
        await $.pumpAndSettle();
      }
    });

    patrolTest('发布新帖子', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到论坛
      await $('论坛').tap();
      await $.pumpAndSettle();

      // 点击发帖按钮
      final addButton = $(FloatingActionButton);
      if (await addButton.exists) {
        await addButton.tap();
        await $.pumpAndSettle();

        // 填写标题
        final titleField = $(TextField).at(0);
        if (await titleField.exists) {
          await titleField.enterText('测试帖子标题');
          await $.pumpAndSettle();
        }

        // 填写内容
        final contentField = $(TextField).at(1);
        if (await contentField.exists) {
          await contentField.enterText('测试帖子内容');
          await $.pumpAndSettle();
        }

        // 提交发布
        final submitButton = $('发布');
        if (await submitButton.exists) {
          await submitButton.tap();
          await $.pumpAndSettle();
        }
      }
    });

    patrolTest('搜索帖子', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到论坛
      await $('论坛').tap();
      await $.pumpAndSettle();

      // 点击搜索图标
      final searchIcon = $(Icon).containing(Icons.search);
      if (await searchIcon.exists) {
        await searchIcon.tap();
        await $.pumpAndSettle();

        // 输入搜索关键词
        final searchField = $(TextField);
        if (await searchField.exists) {
          await searchField.enterText('魔力宝贝');
          await $.pumpAndSettle();
        }
      }
    });
  });

  group('完整用户流程 - 拍卖功能', () {
    patrolTest('浏览拍卖商品', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到拍卖页面
      await $('拍卖').tap();
      await $.pumpAndSettle();

      // 滚动浏览商品
      await $.tester.fling(find.byType(GridView).first, Offset(0, -500), 1000);
      await $.pumpAndSettle();

      // 验证列表存在
      expect($(GridView), findsWidgets);
    });

    patrolTest('查看商品详情', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到拍卖页面
      await $('拍卖').tap();
      await $.pumpAndSettle();

      // 点击第一个商品
      final firstItem = $(GestureDetector).at(0);
      if (await firstItem.exists) {
        await firstItem.tap();
        await $.pumpAndSettle();

        // 验证详情页打开
        expect($(Scaffold), findsOneWidget);

        // 返回
        await $(BackButton).tap();
        await $.pumpAndSettle();
      }
    });

    patrolTest('参与出价', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到拍卖页面
      await $('拍卖').tap();
      await $.pumpAndSettle();

      // 点击第一个商品
      final firstItem = $(GestureDetector).at(0);
      if (await firstItem.exists) {
        await firstItem.tap();
        await $.pumpAndSettle();

        // 点击出价按钮
        final bidButton = $('出价');
        if (await bidButton.exists) {
          await bidButton.tap();
          await $.pumpAndSettle();

          // 输入出价金额
          final priceField = $(TextField);
          if (await priceField.exists) {
            await priceField.enterText('100');
            await $.pumpAndSettle();

            // 确认出价
            final confirmButton = $('确认');
            if (await confirmButton.exists) {
              await confirmButton.tap();
              await $.pumpAndSettle();
            }
          }
        }
      }
    });
  });

  group('完整用户流程 - 消息聊天', () {
    patrolTest('查看聊天列表', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到消息页面
      await $('消息').tap();
      await $.pumpAndSettle();

      // 验证列表存在
      expect($(ListView), findsWidgets);
    });

    patrolTest('发送消息', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到消息页面
      await $('消息').tap();
      await $.pumpAndSettle();

      // 点击第一个聊天
      final firstChat = $(ListTile).at(0);
      if (await firstChat.exists) {
        await firstChat.tap();
        await $.pumpAndSettle();

        // 输入消息
        final messageField = $(TextField);
        if (await messageField.exists) {
          await messageField.enterText('测试消息');
          await $.pumpAndSettle();

          // 发送消息
          final sendButton = $(Icon).containing(Icons.send);
          if (await sendButton.exists) {
            await sendButton.tap();
            await $.pumpAndSettle();
          }
        }
      }
    });
  });

  group('完整用户流程 - 个人中心', () {
    patrolTest('查看个人信息', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到我的页面
      await $('我的').tap();
      await $.pumpAndSettle();

      // 验证用户头像存在
      expect($(CircleAvatar), findsWidgets);
    });

    patrolTest('修改个人信息', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到我的页面
      await $('我的').tap();
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

    patrolTest('查看交易记录', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 导航到我的页面
      await $('我的').tap();
      await $.pumpAndSettle();

      // 点击交易记录
      final tradeButton = $('交易记录');
      if (await tradeButton.exists) {
        await tradeButton.tap();
        await $.pumpAndSettle();

        // 验证列表存在
        expect($(ListView), findsWidgets);

        // 返回
        await $(BackButton).tap();
        await $.pumpAndSettle();
      }
    });
  });

  group('完整用户流程 - 综合场景', () {
    patrolTest('完整购物流程', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 1. 浏览首页
      await $.pumpAndSettle();

      // 2. 导航到拍卖页面
      await $('拍卖').tap();
      await $.pumpAndSettle();

      // 3. 选择商品
      final firstItem = $(GestureDetector).at(0);
      if (await firstItem.exists) {
        await firstItem.tap();
        await $.pumpAndSettle();

        // 4. 查看详情
        await $.pumpAndSettle();

        // 5. 返回首页
        await $(BackButton).tap();
        await $.pumpAndSettle();
      }
    });

    patrolTest('跨页面导航测试', ($) async {
      await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

      // 首页 -> 论坛
      await $('论坛').tap();
      await $.pumpAndSettle();

      // 论坛 -> 拍卖
      await $('拍卖').tap();
      await $.pumpAndSettle();

      // 拍卖 -> 消息
      await $('消息').tap();
      await $.pumpAndSettle();

      // 消息 -> 我的
      await $('我的').tap();
      await $.pumpAndSettle();

      // 返回首页
      await $('首页').tap();
      await $.pumpAndSettle();
    });
  });
}
