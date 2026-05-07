# 知识循环系统架构

实现 **Graphiti ⇄ Obsidian** 双向知识流动的自动化系统。

---

## 🔄 循环流程

```
┌─────────────────────────────────────────────────────────────┐
│                  知识生命周期循环                            │
└─────────────────────────────────────────────────────────────┘

     ┌──────────────────────────────────────────────┐
     │                                              │
     ↓                                              │
┌─────────────┐         ┌─────────────┐          │
│   学习阶段   │         │   积累阶段   │          │
└─────────────┘         └─────────────┘          │
     │                         │                   │
     │ 用户学习新知识           │ 自动存储到 Graphiti │
     │ AI 辅助探索              │ 实时、原始、粗粒度  │
     ↓                         ↓                   │
┌─────────────────────────────────────────────┐   │
│           Graphiti (工作记忆)                │   │
│  - 原始知识点（100+ 条）                      │   │
│  - 时序追踪                                  │   │
│  - 毫秒级检索                                │   │
└─────────────────────────────────────────────┘   │
     │                                              │
     │ /sync-to-obsidian                            │
     │ 精炼、去重、归纳                              │
     ↓                                              │
┌─────────────────────────────────────────────┐   │
│           Obsidian (知识宝库)                │   │
│  - 精炼笔记（10-20 条核心）                   │   │
│  - 高质量、可读                              │   │
│  - 双向链接、图谱                            │   │
└─────────────────────────────────────────────┘   │
     │                                              │
     │ 人工阅读、补充、实践                          │
     │ 添加洞察、完善示例                            │
     ↓                                              │
┌─────────────────────────────────────────────┐   │
│           人类洞察                            │   │
│  - 实践验证结果                              │   │
│  - 补充说明                                  │   │
│  - 关联知识                                  │   │
└─────────────────────────────────────────────┘   │
     │                                              │
     │ /sync-to-graphiti                            │
     │ 反馈人类智慧                                  │
     └──────────────────────────────────────────────┘
```

---

## 🎯 触发时机

### 触发方式

#### 1. 自主执行后自动触发

```yaml
# 在 OpenCode 会话中，完成任务后自动同步
autonomous_execution:
  on_complete:
    - action: sync_to_obsidian
      condition: "knowledge_count > 10"
      # 任务完成后自动精炼和同步
```

#### 2. 手动触发（推荐）

```bash
# 同步到 Obsidian（精炼知识）
/sync-to-obsidian

# 反馈到 Graphiti（人类洞察）  
/sync-to-graphiti

# 完整双向同步
/knowledge-sync --full
```

#### 3. 集成到工作流

```bash
# 在学习任务完成后自动同步
/autonomous "学习 GoRouter 用法"
# ↓ 任务完成
# ↓ 自动触发 /sync-to-obsidian

# 或显式调用
/autonomous "学习 GoRouter 用法" && /sync-to-obsidian
```

---

## 📦 核心组件

### 1. Graphiti → Obsidian 同步器

**职责**：精炼和同步知识

```typescript
interface KnowledgeSyncer {
  // 提取核心知识
  extractCoreKnowledge(
    rawKnowledge: GraphitiKnowledge[]
  ): RefinedKnowledge[];
  
  // 去重和合并
  deduplicate(
    newKnowledge: RefinedKnowledge[],
    existingNotes: ObsidianNote[]
  ): DeduplicatedResult;
  
  // 生成 Obsidian 格式
  generateMarkdown(
    knowledge: RefinedKnowledge
  ): MarkdownContent;
  
  // 创建链接
  createLinks(
    note: MarkdownContent,
    existingNotes: ObsidianNote[]
  ): LinkedNote;
  
  // 同步到 Obsidian
  syncToObsidian(
    notes: LinkedNote[]
  ): SyncResult;
}
```

---

### 2. Obsidian → Graphiti 反馈器

**职责**：将人类洞察反馈给 AI

```typescript
interface FeedbackSyncer {
  // 检测人类修改
  detectHumanEdits(
    notes: ObsidianNote[],
    since: Date
  ): HumanEdit[];
  
  // 提取洞察
  extractInsights(
    edits: HumanEdit[]
  ): Insight[];
  
  // 更新 Graphiti
  updateGraphiti(
    insights: Insight[]
  ): UpdateResult;
  
  // 标记验证状态
  markVerified(
    knowledgeId: string,
    verificationResult: VerificationResult
  ): void;
}
```

