# Phase 3: Flutter 审核前修复与代码清理 - Context

**Gathered:** 2026-05-22
**Status:** Ready for planning
**Source:** Phase 2 discuss-phase decisions inherited

<domain>
## Phase Boundary

执行最小的代码修改确保 Flutter App 通过 App Store 和 Android 应用市场审核。所有功能已有代码，不做新开发，只做审核合规调整。

**关键原则：**
- 最小改动原则 — 只改必须改的部分，不随意重构
- iOS 和 Android 两端同步修改（同一 Flutter 代码库）
- 不影响正常用户功能业务流程
</domain>

<decisions>
## Implementation Decisions

### 提现入口隐藏
- Profile 页面退出登录按钮上方的提现功能入口需要隐藏
- 方法：在 Release build 中不渲染该入口，Debug 模式保留以便本地测试
- 对应 UI：个人中心页面（Profile/ProfilePage）
- 不应移除提现相关业务代码，仅隐藏 UI 入口

### iOS APK 版本更新弹窗禁用
- `HomePage._showUpdateDialog()` 在 iOS 平台不应触发
- 方法：在 iOS 平台（`Platform.isIOS`）跳过更新检查逻辑
- Android 平台保持现有功能不变
- 该弹窗在 App Store 渠道属于违规行为（应用内分发推广）

### 调试日志清理
- 移除所有 `debugPrint(...)` 调用
- 可用替换：使用 `kReleaseMode` 包装或删除非必要日志
- 重点文件：`login_page.dart`（10+处）, `profile_page.dart`（5+处）
- 保留有意义的生产日志（如 `LogUtil` 等正式日志框架的输出）

### 编译警告（Info 级别）
- 不强制修改 8835 条 info 级别 lint 提示（不影响审核）
- 仅修复少量误判为 warning/error 的 lint 问题

### the agent's Discretion
- 具体文件路径和隐藏方式（条件编译 vs 运行时状态判断）
- 日志清理的范围（全盘替换 vs 仅清理关键文件）
</decisions>

<canonical_refs>
## Canonical References

### 相关代码文件
- `molitao_app/lib/pages/study/profile/profile_page.dart` — Profile 页面（含提现入口）
- `molitao_app/lib/pages/study/home/home_page.dart` — HomePage 更新弹窗
- `molitao_app/lib/pages/study/sign/login_page.dart` — 登录页（debugPrint 集中）
- `molitao_app/lib/pages/study/mine/profile_page.dart` — 个人中心（debugPrint 集中）

### 架构决策
- `.planning/ROADMAP.md` — Phase 3 Scope
- `.planning/codebase/CONVENTIONS.md` — 代码规范
</canonical_refs>

<specifics>
## 具体方案

### 隐藏提现入口方案
在 `profile_page.dart` 提现入口 widget 外包裹条件判断：
```dart
if (!kReleaseMode) {
  // 提现入口 widget
}
```
或使用 `Platform.isIOS` 条件。需要在文件头部 import `'dart:io'` 用于平台判断。

### 禁用 iOS 更新弹窗方案
在 `home_page.dart` 的 `_showUpdateDialog()` 调用处添加：
```dart
if (Platform.isIOS) return; // iOS App Store 渠道禁用
```

### 日志清理方案
使用 ast-grep 或 grep 定位所有 `debugPrint(` 调用，逐文件审查后移除或替换为条件编译。
</specifics>

<deferred>
## Deferred Ideas
- 全局 `debugPrint` 替换为日志框架统一处理 — 超出最小改动原则
- 提现业务代码移除 — 仅隐藏入口，保留后台功能
- Android 更新弹窗行为不变
</deferred>

---

*Phase: 03-flutter*
*Context gathered: 2026-05-22 via Phase 2 decision inheritance*
