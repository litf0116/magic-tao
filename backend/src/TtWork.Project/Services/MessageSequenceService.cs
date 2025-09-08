using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Logging;
using Castle.Core.Logging;
using System.Collections.Concurrent;
using System.Threading;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Dapper;
using TtWork.Project.Domains;
using TtWork.Abp.Dapper;

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

        /// <summary>
        /// 为系统消息生成序列号（本地生成，避免Redis依赖）
        /// </summary>
        /// <param name="channelKey">频道标识</param>
        /// <returns>序列号</returns>
        long GetNextSystemSequenceNumber(string channelKey);
    }

    public class MessageSequenceService : IMessageSequenceService
    {
        private readonly ILogger _logger;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        
        // 统一的本地序列号缓存
        private static readonly ConcurrentDictionary<string, long> _sequenceCache = new();
        private static readonly object _sequenceLock = new();

        public MessageSequenceService(
            ILogger logger,
            IRepository<Message, Guid> messageRepository,
            ISqlConnectionFactory sqlConnectionFactory)
        {
            _logger = logger;
            _messageRepository = messageRepository;
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<long> GetNextSequenceNumberAsync(string channelKey)
        {
            return await Task.FromResult(GetNextSequenceNumberLocal(channelKey));
        }

        public async Task<long> GetNextSequenceNumberForChannelAsync(string chan)
        {
            if (string.IsNullOrEmpty(chan))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(chan));
            }

            return await Task.FromResult(GetNextSequenceNumberLocal($"chan:{chan}"));
        }

        public async Task<long> GetNextSequenceNumberForPrivateAsync(long from, long to)
        {
            // 为私聊创建一致的频道标识（较小的ID在前）
            var channelKey = from < to ? $"private:{from}_{to}" : $"private:{to}_{from}";
            return await Task.FromResult(GetNextSequenceNumberLocal(channelKey));
        }

        /// <summary>
        /// 为系统消息生成序列号（本地生成，避免Redis依赖）
        /// </summary>
        /// <param name="channelKey">频道标识</param>
        /// <returns>序列号</returns>
        public long GetNextSystemSequenceNumber(string channelKey)
        {
            // 系统消息直接使用对应的频道标识，不添加system:前缀
            if (channelKey.Contains("private:"))
            {
                // 系统私聊消息直接使用私聊频道标识
                return GetNextSequenceNumberLocal(channelKey);
            }
            else
            {
                // 系统群聊消息直接使用群聊频道标识
                return GetNextSequenceNumberLocal($"chan:{channelKey}");
            }
        }

        /// <summary>
        /// 延迟加载频道序列号 - 从数据库读取现有最大序列号
        /// </summary>
        /// <param name="channelKey">频道标识</param>
        /// <returns>当前最大序列号</returns>
        private async Task<long> LoadChannelSequenceFromDatabaseAsync(string channelKey)
        {
            try
            {
                using var connection = _sqlConnectionFactory.GetOpenConnection();
                
                long maxSequence = 0;
                
                if (channelKey.StartsWith("chan:"))
                {
                    // 群聊频道：从Chan字段查询
                    var chan = channelKey.Substring(5); // 移除"chan:"前缀
                    var result = connection.QuerySingleOrDefault<long?>(
                        @"SELECT MAX(SequenceNumber) 
                          FROM T_Message 
                          WHERE Chan = @Chan", 
                        new { Chan = chan });
                    
                    maxSequence = result ?? 0;
                }
                else if (channelKey.StartsWith("private:"))
                {
                    // 私聊频道：解析用户ID并查询
                    var userKey = channelKey.Substring(8); // 移除"private:"前缀
                    var parts = userKey.Split('_');
                    if (parts.Length == 2 && long.TryParse(parts[0], out var user1) && long.TryParse(parts[1], out var user2))
                    {
                        var result = connection.QuerySingleOrDefault<long?>(
                            @"SELECT MAX(SequenceNumber) 
                              FROM T_Message 
                              WHERE ((From = @User1 AND To = @User2) OR (From = @User2 AND To = @User1))
                              AND To IS NOT NULL", 
                            new { User1 = user1, User2 = user2 });
                        
                        maxSequence = result ?? 0;
                    }
                }
                                
                _logger.Info($"从数据库加载序列号: {channelKey} -> {maxSequence}");
                return maxSequence;
            }
            catch (Exception ex)
            {
                _logger.Error($"从数据库加载序列号失败: {channelKey}", ex);
                return 0; // 加载失败从0开始
            }
        }

        /// <summary>
        /// 本地序列号生成核心方法 - 延迟加载模式
        /// </summary>
        /// <param name="channelKey">频道标识</param>
        /// <returns>序列号</returns>
        private long GetNextSequenceNumberLocal(string channelKey)
        {
            try
            {
                lock (_sequenceLock)
                {
                    // 获取当前序列号，如果不存在则从数据库加载
                    if (!_sequenceCache.TryGetValue(channelKey, out var currentSequence))
                    {
                        // 延迟加载：从数据库获取该频道的最大序列号
                        currentSequence = LoadChannelSequenceFromDatabaseAsync(channelKey).Result;
                        _sequenceCache[channelKey] = currentSequence;
                        _logger.Info($"延迟加载序列号: {channelKey} -> {currentSequence}");
                    }
                    
                    // 生成下一个序列号
                    var nextSequence = currentSequence + 1;
                    _sequenceCache[channelKey] = nextSequence;
                    
                    _logger.Info($"序列号生成成功: {channelKey} -> {nextSequence}");
                    return nextSequence;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"序列号生成失败: {channelKey}", ex);
                
                // 备用方案：使用时间戳
                var fallbackValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _logger.Warn($"使用时间戳作为序列号备用方案: {channelKey} -> {fallbackValue}");
                return fallbackValue;
            }
        }
    }
}