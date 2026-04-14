#!/bin/bash

set -e

TAG="${1:-latest}"

echo "========================================="
echo "一键部署 Molitao Backend"
echo "版本: ${TAG}"
echo "========================================="

# 构建并推送
echo ""
echo "[1/2] 构建并推送镜像..."
"$(dirname "$0")/build-and-push.sh" "$TAG"

# 部署
echo ""
echo "[2/2] 部署到服务器..."
"$(dirname "$0")/deploy.sh" "$TAG"

echo ""
echo "========================================="
echo "✓ 一键部署完成!"
echo "========================================="
