using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace TtWork.Project.Domains;

/// <summary>
/// 群聊等级设置表 (EF Core 版本)
/// </summary>
[Table("t_GroupChatLevelSettings")]
public class GroupChatLevelSetting : Entity<int>
{
    /// <summary>
    /// 等级名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 所需金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountRequired { get; set; }

    /// <summary>
    /// 边框颜色
    /// </summary>
    public string? BorderColor { get; set; }

    /// <summary>
    /// 右边框颜色
    /// </summary>
    public string? RightBorderColor { get; set; }
}
