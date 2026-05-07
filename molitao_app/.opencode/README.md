# 自主执行系统使用指南

**核心理念**: 设定目标 → 系统自动执行 → 达成目标 → 停止

---

## 🚀 快速开始

### 基本用法

```bash
# 在 OpenCode 中输入
/autonomous "你的目标描述"
```

### 示例

```bash
# 技术调研
/autonomous "调研 Flutter 性能优化的所有方法，生成最佳实践文档"

# 代码重构
/autonomous "将用户认证模块从 Provider 重构为 Riverpod"

# 学习新技术
/autonomous "学习 GoRouter 完整用法，创建学习笔记和示例代码"

# 问题解决
/autonomous "修复所有 TypeScript 类型错误"
```

---

## 📋 执行流程

```
1. 目标分析 → 分解为子任务
2. 计划确认 → 显示执行计划
3. 自主执行 → Ralph Loop 循环执行
4. 进度追踪 → 实时报告状态
5. 目标验证 → 检查完成标准
6. 知识存储 → 保存到 Graphiti
```

---

## 🎯 目标设定原则

### ✅ 好的目标

- **具体明确**: "创建用户注册功能，包括表单验证和测试"
- **可验证**: "优化首页加载速度，目标 LCP < 2.5s"
- **范围合理**: "重构用户认证模块"（而非"重构整个项目"）

### ❌ 不好的目标

- **模糊**: "改进代码质量"（什么是"质量"？）
- **过大**: "重构整个项目"（范围太大）
- **无法验证**: "学习 Flutter"（没有明确标准）

---

## 🔄 执行控制

### 实时监控

```bash
/status    # 查看当前进度
/logs      # 查看执行日志
/current   # 查看当前任务
```

### 用户干预

```bash
/pause              # 暂停执行
/resume             # 恢复执行
/adjust "新目标"    # 调整目标
/stop               # 停止并保存进度
```

---

## 📊 输出内容

### 自动生成

- ✅ 执行报告（Markdown）
- ✅ 输出文件（代码/文档）
- ✅ 知识卡片（Graphiti）
- ✅ 检查点（可恢复）

### 存储位置

```
.opencode/
├── checkpoints/        # 执行检查点
├── reports/           # 执行报告
└── logs/              # 执行日志

docs/
├── autonomous_reports/  # 目标完成报告
└── [项目文档]          # 生成的文档
```

---

## 💡 最佳实践

### 1. 分阶段执行

```bash
# 先调研
/autonomous "调研最佳状态管理方案"

# 再实施
/autonomous "根据调研结果重构状态管理"
```

### 2. 增量迭代

```bash
# 基础功能
/autonomous "实现用户认证基础功能"

# 增强功能
/autonomous "添加双因素认证"

# 优化
/autonomous "优化性能和用户体验"
```

### 3. 查询历史经验

```bash
# 查询知识库
opencode graphiti query "如何优化 Flutter 性能"

# 查看历史执行
opencode graphiti query "类似的重构任务"
```

---

## 📚 文档索引

| 文档 | 说明 |
|------|------|
| `GOAL_DRIVEN_SYSTEM.md` | 完整系统架构文档 |
| `GOAL_TRACKING_SYSTEM.md` | 进度追踪和验证系统 |
| `USAGE_EXAMPLES.md` | 详细使用示例 |

---

## 🆘 常见问题

**Q: 执行时间过长？**
A: 使用 `/adjust --mode quick` 切换到快速模式

**Q: 成本超出预期？**
A: 使用 `/adjust` 缩小目标范围

**Q: 想修改目标？**
A: 使用 `/adjust "新目标"` 随时调整

**Q: 如何恢复中断的执行？**
A: 使用 `/autonomous --resume` 恢复

---

## 🎉 开始使用

```bash
# 立即开始你的第一个自主执行任务
/autonomous "你的目标"
```

让 AI 为你自动工作，直到目标达成！
