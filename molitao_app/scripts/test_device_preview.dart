// 测试脚本：验证Device Preview功能
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:device_preview/device_preview.dart';
import 'package:molitao_app/app.dart';

void main() {
  runApp(
    DevicePreview(
      enabled: true,
      builder: (context) => ProviderScope(child: MyApp()),
    ),
  );
}