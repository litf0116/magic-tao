namespace TtWork.Project.Services.Push;

/// <summary>
/// WebPush 配置
/// </summary>
public class WebPushSettings
{
    /// <summary>
    /// VAPID 公钥
    /// </summary>
    public string VapidPublicKey { get; set; }

    /// <summary>
    /// VAPID 私钥
    /// </summary>
    public string VapidPrivateKey { get; set; }

    /// <summary>
    /// VAPID Subject (mailto: 或 https://)
    /// </summary>
    public string VapidSubject { get; set; }
}