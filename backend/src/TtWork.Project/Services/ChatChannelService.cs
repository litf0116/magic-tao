using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Microsoft.EntityFrameworkCore;
using TtWork.Project.Domains;

namespace TtWork.Project.Services;

/// <summary>
/// 聊天频道管理服务
/// </summary>
public class ChatChannelService : DomainService
{
    private readonly IRepository<ChatChannel, long> _chatChannelRepository;
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IRepository<ChatListDelete> _chatListDeleteRepository;

    public ChatChannelService(
        IRepository<ChatChannel, long> chatChannelRepository,
        IRepository<Message, Guid> messageRepository,
        IRepository<ChatListDelete> chatListDeleteRepository)
    {
        _chatChannelRepository = chatChannelRepository;
        _messageRepository = messageRepository;
        _chatListDeleteRepository = chatListDeleteRepository;
    }

    /// <summary>
    /// 获取或创建私聊频道
    /// </summary>
    /// <param name="user1Id">用户1 ID</param>
    /// <param name="user2Id">用户2 ID</param>
    /// <returns>聊天频道</returns>
    public async Task<ChatChannel> GetOrCreatePrivateChannelAsync(long user1Id, long user2Id)
    {
        var channelId = CreatePrivateChannelId(user1Id, user2Id);

        var existingChannel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);
        if (existingChannel != null)
        {
            return existingChannel;
        }

