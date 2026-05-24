# Java 端架构设计与迁移分析

> 基于对现有 .NET ABP 后端的 5 项全面分析（API 清单、实体模型、外部依赖、模块依赖、ABP 模式），为 Java Spring Boot 迁移提供架构设计和目录结构方案。

---

## 一、架构选型确认

### 1.1 技术栈

| 层 | 技术选型 | 说明 |
|----|---------|------|
| 框架 | Spring Boot 3.x | 最新 LTS 版本 |
| ORM | MyBatis-Plus | 手动 SQL 可控性强，与现有 SqlSugar 风格接近 |
| 数据库 | MySQL 8.x | 不变，共用现有数据库 |
| 事务 | `@Transactional` 声明式事务 | 替代 ABP 自动 UOW |
| 权限 | Spring Security + 自定义注解 | 替代 ABP `[AbpAuthorize]` |
| 缓存 | Spring Cache + Redis (Lettuce) | 替代 ABP ICacheManager |
| 映射 | 手动 DTO 转换（自行封装工具类） | 不用 MapStruct，减少编译期依赖 |
| 消息 | Spring ApplicationEventPublisher | 替代 MediatR |
| 定时 | Spring `@Scheduled` + Quartz | 替代 Hangfire |
| 日志 | Logback + 直接写入 Elasticsearch | 替代 Serilog |
| 构建 | Maven（单模块） | 简单项目，无需多模块 |
| 部署 | Docker + docker-compose | 与现有部署方式一致 |

### 1.2 为什么不选 JPA

- JPA 的自动映射不适合迁移场景（表结构已定，差异在字段命名）
- MyBatis-Plus 的代码生成 + 手写 SQL 与现有 SqlSugar 模式一致
- 迁移过程中需要精确控制 SQL（共库操作时尤为关键）

---

## 二、目录结构

### 2.1 顶层结构

```
molitao-java/
├── pom.xml                          # Maven 单模块（后期可拆多模块）
├── src/main/java/com/molitao/
│   ├── MolitaoApplication.java      # Spring Boot 启动类
│   ├── common/                      # 公共基础设施
│   │   ├── config/                  # Spring 配置（Redis、Jackson、CORS...）
│   │   ├── base/                    # 基类（BaseEntity、BaseController...）
│   │   ├── util/                    # 工具类（DTO转换、脱敏、时间...）
│   │   ├── exception/               # 全局异常处理
│   │   └── web/                     # 统一响应格式、拦截器
│   ├── auth/                        # 鉴权模块（独立于业务）
│   │   ├── config/                  # Spring Security 配置
│   │   ├── filter/                  # JWT 过滤器
│   │   ├── controller/              # 登录、注册、Token 刷新
│   │   ├── service/                 # 鉴权逻辑
│   │   └── dto/                     # 登录请求/响应
│   ├── user/                        # 用户模块（P0 - 最先迁移）
│   │   ├── entity/                  # User、UserGroupLevel...
│   │   ├── controller/              # UserController
│   │   ├── service/                 # UserService、UserCacheService
│   │   ├── mapper/                  # MyBatis-Plus Mapper
│   │   └── dto/                     # UserDto、UserDtoBase...
│   ├── cms/                         # CMS/公告模块（高独立性）
│   │   ├── entity/                  # CmsArticle、CmsCategory、Announce
│   │   ├── controller/
│   │   ├── service/
│   │   ├── mapper/
│   │   └── dto/
│   ├── auction/                     # 拍卖模块（P1 - 核心业务）
│   │   ├── entity/                  # AuctionItem、BidHistory...
│   │   ├── controller/
│   │   ├── service/                 # AuctionService、BidEligibilityService
│   │   ├── mapper/
│   │   ├── dto/
│   │   ├── cache/                   # 拍卖缓存管理
│   │   └── event/                   # BidPlacedEvent、AuctionEndedEvent...
│   ├── payment/                     # 支付模块（P2 - 最后迁移）
│   │   ├── entity/                  # PayOrder、UserBalanceLog...
│   │   ├── controller/              # 支付回调 Controller
│   │   ├── service/                 # WechatPayService、PayNotifyService
│   │   ├── mapper/
│   │   └── dto/
│   ├── chat/                        # 聊天/消息模块（与 FreeIM 配合）
│   │   ├── entity/                  # Message、ChatChannel、ChatGroup...
│   │   ├── controller/              # 聊天历史、群组管理
│   │   ├── service/                 # MessageSendingService
│   │   ├── mapper/
│   │   └── dto/
│   └── im/                          # 即时通讯（WebSocket 服务）
│       ├── config/                  # WebSocket 配置
│       ├── handler/                 # 消息处理器
│       ├── service/                 # IMService（类似 ImHelper）
│       └── dto/
├── src/main/resources/
│   ├── application.yml              # 通用配置
│   ├── application-dev.yml          # 开发环境
│   ├── application-prod.yml         # 生产环境
│   └── mapper/                      # MyBatis XML（自动扫包）
└── Dockerfile                       # Docker 构建
```

