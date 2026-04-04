#!/bin/bash

# ============================================================
# 魔力淘 API 集成测试脚本
# 测试日期: $(date +%Y-%m-%d)
# 后端地址: http://127.0.0.1:12580
# ============================================================

BASE_URL="http://127.0.0.1:12580"
RESULTS_DIR="$(cd "$(dirname "$0")" && pwd)/test-results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RESULT_LOG="${RESULTS_DIR}/${TIMESTAMP}_result.log"
RESULT_JSON="${RESULTS_DIR}/${TIMESTAMP}_summary.json"

# 创建结果目录
mkdir -p "$RESULTS_DIR"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# 计数器
TOTAL=0
PASSED=0
FAILED=0
SKIPPED=0

# 测试结果数组
declare -a TEST_NAMES
declare -a TEST_RESULTS
declare -a TEST_DETAILS

# 日志函数
log() {
    echo -e "$1" | tee -a "$RESULT_LOG"
}

print_header() {
    log "\n${BLUE}========================================================${NC}"
    log "${BLUE}  $1${NC}"
    log "${BLUE}========================================================${NC}"
}

print_section() {
    log "\n${YELLOW}>>> $1${NC}"
}

print_success() {
    log "${GREEN}  ✓ $1${NC}"
}

print_error() {
    log "${RED}  ✗ $1${NC}"
}

print_info() {
    log "  $1"
}

# 记录测试结果
record_result() {
    local name="$1"
    local status="$2"  # PASS, FAIL, SKIP
    local detail="$3"
    
    TOTAL=$((TOTAL + 1))
    TEST_NAMES+=("$name")
    TEST_RESULTS+=("$status")
    TEST_DETAILS+=("$detail")
    
    case "$status" in
        PASS) PASSED=$((PASSED + 1)) ;;
        FAIL) FAILED=$((FAILED + 1)) ;;
        SKIP) SKIPPED=$((SKIPPED + 1)) ;;
    esac
}

# 初始化日志
echo "魔力淘 API 集成测试报告" > "$RESULT_LOG"
echo "测试时间: $(date '+%Y-%m-%d %H:%M:%S')" >> "$RESULT_LOG"
echo "后端地址: $BASE_URL" >> "$RESULT_LOG"
echo "========================================" >> "$RESULT_LOG"

# ============================================================
# 步骤0：检查后端服务
# ============================================================
print_header "步骤0：检查后端服务状态"

HEALTH_CHECK=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/swagger/v1/swagger.json" 2>/dev/null)

if [ "$HEALTH_CHECK" = "200" ]; then
    print_success "后端服务正常运行 (HTTP $HEALTH_CHECK)"
    record_result "后端服务检查" "PASS" "HTTP $HEALTH_CHECK"
else
    print_error "后端服务未运行或异常 (HTTP $HEALTH_CHECK)"
    log "${RED}请先启动后端服务: cd backend && dotnet run --project src/TtWork.Project.Web.Host${NC}"
    record_result "后端服务检查" "FAIL" "HTTP $HEALTH_CHECK"
    exit 1
fi

# ============================================================
# 流程1：用户认证流程
# ============================================================
print_header "流程1：用户认证流程"

# TC-001: 管理员登录
print_section "TC-001: 管理员登录 (admin/123456)"

ADMIN_LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/Authenticate" \
    -H "Content-Type: application/json" \
    -d '{
        "userNameOrEmailAddress": "admin",
        "password": "123456"
    }')

ACCESS_TOKEN=$(echo "$ADMIN_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('accessToken',''))" 2>/dev/null)
USER_ID=$(echo "$ADMIN_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('userId',''))" 2>/dev/null)
USER_NAME=$(echo "$ADMIN_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('user',{}).get('userName',''))" 2>/dev/null)
EXPIRE_IN=$(echo "$ADMIN_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('expireInSeconds',''))" 2>/dev/null)

