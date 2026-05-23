using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Moq;
using Shouldly;
using TtWork.Project.Applications;
using TtWork.Project.Domains;
using TtWork.Project.Services;
using Xunit;

namespace TtWork.SoMall.Tests;

/// <summary>
/// ChatChannelService 单元测试
/// 测试聊天频道删除和恢复功能
/// </summary>
public class ChatChannelServiceTests
{
    private readonly Mock<IRepository<ChatChannel, long>> _chatChannelRepositoryMock;
    private readonly Mock<IRepository<Message, Guid>> _messageRepositoryMock;
    private readonly Mock<IRepository<ChatListDelete, int>> _chatListDeleteRepositoryMock;
    private ChatChannelService _service;

    public ChatChannelServiceTests()
    {
        _chatChannelRepositoryMock = new Mock<IRepository<ChatChannel, long>>();
        _messageRepositoryMock = new Mock<IRepository<Message, Guid>>();
        _chatListDeleteRepositoryMock = new Mock<IRepository<ChatListDelete, int>>();
        _service = new ChatChannelService(
            _chatChannelRepositoryMock.Object,
            _messageRepositoryMock.Object,
            _chatListDeleteRepositoryMock.Object);
    }

    [Fact]
    public async Task RestoreUserChannelAsync_ChannelIsDeleted_RestoresUserStatus()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channelId = "private_123_456";
        var channel = new ChatChannel(userId, otherUserId)
        {
            Id = 1L,
            ChannelId = channelId
        };
        channel.SetUserStatus(userId, ChatChannelStatus.Deleted);

        _chatChannelRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ChatChannel, bool>>>()))
            .ReturnsAsync((ChatChannel?)channel);

        // Act
        await _service.RestoreUserChannelAsync(userId, otherUserId);

        // Assert
        channel.GetUserStatus(userId).ShouldBe(ChatChannelStatus.Normal);
        channel.GetUserStatus(otherUserId).ShouldBe(ChatChannelStatus.Normal);
        _chatChannelRepositoryMock.Verify(x => x.UpdateAsync(channel), Times.Once);
    }

    [Fact]
    public void PrivateChannelId_SmallerIdFirst_CreatesCorrectId()
    {
        // Arrange
        long userId1 = 456L;
        long userId2 = 123L;

        // Act
        var channel = new ChatChannel(userId1, userId2);

        // Assert
        channel.ChannelId.ShouldBe("private_123_456");
    }

    [Fact]
    public void PrivateChannelId_SameOrder_CreatesCorrectId()
    {
        // Arrange
        long userId1 = 123L;
        long userId2 = 456L;

        // Act
        var channel = new ChatChannel(userId1, userId2);

        // Assert
        channel.ChannelId.ShouldBe("private_123_456");
    }

    [Fact]
    public void PrivateChannelId_SameUsers_CreatesSameId()
    {
        // Arrange
        long userId1 = 123L;
        long userId2 = 456L;

        // Act
        var channel1 = new ChatChannel(userId1, userId2);
        var channel2 = new ChatChannel(userId2, userId1);

        // Assert
        channel1.ChannelId.ShouldBe(channel2.ChannelId);
    }

    [Fact]
    public void IsUserInChannel_PrivateChannel_UserIsParticipant_ReturnsTrue()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);

        // Act
        var result = channel.IsUserInChannel(userId);

        // Assert
        result.ShouldBe(true);
    }

    [Fact]
    public void IsUserInChannel_PrivateChannel_UserNotParticipant_ReturnsFalse()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);

        // Act
        var result = channel.IsUserInChannel(789L);

        // Assert
        result.ShouldBe(false);
    }

    [Fact]
    public void IsUserInChannel_SystemChannel_ReturnsTrue()
    {
        // Arrange
        var channel = new ChatChannel("auction", "拍卖频道");

        // Act
        var result = channel.IsUserInChannel(123L);

        // Assert
        result.ShouldBe(true);
    }

    [Fact]
    public void GetOtherUserId_PrivateChannel_ReturnsOtherUser()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);

        // Act
        var result = channel.GetOtherUserId(userId);

        // Assert
        result.ShouldBe(otherUserId);
    }

    [Fact]
    public void GetOtherUserId_PrivateChannel_Reverse_ReturnsOtherUser()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);

        // Act
        var result = channel.GetOtherUserId(otherUserId);

        // Assert
        result.ShouldBe(userId);
    }

    [Fact]
    public void GetOtherUserId_SystemChannel_ReturnsNull()
    {
        // Arrange
        var channel = new ChatChannel("auction", "拍卖频道");

        // Act
        var result = channel.GetOtherUserId(123L);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void SetUserStatus_PrivateChannel_UpdatesCorrectUser()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);

        // Act
        channel.SetUserStatus(userId, ChatChannelStatus.Deleted);

        // Assert
        channel.GetUserStatus(userId).ShouldBe(ChatChannelStatus.Deleted);
        channel.GetUserStatus(otherUserId).ShouldBe(ChatChannelStatus.Normal);
    }

    [Fact]
    public void UpdateLastMessage_PrivateChannel_RestoresBothUsersStatus()
    {
        // Arrange
        long userId = 123L;
        long otherUserId = 456L;
        var channel = new ChatChannel(userId, otherUserId);
        channel.SetUserStatus(userId, ChatChannelStatus.Deleted);
        channel.SetUserStatus(otherUserId, ChatChannelStatus.Deleted);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            Msg = "测试消息",
            From = userId
        };
        // 使用反射设置受保护的 Time 属性
        var timeProperty = typeof(Message).GetProperty("Time", BindingFlags.Public | BindingFlags.Instance);
        timeProperty?.SetValue(message, DateTimeOffset.Now.ToUnixTimeSeconds());

        // Act
        channel.UpdateLastMessage(message);

        // Assert
        channel.GetUserStatus(userId).ShouldBe(ChatChannelStatus.Normal);
        channel.GetUserStatus(otherUserId).ShouldBe(ChatChannelStatus.Normal);
    }
}

