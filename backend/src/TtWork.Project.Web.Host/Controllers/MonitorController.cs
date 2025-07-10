using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TtWork.Project.Web.Host.Services;

namespace TtWork.Project.Web.Host.Controllers
{
    /// <summary>
    /// 系统监控API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // 根据需要调整权限
    public class MonitorController : AbpController
    {
        private readonly HealthCheckService _healthCheckService;

        public MonitorController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// 系统健康检查
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Exception = entry.Value.Exception?.Message
                })
            };

            return healthReport.Status == HealthStatus.Healthy
                ? Ok(response)
                : StatusCode(503, response);
        }

        /// <summary>
        /// 系统性能统计
        /// </summary>
        [HttpGet("performance")]
        public IActionResult Performance()
        {
            var stats = PerformanceCounterService.GetStatistics();
            var process = Process.GetCurrentProcess();

            var systemInfo = new
            {
                // 系统资源使用情况
                System = new
                {
                    WorkingSet = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2), // MB
                    PrivateMemory = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2), // MB
                    CpuTime = Math.Round(process.TotalProcessorTime.TotalMilliseconds, 2),
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    StartTime = process.StartTime,
                    Uptime = DateTime.Now - process.StartTime
                },

                // GC统计
                GC = new
                {
                    Generation0 = GC.CollectionCount(0),
                    Generation1 = GC.CollectionCount(1),
                    Generation2 = GC.CollectionCount(2),
                    TotalMemory = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2) // MB
                },

                // API统计
                ApiStatistics = stats
            };

            return Ok(systemInfo);
        }

        /// <summary>
        /// 获取慢请求列表
        /// </summary>
        [HttpGet("slow-requests")]
        public IActionResult SlowRequests()
        {
            var stats = PerformanceCounterService.GetStatistics();
            var slowRequests = stats
                .Select(kvp => kvp.Value)
                .Cast<dynamic>()
                .Where(stat => stat.AvgResponseTime > 1000) // 超过1秒的请求
                .OrderByDescending(stat => stat.AvgResponseTime)
                .Take(20)
                .ToList();

            return Ok(new { SlowRequests = slowRequests });
        }

        /// <summary>
        /// 获取错误统计
        /// </summary>
        [HttpGet("errors")]
        public IActionResult Errors()
        {
            var stats = PerformanceCounterService.GetStatistics();
            var errorStats = stats
                .Select(kvp => kvp.Value)
                .Cast<dynamic>()
                .Where(stat => stat.ErrorCount > 0)
                .OrderByDescending(stat => stat.ErrorRate)
                .Take(20)
                .ToList();

            return Ok(new { ErrorStatistics = errorStats });
        }

        /// <summary>
        /// 清理统计数据
        /// </summary>
        [HttpPost("clear-stats")]
        [Authorize] // 需要权限
        public IActionResult ClearStats()
        {
            // 这里可以添加清理逻辑
            return Ok(new { Message = "统计数据已清理" });
        }
    }
}
