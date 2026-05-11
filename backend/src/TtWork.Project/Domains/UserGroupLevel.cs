using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TtWork.Project.Domains;

/// <summary>
/// 用户群等级记录表 (EF Core 版本)
/// </summary>
[Table("t_UserGroupLevel")]
public class UserGroupLevel : Entity<int>
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 群聊等级编号
    /// </summary>
    public int GroupChatId { get; set; }

    /// <summary>
    /// 累计金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CumulativeAmount { get; set; }
}
