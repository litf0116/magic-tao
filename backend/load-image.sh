#!/bin/bash

# Docker镜像加载脚本
# 用于从tar文件加载镜像到本地Docker环境

set -e  # 遇到错误时退出

echo "=========================================="
echo "开始加载Docker镜像"
echo "=========================================="

# 定义变量
TAR_FILE="molitao-backend-latest.tar"
IMAGE_NAME="litengfei0302/molitao-backend:latest"

# 如果提供了参数，使用参数作为tar文件名
if [ $# -gt 0 ]; then
    TAR_FILE="$1"
    echo "使用指定的tar文件: $TAR_FILE"
fi

# 检查tar文件是否存在
if [ ! -f "$TAR_FILE" ]; then
    echo "❌ 错误: tar文件不存在: $TAR_FILE"
    echo ""
    echo "请确保以下文件之一存在:"
    echo "  - molitao-backend-latest.tar"
    echo "  - molitao-api-alpha.tar (旧版本)"
    echo ""
    echo "如果你有其他名称的tar文件，请作为参数传入:"
    echo "  $0 your-image.tar"
    exit 1
fi

echo "步骤1: 检查现有镜像..."
echo "正在查找现有的molitao相关镜像..."

# 显示现有的相关镜像
EXISTING_IMAGES=$(docker images | grep -E "(molitao|litengfei0302)" || true)
if [ -n "$EXISTING_IMAGES" ]; then
    echo "发现以下相关镜像:"
    echo "$EXISTING_IMAGES"
    echo ""
    # 非交互模式：自动删除现有镜像
    if [ "$AUTO_REMOVE" = "true" ] || [ "$2" = "-y" ] || [ "$2" = "--yes" ]; then
        echo "自动模式：删除现有镜像..."
        docker images | grep -E "(molitao|litengfei0302)" | awk '{print $3}' | xargs -r docker rmi -f || true
    elif [ -t 0 ]; then
        # 仅在交互式终端时提示
        read -p "是否要删除现有镜像并重新加载? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo "正在删除现有镜像..."
            docker images | grep -E "(molitao|litengfei0302)" | awk '{print $3}' | xargs -r docker rmi -f || true
        fi
    else
        echo "非交互模式：跳过删除现有镜像"
    fi
else
    echo "未发现现有的相关镜像"
fi

echo ""
echo "步骤2: 加载Docker镜像..."
echo "tar文件: $TAR_FILE"
echo "文件大小: $(ls -lh "$TAR_FILE" | awk '{print $5}')"

# 加载Docker镜像
echo "正在加载镜像..."
docker load -i "$TAR_FILE"

if [ $? -eq 0 ]; then
    echo "✅ Docker镜像加载成功!"
else
    echo "❌ Docker镜像加载失败!"
    exit 1
fi

echo ""
echo "步骤3: 验证加载的镜像..."

# 显示加载后的镜像信息
echo "已加载的molitao相关镜像:"
docker images | grep -E "(molitao|litengfei0302)" || echo "未发现相关镜像"

echo ""
echo "=========================================="
echo "🎉 镜像加载完成!"
echo "=========================================="

# 显示使用建议
echo ""
echo "💡 接下来可以:"
echo "1. 使用 docker-compose 启动服务:"
echo "   docker-compose -f docker-compose-api.yml up -d"
echo ""
echo "2. 或直接运行容器:"
echo "   docker run -d -p 5000:5000 --name molitao-api $IMAGE_NAME"
echo ""
echo "3. 查看容器日志:"
echo "   docker logs -f molitao-api"