/// <summary>
/// 聊天列表排序测试
/// </summary>
public class ChatListSortTests
{
    [Fact]
    public void ChatList_SortedByOrderThenTime_ReturnsCorrectOrder()
    {
        // Arrange
        var chatList = new List<ChatListItem>
        {
            new ChatListItem { id = 1, order = 1, time = 1000, name = "用户1" },
            new ChatListItem { id = 2, order = 2, time = 2000, name = "用户2" },
            new ChatListItem { id = 3, order = 1, time = 3000, name = "用户3" },
            new ChatListItem { id = 4, order = 2, time = 1000, name = "用户4" }
        };

        // Act
        var sortedList = chatList
            .OrderByDescending(x => x.order)
            .ThenByDescending(x => x.time)
            .ToList();

        // Assert
        sortedList[0].id.ShouldBe(2); // order=2, time=2000
        sortedList[1].id.ShouldBe(4); // order=2, time=1000
        sortedList[2].id.ShouldBe(3); // order=1, time=3000
        sortedList[3].id.ShouldBe(1); // order=1, time=1000
    }

    [Fact]
    public void ChatList_SameOrderDifferentTime_SortsByTime()
    {
        // Arrange
        var chatList = new List<ChatListItem>
        {
            new ChatListItem { id = 1, order = 1, time = 1000, name = "用户1" },
            new ChatListItem { id = 2, order = 1, time = 3000, name = "用户2" },
            new ChatListItem { id = 3, order = 1, time = 2000, name = "用户3" }
        };

        // Act
        var sortedList = chatList
            .OrderByDescending(x => x.order)
            .ThenByDescending(x => x.time)
            .ToList();

        // Assert
        sortedList[0].id.ShouldBe(2); // time=3000
        sortedList[1].id.ShouldBe(3); // time=2000
        sortedList[2].id.ShouldBe(1); // time=1000
    }

    [Fact]
    public void ChatList_DifferentOrderSameTime_SortsByOrder()
    {
        // Arrange
        var chatList = new List<ChatListItem>
        {
            new ChatListItem { id = 1, order = 1, time = 2000, name = "用户1" },
            new ChatListItem { id = 2, order = 3, time = 2000, name = "用户2" },
            new ChatListItem { id = 3, order = 2, time = 2000, name = "用户3" }
        };

        // Act
        var sortedList = chatList
            .OrderByDescending(x => x.order)
            .ThenByDescending(x => x.time)
            .ToList();

        // Assert
        sortedList[0].id.ShouldBe(2); // order=3
        sortedList[1].id.ShouldBe(3); // order=2
        sortedList[2].id.ShouldBe(1); // order=1
    }
}
