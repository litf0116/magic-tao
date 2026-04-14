#!/bin/bash

# DCloud SDK 自动集成脚本
# 使用说明：
# 1. 下载 DCloud Android SDK (https://pan.baidu.com/s/1AFjLggD7g6ue0iKgZ8yVyA 提取码: jrrb)
# 2. 解压 SDK 到本地目录
# 3. 运行此脚本: ./scripts/integrate-sdk.sh <SDK解压目录>

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

if [ $# -eq 0 ]; then
    echo -e "${RED}错误: 请提供 SDK 解压目录路径${NC}"
    echo ""
    echo "用法: $0 <SDK解压目录>"
    echo ""
    echo "示例:"
    echo "  $0 ~/Downloads/HBuilder-Integrate-AS"
    exit 1
fi

SDK_DIR="$1"
PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ANDROID_DIR="$PROJECT_DIR/android"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  DCloud SDK 自动集成脚本  ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

echo -e "${YELLOW}步骤 1: 检查 SDK 目录...${NC}"
if [ ! -d "$SDK_DIR/HBuilder-Integrate" ]; then
    echo -e "${RED}错误: 未找到 HBuilder-Integrate 目录${NC}"
    echo "请确认 SDK 解压路径正确"
    exit 1
fi
echo -e "${GREEN}✓ SDK 目录检查通过${NC}"
echo ""

echo -e "${YELLOW}步骤 2: 创建 libs 目录...${NC}"
mkdir -p "$ANDROID_DIR/app/libs"
echo -e "${GREEN}✓ libs 目录创建完成${NC}"
echo ""

echo -e "${YELLOW}步骤 3: 复制 DCloud 核心库文件...${NC}"
SDK_LIBS_DIR="$SDK_DIR/HBuilder-Integrate/app/libs"

REQUIRED_FILES=(
    "lib.5plus.base-release.aar"
    "uniapp-release.aar"
)

for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$SDK_LIBS_DIR/$file" ]; then
        cp "$SDK_LIBS_DIR/$file" "$ANDROID_DIR/app/libs/"
        echo -e "${GREEN}✓ 已复制: $file${NC}"
    else
        echo -e "${RED}✗ 未找到: $file${NC}"
    fi
done

# 复制其他 aar 文件
for file in "$SDK_LIBS_DIR"/*.aar; do
    filename=$(basename "$file")
    if [[ ! " ${REQUIRED_FILES[@]} " =~ " ${filename} " ]]; then
        cp "$file" "$ANDROID_DIR/app/libs/"
        echo -e "${GREEN}✓ 已复制: $filename${NC}"
    fi
done

# 复制 jar 文件
for file in "$SDK_LIBS_DIR"/*.jar; do
    if [ -f "$file" ]; then
        cp "$file" "$ANDROID_DIR/app/libs/"
        echo -e "${GREEN}✓ 已复制: $(basename $file)${NC}"
    fi
done

echo ""
echo -e "${YELLOW}步骤 4: 复制 assets/data 目录...${NC}"
SDK_ASSETS_DIR="$SDK_DIR/HBuilder-Integrate/app/src/main/assets/data"

if [ -d "$SDK_ASSETS_DIR" ]; then
    rm -rf "$ANDROID_DIR/app/src/main/assets/data"
    cp -r "$SDK_ASSETS_DIR" "$ANDROID_DIR/app/src/main/assets/"
    echo -e "${GREEN}✓ assets/data 目录复制完成${NC}"
else
    echo -e "${RED}✗ 未找到 assets/data 目录${NC}"
fi

echo ""
echo -e "${YELLOW}步骤 5: 创建必要的资源文件...${NC}"

# 创建 dcloud_file_paths.xml
mkdir -p "$ANDROID_DIR/app/src/main/res/xml"
cat > "$ANDROID_DIR/app/src/main/res/xml/dcloud_file_paths.xml" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <external-path name="external" path="."/>
    <external-files-path name="external_files" path="."/>
    <cache-path name="cache" path="."/>
    <files-path name="files" path="."/>
</paths>
EOF
echo -e "${GREEN}✓ dcloud_file_paths.xml 创建完成${NC}"

echo ""
echo -e "${YELLOW}步骤 6: 更新 build.gradle...${NC}"
if [ -f "$ANDROID_DIR/app/build.gradle" ]; then
    # 备份原文件
    cp "$ANDROID_DIR/app/build.gradle" "$ANDROID_DIR/app/build.gradle.bak"
    echo -e "${GREEN}✓ build.gradle 已备份为 build.gradle.bak${NC}"
else
    echo -e "${RED}✗ 未找到 build.gradle${NC}"
fi

echo ""
echo -e "${YELLOW}步骤 7: 更新 MainActivity.java...${NC}"
if [ -f "$ANDROID_DIR/app/src/main/java/com/molitao/app/MainActivity.java" ]; then
    # 备份原文件
    cp "$ANDROID_DIR/app/src/main/java/com/molitao/app/MainActivity.java" \
       "$ANDROID_DIR/app/src/main/java/com/molitao/app/MainActivity.java.bak"
    echo -e "${GREEN}✓ MainActivity.java 已备份为 MainActivity.java.bak${NC}"
else
    echo -e "${RED}✗ 未找到 MainActivity.java${NC}"
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  SDK 集成完成！${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}接下来需要手动完成：${NC}"
echo ""
echo "1. 编辑 android/app/build.gradle，添加 SDK 依赖："
echo "   dependencies {"
echo "       implementation fileTree(dir: 'libs', include: ['*.jar', '*.aar'])"
echo "       ..."
echo "   }"
echo ""
echo "2. 编辑 android/app/src/main/java/com/molitao/app/MainActivity.java："
echo "   public class MainActivity extends PandoraEntryActivity {"
echo "       @Override"
echo "       protected void onCreate(Bundle savedInstanceState) {"
echo "           super.onCreate(savedInstanceState);"
echo "       }"
echo "   }"
echo ""
echo "3. 更新 AndroidManifest.xml（参考 SDK_INTEGRATION_GUIDE.md）"
echo ""
echo "4. 从 dev.dcloud.net.cn 获取 AppKey 并配置"
echo ""
echo "5. 构建 APK："
echo "   npm run build:app-android"
echo "   ./scripts/quick-build.sh"
echo ""
echo -e "${YELLOW}详细说明请查看: android/SDK_INTEGRATION_GUIDE.md${NC}"
echo ""