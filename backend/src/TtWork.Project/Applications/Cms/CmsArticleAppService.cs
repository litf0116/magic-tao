using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Lib;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications.Cms;

public class CmsArticleAppService : AbpAsyncCrudAppService<CmsArticle, CmsArticleDto, long, AppResultRequestDto,
    CmsArticleCreateOrUpdateDto, CmsArticleCreateOrUpdateDto> {
    public CmsArticleAppService(
        IRepository<CmsArticle, long> repository,
        IocManager iocManager) : base(repository,
        iocManager) {
        this.EnableGetEdit = true;

        base.CreatePermissionName = AppPermissions.Administration;
        base.DeletePermissionName = AppPermissions.Administration;
        base.UpdatePermissionName = AppPermissions.Administration;
    }

    public Task<PagedResultDto<CmsArticleDto>> GetAllPublicAsync(AppResultRequestDto input) {
        input.Status = 1;
        if (input.Sorting.IsNullOrEmptyOrWhiteSpace())
            input.Sorting = "order desc";
        return base.GetAllAsync(input);
    }

    protected override IQueryable<CmsArticle> CreateFilteredQuery(AppResultRequestDto input) =>
        base.CreateFilteredQuery(input)
            .WhereIf(input.Pid.HasValue, x => x.CategoryId == input.Pid.Value)
            .WhereIf(input.Status.HasValue, x => (int)x.Status == input.Status);
}


public class CmsCategoryAppService : AbpAsyncCrudAppService<CmsCategory, CmsCategoryDto, long, AppResultRequestDto,
    CmsArticleCreateOrUpdateDto, CmsArticleCreateOrUpdateDto> {
    public CmsCategoryAppService(
        IRepository<CmsCategory, long> repository,
        IocManager iocManager) : base(repository,
        iocManager) {
        this.EnableGetEdit = true;

        base.CreatePermissionName = AppPermissions.Administration;
        base.DeletePermissionName = AppPermissions.Administration;
        base.UpdatePermissionName = AppPermissions.Administration;
        base.GetAllPermissionName = AppPermissions.Administration;
    }
}