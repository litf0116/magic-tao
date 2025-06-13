using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    public SensitiveWordAppService(
        IMediator mediator,
        IRepository<SensitiveWord, long> repository,
        IocManager iocManager) : base(
        repository, iocManager)
    {
        _mediator = mediator;
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
        foreach (var entity in entities)
        {
            await Repository.InsertAsync(entity);
        }
    }

    [HttpPost]
    public async Task ReBuildCache()
    {
        await _mediator.Send(new QueryCacheWords(true));
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