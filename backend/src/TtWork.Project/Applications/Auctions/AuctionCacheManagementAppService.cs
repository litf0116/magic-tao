using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Definitions;
using TtWork.Abp.Applications;
using TtWork.Project.Services.Cache;
using TtWork.Project.Domains;
using MediatR;
using TtWork.Project.Events;
using TtWork.Abp.Applications.Dtos;
using Abp.UI;

namespace TtWork.Project.Applications.Auctions
{
    /// <summary>
    /// 拍卖品缓存管理应用服务
    /// </summary>
    [AbpAuthorize(AppPermissions.Pages.ChatManager)]
    public class AuctionCacheManagementAppService : ITransientDependency
    {
        private readonly IAuctionItemCacheService _cacheService;
        private readonly IMediator _mediator;
        private readonly ILogger<AuctionCacheManagementAppService> _logger;
        private readonly IRepository<AuctionItem, long> _auctionItemRepository;

        public AuctionCacheManagementAppService(
            IAuctionItemCacheService cacheService,
            IMediator mediator,
            ILogger<AuctionCacheManagementAppService> logger,
            IRepository<AuctionItem, long> auctionItemRepository)
        {
            _cacheService = cacheService;
            _mediator = mediator;
            _logger = logger;
            _auctionItemRepository = auctionItemRepository;
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/AuctionCache/Stats")]
        public async Task<object> GetCacheStats()
        {
            try
            {
                _logger.LogInformation("管理员请求获取拍卖品缓存统计信息");

                var cacheStats = await _cacheService.GetCacheStatsAsync();

                // 获取拍品统计信息
                var totalItems = await _auctionItemRepository.CountAsync();
                var activeItems = await _auctionItemRepository.CountAsync(x => x.Status == AuctionStatusEnum.拍卖中);
                var completedItems = await _auctionItemRepository.CountAsync(x => x.Status == AuctionStatusEnum.已成交);
                var listedItems = await _auctionItemRepository.CountAsync(x => x.Status == AuctionStatusEnum.上架);

                return new
                {
                    CacheStats = cacheStats,
                    AuctionStats = new
                    {
                        TotalItems = totalItems,
                        ActiveItems = activeItems,
                        CompletedItems = completedItems,
                        ListedItems = listedItems,
                        ActiveRate = totalItems > 0 ? (double)activeItems / totalItems * 100 : 0,
                        CompletedRate = totalItems > 0 ? (double)completedItems / totalItems * 100 : 0
                    },
                    CachePolicy = new
                    {
                        IsEnabled = AuctionItemCachePolicy.IsCacheEnabled(),
                        DefaultExpireMinutes = AuctionItemCachePolicy.DEFAULT_EXPIRE_MINUTES,
                        ShortExpireSeconds = AuctionItemCachePolicy.SHORT_EXPIRE_SECONDS,
                        LongExpireHours = AuctionItemCachePolicy.LONG_EXPIRE_HOURS
                    },
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存统计信息失败");
                throw new UserFriendlyException("获取缓存统计信息失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取缓存健康状态
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/AuctionCache/Health")]
        public async Task<object> GetCacheHealthStatus()
        {
            try
            {
                var stats = await _cacheService.GetCacheStatsAsync();

                // 健康检查逻辑
                var healthChecks = new List<object>();

                if (!AuctionItemCachePolicy.IsCacheEnabled())
                {
                    healthChecks.Add(new { Type = "Warning", Message = "缓存功能已禁用" });
                }

                // 检查拍品数量是否合理
                var totalItems = await _auctionItemRepository.CountAsync();
                if (totalItems == 0)
                {
                    healthChecks.Add(new { Type = "Info", Message = "系统中没有拍品数据" });
                }

                return new
                {
                    IsHealthy = healthChecks.All(x => !((dynamic)x).Type.ToString().Equals("Error")),
                    HealthChecks = healthChecks,
                    Recommendations = GetHealthRecommendations(),
                    LastCheckTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存健康状态失败");
                return new
                {
                    IsHealthy = false,
                    Error = ex.Message,
                    LastCheckTime = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 获取健康建议
        /// </summary>
        /// <returns></returns>
        private List<string> GetHealthRecommendations()
        {
            var recommendations = new List<string>();

            if (!AuctionItemCachePolicy.IsCacheEnabled())
            {
                recommendations.Add("建议启用缓存以提升系统性能");
            }

            recommendations.Add("定期监控缓存命中率");
            recommendations.Add("在高访问时段前预热缓存");
            recommendations.Add("监控Redis内存使用情况");
            recommendations.Add("关注缓存失效策略的准确性");

            return recommendations;
        }

        /// <summary>
        /// 清除所有拍卖品缓存
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/AuctionCache/ClearAll")]
        public async Task<object> ClearAllCache()
        {
            _logger.LogInformation("管理员请求清除所有拍卖品缓存");
            
            await _cacheService.ClearAuctionCacheAsync();
            
            // 发布缓存清除事件
            await _mediator.Publish(new CacheClearRequestedEvent());
            
            return new { Success = true, Message = "所有拍卖品缓存已清除" };
        }

        /// <summary>
        /// 清除指定拍卖品缓存
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID</param>
        /// <returns></returns>
        [HttpPost("api/AuctionCache/ClearItem/{auctionItemId}")]
        public async Task<object> ClearItemCache(long auctionItemId)
        {
            _logger.LogInformation("管理员请求清除拍卖品缓存，ID: {AuctionItemId}", auctionItemId);
            
            await _cacheService.ClearAuctionCacheAsync(auctionItemId);
            
            // 发布缓存清除事件
            await _mediator.Publish(new CacheClearRequestedEvent(auctionItemId));
            
            return new { Success = true, Message = $"拍卖品{auctionItemId}缓存已清除" };
        }

        /// <summary>
        /// 清除拍卖品列表缓存
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/AuctionCache/ClearList")]
        public async Task<object> ClearListCache()
        {
            _logger.LogInformation("管理员请求清除拍卖品列表缓存");
            
            await _cacheService.ClearAuctionListCacheAsync();
            
            return new { Success = true, Message = "拍卖品列表缓存已清除" };
        }

        /// <summary>
        /// 预热拍卖品缓存
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/AuctionCache/Warmup")]
        public async Task<object> WarmupCache()
        {
            _logger.LogInformation("管理员请求预热拍卖品缓存");
            
            await _cacheService.WarmupCacheAsync();
            
            // 发布缓存预热事件
            await _mediator.Publish(new CacheWarmupRequestedEvent());
            
            return new { Success = true, Message = "拍卖品缓存预热已完成" };
        }

        /// <summary>
        /// 重建所有拍卖品缓存
        /// </summary>
        /// <returns></returns>
        [HttpPost("api/AuctionCache/Rebuild")]
        public async Task<object> RebuildCache()
        {
            _logger.LogInformation("管理员请求重建所有拍卖品缓存");
            
            await _cacheService.RebuildAllCacheAsync();
            
            return new { Success = true, Message = "所有拍卖品缓存重建已完成" };
        }
    }
} 