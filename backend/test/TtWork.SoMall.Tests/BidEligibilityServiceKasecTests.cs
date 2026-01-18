using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using StackExchange.Redis;
using TtWork.Abp.Caches;
using TtWork.Abp.Entity;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;
using TtWork.Project.Services;
using Xunit;

namespace TtWork.SoMall.Tests;

public class BidEligibilityServiceKasecTests
{
    private const string KASEC_CACHE_PREFIX = "Kasec:";
    private readonly Mock<IRedisClient> _redisClientMock;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<BidEligibilityService>> _loggerMock;
    private readonly TestBidEligibilityService _testService;

    public BidEligibilityServiceKasecTests()
    {
        _redisClientMock = new Mock<IRedisClient>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisClientMock.Setup(x => x.Database).Returns(_redisDatabaseMock.Object);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<BidEligibilityService>>();
        _testService = new TestBidEligibilityService(
            _redisClientMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecEnabled_TripleMinimumBid()
    {
        var auctionItemId = 3001L;
        var basePrice = 100m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("true"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false);

        result.IsKasec.ShouldBe(true);
        result.MinBidPrice.ShouldBe(basePrice * 3);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecDisabled_NormalMinimumBid()
    {
        var auctionItemId = 3002L;
        var basePrice = 100m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false);

        result.IsKasec.ShouldBe(false);
        result.MinBidPrice.ShouldBe(basePrice);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecWithCurrentPrice_TripleIncrement()
    {
        var auctionItemId = 3003L;
        var currentPrice = 500m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("true"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.IsKasec.ShouldBe(true);
        var expectedIncrement = 5m;
        var expectedMinPrice = currentPrice + (expectedIncrement * 3);
        result.MinBidPrice.ShouldBe(expectedMinPrice);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecCacheHit_UsesCachedValue()
    {
        var auctionItemId = 3004L;
        var basePrice = 100m;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        _memoryCache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false);

        result.IsKasec.ShouldBe(true);
        _redisDatabaseMock.Verify(
            x => x.StringGetAsync(It.IsAny<RedisKey>(), CommandFlags.None),
            Times.Never);
    }

    [Fact]
    public async Task CheckBidEligibility_FirstBid_KasecEnabled()
    {
        var auctionItemId = 3005L;
        var basePrice = 50m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("true"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false);

        result.IsKasec.ShouldBe(true);
        result.MinBidPrice.ShouldBe(basePrice * 3);
    }

    [Fact]
    public async Task CheckBidEligibility_FirstBid_KasecDisabled()
    {
        var auctionItemId = 3006L;
        var basePrice = 50m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false);

        result.IsKasec.ShouldBe(false);
        result.MinBidPrice.ShouldBe(basePrice);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecDisabled_WithCurrentPrice()
    {
        var auctionItemId = 3007L;
        var currentPrice = 500m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.IsKasec.ShouldBe(false);
        var expectedMinPrice = currentPrice + 5m;
        result.MinBidPrice.ShouldBe(expectedMinPrice);
    }

    [Fact]
    public async Task CheckBidEligibility_KasecHighPrice_AppliesCorrectly()
    {
        var auctionItemId = 3008L;
        var currentPrice = 15000m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("true"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.IsKasec.ShouldBe(true);
        var normalIncrement = 100m;
        var expectedMinPrice = currentPrice + (normalIncrement * 3);
        result.MinBidPrice.ShouldBe(expectedMinPrice);
    }

    [Fact]
    public async Task CheckBidEligibility_ConcurrentRequests_CacheConsistent()
    {
        var auctionItemId = 3009L;
        var basePrice = 100m;
        var cacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";

        _memoryCache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _testService.CheckKasecBidEligibility(auctionItemId, basePrice, false))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.All(r => r.IsKasec == true).ShouldBe(true);
        results.All(r => r.MinBidPrice == basePrice * 3).ShouldBe(true);
    }

    [Fact]
    public async Task CheckBidEligibility_PriceBelow100_NormalIncrement()
    {
        var auctionItemId = 3010L;
        var currentPrice = 50m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 5);
    }

    [Fact]
    public async Task CheckBidEligibility_Price100To1000_NormalIncrement()
    {
        var auctionItemId = 3011L;
        var currentPrice = 500m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 5);
    }

    [Fact]
    public async Task CheckBidEligibility_Price1000To2000_NormalIncrement()
    {
        var auctionItemId = 3012L;
        var currentPrice = 1500m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 10);
    }

    [Fact]
    public async Task CheckBidEligibility_Price2000To5000_NormalIncrement()
    {
        var auctionItemId = 3013L;
        var currentPrice = 3000m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 20);
    }

    [Fact]
    public async Task CheckBidEligibility_Price5000To10000_NormalIncrement()
    {
        var auctionItemId = 3014L;
        var currentPrice = 7000m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 50);
    }

    [Fact]
    public async Task CheckBidEligibility_PriceAbove10000_NormalIncrement()
    {
        var auctionItemId = 3015L;
        var currentPrice = 15000m;

        _redisDatabaseMock
            .Setup(x => x.StringGetAsync($"Auction:Kasec:{auctionItemId}", CommandFlags.None))
            .ReturnsAsync(new RedisValue("false"));

        var result = await _testService.CheckKasecBidEligibility(auctionItemId, currentPrice, true);

        result.MinBidPrice.ShouldBe(currentPrice + 100);
    }

    private class TestBidEligibilityService
    {
        private readonly IRedisClient _redisClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private const string KASEC_CACHE_PREFIX = "Kasec:";

        public TestBidEligibilityService(IRedisClient redisClient, IMemoryCache memoryCache, ILogger logger)
        {
            _redisClient = redisClient;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<KasecBidResult> CheckKasecBidEligibility(long auctionItemId, decimal basePrice, bool hasCurrentPrice)
        {
            var kasecCacheKey = $"{KASEC_CACHE_PREFIX}{auctionItemId}";
            bool isKasec;

            if (_memoryCache.TryGetValue(kasecCacheKey, out bool cachedKasecValue))
            {
                isKasec = cachedKasecValue;
            }
            else
            {
                var kasecVal = await _redisClient.Database.StringGetAsync($"Auction:Kasec:{auctionItemId}");
                isKasec = kasecVal.HasValue && kasecVal == "true";
                _memoryCache.Set(kasecCacheKey, isKasec, TimeSpan.FromSeconds(5));
            }

            decimal minPrice = 0;

            if (hasCurrentPrice)
            {
                if (basePrice < 100)
                {
                    minPrice = basePrice + 5;
                }
                else if (basePrice < 1000)
                {
                    minPrice = basePrice + 5;
                }
                else if (basePrice < 2000)
                {
                    minPrice = basePrice + 10;
                }
                else if (basePrice < 5000)
                {
                    minPrice = basePrice + 20;
                }
                else if (basePrice < 10000)
                {
                    minPrice = basePrice + 50;
                }
                else
                {
                    minPrice = basePrice + 100;
                }

                if (isKasec)
                {
                    minPrice = basePrice + ((minPrice - basePrice) * 3);
                }
            }
            else
            {
                minPrice = isKasec ? basePrice * 3 : basePrice;
            }

            return new KasecBidResult
            {
                IsKasec = isKasec,
                MinBidPrice = minPrice
            };
        }
    }

    private class KasecBidResult
    {
        public bool IsKasec { get; set; }
        public decimal MinBidPrice { get; set; }
    }
}
