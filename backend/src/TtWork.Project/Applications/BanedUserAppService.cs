using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class BanedUserAppService : AbpAsyncCrudAppService<BanedUser, BanedUserDto, long, AppResultRequestDto,
    BanedUserDto, BanedUserDto> {
    public BanedUserAppService(
        IRepository<BanedUser, long> repository,
        IocManager iocManager) : base(repository, iocManager) {
        base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
        base.DeletePermissionName = AppPermissions.Pages.ChatManager;

        base.GetUser = true;
        base.GetCreatorUser = true;
    }

    public override Task<BanedUserDto> UpdateAsync(BanedUserDto input) => throw new Exception("NOT SUPPORTED");
    public override Task<BanedUserDto> CreateAsync(BanedUserDto input) => throw new Exception("NOT SUPPORTED");


    protected override IQueryable<BanedUser> CreateFilteredQuery(AppResultRequestDto input) {
        return base.CreateFilteredQuery(input)
                .WhereIf(input.Status is 1, x => x.EndTime > DateTime.UtcNow)
                .WhereIf(input.Status is 0, x => x.EndTime <= DateTime.UtcNow)
            ;
    }
}