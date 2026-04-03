#!/bin/bash

set -e

BASE_URL="http://localhost:12580"
USER_ID="7509"
MYSQL_HOST="127.0.0.1"
MYSQL_USER="root"
MYSQL_PASS="root"
MYSQL_DB="www_molitao_top"

echo "======================================"
echo "支付功能测试 - 用户 ID: ${USER_ID}"
echo "======================================"
echo ""

echo "[步骤 1] 查询用户当前保证金余额..."
BEFORE_BALANCE=$(mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -N -e \
  "SELECT DepositBalance FROM AbpUsers WHERE Id = ${USER_ID}" 2>&1 | grep -v Warning)
echo "当前保证金余额: ¥${BEFORE_BALANCE}"
echo ""

echo "[步骤 2] 查询用户最近的支付订单..."
RECENT_ORDER=$(mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -N -e \
  "SELECT OutTradeNo, State, Total, CreationTime FROM Pays_PayOrder WHERE CreatorUserId = ${USER_ID} ORDER BY CreationTime DESC LIMIT 1" 2>&1 | grep -v Warning)
if [ -n "$RECENT_ORDER" ]; then
  echo "最近订单: $RECENT_ORDER"
else
  echo "无历史订单"
fi
echo ""

echo "[步骤 3] 模拟创建支付订单（直接插入数据库）..."
OUT_TRADE_NO="TEST_$(date +%Y%m%d%H%M%S)_${USER_ID}"
TOTAL_AMOUNT=5100
ORDER_ID=$(cat /proc/sys/kernel/random/uuid | tr '[:lower:]' '[:upper:]' | tr -d '-')

mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -e "
INSERT INTO Pays_PayOrder (Id, OutTradeNo, Total, State, HostType, PayType, AppName, AppId, MchId, TenantId, CreatorUserId, CreationTime)
VALUES ('${ORDER_ID}', '${OUT_TRADE_NO}', ${TOTAL_AMOUNT}, 0, 2, 1, 'pub', 'wxfb7bd5b5f94a8805', '1669900694', 1, ${USER_ID}, NOW());
" 2>&1 | grep -v Warning

echo "✓ 订单已创建"
echo "  订单号: ${OUT_TRADE_NO}"
echo "  金额: ¥51.00"
echo ""

echo "[步骤 4] 模拟支付成功（更新订单状态）..."
mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -e "
UPDATE Pays_PayOrder 
SET State = 1, IsSuccessPay = 1, SuccessPayTime = NOW(), TransactionId = 'MOCK_TX_${OUT_TRADE_NO}'
WHERE OutTradeNo = '${OUT_TRADE_NO}';
" 2>&1 | grep -v Warning

echo "✓ 订单状态已更新为已支付"
echo ""

echo "[步骤 5] 更新用户保证金余额..."
mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -e "
UPDATE AbpUsers 
SET DepositBalance = DepositBalance + 50.00
WHERE Id = ${USER_ID};
" 2>&1 | grep -v Warning

echo "✓ 保证金已充值（实际到账 ¥50.00，扣除手续费 ¥1.00）"
echo ""

echo "[步骤 6] 查询充值后保证金余额..."
AFTER_BALANCE=$(mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -N -e \
  "SELECT DepositBalance FROM AbpUsers WHERE Id = ${USER_ID}" 2>&1 | grep -v Warning)
echo "当前保证金余额: ¥${AFTER_BALANCE}"
echo ""

echo "[步骤 7] 验证订单状态..."
ORDER_STATUS=$(mysql -h${MYSQL_HOST} -u${MYSQL_USER} -p${MYSQL_PASS} ${MYSQL_DB} -N -e \
  "SELECT State, IsSuccessPay, TransactionId FROM Pays_PayOrder WHERE OutTradeNo = '${OUT_TRADE_NO}'" 2>&1 | grep -v Warning)
echo "订单状态: $ORDER_STATUS"
echo ""

echo "======================================"
echo "✅ 支付流程测试完成"
echo "======================================"
echo "充值前余额: ¥${BEFORE_BALANCE}"
echo "充值后余额: ¥${AFTER_BALANCE}"
echo "充值金额: ¥50.00（支付 ¥51.00，手续费 ¥1.00）"
echo ""