# 推送消息格式与处理

## 📨 消息类型

推送通知分为以下几种类型：

| 类型 | 说明 | 使用场景 |
|------|------|----------|
| **通知消息** | 系统自动展示，用户可见 | 即时提醒、公告 |
| **数据消息** | 仅传递数据，由 App 处理 | 静默更新、后台同步 |
| **静默推送** | 唤醒 App，不显示通知 | iOS 后台更新 |
| **富媒体通知** | 包含图片、视频等多媒体 | 营销推广、重要消息 |
| **可交互通知** | 支持用户操作按钮 | 快捷操作、确认提示 |

## 📦 标准消息格式

### APNs 消息格式

#### 基础通知消息

```json
{
  "aps": {
    "alert": {
      "title": "拍卖出价提醒",
      "body": "您关注的拍品刚刚有新出价！"
    },
    "sound": "default",
    "badge": 1,
    "category": "BID_UPDATE"
  },
  "auctionId": "12345",
  "itemId": "67890",
  "bidAmount": 10000
}
```

#### 静默推送

```json
{
  "aps": {
    "content-available": 1
  },
  "type": "auction_update",
  "auctionId": "12345",
  "timestamp": "2026-03-07T10:30:00Z"
}
```

#### 富媒体通知

```json
{
  "aps": {
    "alert": {
      "title": "新拍品上线",
      "body": "限量版古董手表正在拍卖"
    },
    "mutable-content": 1,
    "sound": "default"
  },
  "image-url": "https://image.molitao.top/auctions/12345.jpg",
  "type": "new_auction",
  "auctionId": "12345"
}
```

#### 可交互通知

```json
{
  "aps": {
    "alert": {
      "title": "出价邀请",
      "body": "您是否接受这个出价？"
    },
    "sound": "default",
    "category": "BID_INVITATION"
  },
  "auctionId": "12345",
  "bidAmount": 10000
}
```

### FCM 消息格式

#### 通知消息

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "拍卖出价提醒",
      "body": "您关注的拍品刚刚有新出价！"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "bids_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_DETAIL"
      },
      "data": {
        "auctionId": "12345",
        "itemId": "67890"
      }
    }
  }
}
```

#### 数据消息

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "data": {
      "type": "auction_update",
      "auctionId": "12345",
      "itemId": "67890",
      "currentBid": 10000,
      "timestamp": "2026-03-07T10:30:00Z"
    },
    "android": {
      "priority": "high",
      "ttl": "3600s"
    }
  }
}
```

#### 主题消息

```json
{
  "message": {
    "topic": "auction_updates",
    "notification": {
      "title": "新拍品上线",
      "body": "刚刚发布了新的拍品！"
    },
    "android": {
      "priority": "high"
    }
  }
}
```

## 🎯 业务消息模板

### 拍卖相关消息

#### 1. 出价成功通知

**iOS (APNs)**

```json
{
  "aps": {
    "alert": {
      "title": "出价成功",
      "body": "您的出价 ¥10,000 已成功提交"
    },
    "sound": "default",
    "badge": 1,
    "category": "BID_SUCCESS"
  },
  "type": "bid_placed",
  "auctionId": "12345",
  "itemId": "67890",
  "amount": 10000,
  "timestamp": "2026-03-07T10:30:00Z"
}
```

**Android (FCM)**

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "出价成功",
      "body": "您的出价 ¥10,000 已成功提交"
    },
    "data": {
      "type": "bid_placed",
      "auctionId": "12345",
      "itemId": "67890",
      "amount": 10000,
      "timestamp": "2026-03-07T10:30:00Z"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "bids_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_DETAIL"
      }
    }
  }
}
```

#### 2. 新出价提醒

**iOS (APNs)**

```json
{
  "aps": {
    "alert": {
      "title": "新出价提醒",
      "body": "您关注的拍品刚刚有新出价 ¥10,000"
    },
    "sound": "default",
    "badge": 1,
    "category": "NEW_BID"
  },
  "type": "new_bid",
  "auctionId": "12345",
  "itemId": "67890",
  "amount": 10000,
  "timestamp": "2026-03-07T10:30:00Z"
}
```

**Android (FCM)**

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "新出价提醒",
      "body": "您关注的拍品刚刚有新出价 ¥10,000"
    },
    "data": {
      "type": "new_bid",
      "auctionId": "12345",
      "itemId": "67890",
      "amount": 10000,
      "timestamp": "2026-03-07T10:30:00Z"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "bids_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_DETAIL"
      }
    }
  }
}
```

