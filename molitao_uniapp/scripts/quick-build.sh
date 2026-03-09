#!/bin/bash

echo "开始准备资源..."

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ASSETS_DIR="$PROJECT_DIR/android/app/src/main/assets"
UNIAPP_OUTPUT_DIR="$PROJECT_DIR/dist/build/app"

echo "创建资源目录..."
mkdir -p "$ASSETS_DIR/apps/__UNI__BE7D07D/www"
mkdir -p "$ASSETS_DIR/data"

echo "复制 UniApp 资源..."
cp -r "$UNIAPP_OUTPUT_DIR"/* "$ASSETS_DIR/apps/__UNI__BE7D07D/www/"

echo "创建 dcloud_control.xml..."
cat > "$ASSETS_DIR/data/dcloud_control.xml" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<control>
    <version>1.0.0</version>
    <appid>__UNI__BE7D07D</appid>
    <appver>1.0.0</appver>
    <versionCode>100</versionCode>
</control>
EOF

echo "资源准备完成"
echo ""

echo "开始 Gradle 构建..."
cd "$PROJECT_DIR/android"

gradle assembleDebug --warning-mode all

if [ $? -eq 0 ]; then
    APK_PATH="$PROJECT_DIR/android/app/build/outputs/apk/debug/app-debug.apk"
    echo ""
    echo "构建成功！"
    echo "APK 路径: $APK_PATH"
    if [ -f "$APK_PATH" ]; then
        echo "APK 大小: $(du -h "$APK_PATH" | cut -f1)"
    fi
else
    echo "构建失败"
    exit 1
fi