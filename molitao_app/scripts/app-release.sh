#!/bin/bash
set -e

APP_NAME="molitao-app"
PROJECT_DIR="/Users/mac/workspace/magic-tao/molitao_app"
RELEASE_DIR="$PROJECT_DIR/release"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/release-config.sh"

if [[ -f "$CONFIG_FILE" ]]; then
    source "$CONFIG_FILE"
fi

UPYUN_BUCKET="${UPYUN_BUCKET:-molitao}"
UPYUN_USER="${UPYUN_USER:-molitao}"
UPYUN_PASSWORD="${UPYUN_PASSWORD:-}"
UPYUN_DOMAIN="${UPYUN_DOMAIN:-http://image.molitao.top}"
UPYUN_API="https://v0.api.upyun.com"
API_BASE="${API_BASE:-https://www.molitao.top/api/services/app}"

# ============================================
# 颜色输出
# ============================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

VERSION_NAME=""
VERSION_CODE=""
DESCRIPTION=""
IS_FORCE_UPDATE="false"
PLATFORM="android"
SKIP_BUILD="false"
AUTH_TOKEN=""

usage() {
    cat << EOF
用法: $0 -v <version> -c <code> [options]

必需参数:
  -v, --version     版本名称 (如: 1.3.0)
  -c, --code        版本号 (如: 130)

可选参数:
  -d, --desc        更新说明
  -f, --force       强制更新标记 (默认: false)
  -p, --platform    平台 (android/ios, 默认: android)
  -s, --skip-build  跳过构建，使用现有 APK
  -t, --token       后端 API 认证 Token
  -h, --help        显示帮助

示例:
  $0 -v 1.3.0 -c 130 -d "修复已知问题" -t "Bearer xxx"
  $0 -v 1.3.0 -c 130 -s -t "Bearer xxx"
EOF
    exit 1
}

while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--version) VERSION_NAME="$2"; shift 2 ;;
        -c|--code) VERSION_CODE="$2"; shift 2 ;;
        -d|--desc) DESCRIPTION="$2"; shift 2 ;;
        -f|--force) IS_FORCE_UPDATE="true"; shift ;;
        -p|--platform) PLATFORM="$2"; shift 2 ;;
        -s|--skip-build) SKIP_BUILD="true"; shift ;;
        -t|--token) AUTH_TOKEN="$2"; shift 2 ;;
        -h|--help) usage ;;
        *) log_error "未知参数: $1"; usage ;;
    esac
done

# 验证必需参数
if [[ -z "$VERSION_NAME" || -z "$VERSION_CODE" ]]; then
    log_error "缺少必需参数"
    usage
fi

# ============================================
# 步骤 1: 构建 APK
# ============================================
build_apk() {
    if [[ "$SKIP_BUILD" == "true" ]]; then
        log_info "跳过构建，使用现有 APK..."
        APK_FILE=$(find "$PROJECT_DIR/build/app/outputs/apk/release" -name "*.apk" 2>/dev/null | head -1)
        if [[ -z "$APK_FILE" ]]; then
            log_error "未找到现有 APK 文件"
            exit 1
        fi
        log_success "使用 APK: $APK_FILE"
        return
    fi

    log_info "开始构建 APK..."
    cd "$PROJECT_DIR"
    
    # 清理并构建
    flutter clean
    flutter pub get
    flutter build apk --release
    
    APK_FILE="$PROJECT_DIR/build/app/outputs/apk/release/app-release.apk"
    
    if [[ ! -f "$APK_FILE" ]]; then
        log_error "APK 构建失败"
        exit 1
    fi
    
    log_success "APK 构建完成: $APK_FILE"
}

# ============================================
# 步骤 2: 上传到又拍云 CDN
# ============================================
upload_to_cdn() {
    log_info "上传 APK 到又拍云 CDN..."
    
    # 生成 CDN 路径
    TIMESTAMP=$(date +%Y%m%d%H%M%S)
    FILE_NAME="${APP_NAME}-v${VERSION_NAME}-${TIMESTAMP}.apk"
    CDN_PATH="/apps/releases/${PLATFORM}/v${VERSION_NAME}/${FILE_NAME}"
    
    # 计算文件 MD5
    FILE_MD5=$(md5 -q "$APK_FILE" 2>/dev/null || md5sum "$APK_FILE" | awk '{print $1}')
    FILE_SIZE=$(stat -f%z "$APK_FILE" 2>/dev/null || stat -c%s "$APK_FILE")
    
    log_info "文件信息: MD5=$FILE_MD5, Size=$FILE_SIZE bytes"
    
    DATE=$(LC_TIME=en_US.UTF-8 date -u +"%a, %d %b %Y %H:%M:%S GMT")
    PASSWORD_MD5=$(echo -n "$UPYUN_PASSWORD" | md5 -q 2>/dev/null || echo -n "$UPYUN_PASSWORD" | md5sum | awk '{print $1}')
    STRING_TO_SIGN="POST&/$UPYUN_BUCKET$CDN_PATH&$DATE&$FILE_SIZE&$PASSWORD_MD5"
    SIGNATURE=$(echo -n "$STRING_TO_SIGN" | md5 -q 2>/dev/null || echo -n "$STRING_TO_SIGN" | md5sum | awk '{print $1}')
    AUTH_HEADER="UpYun $UPYUN_USER:$SIGNATURE"
    
    # 上传文件
    HTTP_CODE=$(curl -s -w "%{http_code}" -o /tmp/upyun_response.txt \
        -X POST \
        -H "Authorization: $AUTH_HEADER" \
        -H "Date: $DATE" \
        -H "mkdir: true" \
        -H "Content-MD5: $FILE_MD5" \
        --data-binary @"$APK_FILE" \
        "${UPYUN_API}/$UPYUN_BUCKET$CDN_PATH")
    
    if [[ "$HTTP_CODE" == "200" ]]; then
        DOWNLOAD_URL="${UPYUN_DOMAIN}${CDN_PATH}"
        log_success "上传成功!"
        log_info "CDN URL: $DOWNLOAD_URL"
    else
        log_error "上传失败 (HTTP $HTTP_CODE)"
        cat /tmp/upyun_response.txt
        exit 1
    fi
}

