using System;
using Newtonsoft.Json;

namespace TtWork.HttpClient.Weixin.Models;

[Serializable]
public class WeChatPayNotificationInput {
    /// <summary>
    /// 通知 ID。
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 通知创建时间。
    /// </summary>
    [JsonProperty("create_time")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 通知类型。
    /// </summary>
    [JsonProperty("event_type")]
    public string EventType { get; set; }

    /// <summary>
    /// 通知数据类型。
    /// </summary>
    [JsonProperty("resource_type")]
    public string ResourceType { get; set; }

    /// <summary>
    /// 回调摘要。
    /// </summary>
    [JsonProperty("summary")]
    public string Summary { get; set; }

    /// <summary>
    /// 通知数据。
    /// </summary>
    [JsonProperty("resource")]
    public ResourceModel Resource { get; set; }

    public class ResourceModel {
        /// <summary>
        /// 加密算法类型。
        /// </summary>
        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        /// <summary>
        /// 数据密文。
        /// </summary>
        [JsonProperty("ciphertext")]
        public string Ciphertext { get; set; }

        /// <summary>
        /// 附加数据。
        /// </summary>
        [JsonProperty("associated_data")]
        public string AssociatedData { get; set; }

        /// <summary>
        /// 原始类型。
        /// </summary>
        [JsonProperty("original_type")]
        public string OriginalType { get; set; }

        /// <summary>
        /// 随机串。
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }
}