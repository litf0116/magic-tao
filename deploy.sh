#!/bin/bash

# Molitao Backend 部署脚本

set -e

SERVER="molitao"
IMAGE_NAME="litengfei0302/molitao-backend"
CONTAINER_NAME="molitao-api-production"
WORKDIR="/Users/mac/workspace/magic-tao/backend"
LOGS_DIR="/data2/logs"
WWWROOT_DIR="/www/wwwroot/www.molitao.top/wwwroot"
CERT_DIR="/www/certs"

echo "========================================="
echo "开始部署 Molitao Backend"
echo "========================================="

cd "$WORKDIR"

# 1. 构建镜像
echo "[1/5] 构建 Docker 镜像..."
docker build -t $IMAGE_NAME:latest -f src/TtWork.Project.Web.Host/Dockerfile .

# 2. 登录 Docker Hub
echo "[2/5] 登录 Docker Hub..."
docker login || echo "⚠️ Docker Hub 登录失败，请检查凭证"

# 3. 推送镜像
echo "[3/5] 推送镜像到 Docker Hub..."
docker push $IMAGE_NAME:latest

# 4. 部署到服务器
echo "[4/5] 部署到服务器 $SERVER..."

# 检查并创建日志目录
ssh $SERVER "mkdir -p $LOGS_DIR && chmod 777 $LOGS_DIR"
echo "✓ 日志目录已创建: $LOGS_DIR"

# 检查并创建 wwwroot 目录
ssh $SERVER "mkdir -p $WWWROOT_DIR"
echo "✓ wwwroot 目录已创建: $WWWROOT_DIR"

# 检查并创建 cert 目录
ssh $SERVER "mkdir -p $CERT_DIR"
echo "✓ cert 目录已创建: $CERT_DIR"

# 拉取最新镜像
ssh $SERVER "docker pull $IMAGE_NAME:latest"

# 停止并删除旧容器
ssh $SERVER "docker stop $CONTAINER_NAME 2>/dev/null || true"
ssh $SERVER "docker rm $CONTAINER_NAME 2>/dev/null || true"

# 启动新容器
ssh $SERVER "docker run -d \
  --name $CONTAINER_NAME \
  --restart always \
  -p 5000:5000 \
  -v $LOGS_DIR:/app/logs \
  -v $WWWROOT_DIR:/app/wwwroot \
  -v $CERT_DIR:/app/cert \
  -e TZ=Asia/Shanghai \
  -e ASPNETCORE_ENVIRONMENT=Production \
  $IMAGE_NAME:latest"

# 5. 验证部署
echo "[5/5] 验证部署..."
sleep 5

CONTAINER_STATUS=$(ssh $SERVER "docker inspect -f '{{.State.Status}}' $CONTAINER_NAME 2>/dev/null || echo 'not_found'")

if [ "$CONTAINER_STATUS" = "running" ]; then
    echo "✓ 容器运行中"
else
    echo "✗ 容器状态异常: $CONTAINER_STATUS"
    exit 1
fi

LOG_FILES=$(ssh $SERVER "ls -lh $LOGS_DIR/*.log 2>/dev/null | wc -l")
echo "✓ 日志目录: $LOGS_DIR ($LOG_FILES 个日志文件)"

echo ""
echo "========================================="
echo "✓ 部署完成！"
echo "========================================="
echo "服务器: $SERVER"
echo "容器: $CONTAINER_NAME"
echo "日志: $LOGS_DIR"
echo "访问地址: http://8.130.178.251:5000"
echo ""
echo "查看日志: ssh $SERVER 'tail -f $LOGS_DIR/api-$(date +%Y%m%d).log'"
echo "查看容器: ssh $SERVER 'docker logs -f $CONTAINER_NAME'"