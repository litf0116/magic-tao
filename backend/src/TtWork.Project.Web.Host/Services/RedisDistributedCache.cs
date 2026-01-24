using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TtWork.Lib.Redis;

namespace TtWork.Project.Web.Host.Services
{
    /// <summary>
    /// 基于 StackExchange.Redis 的 IDistributedCache 实现
    /// 使用现有的 IRedisClient 配置，保持连接复用
    /// </summary>
    public class RedisDistributedCache : IDistributedCache
    {
        private readonly IRedisClient _redisClient;
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);
        private readonly int _databaseId;

        public RedisDistributedCache(IRedisClient redisClient)
        {
            _redisClient = redisClient;
            _databaseId = redisClient.ConnectionMultiplexer.GetDatabase().Database;
        }

        public byte[]? Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<byte[]?>(null);

            try
            {
                var value = _redisClient.Database.StringGet(key);
                if (value.HasValue)
                {
                    return Task.FromResult<byte[]?>(value!);
                }
                return Task.FromResult<byte[]?>(null);
            }
            catch (Exception)
            {
                return Task.FromResult<byte[]?>(null);
            }
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).GetAwaiter().GetResult();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.CompletedTask;

            try
            {
                _redisClient.Database.KeyExpire(key, _defaultExpiry);
            }
            catch (Exception)
            {
                // 静默失败，不影响业务
            }
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.CompletedTask;

            try
            {
                _redisClient.Database.KeyDelete(key);
            }
            catch (Exception)
            {
                // 静默失败，不影响业务
            }
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.CompletedTask;

            try
            {
                var expiry = options.AbsoluteExpirationRelativeToNow ?? _defaultExpiry;
                if (options.AbsoluteExpiration.HasValue)
                {
                    expiry = options.AbsoluteExpiration.Value - DateTimeOffset.UtcNow;
                    if (expiry < TimeSpan.Zero)
                        expiry = _defaultExpiry;
                }

                _redisClient.Database.StringSet(key, value, expiry);
            }
            catch (Exception)
            {
                // 静默失败，不影响业务
            }
            return Task.CompletedTask;
        }
    }
}
