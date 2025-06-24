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
using TtWork.Project.Services;

using TtWork.Project.Services;

namespace TtWork.Project.Applications;

public class ClientAppService(
    IRepository<WechatPaymentNotification, Ulid> wechatPaymentNotificationRepository,
    IRepository<AuctionItem, long> auctionItemRepository,
    IRepository<UserFriend> userFriendRepository,
    IRepository<Message, Guid> messageRepository,
    IRepository<User, long> userRepository,
    IRepository<ChatListDelete> chatListDeleteRepository,
    ChatChannelService chatChannelService,
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
        var user = await userRepository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == AbpSession.UserId!.Value);
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
    /// 同时隐藏对应的ChatChannel频道
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

        // 保持原有的删除记录逻辑（用于兼容性）
        await chatListDeleteRepository.InsertAsync(new ChatListDelete()
        {
            UserId = userId,
            ToUserId = id
        });

        // 新增：隐藏对应的ChatChannel频道
        // 当有新消息时，会通过 ChatChannelService.UpdateChannelLastMessageAsync 自动恢复
        await chatChannelService.HideChannelForUserAsync(id, userId);

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// 批量删除聊天记录
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <returns></returns>
    [HttpPost]
    [AbpAuthorize]
    public async Task BatchDeleteChatList(BatchDeleteChatRequest request)
    {
        if (request?.UserIds == null || !request.UserIds.Any())
        {
            throw new UserFriendlyException("用户ID列表不能为空");
        }

        var userId = AbpSession.UserId!.Value;

        foreach (var id in request.UserIds)
        {
            // 检查是否已经删除过
            var existingRecord = await chatListDeleteRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.ToUserId == id);
            if (existingRecord == null)
            {
                await chatListDeleteRepository.InsertAsync(new ChatListDelete()
                {
                    UserId = userId,
                    ToUserId = id
                });

                // 隐藏对应的ChatChannel频道
                await chatChannelService.HideChannelForUserAsync(id, userId);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// 获取聊天列表（原始版本）
    /// 注意：此方法性能较差，建议使用 GetChatListOptimized 方法
    /// </summary>
    /// <returns>聊天列表</returns>
    [HttpGet]
    [Obsolete("此方法性能较差，建议使用 GetChatListOptimized 方法")]
    public async Task<List<ChatListItem>> GetChatList()
    {
        List<ChatListItem> s = [];
        // var lastlobbyMsg = await messageRepository.GetAll().AsNoTracking().Where(x => x.Chan == LOBBY && x.Type == ChatMessageType.Text).OrderByDescending(x => x.Time)
        // .FirstOrDefaultAsync();
        var lastauctionMsg = await messageRepository.GetAll().AsNoTracking()
            .Where(x => x.Chan == AUCTION && x.Type == ChatMessageType.Text).OrderByDescending(x => x.Time)
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
            time = lastauctionMsg?.Time ?? 0,
            type = 0,
            unread = 0
        });
        if (AbpSession.UserId.HasValue)
        {
            List<ChatListItem> s2 = [];
            var userId = AbpSession.UserId.Value;
            var chatDeleteList = await chatListDeleteRepository.GetAll().AsNoTracking().Where(x => x.UserId == userId)
                .ToListAsync();
            var messageList = await messageRepository.GetAll().AsNoTracking().Where(x =>
                    x.Chan == null && (x.To == userId || x.From == userId) && x.To != null)
                .OrderByDescending(x => x.Time)
                .GroupBy(m => new { m.From, m.To })
                .Select(g => new
                {
                    g.Key.To,
                    g.Key.From,
                    LatestMessage = g.OrderByDescending(m => m.Time).FirstOrDefault()
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

    /// <summary>
    /// 获取聊天列表（优化版本 - 使用ChatChannel表）
    /// 性能更优，支持大量聊天数据
    /// </summary>
    /// <returns>聊天列表</returns>
    [HttpGet("GetChatListOptimized")]
    public async Task<List<ChatListItem>> GetChatListOptimized()
    {
        var result = new List<ChatListItem>();

        // 获取用户已删除的聊天用户ID列表
        List<long> deletedUserIds = [];
        if (AbpSession.UserId.HasValue)
        {
            var userId = AbpSession.UserId.Value;
            var chatDeleteList = await chatListDeleteRepository.GetAll().AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync();

            deletedUserIds = chatDeleteList.Select(x => x.ToUserId).ToList();

            // 使用 ChatChannelService 获取可见的频道列表
            var channels = await chatChannelService.GetVisibleChannelsForUserAsync(userId, deletedUserIds);

            foreach (var channel in channels)
            {
                ChatListItem chatItem = null;

                if (channel.ChannelType == ChatChannelType.System)
                {
                    // 系统频道
                    chatItem = new ChatListItem
                    {
                        id = GetSystemChannelId(channel.ChannelId), // 转换为前端需要的ID格式
                        lastMsg = channel.LastMessageContent ?? "",
                        name = channel.ChannelName ?? channel.ChannelId,
                        order = channel.SortOrder,
                        time = channel.LastMessageTime,
                        type = 0, // 系统频道
                        unread = 0, // 暂时都为0，后续可以实现未读计数
                        avatar = ""
                    };
                }
                else if (channel.ChannelType == ChatChannelType.Private)
                {
                    // 私聊频道
                    var otherUserId = channel.GetOtherUserId(userId);

                    if (otherUserId.HasValue)
                    {
                        // 获取对方用户信息
                        var otherUser = await userCache.GetAsync(otherUserId.Value);
                        if (otherUser != null)
                        {
                            chatItem = new ChatListItem
                            {
                                id = otherUserId.Value,
                                lastMsg = channel.LastMessageContent ?? "",
                                name = otherUser.Name,
                                avatar = otherUser.HeadImgUrl ?? "",
                                order = channel.SortOrder,
                                time = channel.LastMessageTime,
                                type = 1, // 私聊
                                unread = 0 // 暂时都为0，后续可以实现未读计数
                            };
                        }
                        else
                        {
                            // 如果获取不到用户信息，使用消息中的发送者信息
                            chatItem = new ChatListItem
                            {
                                id = otherUserId.Value,
                                lastMsg = channel.LastMessageContent ?? "",
                                name = channel.LastMessageFromName ?? $"用户{otherUserId.Value}",
                                avatar = channel.LastMessageFromAvatar ?? "",
                                order = channel.SortOrder,
                                time = channel.LastMessageTime,
                                type = 1, // 私聊
                                unread = 0
                            };
                        }
                    }
                }

                if (chatItem != null)
                {
                    result.Add(chatItem);
                }
            }
        }
        else
        {
            // 未登录用户只显示系统频道
            var systemChannels = await chatChannelService.GetVisibleChannelsForUserAsync(0, null);
            foreach (var channel in systemChannels.Where(x => x.ChannelType == ChatChannelType.System))
            {
                var chatItem = new ChatListItem
                {
                    id = GetSystemChannelId(channel.ChannelId),
                    lastMsg = channel.LastMessageContent ?? "",
                    name = channel.ChannelName ?? channel.ChannelId,
                    order = channel.SortOrder,
                    time = channel.LastMessageTime,
                    type = 0,
                    unread = 0,
                    avatar = ""
                };
                result.Add(chatItem);
            }
        }

        return result;
    }

    /// <summary>
    /// 将系统频道ID转换为前端需要的数字ID
    /// </summary>
    /// <param name="channelId">频道ID</param>
    /// <returns>数字ID</returns>
    private static long GetSystemChannelId(string channelId)
    {
        return channelId switch
        {
            "-1_auction" => -1,
            "0_lobby" => 0,
            _ => channelId.GetHashCode() // 其他频道使用哈希值
        };
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
    /// 获取用户聊天频道统计信息
    /// </summary>
    /// <returns>聊天频道统计信息</returns>
    [HttpGet("GetChatChannelStats")]
    [AbpAuthorize]
    public async Task<ChatChannelStats> GetChatChannelStats()
    {
        var userId = AbpSession.UserId!.Value;

        // 获取用户已删除的聊天用户ID列表
        var chatDeleteList = await chatListDeleteRepository.GetAll().AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();
        var deletedUserIds = chatDeleteList.Select(x => x.ToUserId).ToList();

        // 获取用户可见的频道列表
        var channels = await chatChannelService.GetVisibleChannelsForUserAsync(userId, deletedUserIds);

        return new ChatChannelStats
        {
            TotalChannels = channels.Count,
            SystemChannels = channels.Count(x => x.ChannelType == ChatChannelType.System),
            PrivateChannels = channels.Count(x => x.ChannelType == ChatChannelType.Private),
            DeletedChats = deletedUserIds.Count,
            TotalMessages = channels.Sum(x => x.MessageCount),
            LastActivity = channels.Where(x => x.LastMessageTime > 0).Any() ?
                           channels.Where(x => x.LastMessageTime > 0).Max(x => x.LastMessageTime) : 0
        };
    }

    /// <summary>
    /// 获取可自动恢复的聊天频道列表
    /// 显示哪些隐藏的聊天可能会因为新消息而自动恢复
    /// </summary>
    /// <returns>可自动恢复的频道列表</returns>
    [HttpGet("GetAutoRestorableChannels")]
    [AbpAuthorize]
    public async Task<object> GetAutoRestorableChannels()
    {
        var userId = AbpSession.UserId!.Value;
        var restorableChannelIds = await chatChannelService.GetAutoRestorableChannelsAsync(userId);

        return new
        {
            count = restorableChannelIds.Count,
            channels = restorableChannelIds,
            message = restorableChannelIds.Count > 0
                ? $"有 {restorableChannelIds.Count} 个隐藏的聊天可能会因为新消息而自动恢复"
                : "当前没有可自动恢复的隐藏聊天"
        };
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