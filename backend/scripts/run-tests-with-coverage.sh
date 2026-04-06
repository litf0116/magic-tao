#!/bin/bash

# Backend 测试覆盖率脚本
# 运行单元测试并生成覆盖率报告

set -e

echo "🧪 Backend - 运行单元测试并生成覆盖率报告..."
echo ""

cd backend

# 检查 reportgenerator 是否安装
if ! command -v reportgenerator &> /dev/null; then
    echo "⚠️  reportgenerator 未安装，正在安装..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# 清理旧的测试结果
echo "🧹 清理旧的测试结果..."
rm -rf TestResults coverage-report coverage.xml

# 运行测试并收集覆盖率
echo ""
echo "🔬 运行测试..."
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../coverage.xml \
  /p:Exclude="[TtWork.*.Tests]*" \
  /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute" \
  --verbosity normal

# 检查测试是否成功
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ 测试通过！"
else
    echo ""
    echo "❌ 测试失败！"
    exit 1
fi

# 生成 HTML 报告
echo ""
echo "📊 生成 HTML 覆盖率报告..."

# 查找覆盖率文件
COVERAGE_FILE=$(find ./TestResults -name "coverage.opencover.xml" | head -n 1)

if [ -z "$COVERAGE_FILE" ]; then
    echo "❌ 未找到覆盖率文件"
    exit 1
fi

reportgenerator \
  -reports:"$COVERAGE_FILE" \
  -targetdir:./coverage-report \
  -reporttypes:Html \
  -historydir:./coverage-history

# 显示覆盖率摘要
echo ""
echo "📈 覆盖率摘要:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# 从 HTML 报告中提取覆盖率信息
if [ -f "./coverage-report/index.html" ]; then
    echo "✅ HTML 报告已生成: backend/coverage-report/index.html"
    echo ""

    # 尝试打开报告
    if [[ "$OSTYPE" == "darwin"* ]]; then
        open ./coverage-report/index.html
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        xdg-open ./coverage-report/index.html 2>/dev/null || true
    fi

    echo "💡 提示: 浏览器将自动打开覆盖率报告"
else
    echo "❌ HTML 报告生成失败"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✨ 测试覆盖率统计完成！"