# 知识精炼工作流

将 Graphiti 中的原始知识精炼为高质量的 Obsidian 笔记。

---

## 🎯 目标

从 Graphiti 的 **100+ 条原始知识** 精炼为 **10-20 条核心笔记**，保证：
- 去重和合并
- 提炼核心要点
- 生成清晰结构
- 创建双向链接

---

## 📋 完整流程

### Phase 1: 知识提取

```typescript
async function extractFromGraphiti(
  projectId: string
): Promise<RawKnowledge> {
  
  // 1. 查询所有有效知识
  const edges = await graphiti.search_edges({
    query: "*",
    group_ids: [projectId],
    filters: {
      invalid_at: null,      // 仍然有效
      confidence: { ">": 0.7 } // 高置信度
    },
    num_results: 200
  });
  
  // 2. 查询实体节点
  const nodes = await graphiti.search_nodes({
    query: "*",
    group_ids: [projectId],
    num_results: 100
  });
  
  // 3. 构建知识图谱
  const knowledgeGraph = buildKnowledgeGraph(edges, nodes);
  
  return {
    edges,
    nodes,
    graph: knowledgeGraph,
    stats: {
      total: edges.length + nodes.length,
      valid: edges.filter(e => e.invalid_at === null).length,
      avgConfidence: calculateAvgConfidence(edges)
    }
  };
}
```

**输出示例**:
```json
{
  "stats": {
    "total": 127,
    "valid": 115,
    "avgConfidence": 0.85
  },
  "categories": {
    "concepts": 45,
    "decisions": 23,
    "patterns": 18,
    "examples": 31,
    "errors": 10
  }
}
```

---

### Phase 2: 知识分类

```typescript
async function classifyKnowledge(
  rawKnowledge: RawKnowledge
): Promise<ClassifiedKnowledge> {
  
  const classified = {
    // 核心概念 - 技术概念和原理
    concepts: [],
    
    // 最佳实践 - 推荐做法
    bestPractices: [],
    
    // 设计决策 - 项目决策记录
    decisions: [],
    
    // 代码示例 - 实际代码片段
    examples: [],
    
    // 问题解决 - 遇到的问题和解决方案
    problemSolving: [],
    
    // 工具使用 - 工具和库的用法
    toolUsage: [],
  };
  
  // 使用 LLM 进行智能分类
  for (const edge of rawKnowledge.edges) {
    const category = await llm.classify({
      content: edge.fact,
      categories: Object.keys(classified),
      context: edge.episodes
    });
    
    classified[category].push({
      id: edge.uuid,
      content: edge.fact,
      confidence: edge.episodes[0].fact.metadata?.confidence || 0.8,
      source: edge.episodes.map(e => e.name),
      valid_at: edge.valid_at,
      tags: extractTags(edge.fact)
    });
  }
  
  return classified;
}
```

**分类规则**:

| 类别 | 关键词 | 示例 |
|------|--------|------|
| 核心概念 | 概念、原理、定义、核心 | "GoRouter 是 Flutter 的声明式路由框架" |
| 最佳实践 | 推荐、应该、最佳、建议 | "应该使用 Riverpod 管理路由状态" |
| 设计决策 | 决定、选择、采用、原因 | "选择 GoRouter 而非 Navigator，因为..." |
| 代码示例 | 示例、代码、实现、用法 | "GoRouter 配置示例..." |
| 问题解决 | 问题、错误、解决、修复 | "遇到路由循环问题，解决方案是..." |
| 工具使用 | 工具、库、包、插件 | "使用 go_router_builder 生成路由" |

---

### Phase 3: 去重和精炼

```typescript
async function refineKnowledge(
  classified: ClassifiedKnowledge
): Promise<RefinedKnowledge> {
  
  // Step 1: 去重
  const deduplicated = await deduplicateKnowledge(classified);
  
  // Step 2: 合并相似概念
  const merged = await mergeSimilarConcepts(deduplicated);
  
  // Step 3: 提炼核心要点
  const corePoints = await extractCorePoints(merged, {
    maxPoints: 20,
    maxLength: 200,  // 每个要点最多 200 字
    minImportance: 0.7
  });
  
  // Step 4: 提取最佳实践
  const bestPractices = await extractBestPractices(merged.bestPractices, {
    maxPractices: 15,
    requireExample: true
  });
  
  // Step 5: 整理代码示例
  const codeExamples = await organizeExamples(merged.examples, {
    maxExamples: 10,
    verifyCode: true
  });
  
  // Step 6: 提取决策记录
  const decisions = await organizeDecisions(merged.decisions);
  
  return {
    corePoints,
    bestPractices,
    codeExamples,
    decisions,
    stats: {
      original: classified.total(),
      afterDedup: deduplicated.total(),
      afterMerge: merged.total(),
      final: corePoints.length + bestPractices.length + codeExamples.length
    }
  };
}
```

