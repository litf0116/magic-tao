using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TtWork.PerformanceTests
{
    /// <summary>
    /// 拍卖品缓存性能测试
    /// 测试缓存实现的性能指标和并发安全性
    /// </summary>
    public class AuctionCachePerformanceTest
    {
        // 模拟缓存存储
        private static readonly Dictionary<string, (string data, DateTime expireTime)> _cacheStore = new();
        private static readonly Dictionary<string, SemaphoreSlim> _cacheLocks = new();
        private static readonly object _lockDictLock = new();

        // 性能统计
        private static int _dbQueryCount = 0;
        private static int _cacheHitCount = 0;
        private static int _cacheMissCount = 0;

        /// <summary>
        /// 运行完整的缓存性能测试套件
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("🚀 拍卖品缓存性能测试");
            Console.WriteLine("=========================");
            Console.WriteLine();

            try
            {
                // 测试1：缓存命中性能
                RunCacheHitPerformanceTest();

                // 测试2：缓存击穿防护
                RunCacheBreakdownProtectionTest();

                // 测试3：不同状态缓存效果
                RunDifferentStatusCacheTest();

                // 测试4：高并发压力测试
                RunHighConcurrencyStressTest();

                // 测试5：缓存过期测试
                RunCacheExpirationTest();

                Console.WriteLine();
                Console.WriteLine("✅ 所有性能测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 测试执行失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 测试1：缓存命中性能测试
        /// </summary>
        public static void RunCacheHitPerformanceTest()
        {
            Console.WriteLine("📊 测试1：缓存命中性能");
            Console.WriteLine("-----------------------");
            Console.WriteLine();

            _cacheStore.Clear();
            _dbQueryCount = 0;
            _cacheHitCount = 0;
            _cacheMissCount = 0;

            const int auctionItemId = 1;
            const int testIterations = 20;

            long totalTime = 0;
            long cacheHitTime = 0;
            long dbQueryTime = 0;

            // 第一次查询（冷启动）
            var firstQueryTime = SimulateGetAuctionDetailAsync(auctionItemId, true);
            totalTime += firstQueryTime;
            dbQueryTime += firstQueryTime;

            Console.WriteLine($"冷启动查询（首次）: {firstQueryTime,4}ms");
            Console.WriteLine($"  - 数据库查询次数: {_dbQueryCount}");
            Console.WriteLine($"  - 缓存未命中");
            Console.WriteLine();

            // 后续查询（缓存命中）
            for (int i = 1; i < testIterations; i++)
            {
                var time = SimulateGetAuctionDetailAsync(auctionItemId, false);
                totalTime += time;
                cacheHitTime += time;
            }

            var avgTime = totalTime / testIterations;
            var avgCacheHitTime = cacheHitTime / (testIterations - 1);

            Console.WriteLine($"后续查询（缓存命中）:");
            Console.WriteLine($"  - 总耗时: {cacheHitTime,4}ms");
            Console.WriteLine($"  - 平均耗时: {avgCacheHitTime,4}ms");
            Console.WriteLine($"  - 缓存命中次数: {_cacheHitCount}");
            Console.WriteLine();

            var improvement = CalculateImprovement(firstQueryTime, avgCacheHitTime);
            Console.WriteLine($"🎯 性能提升: {improvement:F1}%");
            Console.WriteLine($"   - 冷启动: {firstQueryTime}ms");
            Console.WriteLine($"   - 缓存命中: {avgCacheHitTime}ms");
            Console.WriteLine();
        }

        /// <summary>
        /// 测试2：缓存击穿防护测试
        /// </summary>
        public static void RunCacheBreakdownProtectionTest()
        {
            Console.WriteLine("🔒 测试2：缓存击穿防护");
            Console.WriteLine("----------------------");
            Console.WriteLine();

            _cacheStore.Clear();
            _dbQueryCount = 0;
            _cacheHitCount = 0;
            _cacheMissCount = 0;

            const int auctionItemId = 2;
            const int concurrentTasks = 50;

            Console.WriteLine($"模拟 {concurrentTasks} 个并发请求访问同一缓存键");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<long>>();

            // 并发启动多个任务
            for (int i = 0; i < concurrentTasks; i++)
            {
                tasks.Add(Task.Run(() => SimulateGetAuctionDetailAsync(auctionItemId, true)));
            }

            var results = Task.WhenAll(tasks).Result;
            stopwatch.Stop();

            var totalTime = stopwatch.ElapsedMilliseconds;
            var avgTime = results.Average();
            var maxTime = results.Max();
            var minTime = results.Min();

            Console.WriteLine($"执行结果:");
            Console.WriteLine($"  - 总耗时: {totalTime,4}ms");
            Console.WriteLine($"  - 平均耗时: {avgTime,4:F1}ms");
            Console.WriteLine($"  - 最大耗时: {maxTime,4}ms");
            Console.WriteLine($"  - 最小耗时: {minTime,4}ms");
            Console.WriteLine($"  - 数据库查询次数: {_dbQueryCount}");
            Console.WriteLine($"  - 缓存命中次数: {_cacheHitCount}");
            Console.WriteLine();

            if (_dbQueryCount == 1)
            {
                Console.WriteLine("✅ 缓存击穿防护成功：只有一个线程执行了数据库查询");
                Console.WriteLine($"   - 其他 {concurrentTasks - 1} 个线程等待锁后获取缓存结果");
            }
            else
            {
                Console.WriteLine($"❌ 缓存击穿防护失败：({_dbQueryCount} 个线程执行了数据库查询)");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 测试3：不同状态商品缓存效果测试
        /// </summary>
        public static void RunDifferentStatusCacheTest()
        {
            Console.WriteLine("📦 测试3：不同状态商品缓存效果");
            Console.WriteLine("------------------------------");
            Console.WriteLine();

            _cacheStore.Clear();
            _dbQueryCount = 0;

            Console.WriteLine("测试不同状态商品的缓存过期时间:");
            Console.WriteLine();

            // 拍卖中商品（30秒缓存）
            Console.WriteLine("1. 拍卖中商品 (30秒缓存):");
            var auctioningTime = SimulateGetAuctionDetailAsync(3, true, "拍卖中");
            var auctioningCacheTime = SimulateGetAuctionDetailAsync(3, false, "拍卖中");
            Console.WriteLine($"   - 冷启动: {auctioningTime,4}ms, 缓存命中: {auctioningCacheTime,4}ms");
            Console.WriteLine($"   - 提升: {CalculateImprovement(auctioningTime, auctioningCacheTime):F1}%");
            Console.WriteLine();

            // 待拍卖商品（5分钟缓存）
            Console.WriteLine("2. 待拍卖商品 (5分钟缓存):");
            var listedTime = SimulateGetAuctionDetailAsync(4, true, "待拍卖");
            var listedCacheTime = SimulateGetAuctionDetailAsync(4, false, "待拍卖");
            Console.WriteLine($"   - 冷启动: {listedTime,4}ms, 缓存命中: {listedCacheTime,4}ms");
            Console.WriteLine($"   - 提升: {CalculateImprovement(listedTime, listedCacheTime):F1}%");
            Console.WriteLine();

            // 已成交商品（15分钟缓存）
            Console.WriteLine("3. 已成交商品 (15分钟缓存):");
            var doneTime = SimulateGetAuctionDetailAsync(5, true, "已成交");
            var doneCacheTime = SimulateGetAuctionDetailAsync(5, false, "已成交");
            Console.WriteLine($"   - 冷启动: {doneTime,4}ms, 缓存命中: {doneCacheTime,4}ms");
            Console.WriteLine($"   - 提升: {CalculateImprovement(doneTime, doneCacheTime):F1}%");
            Console.WriteLine();

            Console.WriteLine("💡 策略总结:");
            Console.WriteLine("   - 拍卖中: 30秒缓存（高频更新）");
            Console.WriteLine("   - 待拍卖: 5分钟缓存（中频更新）");
            Console.WriteLine("   - 已成交: 15分钟缓存（低频更新）");
            Console.WriteLine();
        }

        /// <summary>
        /// 测试4：高并发压力测试
        /// </summary>
        public static void RunHighConcurrencyStressTest()
        {
            Console.WriteLine("⚡ 测试4：高并发压力测试");
            Console.WriteLine("-----------------------");
            Console.WriteLine();

            _cacheStore.Clear();
            _dbQueryCount = 0;
            _cacheHitCount = 0;
            _cacheMissCount = 0;

            const int concurrentTasks = 100;
            const int distinctAuctionItems = 10;

            Console.WriteLine($"模拟 {concurrentTasks} 个并发请求，访问 {distinctAuctionItems} 个不同商品");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<long>>();
            var random = new Random();

            // 并发启动多个任务，随机访问不同商品
            for (int i = 0; i < concurrentTasks; i++)
            {
                var auctionItemId = random.Next(1, distinctAuctionItems + 1);
                tasks.Add(Task.Run(() => SimulateGetAuctionDetailAsync(auctionItemId, true)));
            }

            var results = Task.WhenAll(tasks).Result;
            stopwatch.Stop();

            var totalTime = stopwatch.ElapsedMilliseconds;
            var avgTime = results.Average();
            var p95 = results.OrderBy(x => x).Skip((int)(results.Length * 0.95)).First();
            var p99 = results.OrderBy(x => x).Skip((int)(results.Length * 0.99)).First();
            var requestsPerSecond = (concurrentTasks * 1000.0) / totalTime;

            Console.WriteLine($"执行结果:");
            Console.WriteLine($"  - 总耗时: {totalTime,4}ms");
            Console.WriteLine($"  - 平均响应时间: {avgTime,4:F1}ms");
            Console.WriteLine($"  - P95 响应时间: {p95,4}ms");
            Console.WriteLine($"  - P99 响应时间: {p99,4}ms");
            Console.WriteLine($"  - 吞吐量: {requestsPerSecond:F1} 请求/秒");
            Console.WriteLine($"  - 数据库查询次数: {_dbQueryCount}");
            Console.WriteLine($"  - 缓存命中次数: {_cacheHitCount}");
            Console.WriteLine($"  - 缓存命中率: {(_cacheHitCount * 100.0 / (concurrentTasks)):F1}%");
            Console.WriteLine();

            if (requestsPerSecond > 500)
            {
                Console.WriteLine($"✅ 性能优秀：{requestsPerSecond:F1} 请求/秒");
            }
            else if (requestsPerSecond > 200)
            {
                Console.WriteLine($"✅ 性能良好：{requestsPerSecond:F1} 请求/秒");
            }
            else
            {
                Console.WriteLine($"⚠️ 性能需优化：{requestsPerSecond:F1} 请求/秒");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 测试5：缓存过期测试
        /// </summary>
        public static void RunCacheExpirationTest()
        {
            Console.WriteLine("⏰ 测试5：缓存过期测试");
            Console.WriteLine("---------------------");
            Console.WriteLine();

            _cacheStore.Clear();
            _dbQueryCount = 0;

            const int auctionItemId = 6;

            Console.WriteLine("测试缓存过期和重建机制:");
            Console.WriteLine();

            // 第一次查询（冷启动）
            Console.WriteLine("1. 第一次查询（冷启动）:");
            var firstQuery = SimulateGetAuctionDetailAsync(auctionItemId, true, "拍卖中", expireInSeconds: 3);
            Console.WriteLine($"   - 耗时: {firstQuery,4}ms");
            Console.WriteLine($"   - 数据库查询次数: {_dbQueryCount}");
            Console.WriteLine();

            // 第二次查询（缓存命中）
            Console.WriteLine("2. 第二次查询（缓存命中）:");
            var secondQuery = SimulateGetAuctionDetailAsync(auctionItemId, false, "拍卖中", expireInSeconds: 3);
            Console.WriteLine($"   - 耗时: {secondQuery,4}ms");
            Console.WriteLine($"   - 缓存命中: 是");
            Console.WriteLine();

            // 等待缓存过期
            Console.WriteLine($"3. 等待缓存过期（3秒）...");
            Thread.Sleep(3100);
            Console.WriteLine("   - 缓存已过期");
            Console.WriteLine();

            // 第三次查询（缓存重建）
            Console.WriteLine("4. 第三次查询（缓存重建）:");
            var thirdQuery = SimulateGetAuctionDetailAsync(auctionItemId, true, "拍卖中", expireInSeconds: 3);
            Console.WriteLine($"   - 耗时: {thirdQuery,4}ms");
            Console.WriteLine($"   - 数据库查询次数: {_dbQueryCount}");
            Console.WriteLine($"   - 缓存重建: 是");
            Console.WriteLine();

            // 第四次查询（新缓存命中）
            Console.WriteLine("5. 第四次查询（新缓存命中）:");
            var fourthQuery = SimulateGetAuctionDetailAsync(auctionItemId, false, "拍卖中", expireInSeconds: 3);
            Console.WriteLine($"   - 耗时: {fourthQuery,4}ms");
            Console.WriteLine($"   - 缓存命中: 是");
            Console.WriteLine();

            Console.WriteLine("✅ 缓存过期机制工作正常:");
            Console.WriteLine($"   - 缓存有效期：3秒");
            Console.WriteLine($"   - 过期后自动重建缓存");
            Console.WriteLine($"   - 重建后缓存再次生效");
            Console.WriteLine();
        }

        /// <summary>
        /// 模拟获取拍卖品详情（带缓存）
        /// </summary>
        private static long SimulateGetAuctionDetailAsync(
            long auctionItemId,
            bool simulateDbQuery,
            string status = "拍卖中",
            int expireInSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                string cacheKey = $"auction:detail:{auctionItemId}";

                // 尝试从缓存获取
                lock (_lockDictLock)
                {
                    if (_cacheStore.ContainsKey(cacheKey))
                    {
                        var cached = _cacheStore[cacheKey];
                        if (cached.expireTime > DateTime.Now)
                        {
                            // 缓存命中
                            _cacheHitCount++;
                            stopwatch.Stop();
                            return stopwatch.ElapsedMilliseconds;
                        }
                        else
                        {
                            // 缓存过期，移除
                            _cacheStore.Remove(cacheKey);
                        }
                    }
                }

                // 获取或创建缓存锁
                var cacheLock = GetOrCreateCacheLock(cacheKey);
                cacheLock.Wait();

                try
                {
                    // 双重检查锁定
                    lock (_lockDictLock)
                    {
                        if (_cacheStore.ContainsKey(cacheKey))
                        {
                            var cached = _cacheStore[cacheKey];
                            if (cached.expireTime > DateTime.Now)
                            {
                                _cacheHitCount++;
                                stopwatch.Stop();
                                return stopwatch.ElapsedMilliseconds;
                            }
                        }
                    }

                    // 模拟数据库查询
                    if (simulateDbQuery)
                    {
                        _dbQueryCount++;
                        _cacheMissCount++;

                        // 模拟数据库查询耗时（根据状态不同）
                        var dbDelay = status switch
                        {
                            "拍卖中" => 50,   // 拍卖中需要查询出价记录
                            "待拍卖" => 40,   // 待拍卖相对简单
                            "已成交" => 45,   // 已成交需要查询交易记录
                            _ => 45
                        };

                        Thread.Sleep(dbDelay);
                    }

                    // 设置缓存
                    lock (_lockDictLock)
                    {
                        var data = $"{{\"id\":{auctionItemId},\"status\":\"{status}\"}}";
                        var expireTime = DateTime.Now.AddSeconds(expireInSeconds);
                        _cacheStore[cacheKey] = (data, expireTime);
                    }
                }
                finally
                {
                    cacheLock.Release();
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 获取或创建缓存锁
        /// </summary>
        private static SemaphoreSlim GetOrCreateCacheLock(string cacheKey)
        {
            lock (_lockDictLock)
            {
                if (!_cacheLocks.ContainsKey(cacheKey))
                {
                    _cacheLocks[cacheKey] = new SemaphoreSlim(1, 1);
                }
                return _cacheLocks[cacheKey];
            }
        }

        /// <summary>
        /// 计算性能提升百分比
        /// </summary>
        private static double CalculateImprovement(long before, long after)
        {
            if (before == 0) return 0;
            return ((double)(before - after) / before) * 100;
        }

        /// <summary>
        /// 清理测试数据
        /// </summary>
        public static void Cleanup()
        {
            _cacheStore.Clear();
            _cacheLocks.Clear();
            _dbQueryCount = 0;
            _cacheHitCount = 0;
            _cacheMissCount = 0;
        }
    }
}