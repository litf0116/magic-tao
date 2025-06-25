using System;
using System.Threading;
using System.Threading.Tasks;
using Abp.Dependency;
using MediatR;
using Microsoft.Extensions.Logging;
using TtWork.Project.Events;
using TtWork.Project.Services.Cache;
using TtWork.Project.Domains;

namespace TtWork.Project.EventHandlers
{
    /// <summary>
    /// 拍卖品缓存事件处理器
    /// </summary>
    public class AuctionItemCacheEventHandler : ITransientDependency,
        INotificationHandler<AuctionItemCreatedEvent>,
        INotificationHandler<AuctionItemUpdatedEvent>,
        INotificationHandler<AuctionItemDeletedEvent>,
        INotificationHandler<AuctionStartedEvent>,
        INotificationHandler<AuctionEndedEvent>,
        INotificationHandler<BidPlacedEvent>,
        INotificationHandler<KasecStatusChangedEvent>,
        INotificationHandler<AuctionItemStatusChangedEvent>,
        INotificationHandler<CacheWarmupRequestedEvent>,
        INotificationHandler<CacheClearRequestedEvent>,
        INotificationHandler<BatchCacheUpdateEvent>
    {
        private readonly IAuctionItemCacheService _cacheService;
        private readonly ILogger<AuctionItemCacheEventHandler> _logger;

        public AuctionItemCacheEventHandler(
            IAuctionItemCacheService cacheService,
            ILogger<AuctionItemCacheEventHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// 处理拍卖品创建事件
        /// </summary>
        public async Task Handle(AuctionItemCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖品创建事件，ID: {AuctionItemId}", notification.AuctionItemId);

                // 设置详情缓存
                await _cacheService.SetAuctionDetailCacheAsync(notification.AuctionItem);

                // 清除列表缓存，因为新增了商品
                await _cacheService.ClearAuctionListCacheAsync();

                // 如果是上架状态的商品，可能需要清除当前拍卖缓存
                if (notification.Status == AuctionStatusEnum.上架)
                {
                    await _cacheService.ClearCurrentAuctionCacheAsync();
                }

                _logger.LogInformation("拍卖品创建事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖品创建事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理拍卖品更新事件
        /// </summary>
        public async Task Handle(AuctionItemUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖品更新事件，ID: {AuctionItemId}", notification.AuctionItemId);

                // 清除详情缓存
                await _cacheService.ClearAuctionDetailCacheAsync(notification.AuctionItemId);

                // 重新设置详情缓存
                await _cacheService.SetAuctionDetailCacheAsync(notification.AuctionItem);

                // 如果状态发生变化，需要清除相关列表缓存
                if (notification.OldStatus.HasValue && notification.OldStatus != notification.Status)
                {
                    await _cacheService.ClearAuctionListCacheAsync(notification.OldStatus);
                    if (notification.Status.HasValue)
                    {
                        await _cacheService.ClearAuctionListCacheAsync(notification.Status.Value);
                    }
                }
                else
                {
                    // 状态未变化，只清除当前状态的列表缓存
                    if (notification.Status.HasValue)
                    {
                        await _cacheService.ClearAuctionListCacheAsync(notification.Status.Value);
                    }
                }

                _logger.LogInformation("拍卖品更新事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖品更新事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理拍卖品删除事件
        /// </summary>
        public async Task Handle(AuctionItemDeletedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖品删除事件，ID: {AuctionItemId}", notification.AuctionItemId);

                // 清除详情缓存
                await _cacheService.ClearAuctionDetailCacheAsync(notification.AuctionItemId);

                // 清除列表缓存
                if (notification.Status.HasValue)
                {
                    await _cacheService.ClearAuctionListCacheAsync(notification.Status.Value);
                }
                else
                {
                    await _cacheService.ClearAuctionListCacheAsync();
                }

                // 清除当前拍卖缓存（如果删除的是拍卖中的商品）
                if (notification.Status == Domains.AuctionStatusEnum.拍卖中)
                {
                    await _cacheService.ClearCurrentAuctionCacheAsync();
                }

                _logger.LogInformation("拍卖品删除事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖品删除事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理拍卖开始事件
        /// </summary>
        public async Task Handle(AuctionStartedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖开始事件，ID: {AuctionItemId}", notification.AuctionItemId);

                // 更新详情缓存
                await _cacheService.SetAuctionDetailCacheAsync(notification.AuctionItem);

                // 清除相关列表缓存
                await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.上架);
                await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.拍卖中);

                // 清除当前拍卖缓存，因为当前拍卖商品可能发生变化
                await _cacheService.ClearCurrentAuctionCacheAsync();

                _logger.LogInformation("拍卖开始事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖开始事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理拍卖结束事件
        /// </summary>
        public async Task Handle(AuctionEndedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖结束事件，ID: {AuctionItemId}, 有出价: {HasBids}", 
                    notification.AuctionItemId, notification.HasBids);

                // 更新详情缓存
                await _cacheService.SetAuctionDetailCacheAsync(notification.AuctionItem);

                // 清除相关列表缓存
                await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.拍卖中);
                
                if (notification.HasBids)
                {
                    // 有出价，清除已成交列表缓存
                    await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.已成交);
                }
                else
                {
                    // 无出价流拍，清除上架列表缓存
                    await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.上架);
                }

                // 清除当前拍卖缓存
                await _cacheService.ClearCurrentAuctionCacheAsync();

                _logger.LogInformation("拍卖结束事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖结束事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理出价事件
        /// </summary>
        public async Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理出价事件，拍卖品ID: {AuctionItemId}, 出价: {BidPrice}, 用户: {BidUserName}", 
                    notification.AuctionItemId, notification.BidPrice, notification.BidUserName);

                // 清除拍卖品详情缓存，因为当前价格和出价用户发生了变化
                await _cacheService.ClearAuctionDetailCacheAsync(notification.AuctionItemId);

                // 清除拍卖中商品列表缓存
                await _cacheService.ClearAuctionListCacheAsync(Domains.AuctionStatusEnum.拍卖中);

                // 清除当前拍卖缓存
                await _cacheService.ClearCurrentAuctionCacheAsync();

                _logger.LogDebug("出价事件处理完成，拍卖品ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理出价事件失败，拍卖品ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理卡秒状态变更事件
        /// </summary>
        public async Task Handle(KasecStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理卡秒状态变更事件，ID: {AuctionItemId}, 卡秒状态: {IsKasec}", 
                    notification.AuctionItemId, notification.IsKasec);

                // 清除拍卖品详情缓存，因为卡秒状态发生了变化
                await _cacheService.ClearAuctionDetailCacheAsync(notification.AuctionItemId);

                // 清除当前拍卖缓存
                await _cacheService.ClearCurrentAuctionCacheAsync();

                _logger.LogDebug("卡秒状态变更事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理卡秒状态变更事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理拍卖品状态变更事件
        /// </summary>
        public async Task Handle(AuctionItemStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("处理拍卖品状态变更事件，ID: {AuctionItemId}, 状态: {OldStatus} -> {NewStatus}", 
                    notification.AuctionItemId, notification.OldStatus, notification.NewStatus);

                // 更新详情缓存
                await _cacheService.SetAuctionDetailCacheAsync(notification.AuctionItem);

                // 清除相关状态的列表缓存
                await _cacheService.ClearAuctionListCacheAsync(notification.OldStatus);
                await _cacheService.ClearAuctionListCacheAsync(notification.NewStatus);

                // 如果涉及拍卖中状态，清除当前拍卖缓存
                if (notification.OldStatus == Domains.AuctionStatusEnum.拍卖中 || 
                    notification.NewStatus == Domains.AuctionStatusEnum.拍卖中)
                {
                    await _cacheService.ClearCurrentAuctionCacheAsync();
                }

                _logger.LogInformation("拍卖品状态变更事件处理完成，ID: {AuctionItemId}", notification.AuctionItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理拍卖品状态变更事件失败，ID: {AuctionItemId}", notification.AuctionItemId);
            }
        }

        /// <summary>
        /// 处理缓存预热请求事件
        /// </summary>
        public async Task Handle(CacheWarmupRequestedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("处理缓存预热请求事件，缓存类型: {CacheTypes}", string.Join(", ", notification.CacheTypes));

                // 执行缓存预热
                await _cacheService.WarmupCacheAsync();

                _logger.LogInformation("缓存预热请求事件处理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理缓存预热请求事件失败");
            }
        }

        /// <summary>
        /// 处理缓存清除请求事件
        /// </summary>
        public async Task Handle(CacheClearRequestedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("处理缓存清除请求事件，拍卖品ID: {AuctionItemId}, 状态: {Status}, 缓存类型: {CacheTypes}", 
                    notification.AuctionItemId?.ToString() ?? "ALL", 
                    notification.Status?.ToString() ?? "ALL", 
                    string.Join(", ", notification.CacheTypes));

                // 执行缓存清除
                if (notification.AuctionItemId.HasValue)
                {
                    await _cacheService.ClearAuctionCacheAsync(notification.AuctionItemId.Value);
                }
                else if (notification.Status.HasValue)
                {
                    await _cacheService.ClearAuctionListCacheAsync(notification.Status.Value);
                }
                else
                {
                    await _cacheService.ClearAuctionCacheAsync();
                }

                _logger.LogInformation("缓存清除请求事件处理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理缓存清除请求事件失败");
            }
        }

        /// <summary>
        /// 处理批量缓存更新事件
        /// </summary>
        public async Task Handle(BatchCacheUpdateEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("处理批量缓存更新事件，拍卖品数量: {Count}, 操作: {Operation}", 
                    notification.AuctionItemIds?.Length ?? 0, notification.Operation);

                if (notification.AuctionItemIds?.Length > 0)
                {
                    foreach (var auctionItemId in notification.AuctionItemIds)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            if (notification.Operation == "clear")
                            {
                                await _cacheService.ClearAuctionDetailCacheAsync(auctionItemId);
                            }
                            else
                            {
                                // 其他操作类型可以在这里扩展
                                await _cacheService.ClearAuctionDetailCacheAsync(auctionItemId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "批量处理拍卖品缓存失败，ID: {AuctionItemId}", auctionItemId);
                        }
                    }

                    // 批量操作后，清除列表缓存
                    await _cacheService.ClearAuctionListCacheAsync();
                }

                _logger.LogInformation("批量缓存更新事件处理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理批量缓存更新事件失败");
            }
        }
    }
} 