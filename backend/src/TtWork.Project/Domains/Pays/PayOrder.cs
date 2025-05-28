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
    /// 状态 0 未支付 1已支付
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
    public DateTime? RefundComplateTime { get; protected set; }
    public int? RefundPrice { get; protected set; } = null;

    #endregion

    public int? ShareFromUserId { get; set; }
    [StringLength(32)] public string AppName { get; set; }

    [StringLength(512)] public string ExtensionData { get; set; }

    public void Refund(in decimal refundPrice, string reason) {
        var canRefundPrice = Total - (RefundPrice ?? 0);
        if (refundPrice * 100 > canRefundPrice) {
            throw new UserFriendlyException($"退款金额不能大于可退款金额,当前可退金额:{canRefundPrice / 100m:0.00}");
        }

        State = PayState.退款中;
        IsRefund = true;
        RefundTime = DateTime.Now;

        // EventBus.Default.Trigger(new PayOrderRefundEvent(this, refundPrice, reason));
    }

    public void RefundComplate(int refundPrice) {
        RefundPrice ??= 0;

        RefundPrice += refundPrice;

        if (RefundPrice == Total) {
            RefundComplateTime = DateTime.Now;
            State = PayState.已退款;
        }
        else {
            IsRefund = false;
            State = PayState.部分退款;
        }
    }

    public void SuccessPay(string notifyId, DateTime? time) {
        State = PayState.已支付;
        IsSuccessPay = true;
        SuccessPayTime = time ?? DateTime.Now;
        this.SetData("Notification_Id", notifyId);
    }

    /// <summary>
    /// 退款操作
    /// </summary>
    /// <param name="value">退款金额，单位：分</param>
    public void Refund(int value) {
        IsRefund = true;
        RefundPrice = value;
        RefundTime = DateTime.Now;
    }


    public void RejectRefund() {
        IsRefund = false;
        RefundPrice = null;
        RefundTime = null;
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
    取消 = -1,
    未支付 = 0,
    已支付 = 1,
    退款中 = 2,
    已退款 = 3,
    部分退款 = 4
}