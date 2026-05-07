# Graphiti 知识图谱系统详解

## 🎯 Graphiti 是什么？

Graphiti 是一个**知识图谱记忆系统**，为 AI Agent 提供：
- **短期记忆** (Short-term Memory)
- **长期记忆** (Long-term Memory)

---

## 💡 为什么需要 Graphiti？

### 问题：AI Agent 的记忆困境

```
传统 AI Agent 问题：
┌────────────────────────────────────┐
│  第 1 天: 学习了 Flutter 基础       │
│  第 2 天: 完全忘记了昨天的内容      │
│  第 3 天: 重新问相同的问题          │
└────────────────────────────────────┘

原因：
- 对话窗口有限（context window）
- 无法跨会话记忆
- 重复劳动，效率低下
```

### 解决方案：Graphiti 知识图谱

```
使用 Graphiti 后：
┌────────────────────────────────────┐
│  第 1 天: 学习 Flutter 基础         │
│          ↓ 存储到知识图谱            │
│  第 2 天: 记得昨天学过的内容         │
│  第 3 天: 基于已有知识继续深入       │
│  第 N 天: 形成完整的知识体系         │
└────────────────────────────────────┘
```

---

## 🧠 双层记忆架构

### 1. 短期记忆 (Short-term Memory)

**作用**: 会话内的连续性记忆

**功能**:
```
实时捕获会话事件：
├─ 决策记录
├─ 任务进度
├─ 文件编辑
├─ 错误信息
└─ 环境变化

持续压缩为快照：
├─ 优先级分层
├─ 保留重要信息
└─ 注入到每次 LLM 调用

效果：
✓ 即使对话被压缩，也能记住之前的工作
✓ 扩展有效上下文窗口
✓ 避免重复劳动
```

**示例**:

```typescript
// 第 1 步: Agent 决定使用 Riverpod
event: {
  type: "decision",
  content: "选择 Riverpod 作为状态管理方案",
  reason: "更适合复杂状态逻辑",
  timestamp: "2026-05-06 17:30"
}

// 第 2 步: Agent 修改文件
event: {
  type: "file_edit",
  file: "lib/auth/auth_provider.dart",
  changes: "添加了 Riverpod Provider"
}

// 第 3 步: 发生错误
event: {
  type: "error",
  message: "Type mismatch in AuthState",
  resolved: false
}

// 持续压缩成快照（自动）
snapshot: {
  decisions: ["使用 Riverpod"],
  active_task: "重构认证模块",
  recent_errors: ["Type mismatch"],
  progress: "60%"
}

// 每次调用 LLM 前，自动注入这个快照
// Agent 永远知道自己在做什么
```

---

### 2. 长期记忆 (Long-term Memory)

**作用**: 跨会话的持久化知识

**功能**:
```
知识持久化：
├─ 项目事实
├─ 过去决策
├─ 学习偏好
└─ 最佳实践

跨会话检索：
├─ 向量搜索
├─ 图谱查询
└─ 关联推理

知识积累：
├─ 新知识自动添加
├─ 旧知识自动更新
└─ 形成知识网络
```

**示例**:

```typescript
// 会话 1: 学习 GoRouter
graphiti.remember({
  content: "GoRouter 使用 StatefulShellRoute 实现底部导航",
  tags: ["flutter", "navigation", "gorouter"],
  context: {
    project: "molitao_app",
    learned_date: "2026-05-01"
  }
});

// 会话 2 (3 天后): Agent 自动回忆
graphiti.query({
  question: "如何实现底部导航？",
  tags: ["navigation"]
});

// 返回: "GoRouter 使用 StatefulShellRoute..."
// Agent 记得之前学过的内容！
```

---

## 🔗 知识图谱结构

### 节点类型

```
知识图谱中的节点：
┌─────────────────────────────────────┐
│  Concept Node (概念节点)             │
│  - 技术概念                          │
│  - 设计模式                          │
│  - 最佳实践                          │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│  Episode Node (事件节点)             │
│  - 具体事件                          │
│  - 决策过程                          │
│  - 问题解决                          │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│  Entity Node (实体节点)              │
│  - 项目                              │
│  - 文件                              │
│  - 代码模块                          │
└─────────────────────────────────────┘
```

