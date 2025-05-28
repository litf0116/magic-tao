using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity;

/// <summary>
/// 用户角色表
/// </summary>
[SugarTable("abpuserroles")]
[Description("用户角色表")]
public class UserRoleEntity : AutoIncrementEntity
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 角色编号
    /// </summary>
    public int RoleId { get; set; }
}