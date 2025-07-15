using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
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
using Newtonsoft.Json.Linq;

namespace TtWork.Project.Applications.Auctions;

public class SubStartNotifyRequest
{
    public long AuctionItemId { get; set; }
    public string openid { get; set; }
}

public class AuctionItemAppService : AbpAsyncCrudAppService<AuctionItem, AuctionItemDto, long, AppResultRequestDto,
    AuctionItemCreateOrUpdateDto, AuctionItemCreateOrUpdateDto>
{
    private readonly IRedisClient _redisClient;
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

    public AuctionItemAppService(
        IRedisClient redisClient,
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
        IBidEligibilityService bidEligibilityService) : base(repository, iocManager)
    {
        _sqlSugarClient = sqlSugarClient;
        _redisClient = redisClient;
        _userCache = userCache;
        _mediator = mediator;
        _repository = repository;
        _banedUserRepository = banedUserRepository;
        _bidHistoryRepository = bidHistoryRepository;
        _notifyRepository = notifyRepository;
        _logger = logger;
        _auctionStartNotifyRepository = auctionStartNotifyRepository;
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
        // base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
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
        var openIds = await _notifyRepository.GetAll().AsNoTracking()
            .Where(x => x.AuctionItemId == id).Select(x => x.openid).ToListAsync();
        var title = name.Length > 16 ? name[..16] : name;
        await _mediator.Publish(new Events.Commands.MessageSendCommand(Events.Commands.MessageType.WechatTemplate,
            new SendWechatTemplateDetail(
                "uniapp",
                openIds.ToArray(),
                "ZuYTYzw2cM0LVhF5ybH5iATMaDl6lZ82OC6cczsglEA",
                new
                {
                    thing2 = new { value = title }, //活动详情
                    thing1 = new { value = isAuction ? "开始拍卖通知" : "出价通知" }, //活动名称
                }, $"pages/index/index"
            )));
    }


    [HttpPost]
    [AbpAuthorize]
    public async Task SubStartNotify(SubStartNotifyRequest input)
    {
        if (await _auctionStartNotifyRepository.GetAll()
                .AsNoTracking()
                .AnyAsync(x => x.AuctionItemId == input.AuctionItemId && x.openid == input.openid))
        {
        }
        else
        {
            await _auctionStartNotifyRepository.InsertAsync(new AuctionStartNotify()
            {
                AuctionItemId = input.AuctionItemId,
                openid = input.openid
            });
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

        //锁,只让一个人出价
        var lockKey = $"Lock:AuctionItem:{input.AuctionItemId}";
        const int lockSeconds = 10;
        try
        {
            var lockTaken =
                await _redisClient.Database.LockTakeAsync(lockKey, input.AuctionItemId,
                    TimeSpan.FromSeconds(lockSeconds));
            if (!lockTaken)
            {
                throw new UserFriendlyException(1, "后台正在处理上一人出价,请稍后再试");
            }

            //查询商品信息
            var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.AuctionItemId);
            if (find == null)
            {
                throw new UserFriendlyException(1, "找不到商品");
            }

            // 由于 BidEligibilityService 已经检查了商品状态，这里可以简化
            if (find.Status != AuctionStatusEnum.拍卖中)
            {
                throw new UserFriendlyException(1, "商品不在拍卖中");
            }

            // 由于 BidEligibilityService 已经计算并检查了最低出价，这里可以简化
            // if (find.CurrentPrice >= input.BidPrice) throw new UserFriendlyException(1, "出价必须大于当前价格");

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
            await _redisClient.Database.LockReleaseAsync(lockKey, input.AuctionItemId);
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
                // 查询最新的商品信息
                var find = await Repository.FirstOrDefaultAsync(x => x.Id == auctionItem.Id);
                if (find == null)
                {
                    _logger.LogError("定时任务回调：找不到商品，ID: {AuctionItemId}", auctionItem.Id);
                    return;
                }

                // 检查商品状态
                if (find.Status.HasFlag(AuctionStatusEnum.已成交))
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

                // 获取并处理卡秒状态
                var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItem.Id}");
                bool wasInKasecMode = kasecVal.HasValue && kasecVal == "true";

                // 定时结束拍卖时将卡秒状态设置为false
                await _redisClient.Database.StringSetAsync($"Auction:Kasec:{auctionItem.Id}", "false");

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
                        .Where(x => x.AuctionItemId == auctionItem.Id)
                        .OrderByDescending(x => x.BidPrice)
                        .FirstOrDefaultAsync();

                    if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
                    {
                        _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
                    }

                    // 计算用户群聊等级
                    if (maxPrice != null && find.CurrentPriceUserId.HasValue)
                    {
                        try
                        {
                            // 安全转换int到decimal，避免数据库范围溢出
                            decimal bidPriceDecimal = Convert.ToDecimal(maxPrice.BidPrice);
                            _logger.LogInformation("用户群聊等级计算: UserId={UserId}, BidPrice={BidPrice}",
                                find.CurrentPriceUserId.Value, bidPriceDecimal);

                            await AddUserGroupChatLevelIncrement(find.CurrentPriceUserId.Value, bidPriceDecimal);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "用户群聊等级计算失败: UserId={UserId}, BidPrice={BidPrice}",
                                find.CurrentPriceUserId.Value, maxPrice.BidPrice);
                            // 不抛出异常，避免影响主流程
                        }
                    }
                    else if (maxPrice != null && !find.CurrentPriceUserId.HasValue)
                    {
                        _logger.LogWarning(
                            "数据不一致：存在出价记录但CurrentPriceUserId为空, AuctionItemId={AuctionItemId}, MaxBidPrice={MaxBidPrice}",
                            auctionItem.Id, maxPrice.BidPrice);
                    }

                    // 设置商品为已成交状态
                    find.SetDeal();
                    await CurrentUnitOfWork.SaveChangesAsync();

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
                    var flowPayload = JObject.FromObject(result);
                    flowPayload["status"] = result.Status.ToString();

                    var flowMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionEnd,
                        chan = "-1_auction",
                        msg = "拍卖结束，无人出价，商品已回退",
                        payload = flowPayload
                    };

                    await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id, null, "-1_auction",
                        flowMessage, true);
                }
                else
                {
                    // 发送拍卖成功消息
                    var successPayload = JObject.FromObject(result);
                    successPayload["status"] = result.Status.ToString();

                    var successMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionEnd,
                        chan = "-1_auction",
                        msg = $"恭喜 {result.DealUserName} 以 ￥{result.FinalPrice} 拍得 {result.Name}",
                        payload = successPayload
                    };

                    await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id, null, "-1_auction",
                        successMessage, true);

                    // 发送成交用户私信
                    var dealMessage = new ChatMessage
                    {
                        type = ChatMessageType.AuctionDeal,
                        msg = result.ToUserMsg,
                        payload = result
                    };

                    if (result.DealUserId.HasValue)
                    {
                        await _messageSendingService.SendAuctionMessageAsync(auctionManagerInfo.Id,
                            result.DealUserId.Value, null, dealMessage, true);
                    }
                    else
                    {
                        _logger.LogWarning("成交用户ID为空，无法发送私信, AuctionItemId={AuctionItemId}", auctionItem.Id);
                    }
                }

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时任务回调处理失败，商品ID: {AuctionItemId}", auctionItem.Id);
            }
        }
    }

    /// <summary>
    /// 累加用户群聊等级金额（用于拍卖成交时）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="incrementAmount">增量金额（本次出价金额）</param>
    /// <returns></returns>
    private async Task AddUserGroupChatLevelIncrement(long userId, decimal incrementAmount)
    {
        try
        {
            _logger.LogInformation("开始累加用户群聊等级金额: UserId={UserId}, IncrementAmount={IncrementAmount}",
                userId, incrementAmount);

            // 检查增量金额范围
            if (incrementAmount < 0)
            {
                _logger.LogWarning("增量金额为负数，跳过处理: UserId={UserId}, IncrementAmount={IncrementAmount}",
                    userId, incrementAmount);
                return;
            }

            if (incrementAmount > 999999999) // 10亿限制
            {
                _logger.LogWarning("增量金额过大，跳过处理: UserId={UserId}, IncrementAmount={IncrementAmount}",
                    userId, incrementAmount);
                return;
            }

            //查询用户群聊等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .FirstAsync(f => f.UserId == userId);
            if (info == null)
            {
                info = new UserGroupLevelEntity() { CumulativeAmount = 0 };
                _logger.LogInformation("用户群聊等级信息不存在，创建新记录: UserId={UserId}", userId);
            }
            else
            {
                _logger.LogInformation(
                    "查询到用户群聊等级信息: UserId={UserId}, CurrentAmount={CurrentAmount}, GroupChatId={GroupChatId}",
                    userId, info.CumulativeAmount, info.GroupChatId);
            }

            // 计算新的累计金额并检查范围
            decimal newCumulativeAmount = info.CumulativeAmount + incrementAmount;
            if (newCumulativeAmount > 999999999) // 10亿限制
            {
                _logger.LogWarning(
                    "累计金额过大，限制为最大值: UserId={UserId}, OldAmount={OldAmount}, IncrementAmount={IncrementAmount}, NewAmount={NewAmount}",
                    userId, info.CumulativeAmount, incrementAmount, newCumulativeAmount);
                newCumulativeAmount = 999999999;
            }

            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
                .Where(w => w.AmountRequired <= newCumulativeAmount) // 找到小于等于当前累计金额的等级配置
                .OrderByDescending(o => o.AmountRequired) // 按金额要求降序排序，找到最接近的等级
                .FirstAsync();
            if (groupChatLevelSettings == null)
            {
                _logger.LogWarning("没有匹配的群聊等级信息: UserId={UserId}, CumulativeAmount={CumulativeAmount}",
                    userId, newCumulativeAmount);
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }

            _logger.LogInformation(
                "匹配到群聊等级信息: UserId={UserId}, LevelId={LevelId}, LevelName={LevelName}, AmountRequired={AmountRequired}",
                userId, groupChatLevelSettings.Id, groupChatLevelSettings.Name, groupChatLevelSettings.AmountRequired);

            //存在用户群聊等级信息就修改
            if (info != null && info.Id != 0)
            {
                info.CumulativeAmount = newCumulativeAmount;
                info.GroupChatId = groupChatLevelSettings.Id;

                _logger.LogInformation(
                    "更新用户群聊等级: UserId={UserId}, NewAmount={NewAmount}, NewGroupChatId={NewGroupChatId}",
                    userId, newCumulativeAmount, groupChatLevelSettings.Id);

                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            else
            {
                _logger.LogInformation(
                    "插入新用户群聊等级: UserId={UserId}, CumulativeAmount={CumulativeAmount}, GroupChatId={GroupChatId}",
                    userId, newCumulativeAmount, groupChatLevelSettings.Id);

                await _sqlSugarClient.Insertable(new UserGroupLevelEntity
                {
                    UserId = userId,
                    CumulativeAmount = newCumulativeAmount,
                    GroupChatId = groupChatLevelSettings.Id,
                }).ExecuteCommandAsync();
            }

            _logger.LogInformation("用户群聊等级金额累加完成: UserId={UserId}, FinalAmount={FinalAmount}",
                userId, newCumulativeAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "累加用户群聊等级金额失败: UserId={UserId}, IncrementAmount={IncrementAmount}, Error={Error}",
                userId, incrementAmount, ex.Message);
            throw new UserFriendlyException($"累加用户群聊等级金额失败，错误信息：" + ex.Message);
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
    public async Task<AuctionItemDto> EndAuction(EntityDto<long> input)
    {
        try
        {
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
            if (find.Status.HasFlag(AuctionStatusEnum.已成交))
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

            // 获取并处理卡秒状态
            var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{input.Id}");
            bool wasInKasecMode = kasecVal.HasValue && kasecVal == "true";

            // 手动结束拍卖时将卡秒状态设置为false
            await _redisClient.Database.StringSetAsync($"Auction:Kasec:{input.Id}", "false");

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
                    .Where(x => x.AuctionItemId == input.Id)
                    .OrderByDescending(x => x.BidPrice)
                    .FirstOrDefaultAsync();

                if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
                {
                    _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
                }

                // 计算用户群聊等级
                if (maxPrice != null && find.CurrentPriceUserId.HasValue)
                {
                    try
                    {
                        // 安全转换int到decimal，避免数据库范围溢出
                        decimal bidPriceDecimal = Convert.ToDecimal(maxPrice.BidPrice);
                        _logger.LogInformation("用户群聊等级计算: UserId={UserId}, BidPrice={BidPrice}",
                            find.CurrentPriceUserId.Value, bidPriceDecimal);

                        await AddUserGroupChatLevelIncrement(find.CurrentPriceUserId.Value, bidPriceDecimal);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "用户群聊等级计算失败: UserId={UserId}, BidPrice={BidPrice}",
                            find.CurrentPriceUserId.Value, maxPrice.BidPrice);
                        // 不抛出异常，避免影响主流程
                    }
                }
                else if (maxPrice != null && !find.CurrentPriceUserId.HasValue)
                {
                    _logger.LogWarning(
                        "数据不一致：存在出价记录但CurrentPriceUserId为空, AuctionItemId={AuctionItemId}, MaxBidPrice={MaxBidPrice}",
                        input.Id, maxPrice.BidPrice);
                }

                // 设置商品为已成交状态
                find.SetDeal();
                await CurrentUnitOfWork.SaveChangesAsync();

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
                var flowPayload = JObject.FromObject(result);
                flowPayload["status"] = result.Status.ToString();

                var flowMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionEnd,
                    chan = "-1_auction",
                    msg = "拍卖结束，无人出价，商品已回退",
                    payload = flowPayload
                };

                await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction",
                    flowMessage, true);
            }
            else
            {
                // 发送拍卖成功消息
                var successPayload = JObject.FromObject(result);
                successPayload["status"] = result.Status.ToString();

                var successMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionEnd,
                    chan = "-1_auction",
                    msg = $"恭喜 {result.DealUserName} 以 ￥{result.FinalPrice} 拍得 {result.Name}",
                    payload = successPayload
                };

                await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction",
                    successMessage, true);

                // 发送成交用户私信
                var dealMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionDeal,
                    msg = result.ToUserMsg,
                    payload = result
                };

                if (result.DealUserId.HasValue)
                {
                    await _messageSendingService.SendPrivateMessageAsync(AbpSession.UserId.Value,
                        result.DealUserId.Value, dealMessage, false, null);
                }
                else
                {
                    _logger.LogWarning("成交用户ID为空，无法发送私信, AuctionItemId={AuctionItemId}", input.Id);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "手动结束竞拍失败: {Message}", ex.Message);
            throw new UserFriendlyException(1, ex.Message);
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

        if (find.Status.HasFlag(AuctionStatusEnum.已成交))
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
            Logger.Error("发送拍卖开始通知失败 {@e}", e);
        }

        return ObjectMapper.Map<AuctionItemDto>(find);
    }

    /// <summary>
    /// 获取待拍卖商品跟已拍卖商品
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
                .WhereIf(input.Status.HasValue, x => x.Status.HasFlag((AuctionStatusEnum)input.Status))
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Name.Contains(input.Keyword))
                .WhereIf(input.UserId.HasValue, x => x.DealUserId == input.UserId.Value) //成功拍得
            ;
    }


    [HttpGet]
    [AbpAuthorize]
    public async Task<object> DateAnlayse(AppResultRequestDto input)
    {
        var query = await Repository.GetAll()
            .WhereIf(input.Status.HasValue, x => x.Status.HasFlag(AuctionStatusEnum.已成交))
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
            .WhereIf(input.Status.HasValue, x => x.Status.HasFlag(AuctionStatusEnum.已成交))
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
            // 将布尔值转换为小写字符串存储到Redis
            var kasecKey = $"Auction:Kasec:{input.AuctionItemId}";
            var kasecValue = input.IsKasec.ToString().ToLower();

            _logger.LogInformation("设置Redis卡秒状态: Key={KasecKey}, Value={KasecValue}", kasecKey, kasecValue);

            await _redisClient.Database.StringSetAsync(kasecKey, kasecValue);

            _logger.LogInformation("Redis卡秒状态设置成功");

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
    public async Task<bool> GetKasecStatus(long auctionItemId)
    {
        var val = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
        return val.HasValue && val == "true";
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
        var result = await base.CreateAsync(input);

        // 清除缓存和发布事件
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearAuctionDetailCacheAsync(result.Id);

        // 发布拍卖品创建事件
        await _mediator.Publish(new AuctionItemCreatedEvent(result));

        return result;
    }

    public override async Task<AuctionItemDto> UpdateAsync(AuctionItemCreateOrUpdateDto input)
    {
        var result = await base.UpdateAsync(input);

        // 清除缓存和发布事件
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearAuctionDetailCacheAsync(result.Id);

        // 发布拍卖品更新事件
        await _mediator.Publish(new AuctionItemUpdatedEvent(result));

        return result;
    }

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        await base.DeleteAsync(input);

        // 清除缓存和发布事件
        await _cacheService.ClearAuctionListCacheAsync();
        await _cacheService.ClearAuctionDetailCacheAsync(input.Id);

        // 发布拍卖品删除事件
        await _mediator.Publish(new AuctionItemDeletedEvent(input.Id));
    }
}