#### 去重算法

```typescript
async function deduplicateKnowledge(
  knowledge: ClassifiedKnowledge
): Promise<ClassifiedKnowledge> {
  
  const result = {};
  
  for (const [category, items] of Object.entries(knowledge)) {
    const unique = [];
    
    for (const item of items) {
      // 检查是否已存在相似知识
      const similar = await findSimilar(item, unique, {
        threshold: 0.85,  // 相似度阈值
        method: 'semantic' // 语义相似度
      });
      
      if (similar) {
        // 合并而不是丢弃
        similar.sources.push(...item.sources);
        similar.confidence = Math.max(similar.confidence, item.confidence);
      } else {
        unique.push(item);
      }
    }
    
    result[category] = unique;
  }
  
  return result;
}
```

#### 核心要点提取

```typescript
async function extractCorePoints(
  knowledge: ClassifiedKnowledge,
  options: ExtractOptions
): Promise<CorePoint[]> {
  
  // 使用 LLM 提炼核心
  const prompt = `
从以下知识中提炼核心要点：

要求：
1. 每个要点不超过 2 句话（最多 ${options.maxLength} 字）
2. 去除重复和冗余内容
3. 按重要性排序（高置信度、高频出现优先）
4. 最多保留 ${options.maxPoints} 条
5. 重要性阈值: ${options.minImportance}

知识内容：
${knowledge.concepts.map(c => `- ${c.content}`).join('\n')}

输出格式：
1. [核心要点]
2. [核心要点]
...
`;

  const response = await llm.generate(prompt);
  
  // 解析并验证
  return parseAndValidate(response, knowledge);
}
```

**精炼效果**:
```
原始知识: 127 条
↓ 去重
去重后: 98 条 (去除 29 条重复)
↓ 合并
合并后: 85 条 (合并 13 条相似)
↓ 提炼
核心要点: 18 条 (提炼精华)
最佳实践: 12 个
代码示例: 8 个
```

---

### Phase 4: 生成 Obsidian 笔记

```typescript
async function generateObsidianNotes(
  refined: RefinedKnowledge,
  projectId: string
): Promise<ObsidianNote[]> {
  
  const notes = [];
  
  // 1. 生成主笔记
  const mainNote = await generateMainNote(refined, projectId);
  notes.push(mainNote);
  
  // 2. 生成分类笔记
  if (refined.bestPractices.length > 5) {
    const bestPracticesNote = await generateBestPracticesNote(refined);
    notes.push(bestPracticesNote);
  }
  
  if (refined.codeExamples.length > 3) {
    const examplesNote = await generateExamplesNote(refined);
    notes.push(examplesNote);
  }
  
  // 3. 生成决策记录笔记
  if (refined.decisions.length > 0) {
    const decisionsNote = await generateDecisionsNote(refined);
    notes.push(decisionsNote);
  }
  
  return notes;
}
```

#### 主笔记模板

```markdown
---
created: 2026-05-06T10:30:00Z
updated: 2026-05-06T10:30:00Z
tags: [synced, from-graphiti, flutter, gorouter]
source: graphiti
graphiti_group: molitao_app
confidence: 0.85
knowledge_count: 127
refined_count: 18
---

# GoRouter 完整指南

> 同步自 Graphiti | 原始知识: 127 条 | 精炼后: 18 条

## 核心概念

### 1. 声明式路由

GoRouter 使用声明式配置定义路由，替代传统的命令式导航。

```dart
final router = GoRouter(
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => HomePage(),
    ),
  ],
);
```

### 2. 路由状态管理

使用 Riverpod 管理路由状态，实现响应式导航。

### 3. 深度链接支持

支持 Web 深度链接和原生 deep link。

## 最佳实践

1. **使用 go_router_builder** - 自动生成路由配置，减少手动错误
2. **路由守卫** - 在 redirect 中实现权限检查
3. **状态持久化** - 使用 Riverpod 保持路由状态
4. **错误处理** - 配置 errorBuilder 处理路由错误

## 相关链接

- [[Flutter 导航]]
- [[Riverpod 状态管理]]
- [[路由守卫实现]]

## 元信息

- 知识来源: Graphiti (127 条原始知识)
- 同步时间: 2026-05-06 10:30
- 平均置信度: 0.85
- 精炼比例: 14.2% (18/127)

---
*此笔记由 Graphiti 自动同步生成*
```

