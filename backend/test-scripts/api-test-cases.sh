#!/bin/bash
# ============================================================
# 前端实际使用接口测试用例
# 测试范围: PC 端 + UniApp 端使用的接口
# 测试账号: feifei/123456 (普通用户), admin/123456 (管理员)
# ============================================================

set -e

BASE_URL="http://127.0.0.1:12580"
PASS=0
FAIL=0
TOTAL=0
TEST_RESULTS=()

# 颜色定义
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}============================================${NC}"
echo -e "${YELLOW}  前端实际使用接口测试用例${NC}"
echo -e "${YELLOW}  测试时间: $(date '+%Y-%m-%d %H:%M:%S')${NC}"
echo -e "${YELLOW}============================================${NC}"

# 获取 Token
echo -e "\n${YELLOW}【获取测试 Token】${NC}"

ADMIN_TOKEN=$(curl -s -X POST "$BASE_URL/api/TokenAuth/Authenticate" \
  -H 'Content-Type: application/json' \
  -d '{"userNameOrEmailAddress": "18012341234", "password": "123456"}' | jq -r '.result.accessToken')

FEIFEI_TOKEN=$(curl -s -X POST "$BASE_URL/api/TokenAuth/Authenticate" \
  -H 'Content-Type: application/json' \
  -d '{"userNameOrEmailAddress": "feifei", "password": "123456"}' | jq -r '.result.accessToken')

if [[ -z "$ADMIN_TOKEN" || "$ADMIN_TOKEN" == "null" ]]; then
  echo -e "${RED}❌ 无法获取 Admin Token，测试终止${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Admin Token: ${ADMIN_TOKEN:0:30}...${NC}"
echo -e "${GREEN}✅ Feifei Token: ${FEIFEI_TOKEN:0:30}...${NC}"

# 测试函数
test_api() {
  local module="$1"
  local name="$2"
  local method="$3"
  local url="$4"
  local token="$5"
  local data="$6"
  local expect_success="${7:-true}"
  local format="${8:-standard}"
  
  ((TOTAL++))
  
  if [[ "$method" == "GET" ]]; then
    RESULT=$(curl -s "$url" -H "Authorization: Bearer $token")
  else
    RESULT=$(curl -s -X $method "$url" \
      -H "Authorization: Bearer $token" \
      -H 'Content-Type: application/json' \
      -d "$data")
  fi
  
  if [[ "$format" == "dontwrap" ]]; then
    SUCCESS=$(echo "$RESULT" | jq 'has("signature") or has("bucket")')
  else
    SUCCESS=$(echo "$RESULT" | jq -r '.success // false')
  fi
  
  if [[ "$SUCCESS" == "$expect_success" ]]; then
    echo -e "${GREEN}✅ [$module] $name${NC}"
    ((PASS++))
    TEST_RESULTS+=("PASS|$module|$name")
  else
    ERROR=$(echo "$RESULT" | jq -r '.error.message // "Unknown"')
    echo -e "${RED}❌ [$module] $name - $ERROR${NC}"
    ((FAIL++))
    TEST_RESULTS+=("FAIL|$module|$name|$ERROR")
  fi
}

# ============================================================
# 1. 认证模块
# ============================================================
echo -e "\n${YELLOW}【1. 认证模块】${NC}"

test_api "认证" "用户登录" "POST" "$BASE_URL/api/TokenAuth/Authenticate" "$FEIFEI_TOKEN" \
  '{"userNameOrEmailAddress": "feifei", "password": "123456"}'

test_api "认证" "微信小程序登录" "POST" "$BASE_URL/api/TokenAuth/WeixinMiniAuthenticate" "$FEIFEI_TOKEN" \
  '{"code": "test_code"}' "false"

test_api "认证" "微信手机登录" "POST" "$BASE_URL/api/TokenAuth/WeixinMiniPhoneAuthenticate" "$FEIFEI_TOKEN" \
  '{"phoneNumber": "18012341234", "code": "test_code"}' "false"

