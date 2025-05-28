using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using TtWork.Lib.Extensions;

namespace TtWork.Project.Events.Commands;

public class MyCountCacheClear : INotification {
    public long? UserId { get; set; }

    public MyCountCacheClear(long? userId) {
        UserId = userId;
    }

    public class MyCountCacheClearHandle(IMemoryCache memoryCache) : INotificationHandler<MyCountCacheClear> {
        public Task Handle(MyCountCacheClear notification, CancellationToken cancellationToken) {
            if (!notification.UserId.HasValue) return Task.CompletedTask;
            
            var cacheKey = AppConsts.CacheKeys.MyCount.FormatWith(notification.UserId);
            memoryCache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }
}