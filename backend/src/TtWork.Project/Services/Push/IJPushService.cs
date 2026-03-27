using System.Collections.Generic;
using System.Threading.Tasks;

namespace TtWork.Project.Services.Push;

/// <summary>
/// 极光推送服务接口
/// </summary>
public interface IJPushService
{
    /// <summary>
    /// 发送广播推送（所有设备）
    /// </summary>
    Task<PushResult> BroadcastAsync(string title, string content, Dictionary<string, string> extras = null);

    /// <summary>
    /// 按注册ID发送推送
    /// </summary>
    Task<PushResult> SendByRegistrationIdAsync(string title, string content, List<string> registrationIds, Dictionary<string, string> extras = null);

    /// <summary>
    /// 按别名发送推送
    /// </summary>
    Task<PushResult> SendByAliasAsync(string title, string content, List<string> aliases, Dictionary<string, string> extras = null);

    /// <summary>
    /// 按标签发送推送
    /// </summary>
    Task<PushResult> SendByTagAsync(string title, string content, List<string> tags, Dictionary<string, string> extras = null);
}

/// <summary>
/// 推送结果
/// </summary>
public class PushResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// 发送数量
    /// </summary>
    public int SendCount { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 原始响应
    /// </summary>
    public string RawResponse { get; set; }

    public static PushResult Ok(string messageId, int sendCount, string rawResponse = null)
    {
        return new PushResult
        {
            Success = true,
            MessageId = messageId,
            SendCount = sendCount,
            RawResponse = rawResponse
        };
    }

    public static PushResult Fail(string errorMessage, string rawResponse = null)
    {
        return new PushResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            RawResponse = rawResponse
        };
    }
}