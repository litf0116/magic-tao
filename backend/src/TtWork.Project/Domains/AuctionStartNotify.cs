using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

[Table("T_AuctionStartNotify")]
public class AuctionStartNotify : CreationAuditedEntity<long>
{
    public long AuctionItemId { get; set; }

    /// <summary>
    /// 用户ID（用于别名推送）
    /// </summary>
    [Column("user_id")]
    public long? UserId { get; set; }

    /// <summary>
    /// 微信 openid（小程序端）
    /// </summary>
    [Column("openid")]
    public string OpenId { get; set; }

    /// <summary>
    /// 平台标识：miniprogram / app
    /// </summary>
    [Column("platform")]
    public string Platform { get; set; }
}