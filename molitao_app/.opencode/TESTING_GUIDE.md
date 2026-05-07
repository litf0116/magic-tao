# 知识循环系统测试指南

本文档说明如何测试 Graphiti ⇄ Obsidian 知识循环系统。

---

## 📋 前置条件

### 1. Graphiti 配置

Graphiti 需要图数据库支持。目前支持的数据库：

- **Neo4j** (推荐生产环境)
- **FalkorDB** (轻量级，适合开发)
- **Redis** (快速缓存)

**配置步骤**：

```bash
# 1. 安装 FalkorDB (最简单的选择)
docker run -d -p 6379:6379 falkordb/falkordb:latest

# 2. 或安装 Neo4j
docker run -d -p 7474:7474 -p 7687:7687 neo4j:latest

# 3. 在 opencode.json 中配置数据库连接
# (参见下方配置示例)
```

### 2. OpenCode 配置

在 `~/.config/opencode/opencode.json` 中添加 Graphiti 配置：

```json
{
  "plugins": [
    "opencode-graphiti@latest"
  ],
  "graphiti": {
    "database": {
      "type": "falkordb",  // 或 "neo4j"
      "host": "localhost",
      "port": 6379,  // Neo4j: 7687
      "database": "graphiti"
    },
    "embedding": {
      "model": "text-embedding-3-small",
      "provider": "openai"
    }
  }
}
```

---

## 🧪 测试步骤

### 步骤 1: 验证 Graphiti 安装

```bash
# 检查 Graphiti 插件
opencode plugins list | grep graphiti

# 测试数据库连接
opencode graphiti test-connection

# 应该看到：
# ✅ Graphiti connected to FalkorDB at localhost:6379
```

### 步骤 2: 学习并积累知识

```bash
# 使用 /autonomous 学习新知识
/autonomous "学习 GoRouter 基础用法"

# AI 会自动：
# 1. 搜索资料
# 2. 提取知识点
# 3. 存储到 Graphiti (自动)
```

**验证 Graphiti 中有知识**：

```bash
# 查询 Graphiti 中的知识
opencode graphiti query "GoRouter"

# 应该返回类似：
# Found 23 knowledge nodes:
# - GoRouter is a declarative routing library
# - GoRoute configuration pattern
# - Route guards implementation
# ...
```

### 步骤 3: 同步到 Obsidian

```bash
# 执行知识精炼和同步
/sync-to-obsidian

# 系统会：
# 1. 从 Graphiti 提取知识
# 2. 分类和去重
# 3. 精炼核心要点
# 4. 生成 Markdown 笔记
# 5. 创建双向链接
# 6. 写入 Obsidian vault
```

**预期输出**：

```
✅ 知识同步完成！

📊 统计信息：
  - Graphiti 原始知识: 23 条
  - 有效知识: 21 条
  - 去重后: 18 条
  - 精炼后核心要点: 8 条

📁 Obsidian 输出：
  - 新建笔记: 2 个
    - concepts/GoRouter.md
    - concepts/GoRouter-Examples.md
  - 创建链接: 12 个

🔗 知识图谱：
  - 新增节点: 3
  - 新增链接: 12
```

**验证 Obsidian 中的笔记**：

```bash
# 检查笔记是否创建
ls ~/Documents/Obsidian/concepts/ | grep -i gorouter

# 应该看到：
# GoRouter.md
# GoRouter-Examples.md
```

### 步骤 4: 人工编辑和补充

在 Obsidian 中：
1. 打开 `GoRouter.md`
2. 阅读精炼后的知识
3. 添加个人实践心得
4. 完善示例代码
5. 添加相关链接

### 步骤 5: 反馈到 Graphiti

```bash
# 将人类洞察同步回 Graphiti
/sync-to-graphiti

# 系统会：
# 1. 检测 Obsidian 中的人工编辑
# 2. 提取人类洞察
# 3. 更新 Graphiti 中的知识
# 4. 标记验证状态
```

**预期输出**：

