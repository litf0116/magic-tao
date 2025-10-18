using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Lib;
using TtWork.Project.Domains;
using TtWork.Project.Events;

namespace TtWork.Project.Applications;

public class SensitiveWordAppService : AbpAsyncCrudAppService<SensitiveWord, SensitiveWordDto, long, AppResultRequestDto
    , SensitiveWordDto, SensitiveWordDto>
{
    private new readonly IMediator _mediator;
    private readonly ILogger<SensitiveWordAppService> _logger;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public SensitiveWordAppService(
        IMediator mediator,
        IRepository<SensitiveWord, long> repository,
        IocManager iocManager,
        ILogger<SensitiveWordAppService> logger,
        IUnitOfWorkManager unitOfWorkManager) : base(
        repository, iocManager)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWorkManager = unitOfWorkManager;
        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
        base.DeletePermissionName = AppPermissions.Pages.ChatManager;
        base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
    }

    [HttpPost]
    public async Task BatchCreateAsync(BatchCreateRequest input)
    {
        var entities = input.Words.Split(',')
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new SensitiveWord { Content = x }).ToList();

        using (var uow = _unitOfWorkManager.Begin())
        {
            foreach (var entity in entities)
            {
                await Repository.InsertAsync(entity);
            }
            await uow.CompleteAsync();
        }

        // 自动重建缓存
        await RebuildCacheWithLogging("批量添加违禁词");
    }

    [HttpPost]
    public async Task ReBuildCache()
    {
        await _mediator.Send(new QueryCacheWords(true));
    }

    /// <summary>
    /// 获取当前缓存中的违禁词列表
    /// </summary>
    /// <returns>违禁词列表</returns>
    [HttpGet]
    public async Task<object> GetCachedWords()
    {
        var words = await _mediator.Send(new QueryCacheWords());
        return new
        {
            totalCount = words.Length,
            words = words,
            cacheKey = AppConsts.SensitiveWordsCacheKey,
            timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// 创建违禁词 - 重写基类方法以添加自动缓存重建
    /// </summary>
    public override async Task<SensitiveWordDto> CreateAsync(SensitiveWordDto input)
    {
        var result = await base.CreateAsync(input);
        await RebuildCacheWithLogging($"创建违禁词: {input.Content}");
        return result;
    }

    /// <summary>
    /// 更新违禁词 - 重写基类方法以添加自动缓存重建
    /// </summary>
    public override async Task<SensitiveWordDto> UpdateAsync(SensitiveWordDto input)
    {
        var result = await base.UpdateAsync(input);
        await RebuildCacheWithLogging($"更新违禁词: {input.Content}");
        return result;
    }

    /// <summary>
    /// 删除违禁词 - 重写基类方法以添加自动缓存重建
    /// </summary>
    public override async Task DeleteAsync(EntityDto<long> input)
    {
        var entity = await Repository.GetAsync(input.Id);
        await base.DeleteAsync(input);
        await RebuildCacheWithLogging($"删除违禁词: {entity?.Content}");
    }

    
    /// <summary>
    /// 带日志的缓存重建方法
    /// </summary>
    private async Task RebuildCacheWithLogging(string operation)
    {
        try
        {
            _logger.LogInformation("开始重建违禁词缓存，操作：{Operation}", operation);
            await _mediator.Send(new QueryCacheWords(true));
            _logger.LogInformation("违禁词缓存重建完成，操作：{Operation}", operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重建违禁词缓存失败，操作：{Operation}", operation);
            throw;
        }
    }

    /// <summary>
    /// 检查缓存健康状态
    /// </summary>
    [HttpGet]
    public async Task<object> CheckCacheHealth()
    {
        try
        {
            var words = await _mediator.Send(new QueryCacheWords());
            var dbCount = await Repository.CountAsync();

            return new
            {
                isHealthy = words.Length > 0,
                cacheCount = words.Length,
                databaseCount = dbCount,
                isSync = words.Length == dbCount,
                lastCheck = DateTime.Now,
                cacheKey = AppConsts.SensitiveWordsCacheKey
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查违禁词缓存健康状态失败");
            return new
            {
                isHealthy = false,
                error = ex.Message,
                lastCheck = DateTime.Now
            };
        }
    }

    protected override IQueryable<SensitiveWord> CreateFilteredQuery(AppResultRequestDto input)
    {
        return base.CreateFilteredQuery(input)
            .WhereIf(!input.Keyword.IsNullOrEmptyOrWhiteSpace(), x => x.Content.Contains(input.Keyword));
    }

    public class BatchCreateRequest
    {
        public string Words { get; set; }
    }
}