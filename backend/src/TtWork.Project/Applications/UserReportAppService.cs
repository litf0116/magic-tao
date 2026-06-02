using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class UserReportAppService : AbpAsyncCrudAppService<UserReport, UserReportDto, long, GetAllUserReportInput, CreateUserReportDto, UserReportDto> {
    public UserReportAppService(
        IRepository<UserReport, long> repository,
        IocManager iocManager) : base(repository, iocManager) {
        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;

        base.GetUser = true;
        base.GetCreatorUser = true;
    }

    public override Task<UserReportDto> CreateAsync(CreateUserReportDto input) {
        return base.CreateAsync(input);
    }

    [HttpPost]
    public async Task ProcessAsync(long id, string adminNote, int status) {
        var report = await Repository.GetAsync(id);
        if (report == null) {
            throw new Exception("Report not found");
        }

        report.Status = status;
        report.AdminNote = adminNote;
        report.ProcessedTime = DateTime.Now;

        await Repository.UpdateAsync(report);
    }

    protected override IQueryable<UserReport> CreateFilteredQuery(GetAllUserReportInput input) {
        var query = Repository.GetAll();

        if (!PermissionChecker.IsGranted(AppPermissions.Pages.ChatManager)) {
            query = query.Where(x => x.ReporterId == AbpSession.UserId);
        }

        if (input.Status.HasValue) {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query;
    }
}

public class GetAllUserReportInput : PagedResultRequestDto {
    public int? Status { get; set; }
}

[AutoMap(typeof(UserReport))]
public class CreateUserReportDto : EntityDto {
    public long MessageId { get; set; }
    public long ReportedUserId { get; set; }

    [MaxLength(200)]
    public string Chan { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; }

    [MaxLength(2000)]
    public string? Evidence { get; set; }
}

[AutoMap(typeof(UserReport))]
public class UserReportDto : EntityDto<long>, ICreationAudited {
    public long ReporterId { get; set; }
    public long MessageId { get; set; }
    public long ReportedUserId { get; set; }

    public string Chan { get; set; }

    public string Reason { get; set; }

    public string? Evidence { get; set; }

    public int Status { get; set; }

    public string? AdminNote { get; set; }

    public DateTime? ProcessedTime { get; set; }

    public DateTime CreationTime { get; set; }

    public long? CreatorUserId { get; set; }

    public UserDtoBase CreatorUser { get; set; }
}