import 'package:device_preview/device_preview.dart';
import 'package:flutter/material.dart';

class DevicePreviewConfig {
  static List<DeviceInfo> get devices => [
        // iPhone SE (小屏)
        DeviceInfo(
          identifier: 'iphone_se',
          name: 'iPhone SE',
          size: const Size(320, 568),
          pixelRatio: 2.0,
          safeAreas: const EdgeInsets.only(top: 20, bottom: 0),
          platform: TargetPlatform.iOS,
        ),
        
        // iPhone 11 (中屏)
        DeviceInfo(
          identifier: 'iphone_11',
          name: 'iPhone 11',
          size: const Size(414, 896),
          pixelRatio: 2.0,
          safeAreas: const EdgeInsets.only(top: 44, bottom: 34),
          platform: TargetPlatform.iOS,
        ),
        
        // Pixel 4a (Android小屏)
        DeviceInfo(
          identifier: 'pixel_4a',
          name: 'Pixel 4a',
          size: const Size(392.72, 850.9),
          pixelRatio: 2.75,
          safeAreas: const EdgeInsets.only(top: 24, bottom: 0),
          platform: TargetPlatform.android,
        ),
        
        // Pixel 6 (Android中屏)
        DeviceInfo(
          identifier: 'pixel_6',
          name: 'Pixel 6',
          size: const Size(412, 915),
          pixelRatio: 3.0,
          safeAreas: const EdgeInsets.only(top: 24, bottom: 0),
          platform: TargetPlatform.android,
        ),
        
        // Pixel 7 Pro (Android大屏)
        DeviceInfo(
          identifier: 'pixel_7_pro',
          name: 'Pixel 7 Pro',
          size: const Size(412, 915),
          pixelRatio: 3.5,
          safeAreas: const EdgeInsets.only(top: 24, bottom: 0),
          platform: TargetPlatform.android,
        ),
        
        // 平板设备
        DeviceInfo(
          identifier: 'tablet',
          name: 'Tablet',
          size: const Size(1024, 768),
          pixelRatio: 2.0,
          safeAreas: EdgeInsets.zero,
          platform: TargetPlatform.android,
        ),
        
        // 折叠屏设备
        DeviceInfo(
          identifier: 'foldable',
          name: 'Foldable',
          size: const Size(600, 800),
          pixelRatio: 3.0,
          safeAreas: EdgeInsets.zero,
          platform: TargetPlatform.android,
        ),
      ];

  static List<CustomDeviceInfo> get customDevices => [
        CustomDeviceInfo(
          identifier: 'small_android',
          name: 'Small Android',
          size: const Size(360, 640),
          pixelRatio: 2.0,
          safeAreas: const EdgeInsets.only(top: 24, bottom: 0),
          platform: TargetPlatform.android,
        ),
        
        CustomDeviceInfo(
          identifier: 'large_android',
          name: 'Large Android',
          size: const Size(412, 915),
          pixelRatio: 3.5,
          safeAreas: const EdgeInsets.only(top: 24, bottom: 0),
          platform: TargetPlatform.android,
        ),
      ];
}

class CustomDeviceInfo extends DeviceInfo {
  const CustomDeviceInfo({
    required super.identifier,
    required super.name,
    required super.size,
    required super.pixelRatio,
    required super.safeAreas,
    required super.platform,
  });
}