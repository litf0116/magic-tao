#!/bin/bash
# 构建并推送镜像到腾讯云 TCR
# 使用方式: 
#   ./scripts/build-and-push.sh              # 使用 latest
#   ./scripts/build-and-push.sh v1.0.0       # 使用指定版本号
#   ./scripts/build-and-push.sh 20250405      # 使用日期作为版本号
#   GITEE_BUILD_INDEX=123 ./scripts/build-and-push.sh  # 使用 Gitee CI 的构建号

set -e

# 配置
REGISTRY="ccr.ccs.tencentyun.com"
NAMESPACE="molitao"
IMAGE_NAME="api"
PROJECT_DIR="backend"

# 读取配置
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CONFIG_FILE="${SCRIPT_DIR}/../credentials.conf"

if [ -f "${CONFIG_FILE}" ]; then
    source "${CONFIG_FILE}"
fi

# 飞书通知函数
send_feishu_notification() {
    local status=$1
    local branch=${GITEE_GIT_BRANCH:-$(git branch --show-current 2>/dev/null || echo "local")}
    local commit=${GITEE_GIT_COMMIT:-$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")}
    local build_time=$(date '+%Y-%m-%d %H:%M:%S')
    
    if [ -z "${FEISHU_WEBHOOK}" ]; then
        echo "未配置飞书 Webhook，跳过通知"
        return
    fi
    
    curl -X POST \
        -H "Content-Type: application/json" \
        -d "{
          \"msg_type\": \"interactive\",
          \"card\": {
            \"header\": {
              \"title\": {
                \"tag\": \"plain_text\",
                \"content\": \"🚀 构建通知\"
              },
              \"template\": \"$([ \"$status\" = \"成功\" ] && echo \"green\" || echo \"red\")\"
            },
            \"elements\": [
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**项目**: molitao 后端服务\"
                }
              },
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**状态**: $status\"
                }
              },
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**镜像**: ${FULL_IMAGE}\"
                }
              },
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**分支**: ${branch}\"
                }
              },
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**Commit**: ${commit}\"
                }
              },
              {
                \"tag\": \"div\",
                \"text\": {
                  \"tag\": \"lark_md\",
                  \"content\": \"**时间**: ${build_time}\"
                }
              }
            ]
          }
        }" \
        "${FEISHU_WEBHOOK}" 2>/dev/null
}

# 版本号设置
BUILD_TAG="${1:-}"
GITEE_BUILD="${GITEE_BUILD_INDEX:-}"

if [ -n "${GITEE_BUILD}" ]; then
    # 如果有 Gitee CI 构建号，使用它
    TAG="${GITEE_BUILD}"
elif [ -n "${BUILD_TAG}" ]; then
    # 使用命令行传入的版本号
    TAG="${BUILD_TAG}"
else
    # 默认使用 latest
    TAG="latest"
fi

# 完整镜像地址
FULL_IMAGE="${REGISTRY}/${NAMESPACE}/${IMAGE_NAME}:${TAG}"
FULL_IMAGE_LATEST="${REGISTRY}/${NAMESPACE}/${IMAGE_NAME}:latest"

echo "=========================================="
echo "构建并推送镜像到腾讯云 TCR"
echo "=========================================="
echo "镜像地址: ${FULL_IMAGE}"
echo ""

# 检查配置
if [ -z "${TENCENT_CCR_USERNAME}" ] || [ -z "${TENCENT_CCR_PASSWORD}" ] || [ -z "${TENCENT_CCR_NAMESPACE}" ]; then
    echo "错误: 缺少必要的配置信息"
    echo "请确保 credentials.conf 中包含:"
    echo "  TENCENT_CCR_USERNAME"
    echo "  TENCENT_CCR_PASSWORD"
    echo "  TENCENT_CCR_NAMESPACE"
    exit 1
fi

# 登录镜像仓库
echo "登录腾讯云镜像仓库..."
docker login ${REGISTRY} -u "${TENCENT_CCR_USERNAME}" --password-stdin <<< "${TENCENT_CCR_PASSWORD}"

# 构建镜像
echo ""
echo "开始构建 Docker 镜像..."
cd "${SCRIPT_DIR}/../${PROJECT_DIR}"
docker build \
  --no-cache \
  -f ./src/TtWork.Project.Web.Host/Dockerfile \
  --build-arg HTTP_PROXY=http://192.168.3.50:10809 \
  --build-arg HTTPS_PROXY=http://192.168.3.50:10809 \
  -t ${FULL_IMAGE} .

# 推送镜像
echo ""
echo "推送镜像到仓库..."
docker push ${FULL_IMAGE}

# 同时推送 latest（如果不是 latest）
if [ "${TAG}" != "latest" ]; then
    docker tag ${FULL_IMAGE} ${FULL_IMAGE_LATEST}
    docker push ${FULL_IMAGE_LATEST}
    echo "同时推送了 latest 标签"
fi

echo ""
echo "=========================================="
echo "完成!"
echo "镜像地址: ${FULL_IMAGE}"
echo "=========================================="

# 发送飞书通知
send_feishu_notification "✅ 成功"

# 登出
docker logout ${REGISTRY} 2>/dev/null || true
