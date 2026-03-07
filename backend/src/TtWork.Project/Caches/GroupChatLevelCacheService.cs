using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SqlSugar;
using TtWork.Abp.Entity;

namespace TtWork.Project.Caches;

public class GroupChatLevelCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ILogger<GroupChatLevelCacheService> _logger;
    private const string CACHE_KEY = "TtWork:Project:GroupChatLevelSettings:All";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    public GroupChatLevelCacheService(
        IMemoryCache memoryCache,
        ISqlSugarClient sqlSugarClient,
        ILogger<GroupChatLevelCacheService> logger)
    {
        _memoryCache = memoryCache;
        _sqlSugarClient = sqlSugarClient;
        _logger = logger;
    }

    public List<GroupChatLevelSettingsEntity> GetAllSettings()
    {
        if (_memoryCache.TryGetValue(CACHE_KEY, out var cached))
            return (List<GroupChatLevelSettingsEntity>)cached!;

        var settings = _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
            .OrderByDescending(o => o.AmountRequired)
            .ToList();

        _memoryCache.Set(CACHE_KEY, settings, CACHE_DURATION);
        _logger.LogDebug("等级配置已缓存，共 {Count} 条", settings.Count);

        return settings;
    }

    public GroupChatLevelSettingsEntity? GetCorrectLevel(decimal cumulativeAmount)
    {
        var settings = GetAllSettings();
        if (settings.Count == 0)
            return null;
        return settings.FirstOrDefault(w => w.AmountRequired <= cumulativeAmount)
               ?? settings.Last();
    }

    public void InvalidateCache()
    {
        _memoryCache.Remove(CACHE_KEY);
        _logger.LogInformation("等级配置缓存已清除");
    }
}
