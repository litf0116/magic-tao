using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 消息配置表
/// </summary>
[SugarTable("t_msgConfiguration")]
[Description("消息配置表")]
public class MsgConfigurationEntity : AutoIncrementEntity
{
    /// <summary>
    /// 类型 1、新用户出价提示 2、提现提示
    /// </summary>
    public int Type { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    public string Msg { get; set; }
}
