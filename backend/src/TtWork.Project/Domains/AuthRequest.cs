using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

/// <summary>
/// 扫码登录授权请求
/// </summary>
[Table("T_AuthRequest")]
public class AuthRequest : AuditedEntity<long>
{
    /// <summary>
    /// 二维码code，32位随机字符串
    /// </summary>
    [StringLength(64)]
    [Required]
    public string Code { get; set; }

    /// <summary>
    /// PC端用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AuthRequestStatus Status { get; set; }

    /// <summary>
    /// 扫描时间
    /// </summary>
    public DateTime? ScannedAt { get; set; }

    /// <summary>
    /// 确认时间
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 检查是否有效（待扫描且未过期）
    /// </summary>
    public bool IsValid() => Status == AuthRequestStatus.Pending && DateTime.Now < ExpiresAt;

    /// <summary>
    /// 标记为已扫描
    /// </summary>
    public void MarkAsScanned()
    {
        Status = AuthRequestStatus.Scanned;
        ScannedAt = DateTime.Now;
    }

    /// <summary>
    /// 标记为已确认
    /// </summary>
    public void MarkAsConfirmed()
    {
        Status = AuthRequestStatus.Confirmed;
        ConfirmedAt = DateTime.Now;
    }

    /// <summary>
    /// 标记为已过期
    /// </summary>
    public void MarkAsExpired()
    {
        Status = AuthRequestStatus.Expired;
    }
}

/// <summary>
/// 授权请求状态
/// </summary>
public enum AuthRequestStatus
{
    /// <summary>
    /// 待扫描
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已扫描待确认
    /// </summary>
    Scanned = 1,

    /// <summary>
    /// 已确认
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 3
}
