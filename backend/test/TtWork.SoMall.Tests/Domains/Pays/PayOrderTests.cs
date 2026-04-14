using Shouldly;
using TtWork.Project.Domains.Pays;
using Xunit;

namespace TtWork.SoMall.Tests.Domains.Pays;

public class PayOrderTests
{
    #region CreateDepositPay Tests

    [Fact]
    public void CreateDepositPay_Should_SetCorrectProperties()
    {
        var payOrder = new PayOrder();
        var amount = 51m;
        var userId = 1L;
        var openid = "";
        var appName = "pub";
        var appid = "wxfb7bd5b5f94a8805";
        var mchid = "1669900694";
        var tenantId = 1;

        payOrder.CreateDepositPay(amount, userId, openid, appName, appid, mchid, tenantId);

        payOrder.Total.ShouldBe(5100);
        payOrder.HostType.ShouldBe(OrderType.保证金);
        payOrder.State.ShouldBe(PayState.未支付);
        payOrder.OutTradeNo.ShouldNotBeNullOrEmpty();
        payOrder.OutTradeNo.Length.ShouldBeLessThanOrEqualTo(48);
        payOrder.CreatorUserId.ShouldBe(userId);
        payOrder.OpenId.ShouldBe(openid);
        payOrder.AppName.ShouldBe(appName);
        payOrder.AppId.ShouldBe(appid);
        payOrder.MchId.ShouldBe(mchid);
        payOrder.TenantId.ShouldBe(tenantId);
        payOrder.PayType.ShouldBe(PayType.微信);
    }

    [Fact]
    public void CreateDepositPay_WithSmallAmount_ShouldConvertCorrectly()
    {
        var payOrder = new PayOrder();
        var amount = 0.01m;

        payOrder.CreateDepositPay(amount, 1, "", "pub", "appid", "mchid", 1);

        payOrder.Total.ShouldBe(1);
    }

    [Fact]
    public void CreateDepositPay_WithLargeAmount_ShouldConvertCorrectly()
    {
        var payOrder = new PayOrder();
        var amount = 10000m;

        payOrder.CreateDepositPay(amount, 1, "", "pub", "appid", "mchid", 1);

        payOrder.Total.ShouldBe(1000000);
    }

    [Fact]
    public void CreateDepositPay_WithEmptyAppName_ShouldThrowException()
    {
        var payOrder = new PayOrder();

        Should.Throw<Abp.UI.UserFriendlyException>(() =>
            payOrder.CreateDepositPay(51m, 1, "", "", "appid", "mchid", 1));
    }

    #endregion

    #region CreateTopUpPay Tests

    [Fact]
    public void CreateTopUpPay_Should_SetCorrectProperties()
    {
        var payOrder = new PayOrder();
        var amount = 100m;

        payOrder.CreateTopUpPay(amount, 1, "openid123", "uniapp", "appid", "mchid", 1);

        payOrder.Total.ShouldBe(10000);
        payOrder.HostType.ShouldBe(OrderType.充值);
        payOrder.State.ShouldBe(PayState.未支付);
        payOrder.OpenId.ShouldBe("openid123");
    }

    #endregion

    #region SuccessPay Tests

    [Fact]
    public void SuccessPay_Should_UpdateStateAndSetTransactionId()
    {
        var payOrder = CreateTestPayOrder();
        var notifyId = "notify_12345";
        var successTime = DateTime.Now;

        payOrder.SuccessPay(notifyId, successTime);

        payOrder.State.ShouldBe(PayState.已支付);
        payOrder.IsSuccessPay.ShouldBeTrue();
        payOrder.SuccessPayTime.ShouldBe(successTime);
    }

    [Fact]
    public void SuccessPay_WithoutTime_ShouldUseCurrentTime()
    {
        var payOrder = CreateTestPayOrder();
        var beforeCall = DateTime.Now;

        payOrder.SuccessPay("notify_123", null);
        var afterCall = DateTime.Now;

        payOrder.SuccessPayTime.ShouldNotBeNull();
        payOrder.SuccessPayTime.ShouldBeGreaterThanOrEqualTo(beforeCall);
        payOrder.SuccessPayTime.ShouldBeLessThanOrEqualTo(afterCall);
    }

    #endregion

    #region Refund Tests

    [Fact]
    public void Refund_Should_SetRefundState()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);
        var refundAmount = 10m;

        payOrder.Refund(refundAmount);

        payOrder.IsRefund.ShouldBeTrue();
        payOrder.RefundTime.ShouldNotBeNull();
        payOrder.RefundPrice.ShouldBe(1000);
    }

    [Fact]
    public void Refund_WithExceedAmount_ShouldThrowException()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);
        var refundAmount = 100m;

        Should.Throw<Abp.UI.UserFriendlyException>(() =>
            payOrder.Refund(refundAmount));
    }

    [Fact]
    public void RefundComplate_WithFullAmount_ShouldSetRefundedState()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);

        payOrder.RefundComplate(5100);

        payOrder.State.ShouldBe(PayState.已退款);
        payOrder.RefundComplateTime.ShouldNotBeNull();
        payOrder.RefundPrice.ShouldBe(5100);
    }

    [Fact]
    public void RefundComplate_WithPartialAmount_ShouldSetPartialRefundState()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);

        payOrder.RefundComplate(1000);

        payOrder.State.ShouldBe(PayState.部分退款);
        payOrder.RefundPrice.ShouldBe(1000);
    }

    [Fact]
    public void RefundComplate_MultipleTimes_ShouldAccumulateAmount()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);

        payOrder.RefundComplate(1000);
        payOrder.RefundComplate(2000);

        payOrder.RefundPrice.ShouldBe(3000);
        payOrder.State.ShouldBe(PayState.部分退款);
    }

    #endregion

    #region RejectRefund Tests

    [Fact]
    public void RejectRefund_Should_ResetRefundProperties()
    {
        var payOrder = CreateTestPayOrder();
        payOrder.SuccessPay("notify_123", DateTime.Now);
        payOrder.Refund(10m);

        payOrder.RejectRefund();

        payOrder.IsRefund.ShouldBeFalse();
        payOrder.RefundPrice.ShouldBeNull();
        payOrder.RefundTime.ShouldBeNull();
    }

    #endregion

    private PayOrder CreateTestPayOrder()
    {
        var payOrder = new PayOrder();
        payOrder.CreateDepositPay(51m, 1, "", "pub", "wxfb7bd5b5f94a8805", "1669900694", 1);
        return payOrder;
    }
}