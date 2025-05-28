using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains.Pays;

/// <summary>
/// 用户余额记录表
/// </summary>
[Table("Pays_UserBalanceLog")]
public class UserBalanceLog : FullAuditedAggregateRoot<Ulid>, IMustHaveTenant, IExtendableObject {
    public UserBalanceLog(BalanceLogType type, decimal amount, string reason = null) {
        Id = Ulid.NewUlid();
        Type = type;
        Amount = amount;
        Reason = reason;
    }

    public int TenantId { get; set; }
    public string ExtensionData { get; set; }

    [Column(TypeName = "decimal(18, 2)")] public decimal Amount { get; private set; } //金额（正数表示支付，负数表示扣除或退还）
    public BalanceLogType Type { get; private set; } //（支付、扣除、退还）
    [StringLength(128)] public string Reason { get; set; }
    public bool IsSuccess { get; private set; }
    public DateTime? SuccessTime { get; private set; }
    [Column(TypeName = "decimal(18, 2)")] public decimal? AfterAmount { get; private set; } //操作后金额
}

[AutoMapFrom(typeof(UserBalanceLog))]
public class UserBalanceLogDto : EntityDto<Ulid> {
    public decimal Amount { get; set; }
    public BalanceLogType Type { get; set; }
    public string Reason { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime? SuccessTime { get; set; }
    public decimal? AfterAmount { get;  set; }
}