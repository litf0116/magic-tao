using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using FluentValidation;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Domains;

namespace TtWork.Project.Domains;

[Table("T_ChatGroups")]
public class ChatGroup : CreationAuditedEntity<long> {
    [StringLength(64)] public string Title { get; set; }
    public int Limit { get; set; } = 5;
    public bool IsHidden { get; set; } = false;
}

[AutoMapFrom(typeof(ChatGroup))]

public class ChatGroupDto : EntityDto<long>, IHaveCreatorUser, IHasCreationTime {
    public string Title { get; set; }
    public int? Limit { get; set; }
    public bool IsHidden { get; set; } = false;

    public UserDtoBase CreatorUser { get; set; }
    public long? CreatorUserId { get; set; }

    public string Chan => $"-{Id}_{Title}";
    public DateTime CreationTime { get; set; }
}

[AutoMapFrom(typeof(ChatGroup))]
[AutoMapTo(typeof(ChatGroup))]
public class ChatGroupCreateOrUpdateDto : EntityDto<long> {
    public string Title { get; set; } = "";
    public int Limit { get; set; } = 5;
}

public class GroupCreateOrUpdateDtoValidator : AbstractValidator<ChatGroupCreateOrUpdateDto> {
    const string reg =
        @"^((?:[\u3400-\u4DB5\u4E00-\u9FEA\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\uD840-\uD868\uD86A-\uD86C\uD86F-\uD872\uD874-\uD879][\uDC00-\uDFFF]|\uD869[\uDC00-\uDED6\uDF00-\uDFFF]|\uD86D[\uDC00-\uDF34\uDF40-\uDFFF]|\uD86E[\uDC00-\uDC1D\uDC20-\uDFFF]|\uD873[\uDC00-\uDEA1\uDEB0-\uDFFF]|\uD87A[\uDC00-\uDFE0])|([0-9a-zA-Z])){4,12}$";

    public GroupCreateOrUpdateDtoValidator() {
        RuleFor(x => x.Title).Must(x => Regex.IsMatch(x, reg, RegexOptions.IgnoreCase))
            .WithMessage("群名称只能是4-12位中文或字母数字组合");
        RuleFor(x => x.Limit).Must(x => x >= 2 && x <= 5).WithMessage("群人数限制只能是2-5人");
    }
}