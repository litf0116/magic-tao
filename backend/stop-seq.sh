#!/bin/bash

echo "=== Seq 日志服务器停止脚本 ==="

# 检查容器是否存在
if docker ps -a | grep -q "seq"; then
    echo "停止 Seq 容器..."
    docker stop seq
    
    echo "删除 Seq 容器..."
    docker rm seq
    
    echo "✓ Seq 日志服务器已停止并删除"
else
    echo "未找到 Seq 容器"
fi

# 可选：询问是否删除数据
read -p "是否删除 Seq 数据目录 ~/seq/data? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf ~/seq/data
    echo "✓ Seq 数据目录已删除"
else
    echo "保留 Seq 数据目录"
fi

echo "=== Seq 停止完成 ==="
