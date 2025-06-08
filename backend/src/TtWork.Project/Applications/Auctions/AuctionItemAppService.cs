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
    private readonly IMediator _mediator;
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
        IUnitOfWorkManager unitOfWorkManager) : base(repository,
        iocManager
        )
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
        var msgConfiguration = await _sqlSugarClient.Queryable<MsgConfigurationEntity>().Where(w => w.Type == 1).FirstAsync();
        // 查询用户群聊等级（只针对0级用户判断保证金）
        var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
            .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
            .Where((a, b) => a.UserId == user.Id)
            .Select((a, b) => new { a.UserId, b.Level })
            .FirstAsync();
        int userLevel = userGroupLevel?.Level ?? 0;
        if (userLevel == 0 && user.DepositBalance < 50)
        {
            throw new UserFriendlyException(msgConfiguration != null ? msgConfiguration.Msg : $"当前用户保证金不足50元，请先去充值保证金（需支付51元，包含1元提现手续费）！");
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
            if (!lockTaken) throw new UserFriendlyException(1, "后台正在处理上一人出价,请稍后再试");
            //查询商品信息
            var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.AuctionItemId);
            if (find == null) throw new UserFriendlyException(1, "找不到商品");
            if (find.Status != AuctionStatusEnum.拍卖中) throw new UserFriendlyException(1, "商品不在拍卖中");


            #region MyRegion

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

            #endregion

            // 读取卡秒状态，若为 true，最低加价三倍
            var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{input.AuctionItemId}");
            if (kasecVal.HasValue && kasecVal == "true")
            {
                minPrice = find.CurrentPrice.Value + ((minPrice - find.CurrentPrice.Value) * 3);
            }

            if (input.BidPrice < minPrice)
                throw new UserFriendlyException(1,
                    "出价必须大于最低加价\n 100以内，1R一加。100~1000，5R一加。1000-2000，10R一加。2000-5000，20R一加。50000-1W，50一加。1W以上，100一加" + (kasecVal.HasValue && kasecVal == "true" ? "\n卡秒期间需三倍加价" : ""));

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
                id = Guid.NewGuid(),
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
    /// 执行定时任务
    /// </summary>
    public async virtual void Callback(AuctionItemDto dto, ChatMessage message, UserDto cacheUser, string ip, AuctionItem auctionItem)
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
        //
        using (var uow = _unitOfWorkManager.Begin())
        {
            var find = await _repository.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (find == null)
            {
                throw new UserFriendlyException(1, "找不到商品");
            }

            if (find.Status.HasFlag(AuctionStatusEnum.已成交))
            {
                return;
            }

            // 如果还没有人出价,重新放回待拍卖状态
            if (find.CurrentPrice == null)
            {
                find.Back();
                await CurrentUnitOfWork.SaveChangesAsync();
                return;
            }

            //从bid记录里找出最高价
            var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                .Where(x => x.AuctionItemId == dto.Id)
                .OrderByDescending(x => x.BidPrice)
                .FirstOrDefaultAsync();
            //核对是否和当前价格一致
            if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
            {
                // throw new UserFriendlyException(1, "出价记录不一致");
                //TODO:仅做记录,后面是不是要用到什么通知
                _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
            }
            //计算用户群聊等级
            await AddUserGroupChatLevel(new UserGroupLevelDto
            {
                CumulativeAmount = maxPrice.BidPrice,
                UserId = find.CurrentPriceUserId.Value,
            });

            //发送的信息
            find.SetDeal();
            await CurrentUnitOfWork.SaveChangesAsync();

            var result = ObjectMapper.Map<AuctionItemDto>(find);
            result.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                               ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";
            var bidUser = await _userCache.GetAsync(result.DealUserId!.Value);

            result.Status = AuctionStatusEnum.已成交;
            result.DealUserAvatar = bidUser.HeadImgUrl;
            //查询用户信息
            var userInfo = await _sqlSugarClient.Queryable<RoleEntity, UserRoleEntity, UserInfoEntity>((r, ur, u) => new JoinQueryInfos(
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
            #region 发送群聊数据
            var input = new SendChangeMsgInput()
            {
                Chan = "-1_auction",
                From = message.from,
                Message = new ChatMessage
                {
                    type = ChatMessageType.AuctionEnd,
                    chan = "-1_auction",
                    from = userInfo.Id,
                    fromName = userInfo.Name,
                    avatar = userInfo.HeadImgUrl,
                    msg = "",
                    payload = result,
                    time = message.GetNowTime(),
                }
            };
            //移除'玩家xxxxx加入群聊'的提示
            //只显示已经修改过名字和头像的玩家的提示   
            if (input.Message is { type: ChatMessageType.Welcome })
            {
                // if (Regex.IsMatch(input.Message.fromName, @"^玩家\d{5}"))
                if (input.Chan is "0_lobby" or "-1_auction")
                    return;
            }
            //input.Message = await CheckMsgText(input.Message);
            input.Message.id = Guid.NewGuid();
            //true, "拍卖师", "tag_AuctionManager"
            input.Message.fromAdmin = true;
            input.Message.fromTag = "拍卖师";
            input.Message.tagClass = "tag_AuctionManager";

            //判断input.form在不在redis的chan里

            ImHelper.SendChanMessage(userInfo.LastModifierUserId, input.Chan, input.Message);

            if (input.Message.type != ChatMessageType.Welcome)
            {
                var entityChanMessage = new Message(input.Message)
                {
                    Ip = ip,
                    FromAdmin = input.Message.fromAdmin,
                    FromTag = input.Message.fromTag,
                    TagClass = input.Message.tagClass
                };
                await _messageRepository.InsertAsync(entityChanMessage);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            #endregion

            #region 给指定用户发送数据
            var sendMsgInput = new SendMsgInput
            {
                From = message.from,
                To = result.DealUserId.Value,
                IsReceipt = true,
                Message = new ChatMessage
                {
                    chan = null,
                    type = ChatMessageType.Text,
                    from = userInfo.LastModifierUserId,
                    fromName = userInfo.Name,
                    avatar = userInfo.HeadImgUrl,
                    msg = result.ToUserMsg,
                    to = result.DealUserId.Value,
                    payload = result,
                    time = message.GetNowTime(),
                    fromAdmin = true
                }
            };

            //TODO 判断是否是好友,管理员可以随便发送
            ImHelper.SendMessage(userInfo.LastModifierUserId, [result.DealUserId.Value], sendMsgInput.Message,
                sendMsgInput.IsReceipt);

            var entity = new Message(sendMsgInput.Message)
            {
                Ip = ip,
                FromAdmin = input.Message.fromAdmin,
                FromTag = input.Message.fromTag,
                TagClass = input.Message.tagClass
            };

            await _messageRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _chatListDeleteRepository.GetAll().Where(x =>
                (x.UserId == cacheUser.Id && x.ToUserId == entity.To) || (x.UserId == entity.To && x.ToUserId == cacheUser.Id)).ExecuteDeleteAsync();
            #endregion
            await uow.CompleteAsync();
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
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>().FirstAsync(f => f.UserId == input.UserId);
            if (info == null)
            {
                info = new UserGroupLevelEntity() { CumulativeAmount = 0 };
            }
            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
             .Where(w => w.AmountRequired <= (input.CumulativeAmount + info.CumulativeAmount))   // 找到小于等于当前累计金额的等级配置
             .OrderByDescending(o => o.AmountRequired)                       // 按金额要求降序排序，找到最接近的等级
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
            catch (Exception e)
            {
                return "";
            }
        }
    }
    private async Task<(bool, string, string)> CheckIsChatAdmin(UserDto currentUser)
    {
        try
        {
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
    public  async Task<AuctionItemDto> GetDetail(long id)
    {
        var info=await _sqlSugarClient.Queryable<AuctionItemEntity>().Where(x => x.Id == id).FirstAsync();

        return info.MapTo<AuctionItemDto>();
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

            var find = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (find == null)
            {
                throw new UserFriendlyException(1, "找不到商品");
            }

            if (find.Status.HasFlag(AuctionStatusEnum.已成交))
            {
                return new AuctionItemDto() { ToUserMsg = "已成交商品不能再次拍卖" };
            }
           
            // 如果还没有人出价,重新放回待拍卖状态
            if (find.CurrentPrice == null)
            {
                find.Back();
                await CurrentUnitOfWork.SaveChangesAsync();
                return ObjectMapper.Map<AuctionItemDto>(find);
            }

            //从bid记录里找出最高价
            var maxPrice = await _bidHistoryRepository.GetAll().AsNoTracking()
                .Where(x => x.AuctionItemId == input.Id)
                .OrderByDescending(x => x.BidPrice)
                .FirstOrDefaultAsync();
            //核对是否和当前价格一致
            if (maxPrice == null || maxPrice.BidPrice != find.CurrentPrice)
            {
                // throw new UserFriendlyException(1, "出价记录不一致");
                //TODO:仅做记录,后面是不是要用到什么通知
                _logger.LogError("出价记录不一致,{@find},{@maxPrice}", find, maxPrice);
            }
            //计算用户群聊等级
            await AddUserGroupChatLevel(new UserGroupLevelDto
            {
                CumulativeAmount = maxPrice.BidPrice,
                UserId = find.CurrentPriceUserId.Value,
            });
            //
            find.SetDeal();
            await CurrentUnitOfWork.SaveChangesAsync();
            var result = ObjectMapper.Map<AuctionItemDto>(find);
            result.ToUserMsg = "恭喜您,您拍得了" + find.Name + ",成交价:" + find.FinalPrice +
                               ",\n老板请稍等\n    拍卖师正在联系卖家确认是否交易\n    以及交易的时间地点\n    请耐心等待";

            var bidUser = await _userCache.GetAsync(result.DealUserId!.Value);

            result.DealUserAvatar = bidUser.HeadImgUrl;
            // 结束后自动关闭卡秒状态
            await _redisClient.Database.StringSetAsync($"Auction:Kasec:{input.Id}", false);
            // 广播卡秒状态变更消息
            var msg = new ChatMessage
            {
                type = ChatMessageType.KasecStatusChanged,
                chan = "-1_auction",
                msg = "",
                payload = new { auctionItemId = input.Id, isKasec = false },
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            await _webSocketController.SendChannelMsg(new SendChangeMsgInput
            {
                Chan = "-1_auction",
                From = AbpSession.UserId.Value,
                Message = msg
            });
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("error:", ex);
            throw new UserFriendlyException(1, ex.Message);
        }
        //finally
        //{
        //    slimlock.Release();
        //}
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
        var getAuctionMidList = await Repository.GetAll().AsNoTracking().Where(x => x.Status == AuctionStatusEnum.拍卖中).ToListAsync();
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
        var query = Repository.GetAll().AsNoTracking()
                .WhereIf(!input.Status.HasValue, x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
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

        return new ListResultDto<AuctionItemDto>(
            ObjectMapper.Map<List<AuctionItemDto>>(items)
        );
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
                .WhereIf(!input.Status.HasValue, x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
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
        var bidList = _bidHistoryRepository.GetAll().AsNoTracking().Where(w => idList.Contains(w.AuctionItemId)).ToList();
        foreach (var item in result.Items)
        {
            //查询最新的出价信息
            var info = bidList.Where(w => w.AuctionItemId == item.Id).OrderByDescending(o => o.BidTime).FirstOrDefault();
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
    public class SetKasecStatusInput {
        public long AuctionItemId { get; set; }
        public bool IsKasec { get; set; }
    }

    [HttpPost]
    [AbpAuthorize(AppPermissions.Pages.ChatManager)]
    public async Task<bool> SetKasecStatus([FromBody] SetKasecStatusInput input)
    {
        await _redisClient.Database.StringSetAsync($"Auction:Kasec:{input.AuctionItemId}", input.IsKasec);
        return true;
    }

    // 2. 获取卡秒状态（所有用户）
    [HttpGet]
    public async Task<bool> GetKasecStatus(long auctionItemId)
    {
        var val = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
        return val.HasValue && val == "true";
    }
}