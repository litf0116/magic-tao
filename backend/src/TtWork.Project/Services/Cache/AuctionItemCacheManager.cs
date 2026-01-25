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
using Newtonsoft.Json;
using TtWork.Abp.Applications.Dtos;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存管理服务
    /// 支持两层缓存架构：L1 本地内存缓存 + L2 Redis 分布式缓存
    /// </summary>
    public class AuctionItemCacheManager : IAuctionItemCacheService
    {
        private readonly IRedisClient _redisClient;
        private readonly IRepository<AuctionItem, long> _auctionItemRepository;
        private readonly IRepository<BidHistory, long> _bidHistoryRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ILogger<AuctionItemCacheManager> _logger;
        private readonly IMemoryCache _memoryCache;

        // 本地缓存TTL（秒）- 设为L2缓存TTL的约1/6，避免脏数据
        private const int LOCAL_CACHE_TTL_SECONDS = 10;

        // 缓存锁字典，防止缓存击穿
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

        // L1 本地缓存键追踪器，用于主动清除
        private static readonly ConcurrentDictionary<string, byte> _l1CacheKeys = new();

        public AuctionItemCacheManager(
            IRedisClient redisClient,
            IRepository<AuctionItem, long> auctionItemRepository,
            IRepository<BidHistory, long> bidHistoryRepository,
            IObjectMapper objectMapper,
            ILogger<AuctionItemCacheManager> logger,
            IMemoryCache memoryCache)
        {
            _redisClient = redisClient;
            _auctionItemRepository = auctionItemRepository;
            _bidHistoryRepository = bidHistoryRepository;
            _objectMapper = objectMapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<ListResultDto<AuctionItemDto>> GetAuctionListAsync(AppResultRequestDto input)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionListFromDatabaseAsync(input);
            }

            try
            {
                // 生成缓存键
                string cacheKey = AuctionItemCacheKeys.GenerateListCacheKey(input);
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);

                // 1. 尝试从 L1 本地缓存获取数据（最快，~1ms）
                if (_memoryCache.TryGetValue(localCacheKey, out ListResultDto<AuctionItemDto> localCached))
                {
                    _logger.LogDebug("拍卖品列表 L1 缓存命中: {CacheKey}", localCacheKey);
                    return localCached;
                }

                // 获取缓存锁，防止缓存击穿
                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    // 2. 双重检查：再次检查 L1 缓存（可能在等待锁时已被其他请求填充）
                    if (_memoryCache.TryGetValue(localCacheKey, out localCached))
                    {
                        return localCached;
                    }

                    // 3. 尝试从 L2 Redis 缓存获取数据（较快，~20ms）
                    var redisValue = await _redisClient.Database.StringGetAsync(cacheKey);
                    if (redisValue.HasValue)
                    {
                        var result = JsonConvert.DeserializeObject<ListResultDto<AuctionItemDto>>(redisValue);
                        // 写入 L1 缓存
                        _memoryCache.Set(localCacheKey, result, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                        // 追踪 L1 缓存键
                        _l1CacheKeys.TryAdd(localCacheKey, 0);
                        _logger.LogDebug("拍卖品列表 L2 缓存命中，已写入 L1: {CacheKey}, Count: {Count}", cacheKey, result.Items?.Count ?? 0);
                        return result;
                    }

                    // 4. 缓存未命中，从数据库获取（较慢，~50ms 索引优化后 ~5ms）
                    var dbResult = await GetAuctionListFromDatabaseAsync(input);

                    // 设置缓存（带随机 TTL 防止雪崩）
                    await SetAuctionListCacheAsync(input, dbResult);
                    // 写入 L1 缓存
                    _memoryCache.Set(localCacheKey, dbResult, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                    // 追踪 L1 缓存键
                    _l1CacheKeys.TryAdd(localCacheKey, 0);

                    _logger.LogDebug("拍卖品列表数据已缓存: {CacheKey}, Count: {Count}", cacheKey, dbResult.Items?.Count ?? 0);
                    return dbResult;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取拍卖品列表缓存失败，降级到数据库查询");
                return await GetAuctionListFromDatabaseAsync(input);
            }
        }

        public async Task<AuctionItemDto> GetAuctionDetailAsync(long auctionItemId)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionDetailFromDatabaseAsync(auctionItemId);
            }

            try
            {
                // 生成缓存键
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItemId);
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);

                // 1. 尝试从 L1 本地缓存获取数据（最快，~1ms）
                if (_memoryCache.TryGetValue(localCacheKey, out AuctionItemDto localCached))
                {
                    _logger.LogDebug("拍卖品详情 L1 缓存命中: {CacheKey}", localCacheKey);
                    return localCached;
                }

                // 获取缓存锁，防止缓存击穿
                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    // 2. 双重检查：再次检查 L1 缓存（可能在等待锁时已被其他请求填充）
                    if (_memoryCache.TryGetValue(localCacheKey, out localCached))
                    {
                        return localCached;
                    }

                    // 3. 尝试从 L2 Redis 缓存获取数据（较快，~20ms）
                    var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
                    if (cachedValue.HasValue)
                    {
                        var cachedResult = JsonConvert.DeserializeObject<AuctionItemDto>(cachedValue);
                        // 写入 L1 缓存
                        _memoryCache.Set(localCacheKey, cachedResult, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                        // 追踪 L1 缓存键
                        _l1CacheKeys.TryAdd(localCacheKey, 0);
                        _logger.LogDebug("拍卖品详情 L2 缓存命中，已写入 L1: {CacheKey}", cacheKey);
                        return cachedResult;
                    }

                    // 4. 缓存未命中，从数据库获取（较慢，~50ms）
                    var result = await GetAuctionDetailFromDatabaseAsync(auctionItemId);

                    // 设置缓存（带随机 TTL 防止雪崩）
                    if (result != null)
                    {
                        await SetAuctionDetailCacheAsync(result);
                        // 写入 L1 缓存
                        _memoryCache.Set(localCacheKey, result, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                        // 追踪 L1 缓存键
                        _l1CacheKeys.TryAdd(localCacheKey, 0);
                    }

                    _logger.LogDebug("拍卖品详情数据已缓存: {CacheKey}", cacheKey);
                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取拍卖品详情缓存失败，降级到数据库查询，ID: {AuctionItemId}", auctionItemId);
                return await GetAuctionDetailFromDatabaseAsync(auctionItemId);
            }
        }

        public async Task<AuctionItemDto> GetCurrentAuctionItemAsync()
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetCurrentAuctionItemFromDatabaseAsync();
            }

            try
            {
                // 生成缓存键
                string cacheKey = AuctionItemCacheKeys.CURRENT_AUCTION;
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);

                // 1. 尝试从 L1 本地缓存获取数据（最快，~1ms）
                if (_memoryCache.TryGetValue(localCacheKey, out AuctionItemDto localCached))
                {
                    _logger.LogDebug("当前拍卖商品 L1 缓存命中: {CacheKey}", localCacheKey);
                    // 处理 "null" 缓存标记
                    if (localCached == null)
                    {
                        return null;
                    }
                    return localCached;
                }

                // 获取缓存锁，防止缓存击穿
                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    // 2. 双重检查：再次检查 L1 缓存
                    if (_memoryCache.TryGetValue(localCacheKey, out localCached))
                    {
                        if (localCached == null)
                        {
                            return null;
                        }
                        return localCached;
                    }

                    // 3. 尝试从 L2 Redis 缓存获取数据（较快，~20ms）
                    var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
                    if (cachedValue.HasValue)
                    {
                        if (cachedValue == "null")
                        {
                            // 缓存空结果，避免缓存穿透
                            _memoryCache.Set<AuctionItemDto>(localCacheKey, null, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                            _l1CacheKeys.TryAdd(localCacheKey, 0);
                            _logger.LogDebug("当前拍卖商品 L2 缓存命中，空结果: {CacheKey}", cacheKey);
                            return null;
                        }

                        var cachedResult = JsonConvert.DeserializeObject<AuctionItemDto>(cachedValue);
                        // 写入 L1 缓存
                        _memoryCache.Set(localCacheKey, cachedResult, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                        // 追踪 L1 缓存键
                        _l1CacheKeys.TryAdd(localCacheKey, 0);
                        _logger.LogDebug("当前拍卖商品 L2 缓存命中，已写入 L1: {CacheKey}", cacheKey);
                        return cachedResult;
                    }

                    // 4. 缓存未命中，从数据库获取（较慢，~50ms）
                    var result = await GetCurrentAuctionItemFromDatabaseAsync();

                    // 设置缓存（带随机 TTL 防止雪崩）
                    await SetCurrentAuctionCacheAsync(result);
                    // 写入 L1 缓存
                    _memoryCache.Set(localCacheKey, result, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                    // 追踪 L1 缓存键
                    _l1CacheKeys.TryAdd(localCacheKey, 0);

                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前拍卖商品缓存失败，降级到数据库查询");
                return await GetCurrentAuctionItemFromDatabaseAsync();
            }
        }

        public async Task<ListResultDto<AuctionItemDto>> GetAuctionMidListAsync(AppResultRequestDto input)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                return await GetAuctionMidListFromDatabaseAsync(input);
            }

            try
            {
                // 生成缓存键
                string cacheKey = AuctionItemCacheKeys.GenerateMidListCacheKey(input);
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);

                // 1. 尝试从 L1 本地缓存获取数据（最快，~1ms）
                if (_memoryCache.TryGetValue(localCacheKey, out ListResultDto<AuctionItemDto> localCached))
                {
                    _logger.LogDebug("拍卖中商品列表 L1 缓存命中: {CacheKey}", localCacheKey);
                    return localCached;
                }

                // 获取缓存锁，防止缓存击穿
                var semaphore = _cacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    // 2. 双重检查：再次检查 L1 缓存
                    if (_memoryCache.TryGetValue(localCacheKey, out localCached))
                    {
                        return localCached;
                    }

                    // 3. 尝试从 L2 Redis 缓存获取数据（较快，~20ms）
                    var cachedValue = await _redisClient.Database.StringGetAsync(cacheKey);
                    if (cachedValue.HasValue)
                    {
                        var cachedResult = JsonConvert.DeserializeObject<ListResultDto<AuctionItemDto>>(cachedValue);
                        // 写入 L1 缓存
                        _memoryCache.Set(localCacheKey, cachedResult, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                        // 追踪 L1 缓存键
                        _l1CacheKeys.TryAdd(localCacheKey, 0);
                        _logger.LogDebug("拍卖中商品列表 L2 缓存命中，已写入 L1: {CacheKey}, Count: {Count}", cacheKey, cachedResult.Items?.Count ?? 0);
                        return cachedResult;
                    }

                    // 4. 缓存未命中，从数据库获取（较慢，~50ms）
                    var result = await GetAuctionMidListFromDatabaseAsync(input);

                    // 设置缓存
                    await SetAuctionMidListCacheAsync(input, result);
                    // 写入 L1 缓存
                    _memoryCache.Set(localCacheKey, result, TimeSpan.FromSeconds(LOCAL_CACHE_TTL_SECONDS));
                    // 追踪 L1 缓存键
                    _l1CacheKeys.TryAdd(localCacheKey, 0);

                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取拍卖中商品列表缓存失败，降级到数据库查询");
                return await GetAuctionMidListFromDatabaseAsync(input);
            }
        }

        public async Task SetAuctionDetailCacheAsync(AuctionItemDto auctionItem)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled() || auctionItem == null)
            {
                return;
            }

            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItem.Id);
                var expireTime = AuctionItemCachePolicy.GetDetailCacheExpire(auctionItem.Status);
                string serializedData = JsonConvert.SerializeObject(auctionItem);

                await _redisClient.Database.StringSetAsync(cacheKey, serializedData, expireTime);
                _logger.LogDebug("拍卖品详情缓存已设置: {CacheKey}, 过期时间: {ExpireTime}", cacheKey, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖品详情缓存失败，ID: {AuctionItemId}", auctionItem.Id);
            }
        }

        public async Task SetAuctionListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result)
        {
            if (!AuctionItemCachePolicy.IsCacheEnabled() || result == null)
            {
                return;
            }

            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateListCacheKey(input);
                // 使用带随机偏移的 TTL，防止缓存雪崩
                var expireTime = AuctionItemCachePolicy.GetListCacheExpireWithJitter(input.Status);
                string serializedData = JsonConvert.SerializeObject(result);

                await _redisClient.Database.StringSetAsync(cacheKey, serializedData, expireTime);
                _logger.LogDebug("拍卖品列表缓存已设置: {CacheKey}, Count: {Count}, 过期时间: {ExpireTime}",
                    cacheKey, result.Items?.Count ?? 0, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖品列表缓存失败");
            }
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

        public async Task ClearAuctionListCacheAsync(AuctionStatusEnum? status = null)
        {
            try
            {
                // 清除 L2 Redis 缓存
                var patterns = AuctionItemCacheKeys.GetListCachePatterns(status);
                foreach (var pattern in patterns)
                {
                    _redisClient.DeleteKeysWithPartten(pattern);
                }

                // 同时清除拍卖中商品列表缓存
                var midPatterns = AuctionItemCacheKeys.GetMidListCachePatterns();
                foreach (var pattern in midPatterns)
                {
                    _redisClient.DeleteKeysWithPartten(pattern);
                }

                // 主动清除 L1 本地缓存
                foreach (var key in _l1CacheKeys.Keys)
                {
                    if (key.StartsWith("local:auction:list:", StringComparison.OrdinalIgnoreCase) ||
                        key.StartsWith("local:auction:mid:", StringComparison.OrdinalIgnoreCase))
                    {
                        _memoryCache.Remove(key);
                    }
                }
                // 清除 L1 键追踪器中列表相关缓存键
                var keysToRemove = _l1CacheKeys.Keys
                    .Where(k => k.StartsWith("local:auction:list:", StringComparison.OrdinalIgnoreCase) ||
                                k.StartsWith("local:auction:mid:", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    _l1CacheKeys.TryRemove(key, out _);
                }

                _logger.LogDebug("拍卖品列表缓存已清除，状态过滤: {Status}", status?.ToString() ?? "ALL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除拍卖品列表缓存失败");
            }
        }

        public async Task ClearAuctionDetailCacheAsync(long auctionItemId)
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateDetailCacheKey(auctionItemId);
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);
                
                // 清除 L2 Redis 缓存
                await _redisClient.Database.KeyDeleteAsync(cacheKey);
                
                // 清除 L1 本地缓存
                _memoryCache.Remove(localCacheKey);
                
                // 从 L1 键追踪器中移除
                _l1CacheKeys.TryRemove(localCacheKey, out _);
                
                _logger.LogDebug("拍卖品详情缓存已清除: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除拍卖品详情缓存失败，ID: {AuctionItemId}", auctionItemId);
            }
        }

        public async Task ClearCurrentAuctionCacheAsync()
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.CURRENT_AUCTION;
                string localCacheKey = AuctionItemCacheKeys.GenerateLocalCacheKey(cacheKey);
                
                // 清除 L2 Redis 缓存
                await _redisClient.Database.KeyDeleteAsync(cacheKey);
                
                // 清除 L1 本地缓存
                _memoryCache.Remove(localCacheKey);
                
                // 从 L1 键追踪器中移除
                _l1CacheKeys.TryRemove(localCacheKey, out _);
                
                _logger.LogDebug("当前拍卖商品缓存已清除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除当前拍卖商品缓存失败");
            }
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
                query = query.OrderBy(x => x.Order).ThenBy(x => x.Id).Take(input.MaxResultCount);
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
            var dtoItems = _objectMapper.Map<List<AuctionItemDto>>(items);
            return new ListResultDto<AuctionItemDto>(dtoItems);
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

            // 获取卡秒状态
            var kasecKey = AuctionItemCacheKeys.GenerateKasecCacheKey(auctionItemId);
            var kasecVal = await _redisClient.Database.StringGetAsync(kasecKey);
            result.IsKasec = kasecVal.HasValue && kasecVal == "true";

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

            // 获取卡秒状态
            var kasecKey = AuctionItemCacheKeys.GenerateKasecCacheKey(auctionItem.Id);
            var kasecVal = await _redisClient.Database.StringGetAsync(kasecKey);
            result.IsKasec = kasecVal.HasValue && kasecVal == "true";

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

        private async Task SetCurrentAuctionCacheAsync(AuctionItemDto currentAuction)
        {
            try
            {
                var expireTime = AuctionItemCachePolicy.GetCurrentAuctionCacheExpire();
                
                if (currentAuction == null)
                {
                    // 缓存空结果，避免缓存穿透
                    await _redisClient.Database.StringSetAsync(AuctionItemCacheKeys.CURRENT_AUCTION, "null", 
                        AuctionItemCachePolicy.GetNullResultCacheExpire());
                }
                else
                {
                    string serializedData = JsonConvert.SerializeObject(currentAuction);
                    await _redisClient.Database.StringSetAsync(AuctionItemCacheKeys.CURRENT_AUCTION, serializedData, expireTime);
                }

                _logger.LogDebug("当前拍卖商品缓存已设置，过期时间: {ExpireTime}", expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置当前拍卖商品缓存失败");
            }
        }

        private async Task SetAuctionMidListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result)
        {
            try
            {
                string cacheKey = AuctionItemCacheKeys.GenerateMidListCacheKey(input);
                var expireTime = AuctionItemCachePolicy.GetMidListCacheExpire();
                string serializedData = JsonConvert.SerializeObject(result);

                await _redisClient.Database.StringSetAsync(cacheKey, serializedData, expireTime);
                _logger.LogDebug("拍卖中商品列表缓存已设置: {CacheKey}, 过期时间: {ExpireTime}", cacheKey, expireTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置拍卖中商品列表缓存失败");
            }
        }

        private async Task ClearAllAuctionCacheAsync()
        {
            try
            {
                var patterns = AuctionItemCacheKeys.GetAllCachePatterns();
                foreach (var pattern in patterns)
                {
                    _redisClient.DeleteKeysWithPartten(pattern);
                }

                _logger.LogDebug("所有拍卖品缓存已清除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除所有拍卖品缓存失败");
            }
        }

        #endregion
    }
}
