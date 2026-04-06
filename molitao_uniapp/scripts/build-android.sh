#!/bin/bash

# 魔力淘 UniApp Android APK 自动打包脚本
# 用法: ./scripts/build-android.sh [debug|release]
# 默认构建 release 版本

set -e

BUILD_TYPE=${1:-release}
PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ANDROID_DIR="$PROJECT_DIR/android"
UNIAPP_OUTPUT_DIR="$PROJECT_DIR/dist/build/app"
ASSETS_DIR="$ANDROID_DIR/app/src/main/assets"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  魔力淘 UniApp Android APK 打包脚本  ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

echo -e "${YELLOW}步骤 1: 检查环境...${NC}"
if ! command -v java &> /dev/null; then
    echo -e "${RED}错误: 未找到 Java${NC}"
    exit 1
fi

if ! command -v gradle &> /dev/null; then
    echo -e "${RED}错误: 未找到 Gradle${NC}"
    exit 1
fi

echo -e "${GREEN}✓ 环境检查通过${NC}"
echo ""

echo -e "${YELLOW}步骤 2: 构建 UniApp 资源...${NC}"
cd "$PROJECT_DIR"

rm -rf dist/build/app
rm -rf "$ASSETS_DIR/apps"

npm run build:app-android

if [ $? -ne 0 ]; then
    echo -e "${RED}错误: UniApp 构建失败${NC}"
    exit 1
fi

echo -e "${GREEN}✓ UniApp 资源构建完成${NC}"
echo ""

echo -e "${YELLOW}步骤 3: 准备资源目录...${NC}"
mkdir -p "$ASSETS_DIR/apps/__UNI__BE7D07D/www"
mkdir -p "$ASSETS_DIR/data"

cp -r "$UNIAPP_OUTPUT_DIR"/* "$ASSETS_DIR/apps/__UNI__BE7D07D/www/"

cat > "$ASSETS_DIR/data/dcloud_control.xml" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<control>
    <version>1.0.0</version>
    <appid>__UNI__BE7D07D</appid>
    <appver>1.0.0</appver>
    <versionCode>100</versionCode>
</control>
EOF

echo -e "${GREEN}✓ 资源准备完成${NC}"
echo ""

echo -e "${YELLOW}步骤 4: 构建 APK (${BUILD_TYPE})...${NC}"
cd "$ANDROID_DIR"

if [ "$BUILD_TYPE" = "debug" ]; then
    ./gradlew assembleDebug
    APK_PATH="$ANDROID_DIR/app/build/outputs/apk/debug/app-debug.apk"
else
    ./gradlew assembleRelease
    APK_PATH="$ANDROID_DIR/app/build/outputs/apk/release/app-release.apk"
fi

if [ $? -ne 0 ]; then
    echo -e "${RED}错误: APK 构建失败${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  构建成功!${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}APK 路径: ${APK_PATH}${NC}"
echo -e "${GREEN}APK 大小: $(du -h "$APK_PATH" | cut -f1)${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  魔力淘 UniApp Android APK 打包脚本  ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# 步骤 1: 检查环境
echo -e "${YELLOW}步骤 1: 检查环境...${NC}"
if ! command -v java &> /dev/null; then
    echo -e "${RED}错误: 未找到 Java${NC}"
    exit 1
fi

if ! command -v gradle &> /dev/null; then
    echo -e "${RED}错误: 未找到 Gradle${NC}"
    exit 1
fi

echo -e "${GREEN}✓ 环境检查通过${NC}"
echo ""

# 步骤 2: 构建 UniApp 资源
echo -e "${YELLOW}步骤 2: 构建 UniApp 资源...${NC}"
cd "$PROJECT_DIR"

# 清理旧的构建输出
rm -rf dist/build/app
rm -rf "$ASSETS_DIR/apps"

# 执行 UniApp 构建
npm run build:app-android

if [ $? -ne 0 ]; then
    echo -e "${RED}错误: UniApp 构建失败${NC}"
    exit 1
fi

echo -e "${GREEN}✓ UniApp 资源构建完成${NC}"
echo ""

# 步骤 3: 创建应用目录结构
echo -e "${YELLOW}步骤 3: 准备资源目录...${NC}"
mkdir -p "$ASSETS_DIR/apps/__UNI__BE7D07D/www"
mkdir -p "$ASSETS_DIR/data"

# 复制编译后的资源
cp -r "$UNIAPP_OUTPUT_DIR"/* "$ASSETS_DIR/apps/__UNI__BE7D07D/www/"

# 创建 dcloud_control.xml
cat > "$ASSETS_DIR/data/dcloud_control.xml" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<control>
    <version>1.0.0</version>
    <appid>__UNI__BE7D07D</appid>
    <appver>1.0.0</appver>
    <versionCode>100</versionCode>
</control>
EOF

echo -e "${GREEN}✓ 资源准备完成${NC}"
echo ""

# 步骤 4: 构建 APK
echo -e "${YELLOW}步骤 4: 构建 APK (${BUILD_TYPE})...${NC}"
cd "$ANDROID_DIR"

if [ "$BUILD_TYPE" = "debug" ]; then
    ./gradlew assembleDebug
    APK_PATH="$ANDROID_DIR/app/build/outputs/apk/debug/app-debug.apk"
else
    ./gradlew assembleRelease
    APK_PATH="$ANDROID_DIR/app/build/outputs/apk/release/app-release.apk"
fi

if [ $? -ne 0 ]; then
    echo -e "${RED}错误: APK 构建失败${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  构建成功!${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}APK 路径: ${APK_PATH}${NC}"
echo -e "${GREEN}APK 大小: $(du -h "$APK_PATH" | cut -f1)${NC}"
echo -e "${GREEN}========================================${NC}