using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Json;
using Abp.Web.Models;
using EasyAbp.Abp.WeChat.Pay.RequestHandling.Dtos;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tt.HttpClient.Weixin;
using Tt.HttpClient.Weixin.Extensions;
using Tt.HttpClient.Weixin.Models;
using TtWork.HttpClient.Weixin;
using TtWork.HttpClient.Weixin.Models;
using TtWork.Abp.AppManagement.Applications.TT.Abp.AppManagement.Application;
using TtWork.Abp.AppManagement.Events;
using TtWork.HttpClient.Weixin.Extensions;
using TtWork.HttpClient.Weixin.Security.Extensions;
using TtWork.HttpClient.Weixin.Security.PlatformCertificate;
using TtWork.Project.Domains.Pays;
using TtWork.Project.Jobs;
using Microsoft.AspNetCore.Hosting;

namespace TtWork.Project.Applications;

[Route("/api/PayNotify")]
public class WeChatPayController(
    IRepository<WechatPaymentNotification, Ulid> wechatPaymentNotificationRepository,
    ILogger<WeChatPayController> logger,
    IHttpContextAccessor httpContextAccessor,
    IMediator mediator,
    IPlatformCertificateManager platformCertificateManager,
    IUnitOfWorkManager unitOfWorkManager,
    IV3PayApi v3PayApi,
    IWebHostEnvironment _env
)
    : AbpController {
    /// <summary>
    /// JS-SDK支付回调地址（在统一下单接口中设置notify_url）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [DontWrapResult]
    [Route("TenPay/{appName}")]
    public virtual async Task<IActionResult> TenPay(string appName) {
        // using StreamReader stream = new StreamReader(httpContextAccessor.HttpContext!.Request.Body);
        // string body = await stream.ReadToEndAsync();
        var body = await GetPostDataAsync();
        logger.LogInformation("Request Body: {@body}", body);
        var app = await mediator.Send(new QueryApp(appName));
        var mchId = app.GetValue("mchId");
        var input = BuildNotifyInputDto(body, mchId);
        logger.LogInformation("NotifyDto: {@input}", input.ToJsonString());

        //验证签名
        if (!await IsSignValidAsync(input, app)) {
            logger.LogError("签名验证不通过");
            return BadRequest(new WeChatPayNotificationOutput {
                Code = "FAIL",
                Message = "签名验证不通过"
            });
        }
        
        logger.LogInformation("签名验证通过");
        var decryptingResult = DecryptResource<WeChatPayPaidEventModel>(input, app);
        logger.LogInformation("解密商品: {@decryptingResult}", decryptingResult.ToJsonString());
        
        if (!await wechatPaymentNotificationRepository.GetAll().AsNoTracking().AnyAsync(x => x.OutTradeNo == decryptingResult.OutTradeNo)) {
            var noti = await wechatPaymentNotificationRepository.InsertAsync(new WechatPaymentNotification {
                Id = Ulid.NewUlid(),
                OutTradeNo = decryptingResult.OutTradeNo,
                TransactionId = decryptingResult.TransactionId,
                MchId = decryptingResult.MchId,
                AppId = decryptingResult.AppId,
                SuccessTime = decryptingResult.SuccessTime,
                RawData = decryptingResult.ToJsonString(false, false)
            });
            await CurrentUnitOfWork.SaveChangesAsync();

            BackgroundJob.Enqueue<TenPayNotifyJob>(z => z.ExecuteAsync(new TenPayNotifyArgs(noti)));
        }
        
        return Ok(new WeChatPayNotificationOutput {
            Code = "SUCCESS"
        });
    }


    protected virtual async Task<string> GetPostDataAsync() {
        Request.EnableBuffering();
        using var streamReader = new StreamReader(Request.Body);
        var postData = await streamReader.ReadToEndAsync();
        Request.Body.Position = 0;
        return postData;
    }


    protected virtual async Task<bool> IsSignValidAsync(NotifyInputDto inputDto, AppDto app) {
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = Path.Combine(wwwrootPath, app.GetValue("certPath").TrimStart('/', '\\'));
        var certificate =
            await platformCertificateManager.GetPlatformCertificateAsync(app.GetValue("mchId"), inputDto.HttpHeader.SerialNumber, app.GetValue("mchKey"), certPath);
        var sb = new StringBuilder();
        sb.Append(inputDto.HttpHeader.Timestamp).Append("\n")
            .Append(inputDto.HttpHeader.Nonce).Append("\n")
            .Append(inputDto.RequestBodyString).Append("\n");
        return certificate.VerifySignature(sb.ToString(), inputDto.HttpHeader.Signature);
    }

    protected virtual TObject DecryptResource<TObject>(NotifyInputDto inputDto, AppDto app) {
        var sourceJson = WeChatPaySecurityUtility.AesGcmDecrypt(app.GetValue("mchKey"),
            inputDto.RequestBody.Resource.AssociatedData,
            inputDto.RequestBody.Resource.Nonce, inputDto.RequestBody.Resource.Ciphertext);
        return JsonConvert.DeserializeObject<TObject>(sourceJson);
    }

    private NotifyInputDto BuildNotifyInputDto(string requestBody, string mchId) {
        var request = httpContextAccessor!.HttpContext!.Request;
        return new NotifyInputDto {
            MchId = mchId,
            RequestBodyString = requestBody,
            RequestBody = JsonConvert.DeserializeObject<WeChatPayNotificationInput>(requestBody),
            HttpHeader = new NotifyHttpHeaderModel(
                request.Headers["Wechatpay-Serial"],
                request.Headers["Wechatpay-TimeStamp"],
                request.Headers["Wechatpay-Nonce"],
                request.Headers["Wechatpay-Signature"])
        };
    }


    [HttpGet]
    [Route("test2")]
    [AbpAuthorize]
    public async Task<object> Test2(string openid) {
        var app = await mediator.Send(new QueryApp("uniapp"));

        var result = await v3PayApi.CreateJsOrderAsync(new CreateOrderRequest() {
            AppId = app.GetValue("appid"), // 请替换为你的 AppId
            MchId = app.GetValue("mchId"),
            Description = "Image形象店-深圳腾大-QQ公仔",
            OutTradeNo = Ulid.NewUlid().ToString(),
            Attach = JsonConvert.SerializeObject(new {
                title = "Image形象店-深圳腾大-QQ公仔", desc = "Image形象店-深圳腾大-QQ公仔"
            }),
            NotifyUrl = app.GetValue("notifyUrl"),

            Amount = new CreateOrderAmountModel {
                Total = 1,
                Currency = "CNY"
            },
            Payer = new CreateOrderRequest.CreateOrderPayerModel {
                OpenId = openid // 请替换为测试用户的 OpenId，具体 Id 可以在微信公众号平台-用户管理进行查看。
            },
        }, app.GetValue("certPath"));


        var p = await v3PayApi.GetJsSdkWeChatPayParametersAsync(
            new GetJsSdkWeChatPayParametersInput() {
                AppId = app.GetValue("appid"),
                MchId = app.GetValue("mchId"),
                PrepayId = result.PrepayId
            }, app.GetValue("certPath")
        );

        return p;
    }
}