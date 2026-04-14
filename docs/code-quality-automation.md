# 代码质量自动化指南

## 🎯 概述

本指南整合了两个核心自动化方案：
1. **AI辅助代码审查** - 解决单人开发缺少代码审查的问题
2. **单元测试覆盖率统计** - 建立测试质量度量体系

---

## 🤖 一、AI辅助代码审查

### 快速开始

#### 方案A：Codex CLI（推荐）

```bash
# 1. 安装 Codex CLI
npm install -g @openai/codex-cli

# 2. 设置 API Key
export OPENAI_API_KEY="your-key-here"

# 3. 测试审查
cd /Users/mac/workspace/magic-tao
codex review --files "backend/src/TtWork.Project/Applications/UserAppService.cs"
```

#### 方案B：Cursor IDE

```bash
# 1. 下载安装 Cursor
# https://cursor.sh/

# 2. 打开项目
cursor /Users/mac/workspace/magic-tao

# 3. 使用快捷键
# Cmd+K: 快速提问
# Cmd+I: 代码审查
```

#### 方案C：GitHub Copilot

```bash
# 1. 安装 VS Code 扩展
# https://marketplace.visualstudio.com/items?itemName=GitHub.copilot

# 2. 订阅服务（$10/月）

# 3. 使用 Chat 功能
# Ctrl+Shift+P → "GitHub Copilot: Open Chat"
```

### Git Hook 自动审查

已配置 `.git/hooks/pre-commit`，每次提交前自动审查代码。

**使用方式：**
```bash
# 正常提交流程
git add .
git commit -m "feat: 添加用户登录功能"

# Hook 会自动运行 Codex 审查
# 如果发现问题，会提示是否继续提交
```

**跳过审查（不推荐）：**
```bash
git commit --no-verify -m "emergency fix"
```

---

## 📊 二、单元测试覆盖率统计

### 快速开始

#### 运行所有模块测试

```bash
# 统一运行所有测试
./run-all-tests.sh

# 输出示例：
# ╔════════════════════════════════════════════╗
# ║   魔力淘 - 统一测试覆盖率统计             ║
# ╚════════════════════════════════════════════╝
#
# [1/4] 运行 Backend 测试...
# [2/4] 运行 PC 测试...
# [3/4] 运行 Flutter 测试...
# [4/4] 运行 UniApp 测试...
#
# 测试结果汇总：
#   ✅ Backend
#   ✅ PC
#   ❌ Flutter
#   ⏭️  UniApp (跳过)
```

#### 运行单个模块测试

**Backend (.NET):**
```bash
cd backend
./scripts/run-tests-with-coverage.sh

# 报告位置：backend/coverage-report/index.html
```

**PC (Vue):**
```bash
cd pc
npm run test:coverage

# 报告位置：pc/coverage/index.html
```

**Flutter App:**
```bash
cd molitao_app
./scripts/run-tests-with-coverage.sh

# 报告位置：molitao_app/coverage/html/index.html
```

**UniApp:**
```bash
cd molitao_uniapp
npm run test:coverage

# 报告位置：molitao_uniapp/coverage/index.html
```

---

## 🔄 三、CI/CD 集成

### GitHub Actions 配置

创建 `.github/workflows/test-coverage.yml`:

```yaml
name: Test Coverage

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: cd backend && ./scripts/run-tests-with-coverage.sh
      - uses: codecov/codecov-action@v3
        with:
          files: ./backend/TestResults/**/coverage.opencover.xml
          flags: backend

  pc:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '20'
      - run: cd pc && npm ci && npm run test:coverage
      - uses: codecov/codecov-action@v3
        with:
          files: ./pc/coverage/lcov.info
          flags: pc

  flutter:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: subosito/flutter-action@v2
        with:
          flutter-version: '3.19.0'
      - run: cd molitao_app && flutter pub get && flutter test --coverage
      - uses: codecov/codecov-action@v3
        with:
          files: ./molitao_app/coverage/lcov.info
          flags: flutter
```

---

## 📈 四、覆盖率目标

### 短期目标（1个月）

