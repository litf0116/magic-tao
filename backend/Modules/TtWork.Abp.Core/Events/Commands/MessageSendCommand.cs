using System;
using MediatR;

namespace TtWork.Abp.Events.Commands
{
    [Flags]
    public enum MessageType
    {
        Email = 1,
        Sms = 2,
        WechatTemplate = 4,
        WechatWorkWebHook = 8,
        WechatWorkApp = 16,
        DTalkWebHook = 128
    }

    public interface IMessageDetail
    {
    }

    public class MessageSendCommand : INotification
    {
        public MessageType MessageType { get; }
        public IMessageDetail Detail { get; }

        public MessageSendCommand(MessageType messageType, IMessageDetail detail)
        {
            MessageType = messageType;
            Detail = detail;
        }
    }
}