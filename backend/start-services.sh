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
mkdir -p ~/seq/data

# 停止并删除现有容器
echo "停止现有容器..."
docker-compose down

# 停止并删除可能存在的 Seq 容器
echo "停止现有 Seq 容器..."
docker stop seq 2>/dev/null || true
docker rm seq 2>/dev/null || true

# 启动 Seq 日志服务器
echo "启动 Seq 日志服务器..."
PH=$(echo 'www_molitao_top' | docker run --rm -i datalust/seq config hash)
docker run \
  --name seq \
  -d \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$PH" \
  -v ~/seq/data:/data \
  -p 5341:80 \
  datalust/seq

if [ $? -eq 0 ]; then
    echo "✓ Seq 日志服务器启动成功"
else
    echo "✗ Seq 日志服务器启动失败"
fi

# 启动服务
echo "启动主要服务..."
docker-compose up -d

# 检查服务状态
echo "检查服务状态..."
sleep 5
docker-compose ps

echo ""
echo "=== 服务启动完成 ==="
echo "TtWork Web Host: http://localhost:5000"
echo "FreeIM ImServer: http://localhost:6001"
echo "Seq 日志服务器: http://localhost:5341 (用户名: admin, 密码: www_molitao_top)"
echo ""
echo "使用以下命令查看日志:"
echo "  docker-compose logs -f ttwork-web-host"
echo "  docker-compose logs -f freeim-imserver"
echo "  docker logs -f seq"
echo ""
echo "使用以下命令停止服务:"
echo "  docker-compose down"
echo "  docker stop seq && docker rm seq"
