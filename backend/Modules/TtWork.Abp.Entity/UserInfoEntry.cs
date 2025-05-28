using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 用户信息
/// </summary>
[SugarTable("abpusers")]
[Description("用户信息")]
public class UserInfoEntity : AutoIncrementEntity
{
    /// <summary>
    /// 用户名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 头像
    /// </summary>
    public string HeadImgUrl { get; set; }
    /// <summary>
    /// IM编号
    /// </summary>
    public int LastModifierUserId { get; set; }
    /// <summary>
    /// 保证金
    /// </summary>
    public decimal DepositBalance { get; set; }
    public string qq { get; set; }
    public string wx { get; set; }
}
