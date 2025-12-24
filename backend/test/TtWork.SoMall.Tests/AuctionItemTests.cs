using Shouldly;
using TtWork.Project.Domains;
using Xunit;

namespace TtWork.SoMall.Tests;

public class AuctionItemTests
{
    [Fact]
    public void RollBack_Should_Reset_Price_To_PreviousBid()
    {
        var previousBid = new BidHistory
        {
            BidPrice = 100,
            BidUserName = "User1",
            CreatorUserId = 1
        };

        var auctionItem = new AuctionItem
        {
            Name = "Test Item",
            CurrentPrice = 150,
            CurrentPriceUserId = 2,
            CurrentPriceUserName = "User2",
            StartingPrice = 50
        };

        auctionItem.RollBack(previousBid);

        auctionItem.CurrentPrice.ShouldBe(100);
        auctionItem.CurrentPriceUserName.ShouldBe("User1");
        auctionItem.CurrentPriceUserId.ShouldBe(1);
    }

    [Fact]
    public void RollBack_With_Null_PreviousBid_Should_Reset_To_Null()
    {
        var auctionItem = new AuctionItem
        {
            Name = "Test Item",
            CurrentPrice = 100,
            CurrentPriceUserId = 2,
            CurrentPriceUserName = "User2",
            StartingPrice = 50
        };

        auctionItem.RollBack(null);

        auctionItem.CurrentPrice.ShouldBeNull();
        auctionItem.CurrentPriceUserName.ShouldBeNull();
        auctionItem.CurrentPriceUserId.ShouldBeNull();
    }

    [Fact]
    public void SetBid_Should_Update_CurrentPrice()
    {
        var auctionItem = new AuctionItem
        {
            Name = "Test Item",
            CurrentPrice = 50,
            StartingPrice = 50
        };

        auctionItem.SetBid(100, 1, "TestUser");

        auctionItem.CurrentPrice.ShouldBe(100);
        auctionItem.CurrentPriceUserId.ShouldBe(1);
        auctionItem.CurrentPriceUserName.ShouldBe("TestUser");
    }

    [Fact]
    public void AuctionStatus_Should_Be_Correct()
    {
        var auctionItem = new AuctionItem
        {
            Name = "Test Item"
        };
        auctionItem.Status.ShouldBe(AuctionStatusEnum.草稿);

        auctionItem.StartAuction();
        auctionItem.Status.ShouldBe(AuctionStatusEnum.拍卖中);
    }
}