---

### Phase 5: 创建双向链接

```typescript
async function createBidirectionalLinks(
  notes: ObsidianNote[],
  vaultPath: string
): Promise<LinkResult> {
  
  const result = {
    created: [],
    updated: [],
    failed: []
  };
  
  for (const note of notes) {
    // 1. 提取笔记中的链接
    const links = extractWikiLinks(note.content);
    
    // 2. 为每个链接创建反向链接
    for (const link of links) {
      const targetPath = findNotePath(link, vaultPath);
      
      if (targetPath) {
        // 在目标笔记中添加反向链接
        await addBacklink(targetPath, {
          source: note.title,
          path: note.path,
          context: extractContext(note, link)
        });
        
        result.created.push({
          from: note.path,
          to: targetPath,
          type: 'bidirectional'
        });
      }
    }
    
    // 3. 更新 Obsidian 图谱缓存
    await updateGraphCache(vaultPath, note);
  }
  
  return result;
}
```

**链接类型**:

| 类型 | 说明 | 示例 |
|------|------|------|
| 概念链接 | 相关概念之间的链接 | `[[GoRouter]] → [[Flutter 导航]]` |
| 实践链接 | 概念到最佳实践的链接 | `[[GoRouter]] → [[路由守卫实现]]` |
| 项目链接 | 同一项目知识的链接 | `[[molitao_app]] → [[认证模块]]` |

---

### Phase 6: 写入 Obsidian

```typescript
async function writeToObsidian(
  notes: ObsidianNote[],
  vaultPath: string
): Promise<WriteResult> {
  
  const result = {
    created: [],
    updated: [],
    skipped: [],
    backedUp: []
  };
  
  for (const note of notes) {
    const targetPath = path.join(vaultPath, note.path);
    
    // 1. 检查是否已存在
    if (fs.existsSync(targetPath)) {
      // 2. 备份现有笔记
      const backupPath = await backupNote(targetPath);
      result.backedUp.push(backupPath);
      
      // 3. 合并内容
      const existing = await readNote(targetPath);
      const merged = await mergeNotes(existing, note, {
        strategy: 'prefer_newer',
        preserveHumanEdits: true
      });
      
      // 4. 写入合并后的内容
      await writeNote(targetPath, merged);
      result.updated.push(note.path);
      
    } else {
      // 创建新笔记
      await writeNote(targetPath, note);
      result.created.push(note.path);
    }
    
    // 5. 更新文件索引
    await updateFileIndex(vaultPath, note);
  }
  
  return result;
}
```

**合并策略**:

```typescript
async function mergeNotes(
  existing: ObsidianNote,
  incoming: ObsidianNote,
  options: MergeOptions
): Promise<ObsidianNote> {
  
  // 保留人类的编辑
  const humanEdits = extractHumanEdits(existing);
  
  // 合并 frontmatter
  const frontmatter = {
    ...existing.frontmatter,
    ...incoming.frontmatter,
    updated: new Date().toISOString(),
    human_edits: humanEdits.length
  };
  
  // 合并内容
  const content = `
${incoming.content}

## 人类补充

${humanEdits.map(e => e.content).join('\n\n')}
`;
  
  return { frontmatter, content };
}
```

---

### Phase 7: 生成同步报告

```typescript
async function generateSyncReport(
  result: SyncResult
): Promise<string> {
  
  return `
# 知识同步报告

**同步时间**: ${result.timestamp}

## 📊 统计信息

### Graphiti 知识
- 原始知识节点: ${result.raw.total}
- 有效知识: ${result.raw.valid}
- 平均置信度: ${result.raw.avgConfidence.toFixed(2)}

### 精炼结果
- 去重后知识: ${result.refined.afterDedup}
- 合并后知识: ${result.refined.afterMerge}
- 核心概念: ${result.refined.corePoints}
- 最佳实践: ${result.refined.bestPractices}
- 代码示例: ${result.refined.codeExamples}

### Obsidian 输出
- 新建笔记: ${result.write.created.length}
- 更新笔记: ${result.write.updated.length}
- 备份文件: ${result.write.backedUp.length}
- 创建链接: ${result.links.created.length}

## 📁 创建的笔记

${result.write.created.map(p => `- \`${p}\``).join('\n')}

## 📝 更新的笔记

${result.write.updated.map(p => `- \`${p}\``).join('\n')}

## 🔗 知识图谱

