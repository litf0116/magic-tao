#!/bin/bash

# 配置变量
DOCKER_USERNAME="litengfei0302"  # 你的 Docker Hub 用户名
IMAGE_NAME="molitao-backend"
VERSION="latest"
FULL_IMAGE_NAME="$DOCKER_USERNAME/$IMAGE_NAME:$VERSION"

echo "开始构建 Docker 镜像..."

# 切换到 backend 目录
cd /Users/mac/workspace/magic-tao/backend

# 构建镜像
docker build -f src/TtWork.Project.Web.Host/Dockerfile -t $FULL_IMAGE_NAME .

if [ $? -eq 0 ]; then
    echo "镜像构建成功: $FULL_IMAGE_NAME"
    
    echo "开始推送镜像到 Docker Hub..."
    
    # 推送镜像
    docker push $FULL_IMAGE_NAME
    
    if [ $? -eq 0 ]; then
        echo "镜像推送成功!"
        echo "你可以通过以下命令拉取镜像:"
        echo "docker pull $FULL_IMAGE_NAME"
    else
        echo "镜像推送失败!"
        exit 1
    fi
else
    echo "镜像构建失败!"
    exit 1
fi
