# Backend 模块 AI 指令扩展

## 技术栈
- .NET 7/8
- ABP Framework
- Entity Framework Core
- SQL Server/MySQL
- SignalR
- AutoMapper

## .NET/C# 开发规范
- 遵循 C# 10 编码规范
- 使用 ABP Framework 模式和约定
- 优先使用依赖注入
- 使用 async/await 异步编程模式
- 类名使用 PascalCase，方法名使用 PascalCase
- 私有字段使用 _camelCase
- 常量使用 PascalCase

## 数据库操作
- 使用 ABP 的仓储模式 (IRepository)
- 迁移文件命名规范: YYYYMMDD_Description
- 禁止硬编码 SQL 字符串，使用 LINQ 或参数化查询
- 使用 UnitOfWork 管理事务
- 实体类继承 AuditedAggregateRoot 或 FullAuditedAggregateRoot

## API 设计
- 遵循 RESTful 原则
- 统一使用 Application Service 层处理业务逻辑
- 统一返回格式: AjaxResponse 或包装类
- API 版本控制通过路由或特性
- 使用 DTO 进行数据传输，AutoMapper 进行对象映射

## 项目结构
```
TtWork.Project/
├── Core/                 # 核心业务逻辑
│   ├── Entities/         # 实体
│   ├── Repositories/     # 仓储接口
│   └── Services/         # 领域服务
├── Application/          # 应用服务层
│   ├── Services/         # 应用服务
│   ├── DTOs/            # 数据传输对象
│   └── AutoMapper/      # 映射配置
├── EntityFrameworkCore/ # 数据访问层
└── Web.Host/           # Web API 主机
```

## 权限和认证
- 使用 ABP 的权限系统
- 定义权限常量: AppPermissions
- 使用 [Authorize] 特性保护接口
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