- 新增节点: ${result.graph.nodesCreated}
- 新增链接: ${result.graph.linksCreated}
- 更新节点: ${result.graph.nodesUpdated}

## ✅ 同步完成

知识已成功从 Graphiti 同步到 Obsidian！

精炼比例: ${(result.refined.total / result.raw.total * 100).toFixed(1)}%
质量提升: 原始 ${result.raw.avgConfidence.toFixed(2)} → 精炼后 ${(result.refined.avgConfidence).toFixed(2)}
`;
}
```

---

## 🚀 使用示例

### 完整执行

```bash
# 执行完整同步流程
/sync-to-obsidian

# 输出：
✅ 知识同步完成！

📊 统计信息：
  - Graphiti 原始知识: 127 条
  - 有效知识: 115 条
  - 去重后: 98 条
  - 精炼后核心要点: 18 条

📁 Obsidian 输出：
  - 新建: 2 个笔记
  - 更新: 3 个笔记
  - 创建链接: 15 个

🔗 知识图谱：
  - 新增节点: 5
  - 新增链接: 15

查看报告: .opencode/reports/sync_2026-05-06.md
```

### 增量同步

```bash
# 只同步最近 24 小时的新知识
/sync-to-obsidian --incremental --since="24h"

# 输出：
✅ 增量同步完成！

📊 本次同步：
  - 新增知识: 23 条
  - 精炼后: 5 条核心要点
  - 更新笔记: 2 个
```

### 指定分类同步

```bash
# 只同步最佳实践和代码示例
/sync-to-obsidian --categories="bestPractices,examples"

# 输出：
✅ 分类同步完成！

📊 同步内容：
  - 最佳实践: 12 个
  - 代码示例: 8 个
  - 创建笔记: 1 个
```

---

## 🎯 质量保证

### 自动检查

```typescript
const qualityChecks = {
  // 1. 最小知识量
  minKnowledge: 5,
  
  // 2. 去重效果
  deduplicationRate: { min: 0.1, max: 0.5 }, // 去重 10%-50%
  
  // 3. 精炼比例
  refinementRatio: { min: 0.1, max: 0.3 }, // 精炼到 10%-30%
  
  // 4. 核心概念数
  coreConcepts: { min: 5, max: 20 },
  
  // 5. 每个概念长度
  conceptLength: { min: 50, max: 200 },
  
  // 6. 必须包含示例
  requireExamples: true,
  
  // 7. 链接完整性
  linkIntegrity: true, // 所有链接必须有效
  
  // 8. 置信度阈值
  minConfidence: 0.7
};
```

### 验证流程

```typescript
async function validateSyncResult(
  result: SyncResult
): Promise<ValidationResult> {
  
  const issues = [];
  
  // 检查精炼比例
  const ratio = result.refined.total / result.raw.total;
  if (ratio < 0.1 || ratio > 0.3) {
    issues.push(`精炼比例 ${ratio.toFixed(2)} 不在合理范围内 (0.1-0.3)`);
  }
  
  // 检查链接完整性
  const brokenLinks = await checkBrokenLinks(result.links.created);
  if (brokenLinks.length > 0) {
    issues.push(`发现 ${brokenLinks.length} 个无效链接`);
  }
  
  // 检查笔记质量
  for (const note of result.notes) {
    const quality = await assessNoteQuality(note);
    if (quality.score < 0.7) {
      issues.push(`笔记 "${note.title}" 质量分数过低: ${quality.score}`);
    }
  }
  
  return {
    valid: issues.length === 0,
    issues,
    score: calculateOverallScore(result)
  };
}
```

---

## 📈 性能优化

### 批量处理

```typescript
// 并行处理多个分类
await Promise.all([
  processCategory('concepts', knowledge.concepts),
  processCategory('bestPractices', knowledge.bestPractices),
  processCategory('examples', knowledge.examples),
]);
```

### 缓存策略

```typescript
const cache = {
  // 缓存相似度计算结果
  similarity: new LRUCache({ max: 1000 }),
  
  // 缓存 LLM 响应
  llmResponses: new LRUCache({ max: 100 }),
  
  // 缓存笔记内容
  noteContent: new LRUCache({ max: 50 }),
};
```

---

## 🔄 下一步

1. **测试工作流** - 运行 `/sync-to-obsidian` 验证完整流程
2. **调整参数** - 根据结果优化精炼参数
3. **设置自动化** - 配置定时同步
4. **反馈循环** - 实现 `/sync-to-graphiti`

---

*这个工作流确保知识从 Graphiti 高质量地流转到 Obsidian！*
