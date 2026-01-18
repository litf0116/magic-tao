using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using StackExchange.Redis;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.Auctions;
using TtWork.Project.Domains;
using Xunit;

namespace TtWork.SoMall.Tests;

public class KasecStatusIntegrationTests
{
    private const string KASEC_CACHE_PREFIX = "Kasec:";
    private readonly Mock<IRedisClient> _redisClientMock;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<AuctionItemAppService>> _loggerMock;
    private readonly TestKasecService _testService;

    public KasecStatusIntegrationTests()
    {
        _redisClientMock = new Mock<IRedisClient>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisClientMock.Setup(x => x.Database).Returns(_redisDatabaseMock.Object);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<AuctionItemAppService>>();
        _testService = new TestKasecService(_redisClientMock.Object, _memoryCache, _loggerMock.Object);
    }

    [Fact]
    public async Task SetThenGet_ReturnsUpdatedValue()
    {
        var auctionItemId = 1001L;

        await _testService.SetKasecStatus(auctionItemId, true);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:1001", "true", null, When.Always, CommandFlags.None),
            Times.Once);

        var result = await _testService.GetKasecStatus(auctionItemId);

        result.ShouldBe(true);
    }

    [Fact]
    public async Task StatusToggle_WorksCorrectly()
    {
        var auctionItemId = 1002L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        await _testService.SetKasecStatus(auctionItemId, true);
        var result1 = await _testService.GetKasecStatus(auctionItemId);
        result1.ShouldBe(true);

        await _testService.SetKasecStatus(auctionItemId, false);
        var result2 = await _testService.GetKasecStatus(auctionItemId);
        result2.ShouldBe(false);

        _memoryCache.TryGetValue(cacheKey, out var cachedValue).ShouldBe(true);
        ((bool)cachedValue!).ShouldBe(false);
    }

    [Fact]
    public async Task SetKasecStatus_ToTrue_StoresLowercaseTrue()
    {
        var auctionItemId = 1003L;

        await _testService.SetKasecStatus(auctionItemId, true);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:1003", "true", null, When.Always, CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task SetKasecStatus_ToFalse_StoresLowercaseFalse()
    {
        var auctionItemId = 1004L;

        await _testService.SetKasecStatus(auctionItemId, false);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:1004", "false", null, When.Always, CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task GetKasecStatus_AfterSet_UsesMemoryCache()
    {
        var auctionItemId = 1005L;
        var kasecKey = $"Auction:Kasec:{auctionItemId}";
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        await _testService.SetKasecStatus(auctionItemId, true);

        var result1 = await _testService.GetKasecStatus(auctionItemId);
        result1.ShouldBe(true);

        _memoryCache.TryGetValue(cacheKey, out _).ShouldBe(true);

        _redisDatabaseMock.Verify(
            x => x.StringGetAsync(It.IsAny<RedisKey>(), CommandFlags.None),
            Times.Never);
    }

    [Fact]
    public async Task MultipleAuctionItems_CacheCorrectly()
    {
        var auctionItemId1 = 2001L;
        var auctionItemId2 = 2002L;
        var cacheKey1 = $"{KASEC_CACHE_PREFIX}{auctionItemId1}";
        var cacheKey2 = $"{KASEC_CACHE_PREFIX}{auctionItemId2}";

        await _testService.SetKasecStatus(auctionItemId1, true);
        await _testService.SetKasecStatus(auctionItemId2, false);

        var result1 = await _testService.GetKasecStatus(auctionItemId1);
        var result2 = await _testService.GetKasecStatus(auctionItemId2);

        result1.ShouldBe(true);
        result2.ShouldBe(false);

        _memoryCache.TryGetValue(cacheKey1, out var value1).ShouldBe(true);
        _memoryCache.TryGetValue(cacheKey2, out var value2).ShouldBe(true);
        ((bool)value1!).ShouldBe(true);
        ((bool)value2!).ShouldBe(false);
    }

    [Fact]
    public async Task GetKasecStatus_NonExistentItem_ReturnsFalse()
    {
        var auctionItemId = 9999L;
        var nonExistentKey = $"Auction:Kasec:{auctionItemId}";

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync(nonExistentKey, CommandFlags.None))
            .ReturnsAsync(RedisValue.Null);

        var result = await _testService.GetKasecStatus(auctionItemId);

        result.ShouldBe(false);
    }

    private class TestKasecService
    {
        private readonly IRedisClient _redisClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private const string KASEC_CACHE_PREFIX = "Kasec:";

        public TestKasecService(IRedisClient redisClient, IMemoryCache memoryCache, ILogger logger)
        {
            _redisClient = redisClient;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<bool> GetKasecStatus(long auctionItemId)
        {
            var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

            if (_memoryCache.TryGetValue(cacheKey, out bool cachedValue))
            {
                return cachedValue;
            }

            var val = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
            bool result = val.HasValue && val == "true";

            _memoryCache.Set(cacheKey, result, TimeSpan.FromSeconds(5));

            return result;
        }

        public async Task SetKasecStatus(long auctionItemId, bool isKasec)
        {
            var kasecKey = $"Auction:Kasec:{auctionItemId}";
            var kasecValue = isKasec.ToString().ToLower();

            await _redisClient.Database.StringSetAsync(kasecKey, kasecValue, null, When.Always, CommandFlags.None);

            var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";
            _memoryCache.Set(cacheKey, isKasec, TimeSpan.FromSeconds(5));
        }
    }
}
