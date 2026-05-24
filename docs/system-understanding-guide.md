# 魔力淘后端系统理解指南

> 基于对代码库的全面分析，整理这个项目特有的约定、模式和陷阱。
> 目标：让你在 AI 辅助维护时，知道哪些东西是"这个项目特有的"，避免误判。

---

## 一、路由体系——两条路径并存

这个项目存在**两套完全不同的路由机制**，新增接口时先确认走哪套。

### 1.1 ABP 动态路由（标准路径）

```
/api/services/app/{ServiceName}/{MethodName}
```

**特点**：
- 继承 `ApplicationService` 即可自动生成路由
- **不需要** `[Route]` / `[HttpGet]` 等属性
- 方法名即 URL 路径段（`GetAllAsync` → `GetAll`，`CreateAsync` → `Create`）

**代表文件**：`AuctionItemAppService.cs`、`UserAppService.cs`、`AppFeatureSwitchAppService.cs`

### 1.2 传统路由（显式路径）

```
/api/{module}/{action}
```

**特点**：
- 需要 `[Route("api/xxx")]` 和 `[HttpGet("yyy")]` 属性
- 通常继承 `AbpController` 而不是 `ApplicationService`
- 路径更短、更 RESTful

**代表文件**：`PostService.cs`、`ContentSecurityAppService.cs`、`PayNotifyAppService.cs`、`MsgConfigurationService.cs`

### 1.3 ⚠️ 关键陷阱

| 文件 | 混淆点 |
|------|--------|
| `ContentSecurityAppService.cs` | 名字带 `AppService`，实际继承 `AbpController`（非 `ApplicationService`），用 `[Route("api/ContentSecurity")]` |
| `PayNotifyAppService.cs` | 同上，名字带 `AppService` 但继承 `AbpController`，路径 `/api/PayNotify` |
| `PostService.cs` | 标准 `AbpController`，路径 `/api/Post` |

> **经验法则**：搜 `/api/services/app/` 找不到的接口，去搜 `[Route]` 属性。

---

## 二、双 ORM 模式——两条腿走路

这是项目最核心的架构特征，理解这个才能理解数据层。

### 2.1 EF Core（ABP 标准）

```
IRepository<TEntity, TKey> → 自动 CRUD → LINQ
```

- ABP 默认 ORM，所有实体都有对应的 EF Core 配置
- 审计字段自动填充（CreationTime, CreatorId...）
- 软删除自动过滤（`IsDeleted == false`）
- 适合**增删改 + 简单查询**

### 2.2 SqlSugar（自定义增强）

```
ISqlSugarClient → 原生 SQL / Queryable → 复杂查询
```

- 单例注册（`SqlSugarSetup.cs:69`），全局只有一个实例
- 适合**复杂分页、统计、多表 JOIN**
- 没有审计字段自动填充，没有软删除过滤器

### 2.3 实际使用模式

```
同一服务里 "EF Core 写 + SqlSugar 读" 是常态
```

**典型例子**：
```csharp
// AuctionItemAppService.cs
var entity = await Repository.GetAsync(id);       // EF Core 查单条
var list = await _sqlSugar.Queryable<Xxx>().ToListAsync();  // SqlSugar 查列表
await Repository.UpdateAsync(entity);              // EF Core 写回
```

**涉及的服务**：`UserAppService`、`AuctionItemAppService`、`ClientAppService`、`MessageAppService`、`MsgConfigurationService` 等约 17 个主要服务。

### 2.4 ⚠️ 关键陷阱

1. **事务边界**：EF Core 的 UOW 和 SqlSugar 的 `Ado.BeginTran()` 是独立的。一个事务里混用两个 ORM 需要手动协调
2. **缓存一致性**：EF Core 有变更追踪，SqlSugar 没有。同一个实体在两种方式下可能读到不同状态
3. **`[NotMapped]` 实体**：如 `Message.cs` 的 `Id` 属性标记了 `[NotMapped]`，说明此实体用了 SqlSugar 的映射方式而非 EF Core

> **经验法则**：看到 `IRepository<T>` → EF Core；看到 `ISqlSugarClient` → SqlSugar；同时看到 → 一个写一个读。

---

## 三、事件总线——MediatR 是主力，IEventBus 是遗留

### 3.1 MediatR（42 处引用，活跃使用）

```
// 发布
await _mediator.Publish(new NotificationCommand(...));

// 处理 - 通过 INotificationHandler<T>
public class AuctionItemCacheEventHandler 
    : INotificationHandler<AuctionCacheClearedEvent>,
      INotificationHandler<AuctionItemBidSuccessEvent>,
      ... 共 10 个 handler
```

**主要事件定义文件**：
- `AuctionItemCacheEvents.cs` — 拍卖缓存相关事件（10 个 handler 集中处理）
- `NotificationCommand.cs` — 通用通知事件
- `MessageSendCommand.cs` — 消息发送事件
- `MyCountCacheClear.cs` — 计数缓存清理

