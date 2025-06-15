using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using TtWork.Project.Web.Host.Services;

namespace TtWork.Project.Web.Host.Startup
{
    /// <summary>
    /// 简单的请求性能监控中间件
    /// </summary>
    public class RequestPerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestPerformanceMiddleware> _logger;

        public RequestPerformanceMiddleware(RequestDelegate next, ILogger<RequestPerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 跳过静态文件和Swagger
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/images"))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // 添加请求上下文
            using (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "Anonymous"))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
            using (LogContext.PushProperty("ClientIP", GetClientIpAddress(context)))
            {
                try
                {
                    // 记录请求开始
                    Log.Information("API请求开始 {Method} {Path} {QueryString}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Request.QueryString);

                    await _next(context);

                    stopwatch.Stop();

                    // 记录请求完成
                    var level = GetLogLevel(context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                    Log.Write(level, "API请求完成 {Method} {Path} {StatusCode} {ElapsedMs}ms {ContentLength}bytes",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        context.Response.ContentLength ?? 0);

                    // 记录性能统计
                    PerformanceCounterService.RecordApiCall(
                        $"{context.Request.Method} {context.Request.Path}",
                        stopwatch.ElapsedMilliseconds,
                        context.Response.StatusCode);

                    // 性能告警：超过3秒的请求
                    if (stopwatch.ElapsedMilliseconds > 3000)
                    {
                        Log.Warning("慢请求告警 {Method} {Path} 耗时 {ElapsedMs}ms",
                            context.Request.Method,
                            context.Request.Path,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Log.Error(ex, "API请求异常 {Method} {Path} {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0] ??
                   context.Request.Headers["X-Real-IP"].ToString() ??
                   context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private Serilog.Events.LogEventLevel GetLogLevel(int statusCode, long elapsedMs)
        {
            if (statusCode >= 500) return Serilog.Events.LogEventLevel.Error;
            if (statusCode >= 400) return Serilog.Events.LogEventLevel.Warning;
            if (elapsedMs > 1000) return Serilog.Events.LogEventLevel.Warning;
            return Serilog.Events.LogEventLevel.Information;
        }
    }
}
