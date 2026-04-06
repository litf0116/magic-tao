# Codex CLI vs OpenCode 代码审查对比指南

## 📊 工具概览

### Codex CLI (v0.115.0)

**定位**: 专用的 AI 代码审查和执行工具

**优势**:
- ✅ 专为代码审查设计
- ✅ `codex review` 命令功能强大
- ✅ 支持审查未提交代码、分支对比、特定提交
- ✅ 集成 Git 工作流
- ✅ 支持自定义审查指令

**认证状态**: ✅ 已登录 (使用 OpenAI API Key)

---

### OpenCode (v1.3.16)

**定位**: 通用 AI 助手，支持多种任务

**优势**:
- ✅ 更通用的 AI 能力
- ✅ 支持多种 AI 模型（已配置阿里云、DeepSeek、MiniMax等）
- ✅ 集成丰富的 MCP 工具
- ✅ 支持会话管理和上下文
- ✅ 支持 TUI 和 Web 界面

**认证状态**: ✅ 已配置多个 providers
- Alibaba Coding Plan (China) ✅
- Z.AI Coding Plan ✅
- DeepSeek ✅
- MiniMax Coding Plan ✅

---

## 🔍 功能对比

| 功能特性 | Codex CLI | OpenCode | 优势方 |
|---------|-----------|----------|--------|
| **代码审查专用命令** | ✅ `codex review` | ⚠️ 通过 `run` 实现 | Codex ⭐ |
| **审查未提交代码** | ✅ `--uncommitted` | ⚠️ 需手动指定 | Codex ⭐ |
| **分支对比审查** | ✅ `--base <branch>` | ⚠️ 需手动操作 | Codex ⭐ |
| **审查特定提交** | ✅ `--commit <SHA>` | ⚠️ 需手动操作 | Codex ⭐ |
| **Git 集成** | ✅ 深度集成 | ⚠️ 通过 MCP | Codex ⭐ |
| **多模型支持** | ❌ 仅 OpenAI | ✅ 多个 providers | OpenCode ⭐ |
| **成本效益** | ⚠️ OpenAI API 收费 | ✅ 阿里云/DeepSeek 较便宜 | OpenCode ⭐ |
| **MCP 工具集成** | ⚠️ 部分支持 | ✅ 完整支持 | OpenCode ⭐ |
| **会话管理** | ✅ `resume` | ✅ 更强大 | 平手 |
| **Web UI** | ⚠️ App 下载 | ✅ 内置 | OpenCode ⭐ |
| **自定义 Agent** | ⚠️ 有限 | ✅ 完整支持 | OpenCode ⭐ |

---

## 🚀 使用场景推荐

### 场景 1: Git 提交前自动审查 ⭐⭐⭐⭐⭐

**推荐**: **Codex CLI**

**理由**:
- 专用 `codex review --uncommitted` 命令
- 深度 Git 集成
- 可直接集成到 Git Hook

**使用方式**:
```bash
# 审查当前未提交的更改
codex review --uncommitted

# 审查特定分支的变更
codex review --base develop

# 审查最近一次提交
codex review --commit HEAD
```

---

### 场景 2: 代码质量深度分析 ⭐⭐⭐⭐

**推荐**: **OpenCode**

**理由**:
- 支持多种 AI 模型
- 可选择不同模型获得不同视角
- 成本更低（阿里云模型）

**使用方式**:
```bash
# 使用默认模型（阿里云 GLM-5）
opencode run "请审查 backend/test-services/UserService_Test.cs 文件，检查代码质量、潜在问题和改进建议"

# 指定其他模型
opencode run -m deepseek/deepseek-chat "审查代码并给出重构建议"

# 附加文件审查
opencode run -f backend/test-services/UserService_Test.cs "请详细审查这个文件"
```

---

### 场景 3: 复杂代码架构审查 ⭐⭐⭐⭐⭐

**推荐**: **OpenCode + MCP 工具**

**理由**:
- 可结合 GitHub MCP 查看历史提交
- 使用 sequential-thinking 深度分析
- 支持上下文对话

**使用方式**:
```bash
# 启动交互式会话
opencode

# 然后在 TUI 中：
# 1. 上传相关文件
# 2. 提问："请审查这个架构设计，考虑扩展性和维护性"
# 3. 继续追问："如何优化这个模块的性能？"
```

---

### 场景 4: 日常快速审查 ⭐⭐⭐⭐⭐

**推荐**: **两者结合**

**方式**:
1. **先用 Codex 快速审查**
   ```bash
   codex review --uncommitted
   ```

2. **再用 OpenCode 深度分析**
   ```bash
   opencode run -f <file> "详细审查这个文件的安全性和性能"
   ```

---

## 💡 最佳实践建议

### 方案 A: 主用 Codex + 辅助 OpenCode（推荐）

**配置 Git Hook**:
```bash
# .git/hooks/pre-commit
#!/bin/bash

echo "🔍 正在进行 AI 代码审查..."

# 获取变更文件
STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.(cs|ts|vue|dart)$')

if [ -z "$STAGED_FILES" ]; then
    echo "✅ 没有需要审查的代码文件"
    exit 0
fi

# 使用 Codex 进行快速审查
echo "🤖 Codex 审查:"
codex review --uncommitted

if [ $? -ne 0 ]; then
    echo "⚠️  Codex 发现问题，建议使用 OpenCode 深度分析："
    echo "  opencode run -f <problem-file> '详细分析这个问题'"
    
    read -p "是否继续提交？(y/n): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo "✅ 代码审查完成"
exit 0
```

---

### 方案 B: 主用 OpenCode（成本优化）

