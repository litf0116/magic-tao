# Flutter 自动化测试指南

## 📁 项目结构

```
molitao_app/
├── integration_test/              # 集成测试目录
│   ├── app_integration_test.dart  # Flutter Integration Test 基础 UI 测试
│   ├── app_e2e_test.dart          # Patrol E2E 基础测试
│   ├── auth_test.dart             # 登录认证测试（8个测试用例）
│   ├── forum_test.dart            # 论坛功能测试（15个测试用例）
│   ├── auction_test.dart          # 拍卖功能测试（16个测试用例）
│   ├── chat_test.dart             # 消息聊天测试（16个测试用例）
│   └── comprehensive_e2e_test.dart # 完整 E2E 综合测试（15个测试用例）
├── test_helpers/                  # 测试辅助工具
│   └── test_helpers.dart          # 测试工具类
├── scripts/                       # 测试脚本
│   └── run_tests.sh               # 测试运行脚本
└── pubspec.yaml                   # 项目依赖配置
```

## 🎯 测试框架说明

### 1. **Flutter Integration Test** - 基础 UI 测试

Flutter 官方提供的集成测试框架，适合：
- ✅ 基础 UI 组件测试
- ✅ 页面导航测试
- ✅ 简单用户交互测试
- ✅ 性能基准测试

### 2. **Patrol** - 完整 E2E 测试

新一代 UI 自动化测试工具，适合：
- ✅ 复杂用户流程测试
- ✅ 原生功能测试（权限、通知等）
- ✅ 更快的测试执行速度
- ✅ 更好的调试体验

## 📊 测试用例统计

### Integration Test 测试用例

| 测试文件 | 测试组 | 测试用例数 |
|---------|-------|-----------|
| app_integration_test.dart | 4 | 7 |
| auth_test.dart | 4 | 8 |
| forum_test.dart | 4 | 15 |
| auction_test.dart | 6 | 16 |
| chat_test.dart | 6 | 16 |
| **总计** | **24** | **62** |

### Patrol E2E 测试用例

| 测试文件 | 测试组 | 测试用例数 |
|---------|-------|-----------|
| app_e2e_test.dart | 7 | 7 |
| comprehensive_e2e_test.dart | 6 | 15 |
| **总计** | **13** | **22** |

## 🚀 快速开始

### 前置条件

1. **连接设备**
```bash
# 检查已连接设备
flutter devices

# 应该看到类似输出：
# 22101317C (mobile) • 827af65d0722 • android-arm64 • Android 14 (API 34)
```

2. **安装依赖**
```bash
flutter pub get
```

### 使用测试脚本运行（推荐）

```bash
# 运行所有测试
./scripts/run_tests.sh -d 827af65d0722 -t all

# 运行指定类型的测试
./scripts/run_tests.sh -d 827af65d0722 -t integration  # Integration Test
./scripts/run_tests.sh -d 827af65d0722 -t e2e          # Patrol E2E 测试
./scripts/run_tests.sh -d 827af65d0722 -t auth         # 登录认证测试
./scripts/run_tests.sh -d 827af65d0722 -t forum        # 论坛功能测试
./scripts/run_tests.sh -d 827af65d0722 -t auction      # 拍卖功能测试
./scripts/run_tests.sh -d 827af65d0722 -t chat         # 消息聊天测试

# 查看帮助信息
./scripts/run_tests.sh --help
```

### 手动运行测试

#### Flutter Integration Test

```bash
# 运行所有集成测试
flutter test integration_test/app_integration_test.dart -d <device_id>

# 运行特定测试文件
flutter test integration_test/auth_test.dart -d <device_id>
flutter test integration_test/forum_test.dart -d <device_id>
flutter test integration_test/auction_test.dart -d <device_id>
flutter test integration_test/chat_test.dart -d <device_id>

# 运行特定测试组
flutter test integration_test/auth_test.dart --name "登录流程测试" -d <device_id>

# 示例：在真机上运行
flutter test integration_test/auth_test.dart -d 827af65d0722
```

#### Patrol E2E 测试

```bash
# 运行所有 E2E 测试
patrol test -d <device_id>

# 运行特定测试文件
patrol test integration_test/app_e2e_test.dart -d <device_id>
patrol test integration_test/comprehensive_e2e_test.dart -d <device_id>

# 示例：在真机上运行
patrol test -d 827af65d0722
```

