# 多登录绑定系统测试用例

## 1. 测试环境

- **后端地址**: http://localhost:12580
- **数据库**: MySQL (www_molitao_top)
- **测试模式**: SMS 验证码输出到日志

## 2. API 接口列表

| 接口 | 方法 | 路径 | 说明 |
|------|------|------|------|
| 发送短信验证码 | POST | /api/TokenAuth/SendSmsCode | 发送登录/绑定手机验证码 |
| 手机号登录 | POST | /api/TokenAuth/PhoneAuthenticate | 使用手机号+验证码登录 |
| 绑定手机号 | POST | /api/services/app/Account/BindPhone | 已登录用户绑定手机号 |
| 获取登录绑定 | GET | /api/services/app/Account/GetLoginBindings | 获取当前用户的所有登录方式 |
| 解绑登录方式 | POST | /api/services/app/Account/UnbindLogin | 解绑指定的登录方式 |

---

## 3. 测试用例

### 3.1 发送短信验证码 (SendSmsCode)

#### TC-001: 发送登录验证码
```bash
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138001"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`
- 日志中输出: `[测试模式] 验证码: XXXXXX (5分钟内有效)`

---

#### TC-002: 发送绑定手机验证码
```bash
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138002",
    "purpose": "bindphone"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`
- 日志中输出验证码

---

#### TC-003: 发送重置密码验证码
```bash
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138003",
    "purpose": "resetpassword"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`

---

#### TC-004: 发送验证码 - 手机号格式错误
```bash
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "12345"
  }'
```

**预期结果**:
- HTTP 400 或验证失败
- 返回错误信息

---

### 3.2 手机号登录 (PhoneAuthenticate)

#### TC-010: 新用户手机号登录 - 自动注册
```bash
# 1. 先获取验证码
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber": "13800138010"}'

# 2. 从日志获取验证码后登录 (替换 CODE)
curl -X POST http://localhost:12580/api/TokenAuth/PhoneAuthenticate \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138010",
    "code": "CODE"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`
- 返回 accessToken、userId
- 自动创建新用户
- 创建 Phone 登录绑定

---

#### TC-011: 已注册用户手机号登录
```bash
# 使用已存在的手机号登录
curl -X POST http://localhost:12580/api/TokenAuth/PhoneAuthenticate \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138010",
    "code": "CODE"
  }'
```

**预期结果**:
- HTTP 200
- 返回 accessToken
- 返回已存在用户信息

---

#### TC-012: 验证码错误
```bash
curl -X POST http://localhost:12580/api/TokenAuth/PhoneAuthenticate \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138010",
    "code": "000000"
  }'
```

**预期结果**:
- HTTP 500 或业务错误
- 错误信息: "验证码错误或已过期"

---

#### TC-013: 验证码过期
```bash
# 等待 6 分钟后使用验证码
curl -X POST http://localhost:12580/api/TokenAuth/PhoneAuthenticate \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138010",
    "code": "EXPIRED_CODE"
  }'
```

**预期结果**:
- 错误信息: "验证码错误或已过期"

---

### 3.3 绑定手机号 (BindPhone)

> 前置条件: 需要已登录用户的 token

#### TC-020: 已登录用户绑定手机号
```bash
# 1. 先获取验证码
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138020",
    "purpose": "bindphone"
  }'

# 2. 绑定手机号 (替换 TOKEN 和 CODE)
curl -X POST http://localhost:12580/api/services/app/Account/BindPhone \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "phoneNumber": "13800138020",
    "code": "CODE"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`
- 用户的 PhoneNumber 更新
- 创建 Phone 登录绑定

---

#### TC-021: 绑定已被其他用户使用的手机号
```bash
# 假设 13800138020 已被用户 A 绑定
# 用户 B 尝试绑定同一手机号
curl -X POST http://localhost:12580/api/services/app/Account/BindPhone \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer USER_B_TOKEN" \
  -d '{
    "phoneNumber": "13800138020",
    "code": "CODE"
  }'
```

**预期结果**:
- 错误信息: "该手机号已被其他账号绑定"
- 绑定失败

