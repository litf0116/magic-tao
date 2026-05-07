# Graphiti 长期记忆系统搭建指南

完整的长期记忆系统架构：OpenCode → Redis (短期) → Graphiti → FalkorDB (长期)

---

## 📋 系统架构

```
┌─────────────────────────────────────────────────────────┐
│                    OpenCode 会话                        │
│                 (opencode-graphiti 插件)                │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ├─ 短期记忆 (热存储)
                   │  └─→ Redis (localhost:6379)
                   │       ├─ 会话事件缓存
                   │       ├─ 优先级快照
                   │       └─ 本地索引
                   │
                   └─ 长期记忆 (持久化)
                      └─→ Graphiti MCP Server (localhost:8000)
                           └─→ FalkorDB (localhost:6379)
                                ├─ 知识图谱存储
                                ├─ 向量索引
                                └─ 时序追踪
```

---

## 🚀 快速搭建（Docker Compose）

### 步骤 1: 创建配置文件

创建 `~/graphiti-stack/docker-compose.yml`：

```yaml
version: '3.8'

services:
  # Redis - 短期记忆热存储
  redis:
    image: redis:7-alpine
    container_name: graphiti-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    networks:
      - graphiti-network

  # FalkorDB - Graphiti 的图数据库后端
  falkordb:
    image: falkordb/falkordb:latest
    container_name: graphiti-falkordb
    ports:
      - "6380:6379"  # 避免与 Redis 冲突
    volumes:
      - falkordb-data:/data
    environment:
      - FALKORDB_ARGS=--save 60 1 --appendonly yes
    healthcheck:
      test: ["CMD", "redis-cli", "-p", "6379", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    networks:
      - graphiti-network

  # Graphiti MCP Server - 知识图谱服务
  graphiti-server:
    image: getzep/graphiti-server:latest
    container_name: graphiti-server
    ports:
      - "8000:8000"
    environment:
      # Graphiti 配置
      - GRAPHITI_URI=redis://falkordb:6379
      - GRAPHITI_DATABASE=graphiti
      
      # OpenAI Embedding 配置 (必需)
      - OPENAI_API_KEY=${OPENAI_API_KEY}
      - OPENAI_EMBEDDING_MODEL=text-embedding-3-small
      
      # 服务配置
      - MCP_SERVER_HOST=0.0.0.0
      - MCP_SERVER_PORT=8000
      
      # 日志级别
      - LOG_LEVEL=INFO
    depends_on:
      falkordb:
        condition: service_healthy
      redis:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/health"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - graphiti-network

volumes:
  redis-data:
    driver: local
  falkordb-data:
    driver: local

networks:
  graphiti-network:
    driver: bridge
```

### 步骤 2: 创建环境变量文件

创建 `~/graphiti-stack/.env`：

```bash
# OpenAI API Key (必需 - 用于 embedding)
OPENAI_API_KEY=sk-your-openai-api-key-here

# 或使用其他兼容的 API (可选)
# OPENAI_API_BASE=https://api.deepseek.com
# OPENAI_EMBEDDING_MODEL=deepseek-embedding
```

### 步骤 3: 启动服务

```bash
cd ~/graphiti-stack

# 启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f graphiti-server

# 检查服务状态
docker-compose ps
```

**预期输出**：
```
NAME                 STATUS              PORTS
graphiti-redis       running (healthy)   0.0.0.0:6379->6379/tcp
graphiti-falkordb    running (healthy)   0.0.0.0:6380->6379/tcp
graphiti-server      running (healthy)   0.0.0.0:8000->8000/tcp
```

---

## ⚙️ 配置 OpenCode

### 步骤 4: 更新 opencode.json

编辑 `~/.config/opencode/opencode.json`：

```json
{
  "plugin": [
    "opencode-graphiti@latest"
  ],
  
  "redis": {
    "endpoint": "redis://localhost:6379",
    "batchSize": 20,
    "batchMaxBytes": 51200,
    "sessionTtlSeconds": 86400,
    "cacheTtlSeconds": 600,
    "drainRetryMax": 3
  },
  
  "graphiti": {
    "endpoint": "http://localhost:8000/mcp",
    "groupIdPrefix": "opencode",
    "driftThreshold": 0.5
  }
}
```

