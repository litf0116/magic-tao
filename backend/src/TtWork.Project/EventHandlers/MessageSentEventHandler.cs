using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Abp.Events.Bus.Handlers;
using Microsoft.EntityFrameworkCore;
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
/// 自动维护ChatChannel表的数据和聊天删除记录恢复
/// </summary>
public class MessageSentEventHandler : IAsyncEventHandler<ChatMessageSentEvent>, ITransientDependency
{
    private readonly ChatChannelService _chatChannelService;
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IRepository<ChatListDelete> _chatListDeleteRepository;

    public MessageSentEventHandler(
        ChatChannelService chatChannelService,
        IRepository<Message, Guid> messageRepository,
        IRepository<ChatListDelete> chatListDeleteRepository)
    {
        _chatChannelService = chatChannelService;
        _messageRepository = messageRepository;
        _chatListDeleteRepository = chatListDeleteRepository;
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

            // 如果是私聊消息，自动恢复被删除的聊天记录
            if (string.IsNullOrEmpty(message.Chan) && message.To.HasValue)
            {
                await RestoreDeletedChatRecords(message.From, message.To.Value);
            }
        }
        catch (Exception)
        {
            // 记录错误但不影响主流程
            // 可以根据需要添加日志记录
            // Logger.Error($"更新聊天频道失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 恢复被删除的聊天记录
    /// 当有新消息时，如果用户之前删除了与对方的聊天，则自动恢复
    /// </summary>
    /// <param name="fromUserId">发送者用户ID</param>
    /// <param name="toUserId">接收者用户ID</param>
    private async Task RestoreDeletedChatRecords(long fromUserId, long toUserId)
    {
        try
        {
            // 删除 T_ChatListDelete 记录
            await _chatListDeleteRepository.GetAll()
                .Where(x => 
                    (x.UserId == fromUserId && x.ToUserId == toUserId) ||
                    (x.UserId == toUserId && x.ToUserId == fromUserId))
                .ExecuteDeleteAsync();

            // 恢复接收方的会话状态
            await _chatChannelService.RestoreUserChannelAsync(toUserId, fromUserId);
            
            // 恢复发送方的状态
            await _chatChannelService.RestoreUserChannelAsync(fromUserId, toUserId);
        }
        catch (Exception)
        {
            // 静默处理，不影响主流程
        }
    }
}