---

### 3. 知识精炼器

**职责**：从原始知识中提炼精华

```typescript
interface KnowledgeRefiner {
  // 识别核心概念
  identifyCoreConcepts(
    rawKnowledge: GraphitiKnowledge[]
  ): Concept[];
  
  // 去重
  removeDuplicates(
    concepts: Concept[]
  ): Concept[];
  
  // 归纳总结
  summarize(
    concepts: Concept[]
  ): Summary;
  
  // 提取示例
  extractExamples(
    rawKnowledge: GraphitiKnowledge[]
  ): Example[];
  
  // 生成最佳实践
  generateBestPractices(
    knowledge: GraphitiKnowledge[]
  ): BestPractice[];
}
```

---

## 🛠️ 实现细节

### 阶段 1: 知识提取

```typescript
async function extractFromGraphiti(
  groupId: string,
  options: ExtractOptions
): Promise<ExtractedKnowledge> {
  
  // 1. 查询 Graphiti 知识
  const rawKnowledge = await graphiti.search_edges({
    query: "*",
    group_ids: [groupId],
    num_results: 100
  });
  
  // 2. 过滤有效知识
  const validKnowledge = rawKnowledge.filter(k => 
    k.invalid_at === null &&  // 仍然有效
    k.confidence > 0.7        // 置信度够高
  );
  
  // 3. 分类知识
  const classified = {
    concepts: [],      // 核心概念
    decisions: [],     // 决策记录
    patterns: [],      // 设计模式
    examples: [],      // 代码示例
    errors: [],        // 错误和解决
  };
  
  validKnowledge.forEach(k => {
    if (k.tags.includes('concept')) classified.concepts.push(k);
    else if (k.tags.includes('decision')) classified.decisions.push(k);
    else if (k.tags.includes('pattern')) classified.patterns.push(k);
    // ...
  });
  
  return classified;
}
```

---

### 阶段 2: 知识精炼

```typescript
async function refineKnowledge(
  extracted: ExtractedKnowledge
): Promise<RefinedKnowledge> {
  
  // 1. 合并重复概念
  const mergedConcepts = await mergeDuplicates(
    extracted.concepts
  );
  
  // 2. 归纳核心要点
  const corePoints = await llm.summarize({
    content: mergedConcepts.map(c => c.fact).join('\n'),
    prompt: `
      从以下知识中提炼核心要点：
      1. 每个要点不超过 2 句话
      2. 去除重复内容
      3. 按重要性排序
      4. 最多保留 15 条
    `
  });
  
  // 3. 提取最佳实践
  const bestPractices = await extractBestPractices(
    extracted.patterns
  );
  
  // 4. 整理示例代码
  const codeExamples = await organizeExamples(
    extracted.examples
  );
  
  return {
    corePoints,
    bestPractices,
    codeExamples,
    decisions: extracted.decisions,
  };
}
```

---

### 阶段 3: 生成 Obsidian 笔记

```typescript
async function generateObsidianNote(
  refined: RefinedKnowledge,
  template: NoteTemplate
): Promise<MarkdownNote> {
  
  const frontmatter = {
    created: new Date().toISOString(),
    updated: new Date().toISOString(),
    tags: ['synced', 'from-graphiti'],
    source: 'graphiti',
    graphiti_group: refined.groupId,
  };
  
  const content = `
# ${refined.title}

## 核心概念

${refined.corePoints.map((p, i) => `${i+1}. ${p}`).join('\n')}

## 最佳实践

${refined.bestPractices.map(p => `
### ${p.title}

${p.description}

\`\`\`dart
${p.code}
\`\`\`
`).join('\n')}

## 设计决策

${refined.decisions.map(d => `
- **${d.question}**: ${d.answer}
  - 原因: ${d.reason}
  - 时间: ${d.valid_at}
`).join('\n')}

## 相关链接

${refined.relatedTopics.map(t => `- [[${t}]]`).join('\n')}

---
*同步自 Graphiti @ ${new Date().toLocaleString()}*
`;
  
  return {
    path: `knowledge/${refined.category}/${refined.title}.md`,
    frontmatter,
    content,
  };
}
```

---

### 阶段 4: 同步到 Obsidian

