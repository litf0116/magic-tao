#!/bin/bash

set -e

SERVER="molitao"
REGISTRY="ccr.ccs.tencentyun.com"
NAMESPACE="molitao"
IMAGE_NAME="api"
CONTAINER_NAME="molitao-api-production"
LOGS_DIR="/data2/logs"
WWWROOT_DIR="/www/wwwroot/www.molitao.top/wwwroot"
CERT_DIR="/www/certs"
IMAGE_TAG="${1:-latest}"
FULL_IMAGE="${REGISTRY}/${NAMESPACE}/${IMAGE_NAME}:${IMAGE_TAG}"

echo "========================================="
echo "部署 Molitao Backend"
echo "镜像: ${FULL_IMAGE}"
echo "========================================="

echo "[1/3] 登录腾讯云镜像仓库..."
ssh $SERVER "docker login ${REGISTRY} -u \${TENCENT_CCR_USERNAME} -p \${TENCENT_CCR_PASSWORD}"

echo "[2/3] 拉取镜像..."
ssh $SERVER "docker pull ${FULL_IMAGE}"

echo "[3/3] 部署容器..."

ssh $SERVER "docker stop ${CONTAINER_NAME} 2>/dev/null || true"
ssh $SERVER "docker rm ${CONTAINER_NAME} 2>/dev/null || true"

ssh $SERVER "mkdir -p ${LOGS_DIR} ${WWWROOT_DIR} ${CERT_DIR}"

ssh $SERVER "docker run -d \
  --name ${CONTAINER_NAME} \
  --restart always \
  -p 5000:5000 \
  -v ${LOGS_DIR}:/app/logs \
  -v ${WWWROOT_DIR}:/app/wwwroot \
  -v ${CERT_DIR}:/app/cert \
  -e TZ=Asia/Shanghai \
  -e ASPNETCORE_ENVIRONMENT=Production \
  ${FULL_IMAGE}"

echo ""
echo "========================================="
echo "✓ 部署完成"
echo "镜像: ${FULL_IMAGE}"
echo "容器: ${CONTAINER_NAME}"
echo "日志: ${LOGS_DIR}"
echo "========================================="
