#!/bin/bash

# Docker 镜像导出脚本
# 将构建好的镜像导出为 tar 文件，方便传输和部署

DOCKER_USERNAME="litengfei0302"
IMAGE_NAME="molitao-backend"
VERSION="latest"
FULL_IMAGE_NAME="$DOCKER_USERNAME/$IMAGE_NAME:$VERSION"
EXPORT_FILE="molitao-backend-${VERSION}.tar"

echo "📦 开始导出 Docker 镜像..."
echo "镜像名称: $FULL_IMAGE_NAME"
echo "导出文件: $EXPORT_FILE"

# 检查镜像是否存在
if ! docker images | grep -q "$DOCKER_USERNAME/$IMAGE_NAME"; then
    echo "❌ 错误: 镜像 $FULL_IMAGE_NAME 不存在"
    echo "请先运行构建脚本构建镜像"
    exit 1
fi

# 删除已存在的导出文件
if [ -f "$EXPORT_FILE" ]; then
    echo "🗑️  删除现有的导出文件..."
    rm "$EXPORT_FILE"
fi

# 导出镜像
echo "📤 正在导出镜像..."
if docker save -o "$EXPORT_FILE" "$FULL_IMAGE_NAME"; then
    echo "✅ 镜像导出成功!"
    
    # 显示文件信息
    if [ -f "$EXPORT_FILE" ]; then
        FILE_SIZE=$(ls -lh "$EXPORT_FILE" | awk '{print $5}')
        echo "📊 导出文件大小: $FILE_SIZE"
        echo "📁 导出文件路径: $(pwd)/$EXPORT_FILE"
    fi
    
    echo ""
    echo "🎉 导出完成!"
    echo ""
    echo "💡 使用方法:"
    echo "1. 将 $EXPORT_FILE 文件传输到目标服务器"
    echo "2. 在目标服务器上运行: docker load -i $EXPORT_FILE"
    echo "3. 运行容器: docker run -d -p 5000:5000 $FULL_IMAGE_NAME"
else
    echo "❌ 镜像导出失败!"
    exit 1
fi