#### 3. 被超出价通知

**iOS (APNs)**

```json
{
  "aps": {
    "alert": {
      "title": "出价被超越",
      "body": "您的出价已被其他买家超越"
    },
    "sound": "default",
    "badge": 1,
    "category": "OUTBID"
  },
  "type": "outbid",
  "auctionId": "12345",
  "itemId": "67890",
  "yourAmount": 10000,
  "newAmount": 11000,
  "timestamp": "2026-03-07T10:30:00Z"
}
```

**Android (FCM)**

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "出价被超越",
      "body": "您的出价已被其他买家超越"
    },
    "data": {
      "type": "outbid",
      "auctionId": "12345",
      "itemId": "67890",
      "yourAmount": 10000,
      "newAmount": 11000,
      "timestamp": "2026-03-07T10:30:00Z"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "bids_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_DETAIL"
      }
    }
  }
}
```

#### 4. 拍卖即将结束通知

**iOS (APNs)**

```json
{
  "aps": {
    "alert": {
      "title": "拍卖即将结束",
      "body": "您关注的拍品将在 10 分钟后结束拍卖"
    },
    "sound": "default",
    "badge": 1,
    "category": "AUCTION_ENDING"
  },
  "type": "auction_ending",
  "auctionId": "12345",
  "itemId": "67890",
  "endTime": "2026-03-07T11:00:00Z",
  "timestamp": "2026-03-07T10:30:00Z"
}
```

**Android (FCM)**

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "拍卖即将结束",
      "body": "您关注的拍品将在 10 分钟后结束拍卖"
    },
    "data": {
      "type": "auction_ending",
      "auctionId": "12345",
      "itemId": "67890",
      "endTime": "2026-03-07T11:00:00Z",
      "timestamp": "2026-03-07T10:30:00Z"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "auction_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_DETAIL"
      }
    }
  }
}
```

#### 5. 拍卖结束通知

**iOS (APNs)**

```json
{
  "aps": {
    "alert": {
      "title": "拍卖已结束",
      "body": "您关注的拍品拍卖已结束，请查看结果"
    },
    "sound": "default",
    "badge": 1,
    "category": "AUCTION_ENDED"
  },
  "type": "auction_ended",
  "auctionId": "12345",
  "itemId": "67890",
  "finalAmount": 12000,
  "timestamp": "2026-03-07T11:00:00Z"
}
```

**Android (FCM)**

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "拍卖已结束",
      "body": "您关注的拍品拍卖已结束，请查看结果"
    },
    "data": {
      "type": "auction_ended",
      "auctionId": "12345",
      "itemId": "67890",
      "finalAmount": 12000,
      "timestamp": "2026-03-07T11:00:00Z"
    },
    "android": {
      "priority": "high",
      "notification": {
        "channel_id": "auction_channel",
        "sound": "default",
        "notification_count": 1,
        "click_action": "OPEN_AUCTION_RESULT"
      }
    }
  }
}
```

### 系统消息

#### 1. 欢迎消息

```json
{
  "aps": {
    "alert": {
      "title": "欢迎加入拍卖平台",
      "body": "开始探索精彩的拍卖世界吧！"
    },
    "sound": "default"
  },
  "type": "welcome",
  "timestamp": "2026-03-07T10:30:00Z"
}
```

#### 2. 系统公告

```json
{
  "aps": {
    "alert": {
      "title": "系统维护通知",
      "body": "系统将于今晚 22:00-23:00 进行维护"
    },
    "sound": "default"
  },
  "type": "system_announcement",
  "startTime": "2026-03-07T22:00:00Z",
  "endTime": "2026-03-07T23:00:00Z",
  "timestamp": "2026-03-07T10:30:00Z"
}
```

## 🔧 消息处理

### 后端消息构建器

```csharp
namespace TtWork.Project.PushNotifications.Builders
{
    public interface IPushMessageBuilder
    {
        IPushMessageBuilder SetTitle(string title);
        IPushMessageBuilder SetBody(string body);
        IPushMessageBuilder AddData(string key, object value);
        IPushMessageBuilder SetCategory(string category);
        IPushMessageBuilder SetSound(string sound);
        IPushMessageBuilder SetBadge(int badge);
        IPushMessageBuilder SetPriority(string priority);
        IPushMessageBuilder SetChannelId(string channelId);
        IPushMessageBuilder SetTtl(int ttlSeconds);
        object Build();
    }