if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "" ]; then
    print_success "管理员登录成功"
    print_info "用户ID: $USER_ID"
    print_info "用户名: $USER_NAME"
    print_info "Token有效期: ${EXPIRE_IN}秒"
    print_info "Token前缀: ${ACCESS_TOKEN:0:30}..."
    record_result "TC-001: 管理员登录" "PASS" "userId=$USER_ID, expire=${EXPIRE_IN}s"
else
    print_error "管理员登录失败"
    print_info "响应: $(echo "$ADMIN_LOGIN_RESPONSE" | head -c 200)"
    record_result "TC-001: 管理员登录" "FAIL" "未获取到token"
    exit 1
fi

# TC-001b: 普通用户登录
print_section "TC-001b: 普通用户登录 (feifei/123456)"

USER_LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/Authenticate" \
    -H "Content-Type: application/json" \
    -d '{
        "userNameOrEmailAddress": "feifei",
        "password": "123456"
    }')

USER_ACCESS_TOKEN=$(echo "$USER_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('accessToken',''))" 2>/dev/null)
FEIFEI_USER_ID=$(echo "$USER_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('userId',''))" 2>/dev/null)
FEIFEI_USER_NAME=$(echo "$USER_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('user',{}).get('userName',''))" 2>/dev/null)

if [ -n "$USER_ACCESS_TOKEN" ] && [ "$USER_ACCESS_TOKEN" != "" ]; then
    print_success "普通用户登录成功"
    print_info "用户ID: $FEIFEI_USER_ID"
    print_info "用户名: $FEIFEI_USER_NAME"
    record_result "TC-001b: 普通用户登录" "PASS" "userId=$FEIFEI_USER_ID"
else
    print_error "普通用户登录失败"
    print_info "响应: $(echo "$USER_LOGIN_RESPONSE" | head -c 200)"
    record_result "TC-001b: 普通用户登录" "FAIL" "未获取到token"
fi

# 设置认证头
AUTH_HEADER="Authorization: Bearer $ACCESS_TOKEN"
USER_AUTH_HEADER="Authorization: Bearer $USER_ACCESS_TOKEN"
VERSION_HEADER="AppVersion: 20260224@1.1.21"

# TC-002: 获取当前登录信息
print_section "TC-002: 获取当前登录信息"

SESSION_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

SESSION_USER_ID=$(echo "$SESSION_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('user',{}).get('id',''))" 2>/dev/null)
SESSION_USER_NAME=$(echo "$SESSION_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('user',{}).get('userName',''))" 2>/dev/null)

if [ -n "$SESSION_USER_ID" ] && [ "$SESSION_USER_ID" != "None" ]; then
    print_success "获取登录信息成功"
    print_info "用户名: $SESSION_USER_NAME"
    print_info "用户ID: $SESSION_USER_ID"
    record_result "TC-002: 获取登录信息" "PASS" "user=$SESSION_USER_NAME, id=$SESSION_USER_ID"
else
    print_error "获取登录信息失败"
    print_info "响应: $(echo "$SESSION_RESPONSE" | head -c 200)"
    record_result "TC-002: 获取登录信息" "FAIL" "未获取到用户信息"
fi

# TC-010a: 获取用户详情
print_section "TC-010a: 获取用户详情"

USER_DETAIL_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/User/Get?id=$SESSION_USER_ID" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

USER_NAME_DETAIL=$(echo "$USER_DETAIL_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('userName',''))" 2>/dev/null)
USER_PHONE=$(echo "$USER_DETAIL_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('phoneNumber','未设置'))" 2>/dev/null)

if [ -n "$USER_NAME_DETAIL" ] && [ "$USER_NAME_DETAIL" != "None" ]; then
    print_success "获取用户详情成功"
    print_info "用户名: $USER_NAME_DETAIL"
    print_info "手机号: $USER_PHONE"
    record_result "TC-010a: 获取用户详情" "PASS" "user=$USER_NAME_DETAIL"
else
    print_error "获取用户详情失败"
    print_info "响应: $(echo "$USER_DETAIL_RESPONSE" | head -c 200)"
    record_result "TC-010a: 获取用户详情" "FAIL" "未获取到用户详情"
