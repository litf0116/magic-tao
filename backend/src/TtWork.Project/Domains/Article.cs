using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using FluentValidation;
using TtWork.Lib;

namespace TtWork.Project.Domains;

public enum AlticleStatusEnum {
    草稿 = 0,
    已发布 = 1
}

[Table("T_CmsArticle")]
public class CmsArticle : FullAuditedAggregateRoot<long> {
    public long CategoryId { get; set; }
    [StringLength(128)] public string Title { get; set; }
    [StringLength(128)] public string TitleImageUrl { get; set; }
    public string Content { get; set; }

    public int Sort { get; set; }
    public AlticleStatusEnum Status { get; set; }
}

[AutoMapFrom(typeof(CmsArticle))]
public class CmsArticleDto : CreationAuditedEntityDto<long> {
    public long CategoryId { get; set; }
    public string Title { get; set; }
    public string TitleImageUrl { get; set; }
    public string Content { get; set; }
    public int Sort { get; set; }
    public AlticleStatusEnum Status { get; set; }
}

[AutoMapFrom(typeof(CmsArticle))]
[AutoMapTo(typeof(CmsArticle))]
public class CmsArticleCreateOrUpdateDto : EntityDto<long> {
    public long CategoryId { get; set; } = 1;
    public string Title { get; set; }
    public string TitleImageUrl { get; set; }
    public string Content { get; set; }
    public int Sort { get; set; }
    public AlticleStatusEnum Status { get; set; } = AlticleStatusEnum.已发布;
}

public class CmsArticleCreateOrUpdateDtoValidator : AbstractValidator<CmsArticleCreateOrUpdateDto> {
    public CmsArticleCreateOrUpdateDtoValidator() {
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("板块必填");
        RuleFor(x => x).Must(x => !x.TitleImageUrl.IsNullOrEmptyOrWhiteSpace()).WithMessage("请上传图片");
    }
}

[Table("T_CmsCategory")]
public class CmsCategory : FullAuditedAggregateRoot<long> {
    [StringLength(128)] public string Title { get; set; }
    [StringLength(128)] public string TitleImageUrl { get; set; }
    public int Sort { get; set; }
}

[AutoMapFrom(typeof(CmsCategory))]
public class CmsCategoryDto : EntityDto<long> {
    public string Title { get; set; }
    public string TitleImageUrl { get; set; }
    public int Sort { get; set; }
}

[AutoMapFrom(typeof(CmsCategory))]
[AutoMapTo(typeof(CmsCategory))]
public class CmsCategoryCreateOrUpdateDto : EntityDto<long> {
    public string Title { get; set; }
    public string TitleImageUrl { get; set; }
    public int Sort { get; set; }
}