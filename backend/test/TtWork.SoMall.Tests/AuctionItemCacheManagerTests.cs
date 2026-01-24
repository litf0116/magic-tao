using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using TtWork.Abp.Applications.Dtos;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;
using TtWork.Project.Services.Cache;
using Xunit;

namespace TtWork.SoMall.Tests;

/// <summary>
/// AuctionItemCacheManager 单元测试
/// 测试 HybridCache 两层缓存架构（本地内存 + Redis 分布式缓存）
/// </summary>
public class AuctionItemCacheManagerTests
{
    private readonly Mock<IRedisClient> _redisClientMock;
    private readonly Mock<IRepository<AuctionItem, long>> _auctionItemRepositoryMock;
    private readonly Mock<IRepository<BidHistory, long>> _bidHistoryRepositoryMock;
    private readonly Mock<IObjectMapper> _objectMapperMock;
    private readonly Mock<ILogger<AuctionItemCacheManager>> _loggerMock;

    public AuctionItemCacheManagerTests()
    {
        _redisClientMock = new Mock<IRedisClient>();
        _auctionItemRepositoryMock = new Mock<IRepository<AuctionItem, long>>();
        _bidHistoryRepositoryMock = new Mock<IRepository<BidHistory, long>>();
        _objectMapperMock = new Mock<IObjectMapper>();
        _loggerMock = new Mock<ILogger<AuctionItemCacheManager>>();
    }

    private AuctionItemCacheManager CreateCacheManager()
    {
        // Create a real HybridCache instance using the actual DI container pattern
        // This requires IDistributedCache which is part of ASP.NET Core shared framework
        // For unit tests, we use a mock approach that doesn't require the full infrastructure
        var mockCache = new Mock<HybridCache>(MockBehavior.Loose, null!, null!, null!);
        return new AuctionItemCacheManager(
            _redisClientMock.Object,
            _auctionItemRepositoryMock.Object,
            _bidHistoryRepositoryMock.Object,
            _objectMapperMock.Object,
            _loggerMock.Object,
            mockCache.Object);
    }

    [Fact]
    public void Constructor_CreatesValidInstance()
    {
        // Arrange & Act
        var cacheManager = CreateCacheManager();

        // Assert
        cacheManager.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetCacheStatsAsync_ReturnsValidStats()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act
        var stats = await cacheManager.GetCacheStatsAsync();

        // Assert
        stats.ShouldNotBeNull();
    }

    [Fact]
    public async Task WarmupCacheAsync_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.WarmupCacheAsync());
    }

    [Fact]
    public async Task RebuildAllCacheAsync_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.RebuildAllCacheAsync());
    }

    [Fact]
    public async Task SetAuctionDetailCacheAsync_NullItem_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.SetAuctionDetailCacheAsync(null!));
    }

    [Fact]
    public async Task SetAuctionDetailCacheAsync_ValidItem_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();
        var auctionItem = new AuctionItemDto { Id = 1L, Name = "Test" };

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.SetAuctionDetailCacheAsync(auctionItem));
    }

    [Fact]
    public async Task SetAuctionListCacheAsync_NullResult_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.SetAuctionListCacheAsync(
            new AppResultRequestDto { MaxResultCount = 10 }, null!));
    }

    [Fact]
    public async Task SetAuctionListCacheAsync_ValidResult_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();
        var input = new AppResultRequestDto { MaxResultCount = 10 };
        var result = new ListResultDto<AuctionItemDto>(new List<AuctionItemDto>());

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.SetAuctionListCacheAsync(input, result));
    }

    [Fact]
    public async Task ClearAuctionCacheAsync_NullId_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearAuctionCacheAsync(null));
    }

    [Fact]
    public async Task ClearAuctionCacheAsync_ValidId_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearAuctionCacheAsync(1L));
    }

    [Fact]
    public async Task ClearAuctionListCacheAsync_NullStatus_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearAuctionListCacheAsync(null));
    }

    [Fact]
    public async Task ClearAuctionDetailCacheAsync_ValidId_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearAuctionDetailCacheAsync(1L));
    }

    [Fact]
    public async Task ClearCurrentAuctionCacheAsync_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearCurrentAuctionCacheAsync());
    }

    [Fact]
    public async Task ClearAllAuctionCacheAsync_DoesNotThrow()
    {
        // Arrange
        var cacheManager = CreateCacheManager();

        // Act & Assert
        Should.NotThrow(async () => await cacheManager.ClearAllAuctionCacheAsync());
    }
}
