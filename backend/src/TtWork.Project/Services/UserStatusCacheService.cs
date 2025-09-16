using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Json;
using Abp.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlSugar;
using StackExchange.Redis;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.Abp.Entity;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;

namespace TtWork.Project.Services
{
    /// <summary>
    /// 用户状态缓存服务 - 优化用户权限、等级、禁言状态检查
    /// </summary>
    public interface IUserStatusCacheService : ITransientDependency
    {
        /// <summary>
        /// 获取用户群聊等级信息（带缓存）
        /// </summary>
        Task<UserGroupLevelInfo> GetUserGroupLevelAsync(long userId);

        /// <summary>
        /// 检查用户禁言状态（带缓存）
        /// </summary>
        Task<BanStatusInfo> CheckBanStatusAsync(long userId, string channel = null);

        /// <summary>
        /// 获取用户管理员信息（带缓存）
        /// </summary>
        Task<AdminInfo> GetAdminInfoAsync(long userId);

        /// <summary>
        /// 获取用户完整状态信息（一次性获取所有状态）
        /// </summary>
        Task<UserFullStatusInfo> GetUserFullStatusAsync(long userId);

        /// <summary>
        /// 批量获取用户状态信息（优化批量查询）
        /// </summary>
        Task<Dictionary<long, UserFullStatusInfo>> BatchGetUserStatusAsync(IEnumerable<long> userIds);

        /// <summary>
        /// 清除用户缓存
        /// </summary>
        Task ClearUserCacheAsync(long userId, bool clearAll = false);
    }

    /// <summary>
    /// 用户状态缓存服务实现
    /// </summary>
    public class UserStatusCacheService : IUserStatusCacheService
    {
        private readonly IRedisClient _redisClient;
        private readonly UserCache _userCache;
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly IRepository<BanedUser, long> _banedUserRepository;
        private readonly ILogger<UserStatusCacheService> _logger;

        // Cache keys
        private const string USER_GROUP_LEVEL_KEY = "UserStatus:GroupLevel:{0}";
        private const string USER_BAN_STATUS_KEY = "UserStatus:BanStatus:{0}:{1}";
        private const string USER_ADMIN_INFO_KEY = "UserStatus:AdminInfo:{0}";
        
        // Cache expiration times
        private static readonly TimeSpan GroupLevelCacheExpiration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan BanStatusCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan AdminInfoCacheExpiration = TimeSpan.FromHours(1);

