# Autonomous Execution - Quick Start Examples

自主执行系统使用示例和最佳实践。

---

## 🚀 快速开始

### 示例 1: 技术调研

```bash
# 在 OpenCode 中输入
/autonomous "调研 Flutter 性能优化的所有方法，包括：
  - 渲染性能优化
  - 内存优化
  - 启动速度优化
  - 包体积优化
生成详细的最佳实践文档，每个优化点都要有代码示例"
```

**执行过程**:

```
🎯 目标: Flutter 性能优化完整调研

📋 执行计划:
  [1] 搜索 Flutter 性能优化资料 (crawl4ai.search)
  [2] 抓取官方文档和最佳实践 (crawl4ai.fetch)
  [3] 整理优化技巧分类 (llm.analyze)
  [4] 为每个优化点收集示例代码 (crawl4ai.search_batch)
  [5] 生成结构化文档 (llm.generate)
  [6] 创建代码示例文件 (file.write)
  [7] 存储到知识库 (graphiti.remember)

⏱️ 预计时间: 40-60 分钟
💰 预计成本: $3-5

✅ 确认开始？[Y/n]
```

**实时进度**:

```
[1/7] 搜索 Flutter 性能优化资料
  ✅ 找到 25 篇相关文章
  ✅ 完成 (3分钟)

[2/7] 抓取官方文档和最佳实践
  ✅ 已抓取 Flutter.dev 官方文档
  ✅ 已抓取 10 篇高质量文章
  ✅ 完成 (8分钟)

[3/7] 整理优化技巧分类
  🔄 正在分析和分类...
  ✅ 已分类为 4 大类 15 小类
  ✅ 完成 (5分钟)

[实时进度: 43%] [已用时: 16分钟] [当前成本: $1.5]
```

**完成输出**:

```
🎉 目标达成！

📊 执行摘要:
  ✓ 已收集 25+ 篇性能优化文章
  ✓ 已整理 15 个优化技巧
  ✓ 已生成 20+ 个代码示例
  ✓ 已创建完整文档

📁 输出文件:
  - docs/flutter_performance_optimization.md (主文档)
  - docs/flutter_performance_examples/ (代码示例)
    ├── render_optimization.dart
    ├── memory_optimization.dart
    ├── startup_optimization.dart
    └── size_optimization.dart

🧠 已存储到知识库:
  - 15 个优化技巧已添加
  - 关联到 Flutter 项目上下文

⏱️ 总用时: 52 分钟
💰 总成本: $4.2
```

---

### 示例 2: 代码重构

```bash
/autonomous "将用户认证模块从 Provider 重构为 Riverpod，确保：
  - 所有功能保持不变
  - 添加单元测试
  - 更新文档
  - 验证集成正常"
```

**执行计划**:

```
🎯 目标: 重构用户认证模块

📋 执行计划:
  [1] 分析现有 Provider 实现 (code.analyze)
  [2] 设计 Riverpod 架构方案 (llm.design)
  [3] 重构状态管理代码 (code.refactor)
  [4] 更新 UI 层代码 (code.refactor)
  [5] 编写单元测试 (test.write)
  [6] 运行测试验证 (test.run)
  [7] 更新技术文档 (file.write)
  [8] 集成测试 (test.integration)

⏱️ 预计时间: 60-90 分钟
💰 预计成本: $5-8
```

**执行过程**:

```
[1/8] 分析现有 Provider 实现
  ✅ 扫描 lib/auth/ 目录
  ✅ 识别 5 个 Provider 类
  ✅ 提取依赖关系
  ✅ 完成 (5分钟)

[2/8] 设计 Riverpod 架构方案
  🔄 设计状态管理方案...
  ✅ 方案：使用 StateNotifier + AsyncValue
  ✅ 已生成架构图
  ✅ 完成 (8分钟)

[3/8] 重构状态管理代码
  🔄 重构 AuthProvider → AuthNotifier
  ✅ 已创建 auth_provider.dart
  ✅ 已迁移状态逻辑
  ✅ 完成 (15分钟)

[测试失败] 第 3 次重试
  ❌ 错误: Type mismatch in AuthState
  🔄 自动调整方案...
  ✅ 已修复类型问题
  
[实时进度: 62%] [已用时: 35分钟] [当前成本: $3.8]
```