### 2.2 包设计原则

1. **按模块分包**（package-by-module），不是按层分包
2. 每个模块内部再按 `entity/controller/service/mapper/dto/` 分层
3. `common/` 不属于业务模块，放全局基础设施
4. `auth/` 独立于业务模块，仅负责鉴权
5. `im/` 为独立 WebSocket 服务，可单独部署

---

## 三、API 设计映射

### 3.1 路由风格转换

| .NET (ABP) 风格 | Java (RESTful) | 说明 |
|----------------|---------------|------|
| `/api/services/app/User/GetAll` | `GET /api/user/page` | 分页查询 |
| `/api/services/app/User/Create` | `POST /api/user` | 创建 |
| `/api/services/app/User/Update` | `PUT /api/user` | 修改 |
| `/api/services/app/User/Delete` | `DELETE /api/user/{id}` | 删除 |
| `/api/services/app/AuctionItem/Bid` | `POST /api/auction/{id}/bid` | 资源操作 |
| `/api/services/app/CmsArticle/GetAllPublicAsync` | `GET /api/cms/article/public` | 公开接口 |
| `/api/PayNotify/TenPay/{appName}` | `POST /api/pay/notify/{appName}` | 支付回调 |
| `/api/services/app/Client/PayDeposit` | `POST /api/payment/deposit` | 保证金支付 |

### 3.2 约定规则

- 公开接口不加 `/services/app` 前缀，直接 `/api/{module}/{action}`
- 分页查询统一 `GET /api/{module}/page?page=1&size=10`
- 详情统一 `GET /api/{module}/{id}`
- 创建统一 `POST /api/{module}`
- 修改统一 `PUT /api/{module}`
- 删除统一 `DELETE /api/{module}/{id}`
- 特殊操作用 `POST /api/{module}/{id}/{action}`

### 3.3 统一响应格式

```java
public class ApiResult<T> {
    private int code;         // 0=成功, 非0=错误码
    private String message;   // 错误时返回原因
    private T data;           // 数据（分页时返回 page 对象）
    private long timestamp;   // 时间戳
}

// 分页响应
public class PageResult<T> {
    private List<T> items;
    private long total;
    private int page;
    private int size;
}
```

---

## 四、数据库交互设计

### 4.1 基类设计

```java
// BaseEntity - 所有实体的基类（对标 ABP FullAuditedAggregateRoot）
@Data
public class BaseEntity {
    @TableId
    private Long id;
    private LocalDateTime creationTime;
    private Long creatorId;
    private LocalDateTime lastModificationTime;
    private Long lastModifierId;
    private Boolean isDeleted;      // 软删除标记
    private LocalDateTime deletionTime;
    private Long deleterId;
}

// 自动填充审计字段
@Component
public class AuditMetaObjectHandler implements MetaObjectHandler {
    @Override
    public void insertFill(MetaObject metaObject) {
        this.strictInsertFill(metaObject, "creationTime", LocalDateTime.class, LocalDateTime.now());
        this.strictInsertFill(metaObject, "creatorId", Long.class, getCurrentUserId());
    }
    
    @Override
    public void updateFill(MetaObject metaObject) {
        this.strictUpdateFill(metaObject, "lastModificationTime", LocalDateTime.class, LocalDateTime.now());
        this.strictUpdateFill(metaObject, "lastModifierId", Long.class, getCurrentUserId());
    }
}
```

### 4.2 表名映射

- 直接使用现有表名（`@TableName("T_AuctionItem")`）
- 表名不变，新旧系统共库期间数据无缝切换
- 注意字段命名差异：C# PascalCase → MySQL snake_case（MyBatis-Plus 自动驼峰转换）

### 4.3 Ulid 主键处理

PayOrder、UserBalanceLog、UserDepositLog 使用 `Ulid` 类型：
- Java 端使用 `String` 类型存储（数据库字段 `varchar(26)`）
- 通过自定义 TypeHandler 或应用层生成 Ulid 字符串
- 或引入 `com.github.f4b6a3:ulid-creator` 库

---

## 五、ABP 横切关注点复现方案

| ABP 特性 | 复现方案 | 优先级 | 工作量 |
|---------|---------|--------|-------|
| 审计字段自动填充 | MyBatis-Plus `MetaObjectHandler` | P0 | 半天 |
| UnitOfWork 自动事务 | Spring `@Transactional` + AOP 切面 | P0 | 1天 |
| 多租户过滤器 | MyBatis-Plus 多租户插件 + ThreadLocal | P1 | 1天 |
| `[AbpAuthorize]` | Spring Security + `@PreAuthorize` 自定义注解 | P0 | 1天 |
| MediatR 事件总线 | Spring `ApplicationEventPublisher` | P0 | 半天 |
| 部门缓存/UserCache | Spring Cache `@Cacheable` + Redis | P0 | 半天 |
| 拍卖缓存 (Redis) | Spring Cache + StringRedisTemplate | P0 | 1天 |
| AutoMapper | 手动转换工具类 `BeanUtils` | P1 | 1天（随用随加） |
| 软删除 ISoftDelete | MyBatis-Plus 逻辑删除插件 | P2 | 半天（1个实体用） |
| Hangfire 后台任务 | `@Scheduled` + `@Async` + Quartz | P1 | 1天 |
| Hangfire 授权 | 内网访问限制，简化处理 | P2 | 工作量可忽略 |

