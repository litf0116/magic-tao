using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TtWork.Project.Web.Host.Middleware
{
    public class AppFeatureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AppFeatureMiddleware> _logger;

        public AppFeatureMiddleware(RequestDelegate next, ILogger<AppFeatureMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var platform = context.Request.Headers["X-Platform"].ToString();
            var appVersion = context.Request.Headers["X-App-Version"].ToString();

            if (!string.IsNullOrEmpty(platform))
            {
                context.Items["X-Platform"] = platform;
            }

            if (!string.IsNullOrEmpty(appVersion))
            {
                context.Items["X-App-Version"] = appVersion;
            }

            _logger.LogDebug("AppFeatureMiddleware: Platform={Platform}, Version={Version}", platform, appVersion);

            await _next(context);
        }
    }

    public static class AppFeatureMiddlewareExtensions
    {
        public static IApplicationBuilder UseAppFeature(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AppFeatureMiddleware>();
        }
    }
}