```typescript
async function syncToObsidian(
  notes: MarkdownNote[]
): Promise<SyncResult> {
  
  const vault = process.env.OBSIDIAN_VAULT_PATH;
  const results = [];
  
  for (const note of notes) {
    const targetPath = path.join(vault, note.path);
    
    // 检查是否已存在
    if (fs.existsSync(targetPath)) {
      // 合并而不是覆盖
      const existing = await readNote(targetPath);
      const merged = await mergeNotes(existing, note);
      await writeNote(targetPath, merged);
      results.push({ path: note.path, action: 'updated' });
    } else {
      // 创建新笔记
      await writeNote(targetPath, note);
      results.push({ path: note.path, action: 'created' });
    }
    
    // 创建双向链接
    await createBidirectionalLinks(note, vault);
  }
  
  return {
    total: notes.length,
    created: results.filter(r => r.action === 'created').length,
    updated: results.filter(r => r.action === 'updated').length,
  };
}
```

---

### 阶段 5: 反馈到 Graphiti

```typescript
async function syncToGraphiti(
  humanEdits: HumanEdit[]
): Promise<FeedbackResult> {
  
  for (const edit of humanEdits) {
    // 1. 检测编辑类型
    const editType = detectEditType(edit);
    
    switch (editType) {
      case 'validation':
        // 人类验证了某个知识点
        await graphiti.update_edge({
          edge_id: edit.relatedGraphitiId,
          metadata: {
            human_verified: true,
            verified_at: edit.timestamp,
            verified_by: 'human',
          }
        });
        break;
        
      case 'addition':
        // 人类添加了新洞察
        await graphiti.add_episode({
          name: `Human Insight: ${edit.title}`,
          episode_body: edit.content,
          source: 'human',
          group_id: edit.groupId,
        });
        break;
        
      case 'correction':
        // 人类修正了错误
        await graphiti.invalidate_edge({
          edge_id: edit.relatedGraphitiId,
          reason: edit.correctionReason,
        });
        await graphiti.add_episode({
          name: `Correction: ${edit.title}`,
          episode_body: edit.correctContent,
          source: 'human',
        });
        break;
    }
  }
}
```

---

## 🎨 使用示例

### 示例 1: 学习新知识后自动同步

```bash
# 用户学习新知识
/autonomous "学习 GoRouter 完整用法"

# AI 自动：
# 1. 搜索资料
# 2. 存储到 Graphiti (100+ 知识点)
# 3. 任务完成后自动触发：
/sync-to-obsidian --auto

# 系统自动：
# 1. 从 Graphiti 提取知识
# 2. 精炼为 15 条核心要点
# 3. 生成 Obsidian 笔记
# 4. 创建双向链接
# 5. 推送到 Obsidian vault
```

**输出**:
```
✅ 知识同步完成！

📊 统计：
  - Graphiti 原始知识: 127 条
  - 精炼后核心要点: 18 条
  - 最佳实践: 12 个
  - 代码示例: 8 个

📁 创建的笔记：
  - ~/Documents/Obsidian/knowledge/flutter/GoRouter.md
  - ~/Documents/Obsidian/knowledge/flutter/GoRouter-Examples.md

🔗 创建的链接：
  - GoRouter → Flutter 导航
  - GoRouter → Riverpod 集成
  - GoRouter → 路由守卫
```

---

### 示例 2: 人类编辑后反馈

```bash
# 1. 人类在 Obsidian 中编辑笔记
#    添加了实践经验和注意事项

# 2. 下次会话开始时，AI 自动检测
/sync-to-graphiti

# 系统自动：
# 1. 检测人类的编辑
# 2. 提取新增的洞察
# 3. 反馈到 Graphiti
# 4. 标记验证状态
```

**输出**:
```
✅ 反馈同步完成！

📝 检测到人类编辑：
  - GoRouter.md: 添加了 3 个实践注意事项
  - Riverpod.md: 更新了最佳实践示例

🧠 反馈到 Graphiti：
  - 新增洞察: 5 条
  - 验证知识点: 12 条
  - 修正错误: 1 条

📊 更新结果：
  - Graphiti 知识准确度: 85% → 92%
  - 人类验证标记: +12
```

---

## 🔄 完整工作流

### 日常学习流程

