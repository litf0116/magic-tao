using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class BidHistoryAppService : AbpAsyncCrudAppService<BidHistory, BidHistoryDto, long, AppResultRequestDto,
    BidHistoryDto, BidHistoryDto> {
    public BidHistoryAppService(IRepository<BidHistory, long> repository, IocManager iocManager) : base(repository,
        iocManager) {
        base.GetAllPermissionName = AppPermissions.Administration;
        base.CreatePermissionName = AppPermissions.Administration;
        base.UpdatePermissionName = AppPermissions.Administration;
        base.DeletePermissionName = AppPermissions.Administration;
    }

    public override Task<BidHistoryDto> UpdateAsync(BidHistoryDto input) => throw new Exception("NOT SUPPORTED");
    public override Task<BidHistoryDto> CreateAsync(BidHistoryDto input) => throw new Exception("NOT SUPPORTED");
    public override Task DeleteAsync(EntityDto<long> input) => throw new Exception("NOT SUPPORTED");

    protected override IQueryable<BidHistory> CreateFilteredQuery(AppResultRequestDto input) {
        return base.CreateFilteredQuery(input)
            .WhereIf(input.Pid.HasValue, x => x.AuctionItemId == input.Pid.Value);
    }
    
    [HttpGet]
    [AbpAuthorize]
    public async Task<object> DateAnlayse(AppResultRequestDto input) {
        var query = await Repository.GetAll()
            .WhereIf(input.From.HasValue, x => x.CreationTime >= input.From)
            .WhereIf(input.To.HasValue, x => x.CreationTime <= input.From)
            .GroupBy(row => new {
                row.CreationTime.Year,
                row.CreationTime.Month,
                row.CreationTime.Date
            }).Select(grp => new {
                Label = $"{grp.Key.Date.Month}月{grp.Key.Date.Day}日",
                grp.Key.Year,
                grp.Key.Month,
                grp.Key.Date,
                Count = grp.Count()
            }).ToListAsync();
        return query.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Date);
    }
    
}