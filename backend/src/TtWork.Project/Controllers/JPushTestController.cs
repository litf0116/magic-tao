using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;
using TtWork.Project.Services.Push;

namespace TtWork.Project.Controllers;

public class PushTestInput
{
    public string Title { get; set; } = "测试推送";
    public string Content { get; set; } = "这是一条测试消息";
    public List<string> RegistrationIds { get; set; }
    public List<string> Aliases { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, string> Extras { get; set; }
}

[Route("api/test/push")]
[DisableAuditing]
public class JPushTestController : AbpController
{
    private readonly IJPushService _jPushService;

    public JPushTestController(IJPushService jPushService)
    {
        _jPushService = jPushService;
    }

    [HttpPost("send")]
    public async Task<object> SendPush([FromBody] PushTestInput input)
    {
        PushResult result;

        if (input.RegistrationIds != null && input.RegistrationIds.Count > 0)
        {
            result = await _jPushService.SendByRegistrationIdAsync(
                input.Title, 
                input.Content, 
                input.RegistrationIds, 
                input.Extras);
        }
        else if (input.Aliases != null && input.Aliases.Count > 0)
        {
            result = await _jPushService.SendByAliasAsync(
                input.Title, 
                input.Content, 
                input.Aliases, 
                input.Extras);
        }
        else if (input.Tags != null && input.Tags.Count > 0)
        {
            result = await _jPushService.SendByTagAsync(
                input.Title, 
                input.Content, 
                input.Tags, 
                input.Extras);
        }
        else
        {
            result = await _jPushService.BroadcastAsync(
                input.Title, 
                input.Content, 
                input.Extras);
        }

        return new
        {
            success = result.Success,
            messageId = result.MessageId,
            sendCount = result.SendCount,
            errorMessage = result.ErrorMessage,
            rawResponse = result.RawResponse
        };
    }

    [HttpPost("broadcast")]
    public async Task<object> Broadcast([FromBody] PushTestInput input)
    {
        var result = await _jPushService.BroadcastAsync(input.Title, input.Content, input.Extras);
        
        return new
        {
            success = result.Success,
            messageId = result.MessageId,
            sendCount = result.SendCount,
            errorMessage = result.ErrorMessage,
            rawResponse = result.RawResponse
        };
    }

    [HttpPost("send-by-registration-id")]
    public async Task<object> SendByRegistrationId([FromBody] PushTestInput input)
    {
        if (input.RegistrationIds == null || input.RegistrationIds.Count == 0)
        {
            return new
            {
                success = false,
                errorMessage = "请提供 RegistrationIds"
            };
        }

        var result = await _jPushService.SendByRegistrationIdAsync(
            input.Title, 
            input.Content, 
            input.RegistrationIds, 
            input.Extras);
        
        return new
        {
            success = result.Success,
            messageId = result.MessageId,
            sendCount = result.SendCount,
            errorMessage = result.ErrorMessage,
            rawResponse = result.RawResponse
        };
    }

    [HttpPost("send-by-alias")]
    public async Task<object> SendByAlias([FromBody] PushTestInput input)
    {
        if (input.Aliases == null || input.Aliases.Count == 0)
        {
            return new
            {
                success = false,
                errorMessage = "请提供 Aliases"
            };
        }

        var result = await _jPushService.SendByAliasAsync(
            input.Title, 
            input.Content, 
            input.Aliases, 
            input.Extras);
        
        return new
        {
            success = result.Success,
            messageId = result.MessageId,
            sendCount = result.SendCount,
            errorMessage = result.ErrorMessage,
            rawResponse = result.RawResponse
        };
    }
}