**最终验证**:

```
✓ 所有测试通过 (12/12)
✓ 集成测试通过
✓ 文档已更新
✓ 功能验证通过

🎉 重构完成！

📁 变更文件:
  - lib/auth/auth_provider.dart (重构)
  - lib/auth/auth_state.dart (新增)
  - test/auth/auth_provider_test.dart (新增)
  - docs/auth_module.md (更新)

📉 改进指标:
  - 代码行数: -120 (更简洁)
  - 类型安全: 100% (Provider: 75%)
  - 测试覆盖: 95% (之前: 40%)
```

---

### 示例 3: 学习新技术

```bash
/autonomous "学习 GoRouter 完整用法，包括：
  - 基本路由配置
  - 嵌套路由
  - 路由守卫
  - 深度链接
  - 动画路由
  
要求：
  - 每个概念都要有可运行的示例
  - 创建学习笔记
  - 建立知识卡片"
```

**执行过程**:

```
🎯 目标: 掌握 GoRouter 完整用法

📋 执行计划:
  [1] 收集 GoRouter 官方文档 (crawl4ai.search)
  [2] 学习基本路由配置 (llm.learn + code.practice)
  [3] 学习嵌套路由 (llm.learn + code.practice)
  [4] 学习路由守卫 (llm.learn + code.practice)
  [5] 学习深度链接 (llm.learn + code.practice)
  [6] 学习动画路由 (llm.learn + code.practice)
  [7] 创建综合示例项目 (code.create)
  [8] 生成学习笔记 (llm.summarize)
  [9] 创建知识卡片 (graphiti.remember)

⏱️ 预计时间: 90-120 分钟
💰 预计成本: $6-10
```

**学习过程**:

```
[1/9] 收集 GoRouter 官方文档
  ✅ 找到官方文档
  ✅ 找到 15 篇教程文章
  ✅ 找到 10 个示例项目
  ✅ 完成 (5分钟)

[2/9] 学习基本路由配置
  🔄 阅读文档并实践...
  ✅ 理解路由配置结构
  ✅ 创建示例代码
  ✅ 运行验证通过
  💡 学习要点已记录
  ✅ 完成 (15分钟)

[3/9] 学习嵌套路由
  🔄 学习 ShellRoute 和子路由...
  ✅ 理解嵌套结构
  ✅ 创建底部导航示例
  ✅ 运行验证通过
  💡 学习要点已记录
  ✅ 完成 (12分钟)

[实时进度: 33%] [已用时: 32分钟]
```

**完成输出**:

```
🎉 学习完成！

📚 学习成果:
  ✓ 掌握 5 个核心概念
  ✓ 创建 8 个示例文件
  ✓ 运行验证全部通过
  ✓ 生成完整学习笔记

📁 输出文件:
  learning/gorouter/
  ├── basics/
  │   ├── 01_simple_routing.dart
  │   └── 02_named_routes.dart
  ├── advanced/
  │   ├── 03_nested_routes.dart
  │   ├── 04_route_guards.dart
  │   ├── 05_deep_links.dart
  │   └── 06_animated_routes.dart
  ├── complete_example/
  │   └── app_with_gorouter.dart
  └── notes/
      ├── gorouter_guide.md (学习笔记)
      └── gorouter_cheatsheet.md (速查表)

🧠 知识卡片:
  - 创建了 20+ 张知识卡片
  - 包含概念、示例、最佳实践
  - 已存储到 Graphiti

⏱️ 总用时: 105 分钟
💰 总成本: $7.8
```

---

### 示例 4: 问题解决

