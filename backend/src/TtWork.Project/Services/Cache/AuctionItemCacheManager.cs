using System;
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
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TtWork.Abp.Applications.Dtos;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存管理服务
    /// 使用 HybridCache：本地内存缓存 + Redis 分布式缓存 + Tag 失效机制
    /// </summary>
    public class AuctionItemCacheManager : IAuctionItemCacheService
    {
        private readonly IRedisClient _redisClient;
        private readonly IRepository<AuctionItem, long> _auctionItemRepository;
        private readonly IRepository<BidHistory, long> _bidHistoryRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ILogger<AuctionItemCacheManager> _logger;
        private readonly HybridCache _cache;

        // 缓存标签常量
        private const string TAG_DETAIL = "auction:detail";
        private const string TAG_LIST = "auction:list";
        private const string TAG_MID_LIST = "auction:mid";
        private const string TAG_CURRENT = "auction:current";

        public AuctionItemCacheManager(
            IRedisClient redisClient,
            IRepository<AuctionItem, long> auctionItemRepository,
            IRepository<BidHistory, long> bidHistoryRepository,
            IObjectMapper objectMapper,
            ILogger<AuctionItemCacheManager> logger,
            HybridCache cache)
        {
            _redisClient = redisClient;
            _auctionItemRepository = auctionItemRepository;
            _bidHistoryRepository = bidHistoryRepository;
            _objectMapper = objectMapper;
            _logger = logger;
            _cache = cache;
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

                // 使用 HybridCache 自动处理 L1/L2 缓存，支持 Tag 失效
                var cacheOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(AuctionItemCachePolicy.GetListCacheExpire(input.Status).TotalSeconds),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)
                };

                var tags = new List<string> { TAG_LIST };
                if (input.Status.HasValue)
                {
                    tags.Add($"{TAG_LIST}:status:{input.Status.Value}");
                }

                return await _cache.GetOrCreateAsync(
                    cacheKey,
                    async cancel => await GetAuctionListFromDatabaseAsync(input),
                    cacheOptions,
                    tags,
                    CancellationToken.None
                );
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

                // 使用 HybridCache 自动处理 L1/L2 缓存，支持 Tag 失效
                var cacheOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(60),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)
                };

                return await _cache.GetOrCreateAsync(
                    cacheKey,
                    async cancel => await GetAuctionDetailFromDatabaseAsync(auctionItemId),
                    cacheOptions,
                    new[] { TAG_DETAIL, $"{TAG_DETAIL}:{auctionItemId}" },
                    CancellationToken.None
                );
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

                // 使用 HybridCache 自动处理 L1/L2 缓存，支持 Tag 失效
                var cacheOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(30),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)
                };

                return await _cache.GetOrCreateAsync(
                    cacheKey,
                    async cancel => await GetCurrentAuctionItemFromDatabaseAsync(),
                    cacheOptions,
                    new[] { TAG_CURRENT },
                    CancellationToken.None
                );
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

                // 使用 HybridCache 自动处理 L1/L2 缓存，支持 Tag 失效
                var cacheOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(30),
                    LocalCacheExpiration = TimeSpan.FromSeconds(10)
                };

                return await _cache.GetOrCreateAsync(
                    cacheKey,
                    async cancel => await GetAuctionMidListFromDatabaseAsync(input),
                    cacheOptions,
                    new[] { TAG_MID_LIST },
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取拍卖中商品列表缓存失败，降级到数据库查询");
                return await GetAuctionMidListFromDatabaseAsync(input);
            }
        }
        /// <summary>
        /// 设置拍卖品详情缓存
        /// 注意：HybridCache 自动处理缓存，此方法保留兼容但不再手动写入
        /// </summary>
        public async Task SetAuctionDetailCacheAsync(AuctionItemDto auctionItem)
        {
            // HybridCache 的 GetOrCreateAsync 会自动处理缓存填充
            // 手动写入会绕过标签系统，导致缓存失效不一致
            // 此方法保留为空操作以保持接口兼容
            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置拍卖品列表缓存
        /// 注意：HybridCache 自动处理缓存，此方法保留兼容但不再手动写入
        /// </summary>
        public async Task SetAuctionListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result)
        {
            // HybridCache 的 GetOrCreateAsync 会自动处理缓存填充
            // 手动写入会绕过标签系统，导致缓存失效不一致
            // 此方法保留为空操作以保持接口兼容
            await Task.CompletedTask;
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
                // 使用 HybridCache 的 Tag 失效机制清除所有列表缓存
                await _cache.RemoveByTagAsync(TAG_LIST);

                // 清除拍卖中商品列表缓存
                await _cache.RemoveByTagAsync(TAG_MID_LIST);

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
                // 使用 Tag 失效机制清除详情缓存
                await _cache.RemoveByTagAsync($"{TAG_DETAIL}:{auctionItemId}");

                _logger.LogDebug("拍卖品详情缓存已清除，ID: {AuctionItemId}", auctionItemId);
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
                // 使用 Tag 失效机制清除当前拍卖缓存
                await _cache.RemoveByTagAsync(TAG_CURRENT);

                _logger.LogDebug("当前拍卖商品缓存已清除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除当前拍卖商品缓存失败");
            }
        }

        public async Task ClearAllAuctionCacheAsync()
        {
            try
            {
                // 清除所有拍卖品相关缓存
                await _cache.RemoveByTagAsync(TAG_LIST);
                await _cache.RemoveByTagAsync(TAG_MID_LIST);
                await _cache.RemoveByTagAsync(TAG_DETAIL);
                await _cache.RemoveByTagAsync(TAG_CURRENT);

                _logger.LogDebug("所有拍卖品缓存已清除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除所有拍卖品缓存失败");
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

        #endregion
    }
} 