#!/bin/bash

# 魔力淘 Backend 上传部署脚本
# 功能: 基于 tar 包上传到服务器并自动部署
# 依赖: 需要先执行 build-and-export-docker.sh 生成 tar 包

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SERVER="molitao"
REMOTE_DIR="/data/dotnetapi"
LOGS_DIR="/data2/logs"
CONTAINER_NAME="molitao-api-production"

show_help() {
    echo "魔力淘 Backend 上传部署脚本"
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  --tar=FILE      指定 tar 包路径（默认: 自动查找最新的）"
    echo "  --server=HOST    服务器别名（默认: molitao）"
    echo "  --skip-build    跳过构建步骤（假设 tar 已存在）"
    echo "  --help          显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0                                    # 自动查找最新 tar 包并部署"
    echo "  $0 --tar=molitao-backend-20250424.tar  # 使用指定 tar 包"
    echo "  $0 --server=backup-server             # 部署到备用服务器"
}

parse_args() {
    TAR_FILE=""
    SKIP_BUILD=false

    for arg in "$@"; do
        case $arg in
            --tar=*)
                TAR_FILE="${arg#*=}"
                ;;
            --server=*)
                SERVER="${arg#*=}"
                ;;
            --skip-build)
                SKIP_BUILD=true
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
        esac
    done

    echo "TAR_FILE=$TAR_FILE"
    echo "SERVER=$SERVER"
    echo "SKIP_BUILD=$SKIP_BUILD"
}