### 关系类型

```
节点之间的关系：
┌────────────────────────────────────┐
│  temporal (时序关系)                │
│  - before / after                   │
│  - during                           │
│  - 事件发生的时间顺序               │
└────────────────────────────────────┘

┌────────────────────────────────────┐
│  causal (因果关系)                  │
│  - causes / caused_by               │
│  - enables / enabled_by             │
│  - 为什么做这个决策                 │
└────────────────────────────────────┘

┌────────────────────────────────────┐
│  associative (关联关系)             │
│  - relates_to                       │
│  - similar_to                       │
│  - part_of                          │
│  - 相关知识点的连接                 │
└────────────────────────────────────┘
```

### 实际图谱示例

```
项目知识图谱片段：

[Flutter Project] ←── belongs_to ──┐
       │                            │
       ├── uses ──→ [Riverpod]      │
       │               │            │
       │               ├── reason ──┤
       │               │            │
       │               ↓            │
       │         [决策记录:          │
       │          "选择 Riverpod     │
       │           因为性能更好"]    │
       │                            │
       ├── implements ──→ [认证模块] │
       │                    │       │
       │                    ├── uses ─┘
       │                    │
       │                    └── has_issue ──→ [类型错误]
       │                                        │
       │                                        └── resolved_by ──→ [修复方案]
       │
       └── learned ──→ [GoRouter 知识]
                          │
                          └── applied_in ──→ [导航实现]
```

---

## 💻 使用示例

### 示例 1: 存储学习成果

```bash
# 在 OpenCode 中学习新技术后
/autonomous "学习 GoRouter 完整用法"
```

**Graphiti 自动存储**:

```typescript
// 知识节点 1
{
  type: "concept",
  content: "GoRouter 使用 GoRoute 定义路由",
  tags: ["flutter", "navigation", "gorouter", "basics"],
  metadata: {
    learned_from: "官方文档",
    confidence: 0.95,
    applicable_projects: ["molitao_app"]
  }
}

// 知识节点 2
{
  type: "pattern",
  content: "StatefulShellRoute 用于底部导航",
  tags: ["flutter", "navigation", "gorouter", "pattern"],
  metadata: {
    use_case: "底部导航",
    benefits: ["保持页面状态", "避免重建"]
  }
}

// 关系
{
  from: "GoRouter basics",
  to: "StatefulShellRoute pattern",
  type: "enables",
  reason: "理解基础后才能应用高级模式"
}
```

---

### 示例 2: 跨会话回忆

```bash
# 3 天后，新的会话
/autonomous "优化应用的导航体验"
```

**Graphiti 自动检索**:

```typescript
// Agent 查询历史知识
const knowledge = await graphiti.query({
  question: "应用当前使用什么导航方案？",
  tags: ["navigation", "molitao_app"]
});

// 返回
{
  answer: "应用使用 GoRouter 作为导航方案",
  related_knowledge: [
    "已学习 StatefulShellRoute 实现底部导航",
    "当前导航配置在 lib/router.dart",
    "有过类型错误修复经验"
  ],
  suggestions: [
    "可以添加路由动画",
    "可以优化深度链接处理"
  ]
}

// Agent 基于历史知识继续工作，而不是从头开始
```

---

### 示例 3: 决策追溯

```bash
# 查询历史决策
/ask "我们为什么选择 Riverpod 而不是 Bloc？"
```

**Graphiti 检索决策记录**:

