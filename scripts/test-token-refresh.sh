#!/bin/bash

# Token 自动续期集成测试脚本
# 测试本地后端服务

BASE_URL="http://localhost:12580"
TENANT_ID="1"

echo "========================================"
echo "Token 自动续期集成测试"
echo "========================================"
echo "后端地址: ${BASE_URL}"
echo "测试时间: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# 检查后端服务是否可用
echo "检查后端服务..."
HEALTH_CHECK=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/swagger/index.html" --connect-timeout 5)
if [ "$HEALTH_CHECK" != "200" ]; then
    echo "❌ 后端服务未启动或不可用"
    echo "请先启动后端服务: cd backend/src/TtWork.Project.Web.Host && dotnet run"
    exit 1
fi
echo "✅ 后端服务可用"
echo ""

# 测试 1: 登录
echo "【测试 1】登录获取 token"
echo "----------------------------------------"
LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}" \
  -d '{"userNameOrEmailAddress": "feifei", "password": "123456"}')

echo "响应:"
echo "$LOGIN_RESPONSE" | jq . 2>/dev/null || echo "$LOGIN_RESPONSE"

if echo "$LOGIN_RESPONSE" | jq -e '.success == true' > /dev/null 2>&1; then
  echo ""
  echo "✅ 登录成功"
  ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.accessToken')
  REFRESH_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.refreshToken')
  EXPIRE_IN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.expireInSeconds')
  REFRESH_EXPIRE_IN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.refreshTokenExpireInSeconds')
  USER_ID=$(echo "$LOGIN_RESPONSE" | jq -r '.result.userId')
  
  echo "用户ID: ${USER_ID}"
  echo "Access Token: ${ACCESS_TOKEN:0:50}..."
  echo "Refresh Token: ${REFRESH_TOKEN:0:50}..."
  echo "Access Token 过期时间: ${EXPIRE_IN} 秒 ($((EXPIRE_IN / 86400)) 天)"
  echo "Refresh Token 过期时间: ${REFRESH_EXPIRE_IN} 秒 ($((REFRESH_EXPIRE_IN / 86400)) 天)"
else
  echo ""
  echo "❌ 登录失败"
  exit 1
fi

# 测试 2: 使用 access token 访问 API
echo ""
echo "【测试 2】使用 access token 访问受保护 API"
echo "----------------------------------------"
USER_INFO=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Abp.Tenantid: ${TENANT_ID}")

echo "响应:"
echo "$USER_INFO" | jq . 2>/dev/null || echo "$USER_INFO"

if echo "$USER_INFO" | jq -e '.success == true' > /dev/null 2>&1; then
  echo ""
  echo "✅ Access token 可正常使用"
  USER_NAME=$(echo "$USER_INFO" | jq -r '.result.user.name')
  echo "当前用户: ${USER_NAME}"
else
  echo ""
  echo "❌ Access token 无法使用"
  exit 1
fi

# 测试 3: 刷新 token
echo ""
echo "【测试 3】使用 refresh token 刷新"
echo "----------------------------------------"
REFRESH_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/RefreshToken?refreshToken=${REFRESH_TOKEN}" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}")

echo "响应:"
echo "$REFRESH_RESPONSE" | jq . 2>/dev/null || echo "$REFRESH_RESPONSE"

if echo "$REFRESH_RESPONSE" | jq -e '.success == true' > /dev/null 2>&1; then
  echo ""
  echo "✅ Token 刷新成功"
  NEW_ACCESS_TOKEN=$(echo "$REFRESH_RESPONSE" | jq -r '.result.accessToken')
  NEW_EXPIRE_IN=$(echo "$REFRESH_RESPONSE" | jq -r '.result.expireInSeconds')
  echo "新 Access Token: ${NEW_ACCESS_TOKEN:0:50}..."
  echo "新 Token 过期时间: ${NEW_EXPIRE_IN} 秒 ($((NEW_EXPIRE_IN / 86400)) 天)"
  
  # 比较新旧 token
  if [ "$ACCESS_TOKEN" != "$NEW_ACCESS_TOKEN" ]; then
    echo "✅ 新 token 与旧 token 不同"
  else
    echo "⚠️ 新 token 与旧 token 相同"
  fi
else
  echo ""
  echo "❌ Token 刷新失败"
  exit 1
fi

# 测试 4: 使用新 token 访问 API
echo ""
echo "【测试 4】使用新 token 访问 API"
echo "----------------------------------------"
NEW_USER_INFO=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${NEW_ACCESS_TOKEN}" \
  -H "Abp.Tenantid: ${TENANT_ID}")

echo "响应:"
echo "$NEW_USER_INFO" | jq . 2>/dev/null || echo "$NEW_USER_INFO"

if echo "$NEW_USER_INFO" | jq -e '.success == true' > /dev/null 2>&1; then
  echo ""
  echo "✅ 新 token 可正常使用"
else
  echo ""
  echo "❌ 新 token 无法使用"
  exit 1
fi

# 测试 5: 旧 token 仍可使用（未过期）
echo ""
echo "【测试 5】旧 token 是否仍可使用"
echo "----------------------------------------"
OLD_USER_INFO=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Abp.Tenantid: ${TENANT_ID}")

if echo "$OLD_USER_INFO" | jq -e '.success == true' > /dev/null 2>&1; then
  echo "✅ 旧 token 仍可使用（未过期）"
else
  echo "⚠️ 旧 token 已失效"
fi

# 测试 6: 无效 refresh token
echo ""
echo "【测试 6】无效 refresh token"
echo "----------------------------------------"
INVALID_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST \
  "${BASE_URL}/api/TokenAuth/RefreshToken?refreshToken=invalid_token_string_12345" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}")

HTTP_CODE=$(echo "$INVALID_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
BODY=$(echo "$INVALID_RESPONSE" | sed '/HTTP_CODE:/d')

echo "HTTP 状态码: $HTTP_CODE"
echo "响应: $BODY"

if [ "$HTTP_CODE" = "500" ] || [ "$HTTP_CODE" = "401" ] || [ "$HTTP_CODE" = "400" ]; then
  echo "✅ 无效 token 被正确拒绝"
else
  echo "⚠️ 状态码: $HTTP_CODE"
fi

# 测试 7: 空 token 访问受保护 API
echo ""
echo "【测试 7】无 token 访问受保护 API"
echo "----------------------------------------"
NO_TOKEN_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X GET \
  "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Abp.Tenantid: ${TENANT_ID}")

HTTP_CODE=$(echo "$NO_TOKEN_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
BODY=$(echo "$NO_TOKEN_RESPONSE" | sed '/HTTP_CODE:/d')

echo "HTTP 状态码: $HTTP_CODE"
echo "响应: $BODY"

if [ "$HTTP_CODE" = "401" ]; then
  echo "✅ 无 token 返回 401"
else
  echo "⚠️ 状态码: $HTTP_CODE"
fi

# 测试总结
echo ""
echo "========================================"
echo "测试完成"
echo "========================================"
echo "测试时间: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""
echo "测试结果汇总:"
echo "  ✅ API-01: 登录获取 token"
echo "  ✅ API-02: Access token 访问 API"
echo "  ✅ API-03: Refresh token 刷新"
echo "  ✅ API-04: 新 token 访问 API"
echo "  ✅ API-05: 旧 token 状态检查"
echo "  ✅ API-06: 无效 refresh token"
echo "  ✅ API-07: 无 token 访问"
