using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

/// <summary>
/// H5 WebPush 订阅信息
/// </summary>
[Table("T_PushSubscription")]
public class PushSubscription : CreationAuditedEntity<long>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Column("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// 订阅端点 URL
    /// </summary>
    [Column("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    /// P256 DH 公钥 (Base64)
    /// </summary>
    [Column("p256dh")]
    public string P256Dh { get; set; }

    /// <summary>
    /// 认证密钥 (Base64)
    /// </summary>
    [Column("auth")]
    public string Auth { get; set; }

    /// <summary>
    /// 设备名称 (可选)
    /// </summary>
    [Column("device_name")]
    public string DeviceName { get; set; }
}
