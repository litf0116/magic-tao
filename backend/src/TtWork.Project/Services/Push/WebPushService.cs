using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Push;

public class WebPushService : IWebPushService, ITransientDependency
{
    private readonly ILogger<WebPushService> _logger;
    private readonly IRepository<PushSubscription, long> _pushSubscriptionRepository;
    private readonly WebPushSettings _settings;

    public WebPushService(
        ILogger<WebPushService> logger,
        IRepository<PushSubscription, long> pushSubscriptionRepository,
        IOptions<WebPushSettings> webPushOptions)
    {
        _logger = logger;
        _pushSubscriptionRepository = pushSubscriptionRepository;
        _settings = webPushOptions.Value;
    }

    public async Task<WebPushResult> SendPushAsync(long userId, string title, string body, string icon = null, string url = null)
    {
        try
        {
            var subscriptions = await _pushSubscriptionRepository.GetAll()
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (subscriptions.Count == 0)
            {
                _logger.LogWarning("[WebPush] 用户 {UserId} 没有订阅记录", userId);
                return WebPushResult.Fail("没有订阅记录");
            }

            var pushClient = new WebPush.WebPushClient();
            var vapidDetails = new WebPush.VapidDetails(_settings.VapidSubject, _settings.VapidPublicKey, _settings.VapidPrivateKey);

            int successCount = 0;
            int failureCount = 0;
            foreach (var sub in subscriptions)
            {
                var endpoint = sub.Endpoint;
                try
                {
                    var subscription = new WebPush.PushSubscription(endpoint, sub.P256Dh, sub.Auth);
                    var payload = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        title,
                        body,
                        icon = icon ?? "/images/logo.png",
                        url = url ?? "/pages/tabbar/index"
                    });

                    await pushClient.SendNotificationAsync(subscription, payload, vapidDetails);
                    successCount++;
                    _logger.LogInformation("[WebPush] 发送成功: {Endpoint}", endpoint);
                }
                catch (WebPush.WebPushException ex)
                {
                    var statusCode = ex.StatusCode;
                    if (statusCode == HttpStatusCode.Gone || statusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("[WebPush] 订阅已失效，删除: {Endpoint}", endpoint);
                        await _pushSubscriptionRepository.DeleteAsync(s => s.Endpoint == endpoint);
                    }
                    else if (statusCode == HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("[WebPush] 限流: {Endpoint}", endpoint);
                        failureCount++;
                    }
                    else
                    {
                        _logger.LogError(ex, "[WebPush] 发送失败: {Endpoint}, StatusCode={StatusCode}", endpoint, statusCode);
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WebPush] 发送异常: {Endpoint}", endpoint);
                    failureCount++;
                }
            }

            return WebPushResult.Ok(successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WebPush] 发送异常: UserId={UserId}", userId);
            return WebPushResult.Fail(ex.Message);
        }
    }
}