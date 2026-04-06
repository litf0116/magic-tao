#!/bin/bash

# Flutter 测试覆盖率脚本
# 运行单元测试并生成覆盖率报告

set -e

echo "🧪 Flutter App - 运行测试并生成覆盖率报告..."
echo ""

cd molitao_app

# 清理旧的覆盖率数据
echo "🧹 清理旧的覆盖率数据..."
rm -rf coverage

# 运行测试并生成覆盖率
echo ""
echo "🔬 运行 Flutter 测试..."
flutter test --coverage --reporter=compact

# 检查是否成功
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ 测试失败！"
    exit 1
fi

echo ""
echo "✅ 测试通过！"

# 检查覆盖率文件
if [ ! -f "coverage/lcov.info" ]; then
    echo "❌ 覆盖率数据生成失败"
    exit 1
fi

echo ""
echo "📊 生成 HTML 覆盖率报告..."

# 检查 genhtml 是否安装
if ! command -v genhtml &> /dev/null; then
    echo "⚠️  genhtml 未安装"
    echo ""
    echo "安装方法:"
    echo "  macOS: brew install lcov"
    echo "  Ubuntu/Debian: sudo apt-get install lcov"
    echo ""
    echo "📈 覆盖率数据已生成: coverage/lcov.info"
    echo "💡 安装 genhtml 后可生成 HTML 报告"
    exit 0
fi

# 移除生成的代码文件
echo "🔍 过滤生成的代码文件..."
lcov --remove coverage/lcov.info \
  '**/*.g.dart' \
  '**/*.freezed.dart' \
  '**/test/**' \
  -o coverage/lcov_cleaned.info \
  2>/dev/null || true

# 生成 HTML 报告
genhtml coverage/lcov_cleaned.info \
  -o coverage/html \
  --title "Flutter App Test Coverage" \
  --legend \
  --show-details

# 显示摘要
echo ""
echo "📈 覆盖率摘要:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
lcov --summary coverage/lcov_cleaned.info 2>&1 | grep -E "(lines|functions)" || true
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# 打开报告
if [ -f "./coverage/html/index.html" ]; then
    echo ""
    echo "✅ HTML 报告已生成: molitao_app/coverage/html/index.html"

    if [[ "$OSTYPE" == "darwin"* ]]; then
        open ./coverage/html/index.html
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        xdg-open ./coverage/html/index.html 2>/dev/null || true
    fi

    echo "💡 提示: 浏览器将自动打开覆盖率报告"
fi

echo ""
echo "✨ 测试覆盖率统计完成！"