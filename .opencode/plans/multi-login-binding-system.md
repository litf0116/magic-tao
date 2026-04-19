# 多登录方式绑定系统设计方案

> 文档版本: v1.0  
> 创建日期: 2026-04-19  
> 状态: 设计阶段

---

## 一、项目背景

### 1.1 现有登录方式

系统目前支持以下登录方式：

| 客户端 | 登录方式 | 实现状态 | 后端 Provider |
|--------|---------|---------|---------------|
| PC端 | 微信扫码登录 | ✅ 已实现 | `WeChatPub` |
| PC端 | 密码登录 | ✅ 已实现 | - |
| 小程序端 | 微信一键登录 | ✅ 已实现 | `WeChatMiniOpenid` |
| APP端 | 微信登录 | ✅ 已实现 | `WeChatApp` |

### 1.2 需求目标

1. **统一多登录方式支持**: 一个用户可绑定多种登录方式（微信、手机号、密码）
2. **新增登录入口**: 
   - PC端: 新增手机号+验证码登录
   - H5端: 新增手机号+验证码登录、微信授权登录
   - APP端: 新增手机号+验证码登录、账号密码登录
3. **账号绑定管理**: 用户可在设置页面管理绑定的登录方式
4. **避免账号冲突**: 处理不同登录方式导致的账号不一致问题

---

## 二、系统架构

### 2.1 整体架构

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           客户端层                                       │
├─────────────┬─────────────┬─────────────┬─────────────┬─────────────────┤
│    PC端     │   小程序端   │    H5端     │    APP端    │                 │
│  Vue 3 +    │  UniApp +   │  UniApp +   │  UniApp +   │                 │
│  Element+   │  Vue 3      │  Vue 3      │  Vue 3      │                 │
└──────┬──────┴──────┬──────┴──────┬──────┴──────┬──────┴─────────────────┘
       │             │             │             │
       └─────────────┴──────┬──────┴─────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        统一认证服务层                                     │
│                   (TokenAuthController)                                  │
├─────────────────────────────────────────────────────────────────────────┤
│  登录入口:                                                               │
│  ├─ POST /api/TokenAuth/Authenticate           (账号密码)               │
│  ├─ POST /api/TokenAuth/PhoneAuthenticate      (手机号验证码) [新增]    │
│  ├─ POST /api/TokenAuth/SendSmsCode            (发送验证码) [新增]      │
│  ├─ POST /api/TokenAuth/WeixinMiniAuthenticate (小程序微信)             │
│  ├─ POST /api/TokenAuth/AuthenticateWeixinApp  (APP微信)                │
│  └─ GET  /api/TokenAuth/QrLogin                (PC扫码)                 │
│                                                                         │
│  账号绑定:                                                               │
│  ├─ POST /api/services/app/Account/BindPhone   (绑定手机号) [新增]      │
│  ├─ POST /api/services/app/Account/SetPassword (设置密码)               │
│  ├─ GET  /api/services/app/Account/GetLoginBindings (获取绑定) [新增]   │
│  └─ POST /api/services/app/Account/UnbindLogin (解绑) [新增]            │
└─────────────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           数据层                                         │
├─────────────────────────────────────────────────────────────────────────┤
│  User 表: 用户核心信息                                                   │
│  ├─ Id, UserName, Name, PhoneNumber, EmailAddress, HeadImgUrl          │
│  └─ Password (密码登录凭证)                                              │
│                                                                         │
│  UserLogin 表: 多登录方式绑定 (ABP框架)                                  │
│  ├─ UserId, TenantId                                                    │
│  ├─ LoginProvider (提供者标识)                                          │
│  └─ ProviderKey (提供者用户标识)                                        │
│                                                                         │
│  SmsVerificationCode 表: 短信验证码 [新增]                               │
│  ├─ PhoneNumber, Code, Purpose, IsUsed, ExpireTime                     │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 用户-登录方式关联模型

