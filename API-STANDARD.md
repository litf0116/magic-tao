# API 兼容标准 v1.0 — Spring Boot ⇔ ABP Framework

> **目标**: 定义 Java Spring Boot 新系统必须遵守的 API 接口规范，确保与现有 C# ABP v9.1.3 后端**100% 二进制兼容**，使 Flutter App / PC Web / UniApp 三个前端可在不修改代码的前提下，于新老系统间无缝切换。

---

## 一、路由规范

### 1.1 两条路由体系并存

ABP 框架同时支持两种路由风格，现有前端两套都使用：

| 风格 | 示例 | 对应 C# 端 |
|------|------|-----------|
| **ABP AppService 动态路由** | `POST /api/services/app/Announce/GetAll` | `AnnounceAppService.GetAll()` |
| **传统 Controller 路由** | `POST /api/TokenAuth/RefreshToken` | `TokenAuthController.RefreshToken()` |

### 1.2 Strangler Fig 迁移策略

```
┌──────────────┐     ┌──────────────────────────────────────┐
│   前端请求     │────▶│  旧 C# ABP 服务 /api/services/app/*   │
│ (不修改代码)    │     │  + /api/TokenAuth/*                   │
└──────────────┘     └──────────────────────────────────────┘
                            │
                            ▼  (逐模块迁移)
                     ┌──────────────────────────────────────┐
                     │  新 Spring Boot 服务 /api/services/app/*  │
                     │  + /api/TokenAuth/*                     │
                     └──────────────────────────────────────┘
```

**规则**：
- **迁移过渡期** — Spring Boot 复用 ABP 风格路径 `/api/services/app/{ServiceName}/{MethodName}`，前端不改代码
- **迁移完成后** — 逐步增加 RESTful 风格 `GET/POST /api/v2/{resources}` 别名，新旧共存
- **最终状态** — 前端逐步过渡到 RESTful 路径后，废弃 ABP 风格路径

### 1.3 路径格式约束

```text
# ABP 风格路径
/api/services/app/{ServiceName}/{Action}
  └── ServiceName = PascalCase 服务名（如 Announce, Account, Product）
  └── Action = PascalCase 方法名（如 GetAll, Create, Update）

# 传统 Controller 路径
/api/{Controller}/{Action}
  └── Controller = PascalCase 控制器名（如 TokenAuth, QrCodeAuth）

# 普通 GET 传参
/api/services/app/Announce/GetAll?MaxResultCount=20&SkipCount=0
```

---

## 二、响应格式（核心兼容点）

### 2.1 ABP AjaxResponse 标准包装

所有响应必须包裹在 `AjaxResponse<T>` 中。**这是最核心的兼容要求**，Flutter AuthInterceptor 和 PC axios 拦截器都依赖它解包。

#### ✅ 成功响应

```json
{
  "__abp": true,
  "success": true,
  "result": { ... },
  "targetUrl": null,
  "unAuthorizedRequest": false,
  "error": null
}
```

**客户端行为**（Flutter `AuthInterceptor.onResponse`）：
- 检测到 `success == true` → 自动解包，提取 `result` 传给业务代码
- `result` 如果是 `Array` → 转换为 `{ items: [...] }` 格式
- 业务代码直接使用 `result` 内容，**不感知包装器**

#### ❌ 错误响应

```json
{
  "success": false,
  "error": {
    "code": 0,
    "message": "用户可见的错误消息",
    "details": "详细调试信息（可选）",
    "validationErrors": [
      {
        "message": "字段级验证错误",
        "members": ["FieldName"]
      }
    ]
  }
}
```

**客户端行为**：
- 检测到 `success == false` → 抛出 `DioException`（Flutter）/ 调用 `ElMessage.error()`（PC）
- message 提取顺序: `error.details` > `error.message` > '请求失败'

#### 2.2 Spring Boot 实现方案

```java
@Data
public class ApiResponse<T> {
    private Boolean __abp = true;
    private Boolean success;
    private T result;
    private String targetUrl;
    private Boolean unAuthorizedRequest;
    private ErrorInfo error;

    public static <T> ApiResponse<T> ok(T result) {
        ApiResponse<T> resp = new ApiResponse<>();
        resp.success = true;
        resp.result = result;
        resp.error = null;
        return resp;
    }

    public static <T> ApiResponse<T> fail(String message, String details, 
                                           List<ValidationError> validationErrors) {
        ApiResponse<T> resp = new ApiResponse<>();
        resp.success = false;
        resp.error = new ErrorInfo(message, details, validationErrors);
        return resp;
    }
}
```

