# Backend 模块 AI 指令扩展

## 技术栈
- .NET 8
- ABP Framework v9
- Entity Framework Core + SqlSugar（双 ORM 并存）
- MySQL
- SignalR
- MediatR（事件总线主力）
- Hangfire（后台作业）
- StackExchange.Redis / IDistributedCache

## .NET/C# 开发规范
- 遵循 C# 12 编码规范
- 使用 ABP Framework 模式和约定
- 优先使用依赖注入
- 使用 async/await 异步编程模式
- 类名使用 PascalCase，方法名使用 PascalCase
- 私有字段使用 _camelCase
- 常量使用 PascalCase
- 主构造函数语法（C# 12）：接受，但新增接口需显式加 [Route] 属性，否则 ABP 路由无法识别

## 数据库操作
- **双 ORM 体系**：EF Core 负责写（增删改），SqlSugar 负责读（复杂查询）
  - EF Core：`IRepository<T>` → LINQ，自动审计字段、软删除过滤
  - SqlSugar：`ISqlSugarClient` → Queryable / 原生 SQL，无审计字段无软删除过滤
  - 同一服务里混用两者是**正常模式**（IRepository 写 + SqlSugar 读）
- **双实体同步规则**：EF Core 实体在 `Domains/`，SqlSugar 实体在 `Modules/TtWork.Abp.Entity/`
  - 修改 `Domains/` 下的字段时必须同步检查 `Modules/TtWork.Abp.Entity/` 对应的实体
  - 对照关系：AuctionItem ↔ AuctionItemEntity、User ↔ UserEntity ……
- 迁移文件命名规范: YYYYMMDD_Description
- 禁止硬编码 SQL 字符串，使用 LINQ 或参数化查询
- 使用 UnitOfWork 管理事务
- 实体类继承 AuditedAggregateRoot 或 FullAuditedAggregateRoot

## API 设计
- 两套路由体系并存：
  - **ABP 动态路由**：继承 `ApplicationService`，URL 为 `/api/services/app/{Name}/{Method}`，不需要 `[Route]` 属性
  - **传统路由**：继承 `AbpController`，URL 由 `[Route("api/xxx")]` 定义
- 新增接口判断规则：
  - 标准 CRUD → 继承 `ApplicationService`，放 `Applications/` 子目录
  - 非标准/外部回调/需要精确路由控制 → 继承 `AbpController`，放 `TtWork.Project.Web.Core/Controllers/`
- 统一返回格式: AjaxResponse 或包装类
- 使用 DTO 进行数据传输，AutoMapper 进行对象映射
- **后端 Job 必须加 `[UnitOfWork]`**，Hangfire 执行上下文无自动 UOW

## 项目结构（真实结构）

### 核心项目：TtWork.Project（src/）
```
TtWork.Project/                      # 应用层 + 领域层
├── AbpApplicationModule.cs          # 模块入口（DI 注册、Hangfire、事件总线）
├── AppConsts.cs                     # 全局常量（缓存键、微信配置）
├── Applications/                    # AppService——标准业务逻辑
│   ├── Auctions/                    #   拍卖核心（AuctionItemAppService）
│   ├── Pays/                        #   支付
│   ├── Auth/                        #   认证
│   ├── Cms/ + AdvertisingSpace/     #   内容管理
│   ├── ClientAppService.cs         #   客户端 API（支付/统计）
│   └── ……
├── Domains/                         # 领域实体
│   ├── AuctionItem.cs              # 核心实体
│   ├── Message.cs                  # 消息（无审计字段、无软删除！）
│   ├── BidHistory.cs               # 出价记录
│   ├── Pays/                       # 支付相关实体
│   └── ……
├── Services/                        # 自定义服务
│   ├── Cache/                      #   缓存服务（AuctionItemCacheManager 等）
│   ├── Push/                       #   推送服务（JPush/WebPush）
│   ├── Messaging/                  #   消息发送
│   ├── BidEligibilityService.cs    #   出价资格
│   ├── NotifyService.cs            #   通知服务
│   └── ……
├── Jobs/                            # Hangfire 后台作业
├── PostBar/                         # 帖子论坛模块（独立，继承 AbpAppServiceBase）
├── Controllers/                     # 传统 Controller（不走 ABP 动态路由）
├── Caches/                          # ⚠️ 已废弃目录，新缓存服务放 Services/Cache/
└── Dto/ + Dtos/                     # DTO 定义
```

### 其他项目
```
TtWork.Project.Web.Core/             # Web 层核心
├── Controllers/                     # 通用 Controller（TokenAuth、QrCodeAuth、LocalDev）
└── Services/                        # Web 拦截器/过滤器

TtWork.Project.Web.Host/             # 启动项目
├── Startup.cs                       # 所有 DI 注册、Middleware 配置
└── Controllers/                     # 入口 Controller（Home、AntiForgery、Monitor）

TtWork.Project.EntityFrameworkCore/  # EF Core 数据层配置 + 迁移

Modules/
├── TtWork.Abp.Core/                 # 共享基础设施（AbpAsyncCrudAppService、SqlSugarSetup、用户管理）
├── TtWork.Abp.Entity/               # SqlSugar 实体（与 Domains/ 的双胞胎）
└── TtWork.Lib/                      # 工具库（Redis、微信支付 SDK 封装）
```

## 新文件放置规范

### 新 Controller
| 基类 | 放哪里 | 示例 |
|------|--------|------|
| `ApplicationService` | `Applications/{模块}/` | AuctionItemAppService |
| `AbpController` | `Web.Core/Controllers/` | TokenAuthController |

### 新 Services
- 有归属子目录的 → 放 `Services/{子目录}/`（已有 Push/、Messaging/、Cache/）
- 无归属的 → 放 `Services/` 根目录

### 新 Entities
| ORM | 放哪里 |
|-----|--------|
| EF Core | `Domains/{模块}/` |
| SqlSugar | `Modules/TtWork.Abp.Entity/` |
| 注意：**两张表都要加**，两边同步 |

### 新 Hangfire Job
- 放 `Jobs/`，**必须加 `[UnitOfWork]` 特性**

## 事件总线
- **主力**：MediatR（`IMediator.Publish` + `INotificationHandler`）
- **遗留**：ABP `IEventBus`（10 处，已冻结，新事件不用）

## 缓存体系
- 底层：`IRedisClient`（StackExchange.Redis 封装）
- 上层：`IDistributedCache`（封装异常静默返回 null，默认过期 5 分钟）
- 缓存键在 `AppConsts.cs` 中定义（格式如 `"MyCount-{0}"`）
- `HybridCache` 被临时禁用（keyed services 兼容性问题）

## 权限和认证
- 使用 ABP 的权限系统
- 权限常量在 `AppPermissions` 中定义，AuthorizationProvider 几乎为空
- 新增接口不需要改 AuthorizationProvider，直接 `[AbpAuthorize(AppPermissions.Pages.xxx)]` 即可
- JWT Token 认证

## 日志和异常
- 使用 ABP 的日志系统 (ILogger)
- 自定义业务异常继承 BusinessException
- 使用统一异常处理中间件
- 敏感信息不记录到日志

## 测试规范
- 单元测试使用 xUnit
- 集成测试使用 EF Core InMemory 数据库
- 测试项目命名: *.Tests
- 模拟对象使用 Moq

## 特定约定
- 删除实体使用软删除 (ISoftDelete)
- 审计字段自动处理 (ICreationAudited, IModificationAudited)
- 多租户支持 (IMultiTenant)
- 缓存使用 IDistributedCache
- 后台作业使用 IBackgroundJobManager