fi

# ============================================================
# 流程2：拍卖核心流程
# ============================================================
print_header "流程2：拍卖核心流程"

# TC-003: 获取拍卖列表
print_section "TC-003: 获取公开拍卖列表"

AUCTION_LIST_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/GetPublicList?MaxResultCount=10" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

AUCTION_COUNT=$(echo "$AUCTION_LIST_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)
AUCTION_ITEM_ID=$(echo "$AUCTION_LIST_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(items[0].get('id','') if items else '')" 2>/dev/null)

if [ -n "$AUCTION_COUNT" ] && [ "$AUCTION_COUNT" != "None" ]; then
    print_success "获取拍卖列表成功"
    print_info "拍卖商品数量: $AUCTION_COUNT"
    if [ -n "$AUCTION_ITEM_ID" ] && [ "$AUCTION_ITEM_ID" != "" ]; then
        print_info "首个拍卖品ID: $AUCTION_ITEM_ID"
    fi
    record_result "TC-003: 获取拍卖列表" "PASS" "count=$AUCTION_COUNT"
else
    print_error "获取拍卖列表失败"
    print_info "响应: $(echo "$AUCTION_LIST_RESPONSE" | head -c 200)"
    record_result "TC-003: 获取拍卖列表" "FAIL" "解析失败"
fi

# TC-003b: 获取拍卖中商品列表
print_section "TC-003b: 获取拍卖中商品列表"

AUCTION_MID_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/services/app/AuctionItem/GetAuctionMidList" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER" \
    -H "Content-Type: application/json" \
    -d '{"maxResultCount": 10}')

