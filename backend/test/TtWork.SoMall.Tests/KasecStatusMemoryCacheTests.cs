using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using StackExchange.Redis;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.Auctions;
using Xunit;

namespace TtWork.SoMall.Tests;

public class KasecStatusMemoryCacheTests
{
    private const string KASEC_CACHE_PREFIX = "Kasec:";
    private readonly Mock<IRedisClient> _redisClientMock;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuctionItemAppService> _logger;

    public KasecStatusMemoryCacheTests()
    {
        _redisClientMock = new Mock<IRedisClient>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisClientMock.Setup(x => x.Database).Returns(_redisDatabaseMock.Object);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = Mock.Of<ILogger<AuctionItemAppService>>();
    }

    [Fact]
    public async Task SetKasecStatus_TrueValue_UpdatesBothRedisAndMemoryCache()
    {
        var auctionItemId = 111L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        await SetKasecStatusToCache(auctionItemId, true);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:111", "true", null, When.Always, CommandFlags.None),
            Times.Once);

        _memoryCache.TryGetValue(cacheKey, out var cachedValue).ShouldBe(true);
        ((bool)cachedValue!).ShouldBe(true);
    }

    [Fact]
    public async Task GetKasecStatus_CacheMiss_FetchesFromRedis()
    {
        var auctionItemId = 456L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";
        var redisValue = new RedisValue("true");

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"{KASEC_CACHE_PREFIX.Replace("Kasec:", "Auction:Kasec:")}{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(redisValue);

        var result = await GetKasecStatusFromCache(auctionItemId);

        result.ShouldBe(true);
        _memoryCache.TryGetValue(cacheKey, out var cachedValue).ShouldBe(true);
        ((bool)cachedValue!).ShouldBe(true);
    }

    [Fact]
    public async Task GetKasecStatus_RedisValueIsFalse_ReturnsFalse()
    {
        var auctionItemId = 789L;
        var redisValue = new RedisValue("false");

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"{KASEC_CACHE_PREFIX.Replace("Kasec:", "Auction:Kasec:")}{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(redisValue);

        var result = await GetKasecStatusFromCache(auctionItemId);

        result.ShouldBe(false);
    }

    [Fact]
    public async Task GetKasecStatus_RedisValueNotExists_ReturnsFalse()
    {
        var auctionItemId = 999L;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"{KASEC_CACHE_PREFIX.Replace("Kasec:", "Auction:Kasec:")}{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(RedisValue.Null);

        var result = await GetKasecStatusFromCache(auctionItemId);

        result.ShouldBe(false);
    }

    [Fact]
    public async Task SetKasecStatus_UpdatesBothRedisAndMemoryCache()
    {
        var auctionItemId = 111L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        await SetKasecStatusToCache(auctionItemId, true);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:111", "true", null, When.Always, CommandFlags.None),
            Times.Once);

        _memoryCache.TryGetValue(cacheKey, out var cachedValue).ShouldBe(true);
        ((bool)cachedValue!).ShouldBe(true);
    }

    [Fact]
    public async Task SetKasecStatus_ToFalse_UpdatesCacheCorrectly()
    {
        var auctionItemId = 222L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        _memoryCache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        await SetKasecStatusToCache(auctionItemId, false);

        _redisDatabaseMock.Verify(
            x => x.StringSetAsync("Auction:Kasec:222", "false", null, When.Always, CommandFlags.None),
            Times.Once);

        _memoryCache.TryGetValue(cacheKey, out var cachedValue).ShouldBe(true);
        ((bool)cachedValue!).ShouldBe(false);
    }

    [Fact]
    public async Task Cache_Expires_After_5_Seconds()
    {
        var auctionItemId = 333L;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";
        var redisValue = new RedisValue("true");

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"{KASEC_CACHE_PREFIX.Replace("Kasec:", "Auction:Kasec:")}{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(redisValue);

        var result1 = await GetKasecStatusFromCache(auctionItemId);
        result1.ShouldBe(true);

        _memoryCache.TryGetValue(cacheKey, out _).ShouldBe(true);

        await Task.Delay(5100);

        _memoryCache.TryGetValue(cacheKey, out var cachedAfterExpiry).ShouldBe(false);
    }

    [Fact]
    public async Task Concurrent_GetKasecStatus_OnlyOneRedisCall()
    {
        var auctionItemId = 444L;
        var redisValue = new RedisValue("true");
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"{KASEC_CACHE_PREFIX.Replace("Kasec:", "Auction:Kasec:")}{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(redisValue);

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => GetKasecStatusFromCache(auctionItemId))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.All(r => r == true).ShouldBe(true);
        _redisDatabaseMock.Verify(
            x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    private async Task<bool> GetKasecStatusFromCache(long auctionItemId)
    {
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        if (_memoryCache.TryGetValue(cacheKey, out bool cachedValue))
        {
            return cachedValue;
        }

        var val = await _redisDatabaseMock.Object.StringGetAsync($"Auction:Kasec:{auctionItemId}");
        bool result = val.HasValue && val == "true";

        _memoryCache.Set(cacheKey, result, TimeSpan.FromSeconds(5));

        return result;
    }

    private async Task SetKasecStatusToCache(long auctionItemId, bool isKasec)
    {
        var kasecKey = $"Auction:Kasec:{auctionItemId}";
        var kasecValue = isKasec.ToString().ToLower();
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        await _redisDatabaseMock.Object.StringSetAsync(kasecKey, kasecValue, null, When.Always, CommandFlags.None);
        _memoryCache.Set(cacheKey, isKasec, TimeSpan.FromSeconds(5));
    }
}
