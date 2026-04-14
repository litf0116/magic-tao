# AI 代码审查快速开始指南

## ✅ 已完成的配置

### 1. 工具安装状态

| 工具 | 版本 | 状态 | 认证 |
|------|------|------|------|
| **Codex CLI** | v0.115.0 | ✅ 已安装 | ✅ 已登录 (OpenAI API Key) |
| **OpenCode** | v1.3.16 | ✅ 已安装 | ✅ 多个 Providers 已配置 |

### 2. OpenCode 配置的 Providers

- ✅ **Alibaba Coding Plan (China)** - 当前使用
- ✅ **Z.AI Coding Plan**
- ✅ **DeepSeek**
- ✅ **MiniMax Coding Plan**

### 3. 自动化配置

- ✅ Git Hook 已配置 (`.git/hooks/pre-commit`)
- ✅ 审查脚本已创建 (`scripts/review-with-opencode.sh`)
- ✅ 支持双重审查（Codex + OpenCode）

---

## 🚀 立即开始使用

### 方式 1: Git 提交时自动审查（推荐）⭐⭐⭐⭐⭐

```bash
# 正常提交流程
git add .
git commit -m "feat: 添加新功能"

# Git Hook 会自动运行：
# 1. Codex 快速审查
# 2. OpenCode 深度分析
# 3. 如发现问题，提示是否继续提交
```

### 方式 2: 手动使用 Codex 快速审查

```bash
# 审查未提交的代码
codex review --uncommitted

# 审查特定分支的变更
codex review --base develop

# 审查最近一次提交
codex review --commit HEAD

# 审查特定文件
codex review --files "backend/src/**/*.cs"
```

### 方式 3: 手动使用 OpenCode 深度审查

```bash
# 使用脚本审查
./scripts/review-with-opencode.sh backend/test-services/UserService_Test.cs

# 或者直接使用命令
opencode run \
  -m alibaba-coding-plan-cn/glm-5 \
  -f backend/test-services/UserService_Test.cs \
  "请详细审查这个文件"

# 切换到其他模型
opencode run \
  -m deepseek/deepseek-chat \
  -f backend/test-services/UserService_Test.cs \
  "深度分析代码架构"
```

### 方式 4: 交互式审查

```bash
# 启动 OpenCode TUI
opencode

# 然后在界面中：
# 1. 使用 Ctrl+P 添加文件
# 2. 输入审查请求
# 3. 继续追问细节
```

---

## 📊 工具对比

### Codex CLI

**优势**:
- ✅ 专为代码审查设计
- ✅ Git 深度集成
- ✅ 审查速度快
- ✅ 支持多种审查模式

**适用场景**:
- 🎯 提交前快速检查
- 🎯 分支代码对比
- 🎯 特定提交审查

**成本**: OpenAI API 调用费用

---

### OpenCode

**优势**:
- ✅ 支持多种 AI 模型
- ✅ 成本更低（阿里云模型）
- ✅ 功能更全面（MCP 工具集成）
- ✅ 支持交互式对话

**适用场景**:
- 🎯 深度代码分析
- 🎯 架构设计讨论
- 🎯 复杂问题排查
- 🎯 成本敏感场景

**成本**: 阿里云 API 费用（约为 Codex 的 1/7）

---

## 🎯 使用建议

### 场景 1: 日常提交（推荐工作流）

```
编写代码 → git add → git commit
         ↓
    Codex 快速审查（自动）
         ↓
    发现问题？
         ↓ Yes
    OpenCode 深度分析
         ↓
    修复问题 → 重新提交
```

### 场景 2: 关键代码审查

```bash
# 步骤 1: Codex 快速检查
codex review --uncommitted

# 步骤 2: OpenCode 深度分析
opencode run -m alibaba-coding-plan-cn/glm-5 \
  -f <关键文件> \
  "作为资深架构师，请深入分析：1. 架构合理性 2. 扩展性 3. 性能瓶颈"

# 步骤 3: 切换模型获取不同视角
opencode run -m deepseek/deepseek-chat \
  -f <关键文件> \
  "从安全角度审查这段代码"
```

