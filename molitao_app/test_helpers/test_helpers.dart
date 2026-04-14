import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

class TestHelpers {
  static Future<void> waitForWidget(
    WidgetTester tester,
    Finder finder, {
    Duration timeout = const Duration(seconds: 10),
  }) async {
    final endTime = DateTime.now().add(timeout);

    while (DateTime.now().isBefore(endTime)) {
      if (finder.evaluate().isNotEmpty) {
        return;
      }
      await tester.pump(const Duration(milliseconds: 100));
    }

    throw TimeoutException('Widget not found: $finder');
  }

  static Future<void> tapAndWait(
    WidgetTester tester,
    Finder finder,
    Finder targetWidget, {
    Duration timeout = const Duration(seconds: 10),
  }) async {
    await tester.tap(finder);
    await tester.pumpAndSettle();
    await waitForWidget(tester, targetWidget, timeout: timeout);
  }

  static Future<void> enterTextAndSubmit(
    WidgetTester tester,
    Finder textField,
    String text,
  ) async {
    await tester.enterText(textField, text);
    await tester.testTextInput.receiveAction(TextInputAction.done);
    await tester.pumpAndSettle();
  }

  static Future<void> scrollUntilVisible(
    WidgetTester tester,
    Finder finder, {
    double delta = 100,
    int maxScrolls = 10,
  }) async {
    int scrollCount = 0;
    while (!finder.hitTestable().evaluate().isNotEmpty &&
        scrollCount < maxScrolls) {
      await tester.drag(find.byType(ListView), Offset(0, -delta));
      await tester.pumpAndSettle();
      scrollCount++;
    }
  }

  static Future<void> takeScreenshot(WidgetTester tester, String name) async {
    await tester.pumpAndSettle();
  }

  static bool isWidgetPresent(Finder finder) {
    return finder.evaluate().isNotEmpty;
  }

  static int getWidgetCount(Finder finder) {
    return finder.evaluate().length;
  }
}