**关键**：使用 `@ControllerAdvice` 全局统一包装所有 Controller 返回值，确保开发人员不可能漏包。

---

## 三、分页格式

### 3.1 请求参数

```json
{
  "sorting": "creationTime desc",
  "skipCount": 0,
  "maxResultCount": 20
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `sorting` | string | 排序字段 + 方向，如 `"creationTime desc"`、`"price asc"` |
| `skipCount` | int | 跳过的记录数 = `(pageIndex - 1) * maxResultCount` |
| `maxResultCount` | int | 每页条数 |

### 3.2 响应格式

```json
{
  "success": true,
  "result": {
    "totalCount": 156,
    "items": [
      { ... },
      { ... }
    ]
  }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `totalCount` | int | 满足条件的总记录数 |
| `items` | array | 当前页数据列表 |

### 3.3 Spring Boot 实现

```java
@Data
public class PagedRequest {
    private String sorting;
    private int skipCount;
    private int maxResultCount = 20;
}

@Data
public class PagedResult<T> {
    private int totalCount;
    private List<T> items;
}
```

---

## 四、认证 & 鉴权

### 4.1 JWT 请求头

所有已认证请求必须携带：

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Abp.Tenantid: 1
Content-Type: application/json
Appname: flutter/pc
AppVersion: 20260224@1.0.0
```

| 请求头 | 必需 | 说明 |
|--------|------|------|
| `Authorization: Bearer {token}` | ✅ 认证后 | JWT Bearer Token |
| `Abp.Tenantid` | ✅ | 租户 ID，目前固定为 `1` |
| `Content-Type` | ✅ | `application/json` |
| `Appname` | ❌ | 客户端标识（flutter / pc / uniapp） |
| `AppVersion` | ❌ | 客户端版本号 |

### 4.2 登录响应格式

**登录成功后**，Flutter AuthInterceptor 会处理 ABP 包装，但 `result` 中的具体字段前端直接使用：

```json
{
  "success": true,
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "encryptedAccessToken": null,
    "expireInSeconds": 86400,
    "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
    "refreshTokenExpireInSeconds": 604800,
    "userId": 12345,
    "shouldResetPassword": false,
    "requiresTwoFactorVerification": false,
    "twoFactorAuthProviders": null
  }
}
```

### 4.3 Token 刷新

```http
POST /api/TokenAuth/RefreshToken
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl..."
}
```

**响应**（注意这里**不带 ABP 包装**，是传统 Controller 的裸返回）：

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "encryptedAccessToken": null,
  "expireInSeconds": 86400
}
```

### 4.4 Token 存储规范（客户端）

| 项 | 存储位置 | Key |
|----|---------|-----|
| accessToken | localStorage / Cookie | `token` |
| refreshToken | localStorage | `refreshToken` |
| expireInSeconds | localStorage | `tokenExpireTime`（存的是 `Date.now() + expireInSeconds * 1000`） |

### 4.5 Spring Boot JWT 规范

```yaml
jwt:
  secret: (与 ABP 相同的密钥)
  issuer: (与 ABP 相同的 issuer)
  access-token-expiration: 86400      # 24小时
  refresh-token-expiration: 604800    # 7天
```

**注意**: 现有前端依赖 JWT token 格式（claims 结构）与 ABP 一致。需要逆向 ABP `TokenAuthController.CreateAccessToken` 确保 claims 结构一致，特别是 `sub`, `nameid`, `role` 等标准 claims。

---

## 五、错误处理规范

### 5.1 错误类型对照表

| ABP 错误类型 | HTTP 状态码 | Spring Boot 等价实现 |
|-------------|------------|-------------------|
| `UserFriendlyException` | 200 (success=false) | `@ResponseStatus(200)` + `ApiResponse.fail()` |
| `ArgumentNullException` → 400 | 400 | `@ResponseStatus(HttpStatus.BAD_REQUEST)` |
| `ValidationException` | 200 (success=false) | 同上，带 validationErrors |
| `AbpAuthorizationException` → 401 | 401 | Spring Security `AccessDeniedException` |
| 未登录 | 401 | `AuthenticationException` |

### 5.2 验证错误格式

ABP 的 model binding 验证错误经过 ABP 包装后会变成如下格式返回给前端：

```json
{
  "success": false,
  "error": {
    "message": "验证失败",
    "details": "请修正以下字段",
    "validationErrors": [
      {
        "message": "'手机号'字段必填",
        "members": ["phone"]
      },
      {
        "message": "'验证码'字段必填",
        "members": ["code"]
      }
    ]
  }
}
```

