using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications.Auth.Dto;

/// <summary>
/// 生成二维码响应
/// </summary>
public class QrCodeGenerateOutputDto
{
    /// <summary>
    /// 二维码code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 二维码内容 (H5 URL格式，支持微信扫一扫和App扫码)
    /// </summary>
    public string QrContent { get; set; }

    /// <summary>
    /// 有效期（秒）
    /// </summary>
    public int ExpiresIn { get; set; }
}

/// <summary>
/// 扫码获取用户信息响应
/// </summary>
public class QrCodeUserInfoDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 手机号（脱敏: 138****1234）
    /// </summary>
    public string Phone { get; set; }
}

/// <summary>
/// 确认登录请求
/// </summary>
public class ConfirmLoginInputDto
{
    /// <summary>
    /// 二维码code
    /// </summary>
    [Required]
    public string Code { get; set; }
}

/// <summary>
/// 轮询状态响应
/// </summary>
public class QrCodeStatusDto
{
    /// <summary>
    /// 状态 (pending/scanned/confirmed/expired)
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 用户信息（仅 confirmed 时返回）
    /// </summary>
    public QrCodeUserInfoDto User { get; set; }
}

/// <summary>
/// 登录结果响应
/// </summary>
public class QrCodeLoginResultDto
{
    /// <summary>
    /// Token
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Token类型 (Bearer)
    /// </summary>
    public string TokenType { get; set; }

    /// <summary>
    /// 有效期（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public QrCodeUserInfoDto User { get; set; }
}