```
┌─────────────────────────────────────────────────────────────┐
│                    User 表 (唯一用户)                        │
├─────────────────────────────────────────────────────────────┤
│  Id | UserName | PhoneNumber | Email | Name | HeadImgUrl   │
│  1001 | user_001 | 138****8000 | ... | 张三 | ...         │
└─────────────────────────────────────────────────────────────┘
       │
       │ 1:N
       ▼
┌─────────────────────────────────────────────────────────────┐
│                 UserLogin 表 (多登录方式绑定)                │
├─────────────────────────────────────────────────────────────┤
│  UserId | LoginProvider      | ProviderKey                  │
│  1001   | WeChatUnionId      | ox123...        (跨平台唯一) │
│  1001   | WeChatMiniOpenid   | mini_abc...      (小程序)    │
│  1001   | WeChatPubOpenid    | pub_def...       (公众号)    │
│  1001   | WeChatApp          | app_ghi...       (APP)       │
│  1001   | Phone              | 13800138000      (手机号)    │
└─────────────────────────────────────────────────────────────┘

注: 密码存储在 User.Password 字段，不单独记录到 UserLogin 表
```

### 2.3 LoginProvider 定义

```csharp
// backend/Modules/TtWork.Abp.Core/Consts.cs
public class LoginProvider {
    // 已有
    public const string WeChatPub = "WeChatPub";           // 公众号登录
    public const string WeChatPubOpenid = "WeChatPubOpenid";
    public const string WeChatMiniOpenid = "WeChatMiniOpenid"; // 小程序登录
    public const string WeChatUnionId = "WeChatUnionId";   // 微信UnionId (跨平台)
    public const string WeChatMiniPhone = "WeChatMiniPhone";   // 小程序手机号授权
    public const string WeChatApp = "WeChatApp";           // APP微信登录
    
    // 新增
    public const string Phone = "Phone";                   // 手机号登录
}
```

---

## 三、各客户端登录方式

### 3.1 PC端 (Vue 3 + Element Plus)

| 登录方式 | 状态 | 说明 |
|---------|------|------|
| 微信扫码登录 | ✅ 已有 | 公众号二维码扫码 |
| 密码登录 | ✅ 已有 | 用户名+密码 |
| 手机号验证码登录 | 🆕 新增 | 手机号+短信验证码 |

**登录页面设计:**

```
┌─────────────────────────────────────────────┐
│               魔力淘登录                      │
├─────────────────────────────────────────────┤
│  [扫码登录]  [密码登录]  [验证码登录] ← Tab  │
├─────────────────────────────────────────────┤
│  Tab 1: 扫码登录 (已有)                      │
│  Tab 2: 密码登录 (已有)                      │
│  Tab 3: 验证码登录 (新增)                    │
│        - 手机号输入框                        │
│        - 验证码输入框 + [获取验证码]         │
│        - [登录] 按钮                         │
└─────────────────────────────────────────────┘
```

### 3.2 小程序端 (UniApp + Vue 3)

| 登录方式 | 状态 | 说明 |
|---------|------|------|
| 微信一键登录 | ✅ 已有 | **保持不变** |

**登录页面设计:**

```
┌─────────────────────────┐
│                         │
│    [微信一键登录]        │  ← 保持现有实现
│                         │
│  □ 同意《用户协议》      │
│  [返回]                  │
└─────────────────────────┘
```

### 3.3 H5端 (UniApp + Vue 3)

| 登录方式 | 状态 | 说明 |
|---------|------|------|
| 账号密码登录 | ✅ 已有 | 用户名+密码 |
| 手机号验证码登录 | 🆕 新增 | 手机号+短信验证码 |
| 微信授权登录 | 🆕 新增 | 公众号网页授权 |

**登录页面设计:**

```
┌─────────────────────────────────────────────┐
│                  登录                        │
├─────────────────────────────────────────────┤
│  [密码登录]  [验证码登录]  ← Tab 切换        │
├─────────────────────────────────────────────┤
│  Tab 1: 密码登录 (已有)                      │
│  Tab 2: 验证码登录 (新增)                    │
├─────────────────────────────────────────────┤
│  ────────── 其他登录方式 ──────────         │
│  [💬 微信授权登录]  ← 新增                   │
│                                             │
│  □ 我已阅读并同意《用户协议》和《隐私政策》  │
│  [返回首页]                                 │
└─────────────────────────────────────────────┘
```

### 3.4 APP端 (UniApp + Vue 3)

