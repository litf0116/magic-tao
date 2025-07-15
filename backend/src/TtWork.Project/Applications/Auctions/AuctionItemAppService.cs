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

/// <summary>
/// 拍卖结束类型
/// </summary>
public enum AuctionEndType
{
    /// <summary>
    /// 手动结束
    /// </summary>
    Manual,

    /// <summary>
    /// 定时结束
    /// </summary>
    Scheduled
}

/// <summary>
/// 拍卖结束处理结果
/// </summary>
public class AuctionEndResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 拍卖商品DTO
    /// </summary>
    public AuctionItemDto AuctionItemDto { get; set; }

    /// <summary>
    /// 提示消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 是否有出价（用于区分回退到待拍卖状态的情况）
    /// </summary>
    public bool HasBids { get; set; }
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
            await _mediator.Publish(new BidPlacedEvent(input.AuctionItemId, bidUserId, input.BidPrice, input.BidUserName));

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

        // 使用统一的拍卖结束处理方法
        using (var uow = _unitOfWorkManager.Begin())
        {
            var result = await ProcessAuctionEndAsync(auctionItem.Id, AuctionEndType.Scheduled, null, ip);

            if (result.Success)
            {
                // 定时结束特有的逻辑：清理聊天删除记录
                if (result.HasBids && result.AuctionItemDto?.DealUserId.HasValue == true)
                {
                    await _chatListDeleteRepository.GetAll().Where(x =>
                            (x.UserId == cacheUser.Id && x.ToUserId == result.AuctionItemDto.DealUserId.Value) ||
                            (x.UserId == result.AuctionItemDto.DealUserId.Value && x.ToUserId == cacheUser.Id))
                        .ExecuteDeleteAsync();
                }
            }

            await uow.CompleteAsync();
        }
    }

    /// <summary>
    /// 通用的拍卖结束处理方法
    /// 统一处理手动结束和定时结束的业务逻辑
    /// </summary>
    /// <param name="auctionItemId">拍卖商品ID</param>
    /// <param name="endType">结束类型</param>
    /// <param name="operatorUserId">操作者用户ID（手动结束时使用）</param>
    /// <param name="ip">IP地址</param>
    /// <returns>拍卖结束处理结果</returns>
    private async Task<AuctionEndResult> ProcessAuctionEndAsync(long auctionItemId, AuctionEndType endType,
        long? operatorUserId = null, string ip = null)
    {
        var result = new AuctionEndResult();

        try
        {
            // === 前置处理阶段 ===
            
            // 1. 查询商品信息
            var find = await Repository.FirstOrDefaultAsync(x => x.Id == auctionItemId);
            if (find == null)
            {
                result.Success = false;
                result.Message = "找不到商品";
                return result;
            }

            // 2. 检查商品状态
            if (find.Status.HasFlag(AuctionStatusEnum.已成交))
            {
                result.Success = false;
                result.Message = "已成交商品不能再次处理";
                result.AuctionItemDto = ObjectMapper.Map<AuctionItemDto>(find);
                result.AuctionItemDto.ToUserMsg = "已成交商品不能再次拍卖";
                return result;
            }

            // === 核心业务处理阶段 ===
            
            AuctionItemDto auctionResult;
            bool hasBids = find.CurrentPrice != null;
            
            if (!hasBids)
            {
                // 3a. 无出价情况处理
                find.Back();
                await CurrentUnitOfWork.SaveChangesAsync();

                result.Success = true;
                result.HasBids = false;
                result.Message = "无出价，商品已回退到待拍卖状态";
                auctionResult = ObjectMapper.Map<AuctionItemDto>(find);
            }
            else
            {
                // 3b. 有出价情况处理
                
                // 验证出价记录一致性
                var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                    .Where(x => x.AuctionItemId == auctionItemId)
                    .OrderByDescending(x => x.BidPrice)
                    .FirstOrDefaultAsync();

                if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
                {
                    _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
                }

                // 计算用户群聊等级
                if (maxPrice != null)
                {
                    await AddUserGroupChatLevelIncrement(find.CurrentPriceUserId.Value, maxPrice.BidPrice);
                }

                // 设置商品为已成交状态
                find.SetDeal();
                await CurrentUnitOfWork.SaveChangesAsync();

                // 构建返回结果
                auctionResult = ObjectMapper.Map<AuctionItemDto>(find);
                auctionResult.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                                          ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";

                var bidUser = await _userCache.GetAsync(auctionResult.DealUserId!.Value);
                auctionResult.DealUserAvatar = bidUser.HeadImgUrl;

                result.Success = true;
                result.HasBids = true;
                result.Message = "拍卖结束处理成功";
            }

            result.AuctionItemDto = auctionResult;

            // === 统一后置处理阶段 ===
            
            // 4. 获取并处理卡秒状态
            var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
            bool wasInKasecMode = kasecVal.HasValue && kasecVal == "true";

            // 无论什么方式结束拍卖，都将卡秒状态设置为false
            await _redisClient.Database.StringSetAsync($"Auction:Kasec:{auctionItemId}", "false");

            // 5. 获取操作用户信息（根据不同情况获取不同用户）
            UserInfoEntity operatorInfo;
            if (operatorUserId.HasValue)
            {
                // 手动结束：使用当前操作用户信息
                var currentUser = await _userCache.GetAsync(operatorUserId.Value);
                operatorInfo = new UserInfoEntity
                {
                    Id = (int)operatorUserId.Value,
                    Name = currentUser.Name,
                    HeadImgUrl = currentUser.HeadImgUrl,
                    LastModifierUserId = (int)operatorUserId.Value,
                };
            }
            else
            {
                // 定时结束：获取拍卖师信息
                operatorInfo = await _sqlSugarClient
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
            }

            // 6. 发送卡秒关闭消息（如果当前处于卡秒状态）
            if (wasInKasecMode)
            {
                // 获取发送者角色信息
                var senderId = operatorUserId ?? operatorInfo.Id;

                // 如果是通过定时任务结束的，获取消息所需的用户信息
                var senderName = operatorInfo.Name;
                var senderAvatar = operatorInfo.HeadImgUrl;

                var kasecMsg = new ChatMessage
                {
                    type = ChatMessageType.KasecStatusChanged,
                    chan = "-1_auction",
                    msg = endType == AuctionEndType.Manual
                        ? "卡秒已关闭，恢复正常加价"
                        : "拍卖已结束，卡秒自动关闭",
                    payload = new { auctionItemId = auctionItemId, isKasec = false }
                };

                await _messageSendingService.SendAuctionMessageAsync(senderId, null, "-1_auction", kasecMsg, true);

                // 记录卡秒关闭日志
                _logger.LogInformation(
                    "卡秒状态已关闭，拍卖ID: {AuctionItemId}, 结束类型: {EndType}, 操作者: {OperatorName}",
                    auctionItemId, endType, senderName);
            }

            // 7. 发送拍卖结束消息（统一发送）
            // 构造 payload，保留原有数字 Status，同时追加小写字符串 status 方便前端直接使用
            var payloadObj = JObject.FromObject(auctionResult);
            payloadObj["status"] = auctionResult.Status.ToString();

            // 为群组消息设置有意义的内容，确保群组列表能显示最后消息
            string groupMessage;
            if (hasBids)
            {
                // 拍卖成功：显示成交信息
                groupMessage = $"恭喜 {auctionResult.DealUserName} 以 ￥{auctionResult.FinalPrice} 拍得 {auctionResult.Name}";
            }
            else
            {
                // 流拍：显示流拍信息
                groupMessage = "拍卖结束，无人出价，商品已回退";
            }

            var auctionEndMessage = new ChatMessage
            {
                type = ChatMessageType.AuctionEnd,
                chan = "-1_auction",
                msg = groupMessage,
                payload = payloadObj
            };

            await _messageSendingService.SendAuctionMessageAsync(operatorInfo.Id, null, "-1_auction", auctionEndMessage, true);

            // 8. 发送成交用户私信（仅在有出价情况下发送）
            if (hasBids && auctionResult.DealUserId.HasValue)
            {
                var dealMessage = new ChatMessage
                {
                    type = ChatMessageType.AuctionDeal,
                    msg = auctionResult.ToUserMsg,
                    payload = auctionResult
                };

                await _messageSendingService.SendAuctionMessageAsync(operatorInfo.Id, auctionResult.DealUserId.Value, null, dealMessage, true);
            }

            // 9. 清除缓存
            await _cacheService.ClearAuctionListCacheAsync();
            await _cacheService.ClearAuctionDetailCacheAsync(auctionItemId);
            await _cacheService.ClearCurrentAuctionCacheAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拍卖结束处理失败，商品ID: {AuctionItemId}, 结束类型: {EndType}", auctionItemId, endType);
            result.Success = false;
            result.Message = $"拍卖结束处理失败: {ex.Message}";
            return result;
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
            //查询用户群聊等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .FirstAsync(f => f.UserId == userId);
            if (info == null)
            {
                info = new UserGroupLevelEntity() { CumulativeAmount = 0 };
            }

            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
                .Where(w => w.AmountRequired <= (incrementAmount + info.CumulativeAmount)) // 找到小于等于当前累计金额的等级配置
                .OrderByDescending(o => o.AmountRequired) // 按金额要求降序排序，找到最接近的等级
                .FirstAsync();
            if (groupChatLevelSettings == null)
            {
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }

            //存在用户群聊等级信息就修改
            if (info != null && info.Id != 0)
            {
                info.CumulativeAmount += incrementAmount;
                info.GroupChatId = groupChatLevelSettings.Id;
                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            else
            {
                await _sqlSugarClient.Insertable(new UserGroupLevelEntity
                {
                    UserId = userId,
                    CumulativeAmount = incrementAmount,
                    GroupChatId = groupChatLevelSettings.Id,
                }).ExecuteCommandAsync();
            }
        }
        catch (Exception ex)
        {
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
    [HttpGet]
    public async Task<AuctionItemDto> EndAuction(EntityDto<long> input)
    {
        try
        {
            // 使用统一的拍卖结束处理方法
            var result = await ProcessAuctionEndAsync(input.Id, AuctionEndType.Manual, AbpSession.UserId.Value, GetIp);

            if (!result.Success)
            {
                if (result.AuctionItemDto != null)
                {
                    return result.AuctionItemDto;
                }

                throw new UserFriendlyException(1, result.Message);
            }

            return result.AuctionItemDto;
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
            
            _logger.LogInformation("构造卡秒消息: Type={MessageType}, Channel={Channel}, Message={Message}, Payload={Payload}", 
                msg.type, msg.chan, msg.msg, JsonConvert.SerializeObject(msg.payload));
            
            // 修复：使用拍卖消息发送方法，作为系统消息发送以提高速度
            _logger.LogInformation("开始发送卡秒消息到MessageSendingService");
            
            var sendResult = await _messageSendingService.SendAuctionMessageAsync(AbpSession.UserId.Value, null, "-1_auction", msg, true);
            
            _logger.LogInformation("卡秒消息发送结果: Success={Success}, MessageId={MessageId}, SequenceNumber={SequenceNumber}, ErrorMessage={ErrorMessage}", 
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

