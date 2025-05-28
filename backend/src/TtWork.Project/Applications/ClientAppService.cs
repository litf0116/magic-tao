using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Json;
using Abp.Runtime.Session;
using Abp.UI;
using FreeIM;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SqlSugar;
using Tt.HttpClient.Weixin;
using TtWork.Abp;
using TtWork.Abp.AppManagement.Apps;
using TtWork.Abp.AppManagement.Events;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.Abp.Entity;
using TtWork.HttpClient.Weixin;
using TtWork.HttpClient.Weixin.Models;
using TtWork.HttpClient.Weixin.Security.PlatformCertificate;
using TtWork.Lib.Extensions;
using TtWork.Project.Domains;
using TtWork.Project.Domains.Pays;
using static TtWork.HttpClient.Weixin.Models.RefundOrderRequest;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Utilities;

namespace TtWork.Project.Applications;

public class ClientAppService(
    IRepository<WechatPaymentNotification, Ulid> wechatPaymentNotificationRepository,
    IRepository<AuctionItem, long> auctionItemRepository,
    IRepository<UserFriend> userFriendRepository,
    IRepository<Message, Guid> messageRepository,
    IRepository<User, long> userRepository,
    IRepository<ChatListDelete> chatListDeleteRepository,
    ILogger<ClientAppService> logger,
    IHttpContextAccessor httpContextAccessor,
    IMediator mediator,
    UserCache userCache,
    IRepository<PayOrder, Ulid> payOrderRepository,
    IAbpSession _abpSession,
    ISqlSugarClient _sqlSugar,
    IV3PayApi v3PayApi,
    IWebHostEnvironment _env,
    IConfiguration _configuration
    ) : AbpAppServiceBase
{
    public IRepository<ChatListDelete> ChatListDeleteRepository { get; } = chatListDeleteRepository;

    /// <summary>
    /// 保证金支付
    /// </summary>
    /// <param name="openid"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpGet]
    [AbpAuthorize]
    public async Task<object> PayDeposit(string openid, string type = "jsapi")
    {
        var app = await mediator.Send(new QueryApp());
        var appid = app.GetValue("appid");
        var mchid = app.GetValue("mchId");
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = wwwrootPath + app.GetValue("certPath");

        var payOrder = new PayOrder();
        payOrder.CreateDepositPay(AppConsts.保证金, AbpSession.UserId!.Value, openid, app.Name, appid, mchid, AbpSession.TenantId!.Value);
        // payOrder.CreateDepositPay(0.01m, AbpSession.UserId!.Value, openid, app.Name, appid, mchid, AbpSession.TenantId!.Value);
        await payOrderRepository.InsertAsync(payOrder);
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            if (type == "jsapi")
            {
                var result = await v3PayApi.CreateJsOrderAsync(new CreateOrderRequest()
                {
                    AppId = appid, // 请替换为你的 AppId
                    MchId = mchid,
                    Description = "保证金支付",
                    OutTradeNo = payOrder.OutTradeNo,
                    Attach = (new { payOrderId = payOrder.Id, name = "保证金支付" }).ToJsonString(false, false),
                    NotifyUrl = app.GetValue("notifyUrl"),
                    Amount = new CreateOrderAmountModel
                    {
                        Total = payOrder.Total,
                        Currency = "CNY"
                    },
                    Payer = new CreateOrderRequest.CreateOrderPayerModel
                    {
                        OpenId = openid
                    },
                }, certPath);

                var p = await v3PayApi.GetJsSdkWeChatPayParametersAsync(
                    new GetJsSdkWeChatPayParametersInput()
                    {
                        AppId = app.GetValue("appid"),
                        MchId = app.GetValue("mchId"),
                        PrepayId = result.PrepayId
                    }, certPath);

                return p;
            }

        }
        catch (Exception e)
        {
            var err = $"[保证金支付]支付发生错误:{e.Message}";
            logger.LogError(e, err);
            throw new UserFriendlyException(err);
        }

        throw new UserFriendlyException($"未知的支付类型:{type}");
    }

    /// <summary>
    /// 支付退款
    /// </summary>
    /// <param name="outTradeNo"></param>
    /// <returns></returns>
    [HttpGet]
    [AbpAuthorize]
    public async Task PayRefund(string outTradeNo)
    {
        //查询订单信息
        var payOrder = await payOrderRepository.FirstOrDefaultAsync(x => x.OutTradeNo == outTradeNo);
        if (payOrder == null)
        {
            throw new UserFriendlyException($"当前订单不存在");
        }

        if (payOrder.State != PayState.已支付)
        {
            throw new UserFriendlyException($"当前订单状态不正确，订单状态：" + (PayState)payOrder.State);
        }
        if (payOrder.IsSuccessPay == false)
        {
            throw new UserFriendlyException($"当前订单状态不正确，没有回调成功！");
        }
        //微信支付回调通知
        var wechatPayNotification = await wechatPaymentNotificationRepository.FirstOrDefaultAsync(x => x.OutTradeNo == outTradeNo);
        if (wechatPayNotification == null)
        {
            throw new UserFriendlyException($"当前订单没有支付回调通知记录");
        }
        var appid = _configuration["Apps:uniapp:appid"];// 服务号的appId
        var mchid = _configuration["Apps:uniapp:mchId"];//申请的支付签名KEY;
        var mchKey = _configuration["Apps:uniapp:mchKey"]; //申请的支付商户ID
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = wwwrootPath + _configuration["Apps:uniapp:certPem"];
        string certKey = wwwrootPath + _configuration["Apps:uniapp:certKey"];

        string serialNumber = RSAUtility.ExportSerialNumberFromCertificate(System.IO.File.ReadAllText(certPath));
        //
        var manager = new InMemoryCertificateManager();
        var options = new WechatTenpayClientOptions()
        {
            MerchantId = mchid, // 商户号
            MerchantV3Secret = mchKey, // 商户 API v3 密钥
            MerchantCertificateSerialNumber = serialNumber, // 商户 API 证书序列号
            MerchantCertificatePrivateKey = System.IO.File.ReadAllText(certKey), // 商户 API 证书私钥
            PlatformCertificateManager = manager
        };
        var client = new WechatTenpayClient(options);
        var request = new CreateRefundDomesticRefundRequest()
        {
            //OutTradeNumber = "商户订单号", // 【商户订单号】 原支付交易对应的商户订单号，与transaction_id二选一
            TransactionId = wechatPayNotification.TransactionId, // 【微信支付订单号】 原支付交易对应的微信订单号，与out_trade_no二选一
                                                                 // 商户自定义退款唯一标识，要做好记录，退款状态查询时入参必填
            OutRefundNumber = $"WX{DateTime.Now.ToString("yyyyMMddHHmmssffffff")}",
            Reason = "用户申请退款",
            //NotifyUrl = "https://...", // 回调地址
            Amount = new CreateRefundDomesticRefundRequest.Types.Amount()
            {
                Total = payOrder.Total, // 单位：分
                Refund = payOrder.Total, // 单位：分
                Currency = "CNY" // 【退款币种】 目前只支持人民币：CNY。
            },
        };
        //发送请求
        var response = await client.ExecuteCreateRefundDomesticRefundAsync(request);
        if (response.IsSuccessful())
        {
            //查询用户信息
            var userInfo = await userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == payOrder.CreatorUserId);
            if (userInfo != null)
            {
                userInfo.DepositBalance -= (payOrder.Total / 100m);
                await userRepository.UpdateAsync(userInfo);
                //
                payOrder.State = PayState.已退款;
                payOrderRepository.Update(payOrder);
            }
            Console.WriteLine("RefundId：", response.RefundId); // 【微信支付退款号】 微信支付退款号
            Console.WriteLine("OutRefundNumber：", response.OutRefundNumber); // 【商户退款单号】 商户系统内部的退款单号，商户系统内部唯一
            Console.WriteLine("TransactionId：", response.TransactionId); // 【微信支付订单号】
            Console.WriteLine("OutRefundNumber：", response.OutTradeNumber); // 【商户订单号】 原支付交易对应的商户订单号
        }
        else
        {
            Console.WriteLine("HTTP 状态：" + response.GetRawStatus());
            Console.WriteLine("错误代码：" + response.ErrorCode);
            Console.WriteLine("错误描述：" + response.ErrorMessage);
        }

    }


    public const string LOBBY = "0_lobby";
    public const string AUCTION = "-1_auction";


    /// <summary>
    /// 用户提现
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpPost]
    [AbpAuthorize]
    public async Task PayWithdrawal(WithdrawalData parameter)
    {
        var userId = _abpSession.UserId;
        if (userId != parameter.UserId)
        {
            throw new UserFriendlyException($"当前用户信息错误，请稍后重试！");
        }
        //获取用户信息
        var user = await userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == AbpSession.UserId!.Value);
        if (user == null)
        {
            throw new UserFriendlyException($"当前用户不存在！");
        }
        if (user.Balance < parameter.Amount)
        {
            throw new UserFriendlyException($"当前用户余额不足，无法提现！");
        }
        //扣除余额
        //var cnt = await userRepository.GetAll().Where(x => x.Id == user.Id).ExecuteUpdateAsync(setter =>
        //     setter.SetProperty(b => b.Balance, b => b.Balance - parameter.Amount));
        //
        await _sqlSugar.Insertable(new WithdrawalAmountEntity
        {
            Amount = parameter.Amount,
            UserId = parameter.UserId,
            Status = 1,
            WithdrawalTime = DateTime.Now
        }).ExecuteCommandAsync();
    }
    /// <summary>
    /// 删除与某人的聊天记录
    /// </summary>
    /// <param name="id">对方用户ID</param>
    /// <returns></returns>
    [HttpGet]
    [AbpAuthorize]
    public async Task DeleteChatList(long id)
    {
        // if (id == 14) {
        //     throw new UserFriendlyException("不能删除与拍卖师的聊天记录");
        // }

        var userId = AbpSession.UserId!.Value;
        await chatListDeleteRepository.InsertAsync(new ChatListDelete()
        {
            UserId = userId,
            ToUserId = id
        });
        await CurrentUnitOfWork.SaveChangesAsync();

        // chatListDeleteRepository.GetAll().Where(x => x.UserId == AbpSession.UserId.Value && x.ToUserId == id).ExecuteDeleteAsync()
    }

    [HttpGet]
    public async Task<List<ChatListItem>> GetChatList()
    {
        List<ChatListItem> s = [];
        var lastlobbyMsg = await messageRepository.GetAll().AsNoTracking().Where(x => x.Chan == LOBBY && x.Type == ChatMessageType.Text).OrderByDescending(x => x.Time)
            .FirstOrDefaultAsync();
        var lastauctionMsg = await messageRepository.GetAll().AsNoTracking().Where(x => x.Chan == AUCTION && x.Type == ChatMessageType.Text).OrderByDescending(x => x.Time)
            .FirstOrDefaultAsync();
        //s.Add(new ChatListItem()
        //{
        //    id = 0,
        //    lastMsg = $"{lastlobbyMsg?.Msg}",
        //    name = "lobby",
        //    order = 100,
        //    time = lastlobbyMsg?.Time ?? 0,
        //    type = 0,
        //    unread = 0
        //});
        s.Add(new ChatListItem()
        {
            id = -1,
            lastMsg = $"{lastauctionMsg?.Msg}",
            name = "auction",
            order = 99,
            time = lastlobbyMsg?.Time ?? 0,
            type = 0,
            unread = 0
        });
        if (AbpSession.UserId.HasValue)
        {
            List<ChatListItem> s2 = [];
            var userId = AbpSession.UserId.Value;
            var chatDeleteList = await chatListDeleteRepository.GetAll().AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
            var messageList = await messageRepository.GetAll().AsNoTracking().Where(x => x.Chan == null && (x.To == userId || x.From == userId) && x.To != null)
                .OrderByDescending(x => x.Time)
                .GroupBy(m => new { m.From, m.To })
                .Select(g => new
                {
                    g.Key.To,
                    LatestMessage = g.OrderByDescending(m => m.Time).First()
                })
                .ToListAsync();


            //Console.WriteLine(messageList.ToJsonString());

            foreach (var item in messageList)
            {
                if (item.LatestMessage.From == userId)
                {
                    var toUser = await userCache.GetAsync(item.LatestMessage.To!.Value);
                    if (toUser != null)
                    {
                        s2.Add(new ChatListItem
                        {
                            id = toUser.Id,
                            lastMsg = $"{item.LatestMessage.Msg}",
                            name = toUser.Name,
                            avatar = toUser.HeadImgUrl,
                            order = 0,
                            time = item.LatestMessage.Time,
                            type = 1,
                            unread = 0
                        });
                    }
                }
                else
                    s2.Add(new ChatListItem
                    {
                        id = item.LatestMessage.From,
                        lastMsg = $"{item.LatestMessage.Msg}",
                        name = $"{item.LatestMessage.FromName}",
                        avatar = item.LatestMessage.Avatar ?? "",
                        order = 0,
                        time = item.LatestMessage.Time,
                        type = 1,
                        unread = 0
                    });
            }

            s.AddRange(s2.GroupBy(x => x.id).Select(g => g.OrderByDescending(x => x.time).First()).ToList()
                .Where(item => chatDeleteList.All(x => x.ToUserId != item.id)).ToList());
        }

        return s;
    }

    public record ChatListItem
    {
        public long id { get; set; }
        public string lastMsg { get; set; }
        public string name { get; set; }
        public int order { get; set; }
        public long time { get; set; }
        public int type { get; set; }
        public int unread { get; set; }

        public string avatar { get; set; }
    }

    /// <summary>
    /// 用户充值
    /// </summary>
    /// <param name="openid"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpGet]
    [AbpAuthorize]
    public async Task<object> TopUp(string openid, decimal amount, string type = "jsapi")
    {
        if (amount < 0.01m)
        {
            throw new UserFriendlyException(1, "充值金额不能小于0.01");
        }

        var app = await mediator.Send(new QueryApp());
        var appid = app.GetValue("appid");
        var mchid = app.GetValue("mchId");

        var payOrder = new PayOrder();
        payOrder.CreateTopUpPay(amount, AbpSession.UserId!.Value, openid, app.Name, appid, mchid, AbpSession.TenantId!.Value);
        await payOrderRepository.InsertAsync(payOrder);
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            var err = $"[用户充值]支付发生错误:{e.Message}";
            logger.LogError(e, err);
            throw new UserFriendlyException(err);
        }

        var _order = new CreateOrderRequest()
        {
            AppId = appid, // 请替换为你的 AppId
            MchId = mchid,
            Description = "用户充值",
            OutTradeNo = payOrder.OutTradeNo,
            Attach = (new { payOrderId = payOrder.Id, name = "用户充值" }).ToJsonString(false, false),
            NotifyUrl = app.GetValue("notifyUrl"),
            Amount = new CreateOrderAmountModel
            {
                Total = payOrder.Total,
                Currency = "CNY"
            },
            Payer = new CreateOrderRequest.CreateOrderPayerModel
            {
                OpenId = openid
            },
        };


        if (type == "jsapi")
        {
            var result = await v3PayApi.CreateJsOrderAsync(_order, app.GetValue("certPath"));

            var p = await v3PayApi.GetJsSdkWeChatPayParametersAsync(
                new GetJsSdkWeChatPayParametersInput()
                {
                    AppId = app.GetValue("appid"),
                    MchId = app.GetValue("mchId"),
                    PrepayId = result.PrepayId
                }, app.GetValue("certPath")
            );

            return p;
        }


        throw new UserFriendlyException($"未知的支付类型:{type}");
    }

    /// <summary>
    /// 获取用户统计
    /// </summary>
    /// <returns></returns>
    [AbpAuthorize]
    public async Task<object> GetMyCount()
    {
        string cacheKey = AppConsts.CacheKeys.MyCount.FormatWith(AbpSession.UserId!.Value);
        //if (!memoryCache.TryGetValue(cacheKey, out object result))
        //{
        var user = await userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == AbpSession.UserId!.Value);
        var auctionSuccess = await auctionItemRepository.GetAll().CountAsync(x => x.Status == AuctionStatusEnum.交易成功);
        var friend = await userFriendRepository.GetAll().CountAsync(x => x.FriendId == AbpSession.UserId!.Value && x.Status == true);
        var balance = user.Balance;
        var depositBalance = user.DepositBalance;

        var result = new
        {
            auctionSuccess,
            friend,
            balance,
            depositBalance
        };
        var cacheEntityOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        //    memoryCache.Set(cacheKey, result, cacheEntityOptions);
        //}

        return result;
    }


    public string Ip
    {
        get
        {
            try
            {
                return httpContextAccessor!.HttpContext!.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                       httpContextAccessor!.HttpContext!.Request.HttpContext!.Connection!.RemoteIpAddress!
                           .ToString();
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
/// <summary>
/// 提现信息
/// </summary>
public class WithdrawalData()
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 提交金额
    /// </summary>
    public decimal Amount { get; set; }
}