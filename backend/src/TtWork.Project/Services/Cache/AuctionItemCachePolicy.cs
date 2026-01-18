using System;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存策略配置
    /// </summary>
    public static class AuctionItemCachePolicy
    {
        /// <summary>
        /// 默认缓存过期时间（分钟）
        /// </summary>
        public const int DEFAULT_EXPIRE_MINUTES = 10;

        /// <summary>
        /// 短期缓存过期时间（秒）
        /// </summary>
        public const int SHORT_EXPIRE_SECONDS = 30;

        /// <summary>
        /// 长期缓存过期时间（小时）
        /// </summary>
        public const int LONG_EXPIRE_HOURS = 2;

        /// <summary>
        /// 根据拍卖品状态获取详情缓存过期时间
        /// </summary>
        /// <param name="status">拍卖品状态</param>
        /// <returns>过期时间</returns>
        public static TimeSpan GetDetailCacheExpire(AuctionStatusEnum status)
        {
            return status switch
            {
                AuctionStatusEnum.拍卖中 => TimeSpan.FromSeconds(30), // 拍卖中变化频繁，30秒缓存
                AuctionStatusEnum.上架 => TimeSpan.FromMinutes(5), // 待拍卖相对稳定，5分钟缓存
                AuctionStatusEnum.已成交 => TimeSpan.FromMinutes(30), // 已成交基本不变，30分钟缓存
                AuctionStatusEnum.交易成功 => TimeSpan.FromHours(1), // 交易完成，1小时缓存
                AuctionStatusEnum.卖家失约 => TimeSpan.FromHours(1), // 失约状态，1小时缓存
                AuctionStatusEnum.买家失约 => TimeSpan.FromHours(1), // 失约状态，1小时缓存
                AuctionStatusEnum.交易关闭 => TimeSpan.FromHours(2), // 关闭状态，2小时缓存
                _ => TimeSpan.FromMinutes(DEFAULT_EXPIRE_MINUTES) // 其他状态，默认10分钟
            };
        }

        /// <summary>
        /// 根据查询状态获取列表缓存过期时间
        /// </summary>
        /// <param name="status">查询状态</param>
        /// <returns>过期时间</returns>
        public static TimeSpan GetListCacheExpire(int? status)
        {
            if (!status.HasValue)
            {
                // 混合状态列表（待拍卖+拍卖中），缓存1分钟
                return TimeSpan.FromMinutes(1);
            }

            return status.Value switch
            {
                (int)AuctionStatusEnum.拍卖中 => TimeSpan.FromSeconds(30), // 拍卖中列表，30秒缓存
                (int)AuctionStatusEnum.上架 => TimeSpan.FromMinutes(5), // 待拍卖列表，5分钟缓存
                (int)AuctionStatusEnum.已成交 => TimeSpan.FromMinutes(15), // 已成交列表，15分钟缓存（相对稳定）
                (int)AuctionStatusEnum.交易成功 => TimeSpan.FromHours(1), // 交易成功，1小时缓存
                (int)AuctionStatusEnum.卖家失约 => TimeSpan.FromHours(2), // 失约状态，2小时缓存
                (int)AuctionStatusEnum.买家失约 => TimeSpan.FromHours(2), // 失约状态，2小时缓存
                (int)AuctionStatusEnum.交易关闭 => TimeSpan.FromHours(3), // 关闭状态，3小时缓存
                _ => TimeSpan.FromMinutes(DEFAULT_EXPIRE_MINUTES) // 其他状态，默认10分钟
            };
        }

        /// <summary>
        /// 获取当前拍卖商品缓存过期时间
        /// </summary>
        /// <returns>过期时间</returns>
        public static TimeSpan GetCurrentAuctionCacheExpire()
        {
            return TimeSpan.FromSeconds(SHORT_EXPIRE_SECONDS); // 30秒缓存，变化频繁
        }

        /// <summary>
        /// 获取拍卖中商品列表缓存过期时间
        /// </summary>
        /// <returns>过期时间</returns>
        public static TimeSpan GetMidListCacheExpire()
        {
            return TimeSpan.FromSeconds(SHORT_EXPIRE_SECONDS); // 30秒缓存，变化频繁
        }

        /// <summary>
        /// 获取卡秒状态缓存过期时间
        /// </summary>
        /// <returns>过期时间</returns>
        public static TimeSpan GetKasecCacheExpire()
        {
            return TimeSpan.FromMinutes(30); // 卡秒状态相对稳定，30分钟缓存
        }

        /// <summary>
        /// 获取统计数据缓存过期时间
        /// </summary>
        /// <param name="statsType">统计类型</param>
        /// <returns>过期时间</returns>
        public static TimeSpan GetStatsCacheExpire(string statsType)
        {
            return statsType switch
            {
                "daily" => TimeSpan.FromHours(1), // 日统计，1小时缓存
                "monthly" => TimeSpan.FromHours(6), // 月统计，6小时缓存
                "yearly" => TimeSpan.FromHours(24), // 年统计，24小时缓存
                _ => TimeSpan.FromHours(LONG_EXPIRE_HOURS) // 默认2小时缓存
            };
        }

        /// <summary>
        /// 获取空结果缓存过期时间（避免缓存穿透）
        /// </summary>
        /// <returns>过期时间</returns>
        public static TimeSpan GetNullResultCacheExpire()
        {
            return TimeSpan.FromSeconds(SHORT_EXPIRE_SECONDS); // 空结果缓存30秒
        }

        /// <summary>
        /// 是否启用缓存（可根据系统负载动态调整）
        /// </summary>
        /// <returns>是否启用</returns>
        public static bool IsCacheEnabled()
        {
            // 检查环境变量
            var envValue = Environment.GetEnvironmentVariable("AUCTION_CACHE_ENABLED");
            if (!string.IsNullOrEmpty(envValue))
            {
                return bool.TryParse(envValue, out var result) && result;
            }

            // 默认启用缓存
            return true;
        }

        /// <summary>
        /// 获取缓存预热的数据量限制
        /// </summary>
        /// <returns>数据量限制</returns>
        public static int GetWarmupDataLimit()
        {
            return 100; // 预热时最多缓存100条数据
        }

        /// <summary>
        /// 获取批量操作的分页大小
        /// </summary>
        /// <returns>分页大小</returns>
        public static int GetBatchSize()
        {
            return 50; // 批量操作每批50条
        }

        /// <summary>
        /// 获取带随机偏移的列表缓存过期时间（防止缓存雪崩）
        /// </summary>
        /// <param name="status">查询状态</param>
        /// <returns>带随机偏移的过期时间</returns>
        public static TimeSpan GetListCacheExpireWithJitter(int? status)
        {
            var baseExpire = GetListCacheExpire(status);
            // 添加 0-5 秒的随机偏移，防止大量缓存同时失效
            var jitterSeconds = Random.Shared.Next(0, 5);
            return baseExpire.Add(TimeSpan.FromSeconds(jitterSeconds));
        }
    }
}