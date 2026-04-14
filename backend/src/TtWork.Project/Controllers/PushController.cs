using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Project.Domains;
using TtWork.Project.Services.Push;

namespace TtWork.Project.Controllers;

public class PushSubscriptionInput
{
    public string Endpoint { get; set; }
    public PushKeys Keys { get; set; }
    public string DeviceName { get; set; }
}

public class PushKeys
{
    public string P256Dh { get; set; }
    public string Auth { get; set; }
}

[Route("api/push")]
[DisableAuditing]
[AbpAuthorize]
public class PushController : AbpController
{
    private readonly IRepository<PushSubscription, long> _pushSubscriptionRepository;

    public PushController(IRepository<PushSubscription, long> pushSubscriptionRepository)
    {
        _pushSubscriptionRepository = pushSubscriptionRepository;
    }

    [HttpPost("subscribe")]
    public async Task<object> Subscribe([FromBody] PushSubscriptionInput input)
    {
        if (string.IsNullOrEmpty(input.Endpoint) || input.Keys == null)
        {
            return new { success = false, message = "无效的订阅信息" };
        }

        var userId = AbpSession.UserId;
        if (!userId.HasValue)
        {
            return new { success = false, message = "请先登录" };
        }

        var existing = await _pushSubscriptionRepository.GetAll()
            .FirstOrDefaultAsync(x => x.Endpoint == input.Endpoint);

        if (existing != null)
        {
            existing.P256Dh = input.Keys.P256Dh;
            existing.Auth = input.Keys.Auth;
            existing.DeviceName = input.DeviceName;
            await _pushSubscriptionRepository.UpdateAsync(existing);
            return new { success = true, message = "更新成功" };
        }

        var subscription = new PushSubscription
        {
            UserId = userId.Value,
            Endpoint = input.Endpoint,
            P256Dh = input.Keys.P256Dh,
            Auth = input.Keys.Auth,
            DeviceName = input.DeviceName
        };

        await _pushSubscriptionRepository.InsertAsync(subscription);
        return new { success = true, message = "订阅成功" };
    }

    [HttpDelete("unsubscribe")]
    public async Task<object> Unsubscribe([FromBody] PushSubscriptionInput input)
    {
        if (string.IsNullOrEmpty(input.Endpoint))
        {
            return new { success = false, message = "无效的订阅信息" };
        }

        var userId = AbpSession.UserId;
        if (!userId.HasValue)
        {
            return new { success = false, message = "请先登录" };
        }

        var subscription = await _pushSubscriptionRepository.GetAll()
            .FirstOrDefaultAsync(x => x.Endpoint == input.Endpoint && x.UserId == userId.Value);

        if (subscription != null)
        {
            await _pushSubscriptionRepository.DeleteAsync(subscription);
            return new { success = true, message = "取消订阅成功" };
        }

        return new { success = false, message = "未找到订阅记录" };
    }
}
