# Token 自动续期测试方案

## 1. 测试概述

### 1.1 测试目标
验证 token 自动续期功能在前后端的正确性，确保用户不会因 token 过期而频繁重新登录。

### 1.2 测试范围
- 后端 API：本地开发服务，使用 curl 测试
- 前端 PC：Vitest 单元测试
- 前端 UniApp：Vitest 单元测试

### 1.3 测试环境
- 后端地址：`http://localhost:21061`（本地开发服务）
- 测试账号：需要预先创建或使用现有账号
- Token 配置：Access Token 7天，Refresh Token 7天

---

## 2. 后端 API 测试（curl）

### 2.1 测试前置条件
- 本地后端服务已启动：`cd backend && dotnet run`
- 服务地址：`http://localhost:21061`
- 准备测试账号：用户名和密码

### 2.2 测试用例

#### API-01: 登录获取 token
**目的**：验证登录接口返回 accessToken、refreshToken、expireInSeconds

**请求**：
```bash
curl -X POST "http://localhost:21061/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1" \
  -d '{
    "userNameOrEmailAddress": "admin",
    "password": "123qwe"
  }'
```

**预期结果**：
- HTTP 状态码：200
- 返回 JSON 包含：
  - `success: true`
  - `result.accessToken`：非空字符串
  - `result.refreshToken`：非空字符串
  - `result.expireInSeconds`：约 604800（7天）
  - `result.refreshTokenExpireInSeconds`：约 604800（7天）
  - `result.userId`：用户ID

**验证命令**：
```bash
# 保存响应并提取 token
RESPONSE=$(curl -s -X POST "http://localhost:21061/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1" \
  -d '{"userNameOrEmailAddress": "admin", "password": "123qwe"}')

echo "$RESPONSE" | jq .

# 提取 token（需要安装 jq）
ACCESS_TOKEN=$(echo "$RESPONSE" | jq -r '.result.accessToken')
REFRESH_TOKEN=$(echo "$RESPONSE" | jq -r '.result.refreshToken')

echo "Access Token: ${ACCESS_TOKEN:0:50}..."
echo "Refresh Token: ${REFRESH_TOKEN:0:50}..."
```

---

#### API-02: 有效 refresh token 刷新
**目的**：验证使用有效 refresh token 可以获取新的 access token

**前置条件**：已完成 API-01，获取到 refreshToken

**请求**：
```bash
curl -X POST "http://localhost:21061/api/TokenAuth/RefreshToken?refreshToken=${REFRESH_TOKEN}" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1"
```

**预期结果**：
- HTTP 状态码：200
- 返回 JSON 包含：
  - `success: true`
  - `result.accessToken`：新的 access token（与原 token 不同）
  - `result.expireInSeconds`：约 604800

**验证命令**：
```bash
REFRESH_RESPONSE=$(curl -s -X POST "http://localhost:21061/api/TokenAuth/RefreshToken?refreshToken=${REFRESH_TOKEN}" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1")

echo "$REFRESH_RESPONSE" | jq .

NEW_ACCESS_TOKEN=$(echo "$REFRESH_RESPONSE" | jq -r '.result.accessToken')
echo "New Access Token: ${NEW_ACCESS_TOKEN:0:50}..."

# 验证新 token 与旧 token 不同
if [ "$ACCESS_TOKEN" != "$NEW_ACCESS_TOKEN" ]; then
  echo "✅ 新 token 与旧 token 不同"
else
  echo "❌ 新 token 与旧 token 相同（异常）"
fi
```

---

#### API-03: 无效 refresh token 刷新
**目的**：验证无效 refresh token 被拒绝

**请求**：
```bash
curl -X POST "http://localhost:21061/api/TokenAuth/RefreshToken?refreshToken=invalid_token_string" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1"
```

**预期结果**：
- HTTP 状态码：500 或返回 `success: false`
- 返回错误信息

