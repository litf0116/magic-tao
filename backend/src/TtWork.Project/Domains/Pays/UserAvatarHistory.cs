using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains.Pays;

/// <summary>
/// 用户头像修改历史（最多保留 5 条）
/// </summary>
[Table("Pays_UserAvatarHistory")]
public class UserAvatarHistory : Entity<long>, IMustHaveTenant
{
    public UserAvatarHistory()
    {
        ChangeTime = DateTime.Now;
    }

    /// <summary>
    /// 租户ID
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 修改前的头像URL（用于回退）
    /// </summary>
    [StringLength(512)]
    public string PreviousHeadImgUrl { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ChangeTime { get; set; }

    /// <summary>
    /// 修改来源：User=用户上传, Admin=管理员修改, System=系统修正
    /// </summary>
    [StringLength(32)]
    public string ChangeSource { get; set; }
}
