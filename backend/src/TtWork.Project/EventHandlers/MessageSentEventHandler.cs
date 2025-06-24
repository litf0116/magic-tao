using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Abp.Events.Bus.Handlers;
using TtWork.Project.Domains;
using TtWork.Project.Services;

namespace TtWork.Project.EventHandlers;

/// <summary>
/// 聊天消息发送事件
/// </summary>
public class ChatMessageSentEvent : EventData
{
    public Guid MessageId { get; set; }

    public ChatMessageSentEvent(Guid messageId)
    {
        MessageId = messageId;
    }
}

/// <summary>
/// 聊天消息发送事件处理器
/// 自动维护ChatChannel表的数据
/// </summary>
public class MessageSentEventHandler : IAsyncEventHandler<ChatMessageSentEvent>, ITransientDependency
{
    private readonly ChatChannelService _chatChannelService;
    private readonly IRepository<Message, Guid> _messageRepository;

    public MessageSentEventHandler(
        ChatChannelService chatChannelService,
        IRepository<Message, Guid> messageRepository)
    {
        _chatChannelService = chatChannelService;
        _messageRepository = messageRepository;
    }

    /// <summary>
    /// 处理聊天消息发送事件
    /// </summary>
    /// <param name="eventData">聊天消息发送事件</param>
    public async Task HandleEventAsync(ChatMessageSentEvent eventData)
    {
        try
        {
            // 查找对应的消息记录
            var message = await _messageRepository.FirstOrDefaultAsync(m => m.Id == eventData.MessageId);
            if (message == null)
            {
                return;
            }

            // 更新对应的聊天频道
            await _chatChannelService.UpdateChannelLastMessageAsync(message);
        }
        catch (Exception)
        {
            // 记录错误但不影响主流程
            // 可以根据需要添加日志记录
            // Logger.Error($"更新聊天频道失败: {ex.Message}", ex);
        }
    }
}
