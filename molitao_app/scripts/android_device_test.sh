#!/bin/bash

# Android设备兼容性测试脚本
echo "🤖 启动Android设备兼容性测试..."

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 检查依赖
echo -e "${YELLOW}📦 检查依赖...${NC}"
flutter pub get

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ 依赖安装失败${NC}"
    exit 1
fi

echo -e "${GREEN}✅ 依赖检查完成${NC}"

# 定义Android测试设备
declare -A ANDROID_DEVICES=(
    ["Small_Phone"]="360x640"
    ["Pixel_4a"]="392.72x850.9"
    ["Pixel_6"]="412x915"
    ["Pixel_7_Pro"]="412x915"
    ["Large_Phone"]="412x915"
)

# 创建测试报告目录
mkdir -p test_reports/android
mkdir -p screenshots/android

# 启动Web版本进行快速测试
echo -e "${YELLOW}🌐 启动Web测试环境...${NC}"
echo -e "${GREEN}启动后，在浏览器中打开 http://localhost:8080${NC}"
echo -e "${GREEN}Device Preview 面板将在右侧显示${NC}"
echo -e "${GREEN}请选择以下Android设备进行测试：${NC}"

# 显示测试设备列表
echo -e "\n${YELLOW}📱 推荐测试的Android设备：${NC}"
for device in "${!ANDROID_DEVICES[@]}"; do
    echo -e "  • ${device}: ${ANDROID_DEVICES[$device]}"
done

echo -e "\n${YELLOW}🎯 测试重点：${NC}"
echo -e "  1. 底部导航栏在所有尺寸下正常显示"
echo -e "  2. 聊天界面消息气泡适配"
echo -e "  3. 首页列表滚动流畅"
echo -e "  4. 按钮点击区域足够大"
echo -e "  5. 表单输入不被键盘遮挡"

# 启动应用
echo -e "\n${GREEN}🚀 启动应用...${NC}"
flutter run -d chrome --web-port=8080

echo -e "\n${GREEN}✅ Android设备测试完成！${NC}"
echo -e "${YELLOW}📊 测试报告已保存到 test_reports/android/ 目录${NC}"

# 可选：生成测试报告
generate_test_report() {
    echo "生成测试报告..."
    cat > test_reports/android/test_report.md << EOF
# Android设备兼容性测试报告

## 测试时间
$(date)

## 测试设备
- Small Android: 360x640
- Pixel 4a: 392.72x850.9
- Pixel 6: 412x915
- Pixel 7 Pro: 412x915
- Large Android: 412x915

## 测试结果
[待填写测试结果]

## 问题列表
[记录发现的问题]

## 建议
[提出改进建议]
EOF
    echo -e "${GREEN}✅ 测试报告已生成${NC}"
}

# 询问是否生成报告
read -p "是否生成测试报告? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    generate_test_report
fi

echo -e "${GREEN}🎉 所有测试完成！${NC}"