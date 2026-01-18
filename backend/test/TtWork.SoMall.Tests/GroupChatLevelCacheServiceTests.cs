using Shouldly;
using TtWork.Abp.Entity;
using Xunit;

namespace TtWork.SoMall.Tests;

public class GroupChatLevelCalculationTests
{
    /// <summary>
    /// 测试等级计算的核心逻辑，不依赖缓存和数据库
    /// 这个测试直接验证根据累计金额计算正确等级的逻辑
    /// </summary>
    [Fact]
    public void CalculateLevel_WithAmount264849_ShouldReturnLevel7()
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 6, Level = 5, AmountRequired = 38888, Name = "诅咒迷宫の双王" },
            new() { Id = 7, Level = 6, AmountRequired = 88888, Name = "龙之沙漏の里雍" },
            new() { Id = 8, Level = 7, AmountRequired = 158888, Name = "军神の李贝留斯" },
            new() { Id = 9, Level = 8, AmountRequired = 308888, Name = "主神の阿尔杰斯" }
        };

        var result = CalculateCorrectLevel(settings, 264849);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(8);
        result.Level.ShouldBe(7);
        result.Name.ShouldBe("军神の李贝留斯");
    }

    [Fact]
    public void CalculateLevel_WithAmount62492_ShouldReturnLevel5()
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" },
            new() { Id = 5, Level = 4, AmountRequired = 11888, Name = "圣坛の犹大" },
            new() { Id = 6, Level = 5, AmountRequired = 38888, Name = "诅咒迷宫の双王" },
            new() { Id = 7, Level = 6, AmountRequired = 88888, Name = "龙之沙漏の里雍" }
        };

        var result = CalculateCorrectLevel(settings, 62492);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(6);
        result.Level.ShouldBe(5);
        result.Name.ShouldBe("诅咒迷宫の双王");
    }

    [Fact]
    public void CalculateLevel_WithAmount0_ShouldReturnLevel0()
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" }
        };

        var result = CalculateCorrectLevel(settings, 0);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
        result.Level.ShouldBe(0);
    }

    [Fact]
    public void CalculateLevel_WithAmountLessThanFirstThreshold_ShouldReturnLowestLevel()
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" }
        };

        var result = CalculateCorrectLevel(settings, 50);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
        result.Level.ShouldBe(0);
    }

    [Fact]
    public void CalculateLevel_WithAmountMoreThanMaxThreshold_ShouldReturnHighestLevel()
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" },
            new() { Id = 8, Level = 7, AmountRequired = 158888, Name = "军神の李贝留斯" },
            new() { Id = 9, Level = 8, AmountRequired = 308888, Name = "主神の阿尔杰斯" }
        };

        var result = CalculateCorrectLevel(settings, 500000);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(9);
        result.Level.ShouldBe(8);
        result.Name.ShouldBe("主神の阿尔杰斯");
    }

    [Fact]
    public void CalculateLevel_WithEmptySettings_ShouldReturnNull()
    {
        var settings = new List<GroupChatLevelSettingsEntity>();

        var result = CalculateCorrectLevel(settings, 1000);

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(88, 2, "哈洞の殴兹那克")]
    [InlineData(888, 3, "黑色祈祷の露比")]
    [InlineData(5888, 4, "贝兹雷姆の神兽")]
    [InlineData(11888, 5, "圣坛の犹大")]
    [InlineData(38888, 6, "诅咒迷宫の双王")]
    [InlineData(88888, 7, "龙之沙漏の里雍")]
    [InlineData(158888, 8, "军神の李贝留斯")]
    public void CalculateLevel_ShouldMatchExpectedLevel(decimal amount, int expectedLevelId, string expectedName)
    {
        var settings = new List<GroupChatLevelSettingsEntity>
        {
            new() { Id = 1, Level = 0, AmountRequired = 0, Name = "普通用户" },
            new() { Id = 2, Level = 1, AmountRequired = 88, Name = "哈洞の殴兹那克" },
            new() { Id = 3, Level = 2, AmountRequired = 888, Name = "黑色祈祷の露比" },
            new() { Id = 4, Level = 3, AmountRequired = 5888, Name = "贝兹雷姆の神兽" },
            new() { Id = 5, Level = 4, AmountRequired = 11888, Name = "圣坛の犹大" },
            new() { Id = 6, Level = 5, AmountRequired = 38888, Name = "诅咒迷宫の双王" },
            new() { Id = 7, Level = 6, AmountRequired = 88888, Name = "龙之沙漏の里雍" },
            new() { Id = 8, Level = 7, AmountRequired = 158888, Name = "军神の李贝留斯" },
            new() { Id = 9, Level = 8, AmountRequired = 308888, Name = "主神の阿尔杰斯" }
        };

        var result = CalculateCorrectLevel(settings, amount);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(expectedLevelId);
        result.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// 核心计算逻辑：找到小于等于累计金额的最大阈值配置
    /// 这个逻辑与 GroupChatLevelCacheService.GetCorrectLevel 保持一致
    /// </summary>
    private static GroupChatLevelSettingsEntity? CalculateCorrectLevel(
        List<GroupChatLevelSettingsEntity> settings,
        decimal cumulativeAmount)
    {
        if (settings.Count == 0)
            return null;

        return settings
            .Where(w => w.AmountRequired <= cumulativeAmount)
            .OrderByDescending(o => o.AmountRequired)
            .FirstOrDefault() ?? settings.Last();
    }
}
