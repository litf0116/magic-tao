---
wave: 1
id: FIX-PLAN-2
title: 代码清理（debugPrint + 编译警告）
description: 清理调试日志残留，修复编译警告
autonomous: true
files_modified:
  - molitao_app/lib/pages/study/sign/login_page.dart
  - molitao_app/lib/pages/study/mine/profile_page.dart
  - molitao_app/lib/pages/study/profile/profile_page.dart
  - molitao_app/lib/pages/study/home/home_page.dart
---

# FIX-PLAN-2: 代码清理

## 目标

1. 移除 `debugPrint` 调试日志残留
2. 修复可能影响审核的编译警告

## 上下文

Phase 1 审计发现大量 `debugPrint` 调用集中在以下文件：
- `login_page.dart` — 10+ 处
- `profile_page.dart`（mine） — 5+ 处
- `profile_page.dart`（profile） — 若干处
- `home_page.dart` — 若干处

审核阶段不应包含调试输出。`debugPrint` 在 Release 模式下自动不输出，但代码中留有调试语句属于不良实践，可能有信息泄漏风险。

## 任务

### Task 1: 清理 debugPrint 调用

**read_first:**
- `molitao_app/lib/pages/study/sign/login_page.dart`
- `molitao_app/lib/pages/study/mine/profile_page.dart`

**action:**
搜索所有 `debugPrint(` 调用，按以下规则处理：
1. 纯调试信息（"XX 页面打开"、"XX 按钮点击"、"数据: XX"）→ 直接删除
2. 有业务价值的日志（关键变量值、错误上下文）→ 保留但包装在 `if (!kReleaseMode)` 中

**不要**修改正式日志框架的输出（如 `LogUtil`、`logger` 等）。

**acceptance_criteria:**
- 重点文件中 `debugPrint(` 调用从计数中清除或降至合理数量
- `flutter analyze` 通过（0 error, 0 warning）
- 无业务逻辑被破坏
- 保留了对调试有价值的日志（包装在 `kReleaseMode` 检查中）

### Task 2: 编译警告清理

**read_first:**
- 运行 `flutter analyze` 查看当前 warning 级别以上的问题

**action:**
检查 `flutter analyze` 输出中是否有 error 或 warning 级别的诊断结果。Phase 2 审计确认当前为 0 error / 0 warning，此 Task 为验证性——确保修改后没有引入新的警告。

如果有 info 级别的提示涉及审核风险（如 `use_key_in_widget_constructors` 等导致 Widget 无法正确构建的问题），按需修复。

**acceptance_criteria:**
- `flutter analyze` 输出 0 error, 0 warning

## 验证

- `flutter analyze` 通过（0 error, 0 warning） ✅
- `flutter build ios --no-codesign --release` 构建通过 ✅
- `flutter build apk --release` 构建通过 ✅