# ============================================================
# 2. 用户管理
# ============================================================
echo -e "\n${YELLOW}【2. 用户管理】${NC}"

test_api "用户" "获取当前用户" "GET" "$BASE_URL/api/services/app/User/GetCurrentUser" "$FEIFEI_TOKEN"

test_api "用户" "获取用户列表" "GET" "$BASE_URL/api/services/app/User/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

test_api "用户" "获取用户详情" "GET" "$BASE_URL/api/services/app/User/Get?id=7509" "$FEIFEI_TOKEN"

test_api "用户" "创建用户" "POST" "$BASE_URL/api/services/app/User/Create" "$ADMIN_TOKEN" \
  '{"userName": "test_user_'$$'", "password": "Test@123456", "emailAddress": "test_'$$'@test.com", "name": "Test", "surname": "User", "phoneNumber": "1380000'$$'", "isActive": true}'

test_api "用户" "更新自己的信息" "PUT" "$BASE_URL/api/services/app/User/Update" "$FEIFEI_TOKEN" \
  '{"id": 7509, "userName": "feifei", "name": "飞飞", "surname": "测试", "emailAddress": "feifei@test.com", "isActive": true}'

test_api "用户" "修改密码" "POST" "$BASE_URL/api/services/app/User/ChangePassword" "$FEIFEI_TOKEN" \
  '{"currentPassword": "123456", "newPassword": "123456"}'

# ============================================================
# 3. 拍卖品管理
# ============================================================
echo -e "\n${YELLOW}【3. 拍卖品管理】${NC}"

test_api "拍卖" "获取拍卖品列表" "GET" "$BASE_URL/api/services/app/AuctionItem/GetPublicList?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "拍卖" "获取所有拍卖品" "GET" "$BASE_URL/api/services/app/AuctionItem/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

# 获取拍卖中商品
AUCTION_ID=$(curl -s "$BASE_URL/api/services/app/AuctionItem/GetPublicList?Status=2&MaxResultCount=1" \
  -H "Authorization: Bearer $FEIFEI_TOKEN" | jq -r '.result.items[0].id')

test_api "拍卖" "获取拍卖品详情" "GET" "$BASE_URL/api/services/app/AuctionItem/Get?id=$AUCTION_ID" "$FEIFEI_TOKEN"

test_api "拍卖" "获取编辑信息" "GET" "$BASE_URL/api/services/app/AuctionItem/GetForEdit?id=$AUCTION_ID" "$ADMIN_TOKEN"

test_api "拍卖" "获取拍卖中商品" "POST" "$BASE_URL/api/services/app/AuctionItem/GetAuctionMidList" "$FEIFEI_TOKEN" \
  '{"skipCount": 0, "maxResultCount": 10}'

test_api "拍卖" "获取我拍得的商品" "GET" "$BASE_URL/api/services/app/AuctionItem/GetMySuccessList?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "拍卖" "创建拍卖品" "POST" "$BASE_URL/api/services/app/AuctionItem/Create" "$ADMIN_TOKEN" \
  '{"name": "测试拍品_'$$'", "imageUrl": "https://picsum.photos/400/300", "description": "测试描述", "startingPrice": 100, "sellerInfo": "测试卖家", "sellerId": 2}'

# ============================================================
# 4. 出价模块
# ============================================================
echo -e "\n${YELLOW}【4. 出价模块】${NC}"

# 获取当前价格
CURRENT_PRICE=$(curl -s "$BASE_URL/api/services/app/AuctionItem/Get?id=$AUCTION_ID" \
  -H "Authorization: Bearer $FEIFEI_TOKEN" | jq -r '.result.currentPrice // .result.startingPrice')

# 计算正确加价
if [[ $CURRENT_PRICE -le 1000 ]]; then
  BID_PRICE=$((CURRENT_PRICE + 5))
elif [[ $CURRENT_PRICE -le 2000 ]]; then
  BID_PRICE=$((CURRENT_PRICE + 10))
elif [[ $CURRENT_PRICE -le 5000 ]]; then
  BID_PRICE=$((CURRENT_PRICE + 20))