**验证命令**：
```bash
INVALID_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST \
  "http://localhost:21061/api/TokenAuth/RefreshToken?refreshToken=invalid_token_string" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: 1")

HTTP_CODE=$(echo "$INVALID_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
BODY=$(echo "$INVALID_RESPONSE" | sed '/HTTP_CODE:/d')

echo "HTTP 状态码: $HTTP_CODE"
echo "响应内容: $BODY"
```

---

#### API-04: 使用新 token 访问受保护 API
**目的**：验证刷新后的新 token 可以正常访问受保护 API

**前置条件**：已完成 API-02，获取到新的 accessToken

**请求**：
```bash
curl -X GET "http://localhost:21061/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${NEW_ACCESS_TOKEN}" \
  -H "Abp.Tenantid: 1"
```

**预期结果**：
- HTTP 状态码：200
- 返回用户信息

**验证命令**：
```bash
USER_INFO=$(curl -s -X GET "http://localhost:21061/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${NEW_ACCESS_TOKEN}" \
  -H "Abp.Tenantid: 1")

echo "$USER_INFO" | jq .
```

---

#### API-05: 使用旧 access token 访问
**目的**：验证旧 access token 在未过期时仍可使用

**请求**：
```bash
curl -X GET "http://localhost:21061/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Abp.Tenantid: 1"
```

**预期结果**：
- HTTP 状态码：200（因为 token 未过期）
- 正常返回用户信息

---

#### API-06: 空 token 访问受保护 API
**目的**：验证无 token 时返回 401

**请求**：
```bash
curl -X GET "http://localhost:21061/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Abp.Tenantid: 1"
```

**预期结果**：
- HTTP 状态码：401
- 返回 `unAuthorizedRequest: true`

---

## 3. 前端单元测试

### 3.1 PC 端测试（Vitest）

#### 测试文件位置
`pc/src/utils/__tests__/tokenManager.test.ts`

#### 测试用例

**FE-PC-01: token 存储管理**
```typescript
describe('Token Storage', () => {
  it('should set and get token correctly', () => {
    setToken('test-token')
    expect(getToken()).toBe('test-token')
  })

  it('should set and get refresh token correctly', () => {
    setRefreshToken('test-refresh-token')
    expect(getRefreshToken()).toBe('test-refresh-token')
  })

  it('should set and get token expire time correctly', () => {
    setTokenExpireTime(3600)
    const expireTime = getTokenExpireTime()
    expect(expireTime).toBeGreaterThan(Date.now())
  })

  it('should clear all tokens', () => {
    setToken('test-token')
    setRefreshToken('test-refresh-token')
    clearAuthTokens()
    expect(getToken()).toBeNull()
    expect(getRefreshToken()).toBeNull()
  })
})
```

**FE-PC-02: token 过期判断**
```typescript
describe('Token Expiration Check', () => {
  it('should return true when token is expiring soon', () => {
    setTokenExpireTime(1800) // 30分钟后过期
    expect(isTokenExpiringSoon(3600)).toBe(true)
  })

  it('should return false when token is not expiring soon', () => {
    setTokenExpireTime(86400) // 24小时后过期
    expect(isTokenExpiringSoon(3600)).toBe(false)
  })

  it('should return false when no expire time set', () => {
    removeTokenExpireTime()
    expect(isTokenExpiringSoon(3600)).toBe(false)
  })
})
```

### 3.2 UniApp 端测试（Vitest）

#### 测试文件位置
`molitao_uniapp/src/utils/__tests__/tokenManager.test.ts`

#### 测试用例
与 PC 端类似，但使用 uni API mock

---

## 4. 前端集成测试（真实后端）

### 4.1 测试脚本

创建测试脚本：`scripts/test-token-refresh.sh`

