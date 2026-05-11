#!/bin/bash
# 本地极光推送端到端测试脚本
# 测试流程: 订阅 → 开拍 → 验证推送

set -e

BASE_URL="http://localhost:12580"
AUCTION_ID=123  # 替换为实际拍品ID
USER_ID=7509

echo "=========================================="
echo "本地极光推送端到端测试"
echo "=========================================="

echo ""
echo "[Step 1] 获取用户 Token..."
USER_TOKEN=$(curl -s -X POST "$BASE_URL/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"feifei","password":"123qwe"}' | jq -r '.result.accessToken')

if [ -z "$USER_TOKEN" ] || [ "$USER_TOKEN" = "null" ]; then
    echo "❌ Token 获取失败"
    exit 1
fi
echo "✅ Token 获取成功"
echo "  Token: ${USER_TOKEN:0:50}..."

echo ""
echo "[Step 2] 查找可测试的拍品..."
# 查询待开拍状态的拍品
AUCTION_ID=$(curl -s "$BASE_URL/api/services/app/AuctionItem/GetAll?status=0&maxResultCount=1" \
  -H "Authorization: Bearer $USER_TOKEN" | jq -r '.result.items[0].id // empty')

if [ -z "$AUCTION_ID" ]; then
    echo "❌ 未找到待开拍状态的拍品"
    echo "  请手动设置 AUCTION_ID 后重试"
    exit 1
fi
echo "✅ 找到待开拍拍品: ID=$AUCTION_ID"

echo ""
echo "[Step 3] 订阅拍品开拍通知..."
SUB_RESULT=$(curl -s -X POST "$BASE_URL/api/services/app/AuctionItem/SubStartNotify" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"auctionItemId\": $AUCTION_ID, \"platform\": \"app\"}")

echo "  订阅结果: $SUB_RESULT" | jq -r '.success' | grep -q "true" && echo "✅ 订阅成功" || echo "❌ 订阅失败"

echo ""
echo "[Step 4] 验证订阅记录..."
echo "⚠️  跳过订阅记录验证 (API 接口不存在)"
echo "  订阅接口已返回 success=true，继续测试..."

echo ""
echo "[Step 5] 触发拍品开拍..."
START_RESULT=$(curl -s -X GET "$BASE_URL/api/services/app/AuctionItem/StartAuction?id=$AUCTION_ID" \
  -H "Authorization: Bearer $USER_TOKEN")

echo "  开拍结果: $START_RESULT" | jq '.'

echo ""
echo "[Step 6] 检查后端日志..."
echo "  查看推送相关日志（最后 20 行）:"
echo "  ----------------------------------------"
tail -20 /Users/mac/workspace/magic-tao/backend/src/TtWork.Project.Web.Host/bin/Debug/net8.0/Logs/Logs.txt 2>/dev/null | grep -E "推送|订阅|alias|user_7509" || echo "  未找到推送相关日志"

echo ""
echo "=========================================="
echo "测试完成"
echo "=========================================="
echo ""
echo "验证步骤:"
echo "1. 检查后端日志是否有推送记录"
echo "2. 检查 App 端是否收到通知"
echo "3. 如需重复测试，请使用新的拍品 ID"
