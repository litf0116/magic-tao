# 魔力淘 Docker 部署指南

## 概述

本项目包含两个主要服务：
- **TtWork.Project.Web.Host**: 主要的 Web API 服务 (端口: 5000)
- **FreeIM ImServer**: 即时通讯服务 (端口: 6001)

## 构建的镜像

- `ttwork-web-host:latest` - 主 Web API 服务
- `freeim-imserver:latest` - 即时通讯服务

## 快速启动

### 方法一：使用启动脚本（推荐）

```bash
# 启动所有服务
./start-services.sh

# 停止所有服务
./stop-services.sh
```

### 方法二：使用 docker-compose

```bash
# 启动服务
docker-compose up -d

# 停止服务
docker-compose down

# 查看日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f ttwork-web-host
docker-compose logs -f freeim-imserver
```

### 方法三：直接运行容器

```bash
# 运行主 Web API 服务
docker run -d --name ttwork-web-host -p 5000:5000 ttwork-web-host:latest

# 运行即时通讯服务
docker run -d --name freeim-imserver -p 6001:6001 freeim-imserver:latest
```

## 服务访问地址

- **主 Web API 服务**: http://localhost:5000
- **即时通讯服务**: http://localhost:6001

## 重新构建镜像

如果代码有更新，需要重新构建镜像：

```bash
# 重新构建主 Web API 服务镜像
docker build -f src/TtWork.Project.Web.Host/Dockerfile -t ttwork-web-host:latest .

# 重新构建即时通讯服务镜像
docker build -f FreeIM/ImServer/Dockerfile -t freeim-imserver:latest .
```

## 目录结构

```
backend/
├── docker-compose.yml          # Docker Compose 配置文件
├── start-services.sh           # 启动服务脚本
├── stop-services.sh            # 停止服务脚本
├── src/TtWork.Project.Web.Host/Dockerfile  # 主服务 Dockerfile
├── FreeIM/ImServer/Dockerfile  # IM 服务 Dockerfile
├── cert/                       # 证书目录（挂载到容器）
└── wwwroot/                    # 静态文件目录（挂载到容器）
```

## 常用命令

```bash
# 查看运行中的容器
docker ps

# 查看所有镜像
docker images

# 删除容器
docker rm -f ttwork-web-host freeim-imserver

# 删除镜像
docker rmi ttwork-web-host:latest freeim-imserver:latest

# 进入容器内部
docker exec -it ttwork-web-host /bin/bash
docker exec -it freeim-imserver /bin/bash
```

## 故障排除

### 端口冲突
如果端口被占用，可以修改 `docker-compose.yml` 中的端口映射：

```yaml
ports:
  - "5001:5000"  # 将主机端口改为 5001
```

### 权限问题
如果遇到权限问题，确保 `cert` 和 `wwwroot` 目录有正确的权限：

```bash
sudo chmod -R 777 cert wwwroot
```

### 查看详细日志
```bash
# 查看容器详细日志
docker logs ttwork-web-host
docker logs freeim-imserver

# 实时查看日志
docker logs -f ttwork-web-host
```

## 生产环境部署建议

1. **环境变量配置**: 在 `docker-compose.yml` 中配置生产环境的环境变量
2. **数据持久化**: 配置数据库和文件存储的持久化卷
3. **反向代理**: 使用 Nginx 或其他反向代理服务器
4. **SSL 证书**: 配置 HTTPS 证书
5. **监控和日志**: 配置日志收集和监控系统 