```bash
#!/bin/bash

# Token 自动续期集成测试脚本
# 测试本地后端服务

BASE_URL="http://localhost:21061"
TENANT_ID="1"

echo "========================================"
echo "Token 自动续期集成测试"
echo "========================================"

# 测试 1: 登录
echo ""
echo "【测试 1】登录获取 token"
echo "----------------------------------------"
LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}" \
  -d '{"userNameOrEmailAddress": "admin", "password": "123qwe"}')

echo "$LOGIN_RESPONSE" | jq .

if echo "$LOGIN_RESPONSE" | jq -e '.success == true' > /dev/null; then
  echo "✅ 登录成功"
  ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.accessToken')
  REFRESH_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.refreshToken')
  EXPIRE_IN=$(echo "$LOGIN_RESPONSE" | jq -r '.result.expireInSeconds')
  echo "Token 过期时间: ${EXPIRE_IN} 秒 ($((EXPIRE_IN / 86400)) 天)"
else
  echo "❌ 登录失败"
  exit 1
fi

# 测试 2: 刷新 token
echo ""
echo "【测试 2】刷新 token"
echo "----------------------------------------"
REFRESH_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/TokenAuth/RefreshToken?refreshToken=${REFRESH_TOKEN}" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}")

echo "$REFRESH_RESPONSE" | jq .

if echo "$REFRESH_RESPONSE" | jq -e '.success == true' > /dev/null; then
  echo "✅ Token 刷新成功"
  NEW_ACCESS_TOKEN=$(echo "$REFRESH_RESPONSE" | jq -r '.result.accessToken')
else
  echo "❌ Token 刷新失败"
  exit 1
fi

# 测试 3: 验证新 token 可用
echo ""
echo "【测试 3】使用新 token 访问 API"
echo "----------------------------------------"
USER_INFO=$(curl -s -X GET "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" \
  -H "Authorization: Bearer ${NEW_ACCESS_TOKEN}" \
  -H "Abp.Tenantid: ${TENANT_ID}")

echo "$USER_INFO" | jq .

if echo "$USER_INFO" | jq -e '.success == true' > /dev/null; then
  echo "✅ 新 token 可正常使用"
else
  echo "❌ 新 token 无法使用"
  exit 1
fi

# 测试 4: 无效 refresh token
echo ""
echo "【测试 4】无效 refresh token"
echo "----------------------------------------"
INVALID_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST \
  "${BASE_URL}/api/TokenAuth/RefreshToken?refreshToken=invalid_token" \
  -H "Content-Type: application/json" \
  -H "Abp.Tenantid: ${TENANT_ID}")

HTTP_CODE=$(echo "$INVALID_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
echo "HTTP 状态码: $HTTP_CODE"

if [ "$HTTP_CODE" = "500" ] || [ "$HTTP_CODE" = "401" ]; then
  echo "✅ 无效 token 被正确拒绝"
else
  echo "⚠️ 状态码非预期值，但可能正常"
fi

echo ""
echo "========================================"
echo "测试完成"
echo "========================================"
```

---

## 5. 测试执行步骤

### 5.1 后端 API 测试

```bash
# 1. 启动本地后端服务
cd backend/src/TtWork.Project.Web.Host
dotnet run

# 2. 等待服务启动（约 10-20 秒）

# 3. 执行测试脚本
chmod +x scripts/test-token-refresh.sh
./scripts/test-token-refresh.sh
```

### 5.2 前端单元测试

```bash
# PC 端
cd pc
npm run test

# UniApp 端
cd molitao_uniapp
npm run test
```

---

## 6. 测试检查清单

| 编号 | 测试项 | 状态 | 备注 |
|------|--------|------|------|
| API-01 | 登录获取 token | ⬜ | |
| API-02 | 有效 refresh token 刷新 | ⬜ | |
| API-03 | 无效 refresh token 刷新 | ⬜ | |
| API-04 | 使用新 token 访问 API | ⬜ | |
| API-05 | 使用旧 token 访问 API | ⬜ | |
| API-06 | 空 token 访问 API | ⬜ | |
| FE-PC-01 | token 存储管理 | ⬜ | |
| FE-PC-02 | token 过期判断 | ⬜ | |
| FE-UNI-01 | token 存储管理 | ⬜ | |
| FE-UNI-02 | token 过期判断 | ⬜ | |

---

## 7. 问题记录

| 日期 | 问题描述 | 解决方案 | 状态 |
|------|----------|----------|------|
| | | | |