---

#### TC-022: 重复绑定同一手机号
```bash
# 用户已绑定 13800138020，再次绑定
curl -X POST http://localhost:12580/api/services/app/Account/BindPhone \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "phoneNumber": "13800138020",
    "code": "CODE"
  }'
```

**预期结果**:
- HTTP 200 (幂等操作)
- 或提示 "该手机号已绑定当前账号"

---

#### TC-023: 未登录用户绑定
```bash
curl -X POST http://localhost:12580/api/services/app/Account/BindPhone \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "13800138020",
    "code": "CODE"
  }'
```

**预期结果**:
- HTTP 401
- 错误信息: 未授权

---

### 3.4 获取登录绑定列表 (GetLoginBindings)

#### TC-030: 获取当前用户的登录绑定
```bash
curl -X GET http://localhost:12580/api/services/app/Account/GetLoginBindings \
  -H "Authorization: Bearer TOKEN"
```

**预期结果**:
```json
{
  "result": [
    {
      "loginProvider": "WeChatPub",
      "providerKey": "openid_xxx",
      "providerDisplayName": "微信公众号",
      "bindTime": "2024-01-01T00:00:00"
    },
    {
      "loginProvider": "Phone",
      "providerKey": "13800138001",
      "providerDisplayName": "手机号",
      "bindTime": "2024-01-02T00:00:00"
    }
  ],
  "success": true
}
```

---

#### TC-031: 未登录用户获取绑定
```bash
curl -X GET http://localhost:12580/api/services/app/Account/GetLoginBindings
```

**预期结果**:
- HTTP 401

---

### 3.5 解绑登录方式 (UnbindLogin)

#### TC-040: 解绑手机号登录
```bash
curl -X POST http://localhost:12580/api/services/app/Account/UnbindLogin \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "loginProvider": "Phone",
    "providerKey": "13800138001"
  }'
```

**预期结果**:
- HTTP 200
- `success: true`
- 删除对应的 UserLogin 记录

---

#### TC-041: 解绑微信登录
```bash
curl -X POST http://localhost:12580/api/services/app/Account/UnbindLogin \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "loginProvider": "WeChatPub",
    "providerKey": "openid_xxx"
  }'
```

**预期结果**:
- HTTP 200
- 删除对应的 UserLogin 记录

---

#### TC-042: 解绑唯一的登录方式
```bash
# 用户只有一种登录方式时尝试解绑
curl -X POST http://localhost:12580/api/services/app/Account/UnbindLogin \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "loginProvider": "Phone",
    "providerKey": "13800138001"
  }'
```

**预期结果**:
- 错误信息: "无法解绑唯一的登录方式，请先绑定其他登录方式"

---

#### TC-043: 解绑不存在的登录方式
```bash
curl -X POST http://localhost:12580/api/services/app/Account/UnbindLogin \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "loginProvider": "Phone",
    "providerKey": "99999999999"
  }'
```

**预期结果**:
- HTTP 200 (幂等操作)
- 或返回 "未找到该绑定"

---

### 3.6 登录提供商常量

```csharp
// 已定义的登录提供商
LoginProvider.WeChatPub        = "WeChatPub"         // 微信公众号
LoginProvider.WeChatPubOpenid  = "WeChatPubOpenid"   // 微信公众号 OpenID
LoginProvider.WeChatMiniOpenid = "WeChatMiniOpenid"  // 微信小程序 OpenID
LoginProvider.WeChatUnionId    = "WeChatUnionId"     // 微信 UnionID
LoginProvider.WeChatMiniPhone  = "WeChatMiniPhone"   // 微信小程序手机号
LoginProvider.WeChatApp        = "WeChatApp"         // 微信 App 登录
LoginProvider.Phone            = "Phone"             // 手机号登录
LoginProvider.Password         = "Password"          // 密码登录
```

---

### 3.7 短信验证码用途 (SmsCodePurpose)

```csharp
public enum SmsCodePurpose
{
    Login,          // 登录
    BindPhone,      // 绑定手机
    ResetPassword   // 重置密码
}
```