| 模块 | 当前 | 目标 | 优先级 |
|------|------|------|--------|
| Backend | ~5% | 50% | 高 |
| PC | ~0% | 40% | 中 |
| Flutter | ~0% | 40% | 中 |
| UniApp | ~5% | 40% | 低 |

### 中期目标（3个月）

| 模块 | 目标 |
|------|------|
| Backend | 70% |
| PC | 60% |
| Flutter | 50% |
| UniApp | 60% |

### 长期目标（6个月）

| 模块 | 目标 |
|------|------|
| 所有模块 | 70%+ |
| 核心业务逻辑 | 80%+ |

---

## 🎓 五、最佳实践

### 测试编写优先级

**优先测试（P0）：**
- ✅ 核心业务逻辑（用户认证、支付、拍卖）
- ✅ 复杂算法和计算
- ✅ 错误处理和边界条件
- ✅ 安全相关功能

**次优先测试（P1）：**
- ⚠️ 工具函数和辅助方法
- ⚠️ API 层
- ⚠️ 数据转换逻辑

**可选测试（P2）：**
- 📝 简单的 CRUD 操作
- 📝 纯展示组件
- 📝 配置文件

### 代码审查检查清单

每次提交前自查：

- [ ] 代码符合 CLAUDE.md 规范
- [ ] 无类型错误（避免 any）
- [ ] 错误处理完善
- [ ] 无安全漏洞
- [ ] 关键逻辑有测试
- [ ] 注释合理（不过度注释）

---

## 🛠️ 六、工具安装

### 必需工具

```bash
# Backend 覆盖率报告生成
dotnet tool install -g dotnet-reportgenerator-globaltool

# Flutter 覆盖率报告生成 (macOS)
brew install lcov

# Flutter 覆盖率报告生成 (Linux)
sudo apt-get install lcov
```

### 可选工具

```bash
# AI 代码审查
npm install -g @openai/codex-cli

# 代码覆盖率徽章
npm install -g coverage-badges
```

---

## 📚 七、相关文档

- **AI代码审查详细指南**: [docs/ai-code-review-guide.md](./ai-code-review-guide.md)
- **测试覆盖率配置指南**: [docs/test-coverage-guide.md](./test-coverage-guide.md)
- **项目编码规范**: [AGENTS.md](../AGENTS.md)
- **后端开发规范**: [backend/CLAUDE.md](../backend/CLAUDE.md)
- **PC前端开发规范**: [pc/CLAUDE.md](../pc/CLAUDE.md)

---

## 🆘 八、常见问题

### Q1: Codex CLI 报错 "API key not found"

**A:** 确保 API Key 已正确设置：
```bash
echo $OPENAI_API_KEY  # 检查是否设置
export OPENAI_API_KEY="sk-..."  # 设置 API Key
```

### Q2: 测试覆盖率报告无法打开

**A:** 检查文件路径和权限：
```bash
# Backend
ls -la backend/coverage-report/index.html

# PC
ls -la pc/coverage/index.html

# Flutter
ls -la molitao_app/coverage/html/index.html
```

### Q3: Flutter 测试覆盖率低

**A:** Flutter 项目刚开始，测试文件很少，需要逐步补充：
```bash
# 查看当前测试文件
find molitao_app/test -name "*_test.dart"
```

### Q4: 如何跳过某个模块的测试

**A:** 编辑 `run-all-tests.sh`，注释掉对应的测试函数调用：
```bash
# run_backend_tests  # 跳过 Backend 测试
run_pc_tests
run_flutter_tests
run_uniapp_tests
```

---

## 💡 九、建议与反馈

作为单人开发者，建议：

1. **每天运行测试**
   ```bash
   ./run-all-tests.sh
   ```

2. **每次提交前审查**
   ```bash
   git add .
   codex review  # 手动审查
   git commit -m "feat: xxx"  # Hook 自动审查
   ```

3. **每周查看覆盖率趋势**
   - 关注覆盖率上升/下降趋势
   - 识别未测试的关键模块

4. **持续学习改进**
   - 记录 AI 发现的常见问题
   - 更新项目规范
   - 优化测试策略

---

**最后更新**: 2026-04-06
**维护者**: LiTengFei0312