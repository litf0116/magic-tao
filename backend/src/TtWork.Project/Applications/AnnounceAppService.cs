using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Lib;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class AnnounceAppService : AbpAsyncCrudAppService<Announce, AnnounceDto, long, AppResultRequestDto,
    AnnounceCreateOrUpdateDto, AnnounceCreateOrUpdateDto> {
    public AnnounceAppService(IRepository<Announce, long> repository, IocManager iocManager) : base(repository,
        iocManager) {
        EnableGetEdit = true;
        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
        base.DeletePermissionName = AppPermissions.Pages.ChatManager;
    }

    /// <summary>
    /// 取得分类下最新的公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<AnnounceDto> GetLatest(EntityDto<long> input) {
        var find = await Repository.GetAll().AsNoTracking()
            .Where(x => x.CategoryId == input.Id)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        return MapToEntityDto(find);
    }

    public Task<PagedResultDto<AnnounceDto>> GetAllPublicAsync(AppResultRequestDto input) {
        input.Status = 1;
        if (input.Sorting.IsNullOrEmptyOrWhiteSpace())
            input.Sorting = "order desc";
        return base.GetAllAsync(input);
    }

    protected override IQueryable<Announce> CreateFilteredQuery(AppResultRequestDto input) {
        return base.CreateFilteredQuery(input)
                .WhereIf(input.Pid.HasValue, x => x.CategoryId == input.Pid)
            ;
    }
}