**创建审查脚本**:
```bash
#!/bin/bash
# scripts/ai-review.sh

FILES="$1"

if [ -z "$FILES" ]; then
    echo "用法: ./scripts/ai-review.sh <文件路径>"
    exit 1
fi

echo "🤖 OpenCode 代码审查:"
echo ""

# 使用阿里云模型审查（更便宜）
opencode run \
  -m alibaba-coding-plan-cn/glm-5 \
  -f "$FILES" \
  "作为资深代码审查专家，请审查这些文件：
  
1. 代码质量：命名规范、代码复杂度、重复代码
2. 安全性：输入验证、SQL注入、XSS、敏感信息
3. 性能：N+1查询、内存泄漏、异步处理
4. 错误处理：异常捕获、边界条件
5. 测试覆盖：关键逻辑是否需要测试

请给出：
- 问题列表（按严重程度排序）
- 修复建议
- 代码示例（如适用）"

echo ""
echo "💡 如需更深入分析，可切换到 DeepSeek 模型："
echo "  opencode run -m deepseek/deepseek-chat -f $FILES '深度分析...'"
```

---

### 方案 C: 双重审查（最高质量）

```bash
#!/bin/bash
# scripts/dual-review.sh

echo "🔬 双重 AI 代码审查流程"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

FILES="$1"

if [ -z "$FILES" ]; then
    FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.(cs|ts|vue|dart)$' | tr '\n' ' ')
fi

if [ -z "$FILES" ]; then
    echo "⚠️  没有需要审查的文件"
    exit 0
fi

echo ""
echo "[1/2] 🤖 Codex 快速审查..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
codex review --uncommitted

CODEX_EXIT=$?

echo ""
echo "[2/2] 🧠 OpenCode 深度分析..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
opencode run \
  -m alibaba-coding-plan-cn/glm-5 \
  -f $FILES \
  "作为代码审查专家，请从以下维度深入分析：
  
## 架构设计
- 模块职责是否清晰
- 依赖关系是否合理
- 扩展性如何

## 代码质量
- 是否符合 SOLID 原则
- 是否有代码坏味道
- 测试覆盖建议

## 安全性
- 潜在的安全风险
- 数据验证是否充分

## 性能
- 性能瓶颈
- 优化建议

请给出详细的改进建议和代码示例。"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ 双重审查完成"

if [ $CODEX_EXIT -ne 0 ]; then
    echo ""
    echo "⚠️  Codex 发现问题，请仔细检查上述建议"
    exit 1
fi
```

---

## 💰 成本对比

### Codex CLI (OpenAI)

**定价**:
- GPT-4: ~$0.03/1K tokens
- GPT-3.5: ~$0.002/1K tokens

**估算**:
- 审查一个中等文件: ~$0.01-0.05
- 每日审查 20 个文件: ~$0.20-1.00
- 月成本: ~$6-30

---

### OpenCode (阿里云/DeepSeek)

**定价**:
- 阿里云 GLM-5: ~¥0.001/1K tokens (~$0.00014)
- DeepSeek: ~¥0.001/1K tokens

**估算**:
- 审查一个中等文件: ~¥0.01-0.05 (~$0.001-0.007)
- 每日审查 20 个文件: ~¥0.2-1.0 (~$0.03-0.14)
- 月成本: ~¥6-30 (~$0.8-4.2)

**成本优势**: OpenCode 使用阿里云模型，成本约为 Codex 的 **1/7**

---

## 🎯 最终建议

### 推荐方案: **混合使用**

```
日常提交流程:
┌─────────────────────────────────────┐
│ 1. 编写代码                          │
│ 2. Git add                          │
│ 3. Git commit (触发 Git Hook)       │
│    ↓                                │
│    Codex review --uncommitted       │ ← 快速审查
│    ↓                                │
│    发现问题？                        │
│    ↓ Yes                            │
│    opencode run "深度分析"          │ ← 详细分析
│    ↓                                │
│    修复后继续提交                    │
└─────────────────────────────────────┘
```

### 配置步骤

**1. 更新 Git Hook**
```bash
chmod +x .git/hooks/pre-commit
```

**2. 创建快捷脚本**
```bash
# 快速审查
echo 'codex review --uncommitted' > scripts/quick-review.sh
chmod +x scripts/quick-review.sh

# 深度审查
echo 'opencode run -m alibaba-coding-plan-cn/glm-5 -f "$@" "详细审查代码"' > scripts/deep-review.sh
chmod +x scripts/deep-review.sh
```

**3. 添加到 package.json**
```json
{
  "scripts": {
    "review": "codex review --uncommitted",
    "review:deep": "opencode run -m alibaba-coding-plan-cn/glm-5 '详细审查当前目录的代码'"
  }
}
```

---

## 📚 相关资源

### Codex CLI
- 官方文档: https://github.com/openai/codex-cli
- 审查命令: `codex review --help`
- 登录状态: `codex login status`

### OpenCode
- 官方网站: https://opencode.ai
- 配置文件: `~/.config/opencode/opencode.json`
- Providers: `opencode providers list`
- 可用模型: `opencode models`

---

## 🔧 故障排查

### Codex 登录问题
```bash
# 检查登录状态
codex login status

# 重新登录
codex login

# 使用环境变量
export OPENAI_API_KEY="sk-..."
codex login --with-api-key
```

### OpenCode Provider 问题
```bash
# 列出 providers
opencode providers list

# 登录特定 provider
opencode providers login https://api.alibaba-cloud.com

# 切换模型
opencode run -m deepseek/deepseek-chat "your message"
```

---

**最后更新**: 2026-04-06
**建议**: 优先使用 OpenCode 进行日常审查，Codex 用于关键代码的快速检查