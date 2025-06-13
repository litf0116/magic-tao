#!/bin/bash

echo "=== 停止魔力淘服务 ==="

# 停止并删除容器
echo "停止 Docker 容器..."
docker-compose down

# 检查是否还有相关容器在运行
echo "检查剩余容器..."
docker ps | grep -E "(ttwork-web-host|freeim-imserver)"

echo ""
echo "=== 服务已停止 ===" 