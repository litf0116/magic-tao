using System;
using MediatR;

namespace TtWork.Abp.Core.Events.Commands;

[Flags]
public enum NotificationType
{
    Audit = 1
}

public class NotificationCommand : INotification
{
    public NotificationType Type { get; set; }
    public long? TenandId { get; }
    public long UserId { get; set; }
    public string Message { get; }

    public string SubMessage { get; set; }

    public NotificationCommand(NotificationType type, long? tenandId, long userId, string message, string subMessage = null)
    {
        Type = type;
        TenandId = tenandId;
        UserId = userId;
        Message = message;
        SubMessage = subMessage;
    }
}