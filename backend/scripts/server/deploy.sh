#!/bin/bash

# 魔力淘API部署管理脚本
# 提供完整的部署流程管理

set -e

COMPOSE_FILE="docker-compose-api.yml"
IMAGE_NAME="litengfei0302/molitao-backend:latest"
TAR_FILE="molitao-backend-latest.tar"

show_help() {
    echo "魔力淘API部署管理脚本"
    echo ""
    echo "用法: $0 [命令] [选项]"
    echo ""
    echo "命令:"
    echo "  load        从tar文件加载镜像"
    echo "  start       启动服务 (默认alpha环境)"
    echo "  stop        停止服务"
    echo "  restart     重启服务"
    echo "  logs        查看日志"
    echo "  status      查看服务状态"
    echo "  clean       清理停止的容器和未使用的镜像"
    echo "  help        显示此帮助信息"
    echo ""
    echo "选��:"
    echo "  --env=[alpha|production]  指定环境 (默认: alpha)"
    echo "  --file=FILE              指定tar文件路径"
    echo ""
    echo "示例:"
    echo "  $0 load                        # 加载默认tar文件"
    echo "  $0 load --file=custom.tar      # 加载指定tar文件"
    echo "  $0 start                       # 启动alpha环境"
    echo "  $0 start --env=production      # 启动生产环境"
    echo "  $0 logs --env=alpha            # 查看alpha环境日志"
}

parse_args() {
    ENV="alpha"
    CUSTOM_TAR=""
    
    for arg in "$@"; do
        case $arg in
            --env=*)
                ENV="${arg#*=}"
                ;;
            --file=*)
                CUSTOM_TAR="${arg#*=}"
                ;;
        esac
    done
}

load_image() {
    echo "🔄 加载Docker镜像..."
    
    local tar_file="$TAR_FILE"
    if [ -n "$CUSTOM_TAR" ]; then
        tar_file="$CUSTOM_TAR"
    fi
    
    if [ -f "./load-image.sh" ]; then
        ./load-image.sh "$tar_file"
    else
        echo "❌ 未找到 load-image.sh 脚本"
        exit 1
    fi
}

start_service() {
    echo "🚀 启动服务 (环境: $ENV)..."
    
    # 检查镜像是否存在
    if ! docker images | grep -q "litengfei0302/molitao-backend"; then
        echo "⚠️  未找到镜像，尝试加载..."
        load_image
    fi
    
    # 启动指定环境的服务
    docker-compose -f "$COMPOSE_FILE" up -d "$ENV"
    
    echo "✅ 服务启动���成!"
    echo ""
    show_status
}

stop_service() {
    echo "🛑 停止服务 (环境: $ENV)..."
    docker-compose -f "$COMPOSE_FILE" stop "$ENV"
    echo "✅ 服务已停止!"
}

restart_service() {
    echo "🔄 重启服务 (环境: $ENV)..."
    docker-compose -f "$COMPOSE_FILE" restart "$ENV"
    echo "✅ 服务重启完成!"
}

show_logs() {
    echo "📋 查看服务日志 (环境: $ENV)..."
    docker-compose -f "$COMPOSE_FILE" logs -f "$ENV"
}

show_status() {
    echo "📊 服务状态:"
    docker-compose -f "$COMPOSE_FILE" ps
    echo ""
    echo "🐳 Docker镜像:"
    docker images | grep -E "(litengfei0302/molitao-backend|REPOSITORY)" || echo "未找到相关镜像"
    echo ""
    echo "🌐 服务访问地址:"
    echo "  Alpha环境:      http://localhost:5001"
    echo "  Production环境: http://localhost:5000"
}

clean_docker() {
    echo "🧹 清理Docker资源..."
    
    # 停止所有相关容器
    docker-compose -f "$COMPOSE_FILE" down
    
    # 清理停止的容器
    docker container prune -f
    
    # 清理未使用的镜像
    docker image prune -f
    
    echo "✅ 清理完成!"
}

# 主程序
case "$1" in
    load)
        parse_args "$@"
        load_image
        ;;
    start)
        parse_args "$@"
        start_service
        ;;
    stop)
        parse_args "$@"
        stop_service
        ;;
    restart)
        parse_args "$@"
        restart_service
        ;;
    logs)
        parse_args "$@"
        show_logs
        ;;
    status)
        parse_args "$@"
        show_status
        ;;
    clean)
        clean_docker
        ;;
    help|--help|-h)
        show_help
        ;;
    "")
        show_help
        ;;
    *)
        echo "❌ 未知命令: $1"
        echo "使用 '$0 help' 查看帮助信息"
        exit 1
        ;;
esac
