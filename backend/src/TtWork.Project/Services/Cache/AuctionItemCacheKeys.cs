using TtWork.Abp.Applications.Dtos;
using TtWork.Project.Domains;

namespace TtWork.Project.Services.Cache
{
    /// <summary>
    /// 拍卖品缓存键管理类
    /// </summary>
    public static class AuctionItemCacheKeys
    {
        /// <summary>
        /// 缓存键前缀
        /// </summary>
        public const string PREFIX = "auction";

        /// <summary>
        /// 拍卖品列表缓存键前缀
        /// </summary>
        public const string LIST_PREFIX = PREFIX + ":list";

        /// <summary>
        /// 拍卖品详情缓存键前缀
        /// </summary>
        public const string DETAIL_PREFIX = PREFIX + ":detail";

        /// <summary>
        /// 当前拍卖商品缓存键
        /// </summary>
        public const string CURRENT_AUCTION = PREFIX + ":current";

        /// <summary>
        /// 拍卖中商品列表缓存键
        /// </summary>
        public const string MID_LIST = PREFIX + ":mid";

        /// <summary>
        /// 卡秒状态缓存键前缀
        /// </summary>
        public const string KASEC_PREFIX = PREFIX + ":kasec";

        /// <summary>
        /// 拍卖统计缓存键前缀
        /// </summary>
        public const string STATS_PREFIX = PREFIX + ":stats";

        /// <summary>
        /// 生成拍卖品列表缓存键
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>缓存键</returns>
        public static string GenerateListCacheKey(AppResultRequestDto input)
        {
            string statusKey = input.Status?.ToString() ?? "default";
            string sortingKey = !string.IsNullOrEmpty(input.Sorting) ? $":sort_{input.Sorting.Replace(" ", "_")}" : "";
            string keywordKey = !string.IsNullOrEmpty(input.Keyword) ? $":kw_{input.Keyword.GetHashCode()}" : "";
            
            return $"{LIST_PREFIX}:{statusKey}:{input.MaxResultCount}{sortingKey}{keywordKey}";
        }

        /// <summary>
        /// 生成拍卖品详情缓存键
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID</param>
        /// <returns>缓存键</returns>
        public static string GenerateDetailCacheKey(long auctionItemId)
        {
            return $"{DETAIL_PREFIX}:{auctionItemId}";
        }

        /// <summary>
        /// 生成卡秒状态缓存键
        /// </summary>
        /// <param name="auctionItemId">拍卖品ID</param>
        /// <returns>缓存键</returns>
        public static string GenerateKasecCacheKey(long auctionItemId)
        {
            return $"{KASEC_PREFIX}:{auctionItemId}";
        }

        /// <summary>
        /// 生成拍卖中商品列表缓存键
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>缓存键</returns>
        public static string GenerateMidListCacheKey(AppResultRequestDto input)
        {
            string statusKey = input.Status?.ToString() ?? "mid";
            return $"{MID_LIST}:{statusKey}:{input.MaxResultCount}";
        }

        /// <summary>
        /// 生成统计数据缓存键
        /// </summary>
        /// <param name="statsType">统计类型</param>
        /// <returns>缓存键</returns>
        public static string GenerateStatsCacheKey(string statsType)
        {
            return $"{STATS_PREFIX}:{statsType}";
        }

        /// <summary>
        /// 获取拍卖品列表相关的所有缓存键模式
        /// </summary>
        /// <param name="status">状态过滤</param>
        /// <returns>缓存键模式数组</returns>
        public static string[] GetListCachePatterns(AuctionStatusEnum? status = null)
        {
            if (status.HasValue)
            {
                return new[] { $"{LIST_PREFIX}:{(int)status.Value}:*" };
            }

            return new[]
            {
                $"{LIST_PREFIX}:default:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.上架}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.拍卖中}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.已成交}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.交易成功}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.卖家失约}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.买家失约}:*",
                $"{LIST_PREFIX}:{(int)AuctionStatusEnum.交易关闭}:*"
            };
        }

        /// <summary>
        /// 获取拍卖品详情相关的所有缓存键模式
        /// </summary>
        /// <returns>缓存键模式</returns>
        public static string GetDetailCachePattern()
        {
            return $"{DETAIL_PREFIX}:*";
        }

        /// <summary>
        /// 获取拍卖中商品列表相关的所有缓存键模式
        /// </summary>
        /// <returns>缓存键模式数组</returns>
        public static string[] GetMidListCachePatterns()
        {
            return new[]
            {
                $"{MID_LIST}:*"
            };
        }

        /// <summary>
        /// 获取所有拍卖品相关的缓存键模式
        /// </summary>
        /// <returns>缓存键模式数组</returns>
        public static string[] GetAllCachePatterns()
        {
            return new[]
            {
                $"{LIST_PREFIX}:*",
                $"{DETAIL_PREFIX}:*",
                $"{MID_LIST}:*",
                $"{CURRENT_AUCTION}",
                $"{KASEC_PREFIX}:*",
                $"{STATS_PREFIX}:*"
            };
        }
    }
} 