| 登录方式 | 状态 | 说明 |
|---------|------|------|
| 微信登录 | ✅ 已有 | 调用微信APP授权 |
| 账号密码登录 | 🆕 新增 | 用户名+密码 |
| 手机号验证码登录 | 🆕 新增 | 手机号+短信验证码 |

**登录页面设计:**

```
┌─────────────────────────────────────────────┐
│                  登录                        │
├─────────────────────────────────────────────┤
│  [密码登录]  [验证码登录]  ← Tab 切换        │
├─────────────────────────────────────────────┤
│  Tab 1: 密码登录 (新增)                      │
│  Tab 2: 验证码登录 (新增)                    │
├─────────────────────────────────────────────┤
│  ────────── 其他登录方式 ──────────         │
│  [💬 微信登录]  ← 已有                       │
│                                             │
│  □ 我已阅读并同意《用户协议》和《隐私政策》  │
│  [返回首页]                                 │
└─────────────────────────────────────────────┘
```

---

## 四、登录流程设计

### 4.1 手机号验证码登录流程

```
┌─────────────────────────────────────────────────────────────────┐
│                     手机号验证码登录流程                         │
└─────────────────────────────────────────────────────────────────┘

用户输入手机号
    │
    ▼
┌─────────────────┐
│ 点击获取验证码   │ ──────► 调用 POST /api/TokenAuth/SendSmsCode
└────────┬────────┘          │
         │                   ▼
         │           ┌─────────────────────┐
         │           │ 短信服务发送验证码   │
         │           │ 存储: Phone + Code  │
         │           │ 过期时间: 5分钟      │
         │           └─────────────────────┘
         │
         ▼
用户输入验证码
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│              调用 POST /api/TokenAuth/PhoneAuthenticate          │
│              参数: { phoneNumber, code }                         │
└─────────────────────────────────────────────────────────────────┘
    │
    ▼
验证验证码
    │
    ├──► 验证失败 ──► 返回错误提示
    │
    └──► 验证成功
              │
              ▼
    ┌─────────────────────────────────────────────────────────────┐
    │ 查询 UserLogin 表                                            │
    │ WHERE LoginProvider = 'Phone' AND ProviderKey = phoneNumber │
    └─────────────────────────────────────────────────────────────┘
              │
              ├──► 找到记录 ──► 获取关联用户 ──► 登录成功
              │
              └──► 未找到记录
                        │
                        ▼
              ┌─────────────────────────────┐
              │ 创建新用户                   │
              │ - UserName: 手机号           │
              │ - PhoneNumber: 手机号        │
              │ - IsPhoneNumberConfirmed: true │
              │ 添加 UserLogin 记录          │
              │ - LoginProvider: Phone       │
              │ - ProviderKey: 手机号        │
              └─────────────────────────────┘
                        │
                        ▼
                  登录成功
```

### 4.2 微信登录关联流程

```
┌─────────────────────────────────────────────────────────────────┐
│                     微信登录关联流程                             │
│             (优先通过 UnionId 跨平台关联)                        │
└─────────────────────────────────────────────────────────────────┘

微信授权登录
    │
    ▼
获取 openid + unionid
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│ Step 1: 优先通过 UnionId 查找用户                               │
│ (UnionId 是微信开放平台下同一主体的唯一标识)                     │
└─────────────────────────────────────────────────────────────────┘
    │
    ├──► 找到用户
    │         │
    │         ▼
    │    ┌─────────────────────────────────────────┐
    │    │ 补充绑定当前平台的 OpenId               │
    │    │ INSERT UserLogin (OpenId)               │
    │    │ 如果不存在的话                          │
    │    └─────────────────────────────────────────┘
    │         │
    │         ▼
    │    登录成功
    │
    └──► 未找到 (UnionId 未绑定)
              │
              ▼
    ┌─────────────────────────────────────────────────────────────┐
    │ Step 2: 通过当前平台 OpenId 查找用户                        │
    │ (可能是旧用户，没有 UnionId)                                │
    └─────────────────────────────────────────────────────────────┘
              │
              ├──► 找到用户
              │         │
              │         ▼
              │    ┌─────────────────────────────────────────┐
              │    │ 补充绑定 UnionId (如果有)               │
              │    │ INSERT UserLogin (UnionId)              │
              │    └─────────────────────────────────────────┘
              │         │
              │         ▼
              │    登录成功
              │
              └──► 未找到
                        │
                        ▼
              ┌─────────────────────────────────────────┐
              │ Step 3: 创建新用户                       │
              │ - UserName: openid/随机                  │
              │ - Name: 微信昵称                         │
              │ - HeadImgUrl: 微信头像                   │
              │ 添加 UserLogin 记录:                     │
              │ - UnionId (如果有)                       │
              │ - 当前平台 OpenId                        │
              └─────────────────────────────────────────┘
                        │
                        ▼
                  登录成功
```