### 场景 3: 大批量代码审查

```bash
# 审查整个目录
find backend/src -name "*.cs" -type f | \
  xargs ./scripts/review-with-opencode.sh

# 或者使用 Codex
codex review --files "backend/src/**/*.cs"
```

---

## 💡 实用技巧

### 1. 自定义审查规则

```bash
# Codex 自定义审查指令
codex review --uncommitted "重点检查：1. 安全漏洞 2. 性能问题"

# OpenCode 自定义审查
opencode run -f <file> "请重点审查：错误处理、边界条件、异常流程"
```

### 2. 多模型对比审查

```bash
# 使用不同模型获得不同视角
echo "=== 阿里云 GLM-5 审查 ===" && \
opencode run -m alibaba-coding-plan-cn/glm-5 -f <file> "审查代码" && \
echo "=== DeepSeek 审查 ===" && \
opencode run -m deepseek/deepseek-chat -f <file> "审查代码"
```

### 3. 导出审查结果

```bash
# 保存审查报告
codex review --uncommitted > review-report.txt

# OpenCode 导出会话
opencode export <session-id> > review-session.json
```

### 4. 跳过自动审查（紧急情况）

```bash
# 跳过 Git Hook
git commit --no-verify -m "emergency fix"
```

---

## 🔧 高级配置

### 配置 Codex 默认模型

```bash
# 编辑配置文件
vim ~/.codex/config.toml

# 添加：
[model]
name = "gpt-4"
```

### 配置 OpenCode 默认模型

```bash
# 编辑配置文件
vim ~/.config/opencode/opencode.json

# 修改：
{
  "model": "alibaba-coding-plan-cn/glm-5"
}
```

### 禁用某个工具

```bash
# 禁用 Codex
mv ~/.codex ~/.codex.bak

# 禁用 OpenCode
mv ~/.config/opencode ~/.config/opencode.bak

# 或者修改 Git Hook，注释掉对应的审查步骤
```

---

## 📚 相关文档

- **详细对比**: [docs/codex-vs-opencode-review.md](./codex-vs-opencode-review.md)
- **测试覆盖率**: [docs/test-coverage-guide.md](./test-coverage-guide.md)
- **AI 审查指南**: [docs/ai-code-review-guide.md](./ai-code-review-guide.md)
- **代码质量自动化**: [docs/code-quality-automation.md](./code-quality-automation.md)

---

## 🆘 常见问题

### Q1: Codex 审查很慢怎么办？

**A:** 尝试使用 OpenCode 的阿里云模型，速度更快且成本更低：
```bash
opencode run -m alibaba-coding-plan-cn/glm-5 -f <file> "审查代码"
```

### Q2: 如何只使用 OpenCode 进行审查？

**A:** 编辑 `.git/hooks/pre-commit`，注释掉 Codex 部分，或者：
```bash
# 临时禁用 Codex
unset OPENAI_API_KEY

# 提交时只使用 OpenCode
git commit -m "message"
```

### Q3: 审查结果不准确怎么办？

**A:** 尝试以下方法：
1. 使用更强大的模型（GPT-4 或 DeepSeek）
2. 提供更详细的审查指令
3. 分步骤审查（先安全，再性能，最后代码质量）

### Q4: 如何查看历史审查记录？

**A:**
```bash
# Codex 会话管理
codex resume

# OpenCode 会话管理
opencode session list
opencode export <session-id>
```

---

## ✨ 下一步行动

1. **立即测试**
   ```bash
   # 修改一个文件
   echo "// test" >> backend/test-services/UserService_Test.cs

   # 尝试提交
   git add backend/test-services/UserService_Test.cs
   git commit -m "test: AI 审查测试"
   ```

2. **选择主要工具**
   - 推荐：日常使用 OpenCode（成本低），关键代码使用 Codex（质量高）

3. **建立审查习惯**
   - 每次提交前自动审查
   - 每周回顾审查结果
   - 持续改进代码质量

---

**最后更新**: 2026-04-06
**工具版本**: Codex v0.115.0, OpenCode v1.3.16