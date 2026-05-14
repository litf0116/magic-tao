#!/bin/bash
# Fastlane Match 预检查脚本
# 检查配置是否完整

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
IOS_DIR="$(dirname "$SCRIPT_DIR")"

echo "=========================================="
echo "  Fastlane Match 配置检查"
echo "=========================================="
echo ""

ERRORS=0
WARNINGS=0

# 1. 检查 fastlane 安装
echo "1️⃣  检查 fastlane 安装..."
if command -v fastlane &> /dev/null; then
    VERSION=$(fastlane --version)
    echo "   ✅ Fastlane 已安装: $VERSION"
else
    echo "   ❌ Fastlane 未安装"
    echo "      解决: brew install fastlane"
    ((ERRORS++))
fi

# 2. 检查配置文件
echo ""
echo "2️⃣  检查配置文件..."

if [ -f "$SCRIPT_DIR/Appfile" ]; then
    echo "   ✅ Appfile 存在"
    
    # 检查是否已配置
    if grep -q "YOUR_APPLE_ID" "$SCRIPT_DIR/Appfile"; then
        echo "   ⚠️  Appfile 未配置 (仍包含占位符)"
        ((WARNINGS++))
    else
        echo "   ✅ Appfile 已配置"
    fi
else
    echo "   ❌ Appfile 不存在"
    ((ERRORS++))
fi

if [ -f "$SCRIPT_DIR/Fastfile" ]; then
    echo "   ✅ Fastfile 存在"
else
    echo "   ❌ Fastfile 不存在"
    ((ERRORS++))
fi

if [ -f "$SCRIPT_DIR/Matchfile" ]; then
    echo "   ✅ Matchfile 存在"
    
    # 检查是否已配置
    if grep -q "YOUR_USERNAME" "$SCRIPT_DIR/Matchfile" || grep -q "YOUR_TEAM_ID" "$SCRIPT_DIR/Matchfile"; then
        echo "   ⚠️  Matchfile 未配置 (仍包含占位符)"
        ((WARNINGS++))
    else
        echo "   ✅ Matchfile 已配置"
    fi
else
    echo "   ❌ Matchfile 不存在"
    ((ERRORS++))
fi

# 3. 检查 MATCH_PASSWORD
echo ""
echo "3️⃣  检查 MATCH_PASSWORD 环境变量..."
if [ -n "$MATCH_PASSWORD" ]; then
    echo "   ✅ MATCH_PASSWORD 已设置 (${#MATCH_PASSWORD} 字符)"
else
    echo "   ⚠️  MATCH_PASSWORD 未设置"
    echo "      解决: export MATCH_PASSWORD=\"your-password\""
    echo "      或添加到 ~/.zshrc: echo 'export MATCH_PASSWORD=\"your-password\"' >> ~/.zshrc"
    ((WARNINGS++))
fi

# 4. 检查 Git 配置
echo ""
echo "4️⃣  检查 Git 配置..."
if [ -f "$SCRIPT_DIR/Matchfile" ]; then
    GIT_URL=$(grep "git_url" "$SCRIPT_DIR/Matchfile" | sed 's/git_url("//' | sed 's/")//')
    if [ -n "$GIT_URL" ] && [[ ! "$GIT_URL" =~ "YOUR_USERNAME" ]]; then
        echo "   ✅ Git 仓库: $GIT_URL"
        
        # 检查是否可以访问
        if git ls-remote "$GIT_URL" &> /dev/null; then
            echo "   ✅ Git 仓库可访问"
        else
            echo "   ⚠️  Git 仓库无法访问 (可能需要 SSH 密钥或仓库未创建)"
            ((WARNINGS++))
        fi
    else
        echo "   ⚠️  Git 仓库未配置"
        ((WARNINGS++))
    fi
else
    echo "   ❌ 无法检查 Git 配置 (Matchfile 不存在)"
    ((ERRORS++))
fi

# 5. 检查 Xcode 项目
echo ""
echo "5️⃣  检查 Xcode 项目..."
if [ -d "$IOS_DIR/Runner.xcodeproj" ]; then
    echo "   ✅ Xcode 项目存在"
    
    # 检查 Bundle ID
    BUNDLE_ID=$(grep "PRODUCT_BUNDLE_IDENTIFIER" "$IOS_DIR/Runner.xcodeproj/project.pbxproj" | head -1 | sed 's/.*= //' | sed 's/;//')
    echo "   ✅ Bundle ID: $BUNDLE_ID"
else
    echo "   ❌ Xcode 项目不存在"
    ((ERRORS++))
fi

# 6. 检查 .gitignore
echo ""
echo "6️⃣  检查 .gitignore..."
if [ -f "$IOS_DIR/.gitignore" ]; then
    echo "   ✅ .gitignore 存在"
    
    if grep -q "AuthKey_.*\.p8" "$IOS_DIR/.gitignore"; then
        echo "   ✅ 已忽略 API Key 文件"
    else
        echo "   ⚠️  未忽略 API Key 文件 (.p8)"
        ((WARNINGS++))
    fi
else
    echo "   ⚠️  .gitignore 不存在"
    ((WARNINGS++))
fi

# 总结
echo ""
echo "=========================================="
echo "  检查结果"
echo "=========================================="
echo ""
echo "❌ 错误: $ERRORS"
echo "⚠️  警告: $WARNINGS"
echo ""

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo "✅ 所有检查通过！可以运行:"
    echo "   cd $IOS_DIR"
    echo "   fastlane create_appstore_cert"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo "⚠️  存在警告，建议修复后再继续"
    exit 0
else
    echo "❌ 存在错误，请先修复"
    exit 1
fi
