using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SqlSugar;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.Abp.Entity;
using TtWork.Abp.Extensions;
using TtWork.Project.Domains;

namespace TtWork.Project.Services;

/// <summary>
/// 出价判断请求
/// </summary>
public class CheckBidEligibilityInput
{
    /// <summary>
    /// 拍卖商品ID
    /// </summary>
    public long AuctionItemId { get; set; }

    /// <summary>
    /// 用户名称
    /// </summary>
    public string BidUserName { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public string BidUserId { get; set; }

    /// <summary>
    /// 出价金额
    /// </summary>
    public int BidPrice { get; set; }
}

/// <summary>
/// 出价判断结果
/// </summary>
public class BidEligibilityResult
{
    /// <summary>
    /// 是否可以出价
    /// </summary>
    public bool CanBid { get; set; }

    /// <summary>
    /// 不能出价的原因（如果可以出价则为空）
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// 最低出价金额
    /// </summary>
    public int MinBidPrice { get; set; }

    /// <summary>
    /// 当前商品价格
    /// </summary>
    public int? CurrentPrice { get; set; }

    /// <summary>
    /// 用户保证金余额
    /// </summary>
    public decimal DepositBalance { get; set; }

    /// <summary>
    /// 用户群聊等级
    /// </summary>
    public int UserLevel { get; set; }

    /// <summary>
    /// 是否处于卡秒状态
    /// </summary>
    public bool IsKasec { get; set; }

    /// <summary>
    /// 商品状态
    /// </summary>
    public AuctionStatusEnum? AuctionStatus { get; set; }

    /// <summary>
    /// 是否被禁言
    /// </summary>
    public bool IsBanned { get; set; }

    /// <summary>
    /// 禁言结束时间（如果被禁言）
    /// </summary>
    public DateTime? BanEndTime { get; set; }
}

/// <summary>
/// 用户出价能力检查结果
/// </summary>
public class UserBidCapabilityResult
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名称
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    public string UserAvatar { get; set; }

    /// <summary>
    /// 是否可以出价
    /// </summary>
    public bool CanBid { get; set; }

    /// <summary>
    /// 不能出价的原因
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// 用户保证金余额
    /// </summary>
    public decimal DepositBalance { get; set; }

    /// <summary>
    /// 用户群聊等级
    /// </summary>
    public int UserLevel { get; set; }

    /// <summary>
    /// 是否被禁言
    /// </summary>
    public bool IsBanned { get; set; }

    /// <summary>
    /// 禁言结束时间（如果被禁言）
    /// </summary>
    public DateTime? BanEndTime { get; set; }

    /// <summary>
    /// 是否是管理员
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// 管理员角色标签
    /// </summary>
    public string AdminTag { get; set; }
}

/// <summary>
/// 出价资格检查服务
/// </summary>
public interface IBidEligibilityService
{
    /// <summary>
    /// 检查用户是否可以出价
    /// </summary>
    /// <param name="input">出价判断请求</param>
    /// <returns>出价判断结果</returns>
    Task<BidEligibilityResult> CheckBidEligibilityAsync(CheckBidEligibilityInput input);

    /// <summary>
    /// 根据用户名称检查用户出价能力
    /// </summary>
    /// <param name="userName">用户名称</param>
    /// <returns>用户出价能力检查结果</returns>
    Task<UserBidCapabilityResult> CheckUserBidCapabilityAsync(string userName);

    /// <summary>
    /// 根据用户ID检查用户出价能力
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户出价能力检查结果</returns>
    Task<UserBidCapabilityResult> CheckUserBidCapabilityAsync(long userId);
}

/// <summary>
/// 出价资格检查服务实现
/// </summary>
public class BidEligibilityService : IBidEligibilityService
{
    private readonly UserCache _userCache;
    private readonly IRepository<AuctionItem, long> _auctionItemRepository;
    private readonly IRepository<BanedUser, long> _banedUserRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly ILogger<BidEligibilityService> _logger;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IMemoryCache _memoryCache;

    // 内存锁字典（替代 Redis 分布式锁）
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> _auctionLocks = new();

    private const string KASEC_CACHE_PREFIX = "Kasec:";

