#!/bin/bash

set +e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ANDROID_DIR="$PROJECT_DIR/android"
LIBS_DIR="$ANDROID_DIR/app/libs"
ASSETS_DIR="$ANDROID_DIR/app/src/main/assets"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  UniApp Android 打包前检查  ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

CHECKS_PASSED=0
CHECKS_FAILED=0

check_pass() {
    echo -e "${GREEN}✓ $1${NC}"
    ((CHECKS_PASSED++))
}

check_fail() {
    echo -e "${RED}✗ $1${NC}"
    ((CHECKS_FAILED++))
}

check_warn() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

echo -e "${YELLOW}检查 1: 基础配置文件...${NC}"

[ -f "$ANDROID_DIR/app/build.gradle" ] && check_pass "build.gradle 存在" || check_fail "build.gradle 缺失"
[ -f "$ANDROID_DIR/app/src/main/AndroidManifest.xml" ] && check_pass "AndroidManifest.xml 存在" || check_fail "AndroidManifest.xml 缺失"
[ -f "$ANDROID_DIR/app/src/main/java/com/molitao/app/MainActivity.java" ] && check_pass "MainActivity.java 存在" || check_fail "MainActivity.java 缺失"
[ -f "$ANDROID_DIR/gradle.properties" ] && check_pass "gradle.properties 存在" || check_fail "gradle.properties 缺失"

echo ""
echo -e "${YELLOW}检查 2: DCloud SDK 库文件...${NC}"

if [ -d "$LIBS_DIR" ]; then
    AAR_COUNT=$(find "$LIBS_DIR" -name "*.aar" 2>/dev/null | wc -l)
    JAR_COUNT=$(find "$LIBS_DIR" -name "*.jar" 2>/dev/null | wc -l)

    if [ "$AAR_COUNT" -gt 0 ]; then
        check_pass "找到 $AAR_COUNT 个 .aar 文件"
    else
        check_fail "未找到 .aar 文件"
    fi

    if [ "$JAR_COUNT" -gt 0 ]; then
        check_pass "找到 $JAR_COUNT 个 .jar 文件"
    else
        check_warn "未找到 .jar 文件"
    fi

    [ -f "$LIBS_DIR/lib.5plus.base-release.aar" ] && check_pass "lib.5plus.base-release.aar 存在" || check_fail "lib.5plus.base-release.aar 缺失"
    [ -f "$LIBS_DIR/uniapp-release.aar" ] && check_pass "uniapp-release.aar 存在" || check_fail "uniapp-release.aar 缺失"
else
    check_fail "libs 目录不存在"
    check_warn "请运行: ./scripts/integrate-sdk.sh <SDK路径>"
fi

echo ""
echo -e "${YELLOW}检查 3: 资源文件...${NC}"

[ -d "$ASSETS_DIR/data" ] && check_pass "assets/data 目录存在" || check_fail "assets/data 目录缺失"
[ -d "$ASSETS_DIR/apps" ] && check_pass "assets/apps 目录存在" || check_fail "assets/apps 目录缺失"
[ -f "$ANDROID_DIR/app/src/main/res/xml/dcloud_file_paths.xml" ] && check_pass "dcloud_file_paths.xml 存在" || check_fail "dcloud_file_paths.xml 缺失"
[ -f "$ANDROID_DIR/app/src/main/res/values/themes.xml" ] && check_pass "themes.xml 存在" || check_fail "themes.xml 缺失"

echo ""
echo -e "${YELLOW}检查 4: UniApp 资源...${NC}"

UNIAPP_APP_DIR="$ASSETS_DIR/apps/__UNI__BE7D07D"
UNIAPP_WWW_DIR="$UNIAPP_APP_DIR/www"

if [ -d "$UNIAPP_WWW_DIR" ]; then
    FILE_COUNT=$(find "$UNIAPP_WWW_DIR" -type f | wc -l)
    if [ "$FILE_COUNT" -gt 0 ]; then
        check_pass "UniApp 资源已编译 ($FILE_COUNT 个文件)"
    else
        check_warn "UniApp 资源目录为空"
        check_warn "请运行: npm run build:app-android"
    fi
else
    check_fail "UniApp 资源不存在"
    check_warn "请运行: npm run build:app-android"
fi

echo ""
echo -e "${YELLOW}检查 5: 签名配置...${NC}"

[ -f "$ANDROID_DIR/app/my-release-key.jks" ] && check_pass "签名密钥存在" || check_fail "签名密钥缺失"

echo ""
echo -e "${YELLOW}检查 6: AppKey 配置...${NC}"

APPKEY=$(grep -oP 'dcloud_appkey.*?value="[^"]*"' "$ANDROID_DIR/app/src/main/AndroidManifest.xml" 2>/dev/null | grep -oP 'value="\K[^"]*' || echo "")

if [ -n "$APPKEY" ]; then
    check_pass "AppKey 已配置"
else
    check_fail "AppKey 未配置"
    check_warn "请访问 dev.dcloud.net.cn 获取 AppKey 并配置"
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  检查结果${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "通过: ${GREEN}$CHECKS_PASSED${NC}  失败: ${RED}$CHECKS_FAILED${NC}"
echo ""

if [ $CHECKS_FAILED -eq 0 ]; then
    echo -e "${GREEN}所有检查通过！可以开始构建 APK。${NC}"
    echo ""
    echo "执行构建："
    echo "  cd android && ./gradlew assembleDebug"
    echo ""
    echo "或使用快速构建脚本："
    echo "  ./scripts/quick-build.sh"
    exit 0
else
    echo -e "${RED}发现 $CHECKS_FAILED 个问题，请解决后再构建。${NC}"
    echo ""
    echo "常见解决方案："
    echo ""
    echo "1. 下载并集成 SDK："
    echo "   ./scripts/integrate-sdk.sh <SDK路径>"
    echo ""
    echo "2. 构建 UniApp 资源："
    echo "   npm run build:app-android"
    echo ""
    echo "3. 配置 AppKey："
    echo "   访问 dev.dcloud.net.cn 获取 AppKey"
    echo "   编辑 AndroidManifest.xml 添加 dcloud_appkey"
    echo ""
    exit 1
fi