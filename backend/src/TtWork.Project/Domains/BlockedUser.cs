using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace TtWork.Project.Domains;

[Table("T_BlockedUsers")]
public class BlockedUser : Entity<long>, ICreationAudited, IMayHaveTenant
{
    public long BlockerId { get; private set; }
    public long BlockedUserId { get; private set; }
    public string Reason { get; private set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public int? TenantId { get; set; }

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
