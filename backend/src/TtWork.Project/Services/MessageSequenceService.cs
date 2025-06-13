using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Runtime.Caching;
using FreeIM;
using StackExchange.Redis;
using Abp.Logging;

namespace TtWork.Project.Services
{
    /// <summary>
    /// 消息序列号生成服务
    /// 确保消息在每个频道内的顺序性
    /// </summary>
    public interface IMessageSequenceService : ITransientDependency
    {
        /// <summary>
        /// 获取下一个序列号
        /// </summary>
        /// <param name="channelKey">频道标识（群聊频道或私聊用户对）</param>
        /// <returns>序列号</returns>
        Task<long> GetNextSequenceNumberAsync(string channelKey);
        
        /// <summary>
        /// 为群聊消息生成序列号
        /// </summary>
        /// <param name="chan">群聊频道</param>
        /// <returns>序列号</returns>
        Task<long> GetNextSequenceNumberForChannelAsync(string chan);
        
        /// <summary>
        /// 为私聊消息生成序列号
        /// </summary>
        /// <param name="from">发送者ID</param>
        /// <param name="to">接收者ID</param>
        /// <returns>序列号</returns>
        Task<long> GetNextSequenceNumberForPrivateAsync(long from, long to);
    }

    public class MessageSequenceService : IMessageSequenceService
    {
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private const string SEQUENCE_KEY_PREFIX = "msg_seq:";
        
        // Redis Lua脚本，确保原子性操作
        private const string REDIS_SCRIPT = @"
            local key = KEYS[1]
            local current = redis.call('GET', key)
            if current == false then
                current = 0
            end
            local next = tonumber(current) + 1
            redis.call('SET', key, next)
            return next
        ";

        public MessageSequenceService(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _logger = LogManager.GetLogger(typeof(MessageSequenceService));
        }

        public async Task<long> GetNextSequenceNumberAsync(string channelKey)
        {
            try
            {
                var redisKey = $"{SEQUENCE_KEY_PREFIX}{channelKey}";
                
                // 尝试使用Redis原子操作
                var database = GetRedisDatabase();
                if (database != null)
                {
                    try
                    {
                        var result = await database.ScriptEvaluateAsync(REDIS_SCRIPT, new RedisKey[] { redisKey });
                        return (long)result;
                    }
                    catch (Exception redisEx)
                    {
                        _logger.Warn($"Redis操作失败，使用备用方案: {redisEx.Message}");
                    }
                }
                
                // Redis不可用时的备用方案：使用内存缓存
                var cache = _cacheManager.GetCache("MessageSequence");
                var currentValue = await cache.GetAsync(redisKey, () => Task.FromResult(0L));
                var nextValue = currentValue + 1;
                await cache.SetAsync(redisKey, nextValue);
                
                _logger.Info($"使用内存缓存生成序列号: {channelKey} -> {nextValue}");
                return nextValue;
            }
            catch (Exception ex)
            {
                _logger.Error($"序列号生成失败: {channelKey}", ex);
                
                // 最后的备用方案：使用时间戳
                var fallbackValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _logger.Warn($"使用时间戳作为序列号备用方案: {channelKey} -> {fallbackValue}");
                return fallbackValue;
            }
        }

        public async Task<long> GetNextSequenceNumberForChannelAsync(string chan)
        {
            if (string.IsNullOrEmpty(chan))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(chan));
            }
            
            return await GetNextSequenceNumberAsync($"chan:{chan}");
        }

        public async Task<long> GetNextSequenceNumberForPrivateAsync(long from, long to)
        {
            // 为私聊创建一致的频道标识（较小的ID在前）
            var channelKey = from < to ? $"private:{from}_{to}" : $"private:{to}_{from}";
            return await GetNextSequenceNumberAsync(channelKey);
        }

        private IDatabase GetRedisDatabase()
        {
            try
            {
                // 尝试通过FreeIM的Redis连接获取数据库
                // 这里需要根据项目的实际Redis配置进行调整
                var connectionString = "localhost:6379"; // 从配置文件读取
                var connection = ConnectionMultiplexer.Connect(connectionString);
                return connection.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.Debug($"无法连接到Redis: {ex.Message}");
                return null;
            }
        }
    }
} 