else
  BID_PRICE=$((CURRENT_PRICE + 50))
fi

test_api "出价" "正确出价" "POST" "$BASE_URL/api/services/app/AuctionItem/Bid" "$FEIFEI_TOKEN" \
  "{\"auctionItemId\": $AUCTION_ID, \"bidPrice\": $BID_PRICE}"

test_api "出价" "低于加价幅度出价" "POST" "$BASE_URL/api/services/app/AuctionItem/Bid" "$FEIFEI_TOKEN" \
  "{\"auctionItemId\": $AUCTION_ID, \"bidPrice\": $((CURRENT_PRICE + 1))}" "false"

# ============================================================
# 5. 消息模块
# ============================================================
echo -e "\n${YELLOW}【5. 消息模块】${NC}"

test_api "消息" "获取频道历史" "GET" "$BASE_URL/api/services/app/Message/GetChanHistory?chan=-1_auction&limit=10" "$FEIFEI_TOKEN"

test_api "消息" "获取私聊历史" "GET" "$BASE_URL/api/services/app/Message/GetPrivateHistory?userId=2&limit=10" "$FEIFEI_TOKEN"

test_api "消息" "获取频道最后ID" "GET" "$BASE_URL/api/services/app/Message/GetChanLastId?chan=-1_auction" "$FEIFEI_TOKEN"

test_api "消息" "获取私聊最后ID" "GET" "$BASE_URL/api/services/app/Message/GetPrivateLastId?userId=2" "$FEIFEI_TOKEN"

# ============================================================
# 6. 聊天群组
# ============================================================
echo -e "\n${YELLOW}【6. 聊天群组】${NC}"

test_api "群组" "获取群组列表" "GET" "$BASE_URL/api/services/app/ChatGroup/GetAll?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "群组" "获取公开群组" "GET" "$BASE_URL/api/services/app/ChatGroup/GetAllPublic?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "群组" "创建群组" "POST" "$BASE_URL/api/services/app/ChatGroup/Create" "$FEIFEI_TOKEN" \
  '{"title": "TestGrp"}'

test_api "群组" "获取群组详情" "GET" "$BASE_URL/api/services/app/ChatGroup/Get?id=49" "$FEIFEI_TOKEN"

test_api "群组" "获取群组用户" "GET" "$BASE_URL/api/services/app/ChatGroup/GetGroupUser?id=49" "$FEIFEI_TOKEN"

# ============================================================
# 7. 公告管理
# ============================================================
echo -e "\n${YELLOW}【7. 公告管理】${NC}"

test_api "公告" "获取公告列表" "GET" "$BASE_URL/api/services/app/Announce/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

test_api "公告" "获取公开公告" "GET" "$BASE_URL/api/services/app/Announce/GetAllPublic?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "公告" "获取最新公告" "GET" "$BASE_URL/api/services/app/Announce/GetLatest" "$FEIFEI_TOKEN"

test_api "公告" "创建公告" "POST" "$BASE_URL/api/services/app/Announce/Create" "$ADMIN_TOKEN" \
  '{"categoryId": 1, "content": "测试公告_'$$'", "imageUrl": "https://picsum.photos/800/400", "sort": 100}'

# ============================================================
# 8. 余额管理
# ============================================================
echo -e "\n${YELLOW}【8. 余额管理】${NC}"

test_api "余额" "获取我的余额记录" "GET" "$BASE_URL/api/services/app/UserBalanceLog/GetMyAll?MaxResultCount=10" "$FEIFEI_TOKEN"

test_api "余额" "获取我的保证金记录" "GET" "$BASE_URL/api/services/app/UserDepositLog/GetMyAll?MaxResultCount=10" "$FEIFEI_TOKEN"

# ============================================================
# 9. 提现模块
# ============================================================
echo -e "\n${YELLOW}【9. 提现模块】${NC}"

test_api "提现" "提现" "POST" "$BASE_URL/api/services/app/Client/PayWithdrawal" "$FEIFEI_TOKEN" \
  '{"amount": 10}'

