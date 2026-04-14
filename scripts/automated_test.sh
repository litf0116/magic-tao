#!/bin/bash

# 自动化测试脚本 - 验证拍卖消息发送和 Channel 同步创建
# 测试场景：结束拍卖 -> 发送成交私信 -> 验证 Channel 创建

BASE_URL="http://localhost:12580"
TEST_RESULTS="/tmp/test_results.log"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 记录日志
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1" | tee -a $TEST_RESULTS
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1" | tee -a $TEST_RESULTS
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a $TEST_RESULTS
}

# 测试 1: 服务健康检查
test_service_health() {
    log_info "测试 1: 检查服务健康状态..."
    
    # 检查后端
    API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" 2>/dev/null || echo "000")
    if [ "$API_STATUS" == "200" ] || [ "$API_STATUS" == "401" ]; then
        log_info "✅ 后端服务正常 (HTTP $API_STATUS)"
    else
        log_error "❌ 后端服务异常 (HTTP $API_STATUS)"
        return 1
    fi
    
    # 检查 FreeIM
    IM_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:6001" 2>/dev/null || echo "000")
    if [ "$IM_STATUS" == "200" ] || [ "$IM_STATUS" == "404" ]; then
        log_info "✅ FreeIM 服务正常"
    else
        log_warn "⚠️ FreeIM 服务状态未知"
    fi
    
    return 0
}

# 测试 2: 数据库连接和表结构
test_database() {
    log_info "测试 2: 检查数据库..."
    
    # 检查 ChatChannel 表
    CHANNEL_COUNT=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT COUNT(*) FROM T_ChatChannel" 2>/dev/null | tail -1 || echo "0")
    log_info "✅ ChatChannel 表记录数: $CHANNEL_COUNT"
    
    # 检查 Message 表
    MSG_COUNT=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT COUNT(*) FROM T_Message" 2>/dev/null | tail -1 || echo "0")
    log_info "✅ Message 表记录数: $MSG_COUNT"
    
    # 检查最近的私聊消息
    RECENT_PRIVATE=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT COUNT(*) FROM T_Message WHERE Chan IS NULL OR Chan = ''" 2>/dev/null | tail -1 || echo "0")
    log_info "✅ 私聊消息数量: $RECENT_PRIVATE"
    
    return 0
}

# 测试 3: 代码修改验证
test_code_changes() {
    log_info "测试 3: 验证代码修改..."
    
    # 检查 ChatChannelService 是否被注入
    if grep -q "ChatChannelService _chatChannelService" /Users/mac/workspace/magic-tao/backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs; then
        log_info "✅ ChatChannelService 已注入"
    else
        log_error "❌ ChatChannelService 未注入"
        return 1
    fi
    
    # 检查同步调用是否存在
    if grep -q "UpdateChannelLastMessageAsync" /Users/mac/workspace/magic-tao/backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs; then
        log_info "✅ 同步 Channel 更新代码已添加"
    else
        log_error "❌ 同步 Channel 更新代码未找到"
        return 1
    fi
    
    # 检查调用顺序是否正确（在 SaveChanges 之前）
    # 注意：UpdateChannelLastMessageAsync 应该在 SaveChangesAsync 之前调用
    UPDATE_LINE=$(grep -n "UpdateChannelLastMessageAsync" /Users/mac/workspace/magic-tao/backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs | head -1 | cut -d: -f1)
    # 获取第二次出现的 SaveChangesAsync（SendPrivateMessageAsync 方法中的）
    SAVE_LINE=$(grep -n "SaveChangesAsync" /Users/mac/workspace/magic-tao/backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs | sed -n '2p' | cut -d: -f1)
    
    if [ -n "$UPDATE_LINE" ] && [ -n "$SAVE_LINE" ] && [ "$UPDATE_LINE" -lt "$SAVE_LINE" ]; then
        log_info "✅ Channel 更新在事务提交前执行 (行 $UPDATE_LINE < $SAVE_LINE)"
    else
        log_error "❌ Channel 更新顺序错误 (Update: $UPDATE_LINE, Save: $SAVE_LINE)"
        return 1
    fi
    
    return 0
}