```bash
# 早晨：开始学习
/autonomous "深入理解 Flutter 动画系统"

# AI 工作中：
# - 搜索资料、学习知识
# - 实时存储到 Graphiti
# - 遇到问题、解决问题

# 晚上：自动同步
# 18:00 - 定时任务触发
/sync-to-obsidian --incremental

# 输出：
# ✅ 同步了今天学到的 23 个知识点
# ✅ 生成了 2 篇精炼笔记
# ✅ 创建了 8 个双向链接

# 晚上：人工阅读
# 在 Obsidian 中：
# - 阅读笔记
# - 添加实践心得
# - 完善示例代码

# 次日：反馈循环
/sync-to-graphiti

# AI 记忆更新：
# ✅ 同步了 5 条人类洞察
# ✅ 标记了 8 个知识点为已验证
```

---

### 项目知识管理流程

```bash
# 开发中：实时记录
/autonomous "重构用户认证模块"

# Graphiti 自动记录：
# - 修改了哪些文件
# - 遇到了什么问题
# - 如何解决的
# - 做了哪些决策

# 每周：知识沉淀
/sync-to-obsidian --weekly

# 输出：
# ✅ 本周决策记录: 8 条
# ✅ 解决的关键问题: 12 个
# ✅ 生成的最佳实践: 5 个

# Obsidian 生成：
# - docs/weekly/2026-W18.md
# - knowledge/auth/Riverpod迁移实践.md

# 团队：知识共享
# 其他成员在 Obsidian 中：
# - 查看决策记录
# - 学习最佳实践
# - 添加补充说明

# 反馈：持续改进
/sync-to-graphiti

# AI 学习：
# ✅ 团队补充的知识
# ✅ 实践验证结果
# ✅ 改进建议
```

---

## 📊 监控和报告

### 同步状态

```bash
/knowledge-status

# 输出：
📊 知识循环系统状态

Graphiti:
  - 总知识节点: 1,234
  - 本周新增: 127
  - 已验证: 456 (37%)
  - 平均置信度: 0.82

Obsidian:
  - 总笔记数: 89
  - 本周新增: 12
  - 同步知识: 56
  - 人类编辑: 23

同步状态:
  - 最后同步: 2026-05-06 18:00
  - 待同步知识: 34
  - 待反馈编辑: 8

健康度: 🟢 良好
```

---

## 🎯 最佳实践

### 1. 同步时机

**推荐做法**：
- ✅ 学习任务完成后立即同步
- ✅ 积累足够知识后手动触发（> 10 条）
- ✅ 项目里程碑完成后完整同步
- ✅ 发现重要洞察时主动反馈

**避免做法**：
- ❌ 过于频繁的同步（知识量太少）
- ❌ 长时间不同步（知识过时）
- ❌ 只单向同步（忽略反馈循环）

```bash
# 好的实践
/autonomous "学习 Flutter 动画系统"
# ↓ 完成后自动积累了 50+ 条知识
/sync-to-obsidian  # 手动触发精炼

# 不好的实践
/sync-to-obsidian  # 知识量太少，精炼效果差
```

---

### 2. 知识质量保证

```typescript
// 精炼时进行质量检查
const qualityChecks = {
  // 最小知识量
  minKnowledge: 5,
  
  // 去重阈值
  similarityThreshold: 0.85,
  
  // 保留核心概念数
  maxCoreConcepts: 20,
  
  // 每个概念最大长度
  maxConceptLength: 200,
  
  // 必须包含示例
  requireExamples: true,
};
```

---

### 3. 避免信息过载

```typescript
// 智能过滤策略
const filterStrategy = {
  // 只同步高置信度知识
  minConfidence: 0.7,
  
  // 只同步最近的知识
  maxAge: "7 days",
  
  // 过滤低价值知识
  excludeTags: ["trivial", "duplicate", "outdated"],
  
  // 优先级排序
  prioritizeBy: ["importance", "recency", "verification"],
};
```

---

## 🚀 下一步

1. **使用同步 Skills**
   - `/sync-to-obsidian` - 精炼知识到 Obsidian
   - `/sync-to-graphiti` - 反馈人类洞察

2. **集成到工作流**
   - 学习任务完成后触发同步
   - 项目里程碑后完整同步
   - 发现洞察时主动反馈

3. **开始使用**
   ```bash
   # 学习新知识
   /autonomous "学习 GoRouter 完整用法"
   
   # 精炼沉淀
   /sync-to-obsidian
   
   # 人工阅读 Obsidian 笔记后反馈
   /sync-to-graphiti
   ```

---

*这个系统让你的知识在 AI 和人类之间流动，既保证积累速度，又保证知识质量！*