```typescript
const decision = await graphiti.query({
  question: "为什么选择 Riverpod？",
  tags: ["decision", "architecture"]
});

// 返回完整决策链
{
  decision: "选择 Riverpod 作为状态管理方案",
  date: "2026-04-15",
  reasoning: [
    "性能更好（编译时优化）",
    "代码更简洁（不需要 BuildContext）",
    "测试更容易（不需要 Widget 测试）"
  ],
  alternatives_considered: [
    {
      name: "Bloc",
      rejected_reason: "模板代码过多"
    },
    {
      name: "Provider",
      rejected_reason: "类型安全性不足"
    }
  ],
  consequences: [
    "需要在团队内培训 Riverpod 用法",
    "已有 Provider 代码需要迁移"
  ]
}
```

---

## 🎯 核心价值

### 1. 知识积累

```
传统方式：
┌────────────────────────────────┐
│ 每次都从头开始学习              │
│ 重复犯错                       │
│ 忘记最佳实践                   │
└────────────────────────────────┘

使用 Graphiti：
┌────────────────────────────────┐
│ 知识持续积累                    │
│ 避免重复犯错                   │
│ 构建知识体系                   │
└────────────────────────────────┘
```

### 2. 上下文扩展

```
LLM 原生能力：
- 上下文窗口: ~200K tokens
- 有效范围: 当前会话

配合 Graphiti：
- 短期记忆: 扩展当前会话范围
- 长期记忆: 跨越多个会话
- 有效范围: 数天、数周、数月
```

### 3. 决策连贯性

```
传统方式：
Day 1: 决定使用 Riverpod
Day 2: 忘记了，想换成 Bloc
Day 3: 又忘记，想换回 Provider
（摇摆不定，浪费时间）

使用 Graphiti：
Day 1: 决定使用 Riverpod，记录原因
Day 2: 查询历史决策，继续执行
Day 3: 基于已有决策深入优化
（方向明确，效率提升）
```

### 4. 知识复用

```
学习一次，多次受益：

项目 A:
  ├─ 学习了 Flutter 性能优化技巧
  └─ 存储到 Graphiti

项目 B (新项目):
  ├─ 自动检索项目 A 的经验
  ├─ 应用已验证的优化技巧
  └─ 跳过学习曲线
```

---

## 🔧 技术实现

### 核心数据模型

Graphiti 知识图谱由四种核心节点组成：

```typescript
// 1. EntityNode (实体节点)
class EntityNode {
  name: string;              // 实体名称
  summary: string;           // 实体摘要
  labels: string[];          // 类型标签
  created_at: Date;          // 创建时间
  attributes: object;        // 自定义属性
}

// 2. EpisodicNode (情节节点)
class EpisodicNode {
  source: 'message' | 'json' | 'text';
  content: string;           // 原始数据
  valid_at: Date;           // 创建时间
  entity_edges: string[];   // 引用的实体
}

// 3. EntityEdge (实体关系/事实)
class EntityEdge {
  name: string;              // 关系名称
  fact: string;              // 事实描述
  
  // ⏰ 时序关键字段
  valid_at: Date;           // 何时变为真
  invalid_at: Date;         // 何时不再为真
  reference_time: Date;     // 时间戳
}

// 4. CommunityNode (社区节点)
class CommunityNode {
  summary: string;          // 成员节点摘要
}
```

### 时序关系管理

Graphiti 的核心创新是**时序事实管理**：

```
示例：Alice 的工作经历

2024-01-01: Alice ──[works_at]──▶ Acme Corp
  valid_at: 2024-01-01
  invalid_at: 2025-06-01  ✗ 已失效

2025-06-01: Alice ──[works_at]──▶ TechStart Inc
  valid_at: 2025-06-01
  invalid_at: null        ✓ 当前有效
```

**查询能力**：
- 当前事实：`WHERE invalid_at IS NULL`
- 历史查询：特定时间点的知识状态
- 变化追踪：实体的演变历史

### 数据存储

```
Graphiti 架构：
┌─────────────────────────────────┐
│  Graphiti MCP Server            │
│  (知识图谱服务)                  │
└─────────────────────────────────┘
           ↓
┌─────────────────────────────────┐
│  图数据库后端                    │
│  - Neo4j (企业级)               │
│  - FalkorDB (轻量级)            │
│  - Kuzu (嵌入式)                │
│  - Amazon Neptune (云服务)      │
└─────────────────────────────────┘
           ↓
┌─────────────────────────────────┐
│  Redis (缓存层)                 │
│  - 快速检索                      │
│  - 短期记忆存储                  │
└─────────────────────────────────┘
```

