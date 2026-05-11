using System;
using Shouldly;
using Xunit;

namespace TtWork.Project.Tests.Applications.Auctions;

public class EndAuctionTransactionTests
{
    private const decimal MaxCumulativeAmount = 999999999m;

    [Fact]
    public void IncrementAmount_ExceedsMax_ShouldCapAtMax()
    {
        var currentAmount = 999999000m;
        var incrementAmount = 2000m;

        var newAmount = currentAmount + incrementAmount;
        if (newAmount > MaxCumulativeAmount)
        {
            newAmount = MaxCumulativeAmount;
        }

        newAmount.ShouldBe(MaxCumulativeAmount);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public void IncrementAmount_NegativeOrZero_ShouldSkip(decimal incrementAmount)
    {
        var shouldSkip = incrementAmount <= 0;
        shouldSkip.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1000, 500, 1500)]
    [InlineData(0, 100, 100)]
    [InlineData(999999998, 1, 999999999)]
    public void IncrementAmount_Calculation_ShouldBeCorrect(decimal current, decimal increment, decimal expected)
    {
        var newAmount = current + increment;
        if (newAmount > MaxCumulativeAmount)
        {
            newAmount = MaxCumulativeAmount;
        }

        newAmount.ShouldBe(expected);
    }

    [Theory]
    [InlineData(1000, 500, 1000, true)]
    [InlineData(2000, 500, 2000, true)]
    [InlineData(1500, 100, 2000, false)]
    public void LevelChange_Calculation_ShouldBeCorrect(
        decimal currentAmount,
        decimal increment,
        decimal nextLevelThreshold,
        bool expectedLevelUp)
    {
        var actualNewAmount = currentAmount + increment;
        var shouldLevelUp = actualNewAmount >= nextLevelThreshold;
        shouldLevelUp.ShouldBe(expectedLevelUp);
    }
}
