#!/bin/bash

set -e

SERVER="molitao"
NGINX_CONF_PATH="/www/server/panel/vhost/nginx/www.molitao.top.conf"
BACKUP_PATH="/www/backups/nginx/www.molitao.top.conf.$(date +%Y%m%d_%H%M%S)"

echo "========================================="
echo "Nginx 缓存配置更新脚本"
echo "========================================="
echo ""

echo "[1/4] 备份当前配置..."
ssh $SERVER "mkdir -p /www/backups/nginx && cp $NGINX_CONF_PATH $BACKUP_PATH"
echo "✓ 已备份到: $BACKUP_PATH"
echo ""

echo "[2/4] 检查当前配置问题..."
CACHE_HEADER=$(ssh $SERVER "grep -c 'location = /index.html' $NGINX_CONF_PATH 2>/dev/null || echo 0")
if [ "$CACHE_HEADER" -gt 0 ]; then
    echo "✓ 已存在 index.html 缓存配置"
    echo ""
    echo "当前 index.html 缓存配置："
    ssh $SERVER "sed -n '/location = \/index.html/,/}/p' $NGINX_CONF_PATH"
    echo ""
    read -p "是否更新配置? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "已取消"
        exit 0
    fi
else
    echo "⚠️  缺少 index.html 缓存配置（这是白屏问题的原因）"
fi
echo ""

echo "[3/4] 更新配置..."
echo ""
echo "请在宝塔面板中手动添加以下配置："
echo ""
echo "位置: 网站 → www.molitao.top → 设置 → 配置文件"
echo ""
echo "在 'location /' 之前添加："
echo ""
echo "----------------------------------------"
cat << 'EOF'
    # index.html 禁止缓存
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }

    # JS/CSS 长期缓存
    location ~* \.(?:css|js)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    # 静态资源长期缓存
    location ~* \.(?:jpg|jpeg|gif|png|ico|svg|webp|woff|woff2|ttf|eot)$ {
        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }
EOF
echo "----------------------------------------"
echo ""

read -p "已在宝塔面板添加配置? (y/N) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "已取消"
    exit 0
fi

echo ""
echo "[4/4] 验证配置..."
echo ""

echo "测试 Nginx 配置语法..."
if ssh $SERVER "nginx -t 2>&1"; then
    echo "✓ 配置语法正确"
else
    echo "❌ 配置语法错误，请检查"
    echo "回滚: ssh $SERVER 'cp $BACKUP_PATH $NGINX_CONF_PATH && nginx -s reload'"
    exit 1
fi

echo ""
echo "重载 Nginx..."
ssh $SERVER "nginx -s reload"
echo "✓ Nginx 已重载"
echo ""

echo "验证 index.html 响应头..."
CACHE_CONTROL=$(curl -sI https://www.molitao.top/index.html 2>/dev/null | grep -i "cache-control" || echo "")
if echo "$CACHE_CONTROL" | grep -q "no-cache"; then
    echo "✓ index.html 缓存控制已生效"
    echo "  $CACHE_CONTROL"
else
    echo "⚠️  index.html 缓存控制未生效，请检查配置"
fi

echo ""
echo "========================================="
echo "✓ 配置更新完成！"
echo "========================================="
echo ""
echo "备份位置: $BACKUP_PATH"
echo ""
echo "让客户清理缓存后即可解决白屏问题"