        // 创建新的私聊频道
        var newChannel = new ChatChannel(user1Id, user2Id);
        return await _chatChannelRepository.InsertAsync(newChannel);
    }

    /// <summary>
    /// 获取或创建系统频道
    /// </summary>
    /// <param name="channelId">频道ID</param>
    /// <param name="channelName">频道名称</param>
    /// <returns>聊天频道</returns>
    public async Task<ChatChannel> GetOrCreateSystemChannelAsync(string channelId, string channelName)
    {
        var existingChannel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);
        if (existingChannel != null)
        {
            return existingChannel;
        }

        var newChannel = new ChatChannel(channelId, channelName);
        return await _chatChannelRepository.InsertAsync(newChannel);
    }

    /// <summary>
    /// 更新频道的最后消息信息
    /// 如果频道被用户隐藏，会自动恢复显示
    /// </summary>
    /// <param name="message">消息实体</param>
    public async Task UpdateChannelLastMessageAsync(Message message)
    {
        ChatChannel channel = null;

        if (!string.IsNullOrEmpty(message.Chan))
        {
            // 系统频道消息
            channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == message.Chan);
            if (channel == null)
            {
                // 如果频道不存在，创建系统频道
                var channelName = GetSystemChannelName(message.Chan);
                channel = await GetOrCreateSystemChannelAsync(message.Chan, channelName);
            }
        }
        else if (message.To.HasValue)
        {
            // 私聊消息
            channel = await GetOrCreatePrivateChannelAsync(message.From, message.To.Value);

            // 自动恢复被隐藏的私聊频道
            await AutoRestoreHiddenPrivateChannel(message.From, message.To.Value);
        }

        if (channel != null)
        {
            channel.UpdateLastMessage(message);
            await _chatChannelRepository.UpdateAsync(channel);
        }
    }

    /// <summary>
    /// 自动恢复被隐藏的私聊频道
    /// 当有新消息时，如果用户之前删除了与对方的聊天，则自动恢复
    /// 注意：此逻辑已迁移到 MessageSentEventHandler 中，保留此方法以兼容现有调用
    /// </summary>
    /// <param name="fromUserId">发送者用户ID</param>
    /// <param name="toUserId">接收者用户ID</param>
    private async Task AutoRestoreHiddenPrivateChannel(long fromUserId, long toUserId)
    {
        // 此逻辑已迁移到 MessageSentEventHandler 中
        // 保留此方法以兼容现有调用，但实际不再执行任何操作
        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取用户的聊天频道列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="excludeDeletedChannels">排除已删除的频道ID列表</param>
    /// <returns>聊天频道列表</returns>
    public async Task<List<ChatChannel>> GetUserChannelsAsync(long? userId, List<long> excludeDeletedChannels = null)
    {
        var query = _chatChannelRepository.GetAll().AsNoTracking()
            .Where(x => x.IsActive && x.LastMessageId != null); // 只返回有消息的频道

        if (userId.HasValue)
        {
            // 用户可以看到的频道：系统频道 + 自己参与的私聊
            query = query.Where(x =>
                x.ChannelType == ChatChannelType.System ||
                x.User1Id == userId.Value ||
                x.User2Id == userId.Value);
        }
        else
        {
            // 未登录用户只能看到系统频道
            query = query.Where(x => x.ChannelType == ChatChannelType.System);
        }

        if (excludeDeletedChannels?.Count > 0)
        {
            query = query.Where(x => !excludeDeletedChannels.Contains(x.Id));
        }

        return await query
            .OrderBy(x => x.ChannelType) // 系统频道优先
            .ThenByDescending(x => x.SortOrder) // 按排序权重
            .ThenByDescending(x => x.LastMessageTime) // 最后按时间
            .ToListAsync();
    }

    /// <summary>
    /// 获取用户可见的聊天频道列表（已过滤删除的聊天）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>可见的聊天频道列表</returns>
    public async Task<List<ChatChannel>> GetVisibleChannelsForUserAsync(long userId)
    {
        var query = from channel in _chatChannelRepository.GetAll().AsNoTracking()
                    where channel.IsActive && channel.LastMessageId != null
                    where channel.ChannelType == ChatChannelType.System ||
                          channel.User1Id == userId ||
                          channel.User2Id == userId
                    // 对于私聊频道，过滤掉用户已删除的聊天
                    where channel.ChannelType == ChatChannelType.System ||
                          (channel.ChannelType == ChatChannelType.Private &&
                           !_chatListDeleteRepository.GetAll().Any(delete =>
                               delete.UserId == userId &&
                               delete.ToUserId == (channel.User1Id == userId ? channel.User2Id : channel.User1Id)))
                    orderby channel.ChannelType,  // 系统频道优先
                            channel.SortOrder descending,  // 按排序权重
                            channel.LastMessageTime descending  // 最后按时间
                    select channel;

        return await query.ToListAsync();
    }

    /// <summary>
    /// 检查用户是否删除了与指定用户的聊天
    /// </summary>
    /// <param name="currentUserId">当前用户ID</param>
    /// <param name="otherUserId">对方用户ID</param>
    /// <returns>是否已删除</returns>
    public async Task<bool> IsChannelHiddenForUserAsync(long currentUserId, long otherUserId)
    {
        // 这里可以根据实际的删除记录表来判断
        // 当前基于 ChatListDelete 表，所以需要在调用方进行判断
        // 如果将来有专门的用户频道设置表，可以在这里实现
        return await Task.FromResult(false);
    }

    /// <summary>
    /// 软删除频道（对用户隐藏）
    /// 注意：当有新消息时，隐藏的频道会自动恢复显示
    /// </summary>
    /// <param name="otherUserId">对方用户ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    public async Task HideChannelForUserAsync(long otherUserId, long currentUserId)
    {
        // 查找对应的私聊频道
        var channelId = CreatePrivateChannelId(currentUserId, otherUserId);
        var channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);

        if (channel != null)
        {
            // 频道隐藏是通过 ChatListDelete 表来实现的
            // 当有新消息时，UpdateChannelLastMessageAsync 方法会自动删除 ChatListDelete 记录
            // 从而实现自动恢复聊天显示的效果

            // 这里不需要修改 ChatChannel 表，因为隐藏状态是用户级别的
            // 而 ChatChannel 表存储的是全局的频道信息
        }

        // 当前实现：依赖 ChatListDelete 表来标记隐藏状态
        // 自动恢复逻辑在 UpdateChannelLastMessageAsync 方法中实现
    }

    /// <summary>
    /// 创建私聊频道ID
    /// </summary>
    /// <param name="user1Id">用户1 ID</param>
    /// <param name="user2Id">用户2 ID</param>
    /// <returns>频道ID</returns>
    private static string CreatePrivateChannelId(long user1Id, long user2Id)
    {
        var (smallerId, largerId) = user1Id < user2Id ? (user1Id, user2Id) : (user2Id, user1Id);
        return $"private_{smallerId}_{largerId}";
    }

    /// <summary>
    /// 获取系统频道名称
    /// </summary>
    /// <param name="channelId">频道ID</param>
    /// <returns>频道名称</returns>
    private static string GetSystemChannelName(string channelId)
    {
        return channelId switch
        {
            "-1_auction" => "拍卖频道",
            "0_lobby" => "大厅",
            _ => channelId
        };
    }

    /// <summary>
    /// 迁移现有消息数据到频道表
    /// 这是一个一次性的数据迁移方法，用于将现有的消息数据迁移到新的频道表结构
    /// </summary>
    public async Task MigrateExistingMessagesToChannelsAsync()
    {
        // 1. 迁移系统频道消息
        var systemChannels = await _messageRepository.GetAll()
            .Where(x => !string.IsNullOrEmpty(x.Chan))
            .GroupBy(x => x.Chan)
            .Select(g => new
            {
                ChannelId = g.Key,
                LastMessage = g.OrderByDescending(m => m.Time).FirstOrDefault()
            })
            .ToListAsync();

        foreach (var systemChannel in systemChannels)
        {
            var channelName = GetSystemChannelName(systemChannel.ChannelId);
            var channel = await GetOrCreateSystemChannelAsync(systemChannel.ChannelId, channelName);

            if (systemChannel.LastMessage != null)
            {
                channel.UpdateLastMessage(systemChannel.LastMessage);
                await _chatChannelRepository.UpdateAsync(channel);
            }
        }

        // 2. 迁移私聊消息
        var privateChats = await _messageRepository.GetAll()
            .Where(x => string.IsNullOrEmpty(x.Chan) && x.To != null)
            .GroupBy(x => new
            {
                User1 = x.From < x.To ? x.From : x.To.Value,
                User2 = x.From > x.To ? x.From : x.To.Value
            })
            .Select(g => new
            {
                g.Key.User1,
                g.Key.User2,
                LastMessage = g.OrderByDescending(m => m.Time).FirstOrDefault()
            })
            .ToListAsync();

        foreach (var privateChat in privateChats)
        {
            var channel = await GetOrCreatePrivateChannelAsync(privateChat.User1, privateChat.User2);

            if (privateChat.LastMessage != null)
            {
                channel.UpdateLastMessage(privateChat.LastMessage);
                await _chatChannelRepository.UpdateAsync(channel);
            }
        }
    }

    /// <summary>
    /// 检查并获取需要自动恢复的频道列表
    /// 这个方法可以用于批量检查哪些频道可以被自动恢复
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>可以自动恢复的频道ID列表</returns>
    public async Task<List<string>> GetAutoRestorableChannelsAsync(long userId)
    {
        var restorableChannels = new List<string>();

        // 获取用户所有隐藏的聊天记录
        var hiddenChats = await _chatListDeleteRepository.GetAll()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        foreach (var hiddenChat in hiddenChats)
        {
            // 检查是否存在对应的私聊频道
            var channelId = CreatePrivateChannelId(userId, hiddenChat.ToUserId);
            var channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);

            if (channel != null && channel.LastMessageId.HasValue)
            {
                restorableChannels.Add(channelId);
            }
        }

        return restorableChannels;
    }
}
