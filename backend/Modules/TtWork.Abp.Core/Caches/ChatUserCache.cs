using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Abp.Caches;

/// <summary>
/// 聊天专用用户缓存 - 只缓存聊天列表需要的基本信息
/// 避免加载角色和权限信息，提升性能
/// </summary>
public class ChatUserCache : ITransientDependency
{
    private readonly IMemoryCache _memoryCache;
    private readonly IRepository<User, long> _userRepository;
    private readonly UserCache _userCache; // 作为降级方案

    // 批量缓存，避免重复查询
    private readonly ConcurrentDictionary<long, UserBasicInfo> _basicCache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public ChatUserCache(
        IMemoryCache memoryCache,
        IRepository<User, long> userRepository,
        UserCache userCache)
    {
        _memoryCache = memoryCache;
        _userRepository = userRepository;
        _userCache = userCache;
    }

    /// <summary>
    /// 获取用户基本信息（轻量级版本）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户基本信息</returns>
    public async Task<UserBasicInfo> GetUserBasicAsync(long userId)
    {
        // 1. 尝试从内存缓存获取
        if (_basicCache.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        // 2. 尝试从IMemoryCache获取
        string cacheKey = $"ChatUser_Basic_{userId}";
        if (_memoryCache.TryGetValue(cacheKey, out UserBasicInfo memoryCached))
        {
            _basicCache.TryAdd(userId, memoryCached);
            return memoryCached;
        }

        // 3. 从数据库查询基本信息（只查询必要字段）
        var userInfo = await _userRepository.GetAll()
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserBasicInfo
            {
                Id = x.Id,
                Name = x.Name,
                HeadImgUrl = x.HeadImgUrl
            })
            .FirstOrDefaultAsync();

        if (userInfo != null)
        {
            // 4. 缓存到两个地方
            _basicCache.TryAdd(userId, userInfo);
            _memoryCache.Set(cacheKey, userInfo, _cacheDuration);
        }
        else
        {
            // 5. 降级方案：使用原有UserCache（如果用户不存在或查询失败）
            var fullUser = await _userCache.GetAsync(userId);
            if (fullUser != null)
            {
                userInfo = new UserBasicInfo
                {
                    Id = fullUser.Id,
                    Name = fullUser.Name,
                    HeadImgUrl = fullUser.HeadImgUrl
                };

                _basicCache.TryAdd(userId, userInfo);
                _memoryCache.Set(cacheKey, userInfo, _cacheDuration);
            }
        }

        return userInfo;
    }

    /// <summary>
    /// 批量获取用户基本信息（性能最优方案）
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    /// <returns>用户基本信息字典</returns>
    public async Task<Dictionary<long, UserBasicInfo>> GetBatchUserBasicAsync(List<long> userIds)
    {
        if (userIds == null || userIds.Count == 0)
            return new Dictionary<long, UserBasicInfo>();

        var result = new Dictionary<long, UserBasicInfo>();
        var uncachedIds = new List<long>();

        // 1. 检查哪些用户已经缓存
        foreach (var userId in userIds.Distinct())
        {
            if (_basicCache.TryGetValue(userId, out var cached))
            {
                result[userId] = cached;
            }
            else
            {
                uncachedIds.Add(userId);
            }
        }

        if (uncachedIds.Count > 0)
        {
            // 2. 批量查询未缓存的用户
            var users = await _userRepository.GetAll()
                .AsNoTracking()
                .Where(x => uncachedIds.Contains(x.Id))
                .Select(x => new UserBasicInfo
                {
                    Id = x.Id,
                    Name = x.Name,
                    HeadImgUrl = x.HeadImgUrl
                })
                .ToListAsync();

            // 3. 缓存查询结果
            foreach (var user in users)
            {
                result[user.Id] = user;
                _basicCache.TryAdd(user.Id, user);

                string cacheKey = $"ChatUser_Basic_{user.Id}";
                _memoryCache.Set(cacheKey, user, _cacheDuration);
            }

            // 4. 对查询不到的用户，使用降级方案
            foreach (var userId in uncachedIds.Except(users.Select(u => u.Id)))
            {
                var fullUser = await _userCache.GetAsync(userId);
                if (fullUser != null)
                {
                    var basicInfo = new UserBasicInfo
                    {
                        Id = fullUser.Id,
                        Name = fullUser.Name,
                        HeadImgUrl = fullUser.HeadImgUrl
                    };

                    result[userId] = basicInfo;
                    _basicCache.TryAdd(userId, basicInfo);

                    string cacheKey = $"ChatUser_Basic_{userId}";
                    _memoryCache.Set(cacheKey, basicInfo, _cacheDuration);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 清除指定用户的缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    public void ClearUserCache(long userId)
    {
        _basicCache.TryRemove(userId, out _);
        string cacheKey = $"ChatUser_Basic_{userId}";
        _memoryCache.Remove(cacheKey);
    }

    /// <summary>
    /// 清除所有缓存（用于测试或重置）
    /// </summary>
    public void ClearAllCache()
    {
        _basicCache.Clear();
        // 注意：IMemoryCache不支持批量删除，这里只清理并发字典
    }

    /// <summary>
    /// 预热缓存（可选）
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    public async Task WarmupCacheAsync(List<long> userIds)
    {
        await GetBatchUserBasicAsync(userIds);
    }
}

/// <summary>
/// 用户基本信息（聊天列表专用）
/// 只包含聊天列表需要的字段，避免加载不必要的数据
/// </summary>
public class UserBasicInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string HeadImgUrl { get; set; }
}