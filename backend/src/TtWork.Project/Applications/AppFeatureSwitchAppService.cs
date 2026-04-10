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

    private static readonly string[] Features = { "ShowAuction", "ShowTradingPost", "ShowBanner" };

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
            Features = new Dictionary<string, bool>()
        };

        foreach (var feature in Features)
        {
            result.Features[feature] = false;
        }

        if (string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(version))
        {
            return result;
        }

        try
        {
            var maxVersion = await GetMaxVersionForFeature("ShowAuction", platform);
            if (!string.IsNullOrEmpty(maxVersion) && CompareVersion(version, maxVersion) <= 0)
            {
                result.Features["ShowAuction"] = true;
            }

            maxVersion = await GetMaxVersionForFeature("ShowTradingPost", platform);
            if (!string.IsNullOrEmpty(maxVersion) && CompareVersion(version, maxVersion) <= 0)
            {
                result.Features["ShowTradingPost"] = true;
            }

            maxVersion = await GetMaxVersionForFeature("ShowBanner", platform);
            if (!string.IsNullOrEmpty(maxVersion) && CompareVersion(version, maxVersion) <= 0)
            {
                result.Features["ShowBanner"] = true;
            }
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

    private async Task<string> GetMaxVersionForFeature(string feature, string platform)
    {
        if (feature == "ShowAuction" && platform == "mp-weixin")
            return await _settingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ShowAuctionMaxVersionMpWeixin);
        if (feature == "ShowTradingPost" && platform == "mp-weixin")
            return await _settingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ShowTradingPostMaxVersionMpWeixin);
        if (feature == "ShowBanner" && platform == "mp-weixin")
            return await _settingManager.GetSettingValueAsync(AppSettings.FeatureSwitch.ShowBannerMaxVersionMpWeixin);
        return string.Empty;
    }

    private int CompareVersion(string version1, string version2)
    {
        var v1Parts = version1.Split('@');
        var v2Parts = version2.Split('@');

        if (v1Parts.Length < 2 || v2Parts.Length < 2)
        {
            return string.Compare(version1, version2, StringComparison.Ordinal);
        }

        var semVer1 = v1Parts[1];
        var semVer2 = v2Parts[1];

        var v1Nums = semVer1.Split('.');
        var v2Nums = semVer2.Split('.');

        for (int i = 0; i < Math.Max(v1Nums.Length, v2Nums.Length); i++)
        {
            int v1Num = i < v1Nums.Length && int.TryParse(v1Nums[i], out var n1) ? n1 : 0;
            int v2Num = i < v2Nums.Length && int.TryParse(v2Nums[i], out var n2) ? n2 : 0;

            if (v1Num > v2Num) return 1;
            if (v1Num < v2Num) return -1;
        }

        return 0;
    }

    [HttpGet]
    public async Task<Dictionary<string, bool>> GetFeatureConfig()
    {
        var platform = GetPlatform();
        var result = new Dictionary<string, bool>();

        foreach (var feature in Features)
        {
            var maxVersion = await GetMaxVersionForFeature(feature, platform);
            result[feature] = !string.IsNullOrEmpty(maxVersion);
        }

        return result;
    }

    [HttpGet]
    public async Task<Dictionary<string, string>> GetFeatureVersionConfig()
    {
        var platform = GetPlatform();
        var result = new Dictionary<string, string>();

        foreach (var feature in Features)
        {
            result[feature] = await GetMaxVersionForFeature(feature, platform);
        }

        return result;
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Administration)]
    public async Task UpdateFeatureSwitch([FromBody] UpdateFeatureSwitchInput input)
    {
        var settingName = $"AppFeatures.{input.Feature}.MaxVersion.{input.Platform}";
        await _settingManager.ChangeSettingForApplicationAsync(settingName, input.MaxVersion);
        
        // 清除设置缓存以确保所有实例都能获取最新值
        var settingCache = _cacheManager.GetCache("AbpZeroSettingCache");
        await settingCache.ClearAsync();
    }
}

public class AppFeatureSwitchDto
{
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, bool> Features { get; set; } = new();
}

public class UpdateFeatureSwitchInput
{
    public string Feature { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string MaxVersion { get; set; } = string.Empty;
}