---

## 六、外部依赖迁移方案

| 服务 | Java 方案 | 关键点 |
|------|----------|--------|
| Redis | Lettuce（Spring Boot 默认） | 连接池、分布式锁（RLock/RedisTemplate） |
| MySQL | MyBatis-Plus + HikariCP | 连接池配置 |
| 微信支付 V3 | `weixin-pay-java` 或直接 HTTP 调用 | 签名验证、回调解密、证书管理 |
| 微信登录 | `weixin-java-mp` / `weixin-java-miniapp` | OAuth2、code2session |
| JPush | 直接调用 REST API | 与现有方式一致，无 SDK 依赖 |
| 阿里云短信 | `aliyun-java-sdk-dysmsapi` | 官方 SDK |
| 又拍云 OSS | `upyun-java-sdk` | 官方 SDK |
| Elasticsearch | `elasticsearch-java` 官方客户端 | 低级 + 高级客户端 |
| WebPush | `web-push-java` 或直接 HTTP | Vapid 密钥验证 |
| FreeIM | `Spring WebSocket` + Redis pub/sub | 需重写消息路由（**最高迁移成本**） |

### FreeIM 迁移决策

FreeIM 是 **迁移成本最高的模块**，因为它涉及：
1. WebSocket 长连接 (Java 端用 Spring WebSocket / Netty)
2. Redis pub/sub 消息分发
3. 客户端 Token 心跳
4. 在线状态管理

**策略**：
- **第一步**：Spring WebSocket + Redis pub/sub 实现基本通道
- **第二步**：保持与现有 FreeIM 的 Redis 消息格式兼容（新旧共存期间）
- **第三步**：全部切完后，再优化架构（Netty + 自定义协议）

---

## 七、迁移阶段计划

### Phase 0 - 基础设施（1-2 周）

```
搭建 Spring Boot 项目骨架
├── 基础配置 (pom.xml, application.yml, Dockerfile)
├── 统一响应格式 ApiResult
├── BaseEntity + 审计填充
├── Spring Security + JWT 鉴权
├── 全局异常处理
└── Redis/数据库连接配置
```

### Phase 1 - 核心底层模块（2-3 周）

```
新功能直接用 Java 写，不写 .NET
├── 新需求 A
├── 新需求 B
└── 老模块按需迁移
```

### Phase 2 - 老模块逐步搬迁

```
每个模块独立迁移流程：
1. Java 端对照原 C# 代码逐行翻译（不重构业务）
2. 自测通过（MyBatis-Plus 查询结果与 .NET 一致）
3. 部署到灰度环境，Nginx 按路由分流验证
4. 灰度观察 3-7 天
5. 摘掉 .NET 对应路由，保留代码 7 天可回退
```

---

## 八、Nginx 路由分流方案（新旧共存）

```nginx
upstream dotnet_backend {
    server 127.0.0.1:5000;   # .NET ABP 服务
}

upstream java_backend {
    server 127.0.0.1:8080;   # Java Spring Boot 服务
}

server {
    listen 443 ssl;
    server_name www.molitao.top;

    # Java - 已经迁移的接口
    location /api/cms/ {
        proxy_pass http://java_backend;
    }
    
    location /api/user/ {
        proxy_pass http://java_backend;
    }

    # .NET - 尚未迁移的接口（走 ABP 约定路由）
    location /api/services/app/AuctionItem/ {
        proxy_pass http://dotnet_backend;
    }
    
    location /api/services/app/Client/ {
        proxy_pass http://dotnet_backend;
    }

    # 支付回调（新旧都需要处理）
    location /api/PayNotify/ {
        proxy_pass http://dotnet_backend;
    }

    # 兜底 - 未明确路由到的 API 走 .NET
    location /api/ {
        proxy_pass http://dotnet_backend;
    }
}
```

---

## 九、迁移安全检查清单（每个模块）

每个模块切到 Java 之前，必须满足以下条件：

- [ ] Java 接口请求参数解析与 .NET 一致
- [ ] Java 接口响应格式与 .NET 一致
- [ ] 核心业务流程在新系统走通
- [ ] 老系统代码保留（Nginx 可秒切回）
- [ ] 灰度放量观察 ≥3 天，无异常日志
- [ ] 生产环境配置项已迁移（Redis、数据库等连接信息）
