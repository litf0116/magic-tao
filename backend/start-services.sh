#!/bin/bash

echo "=== 魔力淘服务启动脚本 ==="

# 检查 Docker 是否运行
if ! docker info > /dev/null 2>&1; then
    echo "错误: Docker 未运行，请先启动 Docker"
    exit 1
fi

# 创建必要的目录
echo "创建必要的目录..."
mkdir -p cert
mkdir -p wwwroot

# 停止并删除现有容器
echo "停止现有容器..."
docker-compose down

# 启动服务
echo "启动服务..."
docker-compose up -d

# 检查服务状态
echo "检查服务状态..."
sleep 5
docker-compose ps

echo ""
echo "=== 服务启动完成 ==="
echo "TtWork Web Host: http://localhost:5000"
echo "FreeIM ImServer: http://localhost:6001"
echo ""
echo "使用以下命令查看日志:"
echo "  docker-compose logs -f ttwork-web-host"
echo "  docker-compose logs -f freeim-imserver"
echo ""
echo "使用以下命令停止服务:"
echo "  docker-compose down"
