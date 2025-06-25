using MediatR;
using TtWork.Project.Domains;

namespace TtWork.Project.Events
{
    /// <summary>
    /// 拍卖品缓存事件基类
    /// </summary>
    public abstract class AuctionItemCacheEventBase : INotification
    {
        public long AuctionItemId { get; }
        public AuctionStatusEnum? Status { get; }

        protected AuctionItemCacheEventBase(long auctionItemId, AuctionStatusEnum? status = null)
        {
            AuctionItemId = auctionItemId;
            Status = status;
        }
    }

    /// <summary>
    /// 拍卖品创建事件
    /// </summary>
    public class AuctionItemCreatedEvent : AuctionItemCacheEventBase
    {
        public AuctionItemDto AuctionItem { get; }

        public AuctionItemCreatedEvent(AuctionItemDto auctionItem) 
            : base(auctionItem.Id, auctionItem.Status)
        {
            AuctionItem = auctionItem;
        }
    }

    /// <summary>
    /// 拍卖品更新事件
    /// </summary>
    public class AuctionItemUpdatedEvent : AuctionItemCacheEventBase
    {
        public AuctionItemDto AuctionItem { get; }
        public AuctionStatusEnum? OldStatus { get; }

        public AuctionItemUpdatedEvent(AuctionItemDto auctionItem, AuctionStatusEnum? oldStatus = null) 
            : base(auctionItem.Id, auctionItem.Status)
        {
            AuctionItem = auctionItem;
            OldStatus = oldStatus;
        }
    }

    /// <summary>
    /// 拍卖品删除事件
    /// </summary>
    public class AuctionItemDeletedEvent : AuctionItemCacheEventBase
    {
        public AuctionItemDeletedEvent(long auctionItemId, AuctionStatusEnum? status = null) 
            : base(auctionItemId, status)
        {
        }
    }

    /// <summary>
    /// 拍卖开始事件
    /// </summary>
    public class AuctionStartedEvent : AuctionItemCacheEventBase
    {
        public AuctionItemDto AuctionItem { get; }

        public AuctionStartedEvent(AuctionItemDto auctionItem) 
            : base(auctionItem.Id, auctionItem.Status)
        {
            AuctionItem = auctionItem;
        }
    }

    /// <summary>
    /// 拍卖结束事件
    /// </summary>
    public class AuctionEndedEvent : AuctionItemCacheEventBase
    {
        public AuctionItemDto AuctionItem { get; }
        public bool HasBids { get; }

        public AuctionEndedEvent(AuctionItemDto auctionItem, bool hasBids) 
            : base(auctionItem.Id, auctionItem.Status)
        {
            AuctionItem = auctionItem;
            HasBids = hasBids;
        }
    }

    /// <summary>
    /// 出价事件
    /// </summary>
    public class BidPlacedEvent : AuctionItemCacheEventBase
    {
        public long BidUserId { get; }
        public int BidPrice { get; }
        public string BidUserName { get; }

        public BidPlacedEvent(long auctionItemId, long bidUserId, int bidPrice, string bidUserName) 
            : base(auctionItemId, AuctionStatusEnum.拍卖中)
        {
            BidUserId = bidUserId;
            BidPrice = bidPrice;
            BidUserName = bidUserName;
        }
    }

    /// <summary>
    /// 卡秒状态变更事件
    /// </summary>
    public class KasecStatusChangedEvent : AuctionItemCacheEventBase
    {
        public bool IsKasec { get; }

        public KasecStatusChangedEvent(long auctionItemId, bool isKasec) 
            : base(auctionItemId, AuctionStatusEnum.拍卖中)
        {
            IsKasec = isKasec;
        }
    }

    /// <summary>
    /// 拍卖品状态变更事件
    /// </summary>
    public class AuctionItemStatusChangedEvent : AuctionItemCacheEventBase
    {
        public AuctionStatusEnum OldStatus { get; }
        public AuctionStatusEnum NewStatus { get; }
        public AuctionItemDto AuctionItem { get; }

        public AuctionItemStatusChangedEvent(AuctionItemDto auctionItem, AuctionStatusEnum oldStatus) 
            : base(auctionItem.Id, auctionItem.Status)
        {
            AuctionItem = auctionItem;
            OldStatus = oldStatus;
            NewStatus = auctionItem.Status;
        }
    }

    /// <summary>
    /// 缓存预热事件
    /// </summary>
    public class CacheWarmupRequestedEvent : INotification
    {
        public string[] CacheTypes { get; }

        public CacheWarmupRequestedEvent(params string[] cacheTypes)
        {
            CacheTypes = cacheTypes ?? new[] { "all" };
        }
    }

    /// <summary>
    /// 缓存清除事件
    /// </summary>
    public class CacheClearRequestedEvent : INotification
    {
        public long? AuctionItemId { get; }
        public AuctionStatusEnum? Status { get; }
        public string[] CacheTypes { get; }

        public CacheClearRequestedEvent(long? auctionItemId = null, AuctionStatusEnum? status = null, params string[] cacheTypes)
        {
            AuctionItemId = auctionItemId;
            Status = status;
            CacheTypes = cacheTypes ?? new[] { "all" };
        }
    }

    /// <summary>
    /// 批量缓存更新事件
    /// </summary>
    public class BatchCacheUpdateEvent : INotification
    {
        public long[] AuctionItemIds { get; }
        public string Operation { get; }

        public BatchCacheUpdateEvent(long[] auctionItemIds, string operation)
        {
            AuctionItemIds = auctionItemIds ?? new long[0];
            Operation = operation ?? "update";
        }
    }
} 