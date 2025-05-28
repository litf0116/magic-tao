using SqlSugar;
using System.ComponentModel;

namespace TtWork.Abp.Entity;

/// <summary>
/// 提现金额
/// </summary>
[SugarTable("t_withdrawalAmount")]
[Description("提现金额")]
public class WithdrawalAmountEntity : AutoIncrementEntity
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 提现金额
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 提现时间
    /// </summary>
    public DateTime WithdrawalTime { get; set; }
    /// <summary>
    /// 审核状态 1 审核中 2 拒绝 3 审核通过
    /// </summary>
    public int Status { get; set; }
}
