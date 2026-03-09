#!/bin/bash

# 创建最小化模拟 SDK 用于演示
# 注意：这只是用于演示和测试的模拟 SDK
# 实际使用时必须下载官方 DCloud SDK

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
MOCK_SDK_DIR="$PROJECT_DIR/.mock_sdk"
SDK_INTEGRATE_DIR="$MOCK_SDK_DIR/HBuilder-Integrate/app"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  创建模拟 SDK（仅用于演示）${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}注意：这只是模拟 SDK，用于演示集成流程${NC}"
echo -e "${YELLOW}实际使用时必须下载官方 DCloud SDK${NC}"
echo ""

mkdir -p "$SDK_INTEGRATE_DIR/libs"
mkdir -p "$SDK_INTEGRATE_DIR/src/main/assets/data"
mkdir -p "$SDK_INTEGRATE_DIR/src/main/res/xml"

echo -e "${YELLOW}创建模拟库文件...${NC}"

# 创建空的 aar 文件作为占位符
cat > "$SDK_INTEGRATE_DIR/libs/lib.5plus.base-release.aar" <<'EOF'
PK
EOF

cat > "$SDK_INTEGRATE_DIR/libs/uniapp-release.aar" <<'EOF'
PK
EOF

echo -e "${GREEN}✓ 模拟库文件创建完成${NC}"

echo -e "${YELLOW}创建模拟 data 目录...${NC}"

# 创建基本的 data 文件
cat > "$SDK_INTEGRATE_DIR/src/main/assets/data/dcloud_control.xml" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<hbuilder>
    <apps>
        <app appid="__UNI__BE7D07D" appver="1.0.0"/>
    </apps>
</hbuilder>
EOF

echo -e "${GREEN}✓ 模拟 data 目录创建完成${NC}"

echo -e "${YELLOW}创建必要的资源文件...${NC}"

cat > "$SDK_INTEGRATE_DIR/src/main/res/xml/dcloud_file_paths.xml" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <external-path name="external" path="."/>
    <external-files-path name="external_files" path="."/>
    <cache-path name="cache" path="."/>
    <files-path name="files" path="."/>
</paths>
EOF

echo -e "${GREEN}✓ 资源文件创建完成${NC}"

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  模拟 SDK 创建完成！${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}SDK 路径：$MOCK_SDK_DIR${NC}"
echo ""
echo -e "${YELLOW}现在运行集成脚本：${NC}"
echo "cd $PROJECT_DIR"
echo "./scripts/integrate-sdk.sh $MOCK_SDK_DIR"
echo ""
echo -e "${YELLOW}⚠️  重要提示：${NC}"
echo "这只是一个模拟 SDK，用于演示集成流程"
echo "实际运行 UniApp 应用时，必须下载官方 DCloud SDK"
echo ""
echo "官方 SDK 下载地址："
echo "  百度云：https://pan.baidu.com/s/1AFjLggD7g6ue0iKgZ8yVyA"
echo "  提取码：jrrb"
echo ""

read -p "是否继续使用模拟 SDK 进行集成演示？(y/n) " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    cd "$PROJECT_DIR"
    ./scripts/integrate-sdk.sh "$MOCK_SDK_DIR"
else
    echo ""
    echo -e "${YELLOW}请下载官方 SDK 后重新运行集成脚本${NC}"
    echo ""
    echo "1. 下载官方 SDK："
    echo "   https://pan.baidu.com/s/1AFjLggD7g6ue0iKgZ8yVyA"
    echo "   提取码：jrrb"
    echo ""
    echo "2. 解压后运行："
    echo "   ./scripts/integrate-sdk.sh <解压目录>"
    echo ""
fi