using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using Jiguang.JPush;
using Jiguang.JPush.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JPushOptions = Jiguang.JPush.Model.Options;

namespace TtWork.Project.Services.Push;

public class JPushSettings
{
    public string AppKey { get; set; }
    public string MasterSecret { get; set; }
    public bool IsProduction { get; set; }
}

public class JPushService : IJPushService, ITransientDependency
{
    private readonly JPushClient _client;
    private readonly bool _isProduction;
    private readonly ILogger<JPushService> _logger;

    public JPushService(IOptions<JPushSettings> options, ILogger<JPushService> logger)
    {
        var opt = options.Value;
        _client = new JPushClient(opt.AppKey, opt.MasterSecret);
        _isProduction = opt.IsProduction;
        _logger = logger;
    }

    public async Task<PushResult> BroadcastAsync(string title, string content, Dictionary<string, string> extras = null)
    {
        try
        {
            var payload = new PushPayload
            {
                Platform = new List<string> { "android", "ios" },
                Audience = "all",
                Notification = BuildNotification(title, content, extras),
                Options = new JPushOptions
                {
                    IsApnsProduction = _isProduction
                }
            };

            var response = await _client.SendPushAsync(payload);
            return HandleResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "广播推送失败: {Title} - {Content}", title, content);
            return PushResult.Fail(ex.Message);
        }
    }

    public async Task<PushResult> SendByRegistrationIdAsync(string title, string content, List<string> registrationIds, Dictionary<string, string> extras = null)
    {
        if (registrationIds == null || registrationIds.Count == 0)
        {
            return PushResult.Fail("注册ID列表不能为空");
        }

        try
        {
            var payload = new PushPayload
            {
                Platform = new List<string> { "android", "ios" },
                Audience = new Audience
                {
                    RegistrationId = registrationIds
                },
                Notification = BuildNotification(title, content, extras),
                Options = new JPushOptions
                {
                    IsApnsProduction = _isProduction
                }
            };

            var response = await _client.SendPushAsync(payload);
            return HandleResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按注册ID推送失败: {RegistrationIds}", string.Join(",", registrationIds));
            return PushResult.Fail(ex.Message);
        }
    }

    public async Task<PushResult> SendByAliasAsync(string title, string content, List<string> aliases, Dictionary<string, string> extras = null)
    {
        if (aliases == null || aliases.Count == 0)
        {
            return PushResult.Fail("别名列表不能为空");
        }

        try
        {
            var payload = new PushPayload
            {
                Platform = new List<string> { "android", "ios" },
                Audience = new Audience
                {
                    Alias = aliases
                },
                Notification = BuildNotification(title, content, extras),
                Options = new JPushOptions
                {
                    IsApnsProduction = _isProduction
                }
            };

            var response = await _client.SendPushAsync(payload);
            return HandleResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按别名推送失败: {Aliases}", string.Join(",", aliases));
            return PushResult.Fail(ex.Message);
        }
    }

    public async Task<PushResult> SendByTagAsync(string title, string content, List<string> tags, Dictionary<string, string> extras = null)
    {
        if (tags == null || tags.Count == 0)
        {
            return PushResult.Fail("标签列表不能为空");
        }

        try
        {
            var payload = new PushPayload
            {
                Platform = new List<string> { "android", "ios" },
                Audience = new Audience
                {
                    Tag = tags
                },
                Notification = BuildNotification(title, content, extras),
                Options = new JPushOptions
                {
                    IsApnsProduction = _isProduction
                }
            };

            var response = await _client.SendPushAsync(payload);
            return HandleResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按标签推送失败: {Tags}", string.Join(",", tags));
            return PushResult.Fail(ex.Message);
        }
    }

    private Notification BuildNotification(string title, string content, Dictionary<string, string> extras)
    {
        var extrasDict = new Dictionary<string, object>();
        if (extras != null)
        {
            foreach (var kvp in extras)
            {
                extrasDict[kvp.Key] = kvp.Value;
            }
        }

        return new Notification
        {
            Alert = content,
            Android = new Android
            {
                Alert = content,
                Title = title,
                Extras = extrasDict
            },
            IOS = new IOS
            {
                Alert = content,
                Sound = "default",
                Badge = "+1",
                Extras = extrasDict
            }
        };
    }

    private PushResult HandleResponse(HttpResponse response)
    {
        if ((int)response.StatusCode == 200)
        {
            var result = response.Content;
            _logger.LogInformation("推送成功: {Response}", result);
            
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(result);
                var msgId = json.RootElement.GetProperty("msg_id").GetString();
                var sendno = json.RootElement.GetProperty("sendno").GetInt32();
                return PushResult.Ok(msgId, sendno, result);
            }
            catch
            {
                return PushResult.Ok("unknown", 0, result);
            }
        }
        else
        {
            _logger.LogWarning("推送失败: {StatusCode} - {Response}", response.StatusCode, response.Content);
            return PushResult.Fail($"HTTP {(int)response.StatusCode}", response.Content);
        }
    }
}