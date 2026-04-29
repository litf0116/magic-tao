using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

/// <summary>
/// 聊天频道表 - 用于记录聊天会话的元数据，提高聊天列表查询性能
/// </summary>
[Table("T_ChatChannel")]
public class ChatChannel : Entity<long>, IHasCreationTime, IHasModificationTime
{
    public ChatChannel()
    {
    }

    /// <summary>
    /// 创建私聊频道
    /// </summary>
    /// <param name="user1Id">用户1 ID</param>
    /// <param name="user2Id">用户2 ID</param>
    /// <param name="lastMessage">最后一条消息</param>
    public ChatChannel(long user1Id, long user2Id, Message lastMessage = null)
    {
        // 确保较小的ID在前面，保证频道唯一性
        var (smallerId, largerId) = user1Id < user2Id ? (user1Id, user2Id) : (user2Id, user1Id);

        ChannelId = $"private_{smallerId}_{largerId}";
        ChannelType = ChatChannelType.Private;
        User1Id = smallerId;
        User2Id = largerId;

        // 初始化用户状态（默认都可见）
        User1Status = ChatChannelStatus.Normal;
        User2Status = ChatChannelStatus.Normal;

        if (lastMessage != null)
        {
            UpdateLastMessage(lastMessage);
        }
    }

    /// <summary>
    /// 创建系统频道
    /// </summary>
    /// <param name="channelId">频道ID</param>
    /// <param name="channelName">频道名称</param>
    public ChatChannel(string channelId, string channelName)
    {
        ChannelId = channelId;
        ChannelType = ChatChannelType.System;
        ChannelName = channelName;
    }

    /// <summary>
    /// 频道唯一标识
    /// 格式：
    /// - 私聊：private_{smallerUserId}_{largerUserId}
    /// - 系统频道：auction, lobby 等
    /// </summary>
    [StringLength(128)]
    public string ChannelId { get; set; }

    /// <summary>
    /// 频道类型
    /// </summary>
    public ChatChannelType ChannelType { get; set; }

    /// <summary>
    /// 频道名称（主要用于系统频道）
    /// </summary>
    [StringLength(128)]
    public string ChannelName { get; set; }

    /// <summary>
    /// 参与者1的用户ID（私聊时使用，较小的ID）
    /// </summary>
    public long? User1Id { get; set; }

    /// <summary>
    /// 参与者2的用户ID（私聊时使用，较大的ID）
    /// </summary>
    public long? User2Id { get; set; }

    /// <summary>
    /// 用户1的会话状态
    /// Normal=正常显示, Deleted=已删除
    /// </summary>
    public ChatChannelStatus User1Status { get; set; } = ChatChannelStatus.Normal;

    /// <summary>
    /// 用户2的会话状态
    /// Normal=正常显示, Deleted=已删除
    /// </summary>
    public ChatChannelStatus User2Status { get; set; } = ChatChannelStatus.Normal;

    /// <summary>
    /// 最后一条消息ID
    /// </summary>
    public Guid? LastMessageId { get; set; }

    /// <summary>
    /// 最后一条消息内容
    /// </summary>
    [StringLength(2048)]
    public string LastMessageContent { get; set; }

    /// <summary>
    /// 最后一条消息发送者ID
    /// </summary>
    public long? LastMessageFromId { get; set; }

    /// <summary>
    /// 最后一条消息发送者名称
    /// </summary>
    [StringLength(64)]
    public string LastMessageFromName { get; set; }

    /// <summary>
    /// 最后一条消息发送者头像
    /// </summary>
    [StringLength(512)]
    public string LastMessageFromAvatar { get; set; }

    /// <summary>
    /// 最后消息时间戳
    /// </summary>
    public long LastMessageTime { get; set; }

    /// <summary>
    /// 是否激活（用于软删除或隐藏频道）
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 消息总数
    /// </summary>
    public int MessageCount { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 更新最后一条消息信息
    /// </summary>
    /// <param name="message">消息实体</param>
    public void UpdateLastMessage(Message message)
    {
        LastMessageId = message.Id;
        LastMessageContent = message.Msg;
        LastMessageFromId = message.From;
        LastMessageFromName = message.FromName;
        LastMessageFromAvatar = message.Avatar;
        LastMessageTime = message.Time;
        LastModificationTime = DateTime.Now;
        MessageCount++;

        // 当有新消息时，自动恢复两个用户的会话状态
        if (ChannelType == ChatChannelType.Private)
        {
            User1Status = ChatChannelStatus.Normal;
            User2Status = ChatChannelStatus.Normal;
        }
    }

    /// <summary>
    /// 检查用户是否属于这个频道
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public bool IsUserInChannel(long userId)
    {
        return ChannelType switch
        {
            ChatChannelType.Private => User1Id == userId || User2Id == userId,
            ChatChannelType.System => true, // 系统频道所有人都能看到
            _ => false
        };
    }

    /// <summary>
    /// 获取对方用户ID（私聊时使用）
    /// </summary>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>对方用户ID</returns>
    public long? GetOtherUserId(long currentUserId)
    {
        if (ChannelType != ChatChannelType.Private) return null;

        return User1Id == currentUserId ? User2Id :
               User2Id == currentUserId ? User1Id : null;
    }

    /// <summary>
    /// 获取用户在频道中的状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户的会话状态</returns>
    public ChatChannelStatus GetUserStatus(long userId)
    {
        if (ChannelType == ChatChannelType.System)
            return ChatChannelStatus.Normal;

        if (User1Id == userId)
            return User1Status;

        if (User2Id == userId)
            return User2Status;

        return ChatChannelStatus.Normal;
    }

    /// <summary>
    /// 设置用户在频道中的状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="status">状态</param>
    public void SetUserStatus(long userId, ChatChannelStatus status)
    {
        if (ChannelType == ChatChannelType.System)
            return;

        if (User1Id == userId)
        {
            User1Status = status;
        }
        else if (User2Id == userId)
        {
            User2Status = status;
        }

        LastModificationTime = DateTime.Now;
    }

    /// <summary>
    /// 检查用户是否能看到这个频道
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可见</returns>
    public bool IsVisibleToUser(long userId)
    {
        if (!IsActive || !IsUserInChannel(userId))
            return false;

        if (ChannelType == ChatChannelType.System)
            return true;

        return GetUserStatus(userId) == ChatChannelStatus.Normal;
    }
}

/// <summary>
/// 聊天频道类型
/// </summary>
public enum ChatChannelType
{
    /// <summary>
    /// 私聊
    /// </summary>
    Private = 1,

    /// <summary>
    /// 系统频道（如拍卖、大厅等）
    /// </summary>
    System = 2,

    /// <summary>
    /// 群聊（预留）
    /// </summary>
    Group = 3
}

/// <summary>
/// 用户会话状态
/// </summary>
public enum ChatChannelStatus
{
    /// <summary>
    /// 正常显示
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 已删除（对用户隐藏）
    /// </summary>
    Deleted = 1,

    /// <summary>
    /// 已置顶（预留）
    /// </summary>
    Pinned = 2,

    /// <summary>
    /// 已静音（预留）
    /// </summary>
    Muted = 3
}