test_api "提现" "获取提现列表" "GET" "$BASE_URL/api/services/app/WithdrawalAmountService/Page?MaxResultCount=10" "$ADMIN_TOKEN"

# ============================================================
# 10. 好友功能
# ============================================================
echo -e "\n${YELLOW}【10. 好友功能】${NC}"

test_api "好友" "获取好友列表" "GET" "$BASE_URL/api/services/app/UserFriend/GetUserFriends?userId=7509" "$FEIFEI_TOKEN"

test_api "好友" "获取好友数量" "GET" "$BASE_URL/api/services/app/UserFriend/GetUserFriendCount?userId=7509" "$FEIFEI_TOKEN"

# ============================================================
# 11. 敏感词管理
# ============================================================
echo -e "\n${YELLOW}【11. 敏感词管理】${NC}"

test_api "敏感词" "获取敏感词列表" "GET" "$BASE_URL/api/services/app/SensitiveWord/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

test_api "敏感词" "创建敏感词" "POST" "$BASE_URL/api/services/app/SensitiveWord/Create" "$ADMIN_TOKEN" \
  '{"word": "测试词_'$$'"}'

# ============================================================
# 12. 角色管理
# ============================================================
echo -e "\n${YELLOW}【12. 角色管理】${NC}"

test_api "角色" "获取角色列表" "GET" "$BASE_URL/api/services/app/Role/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

test_api "角色" "获取所有权限" "GET" "$BASE_URL/api/services/app/Role/GetAllPermissions" "$ADMIN_TOKEN"

# ============================================================
# 13. 会话信息
# ============================================================
echo -e "\n${YELLOW}【13. 会话信息】${NC}"

test_api "会话" "获取当前登录信息" "GET" "$BASE_URL/api/services/app/Session/GetCurrentLoginInformations" "$FEIFEI_TOKEN"

# ============================================================
# 14. App发布
# ============================================================
echo -e "\n${YELLOW}【14. App发布】${NC}"

test_api "App" "检查更新" "GET" "$BASE_URL/api/services/app/AppRelease/CheckUpdate?platform=android&version=1.0.0" "$FEIFEI_TOKEN"

# ============================================================
# 15. 文件上传
# ============================================================
echo -e "\n${YELLOW}【15. 文件上传】${NC}"

test_api "上传" "获取上传签名" "GET" "$BASE_URL/api/services/app/Upload/GetSignature?data=test&policy=test" "$FEIFEI_TOKEN" "" "true" "dontwrap"

# ============================================================
# 16. 租户管理
# ============================================================
echo -e "\n${YELLOW}【16. 租户管理】${NC}"

test_api "租户" "获取租户列表" "GET" "$BASE_URL/api/services/app/Tenant/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

# ============================================================
# 17. 审计日志
# ============================================================
echo -e "\n${YELLOW}【17. 审计日志】${NC}"

test_api "审计" "获取审计日志" "GET" "$BASE_URL/api/services/app/AuditLog/GetAuditLogs?MaxResultCount=10" "$ADMIN_TOKEN"

# ============================================================
# 18. 聊天表情
# ============================================================
echo -e "\n${YELLOW}【18. 聊天表情】${NC}"

test_api "表情" "获取表情列表" "GET" "$BASE_URL/api/services/app/ChatEmoji/GetAll?MaxResultCount=10" "$FEIFEI_TOKEN"

# ============================================================
# 19. 禁言管理
# ============================================================
echo -e "\n${YELLOW}【19. 禁言管理】${NC}"

test_api "禁言" "获取禁言列表" "GET" "$BASE_URL/api/services/app/BanedUser/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

# ============================================================
# 20. 客户端功能
# ============================================================
echo -e "\n${YELLOW}【20. 客户端功能】${NC}"

test_api "客户端" "获取聊天列表" "GET" "$BASE_URL/api/services/app/Client/GetChatList" "$FEIFEI_TOKEN"

test_api "客户端" "获取我的统计" "GET" "$BASE_URL/api/services/app/Client/GetMyCount" "$FEIFEI_TOKEN"

