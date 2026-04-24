# 魔力淘后端部署指南

## 脚本目录结构

```
scripts/                         # 所有脚本集中管理
├── build-and-export-docker.sh  # 构建打包脚本
├── upload-and-deploy.sh         # 上传部署脚本
├── deploy-to-server.sh          # 旧版一键部署脚本（保留参考）
├── deploy.sh                    # 服务器容器管理脚本
└── load-image.sh                # 镜像加载脚本

backend/
├── docker-compose-api.yml      # 服务编排配置
└── appsettings.Production.json # 生产环境配置
```

## 部署流程

```
┌─────────────────────────────────────────────────────────────┐
│                     本地开发机 (scripts/)                    │
├─────────────────────────────────────────────────────────────┤
│  1. 执行 build-and-export-docker.sh                         │
│     └─> 构建镜像 + 导出带时间戳的 tar 包                      │
│     └─> molitao-backend-YYYYMMDD-HHMMSS.tar                │
│                                                             │
│  2. 执行 upload-and-deploy.sh                               │
│     └─> 上传 tar 包到服务器                                │
│     └─> 服务器加载镜像                                      │
│     └─> 重启 docker-compose 服务                            │
└─────────────────────────────────────────────────────────────┘
```
┌─────────────────────────────────────────────────────────────┐
│                     本地开发机 (scripts/local)                │
├─────────────────────────────────────────────────────────────┤
│  1. 执行 build-and-export-docker.sh                         │
│     └─> 构建镜像 + 导出带时间戳的 tar 包                      │
│     └─> molitao-backend-YYYYMMDD-HHMMSS.tar                │
│                                                             │
│  2. 执行 upload-and-deploy.sh                               │
│     └─> 上传 tar 包到服务器                                │
│     └─> 服务器加载镜像                                      │
│     └─> 重启 docker-compose 服务                            │
└─────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                     服务器 molitao                           │
├─────────────────────────────────────────────────────────────┤
│  /data/dotnetapi/                                          │
│    ├── molitao-backend-YYYYMMDD-HHMMSS.tar  (上传的包)    │
│    ├── docker-compose-api.yml               (编排配置)     │
│    └── load-image.sh                        (镜像加载脚本)  │
│                                                             │
│  容器: molitao-api-production                              │
│  端口: 5000                                                │
│  域名: www.molitao.top                                     │
└─────────────────────────────────────────────────────────────┘
```

## 推荐部署方式（分步执行）

### 步骤1: 本地构建并导出镜像

```bash
cd /Users/mac/workspace/magic-tao/scripts
./build-and-export-docker.sh
```

**输出**: `molitao-backend-YYYYMMDD-HHMMSS.tar`

### 步骤2: 上传到服务器并部署

```bash
cd /Users/mac/workspace/magic-tao/scripts
./upload-and-deploy.sh
```

脚本会自动完成：
1. 上传 tar 包到服务器
2. 创建目录
3. 加载镜像
4. 部署容器
5. 验证部署

### 方式B: 手动部署

```bash
# 1. 上传 tar 包
TAR_FILE=$(ls -t molitao-backend-*.tar | head -1)
scp -o StrictHostKeyChecking=no "$TAR_FILE" molitao:/data/dotnetapi/

# 2. 服务器加载镜像
ssh molitao "cd /data/dotnetapi && ./load-image.sh $TAR_FILE -y"

# 3. 服务器重启服务
ssh molitao "cd /data/dotnetapi && docker-compose -f docker-compose-api.yml down && docker-compose -f docker-compose-api.yml up -d production"
```

## 服务器容器管理 (scripts/server/deploy.sh)

在**服务器上**执行此脚本管理容器：

```bash
# 在服务器上操作
cd /data/dotnetapi

# 启动服务
./deploy.sh start --env=production

# 停止服务
./deploy.sh stop --env=production

# 重启服务
./deploy.sh restart --env=production

# 查看日志
./deploy.sh logs --env=production

# 查看状态
./deploy.sh status

# 清理
./deploy.sh clean
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
ssh molitao "cd /data/dotnetapi && docker-compose -f docker-compose-api.yml restart production"

# 停止服务
ssh molitao "cd /data/dotnetapi && docker-compose -f docker-compose-api.yml stop production"
```

## 验证部署

```bash
# 测试健康检查接口
curl -s -o /dev/null -w "%{http_code}" https://www.molitao.top/swagger

# 测试拍卖列表
curl -s "https://www.molitao.top/api/services/app/AuctionItem/GetPublicList"

# 服务器日志验证
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

# 3. 如果镜像不对，重新加载并重启
ssh molitao "cd /data/dotnetapi && \
  docker-compose -f docker-compose-api.yml down && \
  ./load-image.sh <正确的tar包> -y && \
  docker-compose -f docker-compose-api.yml up -d production"
```

### Q: 镜像加载失败

**原因**: 旧镜像占用 tag

```bash
ssh molitao "docker rmi -f litengfei0302/molitao-backend:latest"
ssh molitao "cd /data/dotnetapi && ./load-image.sh <tar包> -y"
```

## 环境配置

| 环境 | 服务器 | 域名 | 端口 |
|------|--------|------|------|
| 生产 | molitao | www.molitao.top | 5000 |
| Alpha | localhost | 127.0.0.1 | 5001 |

## 回滚操作

如果新版本有问题，回滚到旧版本：

```bash
# 1. 找到旧版本的 tar 包
ssh molitao "ls -la /data/dotnetapi/molitao-backend-*.tar"

# 2. 使用 scripts/upload-and-deploy.sh 重新部署
cd /Users/mac/workspace/magic-tao/scripts
./upload-and-deploy.sh --tar=/data/dotnetapi/<旧tar包>
```

## 相关文件

| 文件 | 说明 |
|------|------|
| `scripts/build-and-export-docker.sh` | 构建打包 |
| `scripts/upload-and-deploy.sh` | 上传部署 |
| `scripts/deploy-to-server.sh` | 旧版一键部署（保留参考） |
| `scripts/deploy.sh` | 服务器容器管理 |
| `scripts/load-image.sh` | 镜像加载脚本 |
| `backend/docker-compose-api.yml` | 服务编排配置 |
