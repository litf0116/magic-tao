using System;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Json;
using Microsoft.Extensions.Logging;

namespace TtWork.Project.Jobs;

/// <summary>
/// 微信微信机器人消息
/// </summary>
/// <param name="logger"></param>
public class SendWxWorkJob(ILogger<SendWxWorkJob> logger, IHttpClientFactory httpClientFactory) : ITransientDependency {
    /// <summary>
    /// 发布markdown格式内容
    /// </summary>
    public Task SendMarkdown(string content, string key, bool continueIfError = true, string tag = "") {
        var data = new {
            msgtype = "markdown",
            markdown = new {
                content
            }
        };
        return SendNormal(data.ToJsonString(false, false), key, continueIfError, tag);
    }

    /// <summary>
    /// 自定义发布内容
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="continueIfError">发送失败是否继续下一次工作</param>
    /// <exception cref="Exception"></exception>
    public async Task SendNormal(string content, string key, bool continueIfError = true, string tag = "") {
        using var client = httpClientFactory.CreateClient("https://qyapi.weixin.qq.com");
        client.BaseAddress = new Uri("https://qyapi.weixin.qq.com");
        var c = new StringContent(content, Encoding.UTF8, "application/json");
        var responseMessage = await client.PostAsync($"https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key={key}", c);
        var str = await responseMessage.Content.ReadAsStringAsync();
        var result = str.FromJsonString<ResultDto>();

        logger.LogInformation($"[{tag}]企业微信机器人推送结果: {{@result}}", result.ToJsonString(camelCase: false, indented: false));

        if (result.errcode != 0 && result.errmsg != "ok") {
            if (continueIfError)
                throw new Exception(str);
        }
    }
}

public class ResultDto {
    public int errcode { get; set; }

    public string errmsg { get; set; }
}