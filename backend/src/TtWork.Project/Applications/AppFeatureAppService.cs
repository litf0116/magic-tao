using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp.Definitions;

namespace TtWork.Project.Applications;

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
    public Task<string> GetReviewVersionConfig()
    {
        return _featureSwitchService.GetReviewVersionConfig();
    }

    [HttpGet]
    public Task<Dictionary<string, string>> GetAllReviewVersions()
    {
        return _featureSwitchService.GetAllReviewVersions();
    }

    [HttpGet]
    public Dictionary<string, string> DebugHeaders()
    {
        return _featureSwitchService.DebugHeaders();
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Administration)]
    public Task UpdateReviewVersion([FromBody] UpdateReviewVersionInput input)
    {
        return _featureSwitchService.UpdateReviewVersion(input);
    }
}
