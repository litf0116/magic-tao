using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TtWork.HttpClient.Weixin.Models;

/// <summary>
/// 微信支付 Native 下单请求模型。
/// 用于生成扫码支付二维码。
/// </summary>
public class CreateNativeOrderRequest {
    /// <summary>
    /// 应用 ID。
    /// </summary>
    /// <remarks>
    /// 由微信生成的应用 ID，全局唯一。请求基础下单接口时请注意 APPID 的应用属性。<br/>
    /// 例如公众号场景下，需使用应用属性为公众号的服务号 APPID。
    /// </remarks>
    /// <example>
    /// 示例值：wxd678efh567hg6787。
    /// </example>
    [Required]
    [StringLength(32, MinimumLength = 1)]
    [JsonProperty("appid")]
    public string AppId { get; set; }

    /// <summary>
    /// 直连商户号。
    /// </summary>
    /// <remarks>
    /// 直连商户的商户号，由微信支付生成并下发。
    /// </remarks>
    /// <example>
    /// 示例值：1900000109。
    /// </example>
    [Required]
    [StringLength(32, MinimumLength = 1)]
    [JsonProperty("mchid")]
    public string MchId { get; set; }

    /// <summary>
    /// 商品描述。
    /// </summary>
    /// <remarks>
    /// 商品描述。
    /// </remarks>
    /// <example>
    /// 示例值：Image 形象店 - 深圳腾大-QQ 公仔。
    /// </example>
    [Required]
    [StringLength(127, MinimumLength = 1)]
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// 商户订单号。
    /// </summary>
    /// <remarks>
    /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一。
    /// </remarks>
    /// <example>
    /// 示例值：20150806125346。
    /// </example>
    [Required]
    [StringLength(32, MinimumLength = 6)]
    [JsonProperty("out_trade_no")]
    public string OutTradeNo { get; set; }

    /// <summary>
    /// 交易结束时间。
    /// </summary>
    /// <remarks>
    /// 订单失效时间，遵循 rfc3339 标准格式，格式为 YYYY-MM-DDTHH:mm:ss+TIMEZONE。<br/>
    /// 开发人员只需要传递 DateTime 对象，底层的 Newtonsoft.Json 库会自动将其转换为符合微信支付要求的格式。
    /// </remarks>
    /// <example>
    /// 示例值：2018-06-08T10:34:56+08:00
    /// </example>
    [JsonProperty("time_expire")]
    public DateTime? TimeExpire { get; set; }

    /// <summary>
    /// 附加数据。
    /// </summary>
    /// <remarks>
    /// 附加数据，在查询 API 和支付通知中原样返回，可作为自定义参数使用，实际情况下只有支付完成状态才会返回该字段。
    /// </remarks>
    /// <example>
    /// 示例值：自定义数据  
    /// </example>
    [JsonProperty("attach")]
    [StringLength(128, MinimumLength = 1)]
    public string Attach { get; set; }

    /// <summary>
    /// 通知地址。
    /// </summary>
    /// <remarks>
    /// 异步接收微信支付结果通知的回调地址，通知 URL 必须为外网可访问的 URL，不能携带参数。<br/>
    /// 公网域名必须为 HTTPS，如果是走专线接入，使用专线 NAT IP 或者私有回调域名可使用 HTTP。
    /// </remarks>
    /// <example>
    /// 示例值：https://www.weixin.qq.com/wxpay/pay.php
    /// </example>
    [Required]
    [StringLength(256, MinimumLength = 1)]
    [JsonProperty("notify_url")]
    public string NotifyUrl { get; set; }

    /// <summary>
    /// 订单金额信息。
    /// </summary>
    [Required]
    [JsonProperty("amount")]
    public CreateOrderAmountModel Amount { get; set; }
}
