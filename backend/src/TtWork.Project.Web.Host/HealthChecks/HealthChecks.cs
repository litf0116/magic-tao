using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SqlSugar;
using TtWork.Lib.Redis;

namespace TtWork.Project.Web.Host.HealthChecks
{
    /// <summary>
    /// 数据库健康检查
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(ISqlSugarClient sqlSugarClient, ILogger<DatabaseHealthCheck> logger)
        {
            _sqlSugarClient = sqlSugarClient;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // 执行简单查询测试数据库连接
                var result = await _sqlSugarClient.Ado.GetScalarAsync("SELECT 1");

                if (result != null)
                {
                    return HealthCheckResult.Healthy("数据库连接正常");
                }

                return HealthCheckResult.Unhealthy("数据库查询返回空结果");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库健康检查失败");
                return HealthCheckResult.Unhealthy($"数据库连接失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Redis健康检查
    /// </summary>
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IRedisClient _redisClient;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(IRedisClient redisClient, ILogger<RedisHealthCheck> logger)
        {
            _redisClient = redisClient;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // 测试Redis连接
                var testKey = $"health_check_{DateTime.Now:yyyyMMddHHmmss}";
                await _redisClient.Database.StringSetAsync(testKey, "test", TimeSpan.FromSeconds(10));
                var result = await _redisClient.Database.StringGetAsync(testKey);
                await _redisClient.Database.KeyDeleteAsync(testKey);

                if (result.HasValue)
                {
                    return HealthCheckResult.Healthy("Redis连接正常");
                }

                return HealthCheckResult.Unhealthy("Redis读写测试失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis健康检查失败");
                return HealthCheckResult.Unhealthy($"Redis连接失败: {ex.Message}");
            }
        }
    }
}
