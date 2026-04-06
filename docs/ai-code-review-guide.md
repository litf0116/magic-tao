# AI 辅助代码审查方案

## 概述

针对单人开发项目，使用 AI 工具辅助代码审查，确保代码质量和最佳实践。

## 方案对比

| 方案 | 优势 | 劣势 | 推荐度 |
|------|------|------|--------|
| Codex CLI | 集成Git流程，自动化审查 | 需要OpenAI API key | ⭐⭐⭐⭐⭐ |
| Cursor IDE | 实时审查，IDE集成 | 需要切换编辑器 | ⭐⭐⭐⭐ |
| GitHub Copilot | 实时建议，广泛支持 | 需要订阅费用 | ⭐⭐⭐⭐ |
| Codeium | 免费，多语言支持 | 功能相对简单 | ⭐⭐⭐ |
| 自建AI审查 | 可定制，无费用 | 需要开发维护 | ⭐⭐ |

---

## 方案 1: Codex CLI (推荐) ⭐⭐⭐⭐⭐

### 安装

```bash
# 使用 npm 安装
npm install -g @openai/codex-cli

# 或使用 yarn
yarn global add @openai/codex-cli
```

### 配置

1. **设置 OpenAI API Key**

```bash
# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"

# 或添加到 ~/.zshrc 或 ~/.bashrc
echo 'export OPENAI_API_KEY="your-api-key-here"' >> ~/.zshrc
```

2. **配置项目审查规则**

创建 `.codex/config.json` 在项目根目录：

```json
{
  "review": {
    "rules": [
      "遵循项目 CLAUDE.md 中的编码规范",
      "检查潜在的安全问题",
      "确保错误处理完善",
      "验证类型安全（避免 any/unknown 滥用）",
      "检查命名规范（PascalCase/camelCase）"
    ],
    "severity": "warning",
    "autoFix": false,
    "excludePatterns": [
      "**/node_modules/**",
      "**/dist/**",
      "**/.git/**"
    ]
  }
}
```

3. **使用方式**

```bash
# 审查当前 staged 的文件
codex review

# 审查指定文件
codex review src/api/user.ts backend/src/Services/UserService.cs

# 审查最近的提交
codex review --commit HEAD~1

# 审查特定分支的变更
codex review --branch feature/new-feature
```

4. **Git Hook 自动审查**

已配置 `.git/hooks/pre-commit`，每次提交前自动审查。

---

## 方案 2: Cursor IDE

### 优势

- **实时代码审查**：在编写代码时即时获得建议
- **多文件理解**：AI 理解整个项目上下文
- **Chat 功能**：直接与 AI 讨论代码设计

### 安装

下载地址：https://cursor.sh/

### 配置

1. **导入 VS Code 设置**

Cursor 基于 VS Code，可以导入现有配置。

2. **配置 AI 规则**

在项目根目录创建 `.cursorrules` 文件：

```
# AI Review Rules

## 代码规范
- 遵循 CLAUDE.md 中的编码规范
- 使用 TypeScript 严格模式
- 避免使用 any 类型

## 审查重点
- 安全漏洞（SQL注入、XSS等）
- 性能问题（N+1查询、内存泄漏）
- 错误处理完整性
- 测试覆盖率

## 建议格式
当发现问题时，提供：
1. 问题描述
2. 修复建议
3. 代码示例
```

3. **使用方式**

```bash
# 在 Cursor 中：
# - Cmd+K: 快速提问
# - Cmd+L: 打开 Chat 面板
# - Cmd+I: 代码审查
```

---

## 方案 3: GitHub Copilot

### 安装

1. **VS Code 扩展**

安装 GitHub Copilot 扩展：https://marketplace.visualstudio.com/items?itemName=GitHub.copilot

2. **订阅**

需要 GitHub Copilot 订阅（个人版 $10/月）。

### 配置

在 `.vscode/settings.json` 中：

```json
{
  "github.copilot.enable": {
    "*": true,
    "yaml": false,
    "plaintext": false
  },
  "github.copilot.advanced": {
    "debug.enableEngineTracing": true,
    "debug.showScores": true
  }
}
```

### 使用方式

- **实时代码建议**：编写代码时自动提示
- **Copilot Chat**：Ctrl+Shift+P → "GitHub Copilot: Open Chat"
- **解释代码**：选中代码 → 右键 → "Copilot: Explain This"

---

## 方案 4: Codeium (免费替代)

### 安装

VS Code 扩展：https://marketplace.visualstudio.com/items?itemName=Codeium.codeium

### 配置

免费使用，无需 API Key。

### 优势

- 完全免费
- 支持多种语言
- 轻量级，不占用太多资源

---

## 推荐组合方案

### 开发时：Cursor IDE + GitHub Copilot
- Cursor 处理复杂逻辑和重构
- Copilot 提供实时代码建议

### 提交前：Codex CLI
- 自动审查 staged 文件
- 确保符合项目规范

### CI/CD：自动化审查
- 在 CI 流程中添加 Codex 审查
- 阻断不符合规范的 PR

---

## 审查检查清单

AI 审查应覆盖以下方面：

### ✅ 代码质量
- [ ] 命名规范（PascalCase/camelCase）
- [ ] 代码复杂度（避免嵌套过深）
- [ ] 重复代码（DRY 原则）
- [ ] 注释质量（自文档化代码）

### ✅ 类型安全
- [ ] 避免使用 `any` 类型
- [ ] 正确使用 `unknown` 和类型守卫
- [ ] 接口定义完整
- [ ] 泛型使用合理

### ✅ 错误处理
- [ ] try-catch 包裹异步操作
- [ ] 错误信息有意义
- [ ] 边界条件处理
- [ ] 空值检查

### ✅ 安全性
- [ ] 输入验证
- [ ] SQL 注入防护
- [ ] XSS 防护
- [ ] 敏感信息不泄露

### ✅ 性能
- [ ] 避免 N+1 查询
- [ ] 合理使用缓存
- [ ] 异步操作不阻塞
- [ ] 内存泄漏检查

### ✅ 测试
- [ ] 单元测试覆盖关键逻辑
- [ ] 边界条件测试
- [ ] 错误路径测试

---

## 最佳实践

1. **定期审查**
   - 每天结束前审查当天提交
   - 每周回顾审查发现的问题模式

2. **学习改进**
   - 记录 AI 发现的问题类型
   - 更新项目规范文档
   - 建立"问题知识库"

3. **结合人工判断**
   - AI 建议仅供参考
   - 保留代码设计决策权
   - 理解问题本质，而非盲目修复

4. **持续优化**
   - 调整 AI 审查规则
   - 添加项目特定检查
   - 反馈误报给 AI 提供商

---

## 相关资源

- [Codex CLI 文档](https://github.com/openai/codex-cli)
- [Cursor 使用指南](https://cursor.sh/docs)
- [GitHub Copilot 最佳实践](https://docs.github.com/en/copilot/using-github-copilot/best-practices-for-using-github-copilot)
- [Codeium 文档](https://codeium.com/docs)

---

**最后更新**: 2026-04-06