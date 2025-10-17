using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TtWork.Project.Applications;

namespace TtWork.Project.Caches;

/// <summary>
/// 聊天列表缓存服务 - 使用Redis缓存提升性能
/// </summary>
public class ChatListCacheService
{
    private readonly IDistributedCache _redisCache;
    private readonly ILogger<ChatListCacheService> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);

    public ChatListCacheService(
        IDistributedCache redisCache,
        ILogger<ChatListCacheService> logger)
    {
        _redisCache = redisCache;
        _logger = logger;
    }

    /// <summary>
    /// 获取缓存的聊天列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>聊天列表或null（如果缓存不存在）</returns>
    public async Task<List<ChatListItem>?> GetCachedChatListAsync(long userId)
    {
        try
        {
            string cacheKey = GetCacheKey(userId);
            var cachedData = await _redisCache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
                return null;

            var chatList = JsonSerializer.Deserialize<List<ChatListItem>>(cachedData);

            if (chatList != null)
            {
                _logger.LogDebug($"命中聊天列表缓存，用户ID: {userId}, 聊天数量: {chatList.Count}");
            }

            return chatList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取聊天列表缓存失败，用户ID: {userId}");
            return null; // 缓存失败时降级到数据库查询
        }
    }

    /// <summary>
    /// 缓存聊天列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="chatList">聊天列表</param>
    /// <param name="duration">缓存时长（可选，默认5分钟）</param>
    public async Task SetCachedChatListAsync(long userId, List<ChatListItem> chatList, TimeSpan? duration = null)
    {
        try
        {
            string cacheKey = GetCacheKey(userId);
            var serializedData = JsonSerializer.Serialize(chatList);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration ?? _defaultCacheDuration
            };

            await _redisCache.SetStringAsync(cacheKey, serializedData, options);

            _logger.LogDebug($"缓存聊天列表成功，用户ID: {userId}, 聊天数量: {chatList.Count}, 缓存时长: {options.AbsoluteExpirationRelativeToNow?.TotalMinutes}分钟");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"缓存聊天列表失败，用户ID: {userId}");
            // 缓存失败不影响业务逻辑，静默处理
        }
    }

    /// <summary>
    /// 清除指定用户的聊天列表缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    public async Task ClearChatListCacheAsync(long userId)
    {
        try
        {
            string cacheKey = GetCacheKey(userId);
            await _redisCache.RemoveAsync(cacheKey);

            _logger.LogDebug($"清除聊天列表缓存成功，用户ID: {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"清除聊天列表缓存失败，用户ID: {userId}");
        }
    }

    /// <summary>
    /// 批量清除多个用户的聊天列表缓存
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    public async Task ClearChatListCacheBatchAsync(List<long> userIds)
    {
        if (userIds == null || userIds.Count == 0)
            return;

        try
        {
            var tasks = new List<Task>();
            foreach (var userId in userIds)
            {
                tasks.Add(ClearChatListCacheAsync(userId));
            }

            await Task.WhenAll(tasks);

            _logger.LogDebug($"批量清除聊天列表缓存成功，用户数量: {userIds.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"批量清除聊天列表缓存失败，用户数量: {userIds.Count}");
        }
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>缓存统计信息</returns>
    public async Task<ChatListCacheStats?> GetCacheStatsAsync(long userId)
    {
        try
        {
            string cacheKey = GetCacheKey(userId);
            var cachedData = await _redisCache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
                return null;

            return new ChatListCacheStats
            {
                UserId = userId,
                CacheKey = cacheKey,
                DataSize = cachedData.Length,
                IsCached = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取缓存统计信息失败，用户ID: {userId}");
            return null;
        }
    }

    /// <summary>
    /// 预热缓存 - 为指定用户预先加载聊天列表到缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="chatListLoader">聊天列表加载函数</param>
    public async Task WarmupCacheAsync(long userId, Func<Task<List<ChatListItem>>> chatListLoader)
    {
        try
        {
            var chatList = await chatListLoader();
            if (chatList != null && chatList.Count > 0)
            {
                await SetCachedChatListAsync(userId, chatList);
                _logger.LogDebug($"预热聊天列表缓存成功，用户ID: {userId}, 聊天数量: {chatList.Count}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"预热聊天列表缓存失败，用户ID: {userId}");
        }
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>缓存键</returns>
    private static string GetCacheKey(long userId)
    {
        return $"ChatList:Optimized:{userId}";
    }
}

/// <summary>
/// 聊天列表缓存统计信息
/// </summary>
public class ChatListCacheStats
{
    public long UserId { get; set; }
    public string CacheKey { get; set; }
    public int DataSize { get; set; }
    public bool IsCached { get; set; }
}