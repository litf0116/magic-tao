using System.Threading.Tasks;

namespace TtWork.Project.Services.Push;

public interface IWebPushService
{
    Task<WebPushResult> SendPushAsync(long userId, string title, string body, string icon = null, string url = null);
}

public class WebPushResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int SendCount { get; set; }

    public static WebPushResult Ok(int sendCount) => new() { Success = true, SendCount = sendCount };
    public static WebPushResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
