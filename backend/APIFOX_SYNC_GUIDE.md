# Apifox 自动化同步方案完整指南

## 快速决策

| 需求 | 推荐方案 | 理由 |
|------|---------|------|
| **最快上手** | 手动导入（URL） | 无需配置，直接在 Apifox Web 界面操作 |
| **CI/CD 集成** | Open API | 纯 HTTP 调用，易于集成到自动化流程 |
| **AI 辅助** | MCP Server | 可通过自然语言控制，集成 Claude/Cursor |
| **测试场景** | CLI | 官方支持，适合自动化测试 |

## 方案对比表

| 特性 | 手动导入 | Open API | CLI | MCP Server |
|------|---------|---------|-----|------------|
| **自动化程度** | ❌ 手动 | ✅ 完全自动 | ⚠️ 半自动 | ✅ AI 驱动 |
| **学习成本** | ⭐ 极低 | ⭐⭐ 中等 | ⭐⭐ 中等 | ⭐⭐⭐ 较高 |
| **导入功能** | ✅ | ✅ | ❌ | ✅ |
| **测试功能** | ❌ | ❌ | ✅ | ❌ |
| **CI/CD 支持** | ❌ | ✅ | ✅ | ⚠️ 间接 |
| **AI 集成** | ❌ | ❌ | ❌ | ✅ |
| **实时同步** | ❌ | ✅ | ❌ | ✅ |
| **开发维护** | 官方 | 官方 | 官方 | 社区 |

## 推荐方案详解

### 🥇 方案一：Apifox Open API（推荐用于自动化）

**优势：**
- ✅ 官方支持，稳定可靠
- ✅ 纯 HTTP 调用，易于集成
- ✅ 支持智能合并和覆盖导入
- ✅ 可设置定时任务自动同步

**使用脚本：**
```bash
# 编辑配置
vim backend/sync-to-apifox-api.sh

# 配置你的 Token 和项目 ID
APIFOX_TOKEN="APS-xxxxx"
PROJECT_ID="123456"

# 运行同步
./backend/sync-to-apifox-api.sh
```

**适用场景：**
- CI/CD 流水线集成
- 定时自动同步
- 批量项目管理

### 🥈 方案二：MCP Server（推荐用于 AI 辅助）

**优势：**
- ✅ AI 驱动，自然语言控制
- ✅ 集成 Claude Desktop、Cursor
- ✅ 支持读取、写入、导入
- ✅ 未来扩展性强

**配置步骤：**
1. 安装 MCP Server
   ```bash
   git clone https://github.com/lishuji/apifox-mcp-server.git
   cd apifox-mcp-server
   npm install && npm run build
   ```

2. 配置 Claude Desktop
   ```json
   {
     "mcpServers": {
       "apifox": {
         "command": "node",
         "args": ["/path/to/apifox-mcp-server/dist/index.js"],
         "env": {
           "APIFOX_TOKEN": "APS-xxxxx",
           "APIFOX_PROJECT_ID": "123456"
         }
       }
     }
   }
   ```

3. 使用自然语言
   ```
   "帮我将 http://127.0.0.1:12580/swagger/v1/swagger.json 导入到 Apifox"
   ```

**适用场景：**
- 日常开发中 AI 辅助管理文档
- 团队协作，自然语言操作
- 需要读取和修改 API 文档

### 🥉 方案三：手动导入（最快上手）

**优势：**
- ✅ 无需配置
- ✅ 可视化界面
- ✅ 即时预览

**操作步骤：**
1. 打开 Apifox 项目
2. 项目设置 → 导入数据
3. 选择 URL 导入
4. 输入：`http://127.0.0.1:12580/swagger/v1/swagger.json`
5. 确认导入

**适用场景：**
- 一次性导入
- 快速验证
- 非技术用户

### ⚠️ 方案四：CLI（仅用于测试）

**注意：** Apifox CLI 主要用于**运行测试场景**，不支持导入 API 文档。

**可用功能：**
- 运行自动化测试
- 生成测试报告
- CI/CD 测试集成

**安装：**
```bash
npm i -g apifox-cli@latest
```

**使用：**
```bash
# 登录
apifox login --with-token APS-xxxxx

# 运行测试
apifox run --access-token APS-xxx -t <test-id> -e <env-id>
```

## 完整实施步骤

### Step 1: 准备工作

#### 1.1 获取 Apifox Token

1. 登录 https://app.apifox.com
2. 点击头像 → 账号设置
3. API 访问令牌 → 新建访问令牌
4. 复制 Token（格式：`APS-xxxxx`）

#### 1.2 获取项目 ID

