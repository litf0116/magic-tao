#!/bin/bash

# 测试环境配置
BASE_URL="http://localhost:12580"
USER_ID=14

echo "========================================"
echo "拍品列表排序测试 - 连续发布10个拍品"
echo "========================================"

# Step 1: 获取用户 token
echo ""
echo "1. 获取用户 $USER_ID 的 token..."
TOKEN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/TokenAuth/GenerateTokenForUser" \
    -H "Content-Type: application/json" \
    -d "{\"userId\": $USER_ID}")

TOKEN=$(echo $TOKEN_RESPONSE | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "ERROR: 获取 token 失败"
    echo "$TOKEN_RESPONSE"
    exit 1
fi

echo "   Token 获取成功"

# Step 2-11: 连续发布 10 个拍品并检查列表
echo ""
echo "2. 开始连续发布拍品..."
echo ""

for i in {1..10}; do
    echo "--- 第 $i 次发布 ---"

    # 生成唯一的拍品名称
    TIMESTAMP=$(date +%s)
    AUCTION_NAME="测试拍品_$TIMESTAMP"

    # 发布拍品
    CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/services/app/AuctionItem/Create" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Abp.Tenantid: 1" \
        -d "{
            \"name\": \"$AUCTION_NAME\",
            \"status\": 1,
            \"imageUrl\": \"https://example.com/image_$i.jpg\",
            \"description\": \"测试描述 $i\",
            \"startingPrice\": $((100 + i)),
            \"sellerInfo\": \"测试卖家\",
            \"order\": 0
        }")

    AUCTION_ID=$(echo $CREATE_RESPONSE | grep -o '"id":[0-9]*' | head -1 | cut -d':' -f2)
    echo "   创建: ID=$AUCTION_ID, 名称=$AUCTION_NAME"

    # 等待一下确保缓存刷新
    sleep 0.5

    # 获取拍品列表
    LIST_RESPONSE=$(curl -s -X GET "$BASE_URL/api/services/app/AuctionItem/GetPublicList?MaxResultCount=100" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Abp.Tenantid: 1")

    # 提取前3个拍品的名称和ID
    FIRST_ITEM=$(echo $LIST_RESPONSE | grep -o '"name":"[^"]*"' | head -1 | cut -d'"' -f4)
    FIRST_ID=$(echo $LIST_RESPONSE | grep -o '"id":[0-9]*' | head -1 | cut -d':' -f2)

    # 检查新创建的拍品是否在第一个位置
    if [[ "$FIRST_ITEM" == *"$TIMESTAMP"* ]]; then
        echo "   ✓ 新拍品在列表第一位: $FIRST_NAME (ID=$FIRST_ID)"
    else
        echo "   ✗ 新拍品未在第一位!"
        echo "   期望: $AUCTION_NAME (ID=$AUCTION_ID)"
        echo "   实际第一位: $FIRST_ITEM (ID=$FIRST_ID)"
    fi

    echo ""
done

echo "========================================"
echo "测试完成!"
echo "========================================"
