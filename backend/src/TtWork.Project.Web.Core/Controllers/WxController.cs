using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Json;
using Abp.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TtWork.Lib;
using TtWork.Lib.Redis;
using TtWork.Project.Controllers;
using TTWork.WeiXinMiddleware;

namespace TtWork.Project.Web.Controllers;

public class WxController(
    ILogger<WxController> logger,
    IHttpContextAccessor httpContextAccessor,
    TokenAuthController tokenAuthController,
    IRedisClient redisClient
) : AbpControllerBase {
    private static readonly XmlSerializer _XmlSerializer = new(typeof(WeiXinMessage));

    [DontWrapResult]
    [HttpGet("/api/wx")]
    public async Task<string> GetAsync(string signature, string timestamp, string nonce, string echostr) {
        logger.LogDebug(
            $"HttpGet /api/wx signature:{signature}, timestamp:{timestamp}, nonce:{nonce}, echostr:{echostr}");

        if (CheckSignature(signature, timestamp, nonce, "molitao")) {
            logger.LogDebug("验证成功");
            return echostr;
        }

        logger.LogError("验证失败");
        return "false";
    }

    [DontWrapResult]
    [HttpPost("/api/wx")]
    public async Task PostAsync(string signature, string timestamp, string nonce, string openid,
        string encrypt_type, string msg_signature) {
        logger.LogDebug(
            $"HttpPost /api/wx signature:{signature}, timestamp:{timestamp}, nonce:{nonce}, openid:{openid}, encrypt_type:{encrypt_type}, msg_signature:{msg_signature}");

#if DEBUG

#else
        if (!CheckSignature(signature, timestamp, nonce, "molitao")) {
            logger.LogError("验证失败");
            await httpContextAccessor!.HttpContext!.Response.WriteAsync("验证失败");
            return;
        }
#endif
        using StreamReader stream = new StreamReader(httpContextAccessor.HttpContext!.Request.Body);
        var body = await stream.ReadToEndAsync();

        var weiXinMessage = _XmlSerializer.Deserialize(ClearXmlHeader(body)) as WeiXinMessage;
        if (!(weiXinMessage.MsgType == "event"))
            await OnRecieveMessage(weiXinMessage, httpContextAccessor!.HttpContext!);
        logger.LogInformation("{@body}", body);
        var weiXinContext = new WeiXinContext(weiXinMessage, httpContextAccessor!.HttpContext!, []);
        var lower = weiXinMessage.Event.ToLower();

        var str2 = lower.First<char>().ToString().ToUpper() + lower.Substring(1);
        var method = GetType().GetMethod("On" + str2);
        if (!(method != (MethodInfo)null)) {
            await Task.Delay(0);
        }

        var task = (Task)method.Invoke(this, [weiXinContext]);
        if (task != null) await task;
    }

    public async Task OnUnsubscribe(WeiXinContext context) {
        await context.HttpContext.Response.WriteAsync("bye bye");
    }

    public async Task OnClick(WeiXinContext context) {
        await context.HttpContext.Response.WriteAsync("OnClick");
    }

    public async Task OnView(WeiXinContext context) {
        await context.HttpContext.Response.WriteAsync("OnView");
    }

    public async Task OnLocation(WeiXinContext context) {
        await context.HttpContext.Response.WriteAsync("OnLocation");
    }


    //生成TOKEN到redis
    private async Task<(bool, string)> DoOpenidAuth(string openid, string state) {
        const string qrTokenKey = "Molitao:QrToken:";
        var cache = await redisClient.Database.StringGetAsync(qrTokenKey + state);
        if (cache.HasValue) {
            return (true, "");
        }

        try {
            var result = await tokenAuthController.WeixinPubAuthenticate(openid);
            if (result != null)
                await redisClient.Database.StringSetAsync((RedisKey)(qrTokenKey + state), result.AccessToken,
                    TimeSpan.FromHours(1));
            return (true, "");
        }
        catch (Exception e) {
            return (false, e.Message);
        }
    }


    public async Task OnSubscribe(WeiXinContext context) {
        var textResult = "欢迎关注我们的公众号";
        if (!context.EventKey.IsNullOrEmptyOrWhiteSpace()) {
            await DoOpenidAuth(context.FromUserName, context.EventKey.Replace("qrscene_", ""));
        }

        var resultText = $@"<xml>
<ToUserName><![CDATA[{context.FromUserName}]]></ToUserName>
<FromUserName><![CDATA[{context.ToUserName}]]></FromUserName>
<CreateTime>{context.CreateTime}</CreateTime>
<MsgType><![CDATA[text]]></MsgType>
<Content><![CDATA[{textResult}]]></Content>
</xml>";
        context.HttpContext.Response.StatusCode = 200;
        context.HttpContext.Response.ContentType = "text/xml";
        await context.HttpContext.Response.WriteAsync(resultText);
    }

    public async Task OnScan(WeiXinContext context) {
        var textResult = "扫码登录";

        var result = await DoOpenidAuth(context.FromUserName, context.EventKey);

        var resultText = $@"<xml>
<ToUserName><![CDATA[{context.FromUserName}]]></ToUserName>
<FromUserName><![CDATA[{context.ToUserName}]]></FromUserName>
<CreateTime>{context.CreateTime}</CreateTime>
<MsgType><![CDATA[text]]></MsgType>
<Content><![CDATA[{(result.Item1 ? textResult : result.Item2)}]]></Content>
</xml>";

        context.HttpContext.Response.StatusCode = 200;
        context.HttpContext.Response.ContentType = "text/xml";
        await context.HttpContext.Response.WriteAsync(resultText);
    }

    private async Task OnRecieveMessage(WeiXinMessage weiXinMessage, HttpContext httpContext) {
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/xml";
        var textResult = $"消息已收到";
        var resultText = $"""
                          <xml>
                              <ToUserName><![CDATA[{weiXinMessage.FromUserName}]]></ToUserName>
                              <FromUserName><![CDATA[{weiXinMessage.ToUserName}]]></FromUserName>
                              <CreateTime>{weiXinMessage.CreateTime}</CreateTime>
                              <MsgType><![CDATA[text]]></MsgType>
                              <Content><![CDATA[{textResult}]]></Content>
                          </xml>
                          """;
        await httpContext.Response.WriteAsync(resultText);
    }


    /// <summary>
    /// signature=bf7a3e6826a50df25062e89b50195e27898fdb7f&echostr=324517259902697075&timestamp=1710996003&nonce=605654171
    /// </summary>
    private static bool CheckSignature(string signature, string timestamp, string nonce, string token) {
        List<string> tmpArr = [token, timestamp, nonce];
        tmpArr.Sort();
        var hashBytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Join("", tmpArr)));
        StringBuilder builder = new();
        foreach (var b in hashBytes) {
            builder.Append(b.ToString("x2"));
        }

        var result = builder.ToString();
        return result.Equals(signature, System.StringComparison.CurrentCultureIgnoreCase);
    }

    private string ClearXmlHeader(string input) {
        return Regex.Replace(input, "<\\?xml([^>]+)\\?>", string.Empty, RegexOptions.IgnoreCase);
    }
}