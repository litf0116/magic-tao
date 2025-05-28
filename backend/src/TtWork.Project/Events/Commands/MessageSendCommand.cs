using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using TtWork.Abp.AppManagement.Events;
using TtWork.Abp.Extensions;
using TtWork.Project.Events.Queries;

namespace TtWork.Project.Events.Commands {
    public class MessageSendCommand : INotification {
        public MessageType MessageType { get; }
        public IMessageDetail Detail { get; }

        public MessageSendCommand(MessageType messageType, IMessageDetail detail) {
            MessageType = messageType;
            Detail = detail;
        }
    }

    public class MessageSendQueryHandle(
        IWxSubscribeMessageSender sender,
        IMediator mediator)
        : INotificationHandler<MessageSendCommand> {
        private readonly IWxSubscribeMessageSender _sender = sender;

        public async Task Handle(MessageSendCommand request, CancellationToken cancellationToken) {
            if (request.MessageType == MessageType.WechatTemplate) {
                var detail = request.Detail as SendWechatTemplateDetail;
                var app = await mediator.Send(new QueryApp(detail.AppName), cancellationToken);
                var appid = app.GetValue("appid");
                var appSec = app.GetValue("appsec");
                var token = await mediator.Send(new AccessTokenQuery(appid, appSec), cancellationToken);
                BackgroundJob.Enqueue<IWxSubscribeMessageSender>(
                    z => z.SendAsync(detail.openids, token, detail.template_id, detail.data,
                        detail.page, detail.miniprogram_state, detail.lang));
            }
        }
    }


    public class SendWechatTemplateDetail : IMessageDetail {
        public SendWechatTemplateDetail(string appName, string[] openids, string templateId, object data,
            string page = "", string miniprogramState = "formal", string lang = "zh_CN") {
            this.openids = openids;
            this.AppName = appName;
            this.template_id = templateId;
            this.data = data;
            this.page = page;
            this.miniprogram_state = miniprogramState;
            this.lang = lang;
        }

        public string AppName { get; set; }

        public string[] openids { get; set; }
        public string template_id { get; set; }
        public object data { get; set; }
        public string page { get; set; }
        public string miniprogram_state { get; set; }
        public string lang { get; set; }
    }


    public interface IMessageDetail {
    }

    [Flags]
    public enum MessageType {
        Email = 1,
        Sms = 2,
        WechatTemplate = 4,
        WechatWorkWebHook = 8,
        WechatWorkApp = 16,
        DTalkWebHook = 128
    }
}