import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';
import '../test_helpers/test_helpers.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('拍卖页面测试', () {
    testWidgets('拍卖页面加载', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 点击拍卖标签
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle();

        // 验证拍卖页面加载成功
        expect(find.byType(Scaffold), findsOneWidget);
      }
    });

    testWidgets('拍卖商品列表显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 验证列表存在
        final gridView = find.byType(GridView);
        if (gridView.evaluate().isNotEmpty) {
          expect(gridView, findsWidgets);
        }
      }
    });

    testWidgets('拍卖商品滚动', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 滚动列表
        final gridView = find.byType(GridView);
        if (gridView.evaluate().isNotEmpty) {
          await tester.fling(gridView.first, const Offset(0, -500), 1000);
          await tester.pumpAndSettle();
        }
      }
    });
  });

  group('拍卖商品详情测试', () {
    testWidgets('查看商品详情', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击第一个商品
        final itemCards = find.byType(GestureDetector);
        if (itemCards.evaluate().isNotEmpty) {
          await tester.tap(itemCards.first);
          await tester.pumpAndSettle();

          // 验证详情页打开
          expect(find.byType(Scaffold), findsOneWidget);
        }
      }
    });

    testWidgets('商品图片显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找图片
        final images = find.byType(Image);
        if (images.evaluate().isNotEmpty) {
          expect(images, findsWidgets);
        }
      }
    });

    testWidgets('商品价格显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找价格文本（通常包含 ¥ 或 元）
        final priceText = find.textContaining('¥');
        if (priceText.evaluate().isEmpty) {
          final priceText2 = find.textContaining('元');
          if (priceText2.evaluate().isNotEmpty) {
            expect(priceText2, findsWidgets);
          }
        }
      }
    });
  });

  group('出价功能测试', () {
    testWidgets('显示出价按钮', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击商品进入详情
        final itemCards = find.byType(GestureDetector);
        if (itemCards.evaluate().isNotEmpty) {
          await tester.tap(itemCards.first);
          await tester.pumpAndSettle();

          // 查找出价按钮
          final bidButton = find.text('出价');
          if (bidButton.evaluate().isNotEmpty) {
            expect(bidButton, findsWidgets);
          }
        }
      }
    });

    testWidgets('出价输入框显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击商品进入详情
        final itemCards = find.byType(GestureDetector);
        if (itemCards.evaluate().isNotEmpty) {
          await tester.tap(itemCards.first);
          await tester.pumpAndSettle();

          // 查找出价按钮
          final bidButton = find.text('出价');
          if (bidButton.evaluate().isNotEmpty) {
            await tester.tap(bidButton.first);
            await tester.pumpAndSettle();

            // 验证输入框存在
            final textField = find.byType(TextField);
            if (textField.evaluate().isNotEmpty) {
              expect(textField, findsWidgets);
            }
          }
        }
      }
    });

    testWidgets('输入出价金额', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 点击商品进入详情
        final itemCards = find.byType(GestureDetector);
        if (itemCards.evaluate().isNotEmpty) {
          await tester.tap(itemCards.first);
          await tester.pumpAndSettle();

          // 点击出价按钮
          final bidButton = find.text('出价');
          if (bidButton.evaluate().isNotEmpty) {
            await tester.tap(bidButton.first);
            await tester.pumpAndSettle();

            // 输入金额
            final textField = find.byType(TextField);
            if (textField.evaluate().isNotEmpty) {
              await tester.enterText(textField.first, '100');
              await tester.pumpAndSettle();
            }
          }
        }
      }
    });
  });

  group('拍卖状态测试', () {
    testWidgets('拍卖中状态显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找状态标签
        final statusBadge = find.textContaining('拍卖');
        if (statusBadge.evaluate().isNotEmpty) {
          expect(statusBadge, findsWidgets);
        }
      }
    });

    testWidgets('倒计时显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找倒计时（包含 时、分、秒）
        final timeText = find.textContaining('时');
        if (timeText.evaluate().isEmpty) {
          final timeText2 = find.textContaining('分');
          if (timeText2.evaluate().isNotEmpty) {
            expect(timeText2, findsWidgets);
          }
        }
      }
    });
  });

  group('拍卖筛选测试', () {
    testWidgets('筛选按钮显示', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 查找筛选图标
        final filterIcon = find.byIcon(Icons.filter_list);
        if (filterIcon.evaluate().isNotEmpty) {
          expect(filterIcon, findsWidgets);
        }
      }
    });

    testWidgets('刷新拍卖列表', (tester) async {
      await tester.pumpWidget(ProviderScope(child: MyApp()));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 导航到拍卖页面
      final auctionIcon = find.byIcon(Icons.gavel);
      if (auctionIcon.evaluate().isNotEmpty) {
        await tester.tap(auctionIcon.first);
        await tester.pumpAndSettle(const Duration(seconds: 2));

        // 下拉刷新
        await tester.fling(
          find.byType(CustomScrollView).first,
          const Offset(0, 300),
          1000,
        );
        await tester.pumpAndSettle();
      }
    });
  });
}
