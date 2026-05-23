# 技术债与优化机会记录

> 本文档记录在项目梳理分析过程中发现的代码问题、技术债和优化机会，
> 按严重程度分级。供后续里程碑决策参考。

---

## 分级说明

| 级别 | 标签 | 说明 |
|------|------|------|
| 🔴 **Critical** | `critical` | 影响系统稳定性或安全，需要尽快修复 |
| 🟠 **High** | `high` | 影响开发效率或代码质量，建议修复 |
| 🟡 **Medium** | `medium` | 代码异味或非最优实践，可择机修复 |
| 🔵 **Low** | `low` | 风格/文档问题，非必须修复 |

---

## 🔴 Critical

### C-01: 微信 AppSecret 硬编码（CVE-level 风险） ✅ 已修复

- **文件**: `UserAppService.cs:54-55` → 已迁移至 `appsettings.json`
- **问题**: 微信小程序 AppId `wx8178f2258942133d` 和 AppSecret `ec39ddccf124f18474738f15cb57a38e` 直接硬编码在代码中
- **影响**: 极敏感凭据泄露，任何可读源码的人都能获取微信小程序权限
- **解决方案**: 创建 `WechatSettings` 配置类，`UserAppService` + `ContentSecurityAppService` 均注入 `IOptions<WechatSettings>` 替代静态字段，密钥通过 `appsettings.json` 配置管理
- **提交**: `d373524`

### C-02: `UserAppService` 使用 `new HttpClient()` 创建实例 ✅ 已修复

- **文件**: `UserAppService.cs` → 已改为 `IHttpClientFactory`
- **问题**: 内部使用 `System.Net.Http.HttpClient _httpClient` 作为字段，构造函数中 `new HttpClient()` 注入
- **影响**: Socket 资源泄漏风险
- **解决方案**: `UserAppService` + `ContentSecurityAppService` 均改为注入 `IHttpClientFactory`，每次请求通过 `CreateClient()` 创建（由框架管理生命周期）
- **提交**: `d373524`

---

## 🟠 High

### H-01: `UserAppService` 严重膨胀（~762行）

- **文件**: `UserAppService.cs`
- **问题**: 包含了用户 CRUD、微信登录、头像上传、密码管理、批量查询等多种职责
- **影响**: 违反单一职责原则，可读性和可维护性下降
- **建议**: 拆分为 `AuthAppService`、`ProfileAppService`、`WeChatLoginAppService`

### H-02: 微信登录 API 返回大量无用字段

- **文件**: `UserAppService.cs` 的 `GetWeChatUserInfoByCode` 等方法
- **问题**: 查询使用 `AccountUserDto` 或 `UserEditDto`，包含 jwtClaim/typePermission 等内部字段
- **影响**: 前端收到大量无用数据，API 文档膨胀
- **建议**: 为移动端创建专用的精简 DTO

### H-03: 缺少统一的分页请求验证

- **影响**: `AppResultRequestDto` 直接作为输入，缺少 `MaxResultCount` 上限校验
- **建议**: 添加全局上限校验防止恶意大分页查询

### H-04: `BidHistory` 重复创建（AuctionItemAppService.cs）

- **文件**: `AuctionItemAppService.cs` 的 `Bid` 方法
- **问题**: 每次出价创建自己的 `BidHistory`，可用 `IRepository<BidHistory>` 替代手动 `new`
- **影响**: 隐藏的业务逻辑，与 ABP 模式不一致

### H-05: 代码文件中包含大量调试日志和注释

- **分布**: 全线代码（`AuctionItemAppService.cs` 等）
- **问题**: 大量 `_logger.LogInformation` 调试日志，部分包含了业务逻辑调试信息
- **影响**: 生产环境日志噪声大，性能开销
- **建议**: 按级别清理（Information 日志应只记录有意义的状态变更）

### H-06: 数据库缺少统一的时间索引策略

- **问题**: 只有 `SmsVerificationCodes` 最近添加了复合索引，其他按时间排序的查询（`Message`、`AuctionItem`、`PayOrder`）依赖全表扫描
- **建议**: 对 `CreationTime` 字段建立索引（在线迁移）

### H-07: `UserAppService` 注入大量依赖（18 个）

