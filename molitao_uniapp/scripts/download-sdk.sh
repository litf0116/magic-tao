#!/bin/bash

# DCloud SDK 自动下载脚本

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  DCloud Android SDK 自动下载  ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
SDK_DIR="$PROJECT_DIR/.temp_sdk"
DOWNLOAD_URL="https://pan.baidu.com/s/1AFjLggD7g6ue0iKgZ8yVyA"

echo -e "${YELLOW}方式 1: 从百度云下载（推荐）${NC}"
echo "下载链接：$DOWNLOAD_URL"
echo "提取码：jrrb"
echo ""
echo -e "${YELLOW}手动步骤：${NC}"
echo "1. 在浏览器中打开上述链接"
echo "2. 输入提取码：jrrb"
echo "3. 下载压缩包到当前目录"
echo "4. 解压压缩包"
echo "5. 运行集成脚本：./scripts/integrate-sdk.sh <解压目录>"
echo ""

echo -e "${YELLOW}方式 2: 尝试自动下载${NC}"
echo ""

TEMP_DIR="/tmp/dcloud_sdk_download"
mkdir -p "$TEMP_DIR"

cd "$TEMP_DIR"

echo -e "${GREEN}正在尝试从备用源下载...${NC}"

# 尝试从可能的备用链接下载
ALTERNATIVE_URLS=(
    "https://nativesupport.dcloud.net.cn/downloads/Android-SDK-4.87.2025121004.zip"
    "https://vkceyugu.cdn.bspapp.com/VKCEYUGU-f184e7c3-1912-41b2-b81f-435d1b37c7b4/android_sdk.zip"
)

DOWNLOAD_SUCCESS=false
SDK_FILE=""

for url in "${ALTERNATIVE_URLS[@]}"; do
    echo -e "${YELLOW}尝试下载: $url${NC}"
    
    if curl -L -o "dcloud_sdk.zip" "$url" --max-time 300 --retry 3; then
        if [ -f "dcloud_sdk.zip" ] && [ -s "dcloud_sdk.zip" ]; then
            echo -e "${GREEN}✓ 下载成功${NC}"
            DOWNLOAD_SUCCESS=true
            SDK_FILE="dcloud_sdk.zip"
            break
        fi
    fi
    
    rm -f "dcloud_sdk.zip"
    echo -e "${RED}✗ 下载失败${NC}"
    echo ""
done

if [ "$DOWNLOAD_SUCCESS" = true ]; then
    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}  下载成功！${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    echo -e "${YELLOW}正在解压...${NC}"
    
    EXTRACT_DIR="$TEMP_DIR/dcloud_sdk_extracted"
    mkdir -p "$EXTRACT_DIR"
    
    if unzip -q "$SDK_FILE" -d "$EXTRACT_DIR"; then
        echo -e "${GREEN}✓ 解压完成${NC}"
        echo ""
        
        echo -e "${YELLOW}解压目录：$EXTRACT_DIR${NC}"
        echo ""
        
        echo -e "${YELLOW}现在运行集成脚本：${NC}"
        echo "cd $PROJECT_DIR"
        echo "./scripts/integrate-sdk.sh $EXTRACT_DIR"
        echo ""
        
        read -p "是否现在运行集成脚本？(y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            cd "$PROJECT_DIR"
            ./scripts/integrate-sdk.sh "$EXTRACT_DIR"
        fi
    else
        echo -e "${RED}✗ 解压失败${NC}"
        echo ""
        echo -e "${YELLOW}请尝试手动解压：${NC}"
        echo "unzip $SDK_FILE"
        echo ""
        echo "然后运行集成脚本："
        echo "./scripts/integrate-sdk.sh <解压目录>"
    fi
else
    echo ""
    echo -e "${RED}========================================${NC}"
    echo -e "${RED}  自动下载失败${NC}"
    echo -e "${RED}========================================${NC}"
    echo ""
    echo -e "${YELLOW}请使用手动方式下载：${NC}"
    echo ""
    echo "1. 访问百度云链接："
    echo "   https://pan.baidu.com/s/1AFjLggD7g6ue0iKgZ8yVyA"
    echo ""
    echo "2. 输入提取码：jrrb"
    echo ""
    echo "3. 下载压缩包"
    echo ""
    echo "4. 解压到本地目录"
    echo ""
    echo "5. 运行集成脚本："
    echo "   cd molitao_uniapp"
    echo "   ./scripts/integrate-sdk.sh <解压目录>"
    echo ""
fi

cd "$PROJECT_DIR"