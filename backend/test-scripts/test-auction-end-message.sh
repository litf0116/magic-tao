#!/bin/bash

# 拍卖成功消息展示功能测试脚本
# 测试目标：验证拍卖成功后的 channel 消息发送和展示功能

# 配置
BASE_URL="http://127.0.0.1:12580"
AUCTION_CHANNEL="-1_auction"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 打印函数
print_header() {
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}$1${NC}"
    echo -e "${GREEN}========================================${NC}"
}

print_section() {
    echo -e "\n${YELLOW}>>> $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# 步骤1：登录获取 token
print_header "步骤1：登录获取认证 token"

LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/Authenticate" \
    -H "Content-Type: application/json" \
    -d '{
        "userNameOrEmailAddress": "admin",
        "password": "123qwe"
    }')

# 提取 token
ACCESS_TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ]; then
    print_error "登录失败，无法获取 token"
    echo "响应: $LOGIN_RESPONSE"
    exit 1
fi

print_success "登录成功，获取到 token: ${ACCESS_TOKEN:0:20}..."

# 设置认证头
AUTH_HEADER="Authorization: Bearer $ACCESS_TOKEN"
VERSION_HEADER="AppVersion: 20260224@1.1.21"

# 步骤2：获取当前用户信息
print_header "步骤2：获取当前用户信息"

USER_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
    -H "$AUTH_HEADER")

echo "$USER_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$USER_RESPONSE"

USER_ID=$(echo $USER_RESPONSE | grep -o '"user":{[^}]*' | grep -o '"id":[0-9]*' | cut -d':' -f2)

if [ -z "$USER_ID" ]; then
    print_error "无法获取用户 ID"
    exit 1
fi

print_success "当前用户 ID: $USER_ID"

# 步骤3：获取拍卖商品列表
print_header "步骤3：获取拍卖商品列表"

AUCTION_LIST_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/GetPublicList" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

echo "$AUCTION_LIST_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$AUCTION_LIST_RESPONSE"

# 查找一个可测试的拍卖商品 ID
AUCTION_ITEM_ID=$(echo "$AUCTION_LIST_RESPONSE" | grep -o '"id":[0-9]*' | head -1 | cut -d':' -f2)

if [ -z "$AUCTION_ITEM_ID" ]; then
    print_error "没有找到拍卖商品，请先创建测试数据"
    print_section "创建测试拍卖商品..."

    CREATE_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/services/app/AuctionItem/Create" \
        -H "$AUTH_HEADER" \
        -H "$VERSION_HEADER" \
        -H "Content-Type: application/json" \
        -d '{
            "name": "测试商品-AutoTest",
            "description": "自动化测试商品",
            "startingPrice": 100,
            "images": ["http://example.com/test.jpg"]
        }')

    echo "$CREATE_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$CREATE_RESPONSE"

    AUCTION_ITEM_ID=$(echo "$CREATE_RESPONSE" | grep -o '"id":[0-9]*' | cut -d':' -f2)

    if [ -z "$AUCTION_ITEM_ID" ]; then
        print_error "创建拍卖商品失败"
        exit 1
    fi

    print_success "创建拍卖商品成功，ID: $AUCTION_ITEM_ID"
else
    print_success "找到拍卖商品，ID: $AUCTION_ITEM_ID"
fi

# 步骤4：开始拍卖
print_header "步骤4：开始拍卖"

START_AUCTION_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/StartAuction?id=${AUCTION_ITEM_ID}" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

echo "$START_AUCTION_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$START_AUCTION_RESPONSE"

START_SUCCESS=$(echo "$START_AUCTION_RESPONSE" | grep -o '"success":[a-z]*' | cut -d':' -f2)

if [ "$START_SUCCESS" != "true" ]; then
    print_error "开始拍卖失败，可能商品已经在拍卖中"
else
    print_success "拍卖已开始"
fi

# 步骤5：模拟出价
print_header "步骤5：模拟出价"

BID_PRICE=150
BID_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/services/app/AuctionItem/Bid" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER" \
    -H "Content-Type: application/json" \
    -d "{
        \"auctionItemId\": $AUCTION_ITEM_ID,
        \"bidPrice\": $BID_PRICE
    }")

echo "$BID_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$BID_RESPONSE"

CURRENT_PRICE=$(echo "$BID_RESPONSE" | grep -o '"currentPrice":[0-9]*' | cut -d':' -f2)

if [ -n "$CURRENT_PRICE" ]; then
    print_success "出价成功，当前价格: $CURRENT_PRICE"
else
    print_error "出价失败"
fi

# 步骤6：结束拍卖（手动触发）
print_header "步骤6：结束拍卖（发送 AuctionEnd 消息到 channel）"

