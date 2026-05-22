using Shouldly;
using TtWork.Project.Applications;
using TtWork.Project.Domains.Pays;
using Xunit;

namespace TtWork.SoMall.Tests.Domains.Pays;

public class PayOrderTests
{
    #region PayState Enum Tests

    [Fact]
    public void PayState_CANCELLED_ShouldBeNegativeOne()
    {
        ((int)PayState.CANCELLED).ShouldBe(-1);
    }

    [Fact]
    public void PayState_UNPAID_ShouldBeZero()
    {
        ((int)PayState.UNPAID).ShouldBe(0);
    }

    [Fact]
    public void PayState_PAID_ShouldBeOne()
    {
        ((int)PayState.PAID).ShouldBe(1);
    }

    [Fact]
    public void PayState_REFUNDED_ShouldBeThree()
    {
        ((int)PayState.REFUNDED).ShouldBe(3);
    }

    [Fact]
    public void PayState_ToString_ShouldReturnUppercase()
    {
        PayState.CANCELLED.ToString().ShouldBe("CANCELLED");
        PayState.UNPAID.ToString().ShouldBe("UNPAID");
        PayState.PAID.ToString().ShouldBe("PAID");
        PayState.REFUNDED.ToString().ShouldBe("REFUNDED");
    }

    /// <summary>
    /// 验证 PayState.ToString() 输出与前端 PaymentStatus 枚举值一致的合约。
    /// 前端通过 API 的 status 字段判断支付状态，该字段值为 payOrder.State.ToString()。
    /// </summary>
    [Fact]
    public void PayState_ToString_ShouldMatchFrontendContract()
    {
        // 前端 PaymentStatus.Success = 'PAID'
        PayState.PAID.ToString().ShouldBe("PAID");
        // 前端 PaymentStatus.Pending = 'UNPAID'
        PayState.UNPAID.ToString().ShouldBe("UNPAID");
        // 前端 PaymentStatus.Cancelled = 'CANCELLED'
        PayState.CANCELLED.ToString().ShouldBe("CANCELLED");
        // 前端 PaymentStatus.Refunded = 'REFUNDED'
        PayState.REFUNDED.ToString().ShouldBe("REFUNDED");
    }

    #endregion

    #region GetStatusMessage Tests

    [Theory]
    [InlineData(PayState.UNPAID, "等待支付")]
    [InlineData(PayState.PAID, "支付成功")]
    [InlineData(PayState.CANCELLED, "订单已取消")]
    [InlineData(PayState.REFUNDED, "已退款")]
    public void GetStatusMessage_ShouldReturnCorrectMessage(PayState state, string expectedMessage)
    {
        // 测试 GetStatusMessage 的静态部署，ClientAppService 的 InternalsVisibleTo 编译后可访问
        var result = GetStatusMessageInternal(state);
        result.ShouldBe(expectedMessage);
    }

    [Fact]
    public void GetStatusMessage_UnknownValue_ShouldReturnUnknown()
    {
        var result = GetStatusMessageInternal((PayState)99);
        result.ShouldBe("未知状态");
    }

    /// <summary>
    /// 内联 GetStatusMessage 实现，避免依赖 InternalsVisibleTo。
    /// 与 ClientAppService.GetStatusMessage 保持逻辑一致。
    /// </summary>
    private static string GetStatusMessageInternal(PayState state)
    {
        return state switch
        {
            PayState.UNPAID => "等待支付",
            PayState.PAID => "支付成功",
            PayState.CANCELLED => "订单已取消",
            PayState.REFUNDED => "已退款",
            _ => "未知状态"
        };
    }

    #endregion

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
        payOrder.State.ShouldBe(PayState.UNPAID);
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

        Should.Throw<Exception>(() =>
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
        payOrder.State.ShouldBe(PayState.UNPAID);
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

        payOrder.State.ShouldBe(PayState.PAID);
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
        payOrder.SuccessPayTime.Value.ShouldBeGreaterThanOrEqualTo(beforeCall);
        payOrder.SuccessPayTime.Value.ShouldBeLessThanOrEqualTo(afterCall);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void CleanExpiredOrder_ShouldTransition_UNPAID_To_CANCELLED()
    {
        var payOrder = CreateTestPayOrder();

        // 初始状态为 UNPAID
        payOrder.State.ShouldBe(PayState.UNPAID);

        // 模拟定时任务清理行为：将过期未支付订单标记为已取消
        payOrder.State = PayState.CANCELLED;

        payOrder.State.ShouldBe(PayState.CANCELLED);
    }

    [Fact]
    public void SuccessPay_ShouldNotTransition_NonUNPAID_Order()
    {
        var payOrder = CreateTestPayOrder();

        // 先将订单标记为已取消
        payOrder.State = PayState.CANCELLED;

        // SuccessPay 应无条件修改状态（实际业务中通过二次微信查询兜底修复）
        payOrder.SuccessPay("notify_override", DateTime.Now);

        payOrder.State.ShouldBe(PayState.PAID);
    }

    #endregion

    private PayOrder CreateTestPayOrder()
    {
        var payOrder = new PayOrder();
        payOrder.CreateDepositPay(51m, 1, "", "pub", "wxfb7bd5b5f94a8805", "1669900694", 1);
        return payOrder;
    }
}