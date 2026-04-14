using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Web.Core.Models.TokenAuth;

/// <summary>
/// 微信开放平台移动应用认证模型
/// </summary>
public class WeixinAppAuthenticateModel
{
    /// <summary>
    /// 微信授权码（通过微信登录 SDK 获取）- 旧方式
    /// </summary>
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型（android 或 ios）
    /// </summary>
    public string Platform { get; set; } = "android";

    /// <summary>
    /// 微信返回的 access_token（UniApp uni.login 直接返回）
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 微信返回的 openid
    /// </summary>
    public string Openid { get; set; } = string.Empty;

    /// <summary>
    /// 微信返回的 unionid
    /// </summary>
    public string Unionid { get; set; } = string.Empty;
}