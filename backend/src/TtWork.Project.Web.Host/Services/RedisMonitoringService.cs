using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TtWork.Lib.Redis;

namespace TtWork.Project.Web.Host.Services
{
    /// <summary>
    /// Redis 监控后台服务，定期记录 Redis 性能指标
    /// </summary>
    public class RedisMonitoringService : BackgroundService
    {
        private readonly IRedisPerformanceMonitor _performanceMonitor;
        private readonly ILogger<RedisMonitoringService> _logger;
        private readonly RedisOptions _options;
        private readonly TimeSpan _monitoringInterval;

        public RedisMonitoringService(
            IRedisPerformanceMonitor performanceMonitor,
            IOptions<RedisOptions> options,
            ILogger<RedisMonitoringService> logger)
        {
            _performanceMonitor = performanceMonitor;
            _options = options.Value;
            _logger = logger;

            // 每5分钟监控一次
            _monitoringInterval = TimeSpan.FromMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Redis 监控服务已启动，监控间隔: {Interval}分钟",
                _monitoringInterval.TotalMinutes);

            // 启动时先执行一次
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            await _performanceMonitor.LogRedisInfoAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _performanceMonitor.LogRedisInfoAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Redis 监控执行失败");
                }

                await Task.Delay(_monitoringInterval, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Redis 监控服务正在停止...");
            await base.StopAsync(cancellationToken);
        }
    }
}