```
✅ 反馈同步完成！

📝 检测到人类编辑：
  - GoRouter.md: 添加了 2 个实践注意事项
  - GoRouter-Examples.md: 更新了示例代码

🧠 反馈到 Graphiti：
  - 新增洞察: 3 条
  - 验证知识点: 8 条
  - 修正错误: 1 条

📊 更新结果：
  - Graphiti 知识准确度: 82% → 91%
  - 人类验证标记: +8
```

### 步骤 6: 验证循环完成

```bash
# 再次查询 Graphiti，应该包含人类反馈
opencode graphiti query "GoRouter 实践经验"

# 应该看到：
# Found 5 validated knowledge:
# - [VERIFIED] Avoid using relative routes in nested navigators
# - [VERIFIED] Always dispose controllers in route callbacks
# - [HUMAN_INSIGHT] Deep linking requires special platform config
# ...
```

---

## ✅ 测试检查清单

完成以下测试以验证系统正常工作：

- [ ] Graphiti 数据库连接成功
- [ ] `/autonomous` 学习任务能自动存储知识到 Graphiti
- [ ] Graphiti 查询返回存储的知识
- [ ] `/sync-to-obsidian` 能成功执行
- [ ] Obsidian 中创建了精炼的笔记
- [ ] 笔记包含正确的 frontmatter 和 wikilinks
- [ ] 人工编辑 Obsidian 笔记后保存
- [ ] `/sync-to-graphiti` 能检测到人工编辑
- [ ] Graphiti 更新包含人类洞察
- [ ] 知识准确度提升

---

## 🐛 常见问题

### Q1: Graphiti 连接失败

**症状**：
```
❌ Failed to connect to FalkorDB: Connection refused
```

**解决方案**：
```bash
# 检查数据库是否运行
docker ps | grep falkordb

# 重启数据库
docker restart <falkordb-container-id>

# 或使用 Neo4j
docker run -d -p 7474:7474 -p 7687:7687 neo4j:latest
```

### Q2: 同步后 Obsidian 中没有笔记

**可能原因**：
1. `OBSIDIAN_VAULT_PATH` 未配置
2. 权限问题
3. Graphiti 中没有足够的知识

**解决方案**：
```bash
# 检查环境变量
echo $OBSIDIAN_VAULT_PATH

# 检查 Obsidian vault 路径
ls -la ~/Documents/Obsidian/

# 检查 Graphiti 知识数量
opencode graphiti count
# 应该 > 10 条才值得同步
```

### Q3: 反馈同步没有检测到编辑

**可能原因**：
- 编辑时间不在检测范围内
- `.manifest.json` 未正确更新

**解决方案**：
```bash
# 强制完整同步
/sync-to-graphiti --force --full

# 检查 manifest
cat ~/Documents/Obsidian/.manifest.json | grep "last_sync"
```

---

## 📊 性能基准

测试时应达到的性能指标：

| 操作 | 预期时间 | 说明 |
|------|---------|------|
| Graphiti 查询 (100条) | < 100ms | Redis 缓存命中 |
| 知识精炼 (50→15条) | < 5s | LLM 处理时间 |
| 生成笔记 (3个) | < 2s | 模板渲染 |
| 写入 Obsidian | < 1s | 文件 I/O |
| 创建链接 | < 500ms | 链接检测 |
| **总计同步** | **< 10s** | 完整流程 |

---

## 🎯 下一步

测试通过后：

1. **开始实际使用**
   ```bash
   /autonomous "学习你的技术栈"
   ```

2. **定制工作流**
   - 调整精炼参数（maxCoreConcepts, minConfidence 等）
   - 自定义笔记模板
   - 配置特定项目的同步策略

3. **监控和优化**
   - 查看同步报告
   - 追踪知识质量
   - 优化参数配置

---

## 📚 相关文档

- `KNOWLEDGE_CYCLE_SYSTEM.md` - 完整架构说明
- `KNOWLEDGE_REFINEMENT_WORKFLOW.md` - 精炼工作流详解
- `GRAPHITI_EXPLAINED.md` - Graphiti 核心概念
- `USAGE_EXAMPLES.md` - 更多使用示例

---

*注意：本文档假设 Graphiti 和相关依赖已正确安装和配置。如遇到问题，请参考 OpenCode 官方文档。*