### 3.2 ABP IEventBus（10 处引用，遗留）

```csharp
private readonly IEventBus _eventBus;  // MessageSendingService.cs:44
```

少量使用，主要在 `MessageSendingService` 中，建议新代码统一用 MediatR。

> **经验法则**：加新事件用 `IMediator.Publish` + `INotificationHandler`，不要碰 `IEventBus`。

---

## 四、缓存体系——三层结构

### 4.1 自定义 RedisClient（底层）

```
IRedisClient → StackExchange.Redis → 原生 Redis 操作
```

- 单例注册（`Startup.cs:100`）
- 用于需要精确控制 Redis 操作的场景

### 4.2 自定义 IDistributedCache（上层封装）

```
IDistributedCache → RedisDistributedCache → IRedisClient
```

- 单例注册（`Startup.cs:92`）
- 默认过期时间 5 分钟
- **异常时静默返回 null**（不会抛异常导致请求失败）

### 4.3 ABP ICacheManager（ABP 标准缓存）

- ABP 框架自带的缓存抽象
- 项目中大量使用（`IDistributedCache` 和 `ICacheManager` 并存）

### 4.4 ⚠️ 关键陷阱

- `HybridCache` 被临时禁用（`Startup.cs:95-97` 注释），原因是 keyed services 兼容性问题
- 缓存键使用格式化字符串：`AppConsts.MyCount = "MyCount-{0}"`，搜索时注意

---

## 五、审计字段——深度不一致

不同实体继承不同基类，审计字段覆盖范围不一样：

| 基类 | 字段 | 实体举例 |
|------|------|----------|
| `FullAuditedAggregateRoot<long>` | 7 个字段：创建/修改/删除时间 + 操作人 + 软删除 | `AuctionItem`、`PayOrder`、`UserBalanceLog` |
| `CreationAuditedEntity<long>` | 2 个字段：创建时间 + 创建人 | `BidHistory`、`ChatGroup`、`AuctionStartNotify` |
| `Entity<long>` | 无审计字段 | `Message`、`UserFriend` |

> **经验法则**：`Message` 和 `UserFriend` 没有审计字段，查询时也没有软删除过滤（没有 `IsDeleted`），需要自己判断数据有效性。

---

## 六、权限体系——简化版

权限定义在 `AppPermissions` 常量类中，但 `ProjectNameAuthorizationProvider.cs` 几乎是空的。

实际使用方式：
```csharp
// 直接在 AppService 构造函数中赋值
base.CreatePermissionName = AppPermissions.Pages.ChatManager;
base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
base.DeletePermissionName = AppPermissions.Pages.Admin;
```

权限粒度较粗，主要两级：`ChatManager` 和 `Administration`。

> **经验法则**：新增接口不需要关心权限配置——不需要在 AuthorizationProvider 中注册，直接设 `[AbpAuthorize(AppPermissions.Pages.xxx)]` 即可。

---

## 七、后台作业——Hangfire

4 个后台作业，全部需要 `[UnitOfWork]` 属性（因为 Hangfire 执行上下文没有自动 UOW）：

| Job 类 | 周期 | 作用 |
|--------|------|------|
| `CleanExpiredPayOrderJob` | 每天 | 清理过期支付订单 |
| `TenPayNotifyJob` | 每 1 分钟 | 处理微信支付回调对账 |
| `UserDepositJob` | 每 10 秒 | 处理保证金充值 |
| `UserBalanceJob` | 每 1 秒 | 处理用户余额变动（最高频） |

注册在 `AbpApplicationModule.cs:78-81`。

> **经验法则**：新增 Job 必须加 `[UnitOfWork]`，否则数据库操作不生效。

---

## 八、支付回调——特殊路由

微信支付回调路径：`POST /api/PayNotify/TenPay/{appName}`

**重要**：`PayNotifyAppService.cs` 虽然名字带 `AppService`，但继承 `AbpController`，走的是传统路由而非 ABP 动态路由。这意味着：
- **默认没有 CSRF 保护**（微信回调是外部请求，不走登录）
- **没有权限检查**（回调 URL 配置在微信商户平台）
- **请求体是 XML 格式**，不是 JSON

> **经验法则**：支付回调相关接口在 `PayNotifyAppService.cs` 中，修改时注意它不走 ABP 约定。

---

## 九、危险代码模式——在项目中大量存在

### 9.1 `Task<dynamic>` 返回值（52 处）

```csharp
public async Task<dynamic> GetList(...)
public async Task<dynamic> Page(...)
```

前端无法从 Swagger 推断返回结构，容易造成前后端不一致。

**重点文件**：
- `PostService.cs` — 帖子列表/详情
- `PostCategoryService.cs` — 分类列表
- `WithdrawalAmountService.cs` — 提现分页
- `ClientAppService.cs` — 支付/统计（6 处）
- `AuctionCacheManagementAppService.cs` — 缓存管理（7 处）