    public BidEligibilityService(
        IMemoryCache memoryCache,
        UserCache userCache,
        IRepository<AuctionItem, long> auctionItemRepository,
        IRepository<BanedUser, long> banedUserRepository,
        IRepository<User, long> userRepository,
        ILogger<BidEligibilityService> logger,
        ISqlSugarClient sqlSugarClient)
    {
        _userCache = userCache;
        _auctionItemRepository = auctionItemRepository;
        _banedUserRepository = banedUserRepository;
        _userRepository = userRepository;
        _logger = logger;
        _sqlSugarClient = sqlSugarClient;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 检查用户是否可以出价
    /// </summary>
    /// <param name="input">出价判断请求</param>
    /// <returns>出价判断结果</returns>
    public async Task<BidEligibilityResult> CheckBidEligibilityAsync(CheckBidEligibilityInput input)
    {
        var result = new BidEligibilityResult();

        try
        {
            // 验证输入参数
            if (input.AuctionItemId <= 0)
            {
                result.CanBid = false;
                result.Reason = "拍卖商品ID无效";
                return result;
            }

            long userId = 0;
            UserDto user = null;

            if (!string.IsNullOrEmpty(input.BidUserId) && long.TryParse(input.BidUserId, out userId))
            {
                user = await _userCache.GetAsync(userId);
            }
            else if (!string.IsNullOrEmpty(input.BidUserName))
            {
                var userEntity = await _userRepository.FirstOrDefaultAsync(u => u.Name == input.BidUserName);
                if (userEntity != null)
                {
                    userId = userEntity.Id;
                    user = await _userCache.GetAsync(userId);
                }
            }

            if (user == null)
            {
                result.CanBid = false;
                result.Reason = "用户信息不存在";
                return result;
            }

            if (input.BidPrice <= 0)
            {
                result.CanBid = false;
                result.Reason = "出价金额必须大于0";
                return result;
            }

            result.DepositBalance = user.DepositBalance;

            // 1. 检查用户群聊等级和保证金
            var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                .Where((a, b) => a.UserId == user.Id)
                .Select((a, b) => new { a.UserId, b.Level })
                .FirstAsync();

            int userLevel = userGroupLevel?.Level ?? 0;
            result.UserLevel = userLevel;

            if (userLevel == 0 && user.DepositBalance < 50)
            {
                result.CanBid = false;
                result.Reason = $"当前用户保证金不足50元，请先去充值保证金（需支付51元，包含1元提现手续费）！当前保证金：{user.DepositBalance}元";
                return result;
            }

            // 2. 检查用户名格式
            if (Regex.IsMatch(user.Name, @"^玩家\d{5}"))
            {
                result.CanBid = false;
                result.Reason = "请先修改昵称后再进行出价";
                return result;
            }

            // 3. 检查管理员权限和禁言状态
            var isChatAdmin = await CheckIsChatAdminAsync(user);
            if (!isChatAdmin.Item1)
            {
                // 非管理员检查禁言状态
                var banedUser = await _banedUserRepository.FirstOrDefaultAsync(a =>
                    a.UserId == userId && a.Chan == "-1_auction" && a.EndTime > DateTime.Now);

                if (banedUser != null)
                {
                    result.CanBid = false;
                    result.IsBanned = true;
                    result.BanEndTime = banedUser.EndTime;
                    result.Reason = $"禁言用户禁止出价,结束时间 {banedUser.EndTime:yyyy-MM-dd HH:mm:ss}";
                    return result;
                }
            }

            // 4. 检查商品信息
            var find = await _auctionItemRepository.FirstOrDefaultAsync(x => x.Id == input.AuctionItemId);
            if (find == null)
            {
                result.CanBid = false;
                result.Reason = "找不到商品";
                return result;
            }

            result.AuctionStatus = find.Status;
            result.CurrentPrice = find.CurrentPrice;

            if (find.Status != AuctionStatusEnum.拍卖中)
            {
                result.CanBid = false;
                result.Reason = "商品不在拍卖中";
                return result;
            }

            // 5. 计算最低出价
            // 简化：如果当前价格为null，直接使用起拍价
            var basePrice = find.CurrentPrice ?? find?.StartingPrice ?? 5;
            var minPrice = 0;

            // 6. 检查卡秒状态（从内存缓存获取）
            var kasecCacheKey = $"{KASEC_CACHE_PREFIX}{input.AuctionItemId}";
            bool isKasec = _memoryCache.TryGetValue(kasecCacheKey, out bool cachedKasecValue) && cachedKasecValue;
            result.IsKasec = isKasec;

            if (find.CurrentPrice.HasValue)
            {
                // 最低加价规则
                if (find.CurrentPrice.Value < 100)
                {
                    minPrice = find.CurrentPrice.Value + 5;
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

                // 卡秒模式下三倍加价
                if (isKasec)
                {
                    minPrice = basePrice + ((minPrice - basePrice) * 3);
                }
            }
            else
            {
                // 首次出价
                minPrice = isKasec ? basePrice * 3 : basePrice;
            }

            result.MinBidPrice = minPrice;

            // 7. 检查出价金额
            if (input.BidPrice < minPrice)
            {
                var priceRules = new[]
                {
                    "1000以内（含），5R一加",
                    "1000~2000，10R一加",
                    "2000~5000，20R一加",
                    "5000~1W，50R一加",
                    "1W以上，100R一加"
                };

                var formattedMessage = "出价必须大于最低加价：\n\n" +
                                       string.Join("\n", priceRules) +
                                       (isKasec ? "\n\n⚠️ 卡秒期间需三倍加价" : "");

                result.CanBid = false;
                result.Reason = formattedMessage;
                return result;
            }

            // 8. 检查内存锁状态（替代 Redis 锁）
            if (_auctionLocks.TryGetValue(input.AuctionItemId, out var semaphore) && semaphore.CurrentCount == 0)
            {
                result.CanBid = false;
                result.Reason = "后台正在处理上一人出价,请稍后再试";
                return result;
            }

            // 所有检查通过
            result.CanBid = true;
            result.Reason = "可以出价";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查出价资格时发生错误，用户ID: {UserId}, 拍卖商品ID: {AuctionItemId}",
                input.BidUserId, input.AuctionItemId);
            result.CanBid = false;
            result.Reason = $"检查出价资格时发生错误: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// 根据用户名称检查用户出价能力
    /// </summary>
    /// <param name="userName">用户名称</param>
    /// <returns>用户出价能力检查结果</returns>
    public async Task<UserBidCapabilityResult> CheckUserBidCapabilityAsync(string userName)
    {
        var result = new UserBidCapabilityResult();

        try
        {
            if (string.IsNullOrEmpty(userName))
            {
                result.CanBid = false;
                result.Reason = "用户名称不能为空";
                return result;
            }

            // 查找用户
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Name == userName);
            if (user == null)
            {
                result.CanBid = false;
                result.Reason = "用户不存在";
                return result;
            }

            return await CheckUserBidCapabilityAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户名称检查出价能力时发生错误，用户名称: {UserName}", userName);
            result.CanBid = false;
            result.Reason = $"检查出价能力时发生错误: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// 根据用户ID检查用户出价能力
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户出价能力检查结果</returns>
    public async Task<UserBidCapabilityResult> CheckUserBidCapabilityAsync(long userId)
    {
        var result = new UserBidCapabilityResult();

        try
        {
            if (userId <= 0)
            {
                result.CanBid = false;
                result.Reason = "用户ID无效";
                return result;
            }

            // 获取用户信息
            var user = await _userCache.GetAsync(userId);
            if (user == null)
            {
                result.CanBid = false;
                result.Reason = "用户信息不存在";
                return result;
            }

            result.UserId = userId;
            result.UserName = user.Name;
            result.UserAvatar = user.HeadImgUrl;
            result.DepositBalance = user.DepositBalance;

            // 1. 检查用户名格式
            if (Regex.IsMatch(user.Name, @"^玩家\d{5}"))
            {
                result.CanBid = false;
                result.Reason = "请先修改昵称后再进行出价";
                return result;
            }

            // 2. 检查用户群聊等级和保证金
            var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                .Where((a, b) => a.UserId == user.Id)
                .Select((a, b) => new { a.UserId, b.Level })
                .FirstAsync();

            int userLevel = userGroupLevel?.Level ?? 0;
            result.UserLevel = userLevel;

            if (userLevel == 0 && user.DepositBalance < 50)
            {
                result.CanBid = false;
                result.Reason = $"当前用户保证金不足50元，请先去充值保证金（需支付51元，包含1元提现手续费）！当前保证金：{user.DepositBalance}元";
                return result;
            }

            // 3. 检查管理员权限和禁言状态
            var isChatAdmin = await CheckIsChatAdminAsync(user);
            result.IsAdmin = isChatAdmin.Item1;
            result.AdminTag = isChatAdmin.Item2;

            if (!isChatAdmin.Item1)
            {
                // 非管理员检查禁言状态
                var banedUser = await _banedUserRepository.FirstOrDefaultAsync(a =>
                    a.UserId == userId && a.Chan == "-1_auction" && a.EndTime > DateTime.Now);

                if (banedUser != null)
                {
                    result.CanBid = false;
                    result.IsBanned = true;
                    result.BanEndTime = banedUser.EndTime;
                    result.Reason = $"禁言用户禁止出价,结束时间 {banedUser.EndTime:yyyy-MM-dd HH:mm:ss}";
                    return result;
                }
            }

            // 所有检查通过
            result.CanBid = true;
            result.Reason = "用户具备出价资格";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户ID检查出价能力时发生错误，用户ID: {UserId}", userId);
            result.CanBid = false;
            result.Reason = $"检查出价能力时发生错误: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// 检查是否为聊天管理员
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <returns>(是否管理员, 管理员标签, 标签类名)</returns>
    private async Task<(bool, string, string)> CheckIsChatAdminAsync(UserDto currentUser)
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
            _logger.LogError(e, "获取用户缓存信息失败");
        }

        return (false, "", "");
    }
}