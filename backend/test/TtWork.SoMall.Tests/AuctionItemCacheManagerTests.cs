using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using StackExchange.Redis;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Entity;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;
using TtWork.Project.Services.Cache;
using Xunit;

namespace TtWork.SoMall.Tests;

/// <summary>
/// AuctionItemCacheManager 单元测试
/// 测试拍卖品列表缓存的两层缓存架构
/// </summary>
public class AuctionItemCacheManagerTests : IDisposable
{
    private readonly Mock<IRedisClient> _redisClientMock;
    private readonly Mock<IRepository<AuctionItem, long>> _auctionItemRepositoryMock;
    private readonly Mock<IRepository<BidHistory, long>> _bidHistoryRepositoryMock;
    private readonly Mock<IObjectMapper> _objectMapperMock;
    private readonly Mock<ILogger<AuctionItemCacheManager>> _loggerMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private AuctionItemCacheManager _cacheManager;

    public AuctionItemCacheManagerTests()
    {
        _redisClientMock = new Mock<IRedisClient>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisClientMock.Setup(x => x.Database).Returns(_redisDatabaseMock.Object);

        _auctionItemRepositoryMock = new Mock<IRepository<AuctionItem, long>>();
        _bidHistoryRepositoryMock = new Mock<IRepository<BidHistory, long>>();
        _objectMapperMock = new Mock<IObjectMapper>();
        _loggerMock = new Mock<ILogger<AuctionItemCacheManager>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _cacheManager = new AuctionItemCacheManager(
            _redisClientMock.Object,
            _auctionItemRepositoryMock.Object,
            _bidHistoryRepositoryMock.Object,
            _objectMapperMock.Object,
            _loggerMock.Object,
            _memoryCache);
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }

    [Fact]
    public async Task ClearAuctionCacheAsync_ClearsSpecificItem()
    {
        // Arrange
        long auctionItemId = 1L;

        // Act
        await _cacheManager.ClearAuctionCacheAsync(auctionItemId);

        // Assert
        _redisDatabaseMock.Verify(
            x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClearAuctionListCacheAsync_ClearsAllLists()
    {
        // Act
        await _cacheManager.ClearAuctionListCacheAsync();

        // Assert
        _redisClientMock.Verify(
            x => x.DeleteKeysWithPartten(It.IsAny<string>()),
            Times.AtLeastOnce);
    }
}



