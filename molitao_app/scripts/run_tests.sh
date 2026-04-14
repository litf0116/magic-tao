#!/bin/bash

# Flutter 自动化测试运行脚本
# 使用方法: ./scripts/run_tests.sh [选项]

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 默认配置
DEVICE_ID=""
TEST_TYPE="all"
VERBOSE=false

# 显示帮助信息
show_help() {
    echo -e "${BLUE}Flutter 自动化测试运行脚本${NC}"
    echo ""
    echo "使用方法: ./scripts/run_tests.sh [选项]"
    echo ""
    echo "选项:"
    echo "  -d, --device    指定设备 ID"
    echo "  -t, --type      测试类型: all, integration, e2e, auth, forum, auction, chat"
    echo "  -v, --verbose   详细输出"
    echo "  -h, --help      显示帮助信息"
    echo ""
    echo "示例:"
    echo "  ./scripts/run_tests.sh -d 827af65d0722 -t integration"
    echo "  ./scripts/run_tests.sh -d 827af65d0722 -t auth"
    echo "  ./scripts/run_tests.sh --device 827af65d0722 --type e2e"
    exit 0
}

# 解析参数
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -d|--device)
                DEVICE_ID="$2"
                shift 2
                ;;
            -t|--type)
                TEST_TYPE="$2"
                shift 2
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            -h|--help)
                show_help
                ;;
            *)
                echo -e "${RED}未知选项: $1${NC}"
                show_help
                ;;
        esac
    done
}

# 检查设备连接
check_device() {
    echo -e "${BLUE}检查设备连接...${NC}"
    
    if [ -z "$DEVICE_ID" ]; then
        DEVICES=$(flutter devices | grep -E "^\s+[a-z0-9]+" | head -1 | awk '{print $3}')
        if [ -n "$DEVICES" ]; then
            DEVICE_ID="$DEVICES"
            echo -e "${GREEN}自动检测到设备: $DEVICE_ID${NC}"
        else
            echo -e "${RED}未找到连接的设备${NC}"
            echo "请使用 flutter devices 查看可用设备"
            exit 1
        fi
    fi
    
    if ! flutter devices | grep -q "$DEVICE_ID"; then
        echo -e "${RED}设备 $DEVICE_ID 未连接${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}设备 $DEVICE_ID 已连接${NC}"
}

# 运行 Integration Test
run_integration_test() {
    echo -e "${BLUE}运行 Flutter Integration Test...${NC}"
    
    local test_file=$1
    local test_name=$2
    
    if [ -n "$test_name" ]; then
        flutter test "integration_test/$test_file" --name "$test_name" -d "$DEVICE_ID"
    else
        flutter test "integration_test/$test_file" -d "$DEVICE_ID"
    fi
    
    echo -e "${GREEN}Integration Test 完成${NC}"
}

# 运行 Patrol E2E Test
run_e2e_test() {
    echo -e "${BLUE}运行 Patrol E2E Test...${NC}"
    
    local test_file=$1
    
    patrol test "integration_test/$test_file" -d "$DEVICE_ID"
    
    echo -e "${GREEN}Patrol E2E Test 完成${NC}"
}

# 运行所有测试
run_all_tests() {
    echo -e "${BLUE}运行所有测试...${NC}"
    
    # Integration Tests
    run_integration_test "app_integration_test.dart"
    run_integration_test "auth_test.dart"
    run_integration_test "forum_test.dart"
    run_integration_test "auction_test.dart"
    run_integration_test "chat_test.dart"
    
    # E2E Tests
    run_e2e_test "app_e2e_test.dart"
    run_e2e_test "comprehensive_e2e_test.dart"
    
    echo -e "${GREEN}所有测试完成${NC}"
}

# 主函数
main() {
    parse_args "$@"
    check_device
    
    echo -e "${YELLOW}测试类型: $TEST_TYPE${NC}"
    echo -e "${YELLOW}设备 ID: $DEVICE_ID${NC}"
    echo ""
    
    case $TEST_TYPE in
        all)
            run_all_tests
            ;;
        integration)
            run_integration_test "app_integration_test.dart"
            ;;
        e2e)
            run_e2e_test "app_e2e_test.dart"
            run_e2e_test "comprehensive_e2e_test.dart"
            ;;
        auth)
            run_integration_test "auth_test.dart"
            ;;
        forum)
            run_integration_test "forum_test.dart"
            ;;
        auction)
            run_integration_test "auction_test.dart"
            ;;
        chat)
            run_integration_test "chat_test.dart"
            ;;
        *)
            echo -e "${RED}未知的测试类型: $TEST_TYPE${NC}"
            show_help
            ;;
    esac
}

main "$@"