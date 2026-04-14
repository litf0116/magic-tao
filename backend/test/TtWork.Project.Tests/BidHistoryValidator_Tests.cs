using System.Linq;
using FluentValidation;
using TtWork.Project.Domains;
using Xunit;

namespace TtWork.Project.Tests;

/// <summary>
/// Tests for BidHistoryValidator fix: BidUserName and BidUserAvatar should NOT be required from client
/// since they are set server-side from user cache in AuctionItemAppService.Bid()
/// </summary>
public class BidHistoryValidator_Tests
{
    private readonly BidHistoryValidator _validator;

    public BidHistoryValidator_Tests()
    {
        _validator = new BidHistoryValidator();
    }

    [Fact]
    public void Should_Pass_When_BidUserName_And_BidUserAvatar_Are_Empty()
    {
        // Arrange: Client only sends auctionItemId and bidPrice
        var dto = new BidHistoryCreateDto
        {
            AuctionItemId = 1,
            BidPrice = 100,
            BidUserName = null,
            BidUserAvatar = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert: Should pass validation (BidUserName/BidUserAvatar are set server-side)
        Assert.True(result.IsValid, $"Validation failed: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Fact]
    public void Should_Pass_When_BidUserName_And_BidUserAvatar_Are_Empty_Strings()
    {
        // Arrange
        var dto = new BidHistoryCreateDto
        {
            AuctionItemId = 1,
            BidPrice = 100,
            BidUserName = "",
            BidUserAvatar = ""
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid, $"Validation failed: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Fact]
    public void Should_Fail_When_AuctionItemId_Is_Zero()
    {
        // Arrange
        var dto = new BidHistoryCreateDto
        {
            AuctionItemId = 0,
            BidPrice = 100
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AuctionItemId");
    }

    [Fact]
    public void Should_Fail_When_BidPrice_Is_Zero()
    {
        // Arrange
        var dto = new BidHistoryCreateDto
        {
            AuctionItemId = 1,
            BidPrice = 0
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BidPrice");
    }

    [Fact]
    public void Should_Pass_With_Valid_BidUserName_And_BidUserAvatar()
    {
        // Arrange
        var dto = new BidHistoryCreateDto
        {
            AuctionItemId = 1,
            BidPrice = 100,
            BidUserName = "testuser",
            BidUserAvatar = "http://example.com/avatar.jpg"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid, $"Validation failed: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }
}
