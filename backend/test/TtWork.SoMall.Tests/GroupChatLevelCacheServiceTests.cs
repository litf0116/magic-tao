using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using TtWork.Abp.Entity;
using TtWork.Project.Caches;
using Xunit;

namespace TtWork.SoMall.Tests;

/// <summary>
/// GroupChatLevelCacheService 单元测试
/// 测试用户等级缓存的自动修正功能
/// </summary>
public class GroupChatLevelCacheServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ISqlSugarClient> _sqlSugarClientMock;
    private readonly Mock<ILogger<GroupChatLevelCacheService>> _loggerMock;
    private GroupChatLevelCacheService _service;

    public GroupChatLevelCacheServiceTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _sqlSugarClientMock = new Mock<ISqlSugarClient>();
        _loggerMock = new Mock<ILogger<GroupChatLevelCacheService>>();
        _service = new GroupChatLevelCacheService(
            _memoryCacheMock.Object,
            _sqlSugarClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetAllSettings_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedSettings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" }
        };
        object? cachedValue = cachedSettings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetAllSettings();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.First().Name.ShouldBe("普通用户");
        _sqlSugarClientMock.Verify(x => x.Queryable<GroupChatLevelSettingsEntity>(), Times.Never);
    }

    [Fact]
    public void GetCorrectLevel_WithAmount888_ReturnsLevel2()
    {
        // Arrange
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 3, Level = 2, AmountRequired = 888, Name = "黑色祈祷の露比" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" },
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" }
        };
        object? cachedValue = settings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetCorrectLevel(888m);

        // Assert
        result.ShouldNotBeNull();
        result!.Level.ShouldBe(2);
        result.Name.ShouldBe("黑色祈祷の露比");
    }

    [Fact]
    public void GetCorrectLevel_WithAmount0_ReturnsLevel0()
    {
        // Arrange
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" },
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" }
        };
        object? cachedValue = settings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetCorrectLevel(0m);

        // Assert
        result.ShouldNotBeNull();
        result!.Level.ShouldBe(0);
        result.Name.ShouldBe("普通用户");
    }

    [Fact]
    public void GetCorrectLevel_WithAmount500_ReturnsLevel1()
    {
        // Arrange
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 3, Level = 2, AmountRequired = 888, Name = "黑色祈祷の露比" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" },
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" }
        };
        object? cachedValue = settings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetCorrectLevel(500m);

        // Assert
        result.ShouldNotBeNull();
        result!.Level.ShouldBe(1);
        result.Name.ShouldBe("哈洞の殴兹那克");
    }

    [Fact]
    public void GetCorrectLevel_WithEmptySettings_ReturnsNull()
    {
        // Arrange
        var settings = new List<GroupChatLevelSettingsEntity>();
        object? cachedValue = settings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetCorrectLevel(1000m);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void InvalidateCache_RemovesCache()
    {
        // Act
        _service.InvalidateCache();

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(88, 2, "哈洞の殴兹那克")]
    [InlineData(888, 3, "黑色祈祷の露比")]
    [InlineData(5888, 4, "贝兹雷姆の神兽")]
    [InlineData(11888, 5, "圣坛の犹大")]
    [InlineData(38888, 6, "诅咒迷宫の双王")]
    [InlineData(88888, 7, "龙之沙漏の里雍")]
    [InlineData(158888, 8, "军神の李贝留斯")]
    public void GetCorrectLevel_ShouldMatchExpectedLevel(decimal amount, int expectedLevelId, string expectedName)
    {
        // Arrange
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 9, Level = 8, AmountRequired = 308888, Name = "主神の阿尔杰斯" },
            new() { Id = 8, Level = 7, AmountRequired = 158888, Name = "军神の李贝留斯" },
            new() { Id = 7, Level = 6, AmountRequired = 88888, Name = "龙之沙漏の里雍" },
            new() { Id = 6, Level = 5, AmountRequired = 38888, Name = "诅咒迷宫の双王" },
            new() { Id = 5, Level = 4, AmountRequired = 11888, Name = "圣坛の犹大" },
            new() { Id = 4, Level = 3, AmountRequired = 5888, Name = "贝兹雷姆の神兽" },
            new() { Id = 3, Level = 2, AmountRequired = 888, Name = "黑色祈祷の露比" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" },
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" }
        };
        object? cachedValue = settings;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<string>(), out cachedValue))
            .Returns(true);

        // Act
        var result = _service.GetCorrectLevel(amount);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(expectedLevelId);
        result.Name.ShouldBe(expectedName);
    }
}
