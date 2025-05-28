using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 群聊等级设置表
/// </summary>
[SugarTable("t_GroupChatLevelSettings")]
[Description("群聊等级设置表")]
public class GroupChatLevelSettingsEntity : AutoIncrementEntity
{
    /// <summary>
    /// 等级名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; }
    /// <summary>
    /// 所需金额
    /// </summary>
    public decimal AmountRequired { get; set; }
    /// <summary>
    /// 边框颜色
    /// </summary>
    public string BorderColor { get; set; }
    /// <summary>
    /// 右边框颜色
    /// </summary>
    public string RightBorderColor { get; set; }
}
