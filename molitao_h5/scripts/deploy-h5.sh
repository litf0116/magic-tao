#!/bin/bash

# H5 一键部署脚本
# 用法: ./scripts/deploy-h5.sh [服务器地址] [SSH用户] [部署路径]
# 示例: ./scripts/deploy-h5.sh 47.122.22.33 root /www/wwwroot

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}====== H5 部署脚本 ======${NC}"

# 配置
SERVER="${1:-}"
SSH_USER="${2:-root}"
DEPLOY_PATH="${3:-/www/wwwroot}"
H5_DIR="dist/build/h5"

# 检查参数
if [ -z "$SERVER" ]; then
    echo "用法: $0 <服务器地址> [SSH用户] [部署路径]"
    echo "示例: $0 47.122.22.33 root /www/wwwroot"
    echo ""
    echo "参数说明:"
    echo "  服务器地址  - 服务器 IP 或域名 (必需)"
    echo "  SSH用户     - SSH 用户名 (默认: root)"
    echo "  部署路径    - 服务器部署目录 (默认: /www/wwwroot)"
    exit 1
fi

# 1. 构建 H5
echo -e "${YELLOW}[1/4] 构建 H5 应用...${NC}"
cd "$(dirname "$0")/.."
npm run build:h5

# 2. 修复资源路径（相对路径）
echo -e "${YELLOW}[2/4] 修复资源路径...${NC}"
cd "$H5_DIR"

# 修复 index.html 中的资源路径
sed -i '' 's|href="/static/|href="./static/|g' index.html
sed -i '' 's|src="/static/|src="./static/|g' index.html
sed -i '' 's|href="/assets/|href="./assets/|g' index.html
sed -i '' 's|src="/assets/|src="./assets/|g' index.html

# 修复 manifest.webmanifest 中的图标路径
sed -i '' 's|"src": "/static|"src": "./static|g' manifest.webmanifest

# 修复 sw.js 中的缓存路径
if [ -f "sw.js" ]; then
    sed -i '' 's|"/index.html"|"./index.html"|g' sw.js
    sed -i '' 's|"/manifest.webmanifest"|"./manifest.webmanifest"|g' sw.js
    sed -i '' 's|"/static/|"./static/|g' sw.js
fi

# 修复 JS 文件中的资源路径
find assets -name "*.js" -type f 2>/dev/null | while read jsfile; do
    sed -i '' 's|"/static/|"./static/|g' "$jsfile"
    sed -i '' 's|"/assets/|"./assets/|g' "$jsfile"
done

# 修复 CSS 文件中的资源路径
find assets -name "*.css" -type f 2>/dev/null | while read cssfile; do
    sed -i '' 's|url(/static/|url(./static/|g' "$cssfile"
done

cd - > /dev/null

echo -e "${GREEN}  资源路径修复完成${NC}"

# 3. 创建部署目录（远程）
echo -e "${YELLOW}[3/4] 创建远程部署目录...${NC}"
ssh -o StrictHostKeyChecking=no ${SSH_USER}@${SERVER} "mkdir -p ${DEPLOY_PATH}/h5 && echo '目录创建完成'"

# 4. 上传文件
echo -e "${YELLOW}[4/4] 上传文件到服务器...${NC}"
rsync -avz --progress \
    --exclude "*.map" \
    --exclude ".DS_Store" \
    "${H5_DIR}/" \
    ${SSH_USER}@${SERVER}:${DEPLOY_PATH}/h5/

echo -e "${GREEN}====== 部署完成 ======${NC}"
echo ""
echo -e "访问地址: ${GREEN}https://www.molitao.top/h5/${NC}"
echo ""
echo -e "${YELLOW}提示:${NC}"
echo "  - Service Worker 需要 HTTPS 环境"
echo "  - 首次访问后资源会被缓存"
echo "  - 更新后清除浏览器缓存以获取新版本"