    public class PushMessageBuilder : IPushMessageBuilder
    {
        private readonly Dictionary<string, object> _data = new();
        private string _title;
        private string _body;
        private string _category;
        private string _sound = "default";
        private int? _badge;
        private string _priority = "high";
        private string _channelId;
        private int? _ttl;

        public IPushMessageBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public IPushMessageBuilder SetBody(string body)
        {
            _body = body;
            return this;
        }

        public IPushMessageBuilder AddData(string key, object value)
        {
            _data[key] = value;
            return this;
        }

        public IPushMessageBuilder SetCategory(string category)
        {
            _category = category;
            return this;
        }

        public IPushMessageBuilder SetSound(string sound)
        {
            _sound = sound;
            return this;
        }

        public IPushMessageBuilder SetBadge(int badge)
        {
            _badge = badge;
            return this;
        }

        public IPushMessageBuilder SetPriority(string priority)
        {
            _priority = priority;
            return this;
        }

        public IPushMessageBuilder SetChannelId(string channelId)
        {
            _channelId = channelId;
            return this;
        }

        public IPushMessageBuilder SetTtl(int ttlSeconds)
        {
            _ttl = ttlSeconds;
            return this;
        }

        public object Build()
        {
            // 根据平台构建不同的消息格式
            if (_channelId?.Contains("bids") == true)
            {
                return BuildBidsMessage();
            }
            else
            {
                return BuildAuctionMessage();
            }
        }

        private object BuildBidsMessage()
        {
            return new
            {
                title = _title,
                body = _body,
                sound = _sound,
                badge = _badge,
                category = _category,
                data = _data,
                priority = _priority,
                channel_id = _channelId,
                ttl = _ttl
            };
        }

        private object BuildAuctionMessage()
        {
            return new
            {
                title = _title,
                body = _body,
                sound = _sound,
                badge = _badge,
                category = _category,
                data = _data,
                priority = _priority,
                channel_id = _channelId,
                ttl = _ttl
            };
        }
    }
}
```

### 消息模板

```csharp
namespace TtWork.Project.PushNotifications.Templates
{
    public interface IPushMessageTemplate
    {
        object BuildMessage(DevicePlatform platform);
    }

    public abstract class BasePushMessageTemplate : IPushMessageTemplate
    {
        protected string Type { get; set; }
        protected Dictionary<string, object> Data { get; } = new();

        public abstract object BuildMessage(DevicePlatform platform);

        protected object BuildIosMessage(string title, string body, string category = null)
        {
            return new
            {
                aps = new
                {
                    alert = new
                    {
                        title = title,
                        body = body
                    },
                    sound = "default",
                    badge = 1,
                    category = category
                },
                type = Type,
                data = Data
            };
        }

        protected object BuildAndroidMessage(string title, string body, string channelId)
        {
            return new
            {
                notification = new
                {
                    title = title,
                    body = body
                },
                data = Data,
                android = new
                {
                    priority = "high",
                    notification = new
                    {
                        channel_id = channelId,
                        sound = "default",
                        notification_count = 1
                    }
                }
            };
        }
    }

    public class BidPlacedMessageTemplate : BasePushMessageTemplate
    {
        public BidPlacedMessageTemplate(Guid auctionId, Guid itemId, decimal amount)
        {
            Type = "bid_placed";
            Data["auctionId"] = auctionId.ToString();
            Data["itemId"] = itemId.ToString();
            Data["amount"] = amount;
            Data["timestamp"] = DateTime.UtcNow.ToString("o");
        }

        public override object BuildMessage(DevicePlatform platform)
        {
            var title = "出价成功";
            var body = $"您的出价 ¥{Data["amount"]} 已成功提交";

            return platform switch
            {
                DevicePlatform.iOS => BuildIosMessage(title, body, "BID_SUCCESS"),
                DevicePlatform.Android => BuildAndroidMessage(title, body, "bids_channel"),
                _ => throw new NotSupportedException($"Platform {platform} is not supported")
            };
        }
    }

