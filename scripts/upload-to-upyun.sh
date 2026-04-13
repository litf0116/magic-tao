#!/bin/bash
set -e

API_BASE="http://localhost:5000"
BUCKET="molitao"
OPERATOR="molitao"
PASSWORD="cHILcN0KfdVwcCkQz0Kjou7utChvtkpv"
DOMAIN="http://image.molitao.top"
LOCAL_FILE="$1"
REMOTE_PATH="${2:-apps/$(basename $LOCAL_FILE)}"

if [ -z "$LOCAL_FILE" ]; then
    echo "Usage: $0 <local-file> [remote-path]"
    exit 1
fi

if [ ! -f "$LOCAL_FILE" ]; then
    echo "File not found: $LOCAL_FILE"
    exit 1
fi

FILE_SIZE=$(stat -f%z "$LOCAL_FILE" 2>/dev/null || stat -c%s "$LOCAL_FILE" 2>/dev/null)
echo "Uploading $LOCAL_FILE ($FILE_SIZE bytes) to $REMOTE_PATH"

DEADLINE=$(($(date +%s) + 3600))
SAVE_KEY="/$REMOTE_PATH"

POLICY_JSON="{\"bucket\":\"$BUCKET\",\"save-key\":\"$SAVE_KEY\",\"expiration\":$DEADLINE,\"file-size\":{\"maximum\":$FILE_SIZE}}"
POLICY_BASE64=$(echo -n "$POLICY_JSON" | base64 | tr -d '=\n')

PASSWORD_MD5=$(echo -n "$PASSWORD" | md5 -s - | cut -d' ' -f2)
SIGNATURE=$(echo -n "$POLICY_BASE64" | openssl dgst -sha1 -hmac "$PASSWORD_MD5" -binary | base64 | tr -d '=+\n')

echo "Policy: $POLICY_BASE64"
echo "Signature: $SIGNATURE"

RESPONSE=$(curl -s -X PUT "http://v0.api.upyun.com/$BUCKET" \
    -H "Authorization: Bearer $SIGNATURE" \
    -H "Policy: $POLICY_BASE64" \
    -T "$LOCAL_FILE")

echo "Response: $RESPONSE"
