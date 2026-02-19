using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Applications.Dtos;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存管理服务
    /// 纯本地内存缓存架构（移除 Redis L2，解决 DeleteKeysWithPartten 性能问题）
    /// </summary>
    public class AuctionItemCacheManager : IAuctionItemCacheService
    {
        private readonly IRepository<AuctionItem, long> _auctionItemRepository;
        private readonly IRepository<BidHistory, long> _bidHistoryRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ILogger<AuctionItemCacheManager> _logger;
        private readonly IMemoryCache _memoryCache;

        // 缓存锁字典，防止缓存击穿
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

        // 缓存键追踪器（用于 O(1) 前缀匹配清除）
        private static readonly ConcurrentDictionary<string, DateTime> _cacheKeys = new();

        public AuctionItemCacheManager(
            IRepository<AuctionItem, long> auctionItemRepository,
            IRepository<BidHistory, long> bidHistoryRepository,
            IObjectMapper objectMapper,
            ILogger<AuctionItemCacheManager> logger,
            IMemoryCache memoryCache)
        {
            _auctionItemRepository = auctionItemRepository;
            _bidHistoryRepository = bidHistoryRepository;
            _objectMapper = objectMapper;
            _logger = logger;
            _memoryCache = memoryCache;

            // 启动定期清理任务（每10分钟清理过期键追踪）
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(10));
                    CleanupExpiredKeys();
                }
            });
        }

        /// <summary>
        /// 清理过期的缓存键追踪（防止内存泄漏）
        /// </summary>
        private void CleanupExpiredKeys()
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddHours(-1);
                var expiredKeys = _cacheKeys
                    .Where(kvp => kvp.Value < cutoff)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _cacheKeys.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogDebug("清理过期缓存键追踪，数量: {Count}", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期缓存键失败");
            }
        }

        public async Task<ListResultDto<AuctionItemDto>> GetAuctionListAsync(AppResultRequestDto input)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionListFromDatabaseAsync(input);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // 生成缓存键
                string cacheKey = AuctionItemCacheKeys.GenerateListCacheKey(input);

                // 1. 尝试从内存缓存获取数据（~1ms）
                if (_memoryCache.TryGetValue(cacheKey, out ListResultDto<AuctionItemDto> cached))
                {
                    _logger.LogDebug("[PERF-Cache] 拍卖品列表内存缓存命中: {CacheKey}", cacheKey);
                    return cached;
                }

                // 获取缓存锁，防止缓存击穿
                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    // 2. 双重检查：再次检查内存缓存
                    if (_memoryCache.TryGetValue(cacheKey, out cached))
                    {
                        return cached;
                    }

                    // 3. 缓存未命中，从数据库获取
                    var dbResult = await GetAuctionListFromDatabaseAsync(input);

                    // 4. 写入内存缓存（带随机 TTL 防止雪崩）
                    var expireTime = AuctionItemCachePolicy.GetListCacheExpireWithJitter(input.Status);
                    _memoryCache.Set(cacheKey, dbResult, expireTime);
                    
                    // 5. 追踪缓存键（用于批量清除）
                    _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                    sw.Stop();
                    _logger.LogInformation("[PERF-Cache] 拍卖品列表已缓存到内存: {CacheKey}, Count: {Count}, 耗时: {ElapsedMs}ms",
                        cacheKey, dbResult.Items?.Count ?? 0, sw.ElapsedMilliseconds);
                    
                    return dbResult;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[PERF-Cache] 获取拍卖品列表缓存失败，降级到数据库查询，耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return await GetAuctionListFromDatabaseAsync(input);
            }
        }

        public async Task<AuctionItemDto> GetAuctionDetailAsync(long auctionItemId)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionDetailFromDatabaseAsync(auctionItemId);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItemId);

                if (_memoryCache.TryGetValue(cacheKey, out AuctionItemDto cached))
                {
                    _logger.LogDebug("[PERF-Cache] 拍卖品详情内存缓存命中: {CacheKey}", cacheKey);
                    return cached;
                }

                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    if (_memoryCache.TryGetValue(cacheKey, out cached))
                    {
                        return cached;
                    }

                    var result = await GetAuctionDetailFromDatabaseAsync(auctionItemId);

                    if (result != null)
                    {
                        var expireTime = AuctionItemCachePolicy.GetDetailCacheExpire(result.Status);
                        _memoryCache.Set(cacheKey, result, expireTime);
                        _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);
                    }

                    sw.Stop();
                    _logger.LogInformation("[PERF-Cache] 拍卖品详情已缓存到内存: {CacheKey}, 耗时: {ElapsedMs}ms",
                        cacheKey, sw.ElapsedMilliseconds);
                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[PERF-Cache] 获取拍卖品详情缓存失败，ID: {AuctionItemId}, 耗时: {ElapsedMs}ms",
                    auctionItemId, sw.ElapsedMilliseconds);
                return await GetAuctionDetailFromDatabaseAsync(auctionItemId);
            }
        }

        public async Task<AuctionItemDto> GetCurrentAuctionItemAsync()
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetCurrentAuctionItemFromDatabaseAsync();
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                string cacheKey = AuctionItemCacheKeys.CURRENT_AUCTION;

                if (_memoryCache.TryGetValue(cacheKey, out AuctionItemDto cached))
                {
                    _logger.LogDebug("[PERF-Cache] 当前拍卖商品内存缓存命中: {CacheKey}", cacheKey);
                    return cached;
                }

                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    if (_memoryCache.TryGetValue(cacheKey, out cached))
                    {
                        return cached;
                    }

                    var result = await GetCurrentAuctionItemFromDatabaseAsync();

                    var expireTime = AuctionItemCachePolicy.GetCurrentAuctionCacheExpire();
                    _memoryCache.Set(cacheKey, result, expireTime);
                    _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                    sw.Stop();
                    _logger.LogInformation("[PERF-Cache] 当前拍卖商品已缓存到内存: {CacheKey}, 耗时: {ElapsedMs}ms",
                        cacheKey, sw.ElapsedMilliseconds);
                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[PERF-Cache] 获取当前拍卖商品缓存失败，耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return await GetCurrentAuctionItemFromDatabaseAsync();
            }
        }

        public async Task<ListResultDto<AuctionItemDto>> GetAuctionMidListAsync(AppResultRequestDto input)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionMidListFromDatabaseAsync(input);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateMidListCacheKey(input);

                if (_memoryCache.TryGetValue(cacheKey, out ListResultDto<AuctionItemDto> cached))
                {
                    _logger.LogDebug("[PERF-Cache] 拍卖中商品列表内存缓存命中: {CacheKey}", cacheKey);
                    return cached;
                }

                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    if (_memoryCache.TryGetValue(cacheKey, out cached))
                    {
                        return cached;
                    }

                    var result = await GetAuctionMidListFromDatabaseAsync(input);

                    var expireTime = AuctionItemCachePolicy.GetMidListCacheExpire();
                    _memoryCache.Set(cacheKey, result, expireTime);
                    _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                    sw.Stop();
                    _logger.LogInformation("[PERF-Cache] 拍卖中商品列表已缓存到内存: {CacheKey}, Count: {Count}, 耗时: {ElapsedMs}ms",
                        cacheKey, result.Items?.Count ?? 0, sw.ElapsedMilliseconds);

                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[PERF-Cache] 获取拍卖中商品列表缓存失败，耗时: {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return await GetAuctionMidListFromDatabaseAsync(input);
            }
        }

        public Task SetAuctionDetailCacheAsync(AuctionItemDto auctionItem)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled() || auctionItem == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItem.Id);
                var expireTime = AuctionItemCachePolicy.GetDetailCacheExpire(auctionItem.Status);

                _memoryCache.Set(cacheKey, auctionItem, expireTime);
                _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                _logger.LogDebug("[PERF-Cache] 拍卖品详情缓存已设置: {CacheKey}, 过期时间: {ExpireTime}", cacheKey, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖品详情缓存失败，ID: {AuctionItemId}", auctionItem.Id);
            }

            return Task.CompletedTask;
        }

        public Task SetAuctionListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled() || result == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateListCacheKey(input);
                var expireTime = AuctionItemCachePolicy.GetListCacheExpireWithJitter(input.Status);

                _memoryCache.Set(cacheKey, result, expireTime);
                _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                _logger.LogDebug("[PERF-Cache] 拍卖品列表缓存已设置: {CacheKey}, Count: {Count}, 过期时间: {ExpireTime}",
                    cacheKey, result.Items?.Count ?? 0, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖品列表缓存失败");
            }

            return Task.CompletedTask;
        }

        public async Task ClearAuctionCacheAsync(long? auctionItemId = null)
        {
            try
            {
                if (auctionItemId.HasValue)
                {
                    // 清除指定拍卖品的所有相关缓存
                    await ClearAuctionDetailCacheAsync(auctionItemId.Value);
                }
                else
                {
                    // 清除所有拍卖品缓存
                    await ClearAllAuctionCacheAsync();
                }

                // 总是清除列表缓存和当前拍卖缓存
                await ClearAuctionListCacheAsync();
                await ClearCurrentAuctionCacheAsync();

                _logger.LogInformation("拍卖品缓存已清除，ID: {AuctionItemId}", auctionItemId?.ToString() ?? "ALL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除拍卖品缓存失败，ID: {AuctionItemId}", auctionItemId);
            }
        }

        public Task ClearAuctionListCacheAsync(AuctionStatusEnum? status = null)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int cleared = 0;

            try
            {
                // 根据状态确定前缀
                string prefix = status.HasValue
                    ? $"auction:list:{status.Value}:"
                    : "auction:list:";

                // 遍历追踪的键，按前缀匹配清除（O(1) 操作）
                var keysToRemove = _cacheKeys.Keys
                    .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                    cleared++;
                }

                // 同时清除拍卖中商品列表缓存
                var midPrefix = "auction:mid:";
                var midKeysToRemove = _cacheKeys.Keys
                    .Where(k => k.StartsWith(midPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in midKeysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                    cleared++;
                }

                sw.Stop();
                _logger.LogInformation("[PERF-Cache] 拍卖品列表缓存已清除，数量: {Cleared}, 耗时: {ElapsedMs}ms, 状态: {Status}",
                    cleared, sw.ElapsedMilliseconds, status?.ToString() ?? "ALL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除拍卖品列表缓存失败");
            }

            return Task.CompletedTask;
        }

        public Task ClearAuctionDetailCacheAsync(long auctionItemId)
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItemId);

                _memoryCache.Remove(cacheKey);
                _cacheKeys.TryRemove(cacheKey, out _);

                _logger.LogDebug("[PERF-Cache] 拍卖品详情缓存已清除: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除拍卖品详情缓存失败，ID: {AuctionItemId}", auctionItemId);
            }

            return Task.CompletedTask;
        }

        public Task ClearCurrentAuctionCacheAsync()
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.CURRENT_AUCTION;

                _memoryCache.Remove(cacheKey);
                _cacheKeys.TryRemove(cacheKey, out _);

                _logger.LogDebug("[PERF-Cache] 当前拍卖商品缓存已清除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除当前拍卖商品缓存失败");
            }

            return Task.CompletedTask;
        }

        public async Task WarmupCacheAsync()
        {
            try
            {
                _logger.LogInformation("开始拍卖品缓存预热");

                // 预热当前拍卖商品
                await GetCurrentAuctionItemAsync();

                // 预热待拍卖和拍卖中商品列表
                var defaultInput = new AppResultRequestDto { MaxResultCount = AuctionItemCachePolicy.GetWarmupDataLimit() };
                await GetAuctionListAsync(defaultInput);

                // 预热拍卖中商品列表
                var midInput = new AppResultRequestDto { Status = (int)AuctionStatusEnum.拍卖中, MaxResultCount = AuctionItemCachePolicy.GetWarmupDataLimit() };
                await GetAuctionMidListAsync(midInput);

                // 预热已成交商品列表
                var doneInput = new AppResultRequestDto { Status = (int)AuctionStatusEnum.已成交, MaxResultCount = AuctionItemCachePolicy.GetWarmupDataLimit() };
                await GetAuctionListAsync(doneInput);

                _logger.LogInformation("拍卖品缓存预热完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拍卖品缓存预热失败");
            }
        }

        public async Task<object> GetCacheStatsAsync()
        {
            try
            {
                var stats = new
                {
                    CacheEnabled = AuctionItemCachePolicy.IsCacheEnabled(),
                    CacheKeyPatterns = AuctionItemCacheKeys.GetAllCachePatterns(),
                    Timestamp = DateTime.UtcNow
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存统计信息失败");
                return new { Error = ex.Message };
            }
        }

        public async Task RebuildAllCacheAsync()
        {
            try
            {
                _logger.LogInformation("开始重建所有拍卖品缓存");

                // 清除所有缓存
                await ClearAllAuctionCacheAsync();

                // 重新预热缓存
                await WarmupCacheAsync();

                _logger.LogInformation("所有拍卖品缓存重建完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重建拍卖品缓存失败");
            }
        }

        #region 私有方法

        private async Task<ListResultDto<AuctionItemDto>> GetAuctionListFromDatabaseAsync(AppResultRequestDto input)
        {
            if (input.MaxResultCount <= 0)
            {
                input.MaxResultCount = 100;
            }

            var query = _auctionItemRepository.GetAll().AsNoTracking()
                .WhereIf(!input.Status.HasValue,
                    x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
                .WhereIf(input.Status.HasValue, x => (int)x.Status == input.Status!.Value);

            if (!input.Status.HasValue)
            {
                var allItems = await query.OrderBy(x => x.Order).ThenBy(x => x.Id).ToListAsync();
                var resultItems = _objectMapper.Map<List<AuctionItemDto>>(allItems.Take(input.MaxResultCount).ToList());
                return new ListResultDto<AuctionItemDto>(resultItems);
            }
            else if (input.Status == (int)AuctionStatusEnum.已成交)
            {
                query = query.OrderByDescending(x => x.DealTime).Take(input.MaxResultCount);
            }
            else
            {
                query = query.OrderByDescending(x => x.Id).Take(input.MaxResultCount);
            }

            var items = await query.ToListAsync();
            var resultDtos = _objectMapper.Map<List<AuctionItemDto>>(items);
            return new ListResultDto<AuctionItemDto>(resultDtos);
        }

        private async Task<AuctionItemDto> GetAuctionDetailFromDatabaseAsync(long auctionItemId)
        {
            var auctionItem = await _auctionItemRepository.GetAll().AsNoTracking()
                .Where(x => x.Id == auctionItemId)
                .FirstOrDefaultAsync();

            if (auctionItem == null)
            {
                return null;
            }

            var result = _objectMapper.Map<AuctionItemDto>(auctionItem);

            // 如果是拍卖中的商品，获取最新的出价信息
            if (auctionItem.Status == AuctionStatusEnum.拍卖中)
            {
                var latestBid = await _bidHistoryRepository.GetAll().AsNoTracking()
                    .Where(w => w.AuctionItemId == auctionItemId)
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

            return result;
        }

        private async Task<AuctionItemDto> GetCurrentAuctionItemFromDatabaseAsync()
        {
            var auctionItem = await _auctionItemRepository.GetAll().AsNoTracking()
                .Where(x => x.Status == AuctionStatusEnum.拍卖中)
                .FirstOrDefaultAsync();

            if (auctionItem == null)
            {
                return null;
            }

            var result = _objectMapper.Map<AuctionItemDto>(auctionItem);

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

            return result;
        }

        private async Task<ListResultDto<AuctionItemDto>> GetAuctionMidListFromDatabaseAsync(AppResultRequestDto input)
        {
            var query = _auctionItemRepository.GetAll().AsNoTracking()
                .WhereIf(!input.Status.HasValue,
                    x => x.Status == AuctionStatusEnum.上架 || x.Status == AuctionStatusEnum.拍卖中)
                .WhereIf(input.Status.HasValue, x => (int)x.Status == input.Status!.Value);

            if (!input.Status.HasValue)
            {
                query = query.OrderBy(x => x.Order).ThenBy(x => x.Id);
            }

            var items = await query.ToListAsync();
            var result = new ListResultDto<AuctionItemDto>(_objectMapper.Map<List<AuctionItemDto>>(items));

            // 获取所有商品编号
            var idList = items.Select(x => x.Id).ToList();

            // 查询物品出价信息
            var bidList = await _bidHistoryRepository.GetAll().AsNoTracking()
                .Where(w => idList.Contains(w.AuctionItemId))
                .ToListAsync();

            foreach (var item in result.Items)
            {
                // 查询最新的出价信息
                var info = bidList.Where(w => w.AuctionItemId == item.Id)
                    .OrderByDescending(o => o.BidTime)
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

        private Task SetCurrentAuctionCacheAsync(AuctionItemDto currentAuction)
        {
            try
            {
                var expireTime = AuctionItemCachePolicy.GetCurrentAuctionCacheExpire();
                string cacheKey = AuctionItemCacheKeys.CURRENT_AUCTION;

                _memoryCache.Set(cacheKey, currentAuction, expireTime);
                _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                _logger.LogDebug("[PERF-Cache] 当前拍卖商品缓存已设置，过期时间: {ExpireTime}", expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置当前拍卖商品缓存失败");
            }

            return Task.CompletedTask;
        }

        private Task SetAuctionMidListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result)
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateMidListCacheKey(input);
                var expireTime = AuctionItemCachePolicy.GetMidListCacheExpire();

                _memoryCache.Set(cacheKey, result, expireTime);
                _cacheKeys.TryAdd(cacheKey, DateTime.UtcNow);

                _logger.LogDebug("[PERF-Cache] 拍卖中商品列表缓存已设置: {CacheKey}, 过期时间: {ExpireTime}", cacheKey, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖中商品列表缓存失败");
            }

            return Task.CompletedTask;
        }

        private Task ClearAllAuctionCacheAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int cleared = 0;

            try
            {
                // 清除所有追踪的缓存键
                var keysToRemove = _cacheKeys.Keys.ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                    cleared++;
                }

                sw.Stop();
                _logger.LogInformation("[PERF-Cache] 所有拍卖品缓存已清除，数量: {Cleared}, 耗时: {ElapsedMs}ms",
                    cleared, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除所有拍卖品缓存失败");
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
