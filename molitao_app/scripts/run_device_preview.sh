#!/bin/bash

# 设备预览测试脚本
echo "🚀 启动 Device Preview 测试环境..."

# 检查依赖
echo "📦 检查依赖..."
flutter pub get

# 启动Web版本（最适合Device Preview）
echo "🌐 启动Web版本（推荐用于Device Preview）..."
echo "启动后，在浏览器中打开 http://localhost:8080"
echo "Device Preview 面板将在右侧显示"
flutter run -d chrome --web-port=8080

# 如果需要启动其他平台，可以使用以下命令：
# echo "📱 启动Android设备..."
# flutter run -d android

# echo "🍎 启动iOS设备..."
# flutter run -d ios

echo "✨ Device Preview 已启动！"
echo "📋 使用说明："
echo "   1. 在右侧或底部找到 Device Preview 面板"
echo "   2. 选择不同设备尺寸进行测试"
echo "   3. 实时查看布局适配效果"
echo "   4. 使用截图功能保存测试结果"