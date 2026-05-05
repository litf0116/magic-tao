#!/bin/bash

# Molitao Backend 自动部署脚本
# 执行方式: cd backend && bash scripts/deploy-to-server.sh
# 流程: 构建 -> 上传 -> 加载 -> 部署

set -e

# 获取脚本所在目录和 backend 目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
SERVER="molitao"
REMOTE_DIR="/data/dotnetapi"
LOGS_DIR="/data2/logs"

echo "=========================================="
echo "开始部署 Molitao Backend"
echo "=========================================="
echo "工作目录: $BACKEND_DIR"

# 切换到 backend 目录
cd "$BACKEND_DIR"

# 步骤1: 调用构建脚本
echo "[1/5] 构建并导出Docker镜像..."
bash scripts/build-and-export-docker.sh

# 获取最新导出的tar文件名
TAR_FILE=$(ls -t molitao-backend-*.tar 2>/dev/null | head -1)

if [ -z "$TAR_FILE" ]; then
    echo "❌ 错误: 未找到导出的tar文件"
    exit 1
fi

echo "找到导出文件: $TAR_FILE"

# 步骤2: 上传到服务器
echo "[2/5] 上传到服务器..."
echo "服务器: $SERVER"
echo "远程目录: $REMOTE_DIR"

scp -o StrictHostKeyChecking=no "$TAR_FILE" "${SERVER}:${REMOTE_DIR}/"

if [ $? -eq 0 ]; then
    echo "✅ 文件上传成功!"
else
    echo "❌ 文件上传失败!"
    exit 1
fi

# 步骤3: 在服务器上创建日志目录
echo "[3/5] 检查并创建日志目录..."
ssh -o StrictHostKeyChecking=no "${SERVER}" "mkdir -p ${LOGS_DIR} && chmod 777 ${LOGS_DIR}"
echo "✅ 日志目录已创建: $LOGS_DIR"

# 步骤4: 在服务器上加载镜像
echo "[4/5] 加载Docker镜像..."
ssh -o StrictHostKeyChecking=no "${SERVER}" "cd ${REMOTE_DIR} && bash load-image.sh ${TAR_FILE} -y"

# 步骤5: 使用docker-compose重新部署
echo "[5/5] 使用docker-compose重新部署..."
ssh -o StrictHostKeyChecking=no "${SERVER}" "cd ${REMOTE_DIR} && docker-compose down && docker-compose up -d"

# 验证部署
echo ""
echo "验证部署..."
sleep 3

CONTAINER_STATUS=$(ssh -o StrictHostKeyChecking=no "${SERVER}" "docker inspect -f '{{.State.Status}}' molitao-api-production 2>/dev/null || echo 'not_found'")

if [ "$CONTAINER_STATUS" = "running" ]; then
    echo "✅ 容器运行成功!"
    
    # 检查日志文件
    LOG_FILE="${LOGS_DIR}/api-$(date +%Y%m%d).log"
    sleep 2
    LOG_EXISTS=$(ssh -o StrictHostKeyChecking=no "${SERVER}" "test -f ${LOG_FILE} && echo 'exists' || echo 'not_found'")
    
    if [ "$LOG_EXISTS" = "exists" ]; then
        echo "✅ 日志文件已创建: $LOG_FILE"
        LOG_LINES=$(ssh -o StrictHostKeyChecking=no "${SERVER}" "wc -l < ${LOG_FILE}")
        echo "   日志行数: $LOG_LINES"
    else
        echo "⚠️ 日志文件未创建，可能需要等待一段时间"
    fi
else
    echo "⚠️ 容器状态: $CONTAINER_STATUS"
fi

echo ""
echo "=========================================="
echo "🎉 部署完成!"
echo "=========================================="
echo "镜像文件: $TAR_FILE"
echo "服务器: $SERVER"
echo "远程路径: ${REMOTE_DIR}/${TAR_FILE}"
echo "日志目录: $LOGS_DIR"
echo ""
echo "查看日志: ssh ${SERVER} 'tail -f ${LOGS_DIR}/api-\$(date +%Y%m%d).log'"
echo "查看容器: ssh ${SERVER} 'docker logs -f molitao-api-production'"
echo "查看状态: ssh ${SERVER} 'docker ps | grep molitao'"
echo "=========================================="