前端 PC 端会解析 `validationErrors` 并以 HTML 列表形式展示。

### 5.3 Spring Boot 全局异常处理

```java
@RestControllerAdvice
public class ApiExceptionHandler {

    @ExceptionHandler(UserFriendlyException.class)
    @ResponseStatus(HttpStatus.OK)  // 注意: 错误也返回 200
    public ApiResponse<Void> handleUserFriendly(UserFriendlyException e) {
        return ApiResponse.fail(e.getMessage(), null, null);
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    @ResponseStatus(HttpStatus.OK)
    public ApiResponse<Void> handleValidation(MethodArgumentNotValidException e) {
        List<ValidationError> errors = e.getBindingResult().getFieldErrors().stream()
            .map(f -> new ValidationError(f.getDefaultMessage(), List.of(f.getField())))
            .toList();
        return ApiResponse.fail("验证失败", null, errors);
    }

    @ExceptionHandler(Exception.class)
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    public ApiResponse<Void> handleUnknown(Exception e) {
        return ApiResponse.fail("服务器内部错误", e.getMessage(), null);
    }
}
```

---

## 六、序列化规范

### 6.1 命名规则

| 场景 | 规则 | 示例 |
|------|------|------|
| JSON 字段 | **CamelCase** | `accessToken`, `expireInSeconds`, `totalCount` |
| DTO 类名 | PascalCase | `AuthenticateResultModel`, `PagedResultDto` |
| URL 路径段 | PascalCase | `/api/services/app/Announce/GetAll` |
| 枚举值 | PascalCase | `State.Active`, `UserRole.Admin` |

**为什么不用 SnakeCase？**
- ABP 默认用 `CamelCasePropertyNamesContractResolver` 将 C# `AccessToken` → JSON `accessToken`
- 前端所有 `fromJson` 都基于 CamelCase 写法
- 迁移完成前**必须保持 CamelCase**，否则前端大面积重构

### 6.2 Spring Boot 配置

```yaml
spring:
  jackson:
    property-naming-strategy: LOWER_CAMEL_CASE   # 保持与 ABP 一致
    date-format: yyyy-MM-dd HH:mm:ss
    time-zone: Asia/Shanghai
    default-property-inclusion: non_null          # null 字段可选
```

---

## 七、API 完整检查清单

### 必须 100% 一致的项

- [ ] `{ "success": true, "result": ... }` 包装格式
- [ ] `{ "success": false, "error": { "message": "...", "details": "..." } }` 错误格式
- [ ] `{ "totalCount": N, "items": [...] }` 分页响应格式
- [ ] `Authorization: Bearer {token}` 认证头
- [ ] `Abp.Tenantid: 1` 租户头
- [ ] CamelCase 字段命名
- [ ] `POST /api/TokenAuth/RefreshToken` 刷新端点及响应格式
- [ ] `POST /api/services/app/{Service}/{Action}` ABP 风格路由
- [ ] 错误场景返回 HTTP 200 + `success: false`（而非直接 4xx/5xx）
- [ ] 登录响应包含 `accessToken`, `expireInSeconds`, `refreshToken`, `refreshTokenExpireInSeconds`

### 建议调整的项

- [ ] AppService 方法命名从 PascalCase 改为 CamelCase？（需前端配合改 URL）
- [ ] 最终迁移完成后从 ABP 路径过渡到 RESTful 路径

---

## 八、参考代码位置

| 文件 | 用途 |
|------|------|
| `molitao_app/lib/data/api/auth_interceptor.dart` | Flutter 端响应解包 + JWT 注入逻辑 |
| `molitao_app/lib/data/models/list_result.dart` | Flutter 端分页 DTO |
| `pc/src/utils/request.ts` | PC 端 axios 拦截器 + AbpResponse 定义 |
| `pc/src/utils/cookies.ts` | PC 端 token 存储 |
| `backend/src/TtWork.Project.Web.Core/Controllers/TokenAuthController.cs` | 登录/刷新原始实现 |
| `backend/src/TtWork.Project.Web.Core/Models/TokenAuth/AuthenticateResultModel.cs` | 登录响应 DTO |

---

## 九、Spring Boot 代码生成建议

建议使用以下工具自动生成兼容层代码：

```bash
# 从 ABP C# DTO 生成 Java POJO
# - 保持 CamelCase 字段名
# - 生成对应 ApiResponse<T> 包装

# 关键接口类（需先逆向映射）
# - TokenAuthController → AuthController.java
# - 所有 AppService → Service 类
#
# 认证头常量
# - "Authorization" → Bearer {token}
# - "Abp.Tenantid" → "1"
# - "Content-Type" → "application/json"
```