    public class NewBidMessageTemplate : BasePushMessageTemplate
    {
        public NewBidMessageTemplate(Guid auctionId, Guid itemId, decimal amount)
        {
            Type = "new_bid";
            Data["auctionId"] = auctionId.ToString();
            Data["itemId"] = itemId.ToString();
            Data["amount"] = amount;
            Data["timestamp"] = DateTime.UtcNow.ToString("o");
        }

        public override object BuildMessage(DevicePlatform platform)
        {
            var title = "新出价提醒";
            var body = $"您关注的拍品刚刚有新出价 ¥{Data["amount"]}";

            return platform switch
            {
                DevicePlatform.iOS => BuildIosMessage(title, body, "NEW_BID"),
                DevicePlatform.Android => BuildAndroidMessage(title, body, "bids_channel"),
                _ => throw new NotSupportedException($"Platform {platform} is not supported")
            };
        }
    }

    public class AuctionEndedMessageTemplate : BasePushMessageTemplate
    {
        public AuctionEndedMessageTemplate(Guid auctionId, Guid itemId, decimal finalAmount)
        {
            Type = "auction_ended";
            Data["auctionId"] = auctionId.ToString();
            Data["itemId"] = itemId.ToString();
            Data["finalAmount"] = finalAmount;
            Data["timestamp"] = DateTime.UtcNow.ToString("o");
        }

        public override object BuildMessage(DevicePlatform platform)
        {
            var title = "拍卖已结束";
            var body = "您关注的拍品拍卖已结束，请查看结果";

            return platform switch
            {
                DevicePlatform.iOS => BuildIosMessage(title, body, "AUCTION_ENDED"),
                DevicePlatform.Android => BuildAndroidMessage(title, body, "auction_channel"),
                _ => throw new NotSupportedException($"Platform {platform} is not supported")
            };
        }
    }
}
```

## 📊 消息统计与分析

### 消息发送统计

```csharp
namespace TtWork.Project.PushNotifications.Statistics
{
    public class PushMessageStatistics
    {
        public string MessageType { get; set; }
        public int SentCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate => SentCount > 0 ? (double)SuccessCount / SentCount : 0;
        public DateTime SentAt { get; set; }
    }

    public class PushStatisticsService : ITransientDependency
    {
        private readonly IDistributedCache<List<PushMessageStatistics>> _cache;
        private readonly ILogger<PushStatisticsService> _logger;

        public PushStatisticsService(
            IDistributedCache<List<PushMessageStatistics>> cache,
            ILogger<PushStatisticsService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task RecordMessageAsync(string messageType, int successCount, int failureCount)
        {
            var cacheKey = $"push_statistics:{DateTime.UtcNow:yyyyMMdd}";
            var statistics = await _cache.GetAsync(cacheKey) ?? new List<PushMessageStatistics>();

            statistics.Add(new PushMessageStatistics
            {
                MessageType = messageType,
                SentCount = successCount + failureCount,
                SuccessCount = successCount,
                FailureCount = failureCount,
                SentAt = DateTime.UtcNow
            });

            await _cache.SetAsync(cacheKey, statistics, TimeSpan.FromDays(7));
        }

        public async Task<List<PushMessageStatistics>> GetStatisticsAsync(DateTime date)
        {
            var cacheKey = $"push_statistics:{date:yyyyMMdd}";
            return await _cache.GetAsync(cacheKey) ?? new List<PushMessageStatistics>();
        }

        public async Task<Dictionary<string, double>> GetSuccessRateByTypeAsync(DateTime date)
        {
            var statistics = await GetStatisticsAsync(date);
            return statistics
                .GroupBy(s => s.MessageType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(s => s.SuccessRate)
                );
        }
    }
}
```

## 🔗 参考资料

- [APNs Payload Key Reference](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server/generating_a_remote_notification)
- [FCM Message Types](https://firebase.google.com/docs/cloud-messaging/concept-options)
- [Rich Notifications](https://developer.apple.com/documentation/usernotifications/modifying_content_in_newly_delivered_notifications)
