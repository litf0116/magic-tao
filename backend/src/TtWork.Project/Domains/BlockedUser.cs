using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

[Table("T_BlockedUsers")]
public class BlockedUser : Entity<long>, ICreationAudited
{
    /// <summary>
    /// The user who initiated the block
    /// </summary>
    public long BlockerId { get; private set; }

    /// <summary>
    /// The user who was blocked
    /// </summary>
    public long BlockedUserId { get; private set; }

    /// <summary>
    /// Optional reason for blocking
    /// </summary>
    public string Reason { get; private set; }

    public DateTime CreationTime { get; set; }

    public long? CreatorUserId { get; set; }

    public BlockedUser()
    {
    }

    public BlockedUser(long blockerId, long blockedUserId, string reason = null)
    {
        BlockerId = blockerId;
        BlockedUserId = blockedUserId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason;
    }
}