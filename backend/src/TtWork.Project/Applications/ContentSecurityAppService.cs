using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TtWork.Abp.DomianServices.Weixin;
using TtWork.HttpClient.Weixin;
using TtWork.HttpClient.Weixin.WeixiinResult;

namespace TtWork.Project.Applications;

/// <summary>
/// 内容安全检查服务
/// </summary>
//[AbpAuthorize]
[Route("api/ContentSecurity")]
public class ContentSecurityAppService : AbpController
{
    /// <summary>
    /// 微信小程序静态配置
    /// </summary>
    private static readonly string WechatAppId = "wx8178f2258942133d";
    private static readonly string WechatAppSecret = "ec39ddccf124f18474738f15cb57a38e";

    private readonly WeixinManger _weixinManger;
    private readonly IWeixinApi _weixinApi;
    private readonly ILogger<ContentSecurityAppService> _logger;
    private readonly System.Net.Http.HttpClient _httpClient;

    public ContentSecurityAppService(
        WeixinManger weixinManger,
        IWeixinApi weixinApi,
        ILogger<ContentSecurityAppService> logger,
        System.Net.Http.HttpClient httpClient)
    {
        _weixinManger = weixinManger;
        _weixinApi = weixinApi;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 从URL下载图片
    /// </summary>
    private async Task<byte[]> DownloadImageAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("下载图片失败: {Url}, StatusCode={StatusCode}",
                    imageUrl, response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "下载图片异常: {Url}", imageUrl);
            return null;
        }
    }

    /// <summary>
    /// 调试接口：测试微信连接 (无需认证)
    /// </summary>
    [HttpGet]
    [Route("TestWeixinConnection")]
    public async Task<IActionResult> TestWeixinConnection()
    {
        try
        {
            var appId = WechatAppId;

            var accessToken = await _weixinManger.GetAccessTokenAsync(appId, WechatAppSecret);

            return Ok(new
            {
                success = true,
                appId = appId,
                accessToken = accessToken.Substring(0, Math.Min(50, accessToken.Length)) + "..."
            });
        }
        catch (System.Exception ex)
        {
            return Ok(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 检查文本内容是否安全 (公开API方法)
    /// </summary>
    /// <param name="content">待检查的内容</param>
    /// <param name="scene">场景：1资料 2评论 3论坛 4社交日志</param>
    /// <param name="title">标题（可选）</param>
    /// <param name="openid">用户openid（可选，用于测试）</param>
    /// <returns></returns>
    [HttpPost]
    [Route("CheckContent")]
    public async Task<ContentSecurityCheckResult> CheckContent([FromBody] ContentCheckRequest request)
    {
        return await CheckContentAsync(request.Content, request.Scene, request.Title, request.OpenId);
    }

    /// <summary>
    /// 检查图片内容是否安全 (公开API方法)
    /// </summary>
    /// <param name="mediaUrl">图片URL</param>
    /// <returns></returns>
    [HttpPost]
    [Route("CheckMedia")]
    public async Task<MediaSecurityCheckResult> CheckMedia([FromBody] MediaCheckRequest request)
    {
        return await CheckMediaAsync(request.MediaUrl, request.Scene, request.OpenId);
    }

    /// <summary>
    /// 检查文本内容是否安全
    /// </summary>
    /// <param name="content">待检查的内容</param>
    /// <param name="scene">场景：1资料 2评论 3论坛 4社交日志</param>
    /// <param name="title">标题（可选）</param>
    /// <param name="openid">用户openid（可选，用于测试）</param>
    /// <returns></returns>
    public async Task<ContentSecurityCheckResult> CheckContentAsync(string content, int scene = 3, string title = null,
        string openid = null)
    {
        try
        {
            var appId = WechatAppId;
            var accessToken = await _weixinManger.GetAccessTokenAsync(appId, WechatAppSecret);

            var userOpenid = openid ?? "";

            var result = await _weixinApi.MsgSecCheck(
                accessToken,
                content,
                version: 2,
                scene: scene,
                openid: userOpenid,
                title: title);

            if (result.errcode == 0)
            {
                var hasRisk = false;
                foreach (var detail in result.detail)
                {
                    if (detail.errcode != 0 || detail.suggest == "risky")
                    {
                        hasRisk = true;
                        break;
                    }
                }

                return new ContentSecurityCheckResult
                {
                    IsSafe = !hasRisk,
                    Message = hasRisk ? "内容包含违规信息" : "内容安全",
                    Details = result.detail
                };
            }
            else if (result.errcode == 87014)
            {
                return new ContentSecurityCheckResult
                {
                    IsSafe = false,
                    Message = "内容含有违法违规内容",
                    Details = result.detail
                };
            }
            else
            {
                _logger.LogError("内容安全检查失败: {@result}", result);
                throw new UserFriendlyException($"内容检查失败: {result.errmsg}");
            }
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "内容安全检查异常");
            throw new UserFriendlyException("内容检查服务暂时不可用，请稍后重试");
        }
    }

    /// <summary>
    /// 检查图片内容是否安全 (使用小程序框架级 imgSecCheck 同步接口)
    /// 文档: https://developers.weixin.qq.com/miniprogram/dev/framework/security.imgSecCheck.html
    /// </summary>
    /// <param name="mediaUrl">媒体文件URL</param>
    /// <param name="scene">场景值（保留参数，当前接口不使用）</param>
    /// <param name="openid">用户openid（保留参数，当前接口不使用）</param>
    /// <returns></returns>
    private async Task<MediaSecurityCheckResult> CheckMediaAsync(string mediaUrl, int scene = 1, string openid = null)
    {
        try
        {
            if (string.IsNullOrEmpty(mediaUrl))
            {
                throw new UserFriendlyException("图片URL不能为空");
            }

            // 1. 从URL下载图片
            var imageBytes = await DownloadImageAsync(mediaUrl);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new UserFriendlyException("无法下载图片");
            }

            // 2. 验证文件大小 (微信限制: ≤1MB)
            if (imageBytes.Length > 1024 * 1024)
            {
                throw new UserFriendlyException("图片大小不能超过1MB，请先压缩图片");
            }

            var appId = WechatAppId;
            var accessToken = await _weixinManger.GetAccessTokenAsync(appId, WechatAppSecret);
            var checkResult = await _weixinApi.ImgSecCheck(accessToken, imageBytes);
            _logger.LogInformation("图片安全检查结果: errcode={Errcode}, errmsg={Errmsg}",
                checkResult.errcode, checkResult.errmsg);

            if (checkResult.errcode == 0)
            {
                return new MediaSecurityCheckResult
                {
                    IsSafe = true,
                    Message = "图片安全"
                };
            }
            else if (checkResult.errcode == 87014)
            {
                return new MediaSecurityCheckResult
                {
                    IsSafe = false,
                    Message = "图片含有违法违规内容"
                };
            }
            else
            {
                _logger.LogError("图片安全检查失败: {@result}", checkResult);
                throw new UserFriendlyException($"图片检查失败: {checkResult.errmsg}");
            }
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "图片安全检查异常");
            throw new UserFriendlyException("图片检查服务暂时不可用，请稍后重试");
        }
    }
}

/// <summary>
/// 内容安全检查结果
/// </summary>
public class ContentSecurityCheckResult
{
    /// <summary>
    /// 是否安全
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// 检查结果消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 详细检查结果
    /// </summary>
    public object Details { get; set; }
}

/// <summary>
/// 图片/音频安全检查结果
/// </summary>
public class MediaSecurityCheckResult
{
    /// <summary>
    /// 是否安全
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// 检查结果消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 详细检查结果
    /// </summary>
    public object Details { get; set; }
}

/// <summary>
/// 图片安全检查请求
/// </summary>
public class MediaCheckRequest
{
    public string MediaUrl { get; set; }

    /// <summary>
    /// 兼容前端参数名
    /// </summary>
    public string url
    {
        get => MediaUrl;
        set => MediaUrl = value;
    }

    public int Scene { get; set; } = 1;
    public string OpenId { get; set; }
}

/// <summary>
/// 文本安全检查请求
/// </summary>
public class ContentCheckRequest
{
    public string Content { get; set; }
    public int Scene { get; set; } = 3;
    public string Title { get; set; }
    public string OpenId { get; set; }
}