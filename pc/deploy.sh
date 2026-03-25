#!/bin/bash

# PC 前端部署脚本
# 用法: ./deploy.sh [环境]
# 环境: production (默认) | beta

set -e

# 配置
SERVER="molitao"
WORKDIR="/Users/mac/workspace/magic-tao/pc"
DIST_DIR="$WORKDIR/dist"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# 根据参数选择环境
ENV=${1:-production}

case $ENV in
    production)
        DEPLOY_DIR="/www/wwwroot/www.molitao.top"
        DOMAIN="www.molitao.top"
        ;;
    beta)
        DEPLOY_DIR="/www/wwwroot/beta.molitao.top"
        DOMAIN="beta.molitao.top"
        ;;
    *)
        echo "❌ 未知环境: $ENV"
        echo "用法: $0 [production|beta]"
        exit 1
        ;;
esac

BACKUP_DIR="/www/backups/pc/$TIMESTAMP"

echo "========================================="
echo "PC 前端部署脚本"
echo "========================================="
echo "环境: $ENV"
echo "服务器: $SERVER"
echo "部署目录: $DEPLOY_DIR"
echo "域名: $DOMAIN"
echo ""

cd "$WORKDIR"

# 1. 代码检查
echo "[1/6] 代码检查..."
if ! git diff --quiet; then
    echo "⚠️  存在未提交的更改:"
    git status --short
    read -p "是否继续部署? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ 部署已取消"
        exit 1
    fi
fi
echo "✓ 代码检查完成"

# 2. 安装依赖
echo "[2/6] 检查依赖..."
if [ ! -d "node_modules" ] || [ "package.json" -nt "node_modules" ]; then
    echo "安装依赖..."
    npm install
fi
echo "✓ 依赖就绪"

# 3. 构建项目
echo "[3/6] 构建项目..."
echo "清理旧构建..."
rm -rf "$DIST_DIR"

echo "执行构建..."
npm run build

if [ ! -d "$DIST_DIR" ]; then
    echo "❌ 构建失败: dist 目录不存在"
    exit 1
fi

# 统计构建产物
FILE_COUNT=$(find "$DIST_DIR" -type f | wc -l)
TOTAL_SIZE=$(du -sh "$DIST_DIR" | cut -f1)
echo "✓ 构建完成: $FILE_COUNT 个文件, 总大小 $TOTAL_SIZE"

# 4. 备份服务器旧版本
echo "[4/6] 备份旧版本..."
ssh $SERVER "mkdir -p /www/backups/pc"

if ssh $SERVER "[ -d $DEPLOY_DIR ] && [ \"\$(ls -A $DEPLOY_DIR 2>/dev/null)\" ]"; then
    ssh $SERVER "mkdir -p $BACKUP_DIR && cp -r $DEPLOY_DIR/* $BACKUP_DIR/"
    echo "✓ 已备份到: $BACKUP_DIR"
else
    echo "⚠️  目标目录为空或不存在，跳过备份"
fi

# 5. 部署到服务器
echo "[5/6] 部署到服务器..."

# 确保目录存在
ssh $SERVER "mkdir -p $DEPLOY_DIR"

# 删除旧文件（保留配置文件）
echo "清理旧文件..."
ssh $SERVER "cd $DEPLOY_DIR && rm -rf assets images index.html vite.svg 2>/dev/null || true"

# 上传新文件
echo "上传新文件..."
scp -r "$DIST_DIR"/* $SERVER:$DEPLOY_DIR/

if [ $? -ne 0 ]; then
    echo "❌ 上传失败"
    exit 1
fi

echo "✓ 文件上传完成"

# 6. 验证部署
echo "[6/6] 验证部署..."

# 检查文件数量
REMOTE_FILE_COUNT=$(ssh $SERVER "find $DEPLOY_DIR -type f | wc -l")
echo "✓ 服务器文件数: $REMOTE_FILE_COUNT"

# 检查 HTTP 状态
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://$DOMAIN/ 2>/dev/null || echo "000")

if [ "$HTTP_STATUS" = "200" ]; then
    echo "✓ 网站响应正常 (HTTP $HTTP_STATUS)"
else
    echo "⚠️  网站响应异常 (HTTP $HTTP_STATUS)"
fi

echo ""
echo "========================================="
echo "✓ 部署完成！"
echo "========================================="
echo "环境: $ENV"
echo "域名: https://$DOMAIN"
echo "部署目录: $DEPLOY_DIR"
echo "备份目录: $BACKUP_DIR"
echo ""
echo "回滚命令: ssh $SERVER 'cp -r $BACKUP_DIR/* $DEPLOY_DIR/'"
echo "查看日志: ssh $SERVER 'tail -f /var/log/nginx/error.log'"