### 步骤 5: 重启 OpenCode

```bash
# 重启 OpenCode 使配置生效
# (根据你的启动方式)
```

---

## 🧪 验证安装

### 步骤 6: 测试服务连接

```bash
# 1. 测试 Redis
redis-cli ping
# 应返回: PONG

# 2. 测试 FalkorDB
redis-cli -p 6380 ping
# 应返回: PONG

# 3. 测试 Graphiti Server
curl http://localhost:8000/health
# 应返回: {"status": "healthy"}

# 4. 测试 MCP 端点
curl -X POST http://localhost:8000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "method": "tools/list", "id": 1}'
# 应返回工具列表
```

### 步骤 7: 测试知识存储

在 OpenCode 中执行：

```bash
# 学习新知识（会自动存储到 Graphiti）
/autonomous "学习 GoRouter 基础用法"

# 查询 Graphiti 中的知识
# (通过 MCP 工具)
```

---

## 📊 服务说明

### Redis (端口 6379)

**作用**：短期记忆热存储

**存储内容**：
- 会话事件流
- 优先级快照
- 本地索引缓存
- 临时会话数据

**持久化**：RDB + AOF

**监控**：
```bash
redis-cli info memory
redis-cli info stats
```

### FalkorDB (端口 6380)

**作用**：Graphiti 的图数据库后端

**存储内容**：
- 知识图谱节点 (EntityNode)
- 关系边 (EntityEdge)
- 事件节点 (EpisodicNode)
- 社区节点 (CommunityNode)

**特点**：
- 基于 Redis 的图数据库
- 支持 Cypher 查询
- 向量索引
- 时序追踪

**监控**：
```bash
redis-cli -p 6380 info graph
```

### Graphiti Server (端口 8000)

**作用**：知识图谱服务层

**提供功能**：
- MCP 协议接口
- 知识提取和存储
- 向量嵌入生成
- 图遍历和搜索

**API 端点**：
- `/health` - 健康检查
- `/mcp` - MCP 协议端点

---

## 🔄 使用流程

### 1. 短期记忆（自动）

```bash
# 在 OpenCode 中执行任务
# 插件自动：
# 1. 记录会话事件到 Redis
# 2. 生成优先级快照
# 3. 注入 <session_memory> 到上下文
```

### 2. 长期记忆（后台）

```bash
# 插件自动在后台：
# 1. 批量发送会话事件到 Graphiti
# 2. Graphiti 存储到 FalkorDB
# 3. 生成向量嵌入
# 4. 建立知识图谱
```

### 3. 知识检索

```bash
# 插件自动：
# 1. 检测主题漂移
# 2. 从 Graphiti 搜索相关知识
# 3. 缓存到 Redis
# 4. 注入到上下文
```

### 4. 知识循环

```bash
# 同步到 Obsidian
/sync-to-obsidian

# 反馈到 Graphiti
/sync-to-graphiti
```

---

## 🛠️ 高级配置

### 使用 Neo4j 替代 FalkorDB

如果你更喜欢 Neo4j：

```yaml
# docker-compose.yml 中替换 FalkorDB
neo4j:
  image: neo4j:5-community
  container_name: graphiti-neo4j
  ports:
    - "7474:7474"  # HTTP
    - "7687:7687"  # Bolt
  environment:
    - NEO4J_AUTH=neo4j/password123
    - NEO4J_PLUGINS=["apoc"]
  volumes:
    - neo4j-data:/data
```

**更新 Graphiti Server 配置**：
```bash
- GRAPHITI_URI=bolt://neo4j:7687
- GRAPHITI_USER=neo4j
- GRAPHITI_PASSWORD=password123
```

### 自定义 Embedding 模型

使用其他 OpenAI 兼容的 embedding 服务：

```yaml
# docker-compose.yml
environment:
  - OPENAI_API_BASE=https://api.deepseek.com
  - OPENAI_API_KEY=${DEEPSEEK_API_KEY}
  - OPENAI_EMBEDDING_MODEL=deepseek-embedding
```

### 调整性能参数

