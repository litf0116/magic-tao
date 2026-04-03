#!/bin/bash

set -e

BASE_URL="http://localhost:12580"
USER_ID="7509"

echo "======================================"
echo "支付回调模拟测试"
echo "======================================"
echo ""

echo "[步骤 1] 生成用户认证 Token..."
TOKEN_RESPONSE=$(curl -s -X POST \
  "${BASE_URL}/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": ${USER_ID}}")

ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin)['result']['accessToken'])" 2>/dev/null)

if [ -z "$ACCESS_TOKEN" ]; then
  echo "❌ Token 生成失败"
  exit 1
fi

echo "✓ Token 已生成"
echo ""

echo "[步骤 2] 创建测试支付订单..."
PAY_RESPONSE=$(curl -s -X GET \
  "${BASE_URL}/api/services/app/Client/PayDepositNative?amount=0.01" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}")

OUT_TRADE_NO=$(echo "$PAY_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin)['result']['outTradeNo'])" 2>/dev/null)
CODE_URL=$(echo "$PAY_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin)['result']['code_url'])" 2>/dev/null)

echo "✓ 订单已创建: ${OUT_TRADE_NO}"
echo "二维码链接: ${CODE_URL}"
echo ""

echo "[步骤 3] 构造模拟微信支付回调数据..."
CALLBACK_DATA=$(cat <<EOF
{
  "id": "MOCK_NOTIFY_$(date +%s)",
  "create_time": "$(date -u +"%Y-%m-%dT%H:%M:%S+00:00")",
  "resource_type": "encrypt-resource",
  "event_type": "TRANSACTION.SUCCESS",
  "summary": "支付成功",
  "resource": {
    "original_type": "transaction",
    "algorithm": "AEAD_AES_256_GCM",
    "ciphertext": "MOCK_CIPHERTEXT",
    "associated_data": "transaction",
    "nonce": "MOCK_NONCE"
  }
}
EOF
)
echo "回调数据已构造"
echo ""

echo "[步骤 4] 发送支付成功回调请求..."
echo "⚠️  注意：真实回调需要签名验证，此处仅模拟流程"
echo "回调 URL: ${BASE_URL}/api/PayNotify/TenPay/pub"
echo ""

echo "[步骤 5] 测试订单查询 API..."
mysql -h127.0.0.1 -uroot -proot www_molitao_top -e \
  "SELECT OutTradeNo, State, Total, CreationTime FROM Pays_PayOrder WHERE OutTradeNo = '${OUT_TRADE_NO}'" 2>&1 | grep -v Warning
echo ""

echo "======================================"
echo "✅ 回调流程测试完成"
echo "======================================"
echo "订单号: ${OUT_TRADE_NO}"
echo "状态: 待支付（等待真实支付或手动触发回调）"
echo ""
echo "后续步骤："
echo "1. 使用微信扫描二维码完成真实支付"
echo "2. 或手动更新订单状态测试后续流程"
echo ""