- **文件**: `UserAppService.cs`
- **问题**: 构造函数注入 18 个服务，是典型的"万能服务"反模式
- **影响**: 难以测试，任何修改都要看整个构造函数
- **建议**: 拆分服务

---

## 🟡 Medium

### M-01: FreeIM 配置硬编码了 Redis 密码

- **文件**: `Startup.cs:294-306`
- **问题**: Redis 连接串直接写在代码中（含密码 `7yD3Ddd34`）
- **影响**: 敏感信息泄露
- **建议**: 移入配置或环境变量

### M-02: `BidEligibilityService` 的 Redis key 硬编码

- **检查发现**: `BidEligibilityService` 中使用硬编码字符串构造 Redis key
- **建议**: 抽取为常量

### M-03: 多平台条件编译的环境配置不一致

- **文件**: `Startup.cs:298-305`
- **问题**: FreeIM Redis 配置使用 `#if DEBUG` 区分环境
- **建议**: 统一使用 appsettings 配置文件

### M-04: `CleanExpiredPayOrderJob` 使用 `DateTime.Now`（已代码审查确认）

- **文件**: `CleanExpiredPayOrderJob.cs`
- **问题**: 使用了 `DateTime.Now` 进行比较（项目时间约定要求全部使用 DateTime.Now，所以此项不视为 bug）
- **状态**: 已确认符合项目约定

### M-05: 极光推送 Key 和 Secret 仅在配置层存在

- **文件**: `appsettings.json` 的 `JPush` 段
- **问题**: 依赖配置注入，如被误删则推送功能失效
- **建议**: 添加配置验证和缺失告警

---

## 🔵 Low

### L-01: 部分 Service 使用 `new Random()` 而非 `Random.Shared`

- **分布**: `SmsVerificationCodeService.cs`（已修复）、其他相关服务
- **状态**: SMS 服务已修复

### L-02: 部分时间字段使用 `DateTime.Now` 而非 `DateTime.UtcNow`

- **状态**: 已按项目约定统一为 `DateTime.Now`（北京时间 +8）
- **注意**: 3 处 Unix 时间戳计算保留使用 `DateTime.UtcNow`
- **优先级**: 已处理完成

### L-03: 软著材料中的微信 AppSecret 硬编码

- **文件**: `软著申请/` 目录下的源代码文档
- **问题**: `UserAppService.cs` 的 AppSecret 硬编码被包含在软著材料中
- **建议**: 清理软著材料中的敏感信息

### L-04: `AuctionItemCacheManagerTests.cs` 等测试文件编译错误

- **文件**: 测试项目中的 `AuctionItemCacheManagerTests.cs` 等
- **问题**: 18 个预编译错误，测试类未随 API 变更更新
- **建议**: 移交测试团队修复（非本次里程碑优先）

---

## 摘要

| 级别 | 数量 | 关键项 |
|------|------|--------|
| 🔴 Critical | 2 | AppSecret 硬编码、HttpClient 反模式 |
| 🟠 High | 7 | Service 拆解、DTO 精简、分页校验、索引策略等 |
| 🟡 Medium | 5 | Redis 密码硬编码、配置统一等 |
| 🔵 Low | 4 | Random 修复（已处理）、测试修复等 |
| **合计** | **18** | |

## 里程碑 v1.0 修复状态

| 编号 | 问题 | 级别 | 状态 |
|------|------|------|------|
| L-01 | `new Random()` 修复 | 🔵 Low | ✅ 已修复（`Random.Shared`） |
| L-02 | 时间字段统一 | 🔵 Low | ✅ 已处理（全部 `DateTime.Now`） |
| M-04 | `CleanExpiredPayOrderJob` 时间 | 🟡 Medium | ✅ 已确认符合约定 |
| - | SMS 并发安全 + 官方 SDK | - | ✅ 本里程碑外修复（v1.3.1） |

**遗留待处理（非本里程碑范围）**：
- C-01: AppSecret 移入环境变量 → 建议下个里程碑
- C-02: `UserAppService` HttpClient → 建议下个里程碑
- H-01~H-07: 各项 High 级别问题 → 后续逐步修复
- M-01~M-03, M-05: 中等优先级 → 视需要修复
- L-03, L-04: 低优先级 → 按需处理
