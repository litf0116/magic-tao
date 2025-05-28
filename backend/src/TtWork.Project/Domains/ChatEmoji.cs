using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Domains;

[Table("T_ChatEmoji")]
public class ChatEmoji : CreationAuditedEntity<long>, ISoftDelete {
    [StringLength(256)] public string Url { get; set; }
    public bool IsDeleted { get; set; }
    [StringLength(2048)] public string Payload { get; set; } = "{}";
}

[AutoMapFrom(typeof(ChatEmoji))]
[AutoMapTo(typeof(ChatEmoji))]
public class ChatEmojiDto : EntityDto<long> {
    public string Url { get; set; }
    public string Payload { get; set; }
}