---

## 五、账号绑定管理

### 5.1 账号绑定页面设计

```
┌─────────────────────────────────────────────┐
│              账号与安全                       │
├─────────────────────────────────────────────┤
│                                             │
│  登录方式                                    │
│  ┌─────────────────────────────────────────┐│
│  │ 📱 手机号                                ││
│  │    138****8000                          ││
│  │    已验证  ✓                            ││
│  │                          [修改] [解绑]  ││
│  ├─────────────────────────────────────────┤│
│  │ 💬 微信                                  ││
│  │    已绑定                                ││
│  │                          [解绑]         ││
│  ├─────────────────────────────────────────┤│
│  │ 🔐 登录密码                              ││
│  │    已设置                                ││
│  │                          [修改]         ││
│  └─────────────────────────────────────────┘│
│                                             │
│  添加登录方式                                │
│  ┌─────────────────────────────────────────┐│
│  │  [+ 绑定手机号]  [+ 绑定微信]            ││
│  └─────────────────────────────────────────┘│
│                                             │
├─────────────────────────────────────────────┤
│  ⚠️ 安全提示                                 │
│  • 绑定手机号后可使用手机号+验证码登录        │
│  • 建议绑定多种登录方式，避免账号丢失        │
│  • 解绑前请确保至少保留一种登录方式          │
└─────────────────────────────────────────────┘
```

### 5.2 绑定手机号流程

```
用户已登录 → 个人设置 → 账号与安全 → 绑定手机号
    │
    ▼
输入手机号 → 获取验证码 → 输入验证码
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│              调用 POST /api/services/app/Account/BindPhone       │
│              参数: { phoneNumber, code }                         │
└─────────────────────────────────────────────────────────────────┘
    │
    ▼
验证验证码
    │
    ├──► 验证失败 ──► 返回错误提示
    │
    └──► 验证成功
              │
              ▼
    ┌─────────────────────────────────────────────────────────────┐
    │ 检查手机号是否已被其他用户绑定                              │
    │ SELECT * FROM UserLogin                                     │
    │ WHERE LoginProvider = 'Phone' AND ProviderKey = phoneNumber │
    └─────────────────────────────────────────────────────────────┘
              │
              ├──► 已被绑定
              │         │
              │         ▼
              │    ┌─────────────────────────────────────────────┐
              │    │ 返回错误:                                   │
              │    │ "该手机号已被其他账号绑定，                 │
              │    │  请使用该手机号登录后在设置中合并账号"       │
              │    └─────────────────────────────────────────────┘
              │
              └──► 未被绑定
                        │
                        ▼
              ┌─────────────────────────────────────────┐
              │ 检查当前用户是否已绑定手机号             │
              └─────────────────────────────────────────┘
                        │
                        ├──► 已绑定 ──► 解绑旧手机号
                        │
                        └──► 未绑定
                                  │
                                  ▼
              ┌─────────────────────────────────────────┐
              │ 绑定新手机号                             │
              │ INSERT UserLogin (Phone, phoneNumber)   │
              │ UPDATE User.PhoneNumber = phoneNumber   │
              └─────────────────────────────────────────┘
                                  │
                                  ▼
                           绑定成功
```

### 5.3 冲突处理规则

| 场景 | 处理方式 | 用户提示 |
|------|---------|---------|
| A用户绑定手机号，手机号已被B用户使用 | 禁止绑定 | "该手机号已被其他账号绑定，请使用该手机号登录后在设置中合并账号" |
| A用户绑定微信，微信已被B用户使用 | 禁止绑定 | "该微信已绑定其他账号" |
| 用户解绑最后一个登录方式 | 禁止解绑 | "至少需要保留一种登录方式" |

---

## 六、数据库设计

