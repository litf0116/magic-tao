using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using FreeIM;
using Microsoft.AspNetCore.Mvc;
using TtWork.Project.Services.Messaging;
using TtWork.Project.Services.Messaging.Models;

namespace TtWork.Project.Controllers
{
    public class TestSendChannelMsgInput
    {
        public long From { get; set; }
        public string Chan { get; set; }
        public ChatMessage Message { get; set; }
    }

    /// <summary>
    /// 消息发送测试控制器
    /// </summary>
    [Route("api/test/message")]
    [DisableAuditing]
    public class MessageTestController : AbpController
    {
        private readonly IMessageSendingService _messageSendingService;

        public MessageTestController(IMessageSendingService messageSendingService)
        {
            _messageSendingService = messageSendingService;
        }

        /// <summary>
        /// 测试发送群组消息
        /// </summary>
        /// <param name="input">发送消息的输入参数</param>
        /// <returns>发送结果</returns>
        [HttpPost("send-channel")]
        public async Task<object> SendChannelMessageTest([FromBody] TestSendChannelMsgInput input)
        {
            var options = new MessageSendOptions
            {
                SkipPermissionCheck = true,
                SkipSensitiveWordCheck = true,
                PersistToDatabase = true,
                SendImmediately = true,
                AddUserChatLevel = true,
                AddAdminTag = true
            };

            var result = await _messageSendingService.SendAuctionMessageAsync(
                input.From,null,
                input.Chan,
                input.Message
            );

            return new
            {
                code = result.Success ? 0 : 1,
                data = new
                {
                    messageId = result.MessageId,
                    sequenceNumber = result.SequenceNumber,
                    timestamp = result.Timestamp,
                    message = result.Data
                },
                message = result.Message
            };
        }
    }
} 