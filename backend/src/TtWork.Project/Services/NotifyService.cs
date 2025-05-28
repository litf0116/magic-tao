using System;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Notifications;
using Abp.Runtime.Session;

namespace TtWork.Services
{
    public class NotifyService : ITransientDependency
    {
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAbpSession _abpSession;

        public NotifyService(
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAbpSession abpSession
        )
        {
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _abpSession = abpSession;
        }

        //Subscribe to a general notification
        public async Task SentAuditRequest(int? tenantId, long userId)
        {
            await _notificationSubscriptionManager.SubscribeAsync(new UserIdentifier(tenantId, userId), "SentAuditRequest");
        }

        //Subscribe to an entity notification
        // public async Task Subscribe_CommentPhoto(int? tenantId, long userId, Guid photoId)
        // {
        //     await _notificationSubscriptionManager.SubscribeAsync(new UserIdentifier(tenantId, userId), "CommentPhoto", new EntityIdentifier(typeof(Photo), photoId));
        // }
    }
}