find_latest_tar() {
    local pattern="$SCRIPT_DIR/molitao-backend-*.tar"
    local files=($(ls -t $pattern 2>/dev/null))

    if [ ${#files[@]} -eq 0 ]; then
        echo "错误: 未找到 tar 包，请先执行 build-and-export-docker.sh"
        exit 1
    fi

    TAR_FILE="${files[0]}"
    echo "自动找到最新 tar 包: $TAR_FILE"
}

check_tar_file() {
    if [ -z "$TAR_FILE" ]; then
        find_latest_tar
    fi

    if [ ! -f "$TAR_FILE" ]; then
        echo "错误: tar 文件不存在: $TAR_FILE"
        exit 1
    fi

    FILE_SIZE=$(ls -lh "$TAR_FILE" | awk '{print $5}')
    echo "使用 tar 包: $TAR_FILE (大小: $FILE_SIZE)"
}

check_server() {
    echo ""
    echo "检查服务器连接..."
    if ! ssh -o ConnectTimeout=5 -o BatchMode=yes "$SERVER" "echo '连接成功'" 2>/dev/null; then
        echo "错误: 无法连接到服务器 $SERVER"
        echo "请检查:"
        echo "  1. SSH 密钥配置"
        echo "  2. 服务器地址是否正确"
        exit 1
    fi
    echo "✅ 服务器连接正常"
}

upload_tar() {
    echo ""
    echo "[1/4] 上传 tar 包到服务器..."
    echo "服务器: $SERVER"
    echo "远程目录: $REMOTE_DIR"

    ssh -o StrictHostKeyChecking=no "$SERVER" "mkdir -p ${REMOTE_DIR}"

    scp -o StrictHostKeyChecking=no "$TAR_FILE" "${SERVER}:${REMOTE_DIR}/"

    if [ $? -eq 0 ]; then
        echo "✅ tar 包上传成功"
    else
        echo "错误: tar 包上传失败"
        exit 1
    fi
}

create_directories() {
    echo ""
    echo "[2/4] 创建服务器目录..."

    ssh -o StrictHostKeyChecking=no "$SERVER" "mkdir -p ${LOGS_DIR} && chmod 777 ${LOGS_DIR}"

    echo "✅ 目录创建完成"
    echo "   日志目录: $LOGS_DIR"
}

load_image() {
    echo ""
    echo "[3/4] 加载 Docker 镜像..."

    REMOTE_TAR="${REMOTE_DIR}/$(basename $TAR_FILE)"

    ssh -o StrictHostKeyChecking=no "$SERVER" << SSH_EOF
        cd ${REMOTE_DIR}

        # 停止并删除旧容器（如果存在）
        echo "停止旧容器..."
        docker stop ${CONTAINER_NAME} 2>/dev/null || true
        docker rm ${CONTAINER_NAME} 2>/dev/null || true

        # 停止并删除旧容器（alpha环境）
        docker stop molitao-api-alpha 2>/dev/null || true
        docker rm molitao-api-alpha 2>/dev/null || true

        # 删除旧镜像（如果存在）
        echo "删除旧镜像..."
        docker rmi litengfei0302/molitao-backend:latest 2>/dev/null || true

        # 加载新镜像
        echo "加载镜像: ${REMOTE_TAR}"
        docker load -i "${REMOTE_TAR}"

        # 验证镜像
        echo "验证镜像..."
        docker images | grep litengfei0302/molitao-backend || echo "警告: 镜像加载可能失败"

        echo "镜像加载完成"
SSH_EOF

    if [ $? -eq 0 ]; then
        echo "✅ Docker 镜像加载成功"
    else
        echo "错误: Docker 镜像加载失败"
        exit 1
    fi
}

deploy_container() {
    echo ""
    echo "[4/4] 部署容器..."

    ssh -o StrictHostKeyChecking=no "$SERVER" << SSH_EOF
        cd ${REMOTE_DIR}

        # 检查 docker-compose 文件是否存在
        if [ ! -f "docker-compose-api.yml" ]; then
            echo "警告: docker-compose-api.yml 不存在，尝试直接运行容器..."

            docker run -d \
                --name ${CONTAINER_NAME} \
                --restart always \
                -p 12580:5000 \
                -v ${LOGS_DIR}:/app/logs \
                -e TZ=Asia/Shanghai \
                -e ASPNETCORE_ENVIRONMENT=Production \
                litengfei0302/molitao-backend:latest

            echo "容器启动命令已执行"
        else
            echo "使用 docker-compose 部署..."
            docker-compose -f docker-compose-api.yml up -d production
        fi

        # 等待容器启动
        sleep 3

        # 检查容器状态
        CONTAINER_STATUS=\$(docker inspect -f '{{.State.Status}}' ${CONTAINER_NAME} 2>/dev/null || echo 'not_found')
        echo "容器状态: \$CONTAINER_STATUS"

        if [ "\$CONTAINER_STATUS" = "running" ]; then
            echo "✅ 容器运行正常"
        else
            echo "⚠️ 容器状态异常，请检查日志"
            docker logs ${CONTAINER_NAME} --tail 50
        fi
SSH_EOF

    if [ $? -eq 0 ]; then
        echo "✅ 部署命令执行成功"
    else
        echo "⚠️ 部署命令执行完成，请检查服务器状态"
    fi
}

verify_deployment() {
    echo ""
    echo "=========================================="
    echo "验证部署结果"
    echo "=========================================="

    ssh -o StrictHostKeyChecking=no "$SERVER" << SSH_EOF
        CONTAINER_STATUS=\$(docker inspect -f '{{.State.Status}}' ${CONTAINER_NAME} 2>/dev/null || echo 'not_found')
        echo "容器状态: \$CONTAINER_STATUS"

        if [ "\$CONTAINER_STATUS" = "running" ]; then
            echo "✅ 容器运行正常!"

            # 检查端口
            echo ""
            echo "端口监听检查:"
            netstat -tlnp 2>/dev/null | grep 12580 || ss -tlnp 2>/dev/null | grep 12580 || echo "端口检查命令不可用"

            # 检查日志
            echo ""
            echo "最近日志 (最后 10 行):"
            if [ -d "${LOGS_DIR}" ]; then
                ls -la ${LOGS_DIR}/api-*.log 2>/dev/null | tail -1 | awk '{print $NF}' | xargs tail -10 2>/dev/null || echo "暂无日志文件"
            fi
        else
            echo "⚠️ 容器未正常运行"
            echo ""
            echo "错误日志:"
            docker logs ${CONTAINER_NAME} --tail 30 2>&1 || echo "无法获取日志"
        fi
SSH_EOF
}

show_summary() {
    echo ""
    echo "=========================================="
    echo "部署完成!"
    echo "=========================================="
    echo "tar 包: $(basename $TAR_FILE)"
    echo "服务器: $SERVER"
    echo "远程路径: ${REMOTE_DIR}/$(basename $TAR_FILE)"
    echo ""
    echo "常用命令:"
    echo "  查看容器: ssh $SERVER 'docker ps | grep molitao'"
    echo "  查看日志: ssh $SERVER 'docker logs -f ${CONTAINER_NAME}'"
    echo "  重启服务: ssh $SERVER 'docker restart ${CONTAINER_NAME}'"
    echo "  进入容器: ssh $SERVER 'docker exec -it ${CONTAINER_NAME} /bin/bash'"
    echo "=========================================="
}

main() {
    echo "=========================================="
    echo "魔力淘 Backend 上传部署脚本"
    echo "=========================================="

    parse_args "$@"

    if [ "$SKIP_BUILD" = false ]; then
        echo ""
        echo "提示: 如果需要跳过构建，请使用 --skip-build 选项"
        echo "      假设你已经执行过 build-and-export-docker.sh"
    fi

    check_tar_file
    check_server
    upload_tar
    create_directories
    load_image
    deploy_container
    verify_deployment
    show_summary
}

main "$@"