### 查询能力

```typescript
// 1. 向量搜索（语义相似）
const results = await graphiti.search_edges({
  query: "如何优化 Flutter 性能",
  num_results: 10
});

// 2. 图谱查询（关系推理）
const related = await graphiti.search_nodes({
  query: "Riverpod 相关知识",
  group_ids: ["molitao_app"]
});

// 3. 时序查询（历史追踪）
const history = await graphiti.search_edges({
  query: "项目架构决策",
  reference_time: new Date('2025-01-01')  // 特定时间点
});

// 4. 混合检索（向量 + 关键词 + 图）
const hybrid = await graphiti.hybrid_search({
  query: "性能优化技巧",
  methods: ['vector', 'keyword', 'graph']
});
```

### 与传统 RAG 的对比

| 方面 | 传统 RAG | Graphiti |
|------|----------|----------|
| **数据处理** | 批量处理静态文档 | 增量实时更新 |
| **知识结构** | 文档块 + 向量 | 时序上下文图谱 |
| **历史查询** | 有限 | 原生支持 |
| **事实演变** | 不支持 | 自动追踪 |
| **溯源追踪** | 弱 | 每个事实可追溯到原始数据 |

---

---

## 📊 实际效果对比

### 没有 Graphiti

```
会话 1 (第 1 天):
  Agent: 学习了 GoRouter 基础
  结果: 生成文档，会话结束

会话 2 (第 3 天):
  Agent: 需要重新学习 GoRouter
  问题: 忘记了之前的内容
  浪费: 2 小时重复学习

会话 3 (第 7 天):
  Agent: 又忘记 GoRouter
  问题: 再次重新学习
  浪费: 2 小时重复学习

总耗时: 6 小时
知识积累: 0 (没有持久化)
```

### 有 Graphiti

```
会话 1 (第 1 天):
  Agent: 学习了 GoRouter 基础
  Graphiti: 自动存储知识节点
  结果: 文档 + 知识图谱

会话 2 (第 3 天):
  Agent: 检索历史知识
  Graphiti: 返回 GoRouter 知识
  进展: 继续深入高级用法
  节省: 2 小时

会话 3 (第 7 天):
  Agent: 查询已有知识
  Graphiti: 返回完整知识体系
  进展: 应用到新项目
  节省: 2 小时

总耗时: 2 小时
知识积累: 完整的知识图谱
```

---

## 🚀 与 OpenCode 集成

### 自动集成

```bash
# 安装插件后自动启用
npm install opencode-graphiti

# Agent 自动使用 Graphiti
# 无需手动调用
```

### 手动使用

```bash
# 在 OpenCode 中查询知识
/ask "我们之前学过什么导航方案？"

# Agent 会自动：
# 1. 查询 Graphiti
# 2. 返回历史知识
# 3. 基于已有知识回答
```

---

## 💎 总结

### Graphiti 的核心作用

| 功能 | 说明 | 价值 |
|------|------|------|
| **短期记忆** | 会话内连续性 | 避免重复，提高效率 |
| **长期记忆** | 跨会话持久化 | 知识积累，经验复用 |
| **知识图谱** | 关系推理 | 决策追溯，知识关联 |
| **自动存储** | 无感知记忆 | 降低使用门槛 |
| **智能检索** | 语义+图查询 | 快速找到相关知识 |

### 适用场景

✅ **长期项目开发** - 知识持续积累  
✅ **团队协作** - 共享知识库  
✅ **技术学习** - 构建知识体系  
✅ **代码重构** - 记住历史决策  
✅ **问题排查** - 追溯根本原因  

---

**简单来说：Graphiti 让 AI Agent 拥有了真正的"记忆"，可以跨越会话积累知识，像人类一样学习和成长。**
