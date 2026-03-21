#!/bin/bash
# Beta 测试服务 Nginx SSL 配置部署脚本

set -e

echo "=== Beta 测试服务 Nginx SSL 部署脚本 ==="
echo ""

# 检查 root 权限
if [ "$EUID" -ne 0 ]; then
    echo "❌ 需要root权限: sudo bash $0"
    exit 1
fi

DOMAIN="beta.molitao.top"
CERT_DIR="/etc/nginx/ssl/$DOMAIN"
SRC_DIR="/Users/mac/workspace/magic-tao/beta-deployment"

echo "📋 从项目目录部署:"
echo "  源目录: $SRC_DIR"
echo ""

# 1. 创建证书目录
echo "📁 创建证书目录..."
sudo mkdir -p "$CERT_DIR"

# 2. 复制证书文件
echo "📜 安装 SSL 证书..."
sudo cp "$SRC_DIR/ssl-certificates/$DOMAIN.pem" "$CERT_DIR/"
sudo cp "$SRC_DIR/ssl-certificates/$DOMAIN.key" "$CERT_DIR/"
sudo chmod 600 "$CERT_DIR/$DOMAIN.key"

# 3. 部署配置
echo "⚙️ 部署 Nginx 配置..."
sudo cp "$SRC_DIR/nginx-config/$DOMAIN.conf" /etc/nginx/sites-available/
sudo ln -sf /etc/nginx/sites-available/$DOMAIN.conf /etc/nginx/sites-enabled/

# 4. 测试并重启
echo "🧪 测试配置..."
sudo nginx -t
echo "🔄 重启 Nginx..."
sudo systemctl restart nginx

echo ""
echo "=== ✅ 部署完成 ==="
echo "访问: https://$DOMAIN"