---

## 4. 业务场景测试

### 场景 1: 新用户首次手机号登录
1. 用户输入手机号
2. 调用 SendSmsCode (purpose: login)
3. 用户输入验证码
4. 调用 PhoneAuthenticate
5. **预期**: 自动创建用户，返回 token，创建 Phone 绑定

### 场景 2: 微信用户绑定手机号
1. 用户通过微信登录 (已有 WeChatMiniOpenid 绑定)
2. 用户点击"绑定手机号"
3. 调用 SendSmsCode (purpose: bindphone)
4. 用户输入验证码
5. 调用 BindPhone
6. **预期**: 用户拥有 WeChatMiniOpenid 和 Phone 两种登录方式

### 场景 3: 手机号用户绑定微信
1. 用户通过手机号登录 (已有 Phone 绑定)
2. 用户点击"绑定微信"
3. 微信授权后获取 openid/unionid
4. 后端创建 WeChatPub 或 WeChatMiniOpenid 绑定
5. **预期**: 用户拥有 Phone 和 WeChat 两种登录方式

### 场景 4: 解绑多余登录方式
1. 用户拥有 Phone + WeChatPub 两种登录方式
2. 用户点击解绑 WeChatPub
3. 调用 UnbindLogin
4. **预期**: 解绑成功，用户只剩 Phone 登录方式

### 场景 5: 尝试解绑唯一登录方式
1. 用户只有 Phone 一种登录方式
2. 用户点击解绑 Phone
3. 调用 UnbindLogin
4. **预期**: 解绑失败，提示"无法解绑唯一的登录方式"

### 场景 6: 账号冲突处理
1. 用户 A 通过微信登录
2. 用户 A 尝试绑定手机号 13800138001
3. 手机号 13800138001 已被用户 B 使用
4. **预期**: 绑定失败，提示"该手机号已被其他账号绑定，请联系客服处理账号合并"

---

## 5. 数据库验证

### 验证用户创建
```sql
SELECT * FROM AbpUsers WHERE PhoneNumber = '13800138001';
```

### 验证登录绑定
```sql
SELECT * FROM AbpUserLogins WHERE UserId = {userId};
```

### 验证短信验证码记录
```sql
SELECT * FROM SmsVerificationCodes 
WHERE PhoneNumber = '13800138001' 
ORDER BY CreationTime DESC 
LIMIT 5;
```

---

## 6. 测试执行记录

| 用例编号 | 测试日期 | 结果 | 备注 |
|---------|---------|------|------|
| TC-001 | 2026-04-20 | ✅ 通过 | 验证码正确输出到日志 |
| TC-002 | 2026-04-20 | ✅ 通过 | bindphone 验证码发送成功 |
| TC-003 | 2026-04-20 | ✅ 通过 | resetpassword 验证码发送成功 |
| TC-004 | 2026-04-20 | ⚠️ 问题 | 未验证手机号格式，错误手机号也发送成功 |
| TC-010 | 2026-04-20 | ❌ 失败 | MySQL 写入超时，新用户创建失败 |
| TC-011 | 2026-04-20 | ⏳ 跳过 | 依赖 TC-010 创建用户 |
| TC-012 | 2026-04-20 | ✅ 通过 | 正确返回"验证码错误或已过期" |
| TC-020 | 2026-04-20 | ⏳ 跳过 | MySQL 写入超时，无法测试绑定 |
| TC-030 | 2026-04-20 | ⏳ 跳过 | 需要登录 token |
| TC-031 | 2026-04-20 | ✅ 通过 | 正确返回 401 未授权 |
| TC-040 | 2026-04-20 | ⏳ 跳过 | MySQL 写入超时，无法测试解绑 |

---

## 7. 问题记录

