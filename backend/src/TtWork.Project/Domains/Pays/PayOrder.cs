using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.UI;
using TtWork.Lib;

namespace TtWork.Project.Domains.Pays;

[Table("Pays_PayOrder")]
public class PayOrder : FullAuditedAggregateRoot<Ulid>, IMustHaveTenant, IExtendableObject {
    public PayOrder() {
        Id = Ulid.NewUlid();
    }

    public int TenantId { get; set; }

    /// <summary>
    /// 单位分
    /// </summary>
    public int Total { get; protected set; }

    [StringLength(48)] public string OutTradeNo { get; protected set; }
    [StringLength(48)] public string OpenId { get; protected set; }
    [StringLength(32)] public string MchId { get; protected set; }
    [StringLength(32)] public string AppId { get; protected set; }
    public OrderType HostType { get; protected set; }
    [StringLength(48)] public string HostId { get; protected set; }
    public PayType PayType { get; protected set; }
    /// <summary>
        /// 状态：-1 CANCELLED 0 UNPAID 1 PAID 3 REFUNDED
    /// </summary>
    public PayState State { get;  set; }

    #region 支付
    /// <summary>
    /// 是否成功支付
    /// </summary>
    public bool IsSuccessPay { get; protected set; }
    /// <summary>
    /// 支付时间
    /// </summary>
    public DateTime? SuccessPayTime { get; protected set; }

    #endregion

    #region 退款

    public bool IsRefund { get; protected set; }
    public DateTime? RefundTime { get; protected set; }
    public int? RefundPrice { get; protected set; } = null;

    #endregion

    public int? ShareFromUserId { get; set; }
    [StringLength(32)] public string AppName { get; set; }

    [StringLength(512)] public string ExtensionData { get; set; }

    public void SuccessPay(string notifyId, DateTime? time) {
            State = PayState.PAID;
        IsSuccessPay = true;
        SuccessPayTime = time ?? DateTime.Now;
        this.SetData("Notification_Id", notifyId);
    }

    public void CreateDepositPay(decimal amount, long userId, string openid, string appName, string appid, string mchId, int tenantId) {
        if (appName.IsNullOrEmptyOrWhiteSpace()) {
            throw new UserFriendlyException("未知的APP");
        }

        HostId = null;
        HostType = OrderType.保证金;
        Total = Convert.ToInt32(amount * 100);
        OutTradeNo = $"{Ulid.NewUlid()}";
        CreatorUserId = userId;
        OpenId = openid;
        AppName = appName;
        AppId = appid;
        MchId = mchId;
        TenantId = tenantId;
        PayType = PayType.微信;
    }

    public void CreateTopUpPay(decimal amount, long userId, string openid, string appName, string appid, string mchId, int tenantId) {
        if (appName.IsNullOrEmptyOrWhiteSpace()) {
            throw new UserFriendlyException("未知的APP");
        }

        HostId = null;
        HostType = OrderType.充值;
        Total = Convert.ToInt32(amount * 100);

        OutTradeNo = $"{Ulid.NewUlid()}";
        CreatorUserId = userId;
        OpenId = openid;
        AppName = appName;
        AppId = appid;
        MchId = mchId;
        TenantId = tenantId;
        PayType = PayType.微信;
    }
}

public enum PayType {
    微信 = 1,
    微信扫码 = 2
}

public enum OrderType {
    充值 = 1,
    保证金 = 2,
}

public enum PayState {
    CANCELLED = -1,
    UNPAID = 0,
    PAID = 1,
    REFUNDED = 3,
}