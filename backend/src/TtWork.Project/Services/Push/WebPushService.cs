using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Push;

public class WebPushService : IWebPushService, ITransientDependency
{
    private readonly ILogger<WebPushService> _logger;
    private readonly IRepository<PushSubscription, long> _pushSubscriptionRepository;

    private const string VapidPublicKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEwFIhGozygJ_yRTL6h3HQFXyCtD4xsJaZ-H9W2vu8ejKt3iWz4dvGdKnR1mnWHaT4msQmT4vblTr0_5H4Xmrp6g";
    private const string VapidPrivateKey = "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQglIpGnoJLAumDMTMT-bvhvi_iUrzfgani9WfaZlZjaWhRANCAATAUiEajPKAn_JFMvqHcdAVfIK0PjGwlpn4f1ba-7x6Mq3eJbPh28Z0qdHWadYdpPiaxCZPi9uVOvT_kfheaunq";
    private const string VapidSubject = "mailto:admin@molitao.top";

    public WebPushService(
        ILogger<WebPushService> logger,
        IRepository<PushSubscription, long> pushSubscriptionRepository)
    {
        _logger = logger;
        _pushSubscriptionRepository = pushSubscriptionRepository;
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
            var vapidDetails = new WebPush.VapidDetails(VapidSubject, VapidPublicKey, VapidPrivateKey);

            int successCount = 0;
            foreach (var sub in subscriptions)
            {
                try
                {
                    var subscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256Dh, sub.Auth);
                    var payload = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        title,
                        body,
                        icon = icon ?? "/images/logo.png",
                        url = url ?? "/pages/tabbar/index"
                    });

                    await pushClient.SendNotificationAsync(subscription, payload, vapidDetails);
                    successCount++;
                    _logger.LogInformation("[WebPush] 发送成功: {Endpoint}", sub.Endpoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WebPush] 发送失败: {Endpoint}", sub.Endpoint);
                }
            }

            return WebPushResult.Ok(successCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WebPush] 发送异常: UserId={UserId}", userId);
            return WebPushResult.Fail(ex.Message);
        }
    }
}