## 📝 编写测试用例

### 测试用例分类

#### 1. 登录认证测试（auth_test.dart）

**测试覆盖：**
- ✅ 应用启动时检查登录状态
- ✅ 已登录用户显示用户信息
- ✅ 未登录用户显示登录按钮
- ✅ 显示登录页面
- ✅ 登录表单输入测试
- ✅ 登录按钮响应测试
- ✅ Token 存储测试
- ✅ 自动登录功能测试
- ✅ 退出登录功能测试
- ✅ 网络错误处理
- ✅ 表单验证测试

#### 2. 论坛功能测试（forum_test.dart）

**测试覆盖：**
- ✅ 论坛页面加载
- ✅ 帖子列表显示
- ✅ 帖子分类筛选
- ✅ 帖子滚动加载
- ✅ 显示发帖按钮
- ✅ 打开发帖页面
- ✅ 发帖表单验证
- ✅ 选择帖子分类
- ✅ 查看帖子详情
- ✅ 帖子详情内容显示
- ✅ 帖子详情滚动
- ✅ 搜索框显示
- ✅ 输入搜索关键词

#### 3. 拍卖功能测试（auction_test.dart）

**测试覆盖：**
- ✅ 拍卖页面加载
- ✅ 拍卖商品列表显示
- ✅ 拍卖商品滚动
- ✅ 查看商品详情
- ✅ 商品图片显示
- ✅ 商品价格显示
- ✅ 显示出价按钮
- ✅ 出价输入框显示
- ✅ 输入出价金额
- ✅ 拍卖中状态显示
- ✅ 倒计时显示
- ✅ 筛选按钮显示
- ✅ 刷新拍卖列表

#### 4. 消息聊天测试（chat_test.dart）

**测试覆盖：**
- ✅ 消息页面加载
- ✅ 聊天列表显示
- ✅ 聊天列表滚动
- ✅ 未读消息提示
- ✅ 打开聊天会话
- ✅ 聊天消息列表显示
- ✅ 聊天输入框显示
- ✅ 输入消息内容
- ✅ 发送按钮显示
- ✅ 发送文本消息
- ✅ 消息滚动
- ✅ 返回聊天列表
- ✅ 聊天室切换
- ✅ WebSocket 连接状态
- ✅ 实时消息接收

#### 5. 完整 E2E 测试（comprehensive_e2e_test.dart）

**测试覆盖：**
- ✅ 用户登录完整流程
- ✅ 用户退出登录流程
- ✅ 浏览帖子列表
- ✅ 查看帖子详情
- ✅ 发布新帖子
- ✅ 搜索帖子
- ✅ 浏览拍卖商品
- ✅ 查看商品详情
- ✅ 参与出价
- ✅ 查看聊天列表
- ✅ 发送消息
- ✅ 查看个人信息
- ✅ 修改个人信息
- ✅ 查看交易记录
- ✅ 完整购物流程
- ✅ 跨页面导航测试

### Flutter Integration Test 示例

```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:molitao_app/main.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('测试首页加载', (tester) async {
    // 启动应用
    await tester.pumpWidget(ProviderScope(child: MyApp()));
    await tester.pumpAndSettle();

    // 验证元素存在
    expect(find.text('首页'), findsOneWidget);
  });
}
```

### Patrol E2E 测试示例

```dart
import 'package:patrol/patrol.dart';

void main() {
  patrolTest('完整登录流程', ($) async {
    await $.pumpWidgetAndSettle(ProviderScope(child: MyApp()));

    // 点击我的页面
    await $('我的').tap();
    await $.pumpAndSettle();

    // 输入用户名
    await $.enterText(find.byKey(Key('username')), 'test_user');

    // 点击登录按钮
    await $('登录').tap();
    await $.pumpAndSettle();
  });
}
```

## 🔍 测试辅助工具

项目提供了 `TestHelpers` 类，包含常用测试方法：

```dart
import '../test_helpers/test_helpers.dart';

// 等待控件出现
await TestHelpers.waitForWidget(tester, find.text('欢迎'));

// 点击并等待目标控件
await TestHelpers.tapAndWait(
  tester,
  find.text('登录'),
  find.text('欢迎'),
);

// 输入文本并提交
await TestHelpers.enterTextAndSubmit(
  tester,
  find.byKey(Key('username')),
  'test_user',
);

// 滚动直到可见
await TestHelpers.scrollUntilVisible(tester, find.text('详情'));
```