### 6.1 新增表: SmsVerificationCode

```sql
CREATE TABLE SmsVerificationCodes (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    PhoneNumber VARCHAR(20) NOT NULL COMMENT '手机号',
    Code VARCHAR(6) NOT NULL COMMENT '验证码',
    Purpose VARCHAR(20) NOT NULL COMMENT '用途: Login/BindPhone/ResetPassword',
    IsUsed BIT DEFAULT 0 COMMENT '是否已使用',
    ExpireTime DATETIME NOT NULL COMMENT '过期时间',
    CreationTime DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    TenantId INT NULL COMMENT '租户ID',
    
    INDEX IX_Phone_Purpose_Time (PhoneNumber, Purpose, CreationTime)
) COMMENT '短信验证码记录表';
```

### 6.2 现有表说明

**User 表** (ABP框架自带):
- 存储用户核心信息
- `Password` 字段存储密码登录凭证

**UserLogin 表** (ABP框架自带):
- 存储外部登录绑定关系
- 支持一个用户绑定多个外部登录提供者

---

## 七、API 接口定义

### 7.1 新增接口列表

| 接口 | 方法 | 说明 | 客户端 |
|------|------|------|--------|
| `/api/TokenAuth/SendSmsCode` | POST | 发送短信验证码 | PC, H5, APP |
| `/api/TokenAuth/PhoneAuthenticate` | POST | 手机号+验证码登录 | PC, H5, APP |
| `/api/services/app/Account/BindPhone` | POST | 绑定手机号 | PC, H5, APP |
| `/api/services/app/Account/GetLoginBindings` | GET | 获取登录绑定列表 | PC, H5, APP |
| `/api/services/app/Account/UnbindLogin` | POST | 解绑登录方式 | PC, H5, APP |

### 7.2 接口详细定义

#### 7.2.1 发送短信验证码

**请求:**
```http
POST /api/TokenAuth/SendSmsCode
Content-Type: application/json

{
    "phoneNumber": "13800138000",
    "purpose": "Login"
}
```

**响应:**
```json
{
    "success": true,
    "result": {
        "message": "验证码已发送",
        "expireInSeconds": 300
    }
}
```

**业务规则:**
- 同一手机号 60 秒内只能发送一次
- 同一手机号每天最多发送 10 次
- 验证码 5 分钟后过期

#### 7.2.2 手机号验证码登录

**请求:**
```http
POST /api/TokenAuth/PhoneAuthenticate
Content-Type: application/json

{
    "phoneNumber": "13800138000",
    "code": "123456"
}
```

**响应:**
```json
{
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expireInSeconds": 3600,
    "userId": 1001,
    "needProfileCompletion": false
}
```

#### 7.2.3 绑定手机号

**请求:**
```http
POST /api/services/app/Account/BindPhone
Content-Type: application/json
Authorization: Bearer {token}

{
    "phoneNumber": "13800138000",
    "code": "123456"
}
```

**响应:**
```json
{
    "success": true
}
```

#### 7.2.4 获取登录绑定列表

**请求:**
```http
GET /api/services/app/Account/GetLoginBindings
Authorization: Bearer {token}
```

**响应:**
```json
{
    "success": true,
    "result": {
        "items": [
            {
                "loginProvider": "Phone",
                "providerKey": "138****8000",
                "displayName": "手机号",
                "icon": "phone",
                "isBound": true,
                "boundTime": "2026-04-19T10:00:00Z"
            },
            {
                "loginProvider": "WeChatUnionId",
                "providerKey": "ox***123",
                "displayName": "微信",
                "icon": "wechat",
                "isBound": true,
                "boundTime": "2026-04-18T10:00:00Z"
            }
        ]
    }
}
```

#### 7.2.5 解绑登录方式

**请求:**
```http
POST /api/services/app/Account/UnbindLogin
Content-Type: application/json
Authorization: Bearer {token}

{
    "loginProvider": "Phone"
}
```

---

## 八、文件变更清单

### 8.1 后端

