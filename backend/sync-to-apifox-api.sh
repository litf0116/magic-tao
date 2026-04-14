#!/bin/bash

# Apifox Open API 自动导入脚本
# 使用 Apifox 开放 API 自动导入 Swagger 文档

set -e

# ============================================
# 配置区域 - 请填写你的配置
# ============================================

# Apifox API Token（从 Apifox 个人设置中获取）
APIFOX_TOKEN="APS-xxxxx"  # 替换为你的 Token

# Apifox 项目 ID（从项目 URL 中获取，如 https://app.apifox.com/project/123456）
PROJECT_ID="123456"  # 替换为你的项目 ID

# Swagger JSON URL
SWAGGER_URL="http://127.0.0.1:12580/swagger/v1/swagger.json"

# 导入方式：智能合并（normal）或 覆盖（overwrite）
IMPORT_MODE="normal"

# ============================================
# 脚本逻辑
# ============================================

APIFOX_API_BASE="https://api.apifox.com"
IMPORT_ENDPOINT="/v1/projects/${PROJECT_ID}/import-openapi"

echo "========================================="
echo "Apifox Open API 自动导入工具"
echo "========================================="
echo ""
echo "项目 ID: $PROJECT_ID"
echo "导入模式: $([ "$IMPORT_MODE" = "normal" ] && echo "智能合并" || echo "覆盖导入")"
echo "Swagger URL: $SWAGGER_URL"
echo ""

# 检查 Token 是否配置
if [[ "$APIFOX_TOKEN" == "APS-xxxxx" ]]; then
    echo "❌ 错误：请先配置 APIFOX_TOKEN"
    echo "   获取方式："
    echo "   1. 登录 Apifox"
    echo "   2. 进入 个人设置 → API 访问令牌"
    echo "   3. 创建新的访问令牌"
    echo "   4. 复制 Token 并替换脚本中的 APIFOX_TOKEN"
    exit 1
fi

# 检查项目 ID 是否配置
if [[ "$PROJECT_ID" == "123456" ]]; then
    echo "❌ 错误：请先配置 PROJECT_ID"
    echo "   获取方式："
    echo "   从项目 URL 中获取，如 https://app.apifox.com/project/123456"
    exit 1
fi

# 导出 Swagger 文档
echo "📥 导出 Swagger 文档..."
TEMP_FILE=$(mktemp)
curl -s "$SWAGGER_URL" -o "$TEMP_FILE"

if [ $? -ne 0 ]; then
    echo "❌ 导出失败：无法访问 Swagger URL"
    rm -f "$TEMP_FILE"
    exit 1
fi

echo "✅ 导出成功"
echo ""

# 调用 Apifox API 导入
echo "🚀 调用 Apifox API 导入文档..."
HTTP_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST \
  "${APIFOX_API_BASE}${IMPORT_ENDPOINT}" \
  -H "Authorization: Bearer ${APIFOX_TOKEN}" \
  -H "Content-Type: application/json" \
  -d "{
    \"data\": $(cat "$TEMP_FILE"),
    \"importMode\": \"${IMPORT_MODE}\",
    \"syncOptions\": {
      \"syncApiDefinitions\": true,
      \"syncRequestParams\": true,
      \"syncResponseExamples\": true,
      \"syncApiFolders\": true
    }
  }")

HTTP_BODY=$(echo "$HTTP_RESPONSE" | head -n -1)
HTTP_STATUS=$(echo "$HTTP_RESPONSE" | tail -n 1)

rm -f "$TEMP_FILE"

if [ "$HTTP_STATUS" -eq 200 ] || [ "$HTTP_STATUS" -eq 201 ]; then
    echo "✅ 导入成功！"
    echo ""
    echo "响应："
    echo "$HTTP_BODY" | jq '.' 2>/dev/null || echo "$HTTP_BODY"
else
    echo "❌ 导入失败"
    echo "HTTP 状态码: $HTTP_STATUS"
    echo "响应："
    echo "$HTTP_BODY" | jq '.' 2>/dev/null || echo "$HTTP_BODY"
    exit 1
fi

echo ""
echo "========================================="
echo "✅ 同步完成"
echo "========================================="
echo ""
echo "你可以在 Apifox 中查看导入的文档："
echo "https://app.apifox.com/project/${PROJECT_ID}"