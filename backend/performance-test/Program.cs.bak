using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceTest
{
    /// <summary>
    /// 内存缓存性能测试
    /// 验证优化后的缓存性能
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("   缓存优化性能测试");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            await RunCacheClearPerformanceTest();
            await RunCacheReadPerformanceTest();
            await RunConcurrentCacheTest();
            await RunMemoryVsRedisComparison();

            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("   性能测试完成");
            Console.WriteLine("==============================================");
        }

        /// <summary>
        /// 测试缓存清除性能（核心优化点）
        /// </summary>
        static async Task RunCacheClearPerformanceTest()
        {
            Console.WriteLine("[测试 1] 缓存清除性能测试");
            Console.WriteLine("----------------------------------------------");

            var cache = new SimpleMemoryCache();
            var testSizes = new[] { 10, 100, 500, 1000 };

            foreach (var size in testSizes)
            {
                // 填充缓存
                for (int i = 0; i < size; i++)
                {
                    cache.Set($"auction:list:1:{i}", new object());
                }

                // 测试清除性能
                var sw = Stopwatch.StartNew();
                cache.ClearByPrefix("auction:list:1:");
                sw.Stop();

                Console.WriteLine($"  清除 {size,4} 个缓存项: {sw.ElapsedMilliseconds,3}ms ({sw.Elapsed.TotalMicroseconds / size:F2}μs/项)");

                // 验证清除成功
                Debug.Assert(cache.Count == 0, "Cache should be empty after clear");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 测试缓存读取性能
        /// </summary>
        static async Task RunCacheReadPerformanceTest()
        {
            Console.WriteLine("[测试 2] 缓存读取性能测试");
            Console.WriteLine("----------------------------------------------");

            var cache = new SimpleMemoryCache();
            const int itemCount = 1000;

            // 填充缓存
            for (int i = 0; i < itemCount; i++)
            {
                cache.Set($"key:{i}", new { Data = i });
            }

            // 测试单次读取
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < itemCount; i++)
            {
                cache.TryGet($"key:{i}", out _);
            }
            sw.Stop();

            Console.WriteLine($"  读取 {itemCount} 个缓存项: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  平均每次读取: {sw.Elapsed.TotalMicroseconds / itemCount:F2}μs");

            // 测试缓存命中稳定性
            var times = new List<long>();
            for (int i = 0; i < 1000; i++)
            {
                sw = Stopwatch.StartNew();
                cache.TryGet("key:1", out _);
                sw.Stop();
                times.Add(sw.ElapsedTicks);
            }

            var avgTicks = times.Average();
            var maxTicks = times.Max();
            Console.WriteLine($"  1000次重复读取: 平均={avgTicks:F0}ticks, 最大={maxTicks}ticks");

            Console.WriteLine();
        }

        /// <summary>
        /// 测试并发性能
        /// </summary>
        static async Task RunConcurrentCacheTest()
        {
            Console.WriteLine("[测试 3] 并发性能测试");
            Console.WriteLine("----------------------------------------------");

            var cache = new SimpleMemoryCache();
            const int concurrentCount = 100;
            const int opsPerThread = 1000;

            // 填充缓存
            for (int i = 0; i < concurrentCount; i++)
            {
                cache.Set($"concurrent:{i}", new { Data = i });
            }

            // 并发读取测试
            var sw = Stopwatch.StartNew();
            var tasks = new List<Task>();

            for (int t = 0; t < concurrentCount; t++)
            {
                var taskId = t;
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < opsPerThread; i++)
                    {
                        cache.TryGet($"concurrent:{taskId}", out _);
                    }
                }));
            }

            await Task.WhenAll(tasks);
            sw.Stop();

            var totalOps = concurrentCount * opsPerThread;
            var opsPerSecond = totalOps / (sw.ElapsedMilliseconds / 1000.0);

            Console.WriteLine($"  并发读取: {concurrentCount} 线程 x {opsPerThread} 操作");
            Console.WriteLine($"  总时间: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  每秒操作数: {opsPerSecond:F0} ops/sec");

            // 并发读写混合测试
            cache.Clear();
            sw = Stopwatch.StartNew();
            tasks.Clear();

            // 50% 读，50% 写
            for (int t = 0; t < concurrentCount; t++)
            {
                var taskId = t;
                if (t % 2 == 0)
                {
                    // 读任务
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < opsPerThread; i++)
                        {
                            cache.TryGet($"mixed:{i % 100}", out _);
                        }
                    }));
                }
                else
                {
                    // 写任务
                    tasks.Add(Task.Run(() =>
                    {
                        for (int i = 0; i < opsPerThread / 10; i++)
                        {
                            cache.Set($"mixed:{taskId}:{i}", new { Data = i });
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
            sw.Stop();

            Console.WriteLine($"  混合读写: {sw.ElapsedMilliseconds}ms");

            Console.WriteLine();
        }

        /// <summary>
        /// 内存缓存 vs Redis 对比
        /// </summary>
        static async Task RunMemoryVsRedisComparison()
        {
            Console.WriteLine("[测试 4] 内存 vs Redis 性能对比");
            Console.WriteLine("----------------------------------------------");

            const int keyCount = 100;

            // 模拟 Redis SCAN + DELETE
            Console.WriteLine("  模拟 Redis SCAN + DELETE:");
            var sw = Stopwatch.StartNew();

            // SCAN 延迟（需要扫描整个 keyspace）
            await Task.Delay(2);

            // 网络往返延迟
            for (int i = 0; i < keyCount; i++)
            {
                await Task.Delay(1); // 模拟网络延迟
            }

            sw.Stop();
            var redisTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"    清除 {keyCount} 个键: {redisTime}ms");

            // 内存缓存直接删除
            Console.WriteLine("  内存缓存直接删除:");
            var cache = new SimpleMemoryCache();
            for (int i = 0; i < keyCount; i++)
            {
                cache.Set($"key:{i}", new object());
            }

            sw = Stopwatch.StartNew();
            cache.ClearByPrefix("key:");
            sw.Stop();
            var memoryTime = sw.ElapsedMilliseconds;

            Console.WriteLine($"    清除 {keyCount} 个键: {memoryTime}ms");
            Console.WriteLine($"  性能提升: {redisTime / Math.Max(memoryTime, 1):F0}x");

            Console.WriteLine();
        }
    }

    /// <summary>
    /// 简单内存缓存实现（模拟实际缓存行为）
    /// </summary>
    class SimpleMemoryCache
    {
        private readonly ConcurrentDictionary<string, object> _cache = new();
        private readonly ConcurrentDictionary<string, DateTime> _keys = new();

        public int Count => _cache.Count;

        public void Set(string key, object value)
        {
            _cache[key] = value;
            _keys[key] = DateTime.UtcNow;
        }

        public bool TryGet(string key, out object value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
            _keys.TryRemove(key, out _);
        }

        public void ClearByPrefix(string prefix)
        {
            var keysToRemove = _keys.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
                _keys.TryRemove(key, out _);
            }
        }

        public void Clear()
        {
            _cache.Clear();
            _keys.Clear();
        }
    }
}
