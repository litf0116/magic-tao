using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TtWork.Lib.Redis
{
    /// <summary>
    /// Redis 性能监控工具
    /// </summary>
    public interface IRedisPerformanceMonitor
    {
        Task<RedisPerformanceStats> GetPerformanceStatsAsync();
        Task LogRedisInfoAsync();
    }

    public class RedisPerformanceStats
    {
        public long TotalOutstanding { get; set; }
        public long OutstandingWrite { get; set; }
        public long OutstandingRead { get; set; }
        public long ServerConnected { get; set; }
        public long ConnectionActive { get; set; }
        public double? LastPingTimeMs { get; set; }
        public string RedisVersion { get; set; }
        public long UsedMemory { get; set; }
        public long MaxMemory { get; set; }
        public int ConnectedClients { get; set; }
    }

    public class RedisPerformanceMonitor : IRedisPerformanceMonitor
    {
        private readonly IRedisClient _redisClient;
        private readonly ILogger<RedisPerformanceMonitor> _logger;

        public RedisPerformanceMonitor(IRedisClient redisClient, ILogger<RedisPerformanceMonitor> logger)
        {
            _redisClient = redisClient;
            _logger = logger;
        }

        public async Task<RedisPerformanceStats> GetPerformanceStatsAsync()
        {
            var stats = new RedisPerformanceStats();
            try
            {
                var db = _redisClient.Database;
                var multiplexer = _redisClient.ConnectionMultiplexer;

                // 获取连接统计
                var counters = multiplexer.GetCounters();
                stats.TotalOutstanding = counters.TotalOutstanding;
                // 注意：旧版本 StackExchange.Redis 没有这些属性
                stats.OutstandingWrite = 0; // counters.OutstandingWrite;
                stats.OutstandingRead = 0; // counters.OutstandingRead;
                stats.ServerConnected = multiplexer.IsConnected ? 1 : 0;
                stats.ConnectionActive = multiplexer.IsConnected ? 1 : 0;

                // 测试延迟
                var pingStart = DateTime.UtcNow;
                await db.PingAsync();
                stats.LastPingTimeMs = (DateTime.UtcNow - pingStart).TotalMilliseconds;

                // 获取服务器信息（简化版本，避免版本兼容性问题）
                try
                {
                    var server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);
                    stats.RedisVersion = "Unknown"; // 需要通过其他方式获取
                    // 其他信息需要通过 info 命令获取，这里暂时设为默认值
                    stats.ConnectedClients = 0;
                    stats.UsedMemory = 0;
                    stats.MaxMemory = 0;
                }
                catch
                {
                    stats.RedisVersion = "Unknown";
                    stats.ConnectedClients = 0;
                    stats.UsedMemory = 0;
                    stats.MaxMemory = 0;
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 Redis 性能统计失败");
                return stats;
            }
        }

        public async Task LogRedisInfoAsync()
        {
            var stats = await GetPerformanceStatsAsync();

            _logger.LogInformation("Redis 性能统计: " +
                "未完成操作={TotalOutstanding}, " +
                "写入操作={OutstandingWrite}, " +
                "读取操作={OutstandingRead}, " +
                "连接状态={ConnectionActive}, " +
                "延迟={LastPingTimeMs:F2}ms, " +
                "版本={RedisVersion}, " +
                "内存使用={UsedMemory/1024/1024:F1}MB, " +
                "连接客户端={ConnectedClients}",
                stats.TotalOutstanding,
                stats.OutstandingWrite,
                stats.OutstandingRead,
                stats.ConnectionActive,
                stats.LastPingTimeMs,
                stats.RedisVersion,
                stats.UsedMemory / 1024.0 / 1024.0,
                stats.ConnectedClients);

            // 性能警告
            if (stats.LastPingTimeMs > 100)
            {
                _logger.LogWarning("Redis 延迟过高: {Latency:F2}ms", stats.LastPingTimeMs);
            }

            if (stats.TotalOutstanding > 100)
            {
                _logger.LogWarning("Redis 未完成操作过多: {Count}", stats.TotalOutstanding);
            }

            if (stats.ConnectedClients > 500)
            {
                _logger.LogWarning("Redis 连接客户端过多: {Count}", stats.ConnectedClients);
            }
        }
    }

}