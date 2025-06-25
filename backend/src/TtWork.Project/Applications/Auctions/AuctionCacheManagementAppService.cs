using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Dependency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Definitions;
using TtWork.Project.Services.Cache;
using MediatR;
using TtWork.Project.Events;

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

        public AuctionCacheManagementAppService(
            IAuctionItemCacheService cacheService,
            IMediator mediator,
            ILogger<AuctionCacheManagementAppService> logger)
        {
            _cacheService = cacheService;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/AuctionCache/Stats")]
        public async Task<object> GetCacheStats()
        {
            _logger.LogInformation("管理员请求获取拍卖品缓存统计信息");
            return await _cacheService.GetCacheStatsAsync();
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