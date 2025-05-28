using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 角色表
/// </summary>
[SugarTable("abproles")]
[Description("角色表")]
public class RoleEntity : AutoIncrementEntity
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public int IsStatic { get; set; }
}