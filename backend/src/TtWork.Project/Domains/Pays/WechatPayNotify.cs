using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;
using TtWork.HttpClient.Weixin.Models;

namespace TtWork.Project.Domains.Pays;

[Table("Pays_WechatPaymentNotification")]
public class WechatPaymentNotification : Entity<Ulid>, IHasCreationTime {
    public DateTime CreationTime { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 6)]
    [JsonProperty("out_trade_no")]
    public string OutTradeNo { get; set; }

    [StringLength(32)]
    [JsonProperty("transaction_id")]
    public string TransactionId { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 1)]
    [JsonProperty("mchid")]
    public string MchId { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 1)]
    [JsonProperty("appid")]
    public string AppId { get; set; }

    [StringLength(64)]
    [JsonProperty("success_time")]
    public DateTime? SuccessTime { get; set; }

    // [Column(TypeName = "longtext")] public WeChatPayPaidEventModel RawData { get; set; }
    [Column(TypeName = "longtext")] public string RawData { get; set; }
}