AUCTION_MID_COUNT=$(echo "$AUCTION_MID_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)

if [ -n "$AUCTION_MID_COUNT" ] && [ "$AUCTION_MID_COUNT" != "None" ]; then
    print_success "获取拍卖中商品列表成功"
    print_info "拍卖中商品数量: $AUCTION_MID_COUNT"
    record_result "TC-003b: 获取拍卖中列表" "PASS" "count=$AUCTION_MID_COUNT"
else
    print_error "获取拍卖中商品列表失败"
    print_info "响应: $(echo "$AUCTION_MID_RESPONSE" | head -c 200)"
    record_result "TC-003b: 获取拍卖中列表" "FAIL" "解析失败"
fi

# TC-004: 开始拍卖（如果有待拍卖商品）
print_section "TC-004: 开始拍卖"

# 查找状态为"上架"的商品
PENDING_AUCTION_ID=$(echo "$AUCTION_LIST_RESPONSE" | python3 -c "
import sys, json
d = json.load(sys.stdin)
items = d.get('result',{}).get('items',[])
for item in items:
    status = item.get('status', '')
    if status == '上架' and item.get('name', '').startswith('AutoTest-'):
        print(item.get('id', ''))
        break
" 2>/dev/null)

if [ -z "$PENDING_AUCTION_ID" ] || [ "$PENDING_AUCTION_ID" = "" ]; then
    PENDING_AUCTION_ID=$(echo "$AUCTION_LIST_RESPONSE" | python3 -c "
import sys, json
d = json.load(sys.stdin)
items = d.get('result',{}).get('items',[])
for item in items:
    if item.get('status') == '上架':
        print(item.get('id', ''))
        break
" 2>/dev/null)
fi

if [ -n "$PENDING_AUCTION_ID" ] && [ "$PENDING_AUCTION_ID" != "" ]; then
    print_info "找到待拍卖商品 ID: $PENDING_AUCTION_ID"
    
    START_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/StartAuction?id=$PENDING_AUCTION_ID" \
        -H "$AUTH_HEADER" \
        -H "$VERSION_HEADER")
    
    START_SUCCESS=$(echo "$START_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('success') else 'fail')" 2>/dev/null)
    START_STATUS=$(echo "$START_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('status',''))" 2>/dev/null)
    
    if [ "$START_SUCCESS" = "ok" ]; then
        print_success "开始拍卖成功，状态: $START_STATUS"
        record_result "TC-004: 开始拍卖" "PASS" "auctionId=$PENDING_AUCTION_ID, status=$START_STATUS"
        AUCTION_ITEM_ID="$PENDING_AUCTION_ID"
    else
        START_ERROR=$(echo "$START_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('error',{}).get('message',''))" 2>/dev/null)
        print_error "开始拍卖失败: $START_ERROR"
        record_result "TC-004: 开始拍卖" "FAIL" "error=$START_ERROR"
    fi
else
    print_info "⏭ 无待拍卖商品，跳过开始拍卖测试"
    record_result "TC-004: 开始拍卖" "SKIP" "无待拍卖商品"
fi

# TC-005: 用户出价
print_section "TC-005: 用户出价"

if [ -n "$AUCTION_ITEM_ID" ] && [ "$AUCTION_ITEM_ID" != "" ]; then
    AUCTION_DETAIL=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/GetDetail?id=$AUCTION_ITEM_ID" \
        -H "$AUTH_HEADER" \
        -H "$VERSION_HEADER")
    
    CURRENT_PRICE=$(echo "$AUCTION_DETAIL" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('currentPrice',0) or 0)" 2>/dev/null)
    STARTING_PRICE=$(echo "$AUCTION_DETAIL" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('startingPrice',0) or 0)" 2>/dev/null)
    
    if [ -n "$CURRENT_PRICE" ] && [ "$CURRENT_PRICE" != "None" ] && [ "$CURRENT_PRICE" != "0" ]; then
        BID_PRICE=$((CURRENT_PRICE + 500))
    else
        BID_PRICE=$((STARTING_PRICE + 500))
    fi
    
    print_info "拍卖品ID: $AUCTION_ITEM_ID"
    print_info "当前价格: $CURRENT_PRICE 分"
    print_info "出价金额: $BID_PRICE 分"
    
    BID_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/services/app/AuctionItem/Bid" \
        -H "$USER_AUTH_HEADER" \
        -H "$VERSION_HEADER" \
        -H "Content-Type: application/json" \
        -d "{
            \"auctionItemId\": $AUCTION_ITEM_ID,
            \"bidPrice\": $BID_PRICE
        }")
    
    BID_SUCCESS=$(echo "$BID_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('result') else 'fail')" 2>/dev/null)
    BID_NEW_PRICE=$(echo "$BID_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('currentPrice',''))" 2>/dev/null)
    BID_ERROR=$(echo "$BID_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('error',{}).get('message',''))" 2>/dev/null)
    
    if [ "$BID_SUCCESS" = "ok" ]; then
        print_success "出价成功"
        print_info "新价格: $BID_NEW_PRICE 分"
        record_result "TC-005: 用户出价" "PASS" "auctionId=$AUCTION_ITEM_ID, bidPrice=$BID_PRICE, newPrice=$BID_NEW_PRICE"
    else
        print_error "出价失败"
        print_info "错误信息: $BID_ERROR"
        record_result "TC-005: 用户出价" "FAIL" "error=$BID_ERROR"
    fi
else
    print_info "⏭ 无拍卖中商品，跳过出价测试"
    record_result "TC-005: 用户出价" "SKIP" "无拍卖中商品"
fi

# TC-006: 结束拍卖
print_section "TC-006: 结束拍卖"

if [ -n "$AUCTION_ITEM_ID" ] && [ "$AUCTION_ITEM_ID" != "" ]; then
    print_info "结束拍卖品 ID: $AUCTION_ITEM_ID"
    
    END_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/AuctionItem/EndAuction?id=$AUCTION_ITEM_ID" \
        -H "$AUTH_HEADER" \
        -H "$VERSION_HEADER")
    
    END_SUCCESS=$(echo "$END_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('success') else 'fail')" 2>/dev/null)
    END_STATUS=$(echo "$END_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('status',''))" 2>/dev/null)
    END_DEAL_USER=$(echo "$END_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('dealUserName',''))" 2>/dev/null)
    END_DEAL_PRICE=$(echo "$END_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('dealPrice',''))" 2>/dev/null)
    
    if [ "$END_SUCCESS" = "ok" ]; then
        print_success "结束拍卖成功"
        if [ -n "$END_DEAL_USER" ] && [ "$END_DEAL_USER" != "None" ] && [ "$END_DEAL_USER" != "" ]; then
            print_info "成交用户: $END_DEAL_USER"
            print_info "成交价格: $END_DEAL_PRICE 分"
        else
            print_info "流拍（无人出价）"
        fi
        record_result "TC-006: 结束拍卖" "PASS" "status=$END_STATUS, dealUser=$END_DEAL_USER, dealPrice=$END_DEAL_PRICE"
    else
        END_ERROR=$(echo "$END_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('error',{}).get('message',''))" 2>/dev/null)
        print_error "结束拍卖失败: $END_ERROR"
        record_result "TC-006: 结束拍卖" "FAIL" "error=$END_ERROR"
    fi
else
    print_info "⏭ 无拍卖中商品，跳过结束拍卖测试"
    record_result "TC-006: 结束拍卖" "SKIP" "无拍卖中商品"
fi

# ============================================================
# 流程3：支付系统测试
# ============================================================
print_header "流程3：支付系统测试"

# TC-007: 获取用户统计信息
print_section "TC-007: 获取用户统计信息"

MY_COUNT_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Client/GetMyCount" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

MY_COUNT_SUCCESS=$(echo "$MY_COUNT_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('result') is not None else 'fail')" 2>/dev/null)

if [ "$MY_COUNT_SUCCESS" = "ok" ]; then
    print_success "获取用户统计成功"
    print_info "响应: $(echo "$MY_COUNT_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); r=d.get('result',{}); print(f'余额={r.get(\"balance\",0)}, 押金={r.get(\"depositBalance\",0)}')" 2>/dev/null)"
    record_result "TC-007: 获取用户统计" "PASS" "获取成功"
else
    print_error "获取用户统计失败"
    print_info "响应: $(echo "$MY_COUNT_RESPONSE" | head -c 200)"
    record_result "TC-007: 获取用户统计" "FAIL" "解析失败"
fi

# TC-007b: 获取余额明细
print_section "TC-007b: 获取余额明细"

BALANCE_LOG_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/UserBalanceLog/GetMyAll?MaxResultCount=5" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

BALANCE_LOG_COUNT=$(echo "$BALANCE_LOG_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)

if [ -n "$BALANCE_LOG_COUNT" ] && [ "$BALANCE_LOG_COUNT" != "None" ]; then
    print_success "获取余额明细成功"
    print_info "记录数量: $BALANCE_LOG_COUNT"
    record_result "TC-007b: 获取余额明细" "PASS" "count=$BALANCE_LOG_COUNT"
else
    print_error "获取余额明细失败"
    record_result "TC-007b: 获取余额明细" "FAIL" "解析失败"
fi

# ============================================================
# 流程4：消息聊天测试
# ============================================================
print_header "流程4：消息聊天测试"

# TC-008: 获取聊天列表
print_section "TC-008: 获取聊天列表"

CHAT_LIST_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Client/GetChatList" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

CHAT_LIST_SUCCESS=$(echo "$CHAT_LIST_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('result') is not None else 'fail')" 2>/dev/null)

if [ "$CHAT_LIST_SUCCESS" = "ok" ]; then
    CHAT_LIST_COUNT=$(echo "$CHAT_LIST_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',[]); print(len(items))" 2>/dev/null)
    print_success "获取聊天列表成功"
    print_info "聊天会话数量: $CHAT_LIST_COUNT"
    record_result "TC-008: 获取聊天列表" "PASS" "count=$CHAT_LIST_COUNT"
else
    print_error "获取聊天列表失败"
    print_info "响应: $(echo "$CHAT_LIST_RESPONSE" | head -c 200)"
    record_result "TC-008: 获取聊天列表" "FAIL" "解析失败"
fi

# TC-008b: 获取表情列表
print_section "TC-008b: 获取表情列表"

EMOJI_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/ChatEmoji/GetAll" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

EMOJI_COUNT=$(echo "$EMOJI_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)

if [ -n "$EMOJI_COUNT" ] && [ "$EMOJI_COUNT" != "None" ]; then
    print_success "获取表情列表成功"
    print_info "表情数量: $EMOJI_COUNT"
    record_result "TC-008b: 获取表情列表" "PASS" "count=$EMOJI_COUNT"
else
    print_error "获取表情列表失败"
    record_result "TC-008b: 获取表情列表" "FAIL" "解析失败"
fi

# ============================================================
# 流程5：内容管理测试
# ============================================================
print_header "流程5：内容管理测试"

# TC-009: 获取最新公告
print_section "TC-009: 获取最新公告"

ANNOUNCE_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Announce/GetLatest?Id=4" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

ANNOUNCE_CONTENT=$(echo "$ANNOUNCE_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('content','')[:50])" 2>/dev/null)
ANNOUNCE_SUCCESS=$(echo "$ANNOUNCE_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('result') else 'fail')" 2>/dev/null)

if [ "$ANNOUNCE_SUCCESS" = "ok" ]; then
    print_success "获取公告成功"
    print_info "公告内容: ${ANNOUNCE_CONTENT}..."
    record_result "TC-009: 获取最新公告" "PASS" "content=${ANNOUNCE_CONTENT:0:30}"
else
    print_error "获取公告失败或无公告"
    record_result "TC-009: 获取最新公告" "FAIL" "无公告或解析失败"
fi

# TC-009b: 获取文章列表
print_section "TC-009b: 获取文章列表"

CMS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/CmsArticle/GetAllPublic?Pid=1&MaxResultCount=5" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

CMS_COUNT=$(echo "$CMS_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)

if [ -n "$CMS_COUNT" ] && [ "$CMS_COUNT" != "None" ]; then
    print_success "获取文章列表成功"
    print_info "文章数量: $CMS_COUNT"
    record_result "TC-009b: 获取文章列表" "PASS" "count=$CMS_COUNT"
else
    print_error "获取文章列表失败"
    record_result "TC-009b: 获取文章列表" "FAIL" "解析失败"
fi

# TC-009c: 获取广告位
print_section "TC-009c: 获取广告位"

AD_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/AdvertisingSpace/GetTypeList/home" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

AD_SUCCESS=$(echo "$AD_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print('ok' if d.get('result') is not None else 'fail')" 2>/dev/null)

if [ "$AD_SUCCESS" = "ok" ]; then
    AD_COUNT=$(echo "$AD_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',[]); print(len(items))" 2>/dev/null)
    print_success "获取广告位成功"
    print_info "广告位数量: $AD_COUNT"
    record_result "TC-009c: 获取广告位" "PASS" "count=$AD_COUNT"
else
    print_error "获取广告位失败"
    record_result "TC-009c: 获取广告位" "FAIL" "解析失败"
fi

# ============================================================
# 流程6：用户管理测试
# ============================================================
print_header "流程6：用户管理测试"

# TC-010: 获取用户列表
print_section "TC-010: 获取用户列表"

USER_LIST_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/User/GetAll?MaxResultCount=5" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

USER_LIST_COUNT=$(echo "$USER_LIST_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('totalCount',0); print(items)" 2>/dev/null)

if [ -n "$USER_LIST_COUNT" ] && [ "$USER_LIST_COUNT" != "None" ]; then
    print_success "获取用户列表成功"
    print_info "用户总数: $USER_LIST_COUNT"
    record_result "TC-010: 获取用户列表" "PASS" "totalCount=$USER_LIST_COUNT"
else
    print_error "获取用户列表失败"
    print_info "响应: $(echo "$USER_LIST_RESPONSE" | head -c 200)"
    record_result "TC-010: 获取用户列表" "FAIL" "解析失败"
fi

# TC-010b: 检查密码登录状态
print_section "TC-010b: 检查密码登录状态"

PWD_LOGIN_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/Account/CanUsePasswordLogin" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

PWD_LOGIN_STATUS=$(echo "$PWD_LOGIN_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('result',{}).get('canUse',''))" 2>/dev/null)

if [ -n "$PWD_LOGIN_STATUS" ] && [ "$PWD_LOGIN_STATUS" != "" ]; then
    print_success "检查密码登录状态成功"
    print_info "可使用密码登录: $PWD_LOGIN_STATUS"
    record_result "TC-010b: 检查密码登录" "PASS" "canUse=$PWD_LOGIN_STATUS"
else
    print_error "检查密码登录状态失败"
    print_info "响应: $(echo "$PWD_LOGIN_RESPONSE" | head -c 200)"
    record_result "TC-010b: 检查密码登录" "FAIL" "解析失败"
fi

# TC-010c: 获取好友列表
print_section "TC-010c: 获取好友列表"

FRIEND_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/services/app/UserFriend/GetUserFriends?Id=$SESSION_USER_ID&Status=true" \
    -H "$AUTH_HEADER" \
    -H "$VERSION_HEADER")

FRIEND_COUNT=$(echo "$FRIEND_RESPONSE" | python3 -c "import sys,json; d=json.load(sys.stdin); items=d.get('result',{}).get('items',[]); print(len(items))" 2>/dev/null)

if [ -n "$FRIEND_COUNT" ] && [ "$FRIEND_COUNT" != "None" ]; then
    print_success "获取好友列表成功"
    print_info "好友数量: $FRIEND_COUNT"
    record_result "TC-010c: 获取好友列表" "PASS" "count=$FRIEND_COUNT"
else
    print_error "获取好友列表失败"
    record_result "TC-010c: 获取好友列表" "FAIL" "解析失败"
fi

# ============================================================
# 生成测试摘要
# ============================================================
print_header "测试摘要"

log ""
log "总测试用例: $TOTAL"
log "${GREEN}通过: $PASSED${NC}"
log "${RED}失败: $FAILED${NC}"
log "${YELLOW}跳过: $SKIPPED${NC}"
log ""

PASS_RATE=0
if [ $TOTAL -gt 0 ]; then
    PASS_RATE=$((PASSED * 100 / TOTAL))
    log "通过率: ${PASS_RATE}%"
fi

log ""
log "详细结果:"
log "----------------------------------------"

for i in "${!TEST_NAMES[@]}"; do
    case "${TEST_RESULTS[$i]}" in
        PASS) log "${GREEN}  ✓ ${TEST_NAMES[$i]}${NC} | ${TEST_DETAILS[$i]}" ;;
        FAIL) log "${RED}  ✗ ${TEST_NAMES[$i]}${NC} | ${TEST_DETAILS[$i]}" ;;
        SKIP) log "${YELLOW}  ⏭ ${TEST_NAMES[$i]}${NC} | ${TEST_DETAILS[$i]}" ;;
    esac
done

log ""
log "========================================"
log "测试完成: $(date '+%Y-%m-%d %H:%M:%S')"
log "日志文件: $RESULT_LOG"
log "========================================"

# 生成 JSON 摘要
cat > "$RESULT_JSON" << EOF
{
    "testDate": "$(date '+%Y-%m-%d %H:%M:%S')",
    "baseUrl": "$BASE_URL",
    "summary": {
        "total": $TOTAL,
        "passed": $PASSED,
        "failed": $FAILED,
        "skipped": $SKIPPED,
        "passRate": "${PASS_RATE}%"
    },
    "tests": [
$(for i in "${!TEST_NAMES[@]}"; do
    echo "        {\"name\": \"${TEST_NAMES[$i]}\", \"status\": \"${TEST_RESULTS[$i]}\", \"detail\": \"${TEST_DETAILS[$i]}\"}$([ $i -lt $((${#TEST_NAMES[@]} - 1)) ] && echo ',')"
done)
    ]
}
EOF

log ""
log "JSON 摘要已保存: $RESULT_JSON"
