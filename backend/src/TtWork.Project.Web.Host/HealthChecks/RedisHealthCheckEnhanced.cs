using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TtWork.Lib.Redis;
using Microsoft.Extensions.Logging;

namespace TtWork.Project.Web.Host.HealthChecks
{
    /// <summary>
    /// 增强的 Redis 健康检查
    /// </summary>
    public class RedisHealthCheckEnhanced : IHealthCheck
    {
        private readonly IRedisClient _redisClient;
        private readonly IRedisPerformanceMonitor _performanceMonitor;
        private readonly ILogger<RedisHealthCheckEnhanced> _logger;

        public RedisHealthCheckEnhanced(
            IRedisClient redisClient,
            IRedisPerformanceMonitor performanceMonitor,
            ILogger<RedisHealthCheckEnhanced> logger)
        {
            _redisClient = redisClient;
            _performanceMonitor = performanceMonitor;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 检查基本连接
                if (!_redisClient.ConnectionMultiplexer.IsConnected)
                {
                    return HealthCheckResult.Unhealthy("Redis 未连接");
                }

                // 执行 ping 测试延迟
                var pingStart = DateTime.Now;
                await _redisClient.Database.PingAsync();
                var latencyMs = (DateTime.Now - pingStart).TotalMilliseconds;

                // 获取性能统计
                var stats = await _performanceMonitor.GetPerformanceStatsAsync();

                // 构建健康状态
                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["latency_ms"] = latencyMs,
                    ["outstanding_operations"] = stats.TotalOutstanding,
                    ["connected_clients"] = stats.ConnectedClients,
                    ["used_memory_mb"] = stats.UsedMemory / 1024.0 / 1024.0,
                    ["redis_version"] = stats.RedisVersion ?? "unknown"
                };

                // 根据性能指标判断健康状态
                if (latencyMs > 1000)
                {
                    return HealthCheckResult.Degraded(
                        $"Redis 延迟过高: {latencyMs:F2}ms",
                        data: data);
                }

                if (stats.TotalOutstanding > 1000)
                {
                    return HealthCheckResult.Degraded(
                        $"Redis 未完成操作过多: {stats.TotalOutstanding}",
                        data: data);
                }

                if (stats.ConnectedClients > 1000)
                {
                    return HealthCheckResult.Degraded(
                        $"Redis 连接客户端过多: {stats.ConnectedClients}",
                        data: data);
                }

                return HealthCheckResult.Healthy(
                    $"Redis 运行正常，延迟: {latencyMs:F2}ms",
                    data: data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis 健康检查失败");
                return HealthCheckResult.Unhealthy("Redis 健康检查异常", ex);
            }
        }
    }
}