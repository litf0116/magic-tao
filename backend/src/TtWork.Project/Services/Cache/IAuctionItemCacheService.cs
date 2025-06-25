using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using TtWork.Abp.Applications.Dtos;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存服务接口
    /// </summary>
    public interface IAuctionItemCacheService
    {
        /// <summary>
        /// 获取拍卖品列表（带缓存）
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>拍卖品列表</returns>
        Task<ListResultDto<AuctionItemDto>> GetAuctionListAsync(AppResultRequestDto input);

        /// <summary>
        /// 获取拍卖品详情（带缓存）
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID</param>
        /// <returns>拍卖品详情</returns>
        Task<AuctionItemDto> GetAuctionDetailAsync(long auctionItemId);

        /// <summary>
        /// 获取当前拍卖中的商品（带缓存）
        /// </summary>
        /// <returns>当前拍卖中的商品</returns>
        Task<AuctionItemDto> GetCurrentAuctionItemAsync();

        /// <summary>
        /// 获取拍卖中商品列表（带缓存）
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>拍卖中商品列表</returns>
        Task<ListResultDto<AuctionItemDto>> GetAuctionMidListAsync(AppResultRequestDto input);

        /// <summary>
        /// 设置拍卖品详情缓存
        /// </summary>
        /// <param name="auctionItem">拍卖品详情</param>
        /// <returns></returns>
        Task SetAuctionDetailCacheAsync(AuctionItemDto auctionItem);

        /// <summary>
        /// 设置拍卖品列表缓存
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <param name="result">列表结果</param>
        /// <returns></returns>
        Task SetAuctionListCacheAsync(AppResultRequestDto input, ListResultDto<AuctionItemDto> result);

        /// <summary>
        /// 清除拍卖品相关缓存
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID，null表示清除所有</param>
        /// <returns></returns>
        Task ClearAuctionCacheAsync(long? auctionItemId = null);

        /// <summary>
        /// 清除拍卖品列表缓存
        /// </summary>
        /// <param name="status">状态过滤</param>
        /// <returns></returns>
        Task ClearAuctionListCacheAsync(AuctionStatusEnum? status = null);

        /// <summary>
        /// 清除拍卖品详情缓存
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID</param>
        /// <returns></returns>
        Task ClearAuctionDetailCacheAsync(long auctionItemId);

        /// <summary>
        /// 清除当前拍卖商品缓存
        /// </summary>
        /// <returns></returns>
        Task ClearCurrentAuctionCacheAsync();

        /// <summary>
        /// 预热缓存
        /// </summary>
        /// <returns></returns>
        Task WarmupCacheAsync();

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns></returns>
        Task<object> GetCacheStatsAsync();

        /// <summary>
        /// 重建所有拍卖品缓存
        /// </summary>
        /// <returns></returns>
        Task RebuildAllCacheAsync();
    }
} 