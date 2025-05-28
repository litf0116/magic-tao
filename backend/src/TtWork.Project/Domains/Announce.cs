using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using FluentValidation;
using TtWork.Lib;

namespace TtWork.Project.Domains;

[Table("T_Announce")]
public class Announce : FullAuditedEntity<long> {
    public long CategoryId { get; set; }
    [StringLength(2048)] public string Content { get; set; }
    [StringLength(256)] public string ImageUrl { get; set; }
    public int Sort { get; set; }
}

[AutoMapFrom(typeof(Announce))]
public class AnnounceDto : CreationAuditedEntityDto<long> {
    public long CategoryId { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public int Sort { get; set; }
}

[AutoMapFrom(typeof(Announce))]
[AutoMapTo(typeof(Announce))]
public class AnnounceCreateOrUpdateDto : EntityDto<long> {
    public long? CategoryId { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public int Sort { get; set; }
}

public class AnnounceCreateOrUpdateDtoValidator : AbstractValidator<AnnounceCreateOrUpdateDto> {
    public AnnounceCreateOrUpdateDtoValidator() {
        RuleFor(x => x.CategoryId).Must(x => x is > 0).WithMessage("请选择板块");
        RuleFor(x => x.Content).Must(x => !x.IsNullOrEmptyOrWhiteSpace()).WithMessage("必填,并控制在1000个字符以内");
        RuleFor(x => x.ImageUrl).Length(0, 256);
    }
}