```bash
/autonomous "解决项目中所有的 TypeScript 类型错误，目前有 23 个错误"
```

**执行计划**:

```
🎯 目标: 修复所有 TypeScript 类型错误

📋 执行计划:
  [1] 运行类型检查收集错误 (tsc)
  [2] 分析错误原因 (llm.analyze)
  [3] 分类错误类型 (llm.classify)
  [4] 批量修复简单错误 (code.fix)
  [5] 手动修复复杂错误 (code.refactor)
  [6] 验证修复结果 (test.run)
  [7] 提交修复 (git.commit)

⏱️ 预计时间: 30-45 分钟
💰 预计成本: $2-4
```

**执行过程**:

```
[1/7] 运行类型检查收集错误
  ✅ 扫描完成
  ✅ 发现 23 个错误
  ✅ 错误详情已记录
  ✅ 完成 (1分钟)

[2/7] 分析错误原因
  🔄 分析中...
  ✅ 分类结果:
     - 缺失类型注解: 12 个
     - 类型不匹配: 7 个
     - 泛型问题: 3 个
     - 导入问题: 1 个
  ✅ 完成 (3分钟)

[3/7] 分类错误类型
  ✅ 按修复难度分组
  ✅ 识别依赖关系
  ✅ 制定修复顺序
  ✅ 完成 (2分钟)

[4/7] 批量修复简单错误
  🔄 修复缺失类型注解...
  ✅ 已修复 12 个
  🔄 修复导入问题...
  ✅ 已修复 1 个
  ✅ 完成 (8分钟)

[5/7] 手动修复复杂错误
  🔄 修复类型不匹配 (7个)...
  ✅ 已修复 5 个
  ❌ 2 个需要重构
  
  🔄 重构相关代码...
  ✅ 已修复剩余 2 个
  ✅ 完成 (15分钟)

[实时进度: 85%] [已用时: 29分钟]
```

**完成验证**:

```
[6/7] 验证修复结果
  ✅ 运行 tsc --noEmit
  ✅ 0 errors found!
  ✅ 完成 (1分钟)

[7/7] 提交修复
  ✅ Git commit: "fix: resolve all TypeScript type errors"
  ✅ 完成 (1分钟)

🎉 问题已解决！

📊 修复统计:
  ✓ 总错误: 23 个
  ✓ 已修复: 23 个
  ✓ 成功率: 100%

📝 修复详情:
  - 添加类型注解: 12 处
  - 修复类型不匹配: 7 处
  - 修复泛型问题: 3 处
  - 修复导入路径: 1 处

⏱️ 总用时: 31 分钟
💰 总成本: $2.8
```

---

## 💡 最佳实践

### 1. 目标设定原则

#### ✅ 好的目标

```bash
# 具体且可验证
/autonomous "创建用户注册功能，包括表单验证、API 调用、错误处理和单元测试"

# 有明确的交付物
/autonomous "调研 Flutter 状态管理方案对比，生成包含性能测试数据的报告"

# 范围合理
/autonomous "优化首页加载速度，目标 LCP < 2.5s"
```

#### ❌ 不好的目标

```bash
# 过于模糊
/autonomous "改进代码质量"  # 什么是"质量"？

# 范围过大
/autonomous "重构整个项目"  # 太大了

# 无法验证
/autonomous "学习 Flutter"  # 没有明确标准
```

---

### 2. 执行监控技巧

#### 定期检查进度

```bash
# 查看实时状态
/status

# 查看详细日志
/logs --tail 50

# 查看当前任务详情
/current
```

#### 识别异常信号

```
⚠️ 需要关注的信号:
  - 进度长时间停滞
  - 错误率突然上升
  - 成本增长异常
  - 单个任务耗时过长
```

---

### 3. 干预时机

#### 应该干预的情况

```bash
# 1. 执行方向偏离
/pause
/adjust "重新聚焦在性能优化，忽略其他方面"

# 2. 遇到阻塞无法解决
/pause
# 提供额外上下文或资源

# 3. 时间/成本超预算
/stop
# 调整目标范围或预算
```

