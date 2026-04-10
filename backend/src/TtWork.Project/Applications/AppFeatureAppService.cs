using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp.Definitions;

namespace TtWork.Project.Applications;

/// <summary>
/// 功能开关服务 - 兼容前端路径 /api/services/app/AppFeature/
/// 代理转发到 AppFeatureSwitchAppService
/// </summary>
[Route("api/services/app/[controller]/[action]")]
public class AppFeatureAppService : ApplicationService
{
    private readonly AppFeatureSwitchAppService _featureSwitchService;

    public AppFeatureAppService(AppFeatureSwitchAppService featureSwitchService)
    {
        _featureSwitchService = featureSwitchService;
    }

    [HttpGet]
    [AbpAllowAnonymous]
    public Task<AppFeatureSwitchDto> GetFeatureSwitch()
    {
        return _featureSwitchService.GetFeatureSwitch();
    }

    [HttpGet]
    public Task<Dictionary<string, bool>> GetFeatureConfig()
    {
        return _featureSwitchService.GetFeatureConfig();
    }

    [HttpGet]
    public Task<Dictionary<string, string>> GetFeatureVersionConfig()
    {
        return _featureSwitchService.GetFeatureVersionConfig();
    }

    [HttpGet]
    public Dictionary<string, string> DebugHeaders()
    {
        return _featureSwitchService.DebugHeaders();
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Administration)]
    public Task UpdateFeatureSwitch([FromBody] UpdateFeatureSwitchInput input)
    {
        return _featureSwitchService.UpdateFeatureSwitch(input);
    }
}