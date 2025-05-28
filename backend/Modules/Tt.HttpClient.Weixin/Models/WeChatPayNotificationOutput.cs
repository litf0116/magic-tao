using System;
using Newtonsoft.Json;

namespace EasyAbp.Abp.WeChat.Pay.RequestHandling.Dtos;

[Serializable]
public class WeChatPayNotificationOutput {
    /// <summary>
    /// 返回状态码。
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }

    /// <summary>
    /// 返回信息。
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
}