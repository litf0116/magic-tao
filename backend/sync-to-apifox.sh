#!/bin/bash

# Swagger 文档同步到 Apifox 脚本
# 用途：自动导出 Swagger 文档并准备导入到 Apifox

set -e

# 配置
SWAGGER_URL="http://127.0.0.1:12580/swagger/v1/swagger.json"
OUTPUT_DIR="/Users/mac/workspace/magic-tao/backend"
OUTPUT_FILE="$OUTPUT_DIR/swagger.json"
BACKUP_DIR="$OUTPUT_DIR/swagger-backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "========================================="
echo "Swagger 文档同步工具"
echo "========================================="
echo ""

# 检查后端服务是否运行
echo "🔍 检查后端服务状态..."
if ! curl -s --connect-timeout 5 "$SWAGGER_URL" > /dev/null 2>&1; then
    echo "❌ 错误：无法连接到后端服务"
    echo "   请确保后端服务正在运行：http://127.0.0.1:12580"
    exit 1
fi
echo "✅ 后端服务运行正常"
echo ""

# 创建备份目录
mkdir -p "$BACKUP_DIR"

# 备份旧文档
if [ -f "$OUTPUT_FILE" ]; then
    echo "📦 备份旧文档..."
    cp "$OUTPUT_FILE" "$BACKUP_DIR/swagger_$TIMESTAMP.json"
    echo "✅ 备份完成: swagger_$TIMESTAMP.json"
    echo ""
fi

# 导出最新 Swagger 文档
echo "⬇️  导出 Swagger 文档..."
curl -s "$SWAGGER_URL" -o "$OUTPUT_FILE"

if [ $? -eq 0 ]; then
    echo "✅ 导出成功: $OUTPUT_FILE"

    # 显示文档统计信息
    API_COUNT=$(cat "$OUTPUT_FILE" | grep -o '"/api/' | wc -l | tr -d ' ')
    FILE_SIZE=$(ls -lh "$OUTPUT_FILE" | awk '{print $5}')

    echo ""
    echo "📊 文档统计："
    echo "   - 文件大小: $FILE_SIZE"
    echo "   - API 接口: 约 $(cat "$OUTPUT_FILE" | jq -r '.paths | keys | length' 2>/dev/null || echo "N/A") 个"
    echo "   - 导出时间: $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""
    echo "========================================="
    echo "下一步操作："
    echo "========================================="
    echo ""
    echo "方法一：URL 导入（推荐）"
    echo "  1. 打开 Apifox 项目"
    echo "  2. 项目设置 → 导入数据 → URL 导入"
    echo "  3. 输入: $SWAGGER_URL"
    echo "  4. 选择 '智能合并' 并确认导入"
    echo ""
    echo "方法二：文件导入"
    echo "  1. 打开 Apifox 项目"
    echo "  2. 项目设置 → 导入数据 → 文件导入"
    echo "  3. 上传文件: $OUTPUT_FILE"
    echo "  4. 选择导入方式并确认"
    echo ""
    echo "========================================="
else
    echo "❌ 导出失败"
    exit 1
fi