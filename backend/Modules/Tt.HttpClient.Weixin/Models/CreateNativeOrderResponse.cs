using Newtonsoft.Json;
using Tt.HttpClient.Weixin.ParametersModel;

namespace TtWork.HttpClient.Weixin.Models;

/// <summary>
/// 微信支付 Native 下单响应模型。
/// 返回二维码链接用于生成支付二维码。
/// </summary>
public class CreateNativeOrderResponse : WeChatPayCommonErrorResponse {
    /// <summary>
    /// 二维码链接。
    /// </summary>
    /// <remarks>
    /// 用于生成支付二维码的 URL 链接，用户扫码后可进行支付。<br/>
    /// 该链接有效期为 2 小时。
    /// </remarks>
    /// <example>
    /// 示例值：weixin://wxpay/bizpayurl?pr=xxxxx
    /// </example>
    [JsonProperty("code_url")]
    public string CodeUrl { get; set; }
}
