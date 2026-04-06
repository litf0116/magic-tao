#!/bin/bash

# 统一测试覆盖率脚本
# 运行所有模块的单元测试并生成覆盖率报告

set -e

PROJECT_ROOT=$(pwd)

echo "╔════════════════════════════════════════════╗"
echo "║   魔力淘 - 统一测试覆盖率统计             ║"
echo "╚════════════════════════════════════════════╝"
echo ""

# 颜色定义
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 测试结果数组
declare -a TEST_RESULTS

run_backend_tests() {
    echo -e "${YELLOW}[1/4] 运行 Backend 测试...${NC}"
    cd "$PROJECT_ROOT/backend"

    if [ -f "./scripts/run-tests-with-coverage.sh" ]; then
        if bash ./scripts/run-tests-with-coverage.sh; then
            TEST_RESULTS+=("✅ Backend")
        else
            TEST_RESULTS+=("❌ Backend")
        fi
    else
        echo "⚠️  Backend 测试脚本不存在"
        TEST_RESULTS+=("⏭️  Backend (跳过)")
    fi

    echo ""
}

run_pc_tests() {
    echo -e "${YELLOW}[2/4] 运行 PC 测试...${NC}"
    cd "$PROJECT_ROOT/pc"

    if command -v vitest &> /dev/null; then
        if npm run test:coverage 2>/dev/null; then
            TEST_RESULTS+=("✅ PC")
        else
            TEST_RESULTS+=("❌ PC")
        fi
    else
        echo "⚠️  Vitest 未安装，跳过 PC 测试"
        TEST_RESULTS+=("⏭️  PC (跳过)")
    fi

    echo ""
}

run_flutter_tests() {
    echo -e "${YELLOW}[3/4] 运行 Flutter 测试...${NC}"
    cd "$PROJECT_ROOT/molitao_app"

    if command -v flutter &> /dev/null; then
        if [ -f "./scripts/run-tests-with-coverage.sh" ]; then
            if bash ./scripts/run-tests-with-coverage.sh; then
                TEST_RESULTS+=("✅ Flutter")
            else
                TEST_RESULTS+=("❌ Flutter")
            fi
        else
            flutter test --coverage 2>/dev/null && \
            TEST_RESULTS+=("✅ Flutter") || \
            TEST_RESULTS+=("❌ Flutter")
        fi
    else
        echo "⚠️  Flutter 未安装，跳过 Flutter 测试"
        TEST_RESULTS+=("⏭️  Flutter (跳过)")
    fi

    echo ""
}

run_uniapp_tests() {
    echo -e "${YELLOW}[4/4] 运行 UniApp 测试...${NC}"
    cd "$PROJECT_ROOT/molitao_uniapp"

    if [ -f "./node_modules/.bin/vitest" ]; then
        if npm run test:coverage 2>/dev/null; then
            TEST_RESULTS+=("✅ UniApp")
        else
            TEST_RESULTS+=("❌ UniApp")
        fi
    else
        echo "⚠️  Vitest 未安装，跳过 UniApp 测试"
        TEST_RESULTS+=("⏭️  UniApp (跳过)")
    fi

    echo ""
}

# 运行所有测试
run_backend_tests
run_pc_tests
run_flutter_tests
run_uniapp_tests

# 显示结果汇总
cd "$PROJECT_ROOT"

echo "╔════════════════════════════════════════════╗"
echo "║              测试结果汇总                  ║"
echo "╚════════════════════════════════════════════╝"
echo ""

for result in "${TEST_RESULTS[@]}"; do
    echo "  $result"
done

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# 统计成功和失败数量
SUCCESS=$(printf '%s\n' "${TEST_RESULTS[@]}" | grep -c "✅" || true)
FAILED=$(printf '%s\n' "${TEST_RESULTS[@]}" | grep -c "❌" || true)
SKIPPED=$(printf '%s\n' "${TEST_RESULTS[@]}" | grep -c "⏭️" || true)

echo ""
echo "📊 统计:"
echo "  ✅ 通过: $SUCCESS"
echo "  ❌ 失败: $FAILED"
echo "  ⏭️  跳过: $SKIPPED"
echo ""

if [ "$FAILED" -eq 0 ]; then
    echo -e "${GREEN}✨ 所有测试通过！${NC}"
    exit 0
else
    echo -e "${RED}⚠️  有 $FAILED 个模块测试失败${NC}"
    exit 1
fi