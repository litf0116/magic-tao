using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Web.Core.Models.TokenAuth;

/// <summary>
/// 微信开放平台移动应用认证模型
/// </summary>
public class WeixinAppAuthenticateModel
{
    /// <summary>
    /// 微信授权码（通过微信登录 SDK 获取）
    /// </summary>
    [Required(ErrorMessage = "授权码不能为空")]
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型（android 或 ios）
    /// </summary>
    public string Platform { get; set; } = "android";
}