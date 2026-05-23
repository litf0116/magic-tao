---
wave: 1
id: FIX-PLAN-1
title: 审核合规修改（隐藏提现入口 + 禁用 iOS 更新弹窗）
description: 根据 Phase 2 决策，对 Flutter App 执行最小化的审核合规代码修改
autonomous: true
files_modified:
  - molitao_app/lib/pages/study/profile/profile_page.dart
  - molitao_app/lib/pages/study/home/home_page.dart
---

# FIX-PLAN-1: 审核合规修改

## 目标

1. 隐藏 Profile 页面的提现入口（Release 模式不渲染）
2. 禁用 iOS 端 APK 版本更新弹窗（iOS 平台跳过检查）

## 任务

### Task 1: 隐藏 Profile 页面提现入口

**read_first:**
- `molitao_app/lib/pages/study/profile/profile_page.dart`
- `molitao_app/lib/pages/study/mine/profile_page.dart`

**action:**
在 Profile 页面中找到"提现"相关的 UI widget（退出登录按钮上方的入口），将其包裹在条件判断中——Release 模式下不渲染。

使用 `kReleaseMode` 来自 `foundation.dart`（无需额外 import）。

```dart
if (!kReleaseMode) {
  // 提现入口 widget...
}
```

Debug 模式保留该入口以便本地测试。不删除提现相关的业务代码。

**acceptance_criteria:**
- `profile_page.dart` 中提现入口 widget 被 `if (!kReleaseMode)` 包裹
- `flutter analyze` 通过（0 error, 0 warning）
- Debug 模式运行能看到提现入口
- Release 模式截图确认提现入口不可见
- 提现业务功能代码完整保留，未受影响

### Task 2: 禁用 iOS 端 APK 版本更新弹窗

**read_first:**
- `molitao_app/lib/pages/study/home/home_page.dart`

**action:**
在 `HomePage` 的更新检查逻辑（`_showUpdateDialog` 或类似方法）中，添加 iOS 平台跳过判断。使用 `Platform.isIOS` 来自 `dart:io`。

```dart
// 在更新检查的开头添加
if (Platform.isIOS) return;
```

Android 平台保持现有更新弹窗功能不变。更新弹窗功能是 Android 端 APK 下载更新所用，iOS App Store 渠道不应出现。

如果 HomePage 文件尚未 import `dart:io`，需要添加。

**acceptance_criteria:**
- `home_page.dart` 更新检查逻辑添加了 `Platform.isIOS` 跳过条件
- `flutter analyze` 通过（0 error, 0 warning）
- Android 端更新弹窗功能正常（不受影响）
- iOS 端不会触发 `_showUpdateDialog`

## 验证

- `flutter analyze` 通过 ✅
- `flutter build ios --no-codesign --release` 构建通过 ✅
- `flutter build apk --release` 构建通过 ✅