#### 不应该干预的情况

```
✅ 让系统自动处理:
  - 单个任务失败（会自动重试）
  - 轻微进度延迟（会自适应调整）
  - 资源临时紧张（会优化分配）
```

---

### 4. 结果验证

#### 自动验证

系统会自动检查：
- ✅ 所有任务完成
- ✅ 交付物存在
- ✅ 质量达标
- ✅ 无遗留错误

#### 手动验证

```bash
# 查看输出文件
cat docs/output.md

# 运行测试
npm test

# 检查知识库
opencode graphiti query "今天学到的内容"
```

---

### 5. 知识积累

#### 自动存储

系统自动存储：
- 执行历史
- 学习成果
- 最佳实践
- 错误教训

#### 主动利用

```bash
# 查询历史经验
opencode graphiti query "如何优化 Flutter 性能"

# 查看类似任务
opencode graphiti query "类似的重构任务"

# 获取建议
opencode graphiti query "这个场景的最佳实践"
```

---

## 🎓 进阶技巧

### 1. 组合使用

```bash
# 先调研，后实施
/autonomous "调研最佳的状态管理方案"
# ... 完成后 ...
/autonomous "根据调研结果重构状态管理"
```

### 2. 增量执行

```bash
# 第一阶段: 基础功能
/autonomous "实现用户认证基础功能"

# 第二阶段: 增强功能
/autonomous "为用户认证添加双因素认证"

# 第三阶段: 优化
/autonomous "优化用户认证性能和用户体验"
```

### 3. 协作执行

```bash
# Agent A: 后端
/autonomous "创建用户 API (后端)"

# Agent B: 前端（在另一个会话）
/autonomous "创建用户界面 (前端，使用 Agent A 的 API)"
```

---

## 📚 常见问题

### Q: 执行时间过长怎么办？

**A**: 调整策略

```bash
# 暂停并调整
/pause
/adjust --mode quick  # 切换到快速模式

# 或简化目标
/adjust "聚焦最核心的 3 个功能"
```

---

### Q: 成本超出预期？

**A**: 优化资源使用

```bash
# 查看成本详情
/status --cost

# 调整模型选择
/config set model cheaper_model

# 或调整范围
/adjust "减少示例数量，聚焦核心内容"
```

---

### Q: 中途想修改目标？

**A**: 可以随时调整

```bash
# 添加新要求
/adjust "同时添加性能测试"

# 改变方向
/adjust "改为调研 React 方案（而非 Vue）"

# 缩小范围
/adjust "只实现核心功能，扩展功能后续再说"
```

---

### Q: 如何查看历史执行记录？

**A**: 查询知识库

```bash
# 查看所有执行历史
opencode graphiki query "autonomous execution history"

# 查看特定目标
opencode graphiki query "用户认证重构的执行记录"

# 查看学习成果
opencode graphiki query "今天学到的内容"
```

---

## 🎯 成功案例

### 案例 1: 完整项目搭建

```bash
/autonomous "搭建一个完整的 Flutter 电商应用，包括：
  - 用户认证
  - 商品列表
  - 购物车
  - 订单管理
  - 支付集成
  
要求：MVP 版本，可运行，有基础测试"
```

**结果**: 4 小时完成，包含 80+ 文件，测试覆盖率 75%

---

### 案例 2: 技术债务清理

```bash
/autonomous "清理项目中的所有 TODO 和 FIXME，要么实现要么删除"
```

**结果**: 2.5 小时，处理了 47 个 TODO，实现 35 个，删除 12 个

---

### 案例 3: 文档生成

```bash
/autonomous "为整个项目生成完整的 API 文档和架构文档"
```

**结果**: 3 小时，生成了 150+ 页文档，包含架构图和 API 参考

---

*开始你的自主执行之旅: `/autonomous "你的目标"`*
