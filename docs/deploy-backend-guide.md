# 魔力淘 Backend 打包发布流程

## 概述

本文档描述魔力淘 Backend 的完整打包发布流程，包括构建打包和上传部署两个独立步骤。

## 流程图

```
┌─────────────────────────────────────────────────────────────────┐
│                     完整发布流程                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  步骤 1: 构建打包                                                │
│  命令: bash build-and-export-docker.sh                           │
│  输出: molitao-backend-YYYYMMDD-HHMMSS.tar                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ (手动确认后)
┌─────────────────────────────────────────────────────────────────┐
│  步骤 2: 上传部署                                                │
│  命令: bash upload-and-deploy.sh [--tar=xxx.tar]                │
│  输出: 服务器上运行的容器                                          │
└─────────────────────────────────────────────────────────────────┘
```

## 准备工作

### 1. 确认服务器连接

确保 SSH 密钥配置正确，能够无密码连接服务器：

```bash
# 测试连接
ssh molitao "echo '连接成功'"
```

### 2. 检查本地环境

```bash
# 确认 Docker 运行正常
docker --version

# 确认已登录 Docker Hub (如需要)
docker login
```

## 使用方法

### 方式一: 完整流程 (分步执行)

**步骤 1: 构建打包**

```bash
cd backend/scripts/local

# 构建 Docker 镜像并导出 tar 包
bash build-and-export-docker.sh
```

输出示例:
```
==========================================
开始构建魔力淘API Docker镜像
==========================================
步骤1: 构建Docker镜像...
✅ Docker镜像构建成功!

步骤2: 导出Docker镜像为tar包...
导出文件: molitao-backend-20250424-151200.tar
✅ Docker镜像导出成功!
导出文件大小: 1.2G

==========================================
🎉 所有操作完成!
镜像名称: litengfei0302/molitao-backend:latest
导出文件: molitao-backend-20250424-151200.tar
==========================================
```

**步骤 2: 上传部署**

```bash
cd backend/scripts/local

# 自动查找最新 tar 包并部署
bash upload-and-deploy.sh

# 或指定 tar 包
bash upload-and-deploy.sh --tar=molitao-backend-20250424-151200.tar
```

输出示例:
```
==========================================
魔力淘 Backend 上传部署脚本
==========================================
使用 tar 包: molitao-backend-20250424-151200.tar (大小: 1.2G)

[1/4] 上传 tar 包到服务器...
✅ tar 包上传成功

[2/4] 创建服务器目录...
✅ 目录创建完成

[3/4] 加载 Docker 镜像...
✅ Docker 镜像加载成功

[4/4] 部署容器...
✅ 部署命令执行成功

==========================================
部署完成!
==========================================
tar 包: molitao-backend-20250424-151200.tar
服务器: molitao
远程路径: /data/dotnetapi/molitao-backend-20250424-151200.tar

常用命令:
  查看容器: ssh molitao 'docker ps | grep molitao'
  查看日志: ssh molitao 'docker logs -f molitao-api-production'
  重启服务: ssh molitao 'docker restart molitao-api-production'
==========================================
```

### 方式二: 仅上传部署 (已有 tar 包)

如果 tar 包已存在，可直接执行上传部署:

```bash
cd backend/scripts/local

# 自动查找最新 tar 包
bash upload-and-deploy.sh

# 指定服务器
bash upload-and-deploy.sh --server=backup-server

# 指定 tar 包路径
bash upload-and-deploy.sh --tar=/path/to/xxx.tar
```

## 脚本参数说明

### upload-and-deploy.sh 参数

| 参数 | 说明 | 默认值 |
|------|------|-------|
| `--tar=FILE` | 指定 tar 包路径 | 自动查找最新的 |
| `--server=HOST` | 服务器别名 | molitao |
| `--skip-build` | 跳过构建步骤 | false |
| `--help` | 显示帮助信息 | - |

### build-and-export-docker.sh 参数

无参数，自动生成带时间戳的 tar 包。

## 服务器信息

| 配置 | 值 |
|------|-----|
| 服务器别名 | molitao |
| 服务器地址 | (通过 SSH config 配置) |
| 远程工作目录 | /data/dotnetapi |
| 日志目录 | /data2/logs |
| 容器名称 | molitao-api-production |
| 服务端口 | 12580 |

## 验证部署

### 1. 检查容器状态

```bash
ssh molitao 'docker ps | grep molitao'
```

### 2. 检查服务响应

```bash
curl -s -o /dev/null -w "%{http_code}" http://molitao:12580/
# 预期: 302 (重定向到 index.html)
```

### 3. 查看日志

```bash
# 实时查看日志
ssh molitao 'docker logs -f molitao-api-production'

# 查看特定日期日志
ssh molitao 'tail -f /data2/logs/api-20250424.log'
```

### 4. 进入容器调试

```bash
ssh molitao 'docker exec -it molitao-api-production /bin/bash'
```

## 回滚操作

如果新版本有问题，可以回滚到旧版本:

```bash
# 1. 查看历史 tar 包
ls -la backend/scripts/local/molitao-backend-*.tar

# 2. 使用旧 tar 包重新部署
cd backend/scripts/local
bash upload-and-deploy.sh --tar=molitao-backend-YYYYMMDD-HHMMSS.tar
```

## 故障排查

### 1. SSH 连接失败

```bash
# 检查 SSH 配置
cat ~/.ssh/config | grep molitao

# 测试手动连接
ssh -v molitao
```

### 2. 镜像加载失败

```bash
# 在服务器上手动检查
ssh molitao
docker images | grep molitao
docker load -i /data/dotnetapi/xxx.tar
```

### 3. 容器启动失败

```bash
# 查看容器错误日志
ssh molitao 'docker logs molitao-api-production'

# 检查端口占用
ssh molitao 'netstat -tlnp | grep 12580'
```

## 注意事项

1. **打包前确认**: 执行打包前确认代码已提交，避免误发布未提交的代码
2. **验证后再发布**: 建议先在测试环境验证，再发布到生产
3. **保留历史版本**: 保留最近 2-3 个版本的 tar 包，便于回滚
4. **监控部署过程**: 部署过程中注意观察日志输出

## 相关文件

| 文件 | 说明 |
|------|------|
| `backend/scripts/local/build-and-export-docker.sh` | 本地构建打包脚本 |
| `backend/scripts/local/upload-and-deploy.sh` | 本地上传部署脚本 |
| `backend/scripts/local/deploy-to-server.sh` | 旧版一键部署脚本 |
| `backend/scripts/server/deploy.sh` | 服务器容器管理脚本 |
| `backend/scripts/server/load-image.sh` | 服务器镜像加载脚本 |
| `backend/docker-compose-api.yml` | 容器编排配置 |
| `deploy/scripts/deploy.sh` | 腾讯云镜像仓库部署脚本 |

## 更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-04-24 | v1.0 | 初始文档 |
