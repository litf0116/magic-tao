using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

/// <summary>
/// 用户在会话列表点击了删除后,会记录,下次拉取最新会话的时候不显示他的
/// 当和用户发送消息的时候,删除比记录
/// </summary>
[Table("T_ChatListDelete")]
public class ChatListDelete : Entity<int>, IHasCreationTime {
    [NotMapped] public override int Id { get; set; }
    public long UserId { get; set; }
    public long ToUserId { get; set; }
    public DateTime CreationTime { get; set; }
}