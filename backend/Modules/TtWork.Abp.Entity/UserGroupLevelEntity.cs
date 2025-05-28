using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 用户群等级记录表
/// </summary>
[SugarTable("t_UserGroupLevel")]
[Description("用户群等级记录表")]
public class UserGroupLevelEntity : AutoIncrementEntity
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
    public decimal CumulativeAmount { get; set; }
}