```json
// opencode.json
{
  "redis": {
    "batchSize": 50,           // 增大批次
    "batchMaxBytes": 102400,   // 增大批次字节限制
    "sessionTtlSeconds": 172800, // 延长会话 TTL (48h)
    "cacheTtlSeconds": 1200     // 延长缓存 TTL (20min)
  },
  "graphiti": {
    "driftThreshold": 0.3  // 降低阈值，更频繁刷新
  }
}
```

---

## 📈 监控和维护

### 日志查看

```bash
# 查看所有服务日志
docker-compose logs -f

# 只看 Graphiti Server 日志
docker-compose logs -f graphiti-server

# 查看 Redis 日志
docker-compose logs -f redis
```

### 数据备份

```bash
# Redis 备份
docker exec graphiti-redis redis-cli BGSAVE
docker cp graphiti-redis:/data/dump.rdb ~/backups/redis-$(date +%Y%m%d).rdb

# FalkorDB 备份
docker exec graphiti-falkordb redis-cli -p 6379 BGSAVE
docker cp graphiti-falkordb:/data/dump.rdb ~/backups/falkordb-$(date +%Y%m%d).rdb
```

### 性能监控

```bash
# Redis 内存使用
redis-cli info memory | grep used_memory_human

# FalkorDB 图统计
redis-cli -p 6380 info graph

# Graphiti Server 状态
curl http://localhost:8000/metrics
```

---

## 🔧 故障排查

### 问题 1: Redis 连接失败

**症状**：
```
Error: connect ECONNREFUSED 127.0.0.1:6379
```

**解决方案**：
```bash
# 检查 Redis 是否运行
docker ps | grep redis

# 重启 Redis
docker-compose restart redis

# 检查端口
lsof -i :6379
```

### 问题 2: Graphiti Server 无法启动

**症状**：
```
graphiti-server exited with code 1
```

**解决方案**：
```bash
# 查看日志
docker-compose logs graphiti-server

# 常见原因：
# 1. OPENAI_API_KEY 未设置
# 2. FalkorDB 未就绪
# 3. 端口冲突

# 检查依赖服务
docker-compose ps

# 重启所有服务
docker-compose restart
```

### 问题 3: Embedding 生成失败

**症状**：
```
Error: OpenAI API error: 401 Unauthorized
```

**解决方案**：
```bash
# 检查 API Key
cat ~/graphiti-stack/.env | grep OPENAI_API_KEY

# 测试 API Key
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer $OPENAI_API_KEY"

# 更新配置
docker-compose down
vim ~/graphiti-stack/.env
docker-compose up -d
```

---

## 💰 成本估算

### OpenAI Embedding API

**模型**：`text-embedding-3-small`

**价格**：$0.02 / 1M tokens

**估算**：
- 每个知识点：~200 tokens
- 每天新增：100 知识点 = 20,000 tokens
- 每月成本：100 知识点 × 30 天 × 200 tokens × $0.02 / 1M ≈ **$1.20/月**

### 替代方案

使用本地 embedding 模型（免费）：

```yaml
# 使用 Ollama 本地模型
ollama:
  image: ollama/ollama:latest
  ports:
    - "11434:11434"
  
# 更新 Graphiti 配置
environment:
  - EMBEDDING_PROVIDER=ollama
  - OLLAMA_EMBEDDING_MODEL=nomic-embed-text
```

---

## 🎯 下一步

### 1. 启动服务

```bash
cd ~/graphiti-stack
docker-compose up -d
```

### 2. 验证连接

```bash
# 运行所有测试
curl http://localhost:8000/health
redis-cli ping
redis-cli -p 6380 ping
```

### 3. 开始使用

```bash
# 在 OpenCode 中
/autonomous "学习你的第一个技术主题"

# 查看知识积累
# (通过 Graphiti MCP 工具查询)
```

### 4. 知识循环

```bash
# 同步到 Obsidian
/sync-to-obsidian

# 反馈人类洞察
/sync-to-graphiti
```

---

## 📚 相关文档

- `GRAPHITI_LOCAL_SUPPORT.md` - 当前支持状态
- `TESTING_GUIDE.md` - 测试指南
- `KNOWLEDGE_CYCLE_SYSTEM.md` - 知识循环架构

---

*创建时间：2026-05-06*
*预计搭建时间：30 分钟*
