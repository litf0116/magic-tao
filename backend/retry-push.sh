#!/bin/bash

# Docker Hub 推送重试脚本
# 解决网络超时问题

DOCKER_USERNAME="litengfei0302"
IMAGE_NAME="molitao-backend"
VERSION="latest"
FULL_IMAGE_NAME="$DOCKER_USERNAME/$IMAGE_NAME:$VERSION"

echo "🚀 开始推送镜像到 Docker Hub..."
echo "镜像名称: $FULL_IMAGE_NAME"

# 重试推送函数
retry_push() {
    local max_attempts=3
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        echo "📤 第 $attempt 次尝试推送..."
        
        if docker push $FULL_IMAGE_NAME; then
            echo "✅ 镜像推送成功!"
            return 0
        else
            echo "❌ 第 $attempt 次推送失败"
            if [ $attempt -lt $max_attempts ]; then
                echo "⏳ 等待 10 秒后重试..."
                sleep 10
            fi
            attempt=$((attempt + 1))
        fi
    done
    
    echo "❌ 所有推送尝试都失败了"
    return 1
}

# 检查镜像是否存在
if ! docker images | grep -q "$DOCKER_USERNAME/$IMAGE_NAME"; then
    echo "❌ 错误: 镜像 $FULL_IMAGE_NAME 不存在"
    echo "请先运行构建脚本构建镜像"
    exit 1
fi

# 执行重试推送
if retry_push; then
    echo ""
    echo "🎉 推送完成!"
    echo "你可以通过以下命令拉取镜像:"
    echo "docker pull $FULL_IMAGE_NAME"
    
    # 显示镜像信息
    echo ""
    echo "📊 镜像信息:"
    docker images | grep "$DOCKER_USERNAME/$IMAGE_NAME" | head -5
else
    echo ""
    echo "💡 如果推送继续失败，建议:"
    echo "1. 检查网络连接"
    echo "2. 使用 VPN 或更换网络"
    echo "3. 或者使用镜像导出功能: ./export-image.sh"
fi