## 📊 测试报告

### 查看测试结果

```bash
# Flutter Integration Test 会输出详细测试报告
flutter test integration_test/app_integration_test.dart -d <device_id>

# Patrol 测试会生成更友好的输出
patrol test -d <device_id>
```

### 性能测试

```dart
testWidgets('应用启动性能测试', (tester) async {
  final stopwatch = Stopwatch()..start();

  await tester.pumpWidget(ProviderScope(child: MyApp()));
  await tester.pumpAndSettle();

  stopwatch.stop();

  // 验证启动时间小于 5 秒
  expect(stopwatch.elapsedMilliseconds, lessThan(5000));
});
```

## 🎯 测试最佳实践

### 1. 测试隔离

每个测试用例应该独立运行，不依赖其他测试：

```dart
testWidgets('独立的测试用例', (tester) async {
  // 每个测试都从干净状态开始
  await tester.pumpWidget(ProviderScope(child: MyApp()));
  await tester.pumpAndSettle();
});
```

### 2. 使用 Key 定位控件

为重要控件添加 Key，便于测试定位：

```dart
// 在代码中
TextField(
  key: Key('username_field'),
  ...
)

// 在测试中
await tester.enterText(find.byKey(Key('username_field')), 'test');
```

### 3. 等待异步操作

使用 `pumpAndSettle()` 等待动画和异步操作完成：

```dart
await tester.tap(find.text('加载'));
await tester.pumpAndSettle(); // 等待所有动画完成
```

### 4. 测试用户流程

模拟真实用户操作流程：

```dart
testWidgets('完整购物流程', (tester) async {
  // 1. 浏览商品
  await tester.tap(find.text('商品列表'));
  await tester.pumpAndSettle();

  // 2. 选择商品
  await tester.tap(find.text('商品详情'));
  await tester.pumpAndSettle();

  // 3. 加入购物车
  await tester.tap(find.text('加入购物车'));
  await tester.pumpAndSettle();

  // 4. 验证结果
  expect(find.text('已加入购物车'), findsOneWidget);
});
```

## 🐛 调试测试

### 打印调试信息

```dart
testWidgets('调试测试', (tester) async {
  await tester.pumpWidget(ProviderScope(child: MyApp()));
  await tester.pumpAndSettle();

  // 打印所有文本控件
  final textWidgets = find.byType(Text);
  for (var widget in textWidgets.evaluate()) {
    print((widget.widget as Text).data);
  }
});
```

### 慢速运行

```bash
# 使用 --timeout 参数延长超时时间
flutter test integration_test/app_integration_test.dart --timeout 60s -d <device_id>
```

## 🔄 CI/CD 集成

### GitHub Actions 示例

```yaml
name: Flutter UI Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - uses: subosito/flutter-action@v2

      - name: Install dependencies
        run: flutter pub get

      - name: Run integration tests
        uses: reactivecircus/android-emulator-runner@v2
        with:
          api-level: 34
          script: flutter test integration_test/app_integration_test.dart
```

## 📚 参考资源

- [Flutter Integration Testing](https://docs.flutter.dev/testing/integration-tests)
- [Patrol Documentation](https://patrol.leancode.co/)
- [Flutter Testing Best Practices](https://docs.flutter.dev/testing/best-practices)

## ⚠️ 常见问题

### 1. 测试超时

```bash
# 增加超时时间
flutter test integration_test/app_integration_test.dart --timeout 120s -d <device_id>
```

### 2. 设备未找到

```bash
# 检查设备连接
flutter devices

# 重启 ADB 服务（Android）
adb kill-server
adb start-server
```

### 3. 应用安装失败

```bash
# 卸载旧版本应用
adb uninstall com.molitao.app

# 清理构建缓存
flutter clean
flutter pub get
```

---

**编写测试提示：**
- 🎯 测试用户行为，而不是实现细节
- 🔄 每个测试应该是独立且可重复的
- 📝 使用描述性的测试名称
- ⏱️ 合理设置等待时间，避免测试不稳定