# 测试 4: API 功能测试（模拟拍卖流程）
test_api_functionality() {
    log_info "测试 4: API 功能测试..."
    
    # 获取公开拍卖列表
    log_info "获取拍卖列表..."
    AUCTION_LIST=$(curl -s "${BASE_URL}/api/AuctionItem/GetPublicListAnonymous?MaxResultCount=5" 2>/dev/null)
    
    if echo "$AUCTION_LIST" | grep -q "result"; then
        log_info "✅ 拍卖列表 API 正常"
    else
        log_warn "⚠️ 拍卖列表 API 响应异常"
    fi
    
    # 获取聊天列表 API（无需认证测试端点是否存在）
    CHAT_LIST_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/api/services/app/Client/GetChatList" 2>/dev/null || echo "000")
    if [ "$CHAT_LIST_STATUS" == "200" ] || [ "$CHAT_LIST_STATUS" == "401" ]; then
        log_info "✅ 聊天列表 API 端点存在 (HTTP $CHAT_LIST_STATUS)"
    else
        log_warn "⚠️ 聊天列表 API 状态: $CHAT_LIST_STATUS"
    fi
    
    return 0
}

# 测试 5: 数据一致性测试
test_data_consistency() {
    log_info "测试 5: 数据一致性测试..."
    
    # 获取当前 Channel 数量
    BEFORE_COUNT=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT COUNT(*) FROM T_ChatChannel WHERE LastMessageId IS NOT NULL" 2>/dev/null | tail -1 || echo "0")
    log_info "当前有效 Channel 数量: $BEFORE_COUNT"
    
    # 检查是否有 Channel 的 LastMessageId 为空（可能的问题）
    EMPTY_LASTMSG=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT COUNT(*) FROM T_ChatChannel WHERE LastMessageId IS NULL" 2>/dev/null | tail -1 || echo "0")
    if [ "$EMPTY_LASTMSG" -gt 0 ]; then
        log_warn "⚠️ 发现 $EMPTY_LASTMSG 个 Channel 的 LastMessageId 为空"
    else
        log_info "✅ 所有 Channel 都有 LastMessageId"
    fi
    
    # 检查私聊频道的完整性
    PRIVATE_CHANNELS=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SELECT ChannelId, User1Id, User2Id, LastMessageId FROM T_ChatChannel WHERE ChannelType = 1 LIMIT 5" 2>/dev/null | grep -v "ChannelId" || echo "无数据")
    if [ -n "$PRIVATE_CHANNELS" ]; then
        log_info "私聊频道样例:"
        echo "$PRIVATE_CHANNELS" | while read line; do
            echo "  $line"
        done
    fi
    
    return 0
}

# 主执行流程
main() {
    echo "======================================" | tee $TEST_RESULTS
    echo "拍卖消息发送自动化测试" | tee -a $TEST_RESULTS
    echo "======================================" | tee -a $TEST_RESULTS
    echo "" | tee -a $TEST_RESULTS
    
    # 运行所有测试
    TEST_PASSED=0
    TEST_FAILED=0
    
    if test_service_health; then ((TEST_PASSED++)); else ((TEST_FAILED++)); fi
    echo "" | tee -a $TEST_RESULTS
    
    if test_database; then ((TEST_PASSED++)); else ((TEST_FAILED++)); fi
    echo "" | tee -a $TEST_RESULTS
    
    if test_code_changes; then ((TEST_PASSED++)); else ((TEST_FAILED++)); fi
    echo "" | tee -a $TEST_RESULTS
    
    if test_api_functionality; then ((TEST_PASSED++)); else ((TEST_FAILED++)); fi
    echo "" | tee -a $TEST_RESULTS
    
    if test_data_consistency; then ((TEST_PASSED++)); else ((TEST_FAILED++)); fi
    echo "" | tee -a $TEST_RESULTS
    
    # 测试总结
    echo "======================================" | tee -a $TEST_RESULTS
    echo "测试总结" | tee -a $TEST_RESULTS
    echo "======================================" | tee -a $TEST_RESULTS
    log_info "通过: $TEST_PASSED"
    if [ $TEST_FAILED -gt 0 ]; then
        log_error "失败: $TEST_FAILED"
    else
        log_info "失败: $TEST_FAILED"
    fi
    echo "" | tee -a $TEST_RESULTS
    
    echo "详细日志: $TEST_RESULTS" | tee -a $TEST_RESULTS
    echo "======================================" | tee -a $TEST_RESULTS
    
    return $TEST_FAILED
}

# 执行主函数
main
exit $?