| 问题编号 | 描述 | 状态 | 解决方案 |
|---------|------|------|---------|
| ISSUE-001 | ISmsVerificationCodeService 未注册 | ✅ 已解决 | 在 AbpApplicationModule 中注册 |
| ISSUE-002 | _smsVerificationCodeService 字段未赋值 | ✅ 已解决 | 修改构造函数赋值 |
| ISSUE-003 | LocalizationSourceName 未设置 | ✅ 已解决 | 在 UserRegistrationManager 构造函数中设置 |
| ISSUE-004 | 默认租户未激活 | ✅ 已解决 | UPDATE AbpTenants SET IsActive = 1 |
| ISSUE-005 | MySQL 命令超时 | ⏳ 待解决 | 本地数据库性能问题，INSERT 操作超时 |
| ISSUE-006 | SendSmsCode 未验证手机号格式 | 🆕 新发现 | 需要添加手机号格式验证 |

---

## 8. 测试脚本

### 8.1 快速测试脚本
```bash
#!/bin/bash
BASE_URL="http://localhost:12580"

echo "=== 测试 SendSmsCode ==="
curl -s -X POST $BASE_URL/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber":"13800138001"}'
echo

echo "=== 查看验证码日志 ==="
tail -5 /Users/mac/workspace/magic-tao/backend/src/TtWork.Project.Web.Host/logs/api-$(date +%Y%m%d).log | grep "验证码"
```

### 8.2 完整测试脚本
```bash
#!/bin/bash
# 需要安装 jq 工具

BASE_URL="http://localhost:12580"
LOG_FILE="/Users/mac/workspace/magic-tao/backend/src/TtWork.Project.Web.Host/logs/api-$(date +%Y%m%d).log"

# 测试发送验证码
test_send_sms() {
    local phone=$1
    local purpose=${2:-"login"}
    
    echo "发送验证码到 $phone (purpose: $purpose)"
    result=$(curl -s -X POST $BASE_URL/api/TokenAuth/SendSmsCode \
        -H "Content-Type: application/json" \
        -d "{\"phoneNumber\":\"$phone\",\"purpose\":\"$purpose\"}")
    
    if echo $result | grep -q '"success":true'; then
        echo "✅ 验证码发送成功"
        # 等待日志写入
        sleep 1
        code=$(grep "验证码:" $LOG_FILE | tail -1 | grep -oE '[0-9]{6}')
        echo "验证码: $code"
        echo $code
    else
        echo "❌ 验证码发送失败: $result"
        return 1
    fi
}

# 测试手机号登录
test_phone_auth() {
    local phone=$1
    local code=$2
    
    echo "手机号登录: $phone, 验证码: $code"
    result=$(curl -s -X POST $BASE_URL/api/TokenAuth/PhoneAuthenticate \
        -H "Content-Type: application/json" \
        -d "{\"phoneNumber\":\"$phone\",\"code\":\"$code\"}")
    
    echo $result
    if echo $result | grep -q '"success":true'; then
        echo "✅ 登录成功"
        token=$(echo $result | jq -r '.result.accessToken')
        echo "Token: $token"
    else
        echo "❌ 登录失败"
    fi
}

# 主测试流程
echo "====== 多登录绑定系统测试 ======"

# 测试1: 发送验证码
code=$(test_send_sms "13800138888")
if [ $? -eq 0 ]; then
    # 测试2: 手机号登录
    test_phone_auth "13800138888" "$code"
fi
```

---

## 9. 附录

### 9.1 获取测试用户 Token
```bash
# 方式1: 使用现有微信用户
# 从数据库获取一个用户的 token 或通过微信授权登录

# 方式2: 使用手机号登录创建测试用户
curl -X POST http://localhost:12580/api/TokenAuth/SendSmsCode \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber":"13900000001"}'

# 从日志获取验证码后登录
curl -X POST http://localhost:12580/api/TokenAuth/PhoneAuthenticate \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber":"13900000001","code":"验证码"}'
```

### 9.2 测试数据清理
```sql
-- 清理测试用户
DELETE FROM AbpUserLogins WHERE ProviderKey LIKE '13800138%';
DELETE FROM AbpUsers WHERE PhoneNumber LIKE '13800138%';

-- 清理测试验证码
DELETE FROM SmsVerificationCodes WHERE PhoneNumber LIKE '13800138%';
```
