using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TtWork.Project.Web.Host.Services
{
    /// <summary>
    /// 简单的性能统计服务
    /// </summary>
    public class PerformanceCounterService : BackgroundService
    {
        private readonly ILogger<PerformanceCounterService> _logger;
        private static readonly ConcurrentDictionary<string, PerformanceCounter> _counters = new();

        public PerformanceCounterService(ILogger<PerformanceCounterService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录API调用
        /// </summary>
        public static void RecordApiCall(string endpoint, long responseTimeMs, int statusCode)
        {
            var key = $"api_{endpoint}";
            _counters.AddOrUpdate(key,
                new PerformanceCounter { Endpoint = endpoint },
                (k, counter) =>
                {
                    counter.TotalCalls++;
                    counter.TotalResponseTime += responseTimeMs;
                    counter.LastCallTime = DateTime.Now;

                    if (statusCode >= 400)
                        counter.ErrorCount++;

                    if (responseTimeMs > counter.MaxResponseTime)
                        counter.MaxResponseTime = responseTimeMs;

                    if (responseTimeMs < counter.MinResponseTime || counter.MinResponseTime == 0)
                        counter.MinResponseTime = responseTimeMs;

                    return counter;
                });
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        public static Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();
            var now = DateTime.Now;

            foreach (var kvp in _counters.ToList())
            {
                var counter = kvp.Value;
                if (counter.TotalCalls > 0)
                {
                    stats[kvp.Key] = new
                    {
                        Endpoint = counter.Endpoint,
                        TotalCalls = counter.TotalCalls,
                        ErrorCount = counter.ErrorCount,
                        ErrorRate = Math.Round((double)counter.ErrorCount / counter.TotalCalls * 100, 2),
                        AvgResponseTime = Math.Round((double)counter.TotalResponseTime / counter.TotalCalls, 2),
                        MaxResponseTime = counter.MaxResponseTime,
                        MinResponseTime = counter.MinResponseTime,
                        LastCallTime = counter.LastCallTime,
                        CallsPerMinute = GetCallsPerMinute(counter, now)
                    };
                }
            }

            return stats;
        }

        private static double GetCallsPerMinute(PerformanceCounter counter, DateTime now)
        {
            if (counter.LastCallTime == default) return 0;

            var timeDiff = now - counter.LastCallTime;
            if (timeDiff.TotalMinutes < 1) return counter.TotalCalls;

            return Math.Round(counter.TotalCalls / timeDiff.TotalMinutes, 2);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 每5分钟输出一次性能统计
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                    var stats = GetStatistics();
                    if (stats.Any())
                    {
                        Log.Information("=== 应用性能统计 ===");
                        foreach (var stat in stats.Take(10)) // 只显示前10个最频繁的API
                        {
                            Log.Information("API性能: {@ApiStats}", stat.Value);
                        }

                        // 清理旧数据 (保留最近1小时的数据)
                        CleanOldData();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "性能统计服务异常");
                }
            }
        }

        private void CleanOldData()
        {
            var cutoffTime = DateTime.Now.AddHours(-1);
            var keysToRemove = _counters
                .Where(kvp => kvp.Value.LastCallTime < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _counters.TryRemove(key, out _);
            }
        }
    }

    public class PerformanceCounter
    {
        public string Endpoint { get; set; } = "";
        public long TotalCalls { get; set; }
        public long ErrorCount { get; set; }
        public long TotalResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public DateTime LastCallTime { get; set; }
    }
}