# ============================================================
# 21. CMS文章
# ============================================================
echo -e "\n${YELLOW}【21. CMS文章】${NC}"

test_api "CMS" "获取文章列表" "GET" "$BASE_URL/api/services/app/CmsArticle/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

test_api "CMS" "获取公开文章" "GET" "$BASE_URL/api/services/app/CmsArticle/GetAllPublic?MaxResultCount=10" "$FEIFEI_TOKEN"

# ============================================================
# 22. CMS分类
# ============================================================
echo -e "\n${YELLOW}【22. CMS分类】${NC}"

test_api "CMS" "获取分类列表" "GET" "$BASE_URL/api/services/app/CmsCategory/GetAll?MaxResultCount=10" "$ADMIN_TOKEN"

# ============================================================
# 23. 热词管理
# ============================================================
echo -e "\n${YELLOW}【23. 热词管理】${NC}"

test_api "热词" "获取热词列表" "GET" "$BASE_URL/api/HotWords/GetList" "$FEIFEI_TOKEN"

# ============================================================
# 24. 广告位
# ============================================================
echo -e "\n${YELLOW}【24. 广告位】${NC}"

test_api "广告" "获取广告位列表" "GET" "$BASE_URL/api/AdvertisingSpace/GetList" "$ADMIN_TOKEN"

# ============================================================
# 25. 群聊等级
# ============================================================
echo -e "\n${YELLOW}【25. 群聊等级】${NC}"

test_api "等级" "获取群聊等级列表" "GET" "$BASE_URL/api/GroupChatLevelSettings/GetList" "$FEIFEI_TOKEN"

# ============================================================
# 测试汇总
# ============================================================
echo -e "\n${YELLOW}============================================${NC}"
echo -e "${YELLOW}  测试结果汇总${NC}"
echo -e "${YELLOW}============================================${NC}"
echo -e "✅ 通过: $PASS"
echo -e "❌ 失败: $FAIL"
echo -e "📊 总计: $TOTAL"
echo -e "📈 通过率: $(echo "scale=1; $PASS * 100 / $TOTAL" | bc)%"

# 生成测试报告
REPORT_FILE="test-results/api-test-report-$(date '+%Y%m%d_%H%M%S').md"
mkdir -p test-results

cat > "$REPORT_FILE" << EOF
# 前端接口测试报告

**测试时间**: $(date '+%Y-%m-%d %H:%M:%S')
**测试环境**: $BASE_URL
**测试账号**: feifei/123456, admin/123456

## 测试结果

| 指标 | 数值 |
|------|------|
| 总计 | $TOTAL |
| 通过 | $PASS |
| 失败 | $FAIL |
| 通过率 | $(echo "scale=1; $PASS * 100 / $TOTAL" | bc)% |

## 测试用例明细

| # | 模块 | 接口 | 方法 | 结果 |
|---|------|------|------|------|
EOF

COUNTER=0
for result in "${TEST_RESULTS[@]}"; do
  IFS='|' read -ra PARTS <<< "$result"
  ((COUNTER++))
  STATUS="${PARTS[0]}"
  MODULE="${PARTS[1]}"
  NAME="${PARTS[2]}"
  if [[ "$STATUS" == "PASS" ]]; then
    echo "| $COUNTER | $MODULE | $NAME | - | ✅ |" >> "$REPORT_FILE"
  else
    echo "| $COUNTER | $MODULE | $NAME | - | ❌ ${PARTS[3]} |" >> "$REPORT_FILE"
  fi
done

cat >> "$REPORT_FILE" << 'EOF'

## 测试说明

1. 所有测试用例均使用 curl 调用真实接口
2. 测试数据已准备就绪（拍卖商品、公告、用户余额等）
3. 测试脚本位置: `test-scripts/api-test-cases.sh`
4. 未使用接口记录: `test-scripts/UNUSED-API-REPORT.md`
5. 测试数据准备: `test-scripts/TEST-DATA-PREPARATION.md`
EOF

echo -e "\n📄 测试报告已保存: $REPORT_FILE"

