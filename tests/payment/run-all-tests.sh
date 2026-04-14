#!/bin/bash

echo "======================================"
echo "魔力淘 - 支付功能完整测试套件"
echo "======================================"
echo ""
echo "测试用户: 7509 (feifei)"
echo "测试环境: 本地开发环境"
echo ""

TEST_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "可用测试脚本:"
echo ""
echo "1. 完整支付流程测试（推荐）"
echo "   ${TEST_DIR}/test-payment-api.sh"
echo "   - 生成用户 Token"
echo "   - 创建支付订单"
echo "   - 模拟支付成功"
echo "   - 验证余额更新"
echo ""
echo "2. 支付回调流程测试"
echo "   ${TEST_DIR}/test-payment-callback.sh"
echo "   - 测试支付回调接口"
echo "   - 验证回调数据处理"
echo ""
echo "3. 数据库流程测试（无 API）"
echo "   ${TEST_DIR}/test-payment-flow.sh"
echo "   - 直接数据库操作"
echo "   - 模拟完整支付流程"
echo ""
echo "======================================"
echo "选择测试类型:"
echo "======================================"
echo ""
echo "选项 1: 完整 API 测试（推荐）"
echo "选项 2: 回调测试"
echo "选项 3: 数据库测试"
echo "选项 4: 运行所有测试"
echo "选项 5: 退出"
echo ""

read -p "请选择 (1-5): " choice

case $choice in
  1)
    echo ""
    echo "执行完整 API 测试..."
    bash "${TEST_DIR}/test-payment-api.sh"
    ;;
  2)
    echo ""
    echo "执行回调测试..."
    bash "${TEST_DIR}/test-payment-callback.sh"
    ;;
  3)
    echo ""
    echo "执行数据库测试..."
    bash "${TEST_DIR}/test-payment-flow.sh"
    ;;
  4)
    echo ""
    echo "执行所有测试..."
    echo ""
    echo "===== 测试 1: 数据库流程 ====="
    bash "${TEST_DIR}/test-payment-flow.sh"
    echo ""
    echo "===== 测试 2: 完整 API 流程 ====="
    bash "${TEST_DIR}/test-payment-api.sh"
    echo ""
    echo "===== 测试 3: 回调流程 ====="
    bash "${TEST_DIR}/test-payment-callback.sh"
    ;;
  5)
    echo "退出测试"
    exit 0
    ;;
  *)
    echo "无效选择"
    exit 1
    ;;
esac

echo ""
echo "======================================"
echo "测试执行完成"
echo "======================================"