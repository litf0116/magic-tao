using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp.Definitions;
using TtWork.Project.Core;

namespace TtWork.Project.Applications;

[Route("api/services/app/[controller]/[action]")]
public class AppFeatureSwitchAppService : ApplicationService
{
    private readonly ISettingManager _settingManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheManager _cacheManager;

    public AppFeatureSwitchAppService(
        ISettingManager settingManager, 
        IHttpContextAccessor httpContextAccessor,
        ICacheManager cacheManager)
    {
        _settingManager = settingManager;
        _httpContextAccessor = httpContextAccessor;
        _cacheManager = cacheManager;
    }

    [HttpGet]
    [AbpAllowAnonymous]
    public Dictionary<string, string> DebugHeaders()
    {
        var result = new Dictionary<string, string>();
        
        var httpContext = _httpContextAccessor?.HttpContext;
        
        result["HasHttpContextAccessor"] = (_httpContextAccessor != null).ToString();
        result["HasHttpContext"] = (httpContext != null).ToString();
        
        if (httpContext?.Request?.Headers != null)
        {
            result["HeaderCount"] = httpContext.Request.Headers.Count.ToString();
            result["X-Platform"] = httpContext.Request.Headers.TryGetValue("X-Platform", out var platform) ? platform.ToString() : "not found";
            result["X-App-Version"] = httpContext.Request.Headers.TryGetValue("X-App-Version", out var version) ? version.ToString() : "not found";
        }
        
        return result;
    }

    [HttpGet]
    [AbpAllowAnonymous]
    public async Task<AppFeatureSwitchDto> GetFeatureSwitch()
    {
        var platform = GetPlatform();
        var version = GetVersion();

        var result = new AppFeatureSwitchDto
        {
            Platform = platform,
            Version = version,
            IsReviewMode = false,
            Features = new Dictionary<string, bool>()
        };

        if (string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(version))
        {
            return result;
        }

        try
        {
            var reviewVersion = await GetReviewVersion(platform);
            result.IsReviewMode = !string.IsNullOrEmpty(reviewVersion) && 
                                  string.Equals(NormalizeVersion(version), NormalizeVersion(reviewVersion), StringComparison.OrdinalIgnoreCase);

            // 兼容旧接口：features 全部返回 true（前端实际使用 isReviewMode 判断）
            result.Features["ShowAuction"] = true;
            result.Features["ShowTradingPost"] = true;
            result.Features["ShowBanner"] = true;
        }
        catch (Exception)
        {
        }

        return result;
    }

    private string GetPlatform()
    {
        if (_httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue("X-Platform", out var platformValue) == true)
        {
            return platformValue.ToString();
        }
        return string.Empty;
    }

    private string GetVersion()
    {
        if (_httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue("X-App-Version", out var versionValue) == true)
        {
            return versionValue.ToString();
        }
        return string.Empty;
    }

    private async Task<string> GetReviewVersion(string platform)
    {
        return platform switch
        {
            "mp-weixin" => await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionMpWeixin),
            "app-plus" => await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionAppPlus),
            "h5" => await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionH5),
            _ => string.Empty
        };
    }

    private string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return string.Empty;
        
        var parts = version.Split('@');
        return parts.Length >= 2 ? parts[1] : version;
    }

    [HttpGet]
    public async Task<string> GetReviewVersionConfig()
    {
        var platform = GetPlatform();
        return await GetReviewVersion(platform);
    }

    [HttpGet]
    public async Task<Dictionary<string, string>> GetAllReviewVersions()
    {
        return new Dictionary<string, string>
        {
            ["mp-weixin"] = await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionMpWeixin),
            ["app-plus"] = await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionAppPlus),
            ["h5"] = await SettingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ReviewVersionH5)
        };
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Administration)]
    public async Task UpdateReviewVersion([FromBody] UpdateReviewVersionInput input)
    {
        var settingName = input.Platform switch
        {
            "mp-weixin" => AppSettings.FeatureSwitch.ReviewVersionMpWeixin,
            "app-plus" => AppSettings.FeatureSwitch.ReviewVersionAppPlus,
            "h5" => AppSettings.FeatureSwitch.ReviewVersionH5,
            _ => throw new ArgumentException($"Unsupported platform: {input.Platform}")
        };

        await SettingManager.ChangeSettingForApplicationAsync(settingName, input.ReviewVersion ?? "");
        
        var settingCache = _cacheManager.GetCache("AbpZeroSettingCache");
        await settingCache.ClearAsync();
    }
}

public class AppFeatureSwitchDto
{
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsReviewMode { get; set; }
    public Dictionary<string, bool> Features { get; set; } = new();
}

public class UpdateReviewVersionInput
{
    public string Platform { get; set; } = string.Empty;
    public string ReviewVersion { get; set; } = string.Empty;
}
