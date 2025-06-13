#!/bin/bash

# 魔力淘API Docker镜像构建和导出脚本
# 创建时间: $(date '+%Y-%m-%d %H:%M:%S')

set -e  # 遇到错误时退出

echo "=========================================="
echo "开始构建魔力淘API Docker镜像"
echo "=========================================="

# 定义变量
IMAGE_NAME="gitlab.somall.top:8090/molitao/api:alpha"
TAR_FILE="molitao-api-alpha.tar"
DOCKERFILE_PATH="src/TtWork.Project.Web.Host/Dockerfile"

# 检查Dockerfile是否存在
if [ ! -f "$DOCKERFILE_PATH" ]; then
    echo "错误: Dockerfile 不存在于路径: $DOCKERFILE_PATH"
    exit 1
fi

echo "步骤1: 构建Docker镜像..."
echo "镜像名称: $IMAGE_NAME"
echo "Dockerfile路径: $DOCKERFILE_PATH"

# 构建Docker镜像
docker build -f "$DOCKERFILE_PATH" --network=host -t "$IMAGE_NAME" .

if [ $? -eq 0 ]; then
    echo "✅ Docker镜像构建成功!"
else
    echo "❌ Docker镜像构建失败!"
    exit 1
fi

echo ""
echo "步骤2: 导出Docker镜像为tar包..."
echo "导出文件: $TAR_FILE"

# 如果tar文件已存在，先删除
if [ -f "$TAR_FILE" ]; then
    echo "发现已存在的tar文件，正在删除..."
    rm -f "$TAR_FILE"
fi

# 导出Docker镜像
docker save -o "$TAR_FILE" "$IMAGE_NAME"

if [ $? -eq 0 ]; then
    echo "✅ Docker镜像导出成功!"
    
    # 显示文件信息
    if [ -f "$TAR_FILE" ]; then
        FILE_SIZE=$(ls -lh "$TAR_FILE" | awk '{print $5}')
        echo "导出文件大小: $FILE_SIZE"
    fi
else
    echo "❌ Docker镜像导出失败!"
    exit 1
fi

echo ""
echo "=========================================="
echo "🎉 所有操作完成!"
echo "镜像名称: $IMAGE_NAME"
echo "导出文件: $TAR_FILE"
echo "=========================================="

# 显示Docker镜像信息
echo ""
echo "Docker镜像信息:"
docker images | grep "gitlab.somall.top:8090/molitao/api" || echo "未找到相关镜像" 