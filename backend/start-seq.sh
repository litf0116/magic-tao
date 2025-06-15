#!/bin/bash

echo "=== Seq 日志服务器启动脚本 ==="

# 检查 Docker 是否运行
if ! docker info > /dev/null 2>&1; then
    echo "错误: Docker 未运行，请先启动 Docker"
    exit 1
fi

# 创建数据目录
echo "创建 Seq 数据目录..."
mkdir -p ~/seq/data

# 停止并删除可能存在的 Seq 容器
echo "清理现有 Seq 容器..."
docker stop seq 2>/dev/null || true
docker rm seq 2>/dev/null || true

# 生成密码哈希
echo "生成管理员密码哈希..."
PH=$(echo 'www_molitao_top' | docker run --rm -i datalust/seq config hash)

if [ -z "$PH" ]; then
    echo "错误: 密码哈希生成失败"
    exit 1
fi

echo "密码哈希生成成功"

# 启动 Seq 容器
echo "启动 Seq 日志服务器..."
docker run \
  --name seq \
  -d \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$PH" \
  -v ~/seq/data:/data \
  -p 5341:80 \
  datalust/seq

# 检查启动状态
if [ $? -eq 0 ]; then
    echo "✓ Seq 日志服务器启动成功"
    
    # 等待服务完全启动
    echo "等待服务启动..."
    sleep 10
    
    # 检查容器状态
    if docker ps | grep -q "seq"; then
        echo "✓ Seq 容器运行正常"
        echo ""
        echo "=== 服务信息 ==="
        echo "访问地址: http://localhost:5341"
        echo "管理员用户名: admin"
        echo "管理员密码: www_molitao_top"
        echo "数据存储位置: ~/seq/data"
        echo ""
        echo "=== 常用命令 ==="
        echo "查看日志: docker logs -f seq"
        echo "停止服务: docker stop seq"
        echo "删除容器: docker rm seq"
        echo "重启服务: docker restart seq"
        echo ""
    else
        echo "✗ Seq 容器启动失败"
        echo "查看错误日志: docker logs seq"
        exit 1
    fi
else
    echo "✗ Seq 日志服务器启动失败"
    exit 1
fi

echo "=== Seq 启动完成 ==="
