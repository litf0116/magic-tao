#!/bin/bash

# Android设备快速测试脚本 - 简化版
# 一键启动Flutter应用进行Android兼容性测试

echo "🚀 启动Android设备兼容性测试环境..."

# 检查Flutter环境
if ! command -v flutter &> /dev/null; then
    echo "❌ Flutter未安装，请先安装Flutter"
    exit 1
fi

# 检查项目目录
if [ ! -f "pubspec.yaml" ]; then
    echo "❌ 请在Flutter项目根目录运行此脚本"
    exit 1
fi

echo "📱 准备启动应用..."
echo ""

# 清理并重建
echo "🧹 清理构建缓存..."
flutter clean

echo "📦 获取依赖..."
flutter pub get

# 启动应用
echo "🚀 启动Flutter Web应用..."
echo ""
echo "📋 测试步骤："
echo "  1. 应用启动后，点击右下角Device Preview按钮"
echo "  2. 选择以下Android设备进行测试："
echo "     - 📱 Small Phone (360x640) - 小屏边缘情况"
echo "     - 📱 Medium Phone (412x915) - 主力测试设备"
echo "     - 📱 Large Phone (412x915) - 大屏体验"
echo ""
echo "🎯 测试重点："
echo "  - 底部导航栏适配"
echo "  - 聊天界面布局"
echo "  - 表单输入体验"
echo "  - 图片加载性能"
echo ""
echo "📝 参考文档："
echo "  - 操作指南: ANDROID_TEST_GUIDE.md"
echo "  - 测试报告: ANDROID_TEST_REPORT.md"

# 启动应用
flutter run -d chrome

echo "✅ 测试环境启动完成！"