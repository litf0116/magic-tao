using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TtWork.Project.Domains;

namespace TtWork.Project.Services;

/// <summary>
/// 聊天频道管理服务
/// </summary>
public class ChatChannelService : DomainService
{
    private readonly IRepository<ChatChannel, long> _chatChannelRepository;
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IRepository<ChatListDelete, int> _chatListDeleteRepository;

    public ChatChannelService(
        IRepository<ChatChannel, long> chatChannelRepository,
        IRepository<Message, Guid> messageRepository,
        IRepository<ChatListDelete, int> chatListDeleteRepository)
    {
        _chatChannelRepository = chatChannelRepository;
        _messageRepository = messageRepository;
        _chatListDeleteRepository = chatListDeleteRepository;
    }

    /// <summary>
    /// 删除用户的会话显示
    /// 只设置指定用户的状态为已删除，不影响对方
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="otherUserId">对方用户ID</param>
    public async Task DeleteUserChannelAsync(long userId, long otherUserId)
    {
        var channelId = CreatePrivateChannelId(userId, otherUserId);
        var channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);

        if (channel != null)
        {
            channel.SetUserStatus(userId, ChatChannelStatus.Deleted);
            await _chatChannelRepository.UpdateAsync(channel);
        }
    }

    /// <summary>
    /// 恢复用户的会话显示
    /// 当有新消息时自动调用此方法
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="otherUserId">对方用户ID</param>
    public async Task RestoreUserChannelAsync(long userId, long otherUserId)
    {
        var channelId = CreatePrivateChannelId(userId, otherUserId);
        var channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);

        if (channel != null && channel.GetUserStatus(userId) == ChatChannelStatus.Deleted)
        {
            channel.SetUserStatus(userId, ChatChannelStatus.Normal);
            await _chatChannelRepository.UpdateAsync(channel);
        }
    }

    /// <summary>
    /// 批量恢复用户的会话显示
    /// 用于用户重新登录时自动恢复所有会话（可选功能）
    /// </summary>
    /// <param name="userId">用户ID</param>
    public async Task RestoreAllUserChannelsAsync(long userId)
    {
        var channels = await _chatChannelRepository.GetAll()
            .Where(channel => channel.ChannelType == ChatChannelType.Private &&
                           (channel.User1Id == userId || channel.User2Id == userId) &&
                           (channel.User1Id == userId ? channel.User1Status : channel.User2Status) == ChatChannelStatus.Deleted)
            .ToListAsync();

        foreach (var channel in channels)
        {
            channel.SetUserStatus(userId, ChatChannelStatus.Normal);
        }

        if (channels.Any())
        {
            foreach (var channel in channels)
            {
                await _chatChannelRepository.UpdateAsync(channel);
            }
        }
    }

    /// <summary>
    /// 获取或创建私聊频道
    /// </summary>
    /// <param name="user1Id">用户1 ID</param>
    /// <param name="user2Id">用户2 ID</param>
    /// <returns>聊天频道</returns>
    public async Task<ChatChannel> GetOrCreatePrivateChannelAsync(long user1Id, long user2Id)
    {
        // 禁止自己与自己聊天
        if (user1Id == user2Id)
        {
            throw new UserFriendlyException("不能与自己聊天");
        }


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
    /// 更新频道的最后消息信息
    /// 如果频道被用户隐藏，会自动恢复显示
    /// </summary>
    /// <param name="message">消息实体</param>
    public async Task UpdateChannelLastMessageAsync(Message message)
    {
        // 私聊频道：发送者和接收者都不为空
        if (message.To.HasValue)
        {
            var channel = await GetOrCreatePrivateChannelAsync(message.From, message.To.Value);
            channel.UpdateLastMessage(message);
            await _chatChannelRepository.UpdateAsync(channel);

            // 自动恢复被隐藏的私聊频道
            await AutoRestoreHiddenPrivateChannel(message.From, message.To.Value);
        }
        // 系统频道不再自动创建 ChatChannel 记录
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
    /// 获取用户可见的聊天频道列表（用户状态字段版本）
    /// 单次SQL查询，性能最优
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>可见的聊天频道列表</returns>
    public async Task<List<ChatChannel>> GetVisibleChannelsForUserAsync(long userId)
    {
        var query = _chatChannelRepository.GetAll()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(channel => channel.IsActive && (channel.ChannelType == ChatChannelType.System || channel.LastMessageId != null))
            .Where(channel => !(channel.ChannelType == ChatChannelType.Private && channel.User1Id == channel.User2Id))
            .Where(channel =>
                // 系统频道：所有人可见
                channel.ChannelType == ChatChannelType.System ||
                // 私聊频道：用户参与且状态正常
                (channel.ChannelType == ChatChannelType.Private &&
                 ((channel.User1Id == userId && channel.User1Status == ChatChannelStatus.Normal) ||
                  (channel.User2Id == userId && channel.User2Status == ChatChannelStatus.Normal))))
            .OrderBy(channel => channel.ChannelType)
            .ThenByDescending(channel => channel.SortOrder)
            .ThenByDescending(channel => channel.LastMessageTime);

        var result = await query.ToListAsync();
        Logger.Debug($"GetVisibleChannelsForUserAsync: userId={userId}, totalChannels={result.Count}");
        return result;
    }

    /// <summary>
    /// 获取用户可见的聊天频道（原始版本 - 使用删除表方案）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>可见的聊天频道列表</returns>
    public async Task<List<ChatChannel>> GetVisibleChannelsForUserAsyncLegacy(long userId)
    {
        var deletedUserIds = await _chatListDeleteRepository.GetAll()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ToUserId)
            .ToListAsync();

        var channels = await _chatChannelRepository.GetAll()
            .AsNoTracking()
            .Where(channel => channel.IsActive && channel.LastMessageId != null)
            .Where(channel =>
                channel.ChannelType == ChatChannelType.System ||
                (channel.ChannelType == ChatChannelType.Private &&
                 (channel.User1Id == userId || channel.User2Id == userId)))
            .ToListAsync();

        var result = channels
            .Where(channel =>
                channel.ChannelType == ChatChannelType.System ||
                (channel.ChannelType == ChatChannelType.Private &&
                 !deletedUserIds.Contains(channel.GetOtherUserId(userId) ?? 0)))
            .OrderBy(channel => channel.ChannelType)
            .ThenByDescending(channel => channel.SortOrder)
            .ThenByDescending(channel => channel.LastMessageTime)
            .ToList();

        return result;
    }

    /// <summary>
    /// 获取用户可见的聊天频道列表（版本二：使用子查询过滤删除表）
    /// 2025-09-17 时的实现版本
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>可见的聊天频道列表</returns>
    public async Task<List<ChatChannel>> GetVisibleChannelsForUserAsyncV2(long userId)
    {
        var query = from channel in _chatChannelRepository.GetAll().AsNoTracking()
                    where channel.IsActive && channel.LastMessageId != null
                    where channel.ChannelType == ChatChannelType.System ||
                          channel.User1Id == userId ||
                          channel.User2Id == userId
                    where channel.ChannelType == ChatChannelType.System ||
                          (channel.ChannelType == ChatChannelType.Private &&
                           !_chatListDeleteRepository.GetAll().Any(delete =>
                               delete.UserId == userId &&
                               delete.ToUserId == (channel.User1Id == userId ? channel.User2Id : channel.User1Id)))
                    orderby channel.ChannelType,
                            channel.SortOrder descending,
                            channel.LastMessageTime descending
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
    /// 现在使用用户状态字段来管理删除状态
    /// </summary>
    /// <param name="otherUserId">对方用户ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    public async Task HideChannelForUserAsync(long otherUserId, long currentUserId)
    {
        await DeleteUserChannelAsync(currentUserId, otherUserId);
    }

    /// <summary>
    /// 获取用户已删除的聊天数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>已删除的聊天数量</returns>
    public async Task<int> GetUserDeletedChannelsCountAsync(long userId)
    {
        return await _chatListDeleteRepository.GetAll()
            .CountAsync(x => x.UserId == userId);
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
    /// 迁移现有消息数据到频道表
    /// 这是一个一次性的数据迁移方法，用于将现有的私聊消息数据迁移到新的频道表结构
    /// </summary>
    public async Task MigrateExistingMessagesToChannelsAsync()
    {
        // 迁移私聊消息
        var privateChats = await _messageRepository.GetAll()
            .Where(x => x.To != null)
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
    /// 同步聊天频道数据
    /// 根据 Message 表数据创建或更新私聊频道 ChatChannel 表记录
    /// 用于数据迁移
    /// </summary>
    /// <param name="maxBatchCount">最大处理用户数（分批处理避免内存溢出）</param>
    /// <returns>同步结果统计</returns>
    public async Task<ChannelSyncResult> SyncChannelsFromMessageAsync(int maxBatchCount = 1000)
    {
        var result = new ChannelSyncResult
        {
            StartTime = DateTime.Now
        };

        try
        {
            // 获取所有活跃用户（有聊天记录的用户）
            var activeUserIds = await _messageRepository.GetAll()
                .AsNoTracking()
                .Where(x => x.To != null)
                .SelectMany(x => new[] { x.From, x.To.Value })
                .Distinct()
                .ToListAsync();

            result.TotalActiveUsers = activeUserIds.Count;

            // 分批处理私聊频道
            int privateChannelCreated = 0;
            int privateChannelUpdated = 0;
            int processedUsers = 0;
            int batchNumber = 0;

            foreach (var userId in activeUserIds)
            {
                batchNumber++;
                processedUsers++;

                // 获取该用户的所有私聊对话
                var privateChats = await _messageRepository.GetAll()
                    .AsNoTracking()
                    .Where(x => x.To != null && (x.From == userId || x.To == userId))
                    .GroupBy(x => new
                    {
                        User1 = x.From < x.To ? x.From : x.To.Value,
                        User2 = x.From > x.To ? x.From : x.To.Value
                    })
                    .Select(g => new
                    {
                        g.Key.User1,
                        g.Key.User2,
                        LastMessage = g.OrderByDescending(m => m.Time).FirstOrDefault(),
                        MessageCount = g.Count()
                    })
                    .ToListAsync();

                foreach (var pc in privateChats)
                {
                    var channel = await GetOrCreatePrivateChannelAsync(pc.User1, pc.User2);

                    if (pc.LastMessage != null)
                    {
                        bool needUpdate = channel.LastMessageId == null ||
                                         (pc.LastMessage.Time > channel.LastMessageTime);

                        if (needUpdate)
                        {
                            channel.UpdateLastMessage(pc.LastMessage);
                            await _chatChannelRepository.UpdateAsync(channel);
                            privateChannelUpdated++;
                        }
                    }
                }

                // 分批提交以避免内存问题
                if (batchNumber % 100 == 0)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    Logger.Debug($"同步进度: 已处理 {processedUsers}/{activeUserIds.Count} 用户, 创建私聊频道 {privateChannelCreated}, 更新 {privateChannelUpdated}");
                }
            }

            // 最终保存
            await CurrentUnitOfWork.SaveChangesAsync();

            result.PrivateChannelsCreated = privateChannelCreated;
            result.PrivateChannelsUpdated = privateChannelUpdated;
            result.EndTime = DateTime.Now;
            result.IsSuccess = true;

            Logger.Debug($"私聊频道同步完成: 创建 {privateChannelCreated}, 更新 {privateChannelUpdated}");
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.Now;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            Logger.Error("同步频道数据失败: " + ex.Message);
        }

        return result;
    }

    /// <summary>
    /// 同步单个用户的私聊频道
    /// 根据该用户的私聊消息创建或更新 ChatChannel 记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>同步结果</returns>
    public async Task<UserChannelSyncResult> SyncUserChannelsAsync(long userId)
    {
        var result = new UserChannelSyncResult
        {
            UserId = userId,
            StartTime = DateTime.Now
        };

        try
        {
            // 获取该用户的所有私聊对话
            var privateChats = await _messageRepository.GetAll()
                .AsNoTracking()
                .Where(x => x.To != null && (x.From == userId || x.To == userId))
                .GroupBy(x => new
                {
                    User1 = x.From < x.To ? x.From : x.To.Value,
                    User2 = x.From > x.To ? x.From : x.To.Value
                })
                .Select(g => new
                {
                    g.Key.User1,
                    g.Key.User2,
                    LastMessage = g.OrderByDescending(m => m.Time).FirstOrDefault(),
                    MessageCount = g.Count()
                })
                .ToListAsync();

            result.TotalChannels = privateChats.Count;

            foreach (var pc in privateChats)
            {
                var channel = await GetOrCreatePrivateChannelAsync(pc.User1, pc.User2);

                if (pc.LastMessage != null)
                {
                    bool isNew = channel.LastMessageId == null;
                    bool needUpdate = !isNew && pc.LastMessage.Time > channel.LastMessageTime;

                    if (isNew)
                    {
                        channel.UpdateLastMessage(pc.LastMessage);
                        await _chatChannelRepository.UpdateAsync(channel);
                        result.CreatedChannels++;
                    }
                    else if (needUpdate)
                    {
                        channel.UpdateLastMessage(pc.LastMessage);
                        await _chatChannelRepository.UpdateAsync(channel);
                        result.UpdatedChannels++;
                    }
                }
            }

            result.EndTime = DateTime.Now;
            result.IsSuccess = true;
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.Now;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            Logger.Error("同步用户 " + userId + " 的频道数据失败: " + ex.Message);
        }

        return result;
    }

    /// <summary>
    /// 从 T_ChatListDelete 同步用户删除状态到 UserStatus
    /// 用于数据一致性修复
    /// </summary>
    /// <param name="userId">用户ID</param>
    public async Task SyncUserStatusFromChatListDeleteAsync(long userId)
    {
        var deletedUserIds = await _chatListDeleteRepository.GetAll()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ToUserId)
            .ToListAsync();

        foreach (var otherUserId in deletedUserIds)
        {
            var channelId = CreatePrivateChannelId(userId, otherUserId);
            var channel = await _chatChannelRepository.FirstOrDefaultAsync(x => x.ChannelId == channelId);
            
            if (channel != null)
            {
                channel.SetUserStatus(userId, ChatChannelStatus.Deleted);
                await _chatChannelRepository.UpdateAsync(channel);
            }
        }
    }

    /// <summary>
    /// 批量同步所有用户的删除状态
    /// 执行一次性的数据修复
    /// </summary>
    public async Task SyncAllUserStatusFromChatListDeleteAsync()
    {
        var userIds = await _chatListDeleteRepository.GetAll()
            .AsNoTracking()
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in userIds)
        {
            await SyncUserStatusFromChatListDeleteAsync(userId);
        }
    }
}

/// <summary>
/// 频道同步结果
/// </summary>
public class ChannelSyncResult
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalActiveUsers { get; set; }
    public int PrivateChannelsCreated { get; set; }
    public int PrivateChannelsUpdated { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
}

/// <summary>
/// 用户频道同步结果
/// </summary>
public class UserChannelSyncResult
{
    public long UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalChannels { get; set; }
    public int CreatedChannels { get; set; }
    public int UpdatedChannels { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
}
