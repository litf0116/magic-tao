using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Json;
using Abp.Runtime.Session;
using Abp.UI;
using FreeIM;
using Hangfire;
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
using TtWork.Project.Applications.Pays.Dto;
using static TtWork.HttpClient.Weixin.Models.RefundOrderRequest;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Utilities;
using TtWork.Project.Services;
using TtWork.Project.Caches;
using TtWork.Project.Caches;
using TtWork.Project.Core;
using TtWork.Project.Core.Session;
using TtWork.Project.Core.Utils;
using TtWork.Project.Jobs;

namespace TtWork.Project.Applications;

public class ClientAppService(
    IRepository<WechatPaymentNotification, Ulid> wechatPaymentNotificationRepository,
    IRepository<AuctionItem, long> auctionItemRepository,
    IRepository<UserFriend> userFriendRepository,
    IRepository<User, long> userRepository,
    IRepository<ChatListDelete, int> chatListDeleteRepository,
    IRepository<ChatChannel, long> chatChannelRepository,
    ChatChannelService chatChannelService,
    ILogger<ClientAppService> logger,
    IHttpContextAccessor httpContextAccessor,
    IMediator mediator,
    ChatUserCache chatUserCache,
    IRepository<PayOrder, Ulid> payOrderRepository,
    IRepository<UserDepositLog, Ulid> userDepositLogRepository,
    IAbpSession _abpSession,
    ISqlSugarClient _sqlSugar,
    IV3PayApi v3PayApi,
    IWebHostEnvironment _env,
    IConfiguration _configuration
) : AbpAppServiceBase
{
    /// <summary>
    /// 保证金支付
    /// </summary>
    /// <param name="openid"></param>
    /// <param name="type"></param>
    /// <param name="amount">支付金额，如果不指定则使用默认保证金金额</param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpGet]
    [AbpAuthorize]
    public async Task<object> PayDeposit(string openid, string type = "jsapi", decimal? amount = null)
    {
        var app = await mediator.Send(new QueryApp());
        var appid = app.GetValue("appid");
        var mchid = app.GetValue("mchId");
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = Path.Combine(wwwrootPath, app.GetValue("certPath").TrimStart('/', '\\'));

        var finalAmount = amount ?? AppConsts.保证金;
        var payOrder = new PayOrder();
        payOrder.CreateDepositPay(finalAmount, AbpSession.UserId!.Value, openid, app.Name, appid, mchid,
            AbpSession.TenantId!.Value);
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

            if (type == "h5")
            {
                var clientIp = GetClientIp();
                var result = await v3PayApi.CreateH5OrderAsync(new CreateOrderRequest()
                {
                    AppId = appid,
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
                    SceneInfo = new CreateOrderRequest.CreateOrderSceneInfoModel
                    {
                        PayerClientIp = clientIp
                    },
                }, certPath);

                return new { h5_url = result.H5Url, out_trade_no = payOrder.OutTradeNo };
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
    /// 保证金 Native 支付（PC 端扫码支付）
    /// </summary>
    /// <param name="amount">支付金额，如果不指定则使用默认保证金金额</param>
    /// <returns>包含 code_url 和订单号的对象</returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpGet]
    [AbpAuthorize]
    public async Task<object> PayDepositNative(decimal? amount = null)
    {
        var app = await mediator.Send(new QueryApp("uniapp", false));
        var appid = app.GetValue("appid");
        var mchid = app.GetValue("mchId");
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = Path.Combine(wwwrootPath, app.GetValue("certPath").TrimStart('/', '\\'));

        var finalAmount = amount ?? AppConsts.保证金;
        var payOrder = new PayOrder();
        payOrder.CreateDepositPay(finalAmount, AbpSession.UserId!.Value, "", app.Name, appid, mchid,
            AbpSession.TenantId!.Value);
        await payOrderRepository.InsertAsync(payOrder);
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();

            var result = await v3PayApi.CreateNativeOrderAsync(new CreateNativeOrderRequest()
            {
                AppId = appid,
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
            }, certPath);

            if (!string.IsNullOrEmpty(result.Code))
            {
                var errMsg = $"[保证金 Native 支付]微信支付返回错误：{result.Code} - {result.Message}";
                logger.LogError(errMsg);
                throw new UserFriendlyException(errMsg);
            }

            if (string.IsNullOrEmpty(result.CodeUrl))
            {
                var errMsg = "[保证金 Native 支付]微信支付返回成功但 code_url 为空";
                logger.LogError(errMsg);
                throw new UserFriendlyException(errMsg);
            }

            return new
            {
                code_url = result.CodeUrl,
                outTradeNo = payOrder.OutTradeNo,
                amount = finalAmount
            };
        }
        catch (Exception e)
        {
            var err = $"[保证金 Native 支付]支付发生错误:{e.Message}";
            logger.LogError(e, err);
            throw new UserFriendlyException(err);
        }
    }

    /// <summary>
    /// 查询支付订单状态
    /// </summary>
    /// <param name="outTradeNo">订单号</param>
    /// <returns>订单状态信息</returns>
    [HttpGet]
    [AbpAuthorize]
    public async Task<PayOrderStatusDto> GetPayOrderStatus(string outTradeNo)
    {
        if (outTradeNo.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("订单号不能为空");
        }

        var payOrder = await payOrderRepository.FirstOrDefaultAsync(x =>
            x.OutTradeNo == outTradeNo &&
            x.CreatorUserId == AbpSession.UserId.Value
        );

        if (payOrder == null)
        {
            return new PayOrderStatusDto
            {
                Status = "NOT_FOUND",
                Message = "订单不存在"
            };
        }

        if (payOrder.State != PayState.已支付)
        {
            var app = await mediator.Send(new QueryApp(payOrder.AppName, false));
            var mchid = app.GetValue("mchId");
            string wwwrootPath = _env.WebRootPath;
            string certPath = Path.Combine(wwwrootPath, app.GetValue("certPath").TrimStart('/', '\\'));

            var wechatResult = await v3PayApi.QueryOrderAsync(mchid, outTradeNo, certPath);

            if (!string.IsNullOrEmpty(wechatResult.Code))
            {
                logger.LogError($"[查询订单状态]微信返回错误：{wechatResult.Code} - {wechatResult.Message}");
            }
            else if (wechatResult.TradeState == "SUCCESS")
            {
                if (payOrder.HostType == OrderType.保证金 && payOrder.IsSuccessPay)
                {
                    logger.LogInformation("[GetPayOrderStatus]该订单已处理过，跳过: {OutTradeNo}", outTradeNo);
                }
                else
                {
                    payOrder.SuccessPay(wechatResult.TransactionId, wechatResult.SuccessTime);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (payOrder.HostType == OrderType.保证金)
                    {
                        var existingLog = await userDepositLogRepository.GetAll()
                            .Where(x => x.CreatorUserId == payOrder.CreatorUserId && x.IsSuccess)
                            .Where(x => x.Reason != null && x.Reason.Contains(outTradeNo))
                            .FirstOrDefaultAsync();

                        if (existingLog != null)
                        {
                            logger.LogInformation("[GetPayOrderStatus]该订单已有保证金处理记录，跳过: {OutTradeNo}", outTradeNo);
                        }
                        else
                        {
                            decimal finalAmount = payOrder.Total <= 100
                                ? payOrder.Total
                                : payOrder.Total - 100;

                            var depositLog = new UserDepositLog(BalanceLogType.支付, finalAmount / 100m)
                            {
                                CreatorUserId = payOrder.CreatorUserId,
                                TenantId = payOrder.TenantId,
                                Reason = $"保证金支付:{outTradeNo}"
                            };
                            await userDepositLogRepository.InsertAsync(depositLog);
                            await CurrentUnitOfWork.SaveChangesAsync();

                            BackgroundJob.Enqueue<UserDepositJob>(b => b.ExecuteAsync(depositLog));
                            logger.LogInformation("[GetPayOrderStatus]保证金订单支付成功，触发UserDepositJob: {OutTradeNo}", outTradeNo);
                        }
                    }
                }
            }
        }

        return new PayOrderStatusDto
        {
            OrderId = payOrder.Id.ToString(),
            OutTradeNo = payOrder.OutTradeNo,
            Status = payOrder.State.ToString(),
            Amount = payOrder.Total / 100m,
            PaidTime = payOrder.SuccessPayTime,
            TradeNo = payOrder.IsSuccessPay ? "TRADE_SUCCESS" : null,
            Message = GetStatusMessage(payOrder.State)
        };
    }

    private string GetStatusMessage(PayState state)
    {
        return state switch
        {
            PayState.未支付 => "等待支付",
            PayState.已支付 => "支付成功",
            PayState.取消 => "订单已取消",
            PayState.退款中 => "退款处理中",
            PayState.已退款 => "已退款",
            PayState.部分退款 => "部分退款",
            _ => "未知状态"
        };
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
        var wechatPayNotification =
            await wechatPaymentNotificationRepository.FirstOrDefaultAsync(x => x.OutTradeNo == outTradeNo);
        if (wechatPayNotification == null)
        {
            throw new UserFriendlyException($"当前订单没有支付回调通知记录");
        }

        var appid = _configuration["Apps:uniapp:appid"]; // 服务号的appId
        var mchid = _configuration["Apps:uniapp:mchId"]; //申请的支付签名KEY;
        var mchKey = _configuration["Apps:uniapp:mchKey"]; //申请的支付商户ID
        // 获取 wwwroot 完整路径
        string wwwrootPath = _env.WebRootPath;
        // 组合文件路径
        string certPath = Path.Combine(wwwrootPath, _configuration["Apps:uniapp:certPem"].TrimStart('/', '\\'));
        string certKey = Path.Combine(wwwrootPath, _configuration["Apps:uniapp:certKey"].TrimStart('/', '\\'));

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
            var userInfo = await userRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == payOrder.CreatorUserId);
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
        if (userId == null)
        {
            throw new UserFriendlyException($"请先登录！");
        }

        //获取用户信息
        var user = await userRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId.Value);
        if (user == null)
        {
            throw new UserFriendlyException($"当前用户不存在！");
        }

        if (user.Balance < parameter.Amount)
        {
            throw new UserFriendlyException($"当前用户余额不足，无法提现！");
        }

        await _sqlSugar.Insertable(new WithdrawalAmountEntity
        {
            Amount = parameter.Amount,
            UserId = (int)userId.Value,
            Status = 1,
            WithdrawalTime = DateTime.Now
        }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除与某人的聊天记录
    /// 使用用户状态字段，只影响当前用户的显示
    /// 注意：当有新消息时，隐藏的频道会自动恢复显示
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

        // 使用新的用户状态字段，设置用户的会话状态为已删除
        // 只影响当前用户的显示，不影响对方
        await chatChannelService.DeleteUserChannelAsync(userId, id);
    }

    /// <summary>
    /// 获取聊天列表（实时版本 - 直接从数据库查询）
    /// </summary>
    /// <returns>聊天列表</returns>
    [HttpGet]
    public async Task<List<ChatListItem>> GetChatList()
    {
        var userId = AbpSession.UserId ?? 0;
        var channels = await chatChannelService.GetVisibleChannelsForUserAsync(userId);

        logger.LogInformation($"[GetChatList] Step 1 - Total channels from DB: {channels.Count}");
        logger.LogInformation(
            $"[GetChatList] Step 2 - Channels: {string.Join(", ", channels.Select(c => $"{c.ChannelId}(Type={c.ChannelType},Active={c.IsActive},HasMsg={c.LastMessageId != null})"))}");

        if (channels.Count == 0)
        {
            return new List<ChatListItem>();
        }

        // ===== 版本控制过滤逻辑 =====
        var currentVersion = _abpSession.GetAppVersion();
        var stableVersion = await SettingManager.GetSettingValueAsync(AppSettings.VersionControl.LatestStableVersion);
        var shouldShowAuction = VersionComparer.ShouldShowAuction(currentVersion, stableVersion);

        // 获取请求头中的原始值用于调试
        var httpContextAccessor = IocManager.Instance.Resolve<IHttpContextAccessor>();
        var requestHeaders = httpContextAccessor?.HttpContext?.Request.Headers;
        var appVersionHeaderValue = requestHeaders?["AppVersion"].FirstOrDefault();

        logger.LogInformation(
            $"[GetChatList] Step 3 - currentVersion: [{currentVersion}], stableVersion: [{stableVersion}], shouldShowAuction: {shouldShowAuction}");
        logger.LogInformation(
            $"[GetChatList] Step 3b - RequestHeader AppVersion: [{appVersionHeaderValue}], Headers count: {requestHeaders?.Count}");
        // ===== 版本控制过滤逻辑结束 =====

        var privateUserIds = channels
            .Where(c => c.ChannelType == ChatChannelType.Private)
            .Select(c => c.GetOtherUserId(userId) ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var userInfos = await chatUserCache.GetBatchUserBasicAsync(privateUserIds);

        var result = channels
            .Select(c => ConvertToChatListItem(c, userId, userInfos))
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();

        Logger.Info(
            $"[GetChatList] Step 4 - After ConvertToChatListItem: {result.Count} items, IDs: {string.Join(", ", result.Select(x => x.id))}");

        result = result.Where(x => x.id != AppSettings.VersionControl.AuctionChannelId || shouldShowAuction).ToList();

        Logger.Info(
            $"[GetChatList] Step 5 - After version filter: {result.Count} items, AuctionChannelId={AppSettings.VersionControl.AuctionChannelId}, hasAuction={result.Any(x => x.id == AppSettings.VersionControl.AuctionChannelId)}");

        return result
            .OrderByDescending(x => x.order)
            .ThenByDescending(x => x.time)
            .ToList();
    }

    private ChatListItem? ConvertToChatListItem(ChatChannel c, long userId, Dictionary<long, UserBasicInfo> userInfos)
    {
        if (c.ChannelType == ChatChannelType.System)
        {
            return new()
            {
                id = c.ChannelId switch
                {
                    "-1_auction" => -1,
                    "-10_announcement" => -10,
                    "-11_newbie" => -11,
                    _ => c.ChannelId.GetHashCode()
                },
                lastMsg = c.LastMessageContent ?? "",
                name = c.ChannelName ?? c.ChannelId,
                order = c.SortOrder,
                time = c.LastMessageTime,
                type = 0,
                unread = 0,
                avatar = c.ChannelId switch
                {
                    "-10_announcement" => "/static/images/system-announcement.png",
                    "-11_newbie" => "/static/images/newbie-mod-chat.png",
                    _ => ""
                }
            };
        }

        if (c.ChannelType == ChatChannelType.Private && userId > 0)
        {
            var otherId = c.GetOtherUserId(userId);
            // 排除自己与自己的私聊
            if (otherId == userId)
            {
                return null;
            }

            if (otherId.HasValue && userInfos.TryGetValue(otherId.Value, out var info))
            {
                return new()
                {
                    id = otherId.Value,
                    lastMsg = c.LastMessageContent ?? "",
                    name = info.Name,
                    avatar = info.HeadImgUrl ?? "",
                    order = c.SortOrder,
                    time = c.LastMessageTime,
                    type = 1,
                    unread = 0
                };
            }
        }

        return null;
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
        payOrder.CreateTopUpPay(amount, AbpSession.UserId!.Value, openid, app.Name, appid, mchid,
            AbpSession.TenantId!.Value);
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

        if (type == "h5")
        {
            var clientIp = GetClientIp();
            var h5Order = new CreateOrderRequest()
            {
                AppId = appid,
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
                SceneInfo = new CreateOrderRequest.CreateOrderSceneInfoModel
                {
                    PayerClientIp = clientIp
                },
            };

            var result = await v3PayApi.CreateH5OrderAsync(h5Order, app.GetValue("certPath"));

            return new { h5_url = result.H5Url, out_trade_no = payOrder.OutTradeNo };
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
        var user = await userRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == AbpSession.UserId!.Value);
        var auctionSuccess = await auctionItemRepository.GetAll().CountAsync(x => x.Status == AuctionStatusEnum.交易成功);
        var friend = await userFriendRepository.GetAll()
            .CountAsync(x => x.FriendId == AbpSession.UserId!.Value && x.Status == true);
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

    /// <summary>
    /// 执行聊天数据迁移（一次性操作）
    /// 将现有消息数据迁移到ChatChannel表
    /// </summary>
    /// <returns></returns>
    [HttpPost("MigrateChatData")]
    [AbpAuthorize] // 可以考虑添加管理员权限验证
    public async Task<object> MigrateChatData()
    {
        try
        {
            await chatChannelService.MigrateExistingMessagesToChannelsAsync();

            return new { success = true, message = "聊天数据迁移完成" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "聊天数据迁移失败");
            return new { success = false, message = $"迁移失败: {ex.Message}" };
        }
    }

    /// <summary>
    /// 同步用户删除状态
    /// 将 T_ChatListDelete 数据同步到 ChatChannel.UserStatus
    /// 用于修复数据不一致问题
    /// </summary>
    /// <returns></returns>
    [HttpPost("SyncUserDeleteStatus")]
    [AbpAuthorize]
    public async Task<object> SyncUserDeleteStatus()
    {
        try
        {
            await chatChannelService.SyncUserStatusFromChatListDeleteAsync(AbpSession.UserId!.Value);
            return new { success = true, message = "状态同步完成" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "状态同步失败");
            return new { success = false, message = $"同步失败: {ex.Message}" };
        }
    }

    /// <summary>
    /// 管理员：批量同步所有用户删除状态
    /// 将 T_ChatListDelete 表中所有用户的状态同步到 ChatChannel.UserStatus
    /// </summary>
    /// <returns></returns>
    [HttpPost("SyncAllUserDeleteStatus")]
    [AbpAuthorize]
    public async Task<object> SyncAllUserDeleteStatus()
    {
        try
        {
            await chatChannelService.SyncAllUserStatusFromChatListDeleteAsync();
            return new { success = true, message = "全量同步完成" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "全量同步失败");
            return new { success = false, message = $"同步失败: {ex.Message}" };
        }
    }

    /// <summary>
    /// 获取用户聊天频道统计信息
    /// </summary>
    /// <returns>聊天频道统计信息</returns>
    [HttpGet("GetChatChannelStats")]
    [AbpAuthorize]
    public async Task<ChatChannelStats> GetChatChannelStats()
    {
        var userId = AbpSession.UserId!.Value;

        // 获取用户可见的频道列表（内部已处理删除过滤）
        var channels = await chatChannelService.GetVisibleChannelsForUserAsync(userId);

        // 获取用户删除的聊天数量（使用用户状态字段）
        var deletedChatsCount = await chatChannelService.GetUserDeletedChannelsCountAsync(userId);

        return new ChatChannelStats
        {
            TotalChannels = channels.Count,
            SystemChannels = channels.Count(x => x.ChannelType == ChatChannelType.System),
            PrivateChannels = channels.Count(x => x.ChannelType == ChatChannelType.Private),
            DeletedChats = deletedChatsCount,
            TotalMessages = channels.Sum(x => x.MessageCount),
            LastActivity = channels.Where(x => x.LastMessageTime > 0).Any()
                ? channels.Where(x => x.LastMessageTime > 0).Max(x => x.LastMessageTime)
                : 0
        };
    }

    private string GetClientIp()
    {
        try
        {
            return httpContextAccessor.HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                   httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return "";
        }
    }
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

/// <summary>
/// 聊天频道统计信息
/// </summary>
public class ChatChannelStats
{
    /// <summary>
    /// 总频道数
    /// </summary>
    public int TotalChannels { get; set; }

    /// <summary>
    /// 系统频道数
    /// </summary>
    public int SystemChannels { get; set; }

    /// <summary>
    /// 私聊频道数
    /// </summary>
    public int PrivateChannels { get; set; }

    /// <summary>
    /// 已删除聊天数
    /// </summary>
    public int DeletedChats { get; set; }

    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public long LastActivity { get; set; }
}

/// <summary>
/// 批量删除聊天请求
/// </summary>
public class BatchDeleteChatRequest
{
    /// <summary>
    /// 要删除的用户ID列表
    /// </summary>
    public List<long> UserIds { get; set; } = new List<long>();
}