        public UserStatusCacheService(
            IRedisClient redisClient,
            UserCache userCache,
            ISqlSugarClient sqlSugarClient,
            IRepository<BanedUser, long> banedUserRepository,
            ILogger<UserStatusCacheService> logger)
        {
            _redisClient = redisClient;
            _userCache = userCache;
            _sqlSugarClient = sqlSugarClient;
            _banedUserRepository = banedUserRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户群聊等级信息（带缓存）
        /// </summary>
        public async Task<UserGroupLevelInfo> GetUserGroupLevelAsync(long userId)
        {
            try
            {
                var cacheKey = string.Format(USER_GROUP_LEVEL_KEY, userId);
                var redisDb = _redisClient.Database;
                
                // Try to get from cache first
                var cachedValue = await redisDb.StringGetAsync(cacheKey);
                if (cachedValue.HasValue)
                {
                    var cachedInfo = JsonConvert.DeserializeObject<UserGroupLevelInfo>(cachedValue);
                    
                    // 用户14特殊处理，其他用户采样记录
                    if (userId == 14)
                    {
                        _logger.LogDebug("用户群聊等级缓存命中: UserId={UserId}, Level={Level}", userId, cachedInfo?.Level);
                    }
                    else if (Random.Shared.NextDouble() < 0.05) // 5%采样率
                    {
                        _logger.LogDebug("用户群聊等级缓存命中采样: UserId={UserId}, Level={Level}", userId, cachedInfo?.Level);
                    }
                    return cachedInfo;
                }

                // Cache miss - query database
                var groupLevelInfo = await GetUserGroupLevelFromDatabaseAsync(userId);
                
                // Cache the result
                if (groupLevelInfo != null)
                {
                    await redisDb.StringSetAsync(
                        cacheKey, 
                        JsonConvert.SerializeObject(groupLevelInfo), 
                        GroupLevelCacheExpiration);
                    
                    // 用户14特殊处理，其他用户采样记录
                    if (userId == 14)
                    {
                        _logger.LogDebug("用户群聊等级数据已缓存: UserId={UserId}, Level={Level}", userId, groupLevelInfo.Level);
                    }
                    else if (Random.Shared.NextDouble() < 0.05) // 5%采样率
                    {
                        _logger.LogDebug("用户群聊等级数据已缓存采样: UserId={UserId}, Level={Level}", userId, groupLevelInfo.Level);
                    }
                }

                return groupLevelInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户群聊等级失败，降级到数据库查询: UserId={UserId}", userId);
                return await GetUserGroupLevelFromDatabaseAsync(userId);
            }
        }

        /// <summary>
        /// 检查用户禁言状态（带缓存）
        /// </summary>
        public async Task<BanStatusInfo> CheckBanStatusAsync(long userId, string channel = null)
        {
            try
            {
                var cacheKey = string.Format(USER_BAN_STATUS_KEY, userId, channel ?? "global");
                var redisDb = _redisClient.Database;
                
                // Try to get from cache first (short expiration for time-sensitive data)
                var cachedValue = await redisDb.StringGetAsync(cacheKey);
                if (cachedValue.HasValue)
                {
                    var cachedInfo = JsonConvert.DeserializeObject<BanStatusInfo>(cachedValue);
                    
                    // Validate cached ban hasn't expired
                    if (cachedInfo?.IsBanned == true && cachedInfo.BanEndTime <= DateTime.Now)
                    {
                        // Cache entry is stale, clear it and return fresh data
                        await redisDb.KeyDeleteAsync(cacheKey);
                        return await CheckBanStatusFromDatabaseAsync(userId, channel);
                    }
                    
                    // 用户14特殊处理，其他用户采样记录
                    if (userId == 14)
                    {
                        _logger.LogDebug("用户禁言状态缓存命中: UserId={UserId}, IsBanned={IsBanned}", userId, cachedInfo?.IsBanned);
                    }
                    else if (Random.Shared.NextDouble() < 0.05) // 5%采样率
                    {
                        _logger.LogDebug("用户禁言状态缓存命中采样: UserId={UserId}, IsBanned={IsBanned}", userId, cachedInfo?.IsBanned);
                    }
                    return cachedInfo;
                }

                // Cache miss - query database
                var banStatus = await CheckBanStatusFromDatabaseAsync(userId, channel);
                
                // Cache the result (only if banned, with appropriate expiration)
                if (banStatus.IsBanned)
                {
                    var expiration = banStatus.BanEndTime - DateTime.Now;
                    if (expiration > TimeSpan.Zero)
                    {
                        await redisDb.StringSetAsync(
                            cacheKey, 
                            JsonConvert.SerializeObject(banStatus), 
                            expiration);
                        
                        _logger.LogDebug("用户禁言状态已缓存: UserId={UserId}, Expiration={Expiration}", userId, expiration);
                    }
                }
                else
                {
                    // Cache "not banned" status for short period
                    await redisDb.StringSetAsync(
                        cacheKey, 
                        JsonConvert.SerializeObject(banStatus), 
                        BanStatusCacheExpiration);
                }

                return banStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户禁言状态失败，降级到数据库查询: UserId={UserId}", userId);
                return await CheckBanStatusFromDatabaseAsync(userId, channel);
            }
        }

        /// <summary>
        /// 获取用户管理员信息（带缓存）
        /// </summary>
        public async Task<AdminInfo> GetAdminInfoAsync(long userId)
        {
            try
            {
                var cacheKey = string.Format(USER_ADMIN_INFO_KEY, userId);
                var redisDb = _redisClient.Database;
                
                // Try to get from cache first
                var cachedValue = await redisDb.StringGetAsync(cacheKey);
                if (cachedValue.HasValue)
                {
                    var cachedInfo = JsonConvert.DeserializeObject<AdminInfo>(cachedValue);
                    
                    // 增加用户14的详细日志
                    if (userId == 14)
                    {
                        _logger.LogInformation("=== 用户14管理员信息缓存命中 === UserId={UserId}, IsAdmin={IsAdmin}, AdminTag={AdminTag}, TagClass={TagClass}", 
                            userId, cachedInfo?.IsAdmin, cachedInfo?.AdminTag, cachedInfo?.TagClass);
                    }
                    else if (Random.Shared.NextDouble() < 0.05) // 5%采样率
                    {
                        _logger.LogDebug("用户管理员信息缓存命中采样: UserId={UserId}, IsAdmin={IsAdmin}", userId, cachedInfo?.IsAdmin);
                    }
                    
                    return cachedInfo;
                }

                // Cache miss - get from user cache and process
                var adminInfo = await GetAdminInfoFromUserCacheAsync(userId);
                
                // Cache the result
                if (adminInfo != null)
                {
                    await redisDb.StringSetAsync(
                        cacheKey, 
                        JsonConvert.SerializeObject(adminInfo), 
                        AdminInfoCacheExpiration);
                    
                    // 增加用户14的详细日志
                    if (userId == 14)
                    {
                        _logger.LogInformation("=== 用户14管理员信息已缓存 === UserId={UserId}, IsAdmin={IsAdmin}, AdminTag={AdminTag}, TagClass={TagClass}", 
                            userId, adminInfo.IsAdmin, adminInfo.AdminTag, adminInfo.TagClass);
                    }
                    else
                    {
                        _logger.LogDebug("用户管理员信息已缓存: UserId={UserId}, IsAdmin={IsAdmin}", userId, adminInfo.IsAdmin);
                    }
                }

                return adminInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户管理员信息失败，降级到用户缓存查询: UserId={UserId}", userId);
                return await GetAdminInfoFromUserCacheAsync(userId);
            }
        }

        /// <summary>
        /// 获取用户完整状态信息（一次性获取所有状态）
        /// </summary>
        public async Task<UserFullStatusInfo> GetUserFullStatusAsync(long userId)
        {
            try
            {
                // 并行查询所有状态信息
                var groupLevelTask = GetUserGroupLevelAsync(userId);
                var adminInfoTask = GetAdminInfoAsync(userId);
                var banStatusTask = CheckBanStatusAsync(userId);
                
                await Task.WhenAll(groupLevelTask, adminInfoTask, banStatusTask);
                
                var groupLevel = await groupLevelTask;
                var adminInfo = await adminInfoTask;
                var banStatus = await banStatusTask;
                
                // 增加用户14的详细日志
                if (userId == 14)
                {
                    _logger.LogInformation("=== 用户14完整状态信息获取完成 === UserId={UserId}", userId);
                    _logger.LogInformation("用户14群聊等级信息: Level={Level}, Name={Name}", groupLevel?.Level, groupLevel?.Name);
                    _logger.LogInformation("用户14管理员信息: IsAdmin={IsAdmin}, AdminTag={AdminTag}, TagClass={TagClass}", 
                        adminInfo?.IsAdmin, adminInfo?.AdminTag, adminInfo?.TagClass);
                    _logger.LogInformation("用户14禁言状态: IsBanned={IsBanned}, BanEndTime={BanEndTime}", 
                        banStatus?.IsBanned, banStatus?.BanEndTime);
                }
                
                return new UserFullStatusInfo
                {
                    UserId = userId,
                    GroupLevel = groupLevel,
                    AdminInfo = adminInfo,
                    BanStatus = banStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户完整状态信息失败: UserId={UserId}", userId);
                
                // 降级到逐个查询
                return new UserFullStatusInfo
                {
                    UserId = userId,
                    GroupLevel = await GetUserGroupLevelAsync(userId),
                    AdminInfo = await GetAdminInfoAsync(userId),
                    BanStatus = await CheckBanStatusAsync(userId)
                };
            }
        }

        /// <summary>
        /// 批量获取用户状态信息（优化批量查询）
        /// </summary>
        public async Task<Dictionary<long, UserFullStatusInfo>> BatchGetUserStatusAsync(IEnumerable<long> userIds)
        {
            var userStatusDict = new Dictionary<long, UserFullStatusInfo>();
            var userIdList = userIds.ToList();
            
            try
            {
                // 并行处理所有用户
                var userTasks = userIdList.Select(async userId => 
                {
                    var userStatus = await GetUserFullStatusAsync(userId);
                    return new { UserId = userId, UserStatus = userStatus };
                });

                var results = await Task.WhenAll(userTasks);
                
                foreach (var result in results)
                {
                    userStatusDict[result.UserId] = result.UserStatus;
                }

                return userStatusDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量获取用户状态失败，降级到逐个查询");
                
                // 降级到逐个查询
                foreach (var userId in userIdList)
                {
                    try
                    {
                        userStatusDict[userId] = await GetUserFullStatusAsync(userId);
                    }
                    catch (Exception userEx)
                    {
                        _logger.LogError(userEx, "获取单个用户状态失败: UserId={UserId}", userId);
                        userStatusDict[userId] = new UserFullStatusInfo { UserId = userId };
                    }
                }

                return userStatusDict;
            }
        }

        /// <summary>
        /// 清除用户缓存
        /// </summary>
        public async Task ClearUserCacheAsync(long userId, bool clearAll = false)
        {
            try
            {
                var redisDb = _redisClient.Database;
                var tasks = new List<Task>();
                
                // Always clear group level cache
                tasks.Add(redisDb.KeyDeleteAsync(string.Format(USER_GROUP_LEVEL_KEY, userId)));
                
                // Always clear admin info cache
                tasks.Add(redisDb.KeyDeleteAsync(string.Format(USER_ADMIN_INFO_KEY, userId)));
                
                if (clearAll)
                {
                    // Clear all ban status cache keys for this user
                    var banKeysPattern = string.Format(USER_BAN_STATUS_KEY, userId, "*");
                    _redisClient.DeleteKeysWithPartten(banKeysPattern);
                }
                
                await Task.WhenAll(tasks);
                _logger.LogInformation("用户缓存已清除: UserId={UserId}, ClearAll={ClearAll}", userId, clearAll);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除用户缓存失败: UserId={UserId}", userId);
            }
        }

        #region Private Methods

        private async Task<UserGroupLevelInfo> GetUserGroupLevelFromDatabaseAsync(long userId)
        {
            try
            {
                // Get default level first
                var defaultLevel = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
                    .FirstAsync(f => f.Level == 0);

                // Get user's specific level
                var userLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                    .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                    .Where((a, b) => a.UserId == userId)
                    .Select((a, b) => new UserGroupLevelInfo
                    {
                        UserId = a.UserId,
                        Name = b.Name,
                        Level = b.Level,
                        BorderColor = b.BorderColor,
                        RightBorderColor = b.RightBorderColor
                    })
                    .FirstAsync();

                // Return user level or default level
                return userLevel ?? new UserGroupLevelInfo
                {
                    UserId = userId,
                    Name = defaultLevel?.Name ?? "默认等级",
                    Level = defaultLevel?.Level ?? 0,
                    BorderColor = defaultLevel?.BorderColor,
                    RightBorderColor = defaultLevel?.RightBorderColor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从数据库获取用户群聊等级失败: UserId={UserId}", userId);
                return new UserGroupLevelInfo
                {
                    UserId = userId,
                    Name = "默认等级",
                    Level = 0,
                    BorderColor = "",
                    RightBorderColor = ""
                };
            }
        }

        private async Task<BanStatusInfo> CheckBanStatusFromDatabaseAsync(long userId, string channel = null)
        {
            try
            {
                var banedUser = await _banedUserRepository.FirstOrDefaultAsync(a =>
                    a.UserId == userId && (a.Chan == null || a.Chan == channel) && a.EndTime > DateTime.Now);

                return banedUser != null 
                    ? new BanStatusInfo 
                    { 
                        IsBanned = true, 
                        BanEndTime = banedUser.EndTime,
                        Channel = banedUser.Chan
                    }
                    : new BanStatusInfo { IsBanned = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从数据库检查用户禁言状态失败: UserId={UserId}", userId);
                return new BanStatusInfo { IsBanned = false };
            }
        }

        private async Task<AdminInfo> GetAdminInfoFromUserCacheAsync(long userId)
        {
            try
            {
                var userDto = await _userCache.GetAsync(userId);
                if (userDto == null || userDto.RoleNames == null || userDto.RoleNames.Length == 0)
                {
                    return new AdminInfo { IsAdmin = false, AdminTag = "", TagClass = "" };
                }

                return userDto.RoleNames switch
                {
                    var roles when roles.Contains("Admin") => new AdminInfo { IsAdmin = true, AdminTag = "系统管理员", TagClass = "tag_Admin" },
                    var roles when roles.Contains("AuctionManager") => new AdminInfo { IsAdmin = true, AdminTag = "拍卖师", TagClass = "tag_AuctionManager" },
                    var roles when roles.Contains("Manager") => new AdminInfo { IsAdmin = true, AdminTag = "管理员", TagClass = "tag_Manager" },
                    var roles when roles.Contains("AuctionUser") => new AdminInfo { IsAdmin = false, AdminTag = "竞拍用户", TagClass = "tag_AudtionUser" },
                    _ => new AdminInfo { IsAdmin = false, AdminTag = "", TagClass = "" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从用户缓存获取管理员信息失败: UserId={UserId}", userId);
                return new AdminInfo { IsAdmin = false, AdminTag = "", TagClass = "" };
            }
        }

        #endregion
    }

    /// <summary>
    /// 用户群聊等级信息
    /// </summary>
    public class UserGroupLevelInfo
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string BorderColor { get; set; }
        public string RightBorderColor { get; set; }
    }

    /// <summary>
    /// 用户禁言状态信息
    /// </summary>
    public class BanStatusInfo
    {
        public bool IsBanned { get; set; }
        public DateTime? BanEndTime { get; set; }
        public string Channel { get; set; }
    }

    /// <summary>
    /// 用户管理员信息
    /// </summary>
    public class AdminInfo
    {
        public bool IsAdmin { get; set; }
        public string AdminTag { get; set; }
        public string TagClass { get; set; }
    }

    /// <summary>
    /// 用户完整状态信息
    /// </summary>
    public class UserFullStatusInfo
    {
        public long UserId { get; set; }
        public UserGroupLevelInfo GroupLevel { get; set; }
        public AdminInfo AdminInfo { get; set; }
        public BanStatusInfo BanStatus { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public bool IsActive { get; set; } = true;
    }
}