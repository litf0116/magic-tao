using System;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖缓存配置类
    /// </summary>
    public static class AuctionCacheConfiguration
    {
        /// <summary>
        /// 是否启用缓存（可通过环境变量或配置文件控制）
        /// </summary>
        public static bool IsCacheEnabled
        {
            get
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
            set
            {
                Environment.SetEnvironmentVariable("AUCTION_CACHE_ENABLED", value.ToString());
            }
        }

        /// <summary>
        /// 缓存预热开关
        /// </summary>
        public static bool IsWarmupEnabled
        {
            get
            {
                var envValue = Environment.GetEnvironmentVariable("AUCTION_CACHE_WARMUP_ENABLED");
                if (!string.IsNullOrEmpty(envValue))
                {
                    return bool.TryParse(envValue, out var result) && result;
                }
                return true; // 默认启用预热
            }
        }

        /// <summary>
        /// 缓存监控开关
        /// </summary>
        public static bool IsMonitoringEnabled
        {
            get
            {
                var envValue = Environment.GetEnvironmentVariable("AUCTION_CACHE_MONITORING_ENABLED");
                if (!string.IsNullOrEmpty(envValue))
                {
                    return bool.TryParse(envValue, out var result) && result;
                }
                return true; // 默认启用监控
            }
        }

        /// <summary>
        /// 获取缓存过期时间倍数（用于调试时快速过期）
        /// </summary>
        public static double CacheExpireMultiplier
        {
            get
            {
                var envValue = Environment.GetEnvironmentVariable("AUCTION_CACHE_EXPIRE_MULTIPLIER");
                if (!string.IsNullOrEmpty(envValue) && double.TryParse(envValue, out var result))
                {
                    return Math.Max(0.1, result); // 最小0.1倍
                }
                return 1.0; // 默认1.0倍
            }
        }
    }
}