### 9.2 空 `catch` 块（多处分号模式）

```csharp
catch (Exception)
{
}  // 静默吞掉异常，没有日志
```

### 9.3 中文常量名

```csharp
public const decimal 保证金 = 51m;  // C# 允许中文字符命名，但可能引发混淆
```

### 9.4 `#if DEBUG` 条件编译（6 处）

```csharp
#if DEBUG
    public const string wxworkid = "测试群ID";
#else
    public const string wxworkid = "生产群ID";
#endif
```

调试环境和生产环境的配置通过条件编译区分，排查问题时注意。

---

## 十、项目结构速览

```
src/                                                            
├── TtWork.Project.Web.Host/          # 启动项目（Startup.cs, Program.cs）
│   ├── Services/                      # RedisDistributedCache.cs
│   └── Startup.cs                     # 所有 DI 注册、Middleware 配置
├── TtWork.Project.Web.Core/          # Web 层核心（Controllers）
│   └── Controllers/                   # TokenAuthController, QrCodeAuthController
├── TtWork.Project/                   # 应用层 + 领域层（主要工作区）
│   ├── AbpApplicationModule.cs       # 模块入口（DI 注册、Hangfire、事件总线）
│   ├── AppConsts.cs                  # 全局常量（缓存键、微信配置）
│   ├── Applications/                  # AppService（业务逻辑）
│   │   ├── Auctions/                 # 拍卖（AuctionItemAppService.cs 核心）
│   │   ├── Users/                    # 用户
│   │   ├── Messaging/               # 消息
│   │   ├── Client/                   # 客户端通信
│   │   └── ...                       # 其他模块
│   ├── Domains/                      # 领域实体
│   │   ├── AuctionItem.cs           # 拍卖品 + DTO（DTO 写在实体文件内）
│   │   ├── Message.cs               # 消息（无审计字段）
│   │   ├── Pays/                    # 支付相关实体
│   │   └── ...
│   ├── Jobs/                         # Hangfire 后台作业
│   ├── PostBar/                      # 帖子论坛（独立模块，继承 AbpController）
│   ├── Controllers/                  # 传统 Controller（不走 ABP 动态路由）
│   └── Services/                     # 自定义服务（消息发送、好友管理）
├── TtWork.Project.Core/             # 核心模块（权限定义、用户管理等）
│   └── ProjectNameAuthorizationProvider.cs  # 权限提供者（几乎空实现）
├── TtWork.Project.EntityFrameworkCore/  # EF Core 数据层
│   └── EntityFrameworkCore/
│       ├── AbpDbContext.cs           # DbContext（Fluent API 配置）
│       └── Migrations/               # 数据库迁移（YYYYMMDD_HHMMSS_desc 格式）
└── TtWork.Project.Migrator/         # 数据库迁移启动器
```

**共享模块**（不在 src 内，在 Modules/ 下）：
```
Modules/
├── TtWork.Abp.Core/                 # 共享基础设施
│   ├── AbpAsyncCrudAppService.cs     # 自定义 CRUD 基类
│   ├── Authorization/Users/          # 用户授权管理器
│   └── SqlSugar/SqlSugarSetup.cs     # SqlSugar 注册配置
├── TtWork.Lib/                      # 工具库
│   ├── Redis/                       # RedisClient.cs
│   ├── WeixinPay/                   # 微信支付 SDK 封装
│   └── ...
```

---

## 快速参考卡

| 你要做什么 | 参考文件 | 注意 |
|-----------|---------|------|
| 新加一个 CRUD 接口 | 参考 `AuctionItemAppService.cs` | 继承 `AsyncCrudAppService` + DTO 加 `[AutoMapFrom]` |
| 新加一个复杂查询接口 | 参考 `PostService.cs` | 继承 `AbpController` + 注入 `ISqlSugarClient` |
| 修改支付流程 | `ClientAppService.cs` + `PayNotifyAppService.cs` | 留意回调路径是传统路由 |
| 修改聊天功能 | `MessageAppService.cs` + `MessageSendingService.cs` | Message 实体无审计字段 |
| 修改缓存逻辑 | `RedisDistributedCache.cs` + `AppConsts.cs` | 缓存键在常量中定义 |
| 加后台定时任务 | `Jobs/xxxJob.cs` + `AbpApplicationModule.cs` | 必须加 `[UnitOfWork]` |
| 发布事件 | `IMediator.Publish` | 不要用 `IEventBus` |
| 加权限控制 | `AppPermissions` 常量 + `[AbpAuthorize]` | 不需要改 `AuthorizationProvider` |

---

> **最后更新**：2026-05-24
> **适用范围**：backend/ 目录下的 .NET 8 + ABP Framework v9 代码库
> **与 AI 配合建议**：把这篇文章丢给 AI，然后告诉它"按这个指南来"——AI 能更好地理解项目特殊约定
