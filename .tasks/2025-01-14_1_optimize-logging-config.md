# 背景

文件名：2025-01-14_1_optimize-logging-config.md
创建于：2025-01-14 19:15:00
创建者：Claude
主分支：master
任务分支：task/optimize-logging-config_2025-01-14_1
Yolo 模式：Off

# 任务描述

用户反馈后端日志输出过多，本地服务测试时出现 MySQL 连接复用错误。需要优化日志配置以减少日志输出，并解决 MySQL 连接问题。

# 项目概览

这是一个基于 ABP 框架的.NET 8.0 项目，使用 MySQL 数据库和 Redis 缓存。项目包含拍卖系统、聊天系统等功能模块。

⚠️ 警告：永远不要修改此部分 ⚠️
核心 RIPER-5 协议规则：

1. 必须在每个响应的开头声明当前模式
2. 未经明确许可不能在模式之间转换
3. 在 EXECUTE 模式中必须 100%忠实地遵循计划
4. 在 REVIEW 模式中必须标记即使是最小的偏差
5. 必须将分析深度与问题重要性相匹配
   ⚠️ 警告：永远不要修改此部分 ⚠️

# 分析

## 问题 1：日志输出过多

### 当前配置分析：

1. **开发环境配置** (`appsettings.json`)：

   - `Default`: "Debug" - 默认日志级别为 Debug
   - `Microsoft.EntityFrameworkCore`: "Debug" - EF Core 日志级别为 Debug
   - `TtWork`: "Debug" - 应用代码日志级别为 Debug

2. **Serilog 配置** (`Startup.cs`)：

   - 开发环境：最低级别为 Debug，EF Core 为 Debug
   - 生产环境：最低级别为 Information，EF Core 为 Warning

3. **性能监控中间件** (`RequestPerformanceMiddleware.cs`)：

   - 记录所有 API 请求的开始和完成
   - 记录慢请求告警（超过 3 秒）
   - 记录性能统计

4. **性能统计服务** (`PerformanceCounterService.cs`)：
   - 每 5 分钟输出一次性能统计信息

### 问题根源：

- 开发环境下日志级别设置过低（Debug），会产生大量详细日志
- EF Core 的 Debug 级别会输出所有 SQL 查询
- 性能监控中间件会记录所有 API 请求
- 性能统计服务每 5 分钟输出一次统计信息

## 问题 2：MySQL 连接复用错误

### 错误信息：

```
This MySqlConnection is already in use. See https://fl.vu/mysql-conn-reuse
```

### 当前配置分析：

1. **连接字符串配置**：

   - 开发环境：`pooling=true` 已启用连接池
   - 生产环境：未明确配置连接池参数

2. **EF Core 配置** (`AbpDbContextConfigurer.cs`)：

   - 使用 Pomelo.EntityFrameworkCore.MySql
   - 开发环境启用了敏感数据日志记录
   - 未配置连接池大小限制

3. **Hangfire 配置**：
   - 使用相同的数据库连接字符串
   - 可能造成连接竞争

### 问题根源：

- EF Core 和 Hangfire 可能同时使用数据库连接
- 连接池配置不当导致连接复用问题
- 开发环境下的敏感数据日志记录可能影响连接管理

# 提议的解决方案

## 日志优化方案：

1. **调整日志级别**：

   - 开发环境：将 EF Core 日志级别从 Debug 调整为 Warning
   - 应用代码日志级别从 Debug 调整为 Information
   - 保留错误和警告日志

2. **优化性能监控**：

   - 减少性能统计输出频率（从 5 分钟改为 30 分钟）
   - 只在生产环境启用详细的 API 请求日志
   - 开发环境只记录慢请求和错误

3. **Serilog 配置优化**：
   - 开发环境：最低级别调整为 Information
   - 生产环境：保持当前配置

## MySQL 连接优化方案：

1. **连接字符串优化**：

   - 添加连接池大小限制：`MaxPoolSize=100;MinPoolSize=5`
   - 添加连接超时配置：`ConnectionTimeout=30;CommandTimeout=30`
   - 添加连接重置配置：`ConnectionReset=true`

2. **EF Core 配置优化**：

   - 添加连接池配置
   - 优化事务隔离级别
   - 开发环境禁用敏感数据日志记录

3. **Hangfire 配置优化**：
   - 使用独立的连接字符串
   - 调整连接池参数

# 当前执行步骤："2. 优化开发环境配置"

# 任务进度

[2025-01-14 19:15:00]

- 已修改：创建任务文件
- 更改：创建了详细的问题分析和解决方案文档
- 原因：记录当前发现的问题和解决方案
- 阻碍因素：无
- 状态：成功

[2025-01-14 19:25:00]

- 已修改：backend/src/TtWork.Project.Web.Host/appsettings.json
- 更改：
  1. 优化 MySQL 连接字符串，添加连接池和超时配置
  2. 调整日志级别，减少开发环境日志输出
- 原因：解决 MySQL 连接复用错误和减少日志输出
- 阻碍因素：JSON 文件中的注释导致 linter 错误（不影响功能）
- 状态：成功

[2025-01-14 19:30:00]

- 已修改：backend/src/TtWork.Project.Web.Host/appsettings.json
- 更改：移除不支持的 CommandTimeout 和 DefaultCommandTimeout 参数
- 原因：MySqlConnector 不支持这些参数，导致启动失败
- 阻碍因素：无
- 状态：成功

# 最终审查

[待完成]
