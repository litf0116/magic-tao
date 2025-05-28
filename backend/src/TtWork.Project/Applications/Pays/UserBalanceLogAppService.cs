using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Applications.Pays;


public class UserDepositLogAppService : AbpAsyncCrudAppService<UserDepositLog, UserDepositLogDto, Ulid, AppResultRequestDto, UserDepositLogDto, UserDepositLogDto>
{
    private readonly ILogger<UserBalanceLogAppService> _logger;

    public UserDepositLogAppService(
        ILogger<UserBalanceLogAppService> logger,
        IRepository<UserDepositLog, Ulid> repository,
        IocManager iocManager
    ) : base(repository, iocManager)
    {
        _logger = logger;

        base.CreatePermissionName = AppPermissions.Administration;
        base.UpdatePermissionName = AppPermissions.Administration;
        base.GetPermissionName = AppPermissions.Administration;
        base.DeletePermissionName = AppPermissions.Administration;
    }

    [HttpGet]
    [AbpAuthorize]
    public async Task<PagedResultDto<UserDepositLogDto>> GetMyAllAsync(AppResultRequestDto input)
    {
        input.UserId = AbpSession.UserId!.Value;
        return await base.GetAllAsync(input);
    }

    [AbpAuthorize]
    public override async Task<PagedResultDto<UserDepositLogDto>> GetAllAsync(AppResultRequestDto input)
    {
        if (!await IsInRoleAsync(AbpSession.UserId!.Value, AppPermissions.Administration))
        {
            input.UserId = AbpSession.UserId.Value;
        }
        return await base.GetAllAsync(input);
    }

    protected override IQueryable<UserDepositLog> CreateFilteredQuery(AppResultRequestDto input)
    {
        var result = base.CreateFilteredQuery(input)
            .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value);
        return result;
    }
}




public class UserBalanceLogAppService : AbpAsyncCrudAppService<UserBalanceLog, UserBalanceLogDto, Ulid, AppResultRequestDto, UserBalanceLogDto, UserBalanceLogDto>
{
    private readonly ILogger<UserBalanceLogAppService> _logger;

    public UserBalanceLogAppService(
        ILogger<UserBalanceLogAppService> logger,
        IRepository<UserBalanceLog, Ulid> repository,
        IocManager iocManager
    ) : base(repository, iocManager)
    {
        _logger = logger;

        base.CreatePermissionName = AppPermissions.Administration;
        base.UpdatePermissionName = AppPermissions.Administration;
        base.GetPermissionName = AppPermissions.Administration;
        base.DeletePermissionName = AppPermissions.Administration;
    }

    [HttpGet]
    [AbpAuthorize]
    public async Task<PagedResultDto<UserBalanceLogDto>> GetMyAllAsync(AppResultRequestDto input)
    {
        input.UserId = AbpSession.UserId!.Value;
        var reslut = await base.GetAllAsync(input);
        return reslut;
    }

    [AbpAuthorize]
    public override async Task<PagedResultDto<UserBalanceLogDto>> GetAllAsync(AppResultRequestDto input)
    {
        if (!await IsInRoleAsync(AbpSession.UserId!.Value, AppPermissions.Administration))
        {
            input.UserId = AbpSession.UserId.Value;
        }
        return await base.GetAllAsync(input);
    }

    protected override IQueryable<UserBalanceLog> CreateFilteredQuery(AppResultRequestDto input)
    {
        var result = base.CreateFilteredQuery(input)
            .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value);
        return result;
    }
}