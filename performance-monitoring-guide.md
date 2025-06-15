# 魔力淘 - 简单应用性能监控方案

## 🚀 已实施的监控功能

### 1. 请求性能监控中间件
- **功能**: 自动记录所有API请求的响应时间、状态码、用户信息
- **告警**: 自动标记超过3秒的慢请求
- **日志**: 集成到现有Serilog系统

### 2. 系统健康检查
- **数据库健康检查**: 验证MySQL连接和查询
- **Redis健康检查**: 验证Redis连接和读写操作
- **访问地址**: `GET /api/monitor/health`

### 3. 性能统计服务
- **API调用统计**: 自动收集每个API的调用次数、响应时间、错误率
- **系统资源监控**: 内存使用、CPU时间、线程数等
- **定时报告**: 每5分钟输出性能统计到日志

### 4. 监控API接口

#### 获取系统性能统计
```http
GET /api/monitor/performance
```
**返回数据**:
```json
{
  "System": {
    "WorkingSet": 128.5,      // 工作集内存 (MB)
    "PrivateMemory": 95.2,    // 私有内存 (MB)
    "ThreadCount": 23,        // 线程数
    "Uptime": "2.05:30:15"    // 运行时间
  },
  "GC": {
    "Generation0": 145,       // GC代数统计
    "TotalMemory": 45.8       // 托管内存 (MB)
  },
  "ApiStatistics": {
    "api_GET /api/users": {
      "TotalCalls": 1250,
      "ErrorRate": 2.4,
      "AvgResponseTime": 245.5,
      "MaxResponseTime": 1800,
      "CallsPerMinute": 12.5
    }
  }
}
```

#### 获取慢请求列表
```http
GET /api/monitor/slow-requests
```

#### 获取错误统计
```http
GET /api/monitor/errors
```

## 📊 使用Seq日志平台查看监控数据

### 访问地址
- **开发环境**: http://localhost:5341
- **生产环境**: http://172.24.38.98:5341

### 有用的Seq查询语句

#### 查看所有API请求
```sql
@mt = 'API请求完成'
```

#### 查看慢请求 (>1秒)
```sql
@mt = 'API请求完成' and ElapsedMs > 1000
```

#### 查看错误请求
```sql
@mt = 'API请求完成' and StatusCode >= 400
```

#### 查看特定API的性能
```sql
@mt = 'API请求完成' and Path like '%/api/auction%'
```

#### 查看用户行为分析
```sql
@mt = 'API请求完成' and UserId is not null
| group by UserId
| select count() as CallCount, avg(ElapsedMs) as AvgTime
```

## 🔍 性能分析建议

### 关键指标监控
1. **响应时间**: 关注平均响应时间 > 500ms 的API
2. **错误率**: 关注错误率 > 5% 的API
3. **并发量**: 关注高频调用的API
4. **资源使用**: 关注内存使用 > 512MB 的情况

### 瓶颈识别
1. **数据库瓶颈**: 查看SQL执行日志，关注慢SQL
2. **缓存效率**: 监控Redis连接数和响应时间
3. **外部依赖**: 关注微信API、支付API的调用时间
4. **业务逻辑**: 关注拍卖、支付等核心业务的性能

## 🚨 告警配置建议

### Seq告警规则
1. **慢请求告警**: `ElapsedMs > 3000`
2. **错误率告警**: `StatusCode >= 500`
3. **高内存使用**: `WorkingSet > 512`
4. **数据库连接失败**: `@mt = '数据库健康检查失败'`

## 📈 性能优化建议

基于监控数据，可以进行以下优化：

1. **数据库优化**
   - 为慢查询添加索引
   - 实施查询缓存
   - 考虑读写分离

2. **缓存策略**
   - 为高频API添加Redis缓存
   - 优化缓存过期策略
   - 实施缓存预热

3. **代码优化**
   - 异步化长时间操作
   - 优化算法复杂度
   - 减少不必要的数据查询

4. **架构优化**
   - 实施API限流
   - 添加负载均衡
   - 考虑微服务拆分

## 🔧 部署说明

1. **编译项目**: 确保所有新增文件被包含在项目中
2. **更新配置**: 无需额外配置，使用现有Serilog设置
3. **重启应用**: 重启后监控功能自动生效
4. **验证监控**: 访问 `/api/monitor/health` 确认功能正常

## 💡 扩展建议

后续可以考虑的增强功能：
1. **APM集成**: 如Application Insights、Elastic APM
2. **可视化面板**: 如Grafana、Kibana
3. **实时告警**: 如邮件、短信、钉钉通知
4. **分布式追踪**: 如Jaeger、Zipkin