---

## 十、Spring Boot 编码规范

### 10.1 参考实现模块

本规范对应的 Java 代码位于 [`backend-java/`](./backend-java/) 目录，可直接作为新项目的骨架模板。

```
backend-java/
├── pom.xml                          # Spring Boot 3.4 + JDK 21
└── src/main/java/com/molitao/
    ├── MolitaoApplication.java      # 应用入口
    ├── common/
    │   ├── api/                     # 响应包装器（核心兼容层）
    │   │   ├── ApiResponse.java     #   ABP AjaxResponse 包装器
    │   │   ├── ErrorInfo.java       #   错误信息
    │   │   ├── ValidationError.java #   字段级验证错误
    │   │   ├── PagedRequest.java    #   分页请求参数
    │   │   └── PagedResult.java     #   分页响应体
    │   ├── exception/
    │   │   └── UserFriendlyException.java  # 业务异常（→ 200 + success:false）
    │   ├── web/
    │   │   ├── ApiResponseAdvice.java      # 全局自动包装器
    │   │   ├── ApiExceptionHandler.java    # 全局异常处理器
    │   │   └── HealthController.java       # 健康检查端点
    │   └── config/
    │       └── JacksonConfig.java    # CamelCase + 时区 + 日期格式
    └── resources/
        └── application.yml          # 配置文件
```

### 10.2 编码规则

#### ① 返回业务数据 — 直接返回 POJO

```java
@RestController
public class UserController {

    @GetMapping("/api/services/app/User/Get")
    public UserDto get(long id) {
        // 1) 正常返回 POJO
        // 2) ApiResponseAdvice 自动包装为 { success: true, result: {...} }
        return userService.get(id);
    }
}
```

**禁止**手动 `return ApiResponse.ok(...)` — 由 `ApiResponseAdvice` 统一处理。

#### ② 返回业务错误 — 抛 UserFriendlyException

```java
if (user == null) {
    throw new UserFriendlyException("用户不存在或已注销");
    // → HTTP 200 + { success: false, error: { message: "用户不存在或已注销" } }
}
```

#### ③ 参数校验 — 用 @Valid

```java
@PostMapping("/api/services/app/User/Create")
public UserDto create(@Valid @RequestBody CreateUserDto dto) {
    return userService.create(dto);
}
```

验证失败自动 → HTTP 200 + `{ success: false, error: { validationErrors: [...] } }`

#### ④ 分页查询 — 用 PagedRequest / PagedResult

```java
@GetMapping("/api/services/app/User/GetAll")
public PagedResult<UserDto> getAll(PagedRequest request) {
    // 内部用 request.getSkipCount(), request.getMaxResultCount()
    // 返回 PagedResult → 自动包装为 { success: true, result: { totalCount, items } }
    return userService.list(request);
}
```

#### ⑤ 文件下载 — 用 ResponseEntity&lt;Resource&gt;

```java
@GetMapping("/api/file/download")
public ResponseEntity<Resource> download(long id) {
    Resource file = fileService.getFile(id);
    return ResponseEntity.ok()
            .contentType(MediaType.APPLICATION_OCTET_STREAM)
            .body(file);
    // ApiResponseAdvice 检测到 Resource 类型 → 跳过包装
}
```

### 10.3 禁止行为

| ❌ | 原因 |
|----|------|
| `return ApiResponse.ok(data)` | ApiResponseAdvice 会再包一层 → `{ success: true, result: { success, result } }` |
| `throw new RuntimeException(msg)` | 兜底走 HTTP 500，前端不会解析为业务错误 |
| `@JsonInclude(NON_NULL)` 加在单个 DTO | 全局已在 JacksonConfig 生效 |
| 接口路径用 snake-case | 前端请求的是 `/api/services/app/User/GetAll` |

### 10.4 迁移一个模块的标准步骤

```
Step 1: 在 backend-java/ 下创建 Service + Controller + DTO
Step 2: 对接数据库（MyBatis-Plus / JPA）
Step 3: 手动测试确认 JSON 输出与 ABP 完全一致
Step 4: 前端改 base URL 指向新服务，验证功能
Step 5: 确认无误后，在网关/负载均衡切流量
```

---

> **总结**: API 兼容的关键在于**格式（契约）而非实现**。只要 Spring Boot 输出的 JSON 结构与 ABP 完全一致，前端改一个 base URL 就能完成切换。所有资源投入应该集中在：① 全局响应包装器② 错误处理③ JWT token 体系④ 分页格式 — 这四件事做好了，迁移就成功了一半。