# ============================================
# 步骤 3: 调用后端 API 创建发布记录
# ============================================
create_release_record() {
    log_info "创建发布记录..."
    
    if [[ -z "$AUTH_TOKEN" ]]; then
        log_warn "未提供认证 Token，跳过创建发布记录"
        log_info "请手动调用 API:"
        echo ""
        echo "POST $API_BASE/AppRelease/PublishAppReleaseByUrl"
        echo "Authorization: Bearer <token>"
        echo "Content-Type: application/json"
        echo ""
        echo "{"
        echo "  \"versionName\": \"$VERSION_NAME\","
        echo "  \"versionCode\": $VERSION_CODE,"
        echo "  \"description\": \"$DESCRIPTION\","
        echo "  \"downloadUrl\": \"$DOWNLOAD_URL\","
        echo "  \"fileName\": \"$FILE_NAME\","
        echo "  \"fileSize\": $FILE_SIZE,"
        echo "  \"isForceUpdate\": $IS_FORCE_UPDATE,"
        echo "  \"platform\": \"$PLATFORM\""
        echo "}"
        return
    fi
    
    # 调用 API
    RESPONSE=$(curl -s -w "\n%{http_code}" \
        -X POST \
        -H "Authorization: $AUTH_TOKEN" \
        -H "Content-Type: application/json" \
        -d "{
            \"versionName\": \"$VERSION_NAME\",
            \"versionCode\": $VERSION_CODE,
            \"description\": \"$DESCRIPTION\",
            \"downloadUrl\": \"$DOWNLOAD_URL\",
            \"fileName\": \"$FILE_NAME\",
            \"fileSize\": $FILE_SIZE,
            \"isForceUpdate\": $IS_FORCE_UPDATE,
            \"platform\": \"$PLATFORM\"
        }" \
        "$API_BASE/AppRelease/PublishAppReleaseByUrl")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -1)
    BODY=$(echo "$RESPONSE" | sed '$d')
    
    if [[ "$HTTP_CODE" == "200" ]]; then
        log_success "发布记录创建成功! ID: $BODY"
    else
        log_error "创建发布记录失败 (HTTP $HTTP_CODE)"
        echo "$BODY"
        exit 1
    fi
}

# ============================================
# 步骤 4: 生成发布报告
# ============================================
generate_report() {
    REPORT_FILE="$RELEASE_DIR/v${VERSION_NAME}/release-report.txt"
    mkdir -p "$(dirname "$REPORT_FILE")"
    
    cat > "$REPORT_FILE" << EOF
========================================
魔力淘 App 发布报告
========================================
发布时间: $(date '+%Y-%m-%d %H:%M:%S')
版本名称: $VERSION_NAME
版本号: $VERSION_CODE
平台: $PLATFORM
强制更新: $IS_FORCE_UPDATE

文件信息:
  文件名: $FILE_NAME
  文件大小: $FILE_SIZE bytes ($(( FILE_SIZE / 1024 / 1024 )) MB)
  MD5: $FILE_MD5

下载地址:
  CDN URL: $DOWNLOAD_URL

更新说明:
$DESCRIPTION
========================================
EOF
    
    log_success "发布报告已生成: $REPORT_FILE"
}

# ============================================
# 主流程
# ============================================
echo ""
echo "========================================"
echo "  魔力淘 App Release 发版工具"
echo "========================================"
echo ""
log_info "版本: $VERSION_NAME ($VERSION_CODE)"
log_info "平台: $PLATFORM"
log_info "强制更新: $IS_FORCE_UPDATE"
echo ""

build_apk
upload_to_cdn
create_release_record
generate_report

echo ""
log_success "========================================"
log_success "  发版完成!"
log_success "========================================"
log_info "下载地址: $DOWNLOAD_URL"
