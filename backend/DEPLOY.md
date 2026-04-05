# 魔力淘后端部署指南

## 部署流程概述

```
┌─────────────────────────────────────────────────────────────┐
│                     本地开发机                              │
├─────────────────────────────────────────────────────────────┤
│  1. 执行 build-and-export-docker.sh                         │
│     └─> 构建镜像 + 导出带时间戳的 tar 包                      │
│     └─> molitao-backend-YYYYMMDD-HHMMSS.tar                │
│                                                             │
│  2. 执行 deploy-to-server.sh                                │
│     └─> 自动上传 tar 包到服务器                              │
│     └─> 服务器执行 load-image.sh 加载镜像                     │
│     └─> 重启 docker-compose 服务                            │
│     └─> 验证部署状态                                        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     服务器 molitao                           │
├─────────────────────────────────────────────────────────────┤
│  /data/dotnetapi/                                          │
│    ├── molitao-backend-YYYYMMDD-HHMMSS.tar  (上传的包)    │
│    ├── docker-compose.yml                    (编排配置)     │
│    └── load-image.sh                        (镜像加载脚本)  │
│                                                             │
│  容器: molitao-api-production                              │
│  端口: 5000                                                │
│  域名: www.molitao.top                                     │
└─────────────────────────────────────────────────────────────┘
```

## 一键部署命令

在本地 `backend` 目录下执行：

```bash
cd /Users/mac/workspace/magic-tao/backend
./deploy-to-server.sh
```

脚本会自动完成：
1. 构建 Docker 镜像
2. 导出为 tar 包
3. 上传到服务器
4. 加载镜像
5. 重启服务
6. 验证部署

## 分步部署（如需单独操作）

### 步骤1: 本地构建并导出镜像

```bash
cd backend
./build-and-export-docker.sh
```

**注意**：
- 导出文件格式: `molitao-backend-YYYYMMDD-HHMMSS.tar`
- 使用 `latest` tag 覆盖本地镜像
- **每次构建都会生成新的带时间戳的 tar 包**

### 步骤2: 上传到服务器

```bash
# 方式A: 使用完整部署脚本（推荐）
./deploy-to-server.sh

# 方式B: 手动上传
TAR_FILE=$(ls -t molitao-backend-*.tar | head -1)
scp -o StrictHostKeyChecking=no "$TAR_FILE" molitao:/data/dotnetapi/
```

### 步骤3: 服务器加载镜像

```bash
# 使用带时间戳的 tar 包加载
ssh molitao "cd /data/dotnetapi && \
  ./load-image.sh molitao-backend-YYYYMMDD-HHMMSS.tar -y"
```

**重要**：
- `-y` 参数表示自动确认删除旧镜像
- 不指定 `-y` 会提示确认
- 必须传入完整的 tar 包名称（不能用 `molitao-backend-latest.tar`）

### 步骤4: 重启服务

```bash
ssh molitao "cd /data/dotnetapi && \
  docker-compose down && \
  docker-compose up -d production"
```

## 服务器常用命令

```bash
# 查看容器状态
ssh molitao "docker ps | grep molitao"

# 查看容器日志
ssh molitao "docker logs -f molitao-api-production"

# 查看服务健康状态
ssh molitao "curl -s http://localhost:5000/swagger"

# 重启服务
ssh molitao "cd /data/dotnetapi && docker-compose restart production"

# 停止服务
ssh molitao "cd /data/dotnetapi && docker-compose stop production"
```

## 验证部署

### API 验证

```bash
# 测试健康检查接口
curl -s -o /dev/null -w "%{http_code}" https://www.molitao.top/swagger

# 测试新增的 API
curl -s "https://www.molitao.top/api/services/app/Client/PayDepositNative?amount=0.01"
# 期望: 返回 401 Unauthorized（接口存在但需要登录）

# 测试拍卖列表
curl -s "https://www.molitao.top/api/services/app/AuctionItem/GetPublicList"
```

### 服务器日志验证

```bash
ssh molitao "docker logs molitao-api-production --tail 50"
```

## 常见问题

### Q: 部署后接口返回 404

**原因**: 服务器容器使用的还是旧镜像

**解决方法**:
```bash
# 1. 确认 tar 包已上传
ssh molitao "ls -la /data/dotnetapi/molitao-backend-*.tar | tail -3"

# 2. 确认加载的是正确的镜像
ssh molitao "docker images | grep molitao"

# 3. 确认容器使用的是最新镜像
ssh molitao "docker inspect molitao-api-production | grep Image"

# 4. 如果镜像不对，重新加载并重启
ssh molitao "cd /data/dotnetapi && \
  docker-compose down && \
  ./load-image.sh <正确的tar包> -y && \
  docker-compose up -d production"
```

### Q: docker-compose 命令找不到

**原因**: docker-compose 文件名不对

```bash
# 检查实际文件名
ssh molitao "ls /data/dotnetapi/docker-compose*.yml"
# 可能需要用 docker-compose.yml 而不是 docker-compose-api.yml
```

### Q: 镜像加载失败

**原因**: 旧镜像占用 tag

```bash
# 强制删除旧镜像后再加载
ssh molitao "docker rmi -f litengfei0302/molitao-backend:latest"
ssh molitao "cd /data/dotnetapi && ./load-image.sh <tar包> -y"
```

## 环境配置

| 环境 | 服务器 | 域名 | 端口 |
|------|--------|------|------|
| 生产 | molitao | www.molitao.top | 5000 |
| Alpha | localhost | 127.0.0.1 | 5001 |

## 相关文件

| 文件 | 说明 |
|------|------|
| `build-and-export-docker.sh` | 构建并导出 Docker 镜像 |
| `load-image.sh` | 从 tar 包加载镜像 |
| `deploy-to-server.sh` | 一键部署脚本 |
| `docker-compose.yml` | 服务编排配置 |
| `appsettings.Production.json` | 生产环境配置 |

## 回滚操作

如果新版本有问题，回滚到旧版本：

```bash
# 1. 找到旧版本的 tar 包
ssh molitao "ls -la /data/dotnetapi/molitao-backend-*.tar"

# 2. 加载旧版本
ssh molitao "cd /data/dotnetapi && ./load-image.sh <旧tar包> -y"

# 3. 重启服务
ssh molitao "cd /data/dotnetapi && docker-compose down && docker-compose up -d production"
```