从项目 URL 中获取：
```
https://app.apifox.com/project/123456
                        ^^^^^^^ 项目 ID
```

#### 1.3 确认 Swagger 可访问

```bash
curl http://127.0.0.1:12580/swagger/v1/swagger.json | jq '.info'
```

### Step 2: 选择方案并实施

#### 方案 A：使用 Open API（推荐）

```bash
# 1. 编辑脚本
vim backend/sync-to-apifox-api.sh

# 2. 修改配置
APIFOX_TOKEN="你的Token"
PROJECT_ID="你的项目ID"

# 3. 运行同步
./backend/sync-to-apifox-api.sh
```

#### 方案 B：使用 MCP Server

```bash
# 1. 安装 MCP Server
git clone https://github.com/lishuji/apifox-mcp-server.git
cd apifox-mcp-server
npm install && npm run build

# 2. 配置 Claude Desktop
vim ~/Library/Application\ Support/Claude/claude_desktop_config.json

# 3. 重启 Claude Desktop

# 4. 在 Claude 中使用
# "将 http://127.0.0.1:12580/swagger/v1/swagger.json 导入到 Apifox"
```

#### 方案 C：手动导入

直接在 Apifox Web 界面操作，详见上文。

### Step 3: 设置自动同步（可选）

#### 使用 crontab 定时同步

```bash
# 编辑 crontab
crontab -e

# 每天凌晨 2 点同步
0 2 * * * /Users/mac/workspace/magic-tao/backend/sync-to-apifox-api.sh
```

#### 使用 GitHub Actions

```yaml
# .github/workflows/sync-api-docs.yml
name: Sync API Docs

on:
  push:
    branches: [main]

jobs:
  sync:
    runs-on: ubuntu-latest
    steps:
      - name: Sync to Apifox
        env:
          APIFOX_TOKEN: ${{ secrets.APIFOX_TOKEN }}
          PROJECT_ID: ${{ secrets.APIFOX_PROJECT_ID }}
        run: |
          curl -X POST \
            "https://api.apifox.com/v1/projects/${PROJECT_ID}/import-openapi" \
            -H "Authorization: Bearer ${APIFOX_TOKEN}" \
            -H "Content-Type: application/json" \
            -d '{"url":"http://your-server/swagger/v1/swagger.json"}'
```

## 常见问题

### Q1: Token 认证失败怎么办？

**A:**
1. 检查 Token 格式（应为 `APS-xxxxx`）
2. 确认 Token 未过期
3. 检查 Token 权限设置

### Q2: 导入后接口分组混乱？

**A:**
使用智能合并模式，并在导入时勾选"同步 API 分组"

### Q3: 如何查看导入日志？

**A:**
- Open API 方式：查看脚本输出
- MCP 方式：查看 Claude Desktop 日志
- 手动方式：在 Apifox 界面查看导入历史

### Q4: 支持增量更新吗？

**A:**
支持！使用"智能合并"模式，只更新变化的接口，保留手动修改。

### Q5: 多个项目如何管理？

**A:**
- Open API：为每个项目创建不同的脚本配置
- MCP：在自然语言中指定项目 ID
- 手动：手动选择目标项目

## 最佳实践

### ✅ 推荐做法

1. **开发环境：使用 MCP Server**
   - 集成到 Cursor/Claude Desktop
   - 自然语言管理文档

2. **CI/CD：使用 Open API**
   - 自动化同步
   - 无人值守

3. **测试：使用 CLI**
   - 运行自动化测试
   - 生成测试报告

### ❌ 避免做法

1. 不要在代码中硬编码 Token
2. 不要频繁全量覆盖导入
3. 不要忽略导入失败的错误

## 相关文件

- `backend/sync-to-apifox.sh` - 手动导出脚本
- `backend/sync-to-apifox-api.sh` - Open API 自动同步脚本
- `backend/apifox-mcp-guide.md` - MCP Server 配置指南
- `backend/swagger.json` - 导出的 Swagger 文档

## 参考资源

- [Apifox CLI 文档](https://docs.apifox.com/apifox-cli)
- [Apifox Open API 文档](https://apifox-openapi.apifox.cn)
- [Apifox MCP Server](https://github.com/lishuji/apifox-mcp-server)
- [MCP 官方网站](https://modelcontextprotocol.io)

---

**下一步建议：**

根据你的需求选择方案：
- 🚀 **快速开始** → 使用 `sync-to-apifox-api.sh` 脚本
- 🤖 **AI 驱动** → 配置 MCP Server
- 📦 **一次性导入** → 手动 URL 导入