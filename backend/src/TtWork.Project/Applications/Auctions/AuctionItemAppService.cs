using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using FreeIM;
using FreeScheduler;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SqlSugar;
using SqlSugar.DistributedSystem.Snowflake;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.Abp.Definitions;
using TtWork.Abp.Entity;
using TtWork.Abp.Events.Commands;
using TtWork.Abp.Extensions;
using TtWork.Lib;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.GroupChatLevelSettings.Dto;
using TtWork.Project.Domains;
using TtWork.Project.Events;
using TtWork.Project.Events.Commands;
using TtWork.Project.Jobs;
using TTWork.WeiXinMiddleware.Utils;
using static OfficeOpenXml.ExcelErrorValue;
using Abp.Events.Bus;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using TtWork.Project.EventHandlers;
using TtWork.Project.Services.Cache;
using TtWork.Project.Services.Messaging;
using TtWork.Project.Services.Messaging.Models;
using TtWork.Project.Services;
using TtWork.Project.Services.Push;
using Newtonsoft.Json.Linq;

namespace TtWork.Project.Applications.Auctions;

public class SubStartNotifyRequest
{
    public long AuctionItemId { get; set; }
    public long? userId { get; set; }
    public string openid { get; set; }
    public string platform { get; set; }
}

public class AuctionItemAppService : AbpAsyncCrudAppService<AuctionItem, AuctionItemDto, long, AppResultRequestDto,
    AuctionItemCreateOrUpdateDto, AuctionItemCreateOrUpdateDto>
{
    private readonly UserCache _userCache;
    private new readonly IMediator _mediator;
    private readonly IRepository<AuctionItem, long> _repository;
    private readonly IRepository<BanedUser, long> _banedUserRepository;
    private readonly IRepository<BidHistory, long> _bidHistoryRepository;
    private readonly IRepository<AuctionStartNotify, long> _notifyRepository;

    private readonly IRepository<AuctionStartNotify, long> _auctionStartNotifyRepository;
    private readonly ILogger<AuctionItemAppService> _logger;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<ChatListDelete> _chatListDeleteRepository;
    private readonly IEventBus _eventBus;
    private readonly IAuctionItemCacheService _cacheService;
    private readonly IMessageSendingService _messageSendingService;
    private readonly IBidEligibilityService _bidEligibilityService;
    private readonly IMemoryCache _memoryCache;
    private readonly IRepository<UserLogin, long> _userLoginRepository;
    private readonly IJPushService _jPushService;
    private readonly IWebPushService _webPushService;
    private readonly IRepository<UserGroupLevel, int> _userGroupLevelRepository;
    private readonly IRepository<GroupChatLevelSetting, int> _groupChatLevelSettingRepository;

    // 内存锁字典（替代 Redis 分布式锁）
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> _auctionLocks = new();

    private const string KASEC_CACHE_PREFIX = "Kasec:";

    public AuctionItemAppService(
        IMemoryCache memoryCache,
        UserCache userCache,
        IMediator mediator,
        IRepository<AuctionItem, long> repository,
        IRepository<BanedUser, long> banedUserRepository,
        IRepository<BidHistory, long> bidHistoryRepository,
        IRepository<AuctionStartNotify, long> notifyRepository,
        IocManager iocManager,
        ILogger<AuctionItemAppService> logger,
        IRepository<AuctionStartNotify, long> auctionStartNotifyRepository,
        ISqlSugarClient sqlSugarClient,
        IRepository<Message, Guid> messageRepository,
        IHttpContextAccessor httpContextAccessor,
        IRepository<ChatListDelete> chatListDeleteRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IEventBus eventBus,
        IAuctionItemCacheService cacheService,
        IMessageSendingService messageSendingService,
        IBidEligibilityService bidEligibilityService,
        IRepository<UserLogin, long> userLoginRepository,
        IJPushService jPushService,
        IWebPushService webPushService,
        IRepository<UserGroupLevel, int> userGroupLevelRepository,
        IRepository<GroupChatLevelSetting, int> groupChatLevelSettingRepository) : base(repository, iocManager)
    {
        _sqlSugarClient = sqlSugarClient;
        _userCache = userCache;
        _mediator = mediator;
        _repository = repository;
        _banedUserRepository = banedUserRepository;
        _bidHistoryRepository = bidHistoryRepository;
        _notifyRepository = notifyRepository;
        _logger = logger;
        _auctionStartNotifyRepository = auctionStartNotifyRepository;
        _userLoginRepository = userLoginRepository;
        EnableGetEdit = true;

        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
        base.DeletePermissionName = AppPermissions.Pages.ChatManager;
        _unitOfWorkManager = unitOfWorkManager;
        _messageRepository = messageRepository;
        _httpContextAccessor = httpContextAccessor;
        _chatListDeleteRepository = chatListDeleteRepository;
        _eventBus = eventBus;
        _cacheService = cacheService;
        _messageSendingService = messageSendingService;
        _bidEligibilityService = bidEligibilityService;
        _memoryCache = memoryCache;
        _jPushService = jPushService;
        _webPushService = webPushService;
        _userGroupLevelRepository = userGroupLevelRepository;
        _groupChatLevelSettingRepository = groupChatLevelSettingRepository;
    }


    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="isAuction">是否是开始拍卖</param>
    /// <returns></returns>
    private async Task Notify(long id, string name, bool isAuction = true)
    {
        var subscribers = await _notifyRepository.GetAll().AsNoTracking()
            .Where(x => x.AuctionItemId == id)
            .Select(x => new { x.UserId, x.OpenId, x.Platform })
            .ToListAsync();

        _logger.LogInformation(
            "准备发送拍卖开拍订阅通知: AuctionItemId={AuctionItemId}, Name={Name}, SubscriberCount={Count}",
            id, name, subscribers.Count);

        if (subscribers.Count == 0)
        {
            _logger.LogWarning("没有订阅用户，跳过发送通知");
            return;
        }

        var title = name.Length > 16 ? name[..16] : name;
        var content = isAuction ? "拍卖即将开始，快来参与吧！" : "有人出价了，快来查看！";

        // 1. 发送微信模板消息（小程序端）- 使用小程序模板
        var miniprogramOpenIds = subscribers
            .Where(x => x.Platform == "miniprogram" && !string.IsNullOrEmpty(x.OpenId))
            .Select(x => x.OpenId)
            .ToArray();

        if (miniprogramOpenIds.Length > 0)
        {
            _logger.LogInformation("发送微信模板消息(小程序): {Count} 个用户", miniprogramOpenIds.Length);

            await _mediator.Publish(new Events.Commands.MessageSendCommand(
                Events.Commands.MessageType.WechatTemplate,
                new SendWechatTemplateDetail(
                    "uniapp",
                    miniprogramOpenIds,
                    "ZuYTYzw2cM0LVhF5ybH5iATMaDl6lZ82OC6cczsglEA", // 小程序模板ID
                    new
                    {
                        thing2 = new { value = title },
                        thing1 = new { value = isAuction ? "开始拍卖通知" : "出价通知" },
                    },
                    $"pages/index/index"
                )));
        }

        // 2. 发送通知（App端）- 微信订阅消息 + 极光推送
        var appSubscribers = subscribers
            .Where(x => x.Platform == "app")
            .ToList();

        if (appSubscribers.Count > 0)
        {
            // 2.1 发送微信模板消息
            var appOpenIds = appSubscribers
                .Where(x => !string.IsNullOrEmpty(x.OpenId))
                .Select(x => x.OpenId)
                .ToArray();

            _logger.LogInformation("[Notify] App订阅者详情: TotalCount={0}, WithOpenIdCount={1}, OpenIds={2}",
                appSubscribers.Count,
                appOpenIds.Length,
                string.Join(",", appOpenIds.Select(o => o ?? "NULL")));

            if (appOpenIds.Length > 0)
            {
                _logger.LogInformation("发送微信模板消息(App): {Count} 个用户", appOpenIds.Length);

                await _mediator.Publish(new Events.Commands.MessageSendCommand(
                    Events.Commands.MessageType.WechatTemplate,
                    new SendWechatTemplateDetail(
                        "app",
                        appOpenIds,
                        "aCmoAwuGevXMgA6mlq6x5pXrj7yNx5HJ6akzkHDCDPg",
                        new
                        {
                            thing2 = new { value = title },
                            thing1 = new { value = isAuction ? "开始拍卖通知" : "出价通知" },
                        },
                        $"pages/index/index"
                    )));
            }
            else
            {
                _logger.LogWarning(
                    "[Notify] App订阅者没有有效的OpenId，无法发送微信订阅消息: SubscriberCount={Count}, 有UserId但无OpenId将仅发送极光推送",
                    appSubscribers.Count);
            }

            // 2.2 发送极光推送
            var userIds = appSubscribers
                .Where(x => x.UserId.HasValue)
                .Select(x => $"user_{x.UserId}")
                .Distinct()
                .ToList();

            if (userIds.Count > 0)
            {
                _logger.LogInformation("发送极光推送(App): {Count} 个用户", userIds.Count);

                try
                {
                    var result = await _jPushService.SendByAliasAsync(
                        title,
                        content,
                        userIds,
                        new Dictionary<string, string>
                        {
                            { "type", isAuction ? "auction_start" : "auction_bid" },
                            { "auctionItemId", id.ToString() }
                        });

                    if (result.Success)
                    {
                        _logger.LogInformation("极光推送发送成功: MessageId={MessageId}", result.MessageId);
                    }
                    else
                    {
                        _logger.LogWarning("极光推送发送失败: {Error}", result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "极光推送发送异常");
                }
            }
        }

        // 3. 发送 WebPush（H5端）
        var h5Subscribers = subscribers
            .Where(x => x.Platform == "h5" && x.UserId.HasValue)
            .Select(x => x.UserId!.Value)
            .Distinct()
            .ToList();

        if (h5Subscribers.Count > 0)
        {
            _logger.LogInformation("发送 WebPush(H5): {Count} 个用户", h5Subscribers.Count);

            foreach (var userId in h5Subscribers)
            {
                try
                {
                    var result = await _webPushService.SendPushAsync(
                        userId,
                        title,
                        content,
                        null,
                        $"/pages/chat/auction?id={id}"
                    );

                    if (result.Success)
                    {
                        _logger.LogInformation("WebPush 发送成功: UserId={UserId}", userId);
                    }
                    else
                    {
                        _logger.LogWarning("WebPush 发送失败: UserId={UserId}, Error={Error}", userId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebPush 发送异常: UserId={UserId}", userId);
                }
            }
        }
    }


    [HttpPost]
    [AbpAuthorize]
    public async Task SubStartNotify(SubStartNotifyRequest input)
    {
        var platform = input.platform ?? "miniprogram";
        var userId = AbpSession.UserId;

        _logger.LogInformation("[SubStartNotify] 收到订阅请求: AuctionItemId={0}, UserId={1}, Platform={2}, OpenId={3}",
            input.AuctionItemId, userId, platform, input.openid ?? "NULL");

        var query = _auctionStartNotifyRepository.GetAll()
            .Where(x => x.AuctionItemId == input.AuctionItemId);

        if (platform == "miniprogram")
        {
            query = query.Where(x => x.OpenId == input.openid);
        }
        else if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        var exists = await query.AnyAsync();
        _logger.LogInformation("[SubStartNotify] 查询现有订阅: AuctionItemId={0}, Platform={1}, Exists={2}",
            input.AuctionItemId, platform, exists);

        if (!exists)
        {
            await _auctionStartNotifyRepository.InsertAsync(new AuctionStartNotify
            {
                AuctionItemId = input.AuctionItemId,
                UserId = userId,
                OpenId = input.openid,
                Platform = platform
            });

            _logger.LogInformation("[SubStartNotify] 保存订阅记录: AuctionItemId={0}, UserId={1}, Platform={2}, OpenId={3}",
                input.AuctionItemId, userId, platform, input.openid ?? "NULL");
        }
        else
        {
            _logger.LogInformation("[SubStartNotify] 订阅已存在，跳过保存: AuctionItemId={0}, Platform={1}",
                input.AuctionItemId, platform);
        }
    }

    /// <summary>
    /// 测试接口：手动触发拍卖开始通知
    /// </summary>
    [HttpPost]
    [AbpAuthorize]
    public async Task TestSendAuctionStartNotify(long auctionItemId)
    {
        var auctionItem = await _repository.FirstOrDefaultAsync(x => x.Id == auctionItemId);
        if (auctionItem == null)
        {
            throw new UserFriendlyException("拍品不存在");
        }

        _logger.LogInformation("手动触发拍卖开始通知测试: AuctionItemId={AuctionItemId}, Name={Name}", auctionItemId,
            auctionItem.Name);

        try
        {
            await Notify(auctionItem.Id, auctionItem.Name);
            _logger.LogInformation("手动触发拍卖开始通知成功: AuctionItemId={AuctionItemId}", auctionItemId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "手动触发拍卖开始通知失败: AuctionItemId={AuctionItemId}, Error={Error}", auctionItemId, e.Message);
            throw;
        }
    }

    /// <summary>
    /// 出价
    /// </summary>
    [AbpAuthorize]
    [HttpPost]
    public async Task<AuctionItemDto> Bid(BidHistoryCreateDto input)
    {
        long bidUserId = AbpSession.UserId!.Value;
        var user = await _userCache.GetAsync(bidUserId);

        if (user != null)
        {
            input.BidUserName = user.Name;
            input.BidUserAvatar = user.HeadImgUrl;
        }

        // 使用统一的出价资格检查服务
        var bidEligibilityCheck = await _bidEligibilityService.CheckBidEligibilityAsync(new CheckBidEligibilityInput
        {
            AuctionItemId = input.AuctionItemId,
            BidUserId = bidUserId.ToString(),
            BidUserName = input.BidUserName,
            BidPrice = input.BidPrice
        });

        if (!bidEligibilityCheck.CanBid)
        {
            throw new UserFriendlyException(bidEligibilityCheck.Reason);
        }

        // 内存锁，只让一个人出价（替代 Redis 分布式锁）
        var semaphore = _auctionLocks.GetOrAdd(input.AuctionItemId, _ => new SemaphoreSlim(1, 1));
        const int lockTimeoutMs = 10000;

        if (!await semaphore.WaitAsync(lockTimeoutMs))
        {
            throw new UserFriendlyException(1, "后台正在处理上一人出价,请稍后再试");
        }

        try
        {
            //查询商品信息
            var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.AuctionItemId);
            if (find == null)
            {
                throw new UserFriendlyException(1, "找不到商品");
            }

            if (find.Status != AuctionStatusEnum.拍卖中)
            {
                throw new UserFriendlyException(1, "商品不在拍卖中");
            }

            // ⚠️ 锁内部重新验证出价金额（防止 TOCTOU 并发竞争）
            // CheckBidEligibilityAsync 在锁外部执行，读取的可能是旧价格
            // 两个并发请求可能同时通过资格检查，但锁串行后第二个请求的价格可能已失效
            var kasecKey = $"{KASEC_CACHE_PREFIX}{input.AuctionItemId}";
            bool isKasec = _memoryCache.TryGetValue(kasecKey, out string kasecVal) && kasecVal == "true";

            var minPrice = AuctionItem.CalculateMinBidPrice(find.CurrentPrice, find.StartingPrice, isKasec);

            if (input.BidPrice < minPrice)
            {
                throw new UserFriendlyException(1,
                    $"当前最新价格已更新为{find.CurrentPrice ?? 0}元，最低出价为{minPrice}元，" +
                    $"您的出价{input.BidPrice}元已失效，请刷新后重新出价");
            }

            var addInfo = ObjectMapper.Map<BidHistory>(input);

            await _bidHistoryRepository.InsertAsync(addInfo);

            find.SetBid(input.BidPrice, bidUserId, input.BidUserName);

            await CurrentUnitOfWork.SaveChangesAsync();

            var result = ObjectMapper.Map<AuctionItemDto>(find);
            result.UseCountdownTime = addInfo.CreationTime;

            var msg = new ChatMessage()
            {
                type = ChatMessageType.AuctionBid,
                msg = $"{result.CurrentPrice}",
                payload = result,
                chan = "-1_auction",
                to = result.DealUserId
            };

            await _messageSendingService.SendChannelMessageAsync(bidUserId, "-1_auction", msg);
            var ip = GetIp;

            // 清除缓存，因为出价改变了商品状态
            await _cacheService.ClearAuctionDetailCacheAsync(input.AuctionItemId);
            await _cacheService.ClearAuctionListCacheAsync();
            await _cacheService.ClearCurrentAuctionCacheAsync();

            // 发布出价事件
            await _mediator.Publish(new BidPlacedEvent(input.AuctionItemId, bidUserId, input.BidPrice,
                input.BidUserName));

            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static Scheduler _scheduler;
    public static Dictionary<long, string> _tempTaskId = new Dictionary<long, string>();


    /// <summary>
    /// 执行定时任务回调
    /// </summary>
    public async virtual void Callback(AuctionItemDto dto, ChatMessage message, UserDto cacheUser, string ip,
        AuctionItem auctionItem)
    {
        //手动结束竞拍不在运行
        if (!_tempTaskId.ContainsKey(auctionItem.Id))
        {
            return;
        }

        //移除任务
        _scheduler.RemoveTempTask(_tempTaskId[auctionItem.Id]);
        _tempTaskId.Remove(auctionItem.Id);
        _scheduler.Dispose();
        _scheduler = null;

        // 定时任务独立处理流程
        using (var uow = _unitOfWorkManager.Begin())
        {
            try
            {
                _logger.LogInformation(
                    "========== 开始定时结束拍卖 ========== AuctionItemId={AuctionItemId}, Operation=Scheduled",
                    auctionItem.Id);

                // 查询最新的商品信息
                var find = await Repository.FirstOrDefaultAsync(x => x.Id == auctionItem.Id);
                if (find == null)
                {
                    _logger.LogError("定时任务回调：找不到商品，ID: {AuctionItemId}", auctionItem.Id);
                    return;
                }

                // 检查商品状态
                if (find.Status == AuctionStatusEnum.已成交)
                {
                    _logger.LogWarning("定时任务回调：商品已成交，无需处理，ID: {AuctionItemId}", auctionItem.Id);
                    return;
                }

                // 获取拍卖师信息（定时任务使用拍卖师身份）
                var auctionManagerInfo = await _sqlSugarClient
                    .Queryable<RoleEntity, UserRoleEntity, UserInfoEntity>((r, ur, u) =>
                        new JoinQueryInfos(
                            JoinType.Inner, r.Id == ur.RoleId, JoinType.Inner, ur.UserId == u.Id
                        ))
                    .Where((r, ur, u) => r.Name == "AuctionManager")
                    .Select((r, ur, u) => new UserInfoEntity
                    {
                        Id = u.Id,
                        Name = u.Name,
                        HeadImgUrl = u.HeadImgUrl,
                        LastModifierUserId = u.LastModifierUserId,
                    })
                    .FirstAsync();

                // 获取并处理卡秒状态（使用内存缓存）
                var kasecKey = $"Kasec:{auctionItem.Id}";
                bool wasInKasecMode = _memoryCache.TryGetValue(kasecKey, out string kasecVal) && kasecVal == "true";

                // 定时结束拍卖时将卡秒状态设置为false
                _memoryCache.Set(kasecKey, "false", TimeSpan.FromMinutes(30));

                bool hasBids = find.CurrentPrice != null;
                AuctionItemDto result;

                // ============ 第一阶段：完成所有数据库操作 ============
                if (!hasBids)
                {
                    // 无出价情况：回退到待拍卖状态
                    find.Back();
                    await CurrentUnitOfWork.SaveChangesAsync();
                    result = ObjectMapper.Map<AuctionItemDto>(find);
                }
                else
                {
                    // 有出价情况：设置为已成交

                    // 验证出价记录一致性
                    var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                        .Where(x => x.AuctionItemId == auctionItem.Id && !x.IsRollBack)
                        .OrderByDescending(x => x.BidPrice)
                        .FirstOrDefaultAsync();

                    if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
                    {
                        _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
                    }

                    // 设置商品为已成交状态
                    _logger.LogInformation(
                        "准备设置成交状态: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, FinalPrice={FinalPrice}",
                        find.Id, find.CurrentPriceUserId, find.CurrentPrice);

                    find.SetDeal();

                    // 保存成交状态所需的数据，用于群聊等级计算
                    long? groupChatLevelUserId = find.CurrentPriceUserId;
                    decimal groupChatLevelAmount = maxPrice != null ? Convert.ToDecimal(maxPrice.BidPrice) : 0;

                    // 群聊等级计算（在事务内执行，与成交状态保持原子性）
                    if (groupChatLevelUserId.HasValue && groupChatLevelAmount > 0)
                    {
                        await AddUserGroupChatLevelIncrement(groupChatLevelUserId.Value, groupChatLevelAmount);
                    }

                    _logger.LogInformation("成交状态和群聊等级已处理: AuctionItemId={AuctionItemId}", find.Id);

                    // 构建返回结果
                    result = ObjectMapper.Map<AuctionItemDto>(find);
                    result.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                                       ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";

                    if (result.DealUserId.HasValue)
                    {
                        var bidUser = await _userCache.GetAsync(result.DealUserId.Value);
                        result.DealUserAvatar = bidUser.HeadImgUrl;
                    }
                    else
                    {
                        _logger.LogWarning("成交用户ID为空，无法获取用户头像信息, AuctionItemId={AuctionItemId}", auctionItem.Id);
                    }

                    // 定时结束特有的逻辑：清理聊天删除记录
                    if (result.DealUserId.HasValue)
                    {
                        await _chatListDeleteRepository.GetAll().Where(x =>
                                (x.UserId == cacheUser.Id && x.ToUserId == result.DealUserId.Value) ||
                                (x.UserId == result.DealUserId.Value && x.ToUserId == cacheUser.Id))
                            .ExecuteDeleteAsync();
                    }
                }

                // ============ 第二阶段：清除缓存 ============
                await _cacheService.ClearAuctionListCacheAsync();
                await _cacheService.ClearAuctionDetailCacheAsync(auctionItem.Id);
                await _cacheService.ClearCurrentAuctionCacheAsync();

                // ============ 第三阶段：发送消息通知 ============
                // 发送卡秒关闭消息（如果当前处于卡秒状态）
                if (wasInKasecMode)
                {
                    var kasecMsg = new ChatMessage
                    {
                        type = ChatMessageType.KasecStatusChanged,
                        chan = "-1_auction",
                        msg = "拍卖已结束，卡秒自动关闭",
                        payload = new { auctionItemId = auctionItem.Id, isKasec = false }
                    };

                    await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id, null, "-1_auction",
                        kasecMsg, true);

                    _logger.LogInformation("卡秒状态已关闭，拍卖ID: {AuctionItemId}, 结束类型: Scheduled, 操作者: {OperatorName}",
                        auctionItem.Id, auctionManagerInfo.Name);
                }

                if (!hasBids)
                {
                    // 发送流拍消息
                    var flowMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionEnd,
                        chan = "-1_auction",
                        msg = "拍卖结束，无人出价，商品已回退",
                        payload = result
                    };

                    try
                    {
                        await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id, null, "-1_auction",
                            flowMessage, true);
                        _logger.LogInformation("定时任务流拍消息发送成功: AuctionItemId={AuctionItemId}", auctionItem.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "定时任务流拍消息发送失败: AuctionItemId={AuctionItemId}, Error={Error}",
                            auctionItem.Id, ex.Message);
                    }
                }
                else
                {
                    // 发送拍卖成功消息
                    var successMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionEnd,
                        chan = "-1_auction",
                        msg = $"恭喜 {result.DealUserName} 以 ￥{result.FinalPrice} 拍得 {result.Name}",
                        payload = result
                    };

                    try
                    {
                        await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id, null, "-1_auction",
                            successMessage, true);
                        _logger.LogInformation(
                            "定时任务拍卖成功消息发送成功: AuctionItemId={AuctionItemId}, DealUserName={DealUserName}",
                            auctionItem.Id, result.DealUserName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "定时任务拍卖成功消息发送失败: AuctionItemId={AuctionItemId}, Error={Error}",
                            auctionItem.Id, ex.Message);
                    }

                    // 发送成交用户私信（使用编码机制，将AuctionDeal编码为AuctionEnd类型）
                    var dealMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionDeal, // 原始类型，会被自动编码为AuctionEnd
                        msg = result.ToUserMsg,
                        payload = result
                    };

                    if (result.DealUserId.HasValue)
                    {
                        _logger.LogInformation(
                            "开始发送拍卖成交私信: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, ToUserMsg={ToUserMsg}",
                            auctionItem.Id, result.DealUserId.Value, result.ToUserMsg);

                        try
                        {
                            // 使用SendPrivateMessageAsync，会自动将AuctionDeal编码为AuctionEnd类型
                            await _messageSendingService.SendPrivateMessageAsync(auctionManagerInfo.Id,
                                result.DealUserId.Value, dealMessage, false, null);

                            _logger.LogInformation("拍卖成交私信发送成功: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}",
                                auctionItem.Id, result.DealUserId.Value);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "拍卖成交私信发送失败: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, Error={Error}",
                                auctionItem.Id, result.DealUserId.Value, ex.Message);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("成交用户ID为空，无法发送私信, AuctionItemId={AuctionItemId}", auctionItem.Id);
                    }
                }

                // 发布拍卖结束事件
                await _mediator.Publish(new AuctionEndedEvent(result, hasBids));

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务回调处理失败，商品ID: {AuctionItemId}, Error: {Error}", auctionItem.Id, ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// 累加用户群聊等级金额（用于拍卖成交时）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="incrementAmount">增量金额（本次出价金额）</param>
    /// <returns></returns>
    private const decimal MaxCumulativeAmount = 999999999;

    private async Task AddUserGroupChatLevelIncrement(long userId, decimal incrementAmount)
    {
        try
        {
            _logger.LogInformation("开始累加用户群聊等级金额: UserId={UserId}, IncrementAmount={IncrementAmount}",
                userId, incrementAmount);

            if (incrementAmount < 0)
            {
                _logger.LogWarning("增量金额为负数，跳过处理: UserId={UserId}, IncrementAmount={IncrementAmount}",
                    userId, incrementAmount);
                return;
            }

            if (incrementAmount > MaxCumulativeAmount)
            {
                _logger.LogWarning("增量金额过大，跳过处理: UserId={UserId}, IncrementAmount={IncrementAmount}",
                    userId, incrementAmount);
                return;
            }

            var info = await _userGroupLevelRepository.GetAll()
                .FirstOrDefaultAsync(f => f.UserId == userId);
            if (info == null)
            {
                info = new UserGroupLevel { CumulativeAmount = 0 };
                _logger.LogInformation("用户群聊等级信息不存在，创建新记录: UserId={UserId}", userId);
            }
            else
            {
                _logger.LogInformation(
                    "查询到用户群聊等级信息: UserId={UserId}, CurrentAmount={CurrentAmount}, GroupChatId={GroupChatId}",
                    userId, info.CumulativeAmount, info.GroupChatId);
            }

            decimal newCumulativeAmount = info.CumulativeAmount + incrementAmount;
            if (newCumulativeAmount > MaxCumulativeAmount)
            {
                _logger.LogWarning(
                    "累计金额过大，限制为最大值: UserId={UserId}, OldAmount={OldAmount}, IncrementAmount={IncrementAmount}, NewAmount={NewAmount}",
                    userId, info.CumulativeAmount, incrementAmount, newCumulativeAmount);
                newCumulativeAmount = MaxCumulativeAmount;
            }

            var groupChatLevelSetting = await _groupChatLevelSettingRepository.GetAll()
                .Where(w => w.AmountRequired <= newCumulativeAmount)
                .OrderByDescending(o => o.AmountRequired)
                .FirstOrDefaultAsync();
            if (groupChatLevelSetting == null)
            {
                _logger.LogWarning("没有匹配的群聊等级信息，跳过等级更新: UserId={UserId}, CumulativeAmount={CumulativeAmount}",
                    userId, newCumulativeAmount);
                return;
            }

            _logger.LogInformation(
                "匹配到群聊等级信息: UserId={UserId}, LevelId={LevelId}, LevelName={LevelName}, AmountRequired={AmountRequired}",
                userId, groupChatLevelSetting.Id, groupChatLevelSetting.Name, groupChatLevelSetting.AmountRequired);

            if (info != null && info.Id != 0)
            {
                info.CumulativeAmount = newCumulativeAmount;
                info.GroupChatId = (int)groupChatLevelSetting.Id;

                _logger.LogInformation(
                    "更新用户群聊等级: UserId={UserId}, NewAmount={NewAmount}, NewGroupChatId={NewGroupChatId}",
                    userId, newCumulativeAmount, groupChatLevelSetting.Id);

                await _userGroupLevelRepository.UpdateAsync(info);
            }
            else
            {
                _logger.LogInformation(
                    "插入新用户群聊等级: UserId={UserId}, CumulativeAmount={CumulativeAmount}, GroupChatId={GroupChatId}",
                    userId, newCumulativeAmount, groupChatLevelSetting.Id);

                await _userGroupLevelRepository.InsertAsync(new UserGroupLevel
                {
                    UserId = userId,
                    CumulativeAmount = newCumulativeAmount,
                    GroupChatId = (int)groupChatLevelSetting.Id,
                });
            }

            _logger.LogInformation("用户群聊等级金额累加完成: UserId={UserId}, FinalAmount={FinalAmount}",
                userId, newCumulativeAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "累加用户群聊等级金额失败，跳过等级更新: UserId={UserId}, IncrementAmount={IncrementAmount}",
                userId, incrementAmount);
        }
    }

    /// <summary>
    /// 设置用户群聊等级总金额（用于手动调整时）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="totalAmount">总金额</param>
    /// <returns></returns>
    private async Task SetUserGroupChatLevelTotal(long userId, decimal totalAmount)
    {
        try
        {
            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
                .Where(w => w.AmountRequired <= totalAmount) // 找到小于等于当前累计金额的等级配置
                .OrderByDescending(o => o.AmountRequired) // 按金额要求降序排序，找到最接近的等级
                .FirstAsync();
            if (groupChatLevelSettings == null)
            {
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }

            //查询用户等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>().FirstAsync(f => f.UserId == userId);
            if (info != null)
            {
                info.CumulativeAmount = totalAmount;
                info.GroupChatId = groupChatLevelSettings.Id;
                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            else
            {
                await _sqlSugarClient.Insertable(new UserGroupLevelEntity
                {
                    UserId = userId,
                    CumulativeAmount = totalAmount,
                    GroupChatId = groupChatLevelSettings.Id,
                }).ExecuteCommandAsync();
            }
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"设置用户群聊等级总金额失败，错误信息：" + ex.Message);
        }
    }

    /// <summary>
    /// 添加用户群聊等级（原有方法，保持向后兼容）
    /// 注意：此方法中的CumulativeAmount参数实际作为增量金额处理，建议使用新的重载方法以获得更清晰的语义
    /// </summary>
    /// <param name="input">用户群聊等级信息，其中CumulativeAmount作为增量金额</param>
    /// <returns></returns>
    private async Task AddUserGroupChatLevel(UserGroupLevelDto input)
    {
        try
        {
            //查询用户群聊等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .FirstAsync(f => f.UserId == input.UserId);
            if (info == null)
            {
                info = new UserGroupLevelEntity() { CumulativeAmount = 0 };
            }

            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
                .Where(w => w.AmountRequired <= (input.CumulativeAmount + info.CumulativeAmount)) // 找到小于等于当前累计金额的等级配置
                .OrderByDescending(o => o.AmountRequired) // 按金额要求降序排序，找到最接近的等级
                .FirstAsync();
            if (groupChatLevelSettings == null)
            {
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }

            //存在用户群聊等级信息就修改
            if (info != null && info.Id != 0)
            {
                info.CumulativeAmount += input.CumulativeAmount;
                info.GroupChatId = groupChatLevelSettings.Id;
                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            else
            {
                await _sqlSugarClient.Insertable(new UserGroupLevelEntity
                {
                    UserId = input.UserId,
                    CumulativeAmount = input.CumulativeAmount,
                    GroupChatId = groupChatLevelSettings.Id,
                }).ExecuteCommandAsync();
            }
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"添加失败，错误信息：" + ex.Message);
        }
    }

    public string GetIp
    {
        get
        {
            try
            {
                return _httpContextAccessor!.HttpContext!.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                       _httpContextAccessor!.HttpContext!.Request.HttpContext!.Connection!.RemoteIpAddress!
                           .ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

    /// <summary>
    /// 判断时间是否到达指定的时、分、秒
    /// </summary>
    /// <param name="t1">当前时间</param>
    /// <param name="t2">目标时间</param>
    /// <returns>true:当前时间大于等于目标时间，false:当前时间小于目标时间</returns>
    private bool CompareTimeToSecond(DateTime t1, DateTime t2)
    {
        TimeSpan ts1 = new TimeSpan(0, t1.Hour, t1.Minute, t1.Second);
        TimeSpan ts2 = new TimeSpan(0, t2.Hour, t2.Minute, t2.Second);
        return ts1 >= ts2;
    }

    public override Task<AuctionItemDto> GetAsync(EntityDto<long> input)
    {
        return base.GetAsync(input);
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("api/AuctionItem/GetDetail")]
    public async Task<AuctionItemDto> GetDetail(long id)
    {
        // 使用新的缓存服务
        var result = await _cacheService.GetAuctionDetailAsync(id);

        if (result == null)
        {
            throw new UserFriendlyException(1, "找不到商品");
        }

        return result;
    }

    public static object lockObj = new object();

    /// <summary>
    /// 结束竞拍
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [AbpAuthorize(AppPermissions.Pages.ChatManager)]
    [HttpGet]
    [UnitOfWork]
    public async Task<AuctionItemDto> EndAuction(EntityDto<long> input)
    {
        try
        {
            _logger.LogInformation(
                "========== 开始结束拍卖 ========== AuctionItemId={AuctionItemId}, UserId={UserId}, Operation=Manual",
                input.Id, AbpSession.UserId);

            // 检查用户登录状态
            if (!AbpSession.UserId.HasValue)
            {
                throw new UserFriendlyException(1, "用户未登录");
            }

            // 查询商品信息
            var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (find == null)
            {
                throw new UserFriendlyException(1, "找不到商品");
            }

            // 检查商品状态
            if (find.Status == AuctionStatusEnum.已成交)
            {
                var existingResult = ObjectMapper.Map<AuctionItemDto>(find);
                existingResult.ToUserMsg = "已成交商品不能再次拍卖";
                return existingResult;
            }

            // 获取当前用户信息
            var currentUser = await _userCache.GetAsync(AbpSession.UserId.Value);
            if (currentUser == null)
            {
                throw new UserFriendlyException(1, "获取用户信息失败");
            }

            // 获取并处理卡秒状态（使用内存缓存）
            var kasecKey = $"Kasec:{input.Id}";
            bool wasInKasecMode = _memoryCache.TryGetValue(kasecKey, out string kasecVal) && kasecVal == "true";

            // 手动结束拍卖时将卡秒状态设置为false
            _memoryCache.Set(kasecKey, "false", TimeSpan.FromMinutes(30));

            AuctionItemDto result;
            bool hasBids = find.CurrentPrice != null;

            // ============ 第一阶段：完成所有数据库操作 ============
            if (!hasBids)
            {
                // 无出价情况：回退到待拍卖状态
                find.Back();
                await CurrentUnitOfWork.SaveChangesAsync();
                result = ObjectMapper.Map<AuctionItemDto>(find);
            }
            else
            {
                // 有出价情况：设置为已成交

                // 验证出价记录一致性
                var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                    .Where(x => x.AuctionItemId == input.Id && !x.IsRollBack)
                    .OrderByDescending(x => x.BidPrice)
                    .FirstOrDefaultAsync();

                if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
                {
                    _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
                }

                // 设置商品为已成交状态
                _logger.LogInformation(
                    "准备设置成交状态: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, FinalPrice={FinalPrice}",
                    find.Id, find.CurrentPriceUserId, find.CurrentPrice);

                find.SetDeal();

                // 保存成交状态所需的数据，用于群聊等级计算
                long? groupChatLevelUserId = find.CurrentPriceUserId;
                decimal groupChatLevelAmount = maxPrice != null ? Convert.ToDecimal(maxPrice.BidPrice) : 0;

                // 群聊等级计算（在事务内执行，与成交状态保持原子性）
                if (groupChatLevelUserId.HasValue && groupChatLevelAmount > 0)
                {
                    await AddUserGroupChatLevelIncrement(groupChatLevelUserId.Value, groupChatLevelAmount);
                }

                _logger.LogInformation("成交状态和群聊等级已处理: AuctionItemId={AuctionItemId}", find.Id);

                // 构建返回结果
                result = ObjectMapper.Map<AuctionItemDto>(find);
                result.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                                   ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";

                if (result.DealUserId.HasValue)
                {
                    var bidUser = await _userCache.GetAsync(result.DealUserId.Value);
                    result.DealUserAvatar = bidUser.HeadImgUrl;
                }
                else
                {
                    _logger.LogWarning("成交用户ID为空，无法获取用户头像信息, AuctionItemId={AuctionItemId}", find.Id);
                }
            }

            // ============ 第二阶段：清除缓存 ============
            await _cacheService.ClearAuctionListCacheAsync();
            await _cacheService.ClearAuctionDetailCacheAsync(input.Id);
            await _cacheService.ClearCurrentAuctionCacheAsync();

            // ============ 第三阶段：发送消息通知 ============
            // 发送卡秒关闭消息（如果当前处于卡秒状态）
            if (wasInKasecMode)
            {
                var kasecMsg = new ChatMessage
                {
                    type = ChatMessageType.KasecStatusChanged,
                    chan = "-1_auction",
                    msg = "卡秒已关闭，恢复正常加价",
                    payload = new { auctionItemId = input.Id, isKasec = false }
                };

                await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction",
                    kasecMsg, true);

                _logger.LogInformation("卡秒状态已关闭，拍卖ID: {AuctionItemId}, 结束类型: Manual, 操作者: {OperatorName}",
                    input.Id, currentUser.Name);
            }

            if (!hasBids)
            {
                // 发送流拍消息
                var flowMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionEnd,
                    chan = "-1_auction",
                    msg = "拍卖结束，无人出价，商品已回退",
                    payload = result
                };

                try
                {
                    await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction",
                        flowMessage, true);
                    _logger.LogInformation("流拍消息发送成功: AuctionItemId={AuctionItemId}", input.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "流拍消息发送失败: AuctionItemId={AuctionItemId}, Error={Error}", input.Id,
                        ex.Message);
                }
            }
            else
            {
                // 发送拍卖成功消息
                var successMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionEnd,
                    chan = "-1_auction",
                    msg = $"恭喜 {result.DealUserName} 以 ￥{result.FinalPrice} 拍得 {result.Name}",
                    payload = result
                };

                try
                {
                    await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction",
                        successMessage, true);
                    _logger.LogInformation("拍卖成功消息发送成功: AuctionItemId={AuctionItemId}, DealUserName={DealUserName}",
                        input.Id, result.DealUserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "拍卖成功消息发送失败: AuctionItemId={AuctionItemId}, Error={Error}", input.Id,
                        ex.Message);
                }

                // 发送成交用户私信（使用编码机制，将AuctionDeal编码为AuctionEnd类型）
                var dealMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionDeal, // 原始类型，会被自动编码为AuctionEnd
                    msg = result.ToUserMsg,
                    payload = result
                };

                if (result.DealUserId.HasValue)
                {
                    _logger.LogInformation(
                        "开始发送手动结束拍卖成交私信: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, ToUserMsg={ToUserMsg}",
                        input.Id, result.DealUserId.Value, result.ToUserMsg);

                    try
                    {
                        // 使用SendPrivateMessageAsync，会自动将AuctionDeal编码为AuctionEnd类型
                        await _messageSendingService.SendPrivateMessageAsync(AbpSession.UserId.Value,
                            result.DealUserId.Value, dealMessage, false, null);

                        _logger.LogInformation("手动结束拍卖成交私信发送成功: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}",
                            input.Id, result.DealUserId.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "手动结束拍卖成交私信发送失败: AuctionItemId={AuctionItemId}, DealUserId={DealUserId}, Error={Error}",
                            input.Id, result.DealUserId.Value, ex.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("成交用户ID为空，无法发送私信, AuctionItemId={AuctionItemId}", input.Id);
                }
            }

            // 发布拍卖结束事件
            await _mediator.Publish(new AuctionEndedEvent(result, hasBids));

            _logger.LogInformation(
                "========== 结束拍卖成功 ========== AuctionItemId={AuctionItemId}, DealUserName={DealUserName}",
                input.Id, result.DealUserName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "========== 结束拍卖失败 ========== AuctionItemId={AuctionItemId}, Error={Error}, StackTrace={StackTrace}",
                input.Id, ex.Message, ex.StackTrace);
            throw new UserFriendlyException(1, $"系统内部错误，请稍后重试。错误ID: {Guid.NewGuid()}");
        }
    }

    /// <summary>
    /// 拍卖物品
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [AbpAuthorize(AppPermissions.Pages.ChatManager)]
    [HttpGet]
    public async Task<AuctionItemDto> StartAuction(EntityDto<long> input)
    {
        //获取拍卖中的物品
        var getAuctionMidList = await Repository.GetAll().AsNoTracking().Where(x => x.Status == AuctionStatusEnum.拍卖中)
            .ToListAsync();
        if (getAuctionMidList.Count > 0)
        {
            throw new UserFriendlyException("已存在拍卖的商品！");
        }

        var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

        if (find == null)
        {
            throw new UserFriendlyException(1, "找不到商品");
        }

        if (find.Status == AuctionStatusEnum.拍卖中)
        {
            throw new UserFriendlyException("当前商品已在拍卖中");
        }

        if (find.Status == AuctionStatusEnum.已成交)
            throw new UserFriendlyException(1, "已成交商品不能再次拍卖");

        find.StartAuction();
        await CurrentUnitOfWork.SaveChangesAsync();

        // 清除缓存
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearAuctionDetailCacheAsync(input.Id);
        await _cacheService.ClearCurrentAuctionCacheAsync();

        // 发布拍卖开始事件
        var auctionDto = ObjectMapper.Map<AuctionItemDto>(find);
        await _mediator.Publish(new AuctionStartedEvent(auctionDto));

        try
        {
            await Notify(find.Id, find.Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "========== 发送拍卖开始通知失败 ========== AuctionItemId={AuctionItemId}, AuctionName={AuctionName}, Error={Error}, StackTrace={StackTrace}",
                find.Id, find.Name, e.Message, e.StackTrace);
            // 注意：这里不抛出异常，因为拍卖流程本身已经成功，只是通知失败了
        }

        return ObjectMapper.Map<AuctionItemDto>(find);
    }

    /// <summary>
    /// 获取待拍卖商品跟已拍卖商品（需要登录）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<ListResultDto<AuctionItemDto>> GetPublicList(AppResultRequestDto input)
    {
        // 如果没有传递 MaxResultCount，设置默认值 100
        if (input.MaxResultCount <= 0)
        {
            input.MaxResultCount = 100;
        }

        // 使用新的缓存服务
        return await _cacheService.GetAuctionListAsync(input);
    }

    /// <summary>
    /// 获取待拍卖商品跟已拍卖商品（无需登录的公开接口）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisableAuditing]
    [HttpGet("api/AuctionItem/GetPublicListAnonymous")]
    public async Task<PagedResultDto<AuctionItemDto>> GetPublicListAnonymous(AppResultRequestDto input)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 如果没有传递 MaxResultCount，设置默认值 100
        if (input.MaxResultCount <= 0)
        {
            input.MaxResultCount = 100;
        }

        // 使用新的缓存服务
        var result = await _cacheService.GetAuctionListAsync(input);

        sw.Stop();
        _logger.LogInformation("[PERF-API] GetPublicListAnonymous 总耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        return result;
    }


    /// <summary>
    /// 查询拍卖中的商品
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AbpAuthorize]
    [DisableAuditing]
    [HttpPost]
    public async Task<ListResultDto<AuctionItemDto>> GetAuctionMidList(AppResultRequestDto input)
    {
        // 使用新的缓存服务
        return await _cacheService.GetAuctionMidListAsync(input);
    }

    [AbpAuthorize]
    public async Task<PagedResultDto<AuctionItemDto>> GetMySuccessList(AppResultRequestDto input)
    {
        input.UserId = AbpSession.UserId!.Value;
        input.Status = (int)AuctionStatusEnum.已成交;
        input.Sorting = "DealTime desc";
        return await GetAllAsync(input);
    }

    [AbpAuthorize]
    public override Task<PagedResultDto<AuctionItemDto>> GetAllAsync(AppResultRequestDto input)
    {
        return base.GetAllAsync(input);
    }

    protected override IQueryable<AuctionItem> ApplySorting(IQueryable<AuctionItem> query,
        AppResultRequestDto input)
    {
        return base.ApplySorting(query, input);
    }

    protected override IQueryable<AuctionItem> CreateFilteredQuery(AppResultRequestDto input)
    {
        return base.CreateFilteredQuery(input)
                .WhereIf(input.Status.HasValue, x => x.Status == (AuctionStatusEnum)input.Status)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Name.Contains(input.Keyword))
                .WhereIf(input.UserId.HasValue, x => x.DealUserId == input.UserId.Value) //成功拍得
            ;
    }


    [HttpGet]
    [AbpAuthorize]
    public async Task<object> DateAnlayse(AppResultRequestDto input)
    {
        var query = await Repository.GetAll()
            .WhereIf(input.Status.HasValue, x => x.Status == AuctionStatusEnum.已成交)
            .WhereIf(input.From.HasValue, x => x.CreationTime >= input.From)
            .WhereIf(input.To.HasValue, x => x.CreationTime <= input.From)
            .GroupBy(row => new
            {
                row.CreationTime.Year,
                row.CreationTime.Month,
                row.CreationTime.Date
            }).Select(grp => new
            {
                Label = $"{grp.Key.Date.Month}月{grp.Key.Date.Day}日",
                grp.Key.Year,
                grp.Key.Month,
                grp.Key.Date,
                Count = grp.Sum(x => x.FinalPrice)
            }).ToListAsync();
        return query.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Date);
    }

    [HttpGet]
    [AbpAuthorize]
    public async Task<object> DateAnlayse2(AppResultRequestDto input)
    {
        var query = await Repository.GetAll()
            .WhereIf(input.Status.HasValue, x => x.Status == AuctionStatusEnum.已成交)
            .WhereIf(input.From.HasValue, x => x.CreationTime >= input.From)
            .WhereIf(input.To.HasValue, x => x.CreationTime <= input.From)
            .GroupBy(row => new
            {
                row.CreationTime.Year,
                row.CreationTime.Month,
                row.CreationTime.Date
            }).Select(grp => new
            {
                Label = $"{grp.Key.Date.Month}月{grp.Key.Date.Day}日",
                grp.Key.Year,
                grp.Key.Month,
                grp.Key.Date,
                Count = grp.Count()
            }).ToListAsync();
        return query.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Date);
    }

    // 1. 设置卡秒状态（管理员权限）
    public class SetKasecStatusInput
    {
        public long AuctionItemId { get; set; }
        public bool IsKasec { get; set; }
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Pages.ChatManager)]
    public async Task<bool> SetKasecStatus([FromBody] SetKasecStatusInput input)
    {
        _logger.LogInformation("=== 卡秒状态设置开始 === AuctionItemId={AuctionItemId}, IsKasec={IsKasec}, UserId={UserId}",
            input.AuctionItemId, input.IsKasec, AbpSession.UserId.Value);

        try
        {
            // 设置卡秒状态到内存缓存
            var kasecKey = $"Kasec:{input.AuctionItemId}";
            var kasecValue = input.IsKasec.ToString().ToLower();

            _memoryCache.Set(kasecKey, kasecValue, TimeSpan.FromMinutes(30));
            _logger.LogInformation("[PERF-Cache] 卡秒状态已设置: Key={KasecKey}, Value={KasecValue}", kasecKey, kasecValue);

            // 获取当前用户信息（拍卖师）
            // var currentUser = await _userCache.GetAsync(AbpSession.UserId.Value);
            // var (isAdmin, adminTag, tagClass) = await CheckIsChatAdmin();

            // 广播卡秒状态变更消息 - 修复：使用拍卖消息发送方法，提高发送速度
            var msg = new ChatMessage
            {
                type = ChatMessageType.KasecStatusChanged,
                chan = "-1_auction",
                msg = input.IsKasec ? "拍卖师已开启卡秒，需三倍加价！" : "卡秒已关闭，恢复正常加价",
                payload = new { auctionItemId = input.AuctionItemId, isKasec = input.IsKasec }
            };

            _logger.LogInformation(
                "构造卡秒消息: Type={MessageType}, Channel={Channel}, Message={Message}, Payload={Payload}",
                msg.type, msg.chan, msg.msg, JsonConvert.SerializeObject(msg.payload));

            // 修复：使用拍卖消息发送方法，作为系统消息发送以提高速度
            _logger.LogInformation("开始发送卡秒消息到MessageSendingService");

            var sendResult =
                await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction", msg,
                    true);

            _logger.LogInformation(
                "卡秒消息发送结果: Success={Success}, MessageId={MessageId}, SequenceNumber={SequenceNumber}, ErrorMessage={ErrorMessage}",
                sendResult.Success, sendResult.MessageId, sendResult.SequenceNumber, sendResult.Message);

            if (!sendResult.Success)
            {
                _logger.LogError("卡秒消息发送失败: {ErrorMessage}", sendResult.Message);
            }

            // 清除详情缓存，因为卡秒状态改变了
            _logger.LogInformation("清除相关缓存");
            await _cacheService.ClearAuctionDetailCacheAsync(input.AuctionItemId);
            await _cacheService.ClearCurrentAuctionCacheAsync();

            // 发布卡秒状态变更事件
            _logger.LogInformation("发布卡秒状态变更事件");
            await _mediator.Publish(new KasecStatusChangedEvent(input.AuctionItemId, input.IsKasec));

            _logger.LogInformation("=== 卡秒状态设置完成 === AuctionItemId={AuctionItemId}, IsKasec={IsKasec}",
                input.AuctionItemId, input.IsKasec);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卡秒状态设置失败: AuctionItemId={AuctionItemId}, IsKasec={IsKasec}, UserId={UserId}",
                input.AuctionItemId, input.IsKasec, AbpSession.UserId.Value);
            throw;
        }
    }

    // 2. 获取卡秒状态（所有用户）
    [HttpGet]
    public Task<bool> GetKasecStatus(long auctionItemId)
    {
        var kasecKey = $"Kasec:{auctionItemId}";

        bool result = _memoryCache.TryGetValue(kasecKey, out string kasecVal) && kasecVal == "true";

        return Task.FromResult(result);
    }

    /// <summary>
    /// 获取当前正在拍卖的商品（单个）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<AuctionItemDto> GetCurrentAuctionItem()
    {
        // 使用新的缓存服务
        return await _cacheService.GetCurrentAuctionItemAsync();
    }

    // 重写CRUD方法，在数据变更时清理缓存
    public override async Task<AuctionItemDto> CreateAsync(AuctionItemCreateOrUpdateDto input)
    {
        // 调试：检查description字段
        _logger.LogInformation("创建拍品时description字段: {Description}", input.Description);

        var result = await base.CreateAsync(input);

        // 调试：检查创建后的description字段
        _logger.LogInformation("创建拍品后description字段: {Description}", result.Description);

        // 同步清除所有相关缓存，确保数据一致性
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearCurrentAuctionCacheAsync();

        // 预热新创建的拍卖品详情缓存
        await _cacheService.SetAuctionDetailCacheAsync(result);

        // 发布拍卖品创建事件
        await _mediator.Publish(new AuctionItemCreatedEvent(result));

        return result;
    }

    public override async Task<AuctionItemDto> UpdateAsync(AuctionItemCreateOrUpdateDto input)
    {
        // 调试：检查更新时description字段
        _logger.LogInformation("更新拍品时description字段: {Description}", input.Description);

        var result = await base.UpdateAsync(input);

        // 调试：检查更新后的description字段
        _logger.LogInformation("更新拍品后description字段: {Description}", result.Description);

        // 同步清除所有相关缓存，确保数据一致性
        // 先清除所有列表缓存，避免并发请求写入脏数据
        await _cacheService.ClearAuctionListCacheAsync();

        // 清除当前拍卖缓存（如果当前拍卖是这个商品）
        await _cacheService.ClearCurrentAuctionCacheAsync();

        // 清除详情缓存并重新预热
        await _cacheService.ClearAuctionDetailCacheAsync(result.Id);
        await _cacheService.SetAuctionDetailCacheAsync(result);

        // 发布拍卖品更新事件（用于其他异步处理）
        await _mediator.Publish(new AuctionItemUpdatedEvent(result));

        return result;
    }

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        // 先获取商品信息（用于事件发布）
        var auctionItem = await Repository.FirstOrDefaultAsync(input.Id);
        var status = auctionItem?.Status;

        await base.DeleteAsync(input);

        // 同步清除所有相关缓存，确保数据一致性
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearCurrentAuctionCacheAsync();
        await _cacheService.ClearAuctionDetailCacheAsync(input.Id);

        // 发布拍卖品删除事件
        await _mediator.Publish(new AuctionItemDeletedEvent(input.Id, status));
    }
}