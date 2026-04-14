#!/bin/bash

# OpenCode 代码审查脚本
# 使用阿里云 GLM-5 模型进行代码审查

set -e

FILES="$@"
MODEL="alibaba-coding-plan-cn/glm-5"

if [ -z "$FILES" ]; then
    echo "🔍 未指定文件，审查当前 staged 的文件..."
    FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.(cs|ts|vue|dart)$' | tr '\n' ' ')
fi

if [ -z "$FILES" ]; then
    echo "✅ 没有需要审查的代码文件"
    exit 0
fi

echo "╔════════════════════════════════════════════╗"
echo "║       OpenCode 代码审查 (GLM-5)            ║"
echo "╚════════════════════════════════════════════╝"
echo ""
echo "📄 审查文件:"
echo "$FILES" | tr ' ' '\n' | grep -v '^$' | while read file; do
    echo "  - $file"
done
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

opencode run \
  -m "$MODEL" \
  $([ ! -z "$FILES" ] && echo "-f $FILES") \
  "作为资深代码审查专家，请审查这些文件。

## 审查维度

### 1. 代码质量 (权重: 30%)
- 命名规范（PascalCase/camelCase）
- 代码复杂度（避免过深嵌套）
- 重复代码（DRY 原则）
- 注释质量（必要且有意义的注释）

### 2. 类型安全 (权重: 20%)
- 避免使用 any 类型
- 正确的类型定义
- 接口完整性

### 3. 错误处理 (权重: 20%)
- try-catch 包裹异步操作
- 有意义的错误信息
- 边界条件处理
- 空值检查

### 4. 安全性 (权重: 15%)
- 输入验证
- SQL 注入防护
- XSS 防护
- 敏感信息保护

### 5. 性能 (权重: 15%)
- 避免 N+1 查询
- 合理使用缓存
- 异步操作优化
- 内存泄漏风险

## 输出格式

请按以下格式输出：

**严重问题 (Critical)** - 必须修复
- 🔴 [文件名:行号] 问题描述
  - 影响: ...
  - 建议: ...
  - 示例代码: ...

**重要问题 (High)** - 强烈建议修复
- 🟠 [文件名:行号] 问题描述
  - 建议: ...

**一般问题 (Medium)** - 建议改进
- 🟡 [文件名:行号] 问题描述
  - 建议: ...

**优化建议 (Low)** - 可选改进
- 🟢 [文件名:行号] 建议描述

## 总结
- 问题数量统计
- 主要改进方向
- 整体代码质量评分 (1-10)"

EXIT_CODE=$?

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ 审查完成"
    echo ""
    echo "💡 提示:"
    echo "  - 使用 DeepSeek 模型深度分析: ./scripts/review-with-opencode.sh -m deepseek/deepseek-chat $FILES"
    echo "  - 使用 Codex 快速审查: codex review --uncommitted"
else
    echo "❌ 审查过程出现问题"
    exit 1
fi