using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;

namespace TtWork.Project.Domains;

[Table("T_SensitiveWords")]
public class SensitiveWord : Entity<long>, IMayHaveTenant {
    [StringLength(64)] public string Content { get; set; }

    public int? TenantId { get; set; }
}

[AutoMapFrom(typeof(SensitiveWord))]
[AutoMapTo(typeof(SensitiveWord))]
public class SensitiveWordDto : EntityDto<long> {
    [StringLength(64)] public string Content { get; set; }
}