END_AUCTION_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/EndAuction?id=${AUCTION_ITEM_ID}" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

echo "$END_AUCTION_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$END_AUCTION_RESPONSE"

# 验证拍卖成功消息
FINAL_PRICE=$(echo "$END_AUCTION_RESPONSE" | grep -o '"finalPrice":[0-9]*' | cut -d':' -f2)
DEAL_USER_ID=$(echo "$END_AUCTION_RESPONSE" | grep -o '"dealUserId":[0-9]*' | cut -d':' -f2)
DEAL_USER_NAME=$(echo "$END_AUCTION_RESPONSE" | grep -o '"dealUserName":"[^"]*' | cut -d'"' -f4)

if [ -n "$FINAL_PRICE" ] && [ "$FINAL_PRICE" != "null" ]; then
    print_success "拍卖成功成交！"
    print_section "成交信息："
    echo "  - 成交价格: ￥$FINAL_PRICE"
    echo "  - 成交用户ID: $DEAL_USER_ID"
    echo "  - 成交用户: $DEAL_USER_NAME"
    echo "  - 预期消息类型: AuctionEnd (1010)"
    echo "  - 预期 channel: $AUCTION_CHANNEL"
    echo "  - 预期消息内容: 恭喜 $DEAL_USER_NAME 以 ￥$FINAL_PRICE 拍得 [商品名]"
else
    print_error "拍卖未成交（流拍）"
    echo "  - 预期消息类型: AuctionEnd (1010)"
    echo "  - 预期消息内容: 拍卖结束，无人出价，商品已回退"
fi

# 步骤7：获取消息列表验证
print_header "步骤7：获取 channel 消息列表验证"

# 获取拍卖频道的消息
sleep 2  # 等待消息处理完成
MESSAGES_RESPONSE=$(curl -s -X POST "${BASE_URL}/ws/get-history" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER" \
    -H "Content-Type: application/json" \
    -d "{
        \"chan\": \"$AUCTION_CHANNEL\",
        \"lastMessageId\": null,
        \"limit\": 20
    }")

echo "$MESSAGES_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$MESSAGES_RESPONSE"

# 检查是否包含 AuctionEnd 消息
if echo "$MESSAGES_RESPONSE" | grep -q "AuctionEnd\|1010\|拍卖结束\|恭喜"; then
    print_success "✓ 验证成功：发现拍卖结束消息"
else
    print_error "✗ 验证失败：未发现拍卖结束消息"
fi

# 步骤8：检查用户私聊（如果成交）
if [ -n "$DEAL_USER_ID" ]; then
    print_header "步骤8：检查成交用户私聊消息"

    # 获取当前用户的私聊列表
    CHAT_LIST_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Client/GetChatList" \
        -H "$AUTH_HEADER" \
        -H "$VERSION_HEADER")

    echo "$CHAT_LIST_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$CHAT_LIST_RESPONSE"

    if echo "$CHAT_LIST_RESPONSE" | grep -q "$DEAL_USER_ID"; then
        print_success "✓ 验证成功：发现与成交用户的私聊频道"
        print_section "私聊频道中应该包含:"
        echo "  - 消息类型: AuctionDeal (编码为 AuctionEnd)"
        echo "  - 消息内容: 恭喜您,您拍得了..."
    else
        print_error "✗ 验证失败：未发现与成交用户的私聊频道"
    fi
fi

# 步骤9：总结
print_header "测试总结"

echo -e "${GREEN}测试场景完成！${NC}"
echo ""
echo "✓ 已测试功能："
echo "  1. 用户登录认证"
echo "  2. 获取拍卖商品列表"
echo "  3. 开始拍卖"
echo "  4. 模拟出价"
echo "  5. 结束拍卖（手动）"
echo "  6. 验证 channel 消息发送"
echo "  7. 验证消息列表"
echo "  8. 验证私聊频道创建（如果成交）"
echo ""
echo "📝 注意事项："
echo "  - 拍卖成功会发送两条消息："
echo "    1. AuctionEnd 消息到 -1_auction 频道（所有用户可见）"
echo "    2. AuctionDeal 私信给成交用户（仅成交用户可见，自动编码为 AuctionEnd）"
echo "  - 请查看后端日志确认消息发送情况"
echo "  - 前端需要通过 WebSocket 接收并展示消息"
echo ""
echo -e "${YELLOW}下一步：${NC}"
echo "  1. 查看后端日志: backend/logs/*.log"
echo "  2. 使用 WebSocket 客户端连接 ws://127.0.0.1:12580/ws"
echo "  3. 订阅频道: ws/sub-channel"
echo "  4. 查看前端 PC 页面的消息展示"