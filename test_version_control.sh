#!/bin/bash

# 版本控制功能测试脚本
# 测试 GetChatList API 的拍卖频道过滤逻辑

BASE_URL="http://localhost:12580"
API_ENDPOINT="/api/services/app/Client/GetChatList"

# 颜色输出
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================="
echo "版本控制功能测试"
echo "========================================="
echo ""

# 测试函数
test_version_control() {
    local test_name="$1"
    local version="$2"
    local expected_result="$3"
    
    echo "----------------------------------------"
    echo "测试: $test_name"
    echo "版本号: ${version:-无}"
    echo "预期结果: $expected_result"
    echo ""
    
    # 构建请求头
    if [ -z "$version" ]; then
        # 无版本号
        response=$(curl -s -X GET \
            "${BASE_URL}${API_ENDPOINT}" \
            -H "Content-Type: application/json" \
            -H "Abp.Tenantid: 1" \
            2>/dev/null)
    else
        # 有版本号
        response=$(curl -s -X GET \
            "${BASE_URL}${API_ENDPOINT}" \
            -H "Content-Type: application/json" \
            -H "Abp.Tenantid: 1" \
            -H "AppVersion: $version" \
            2>/dev/null)
    fi
    
    # 检查响应
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 请求成功${NC}"
        echo "响应: $(echo $response | jq -c '.result' 2>/dev/null || echo $response)"
        
        # 检查是否包含 id=-1 (拍卖频道)
        if echo "$response" | grep -q '"id":-1'; then
            echo -e "${GREEN}✓ 包含拍卖频道 (id=-1)${NC}"
        else
            echo -e "${YELLOW}✗ 不包含拍卖频道${NC}"
        fi
    else
        echo -e "${RED}✗ 请求失败${NC}"
    fi
    echo ""
}

# 测试场景
echo "开始测试..."
echo ""

# 场景1: 客户端版本 = 稳定版本 (20260224@1.1.21)
test_version_control \
    "客户端版本 = 稳定版本" \
    "20260224@1.1.21" \
    "显示拍卖频道"

# 场景2: 客户端版本 > 稳定版本 (20260307@1.1.22)
test_version_control \
    "客户端版本 > 稳定版本" \
    "20260307@1.1.22" \
    "隐藏拍卖频道"

# 场景3: 客户端版本 < 稳定版本 (20260220@1.1.20)
test_version_control \
    "客户端版本 < 稳定版本" \
    "20260220@1.1.20" \
    "显示拍卖频道"

# 场景4: 无版本号 (策略A)
test_version_control \
    "无版本号 (策略A保护)" \
    "" \
    "显示拍卖频道"

echo "========================================="
echo "测试完成"
echo "========================================="