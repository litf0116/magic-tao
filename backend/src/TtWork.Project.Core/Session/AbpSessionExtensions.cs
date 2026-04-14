using Abp.Dependency;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace TtWork.Project.Core.Session;

/// <summary>
/// ABP Session 扩展方法
/// </summary>
public static class AbpSessionExtensions
{
    private const string AppVersionHeader = "AppVersion";

    /// <summary>
    /// 从请求头获取应用版本号
    /// </summary>
    /// <param name="session">ABP Session</param>
    /// <returns>版本号字符串</returns>
    public static string? GetAppVersion(this IAbpSession session)
    {
        // 通过 IocManager 解析 IHttpContextAccessor
        var httpContextAccessor = IocManager.Instance.Resolve<IHttpContextAccessor>();
        return httpContextAccessor?.HttpContext?.Request.Headers[AppVersionHeader].FirstOrDefault();
    }
}