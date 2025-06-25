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
using TtWork.Project.Controllers;
using TtWork.Project.Domains;
using TtWork.Project.Events;
using TtWork.Project.Events.Commands;
using TtWork.Project.Jobs;
using TTWork.WeiXinMiddleware.Utils;
using static OfficeOpenXml.ExcelErrorValue;
using Abp.Events.Bus;
using TtWork.Project.EventHandlers;

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
    private readonly WebSocketController _webSocketController;
    private readonly IRepository<AuctionStartNotify, long> _auctionStartNotifyRepository;
    private readonly ILogger<AuctionItemAppService> _logger;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<ChatListDelete> _chatListDeleteRepository;
    private readonly IEventBus _eventBus;

    public AuctionItemAppService(
        IRedisClient redisClient,
        UserCache userCache,
        IMediator mediator,
        IRepository<AuctionItem, long> repository,
        IRepository<BanedUser, long> banedUserRepository,
        IRepository<BidHistory, long> bidHistoryRepository,
        IRepository<AuctionStartNotify, long> notifyRepository,
        WebSocketController webSocketController,
        IocManager iocManager,
        ILogger<AuctionItemAppService> logger,
        IRepository<AuctionStartNotify, long> auctionStartNotifyRepository,
        ISqlSugarClient sqlSugarClient,
        IRepository<Message, Guid> messageRepository,
        IHttpContextAccessor httpContextAccessor,
        IRepository<ChatListDelete> chatListDeleteRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IEventBus eventBus) : base(repository, iocManager)
    {
        _sqlSugarClient = sqlSugarClient;
        _redisClient = redisClient;
        _userCache = userCache;
        _mediator = mediator;
        _repository = repository;
        _banedUserRepository = banedUserRepository;
        _bidHistoryRepository = bidHistoryRepository;
        _notifyRepository = notifyRepository;
        _webSocketController = webSocketController;
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
        // base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
    }


    private async Task<(bool, string, string)> CheckIsChatAdmin()
    {
        try
        {
            var currentUser = await _userCache.GetAsync(AbpSession.UserId!.Value);
            if (currentUser is { RoleNames.Length: > 0 })
            {
                if (currentUser.RoleNames.Contains("AuctionManager"))
                    return (true, "拍卖师", "tag_AuctionManager");
                if (currentUser.RoleNames.Contains("Manager"))
                    return (true, "管理员", "tag_Manager");
                if (currentUser.RoleNames.Contains("AuctionUser"))
                    return (false, "竞拍用户", "tag_AudtionUser");
                if (currentUser.RoleNames.Contains("Admin"))
                    return (true, "系统管理员", "tag_Admin");
            }
        }
        catch (Exception e)
        {
            Logger.Error("获取用户缓存信息失败", e);
        }

        return (false, "", "");
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
        var user = await _userCache.GetAsync(AbpSession.UserId!.Value);
        //获取消息提示
        var msgConfiguration =
            await _sqlSugarClient.Queryable<MsgConfigurationEntity>().Where(w => w.Type == 1).FirstAsync();
        // 查询用户群聊等级（只针对0级用户判断保证金）
        var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
            .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
            .Where((a, b) => a.UserId == user.Id)
            .Select((a, b) => new { a.UserId, b.Level })
            .FirstAsync();
        int userLevel = userGroupLevel?.Level ?? 0;
        if (userLevel == 0 && user.DepositBalance < 50)
        {
            throw new UserFriendlyException(msgConfiguration != null
                ? msgConfiguration.Msg
                : $"当前用户保证金不足50元，请先去充值保证金（需支付51元，包含1元提现手续费）！");
        }

        //拍卖场不修改名字头像不给出价权限
        if (Regex.IsMatch(input.BidUserName, @"^玩家\d{5}"))
        {
            throw new UserFriendlyException("请先修改昵称后再进行出价");
        }

        var isChatAdmin = await CheckIsChatAdmin();
        if (!isChatAdmin.Item1)
        {
            // 非管理判断是否被禁言
            var banedUser = await _banedUserRepository.FirstOrDefaultAsync(a =>
                a.UserId == AbpSession.UserId!.Value && a.Chan == "-1_auction" && a.EndTime > DateTime.Now);
            if (banedUser != null)
            {
                throw new UserFriendlyException($"禁言用户禁止出价,结束时间 {banedUser.EndTime:yyyy-MM-dd HH:mm:ss}");
            }
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

            if (find.Status != AuctionStatusEnum.拍卖中)
            {
                throw new UserFriendlyException(1, "商品不在拍卖中");
            }

            var basePrice = find.CurrentPrice ?? find?.StartingPrice ?? 1;

            #region 计算最低出价

            var minPrice = 0;
            if (find.CurrentPrice.HasValue)
            {
                // 算法：
                // 100以内，1R一加
                // 100~1000，5R一加
                // 1000-2000，10R一加
                // 2000-5000，20R一加
                // 50000-1W，50一加
                // 1W以上，100一加
                if (find.CurrentPrice.Value < 100)
                {
                    minPrice = find.CurrentPrice.Value + 1;
                }
                else if (find.CurrentPrice.Value < 1000)
                {
                    minPrice = find.CurrentPrice.Value + 5;
                }
                else if (find.CurrentPrice.Value < 2000)
                {
                    minPrice = find.CurrentPrice.Value + 10;
                }
                else if (find.CurrentPrice.Value < 5000)
                {
                    minPrice = find.CurrentPrice.Value + 20;
                }
                else if (find.CurrentPrice.Value < 10000)
                {
                    minPrice = find.CurrentPrice.Value + 50;
                }
                else
                {
                    minPrice = find.CurrentPrice.Value + 100;
                }
            }
            else
            {
                minPrice = basePrice;
            }

            #endregion

            // 读取卡秒状态，若为 true，最低加价三倍
            var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{input.AuctionItemId}");
            if (kasecVal.HasValue && kasecVal == "true")
            {
                minPrice = basePrice + ((minPrice - basePrice) * 3);
            }

            if (input.BidPrice < minPrice)
            {
                var priceRules = new[]
                {
                    "100以内，1R一加",
                    "100~1000，5R一加",
                    "1000~2000，10R一加",
                    "2000~5000，20R一加",
                    "5000~1W，50一加",
                    "1W以上，100一加"
                };

                var formattedMessage = "出价必须大于最低加价：\n\n" +
                                       string.Join("\n", priceRules) +
                                       (kasecVal.HasValue && kasecVal == "true" ? "\n\n⚠️ 卡秒期间需三倍加价" : "");

                throw new UserFriendlyException(1, formattedMessage);
            }
            // if (find.CurrentPrice >= input.BidPrice) throw new UserFriendlyException(1, "出价必须大于当前价格");

            var addInfo = ObjectMapper.Map<BidHistory>(input);

            await _bidHistoryRepository.InsertAsync(addInfo);

            find.SetBid(input.BidPrice, AbpSession.UserId!.Value, input.BidUserName);

            await CurrentUnitOfWork.SaveChangesAsync();

            var result = ObjectMapper.Map<AuctionItemDto>(find);
            result.UseCountdownTime = addInfo.CreationTime;
            // chatStore.sendChannelMsg(`${res.currentPrice}`, '-1_auction', ChatMessageType.AuctionBid, res)

            var msg = new ChatMessage()
            {
                type = ChatMessageType.AuctionBid,
                msg = $"{result.CurrentPrice}",
                payload = result,
                from = AbpSession.UserId.Value,
                fromName = user.Name,
                avatar = user.HeadImgUrl,
                chan = "-1_auction",
                to = result.DealUserId
            };
            msg.time = msg.GetNowTime();

            await _webSocketController.SendChannelMsg(new SendChangeMsgInput()
            {
                Chan = "-1_auction",
                From = AbpSession.UserId.Value,
                Message = msg
            });
            var ip = GetIp;

            // 清除缓存，因为出价改变了商品状态
            ClearAuctionListCache();
            ClearAuctionDetailCache(input.AuctionItemId);
            ClearCurrentAuctionCache();

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

            // 3. 处理无出价情况
            if (find.CurrentPrice == null)
            {
                find.Back();
                await CurrentUnitOfWork.SaveChangesAsync();

                result.Success = true;
                result.HasBids = false;
                result.Message = "无出价，商品已回退到待拍卖状态";
                result.AuctionItemDto = ObjectMapper.Map<AuctionItemDto>(find);

                // 清除缓存
                ClearAuctionListCache();
                ClearAuctionDetailCache(auctionItemId);
                ClearCurrentAuctionCache();

                return result;
            }

            // 4. 验证出价记录一致性
            var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                .Where(x => x.AuctionItemId == auctionItemId)
                .OrderByDescending(x => x.BidPrice)
                .FirstOrDefaultAsync();

            if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
            {
                _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
            }

            // 5. 计算用户群聊等级
            if (maxPrice != null)
            {
                await AddUserGroupChatLevel(new UserGroupLevelDto
                {
                    CumulativeAmount = maxPrice.BidPrice,
                    UserId = find.CurrentPriceUserId.Value,
                });
            }

            // 6. 设置商品为已成交状态
            find.SetDeal();
            await CurrentUnitOfWork.SaveChangesAsync();

            // 7. 构建返回结果
            var auctionResult = ObjectMapper.Map<AuctionItemDto>(find);
            auctionResult.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                                      ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";

            var bidUser = await _userCache.GetAsync(auctionResult.DealUserId!.Value);
            auctionResult.DealUserAvatar = bidUser.HeadImgUrl;

            // 8. 获取并处理卡秒状态
            var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
            bool wasInKasecMode = kasecVal.HasValue && kasecVal == "true";

            // 无论什么方式结束拍卖，都将卡秒状态设置为false
            await _redisClient.Database.StringSetAsync($"Auction:Kasec:{auctionItemId}", "false");

            // 9. 获取操作用户信息（根据不同情况获取不同用户）
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

            // 10. 发送卡秒关闭消息（如果当前处于卡秒状态）
            if (wasInKasecMode)
            {
                // 获取发送者角色信息
                var senderId = operatorUserId ?? operatorInfo.Id;
                var (isAdmin, adminTag, tagClass) = operatorUserId.HasValue
                    ? await CheckIsChatAdmin()
                    : (true, "拍卖师", "tag_AuctionManager");

                // 如果是通过定时任务结束的，获取消息所需的用户信息
                var senderName = operatorInfo.Name;
                var senderAvatar = operatorInfo.HeadImgUrl;

                var kasecMsg = new ChatMessage
                {
                    type = ChatMessageType.KasecStatusChanged,
                    chan = "-1_auction",
                    from = senderId,
                    fromName = senderName,
                    avatar = senderAvatar,
                    fromAdmin = isAdmin,
                    fromTag = adminTag,
                    tagClass = tagClass,
                    msg = endType == AuctionEndType.Manual
                        ? "卡秒已关闭，恢复正常加价"
                        : "拍卖已结束，卡秒自动关闭",
                    payload = new { auctionItemId = auctionItemId, isKasec = false },
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await _webSocketController.SendChannelMsg(new SendChangeMsgInput
                {
                    Chan = "-1_auction",
                    From = senderId,
                    Message = kasecMsg
                });

                // 记录卡秒关闭日志
                _logger.LogInformation(
                    "卡秒状态已关闭，拍卖ID: {AuctionItemId}, 结束类型: {EndType}, 操作者: {OperatorName}",
                    auctionItemId, endType, senderName);
            }

            // 11. 发送拍卖结束消息（统一发送）
            var auctionEndMessage = new ChatMessage
            {
                type = ChatMessageType.AuctionEnd,
                chan = "-1_auction",
                from = operatorInfo.Id,
                fromName = operatorInfo.Name,
                avatar = operatorInfo.HeadImgUrl,
                msg = "",
                payload = auctionResult,
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                fromAdmin = true,
                fromTag = "拍卖师",
                tagClass = "tag_AuctionManager"
            };

            var auctionEndInput = new SendChangeMsgInput()
            {
                Chan = "-1_auction",
                From = operatorInfo.Id,
                Message = auctionEndMessage
            };

            auctionEndInput.Message.id = Guid.NewGuid();
            await _webSocketController.SendChannelMsg(auctionEndInput);

            // 12. 发送成交用户私信（统一发送）
            var dealMessage = new ChatMessage
            {
                chan = null,
                type = ChatMessageType.AuctionDeal,
                from = operatorInfo.LastModifierUserId,
                fromName = operatorInfo.Name,
                avatar = operatorInfo.HeadImgUrl,
                msg = auctionResult.ToUserMsg,
                to = auctionResult.DealUserId.Value,
                payload = auctionResult,
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                fromAdmin = true,
                fromTag = "拍卖师",
                tagClass = "tag_AuctionManager"
            };

            var dealMsgInput = new SendMsgInput
            {
                From = operatorInfo.Id,
                To = auctionResult.DealUserId.Value,
                IsReceipt = true,
                Message = dealMessage
            };

            dealMsgInput.Message.id = Guid.NewGuid();
            await _webSocketController.SendMsg(dealMsgInput);

            // 13. 清除缓存
            ClearAuctionListCache();
            ClearAuctionDetailCache(auctionItemId);
            ClearCurrentAuctionCache();

            // 14. 设置返回结果
            result.Success = true;
            result.HasBids = true;
            result.Message = "拍卖结束处理成功";
            result.AuctionItemDto = auctionResult;

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
    /// 添加用户群聊等级
    /// </summary>
    /// <param name="input"></param>
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

    private Task<(bool, string, string)> CheckIsChatAdmin(UserDto currentUser)
    {
        try
        {
            if (currentUser is { RoleNames.Length: > 0 })
            {
                if (currentUser.RoleNames.Contains("AuctionManager"))
                    return Task.FromResult((true, "拍卖师", "tag_AuctionManager"));
                if (currentUser.RoleNames.Contains("Manager"))
                    return Task.FromResult((true, "管理员", "tag_Manager"));
                if (currentUser.RoleNames.Contains("AuctionUser"))
                    return Task.FromResult((false, "竞拍用户", "tag_AudtionUser"));
                if (currentUser.RoleNames.Contains("Admin"))
                    return Task.FromResult((true, "系统管理员", "tag_Admin"));
            }
        }
        catch (Exception e)
        {
            Logger.Error("获取用户缓存信息失败", e);
        }

        return Task.FromResult((false, "", ""));
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
        // 生成缓存键
        string cacheKey = GenerateAuctionDetailCacheKey(id);

        // 尝试从缓存获取数据
        var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            var cachedResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AuctionItemDto>(cachedValue);
            return cachedResult;
        }

        // 缓存未命中，从数据库获取
        var auctionItem = await Repository.GetAll().AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (auctionItem == null)
        {
            throw new UserFriendlyException(1, "找不到商品");
        }

        var result = ObjectMapper.Map<AuctionItemDto>(auctionItem);

        // 如果是拍卖中的商品，获取最新的出价信息
        if (auctionItem.Status == AuctionStatusEnum.拍卖中)
        {
            var latestBid = await _bidHistoryRepository.GetAll().AsNoTracking()
                .Where(w => w.AuctionItemId == id)
                .OrderByDescending(o => o.BidTime)
                .FirstOrDefaultAsync();

            if (latestBid != null)
            {
                result.CurrentPrice = latestBid.BidPrice;
                result.CurrentPriceUserName = latestBid.BidUserName;
                result.CurrentPriceTime = latestBid.BidTime;
                result.UseCountdownTime = latestBid.CreationTime;
            }
        }

        // 获取卡秒状态
        var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{id}");
        result.IsKasec = kasecVal.HasValue && kasecVal == "true";

        // 设置缓存，根据商品状态设置不同的过期时间
        int cacheMinutes = GetDetailCacheExpireMinutes(auctionItem.Status);
        string serializedResult = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        await _redisClient.Database.StringSetAsync(cacheKey, serializedResult, TimeSpan.FromMinutes(cacheMinutes));

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
        ClearAuctionListCache();
        ClearAuctionDetailCache(input.Id);
        ClearCurrentAuctionCache();

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

        // 生成缓存键
        string cacheKey = GenerateAuctionListCacheKey(input);

        // 尝试从缓存获取数据
        var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            var cachedResult =
                Newtonsoft.Json.JsonConvert.DeserializeObject<ListResultDto<AuctionItemDto>>(cachedValue);
            return cachedResult;
        }

        var query = Repository.GetAll().AsNoTracking()
                .WhereIf(!input.Status.HasValue,
                    x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
                .WhereIf(input.Status.HasValue, x => (int)x.Status == input.Status!.Value)
            ;

        if (!input.Status.HasValue)
        {
            query = query.OrderBy(x => x.Order)
                .ThenBy(x => x.Id);
        }
        else if (input.Status == (int)AuctionStatusEnum.已成交)
        {
            query = query.OrderByDescending(x => x.DealTime).Take(input.MaxResultCount);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var items = await query.ToListAsync();

        var result = new ListResultDto<AuctionItemDto>(
            ObjectMapper.Map<List<AuctionItemDto>>(items)
        );

        // 设置缓存，根据状态设置不同的过期时间
        int cacheMinutes = GetCacheExpireMinutes(input.Status);
        string serializedResult = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        await _redisClient.Database.StringSetAsync(cacheKey, serializedResult, TimeSpan.FromMinutes(cacheMinutes));

        return result;
    }

    /// <summary>
    /// 生成拍卖列表缓存键
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private string GenerateAuctionListCacheKey(AppResultRequestDto input)
    {
        string statusKey = input.Status?.ToString() ?? "default";
        return $"auction:list:{statusKey}:{input.MaxResultCount}";
    }

    /// <summary>
    /// 根据状态获取缓存过期时间（分钟）
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    private int GetCacheExpireMinutes(int? status)
    {
        if (!status.HasValue)
        {
            // 待拍卖和拍卖中的商品，缓存2分钟（变化较频繁）
            return 2;
        }
        else if (status == (int)AuctionStatusEnum.已成交)
        {
            // 已成交商品，缓存10分钟（相对稳定）
            return 10;
        }
        else
        {
            // 其他状态，缓存5分钟
            return 5;
        }
    }

    /// <summary>
    /// 清除拍卖列表相关缓存
    /// </summary>
    /// <param name="auctionItemId">可选，特定商品ID</param>
    /// <returns></returns>
    private void ClearAuctionListCache(long? auctionItemId = null)
    {
        try
        {
            // 清除所有相关的缓存键
            var patterns = new[]
            {
                "auction:list:default:*",
                $"auction:list:{(int)AuctionStatusEnum.上架}:*",
                $"auction:list:{(int)AuctionStatusEnum.拍卖中}:*",
                $"auction:list:{(int)AuctionStatusEnum.已成交}:*"
            };

            foreach (var pattern in patterns)
            {
                _redisClient.DeleteKeysWithPartten(pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除拍卖列表缓存失败");
        }
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
        var query = Repository.GetAll().AsNoTracking()
                .WhereIf(!input.Status.HasValue,
                    x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
                .WhereIf(input.Status.HasValue, x => (int)x.Status == input.Status!.Value)
            ;

        if (!input.Status.HasValue)
        {
            query = query.OrderBy(x => x.Order)
                .ThenBy(x => x.Id);
        }

        //查询拍卖中的物品
        var items = await query.ToListAsync();
        var result = new ListResultDto<AuctionItemDto>(ObjectMapper.Map<List<AuctionItemDto>>(items));
        //获取所以商品编号
        var idList = items.Select(x => x.Id).ToList();
        //查询物品出价信息
        var bidList = _bidHistoryRepository.GetAll().AsNoTracking().Where(w => idList.Contains(w.AuctionItemId))
            .ToList();
        foreach (var item in result.Items)
        {
            //查询最新的出价信息
            var info = bidList.Where(w => w.AuctionItemId == item.Id).OrderByDescending(o => o.BidTime)
                .FirstOrDefault();
            if (info != null)
            {
                item.CurrentPrice = info.BidPrice;
                item.CurrentPriceUserName = info.BidUserName;
                item.CurrentPriceTime = info.BidTime;
                item.UseCountdownTime = info.CreationTime;
            }
        }

        return result;
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
        // 将布尔值转换为小写字符串存储到Redis
        await _redisClient.Database.StringSetAsync($"Auction:Kasec:{input.AuctionItemId}",
            input.IsKasec.ToString().ToLower());

        // 获取当前用户信息（拍卖师）
        var currentUser = await _userCache.GetAsync(AbpSession.UserId.Value);
        var (isAdmin, adminTag, tagClass) = await CheckIsChatAdmin();

        // 广播卡秒状态变更消息
        var msg = new ChatMessage
        {
            type = ChatMessageType.KasecStatusChanged,
            chan = "-1_auction",
            from = AbpSession.UserId.Value,
            fromName = currentUser.Name,
            avatar = currentUser.HeadImgUrl,
            fromAdmin = isAdmin,
            fromTag = adminTag,
            tagClass = tagClass,
            msg = input.IsKasec ? "拍卖师已开启卡秒，需三倍加价！" : "卡秒已关闭，恢复正常加价",
            payload = new { auctionItemId = input.AuctionItemId, isKasec = input.IsKasec },
            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        await _webSocketController.SendChannelMsg(new SendChangeMsgInput
        {
            Chan = "-1_auction",
            From = AbpSession.UserId.Value,
            Message = msg
        });

        // 清除详情缓存，因为卡秒状态改变了
        ClearAuctionDetailCache(input.AuctionItemId);
        ClearCurrentAuctionCache();

        return true;
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
        // 生成当前拍卖商品缓存键
        string cacheKey = "auction:current";

        // 尝试从缓存获取数据
        var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            if (cachedValue == "null")
            {
                return null;
            }

            var cachedResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AuctionItemDto>(cachedValue);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        // 缓存未命中，从数据库获取
        var auctionItem = await Repository.GetAll().AsNoTracking()
            .Where(x => x.Status == AuctionStatusEnum.拍卖中)
            .FirstOrDefaultAsync();

        if (auctionItem == null)
        {
            // 如果没有拍卖中的商品，缓存null结果30秒
            await _redisClient.Database.StringSetAsync(cacheKey, "null", TimeSpan.FromSeconds(30));
            return null;
        }

        var result = ObjectMapper.Map<AuctionItemDto>(auctionItem);

        // 获取最新的出价信息
        var latestBid = await _bidHistoryRepository.GetAll().AsNoTracking()
            .Where(w => w.AuctionItemId == auctionItem.Id)
            .OrderByDescending(o => o.BidTime)
            .FirstOrDefaultAsync();

        if (latestBid != null)
        {
            result.CurrentPrice = latestBid.BidPrice;
            result.CurrentPriceUserName = latestBid.BidUserName;
            result.CurrentPriceTime = latestBid.BidTime;
            result.UseCountdownTime = latestBid.CreationTime;
        }

        // 获取卡秒状态
        var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItem.Id}");
        result.IsKasec = kasecVal.HasValue && kasecVal == "true";

        // 缓存结果，拍卖中的商品缓存30秒（变化频繁）
        string serializedResult = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        await _redisClient.Database.StringSetAsync(cacheKey, serializedResult, TimeSpan.FromSeconds(30));

        return result;
    }

    // 重写CRUD方法，在数据变更时清理缓存
    public override async Task<AuctionItemDto> CreateAsync(AuctionItemCreateOrUpdateDto input)
    {
        var result = await base.CreateAsync(input);

        // 清除缓存
        ClearAuctionListCache();
        ClearAuctionDetailCache(result.Id);

        return result;
    }

    public override async Task<AuctionItemDto> UpdateAsync(AuctionItemCreateOrUpdateDto input)
    {
        var result = await base.UpdateAsync(input);

        // 清除缓存
        ClearAuctionListCache();
        ClearAuctionDetailCache(result.Id);

        return result;
    }

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        await base.DeleteAsync(input);

        // 清除缓存
        ClearAuctionListCache();
        ClearAuctionDetailCache(input.Id);
    }

    /// <summary>
    /// 生成拍卖品详情缓存键
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string GenerateAuctionDetailCacheKey(long id)
    {
        return $"auction:detail:{id}";
    }

    /// <summary>
    /// 根据拍卖品状态获取详情缓存过期时间（分钟）
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    private int GetDetailCacheExpireMinutes(AuctionStatusEnum status)
    {
        return status switch
        {
            AuctionStatusEnum.拍卖中 => 1, // 拍卖中的商品变化频繁，缓存1分钟
            AuctionStatusEnum.上架 => 5, // 待拍卖商品相对稳定，缓存5分钟
            AuctionStatusEnum.已成交 => 30, // 已成交商品基本不变，缓存30分钟
            AuctionStatusEnum.交易成功 => 60, // 交易完成的商品，缓存1小时
            AuctionStatusEnum.卖家失约 => 60, // 失约状态商品，缓存1小时
            AuctionStatusEnum.买家失约 => 60, // 失约状态商品，缓存1小时
            AuctionStatusEnum.交易关闭 => 60, // 关闭状态商品，缓存1小时
            _ => 10 // 其他状态，缓存10分钟
        };
    }

    /// <summary>
    /// 清除拍卖品详情缓存
    /// </summary>
    /// <param name="auctionItemId">拍卖品ID，如果为null则清除所有详情缓存</param>
    private void ClearAuctionDetailCache(long? auctionItemId = null)
    {
        try
        {
            if (auctionItemId.HasValue)
            {
                // 清除指定商品的详情缓存
                string cacheKey = GenerateAuctionDetailCacheKey(auctionItemId.Value);
                _redisClient.Database.KeyDelete(cacheKey);
            }
            else
            {
                // 清除所有商品详情缓存
                _redisClient.DeleteKeysWithPartten("auction:detail:*");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除拍卖品详情缓存失败，商品ID: {AuctionItemId}", auctionItemId);
        }
    }

    /// <summary>
    /// 清除当前拍卖商品缓存
    /// </summary>
    private void ClearCurrentAuctionCache()
    {
        try
        {
            _redisClient.Database.KeyDelete("auction:current");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除当前拍卖商品缓存失败");
        }
    }
}