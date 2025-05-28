namespace TtWork.HttpClient.Weixin.Models;

public class WeChatRequestHandlingResult
{
    public bool Success { get; set; }

    public string FailureReason { get; set; }

    public WeChatRequestHandlingResult()
    {
    }

    public WeChatRequestHandlingResult(bool success, string failureReason = null)
    {
        Success = success;
        FailureReason = failureReason;
    }
}