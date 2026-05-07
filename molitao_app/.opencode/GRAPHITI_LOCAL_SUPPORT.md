# Graphiti 本地支持状态

## 📊 当前状态

### ✅ 已安装

**opencode-graphiti 插件**：
- 版本：0.2.3
- 位置：`~/.config/opencode/node_modules/opencode-graphiti/`
- 状态：已安装，但未配置

### ❌ 缺少依赖

**需要配置的后端服务**：

1. **Redis** (必需) - 短期记忆热存储
   - 用途：会话事件缓存、本地快照
   - 默认端口：6379
   - 状态：❌ 未运行

2. **Graphiti MCP Server** (可选) - 长期记忆持久化
   - 用途：跨会话知识图谱存储
   - 默认端口：8000
   - 状态：❌ 未运行

3. **FalkorDB 或 Neo4j** (Graphiti 后端)
   - 用途：Graphiti 的图数据库
   - 状态：❌ 未运行

---

## 🔧 如何启用 Graphiti

### 方案 1: 最小配置（只用短期记忆）

**只需要 Redis**：

```bash
# 1. 启动 Redis
docker run -d -p 6379:6379 redis:latest

# 或使用 Docker Compose
# docker-compose.yml
version: '3'
services:
  redis:
    image: redis:latest
    ports:
      - "6379:6379"
```

**效果**：
- ✅ 短期记忆：会话持续性摘要
- ✅ 会话事件缓存
- ✅ 本地快照重建
- ❌ 长期记忆：跨会话知识持久化（需要 Graphiti）

### 方案 2: 完整配置（短期 + 长期记忆）

**需要 Redis + Graphiti + 图数据库**：

```bash
# 1. 启动 Redis
docker run -d -p 6379:6379 redis:latest

# 2. 启动 FalkorDB (Graphiti 的轻量级后端)
docker run -d -p 6379:6379 falkordb/falkordb:latest

# 3. 启动 Graphiti MCP Server
pip install graphiti-core
graphiti-server --port 8000

# 或使用 Docker (如果有官方镜像)
# docker run -d -p 8000:8000 getzep/graphiti-server:latest
```

**配置 opencode.json**：

```json
{
  "plugin": [
    "opencode-graphiti@latest"
  ],
  "redis": {
    "endpoint": "redis://localhost:6379",
    "batchSize": 20,
    "sessionTtlSeconds": 86400,
    "cacheTtlSeconds": 600
  },
  "graphiti": {
    "endpoint": "http://localhost:8000/mcp",
    "groupIdPrefix": "opencode",
    "driftThreshold": 0.5
  }
}
```

**效果**：
- ✅ 短期记忆：会话持续性
- ✅ 长期记忆：跨会话知识持久化
- ✅ 知识图谱存储和检索
- ✅ 向量/图搜索

---

## 📋 推荐方案

### 对于开发/测试：方案 1（只用 Redis）

**优点**：
- 配置简单，只需要 Redis
- 立即可用
- 短期记忆功能完整

**缺点**：
- 无跨会话持久化
- 重启后会话记忆丢失

**适合场景**：
- 单次会话任务
- 快速测试
- 不需要长期记忆

### 对于生产环境：方案 2（完整配置）

**优点**：
- 完整的记忆系统
- 跨会话知识保留
- 可用于知识循环系统

**缺点**：
- 配置复杂
- 需要多个服务

**适合场景**：
- 长期项目开发
- 知识积累和复用
- 我们的知识循环系统

---

## 🚀 快速开始

### 步骤 1: 启动 Redis（5分钟）

```bash
# 使用 Docker 启动
docker run -d --name redis-local -p 6379:6379 redis:latest

# 验证 Redis 运行
docker ps | grep redis
redis-cli ping
# 应该返回: PONG
```

### 步骤 2: 重启 OpenCode

```bash
# 重启 OpenCode 以加载 Graphiti 插件
# (根据你的启动方式)
```

### 步骤 3: 验证功能

```bash
# 检查插件是否加载
opencode plugins list | grep graphiti

# 应该看到：
# ✅ opencode-graphiti@0.2.3
```

### 步骤 4: 测试短期记忆

```bash
# 在 OpenCode 中执行
/some-command

# 插件会自动：
# 1. 记录会话事件到 Redis
# 2. 生成会话快照
# 3. 注入 <session_memory> 到上下文
```

---

## 💡 当前插件提供的功能

### 已实现（无需 Graphiti 后端）

1. **短期记忆**
   - 会话事件记录
   - 优先级分层快照
   - 会话持续性摘要
   - 主题漂移检测

2. **MCP 工具**
   - `session_execute` - 执行命令并记录
   - `session_search` - 搜索本地缓存
   - `session_index` - 索引内容到本地
   - `session_batch_execute` - 批量执行

### 需要 Graphiti 后端

1. **长期记忆**
   - 跨会话知识持久化
   - 知识图谱存储
   - 向量/图搜索
   - 知识精炼和同步

2. **知识循环系统**
   - `/sync-to-obsidian` - 需要读取 Graphiti 知识
   - `/sync-to-graphiti` - 需要写入 Graphiti

---

## 🎯 建议行动

### 立即可做

1. **启动 Redis**（5分钟）
   ```bash
   docker run -d -p 6379:6379 redis:latest
   ```

2. **测试短期记忆**
   - 在 OpenCode 中执行任务
   - 观察会话快照注入
   - 验证上下文连续性

### 后续配置（可选）

1. **安装 FalkorDB**（10分钟）
   ```bash
   docker run -d -p 6379:6379 falkordb/falkordb:latest
   ```

2. **安装 Graphiti MCP Server**（15分钟）
   ```bash
   pip install graphiti-core
   graphiti-server --port 8000
   ```

3. **配置知识循环**
   - 配置 Graphiti 连接
   - 测试 `/sync-to-obsidian`
   - 测试 `/sync-to-graphiti`

---

## 📊 总结

| 功能 | Redis | Graphiti 后端 | 当前状态 |
|------|-------|--------------|---------|
| 短期记忆 | ✅ 必需 | ❌ 不需要 | ⏳ 待配置 Redis |
| 会话快照 | ✅ 必需 | ❌ 不需要 | ⏳ 待配置 Redis |
| 长期记忆 | ✅ 必需 | ✅ 必需 | ❌ 未配置 |
| 知识图谱 | ❌ 不需要 | ✅ 必需 | ❌ 未配置 |
| 知识循环 | ✅ 必需 | ✅ 必需 | ❌ 未配置 |

---

## ✅ 结论

**Graphiti 插件已安装**，但需要配置后端服务才能使用：

1. **最小可用**：启动 Redis → 获得短期记忆功能
2. **完整功能**：Redis + Graphiti + FalkorDB → 获得全部功能

**推荐**：先启动 Redis 测试短期记忆，后续再配置 Graphiti 后端启用知识循环系统。

---

*文档创建：2026-05-06*