```
backend/
├── Modules/TtWork.Abp.Core/
│   └── Consts.cs                                    # 新增 LoginProvider.Phone
│
├── src/TtWork.Project/
│   ├── Domains/
│   │   └── SmsVerificationCode.cs                   # 新增: 验证码实体
│   │
│   └── Applications/Core/Authorization/Accounts/
│       ├── AccountAppService.cs                     # 扩展: 绑定/解绑方法
│       └── Dto/
│           ├── SendSmsCodeInput.cs                  # 新增
│           ├── PhoneAuthenticateInput.cs            # 新增
│           ├── BindPhoneInput.cs                    # 新增
│           └── LoginBindingDto.cs                   # 新增
│
├── src/TtWork.Project.Web.Core/
│   └── Controllers/TokenAuthController.cs           # 新增: PhoneAuthenticate, SendSmsCode
│
└── src/TtWork.Project.EntityFrameworkCore/
    └── Migrations/
        └── 20260419_AddSmsVerificationCode.cs       # 新增: 数据库迁移
```

### 8.2 PC端

```
pc/src/
├── views/auth/
│   └── login.vue                    # 修改: 新增验证码登录 Tab
│
├── views/user/settings/
│   └── AccountSecurity.vue          # 新增: 账号绑定管理页面
│
├── api/
│   └── appService.ts                # 扩展: 新增 API 类型定义
│
└── components/
    ├── PhoneLogin.vue               # 新增: 手机号登录组件
    └── BindPhoneDialog.vue          # 新增: 绑定手机号弹窗
```

### 8.3 H5端 (molitao_h5)

```
molitao_h5/src/
├── pages/index/
│   └── login.vue                    # 修改: 新增验证码登录Tab + 微信授权登录
│
├── stores/
│   └── userStore.ts                 # 扩展: 新增 phoneLogin() 方法
│
├── utils/
│   └── api.ts                       # 扩展: 新增 API 接口
│
├── pages/user/settings/
│   └── AccountSecurity.vue          # 新增: 账号绑定管理页面
│
└── components/login/
    ├── PasswordLogin.vue            # 新增: 密码登录组件
    ├── PhoneLogin.vue               # 新增: 手机号登录组件
    ├── WechatLogin.vue              # 新增: 微信登录组件
    └── LoginTabs.vue                # 新增: 登录方式Tab容器
```

### 8.4 APP/小程序端 (molitao_uniapp)

```
molitao_uniapp/src/
├── pages/index/
│   └── login.vue                    # 修改: APP端新增密码/验证码登录
│
├── stores/
│   └── userStore.ts                 # 扩展: 新增 phoneLogin() 方法
│
├── utils/
│   └── api.ts                       # 扩展: 新增 API 接口
│
└── pages/user/settings/
    └── AccountSecurity.vue          # 新增: 账号绑定管理页面 (APP专用)
```

---

## 九、实施计划

### 9.1 阶段划分

| 阶段 | 任务 | 工期 | 依赖 |
|------|------|------|------|
| **Phase 1** | 后端基础设施 | 2天 | - |
| **Phase 2** | 后端 API 开发 | 3天 | Phase 1 |
| **Phase 3** | PC端改造 | 2天 | Phase 2 |
| **Phase 4** | H5端改造 | 2天 | Phase 2 |
| **Phase 5** | APP端改造 | 2天 | Phase 2 |
| **Phase 6** | 测试与优化 | 2天 | Phase 3-5 |

**总工期: 约 13 个工作日**

### 9.2 风险点

| 风险 | 影响 | 应对措施 |
|------|------|---------|
| 短信服务稳定性 | 用户无法收到验证码 | 选择稳定服务商、重试机制、备用登录方式 |
| 账号合并冲突 | 用户数据混乱 | 严格限制自动合并、用户确认机制 |
| 旧用户没有 UnionId | 跨平台无法关联 | 引导用户重新授权、通过手机号关联 |

---

## 十、安全建议

1. **验证码安全**
   - 6 位数字验证码
   - 5 分钟过期
   - 同一手机号 60 秒内只能发送一次
   - 同一手机号每天最多 10 次

2. **登录安全**
   - 记录登录日志
   - 异常登录提醒

3. **绑定安全**
   - 绑定操作需要验证码确认
   - 解绑需要验证身份
   - 关键操作记录日志

---

## 十一、变更记录

| 版本 | 日期 | 变更内容 | 作者 |
|------|------|---------|------|
| v1.0 | 2026-04-19 | 初始版本 | AI Assistant |
