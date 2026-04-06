# Apifox MCP Server 配置指南

## 什么是 Apifox MCP Server？

Apifox MCP Server 是一个 Model Context Protocol (MCP) 服务器，允许 AI 助手（如 Claude、Cursor、Claude Desktop）通过自然语言自动管理 Apifox API 文档。

## 支持的功能

- ✅ 读取 API 文档
- ✅ 写入 API 文档
- ✅ 导入 OpenAPI/Swagger 文档
- ✅ 自动化文档管理

## 安装方式

### 方式一：使用 lishuji/apifox-mcp-server

```bash
# 克隆仓库
git clone https://github.com/lishuji/apifox-mcp-server.git
cd apifox-mcp-server

# 安装依赖
npm install

# 构建
npm run build
```

### 方式二：使用 Warren-W/apifox-mcp

```bash
# 克隆仓库
git clone https://github.com/Warren-W/apifox-mcp.git
cd apifox-mcp

# 安装依赖
npm install

# 构建
npm run build
```

## 配置 Claude Desktop

### macOS 配置

编辑 `~/Library/Application Support/Claude/claude_desktop_config.json`：

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

### Windows 配置

编辑 `%APPDATA%\Claude\claude_desktop_config.json`：

```json
{
  "mcpServers": {
    "apifox": {
      "command": "node",
      "args": ["C:\\path\\to\\apifox-mcp-server\\dist\\index.js"],
      "env": {
        "APIFOX_TOKEN": "APS-xxxxx",
        "APIFOX_PROJECT_ID": "123456"
      }
    }
  }
}
```

## 获取 Apifox Token 和项目 ID

### 获取 API Token

1. 登录 Apifox: https://app.apifox.com
2. 点击右上角头像 → **账号设置**
3. 左侧菜单选择 **API 访问令牌**
4. 点击 **新建访问令牌**
5. 输入令牌名称，选择权限
6. 复制生成的 Token（格式：`APS-xxxxx`）

### 获取项目 ID

1. 打开你的 Apifox 项目
2. 从 URL 中获取项目 ID
   - 例如：`https://app.apifox.com/project/123456`
   - 项目 ID 为：`123456`

## 使用方式

配置完成后，重启 Claude Desktop，然后可以直接向 AI 助手发出指令：

### 示例指令

```
# 导入 Swagger 文档
"帮我将 http://127.0.0.1:12580/swagger/v1/swagger.json 导入到 Apifox 项目中"

# 查看 API 列表
"列出 Apifox 项目中的所有 API 接口"

# 更新 API 文档
"更新用户登录接口的文档，添加新的请求参数说明"

# 同步最新文档
"从 Swagger URL 同步最新的 API 文档到 Apifox"
```

## MCP Server 工具列表

根据 Apifox MCP Server 实现，通常提供以下工具：

### 1. `import_openapi`
导入 OpenAPI/Swagger 文档

**参数：**
- `url`: Swagger JSON URL
- `importMode`: 导入模式（normal/overwrite）

### 2. `list_apis`
列出项目中的所有 API

**参数：**
- `projectId`: 项目 ID（可选，使用配置中的默认项目）

### 3. `get_api_detail`
获取 API 详细信息

**参数：**
- `apiId`: API ID

### 4. `update_api`
更新 API 文档

**参数：**
- `apiId`: API ID
- `data`: 更新的数据

## 在 Cursor 中使用

Cursor 也支持 MCP，配置方式类似：

编辑 `.cursor/mcp.json`：

```json
{
  "servers": {
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

## 自动化工作流示例

### CI/CD 集成

```yaml
# .github/workflows/sync-api-docs.yml
name: Sync API Docs to Apifox

on:
  push:
    branches: [main]
    paths:
      - 'backend/src/**'

jobs:
  sync:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install Apifox MCP Server
        run: |
          git clone https://github.com/lishuji/apifox-mcp-server.git
          cd apifox-mcp-server
          npm install
          npm run build

      - name: Sync Swagger to Apifox
        env:
          APIFOX_TOKEN: ${{ secrets.APIFOX_TOKEN }}
          APIFOX_PROJECT_ID: ${{ secrets.APIFOX_PROJECT_ID }}
        run: |
          # 使用 MCP Server 或直接调用 API
          curl -X POST \
            "https://api.apifox.com/v1/projects/${APIFOX_PROJECT_ID}/import-openapi" \
            -H "Authorization: Bearer ${APIFOX_TOKEN}" \
            -H "Content-Type: application/json" \
            -d @backend/swagger.json
```

## 故障排查

### MCP Server 无法启动

**检查：**
1. Node.js 版本 >= 16
2. 依赖是否完整安装
3. 构建是否成功

### Token 认证失败

**检查：**
1. Token 格式是否正确（`APS-xxxxx`）
2. Token 是否过期
3. Token 权限是否足够

### 导入失败

**检查：**
1. Swagger JSON 格式是否正确
2. 项目 ID 是否正确
3. 网络连接是否正常

## 参考资源

- **Apifox MCP Server (lishuji)**: https://github.com/lishuji/apifox-mcp-server
- **Apifox MCP (Warren-W)**: https://github.com/Warren-W/apifox-mcp
- **MCP 官方文档**: https://modelcontextprotocol.io
- **Apifox Open API 文档**: https://apifox-openapi.apifox.cn

## 下一步

1. 选择一个 MCP Server 实现（推荐 lishuji/apifox-mcp-server）
2. 安装并配置
3. 配置 Claude Desktop 或 Cursor
4. 测试导入功能
5. 享受 AI 自动化管理 API 文档的便捷！