using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace TtWork.Lib.Redis
{
    public interface IRedisClient
    {
        IDatabase Database { get; }
        ConnectionMultiplexer ConnectionMultiplexer { get; }

        void DeleteKeysWithPartten(string pattern);
        Task DeleteKeysWithParttenAsync(string pattern);
    }

    public class RedisClient : IRedisClient, IDisposable
    {
        private readonly IOptionsSnapshot<RedisOptions> _optionsAccessor;
        private readonly ILogger<RedisClient> _logger;
        private bool _disposed = false;

        public IDatabase Database { get; private set; }
        public ConnectionMultiplexer ConnectionMultiplexer { get; private set; }

        public void DeleteKeysWithPartten(string pattern)
        {
            try
            {
                foreach (var ep in ConnectionMultiplexer.GetEndPoints())
                {
                    var server = ConnectionMultiplexer.GetServer(ep);
                    var keys = server.Keys(database: _optionsAccessor.Value.DatabaseId, pattern: pattern).ToArray();
                    if (keys.Length > 0)
                    {
                        Database.KeyDeleteAsync(keys);
                        _logger.LogDebug("删除了 {Count} 个匹配模式 {Pattern} 的键", keys.Length, pattern);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模式匹配的键失败: {Pattern}", pattern);
            }
        }

        public async Task DeleteKeysWithParttenAsync(string pattern)
        {
            try
            {
                foreach (var ep in ConnectionMultiplexer.GetEndPoints())
                {
                    var server = ConnectionMultiplexer.GetServer(ep);
                    var keys = server.Keys(database: _optionsAccessor.Value.DatabaseId, pattern: pattern).ToArray();
                    if (keys.Length > 0)
                    {
                        await Database.KeyDeleteAsync(keys);
                        _logger.LogDebug("删除了 {Count} 个匹配模式 {Pattern} 的键", keys.Length, pattern);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模式匹配的键失败: {Pattern}", pattern);
            }
        }

        public RedisClient(IOptionsSnapshot<RedisOptions> optionsAccessor, ILogger<RedisClient> logger)
        {
            _optionsAccessor = optionsAccessor;
            _logger = logger;

            // 创建配置选项
            var configurationOptions = new ConfigurationOptions();

            // 解析连接字符串
            ParseConnectionString(optionsAccessor.Value.ConnectionString, configurationOptions);

            // 应用优化的默认配置
            configurationOptions.SyncTimeout = optionsAccessor.Value.SyncTimeout > 0
                ? optionsAccessor.Value.SyncTimeout
                : 5000;

            configurationOptions.AsyncTimeout = optionsAccessor.Value.AsyncTimeout > 0
                ? optionsAccessor.Value.AsyncTimeout
                : 5000;

            configurationOptions.ConnectRetry = optionsAccessor.Value.ConnectRetry;
            configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(1000); // 指数退避重试
            configurationOptions.AbortOnConnectFail = optionsAccessor.Value.AbortOnConnectFail; // 连接失败不中止，允许重试
            configurationOptions.KeepAlive = optionsAccessor.Value.KeepAlive; // 保持连接活跃
            configurationOptions.ConnectTimeout = optionsAccessor.Value.ConnectTimeout;

            // 优化性能配置（移除不支持的属性）
            // configurationOptions.DefaultVersion = new Version(3, 0); // Redis 3.0+ 优化

            // 设置客户端名称，便于监控和调试
            configurationOptions.ClientName = $"MagicTao_{Environment.MachineName}_{Guid.NewGuid().ToString("N")[..8]}";

            // 详细日志记录
            _logger.LogInformation("正在连接 Redis: {EndPoint}, 数据库: {DatabaseId}",
                configurationOptions.EndPoints.Count > 0 ? configurationOptions.EndPoints[0] : "未知",
                optionsAccessor.Value.DatabaseId);

            try
            {
                // 建立连接
                ConnectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);

                // 注册连接事件
                RegisterConnectionEvents();

                Database = ConnectionMultiplexer.GetDatabase(optionsAccessor.Value.DatabaseId);

                // 测试连接
                Database.Ping();

                _logger.LogInformation("Redis 连接成功，数据库 ID: {DatabaseId}, 连接数: {ConnectionCount}",
                    optionsAccessor.Value.DatabaseId, ConnectionMultiplexer.GetCounters().TotalOutstanding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis 连接失败，将在后台自动重试");
                // 连接失败时仍然继续，StackExchange.Redis 会自动重试
                ConnectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                RegisterConnectionEvents();
                Database = ConnectionMultiplexer.GetDatabase(optionsAccessor.Value.DatabaseId);
            }
        }

        private void ParseConnectionString(string connectionString, ConfigurationOptions config)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                config.EndPoints.Add("127.0.0.1", 6379);
                return;
            }

            // 解析连接字符串中的配置
            var parts = connectionString.Split(',');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].Trim().ToLowerInvariant())
                    {
                        case "syncTimeout":
                            if (int.TryParse(keyValue[1].Trim(), out var syncTimeout))
                                config.SyncTimeout = syncTimeout;
                            break;
                        case "asyncTimeout":
                            if (int.TryParse(keyValue[1].Trim(), out var asyncTimeout))
                                config.AsyncTimeout = asyncTimeout;
                            break;
                        case "connectTimeout":
                            if (int.TryParse(keyValue[1].Trim(), out var connectTimeout))
                                config.ConnectTimeout = connectTimeout;
                            break;
                        case "password":
                            config.Password = keyValue[1].Trim();
                            break;
                        case "ssl":
                            if (bool.TryParse(keyValue[1].Trim(), out var ssl))
                                config.Ssl = ssl;
                            break;
                        case "defaultDatabase":
                            if (int.TryParse(keyValue[1].Trim(), out var db))
                                config.DefaultDatabase = db;
                            break;
                    }
                }
                else
                {
                    // 如果不是键值对，则认为是服务器地址
                    if (!part.Contains(":"))
                    {
                        config.EndPoints.Add(part.Trim(), 6379);
                    }
                    else
                    {
                        var hostPort = part.Split(':');
                        if (hostPort.Length == 2 && int.TryParse(hostPort[1].Trim(), out var port))
                        {
                            config.EndPoints.Add(hostPort[0].Trim(), port);
                        }
                    }
                }
            }
        }

        private void RegisterConnectionEvents()
        {
            if (ConnectionMultiplexer != null)
            {
                ConnectionMultiplexer.ConnectionFailed += (sender, args) =>
                {
                    _logger.LogError(args.Exception, "Redis 连接失败: {EndPoint}, {FailureType}",
                        args.EndPoint, args.FailureType);
                };

                ConnectionMultiplexer.ConnectionRestored += (sender, args) =>
                {
                    _logger.LogInformation("Redis 连接已恢复: {EndPoint}", args.EndPoint);
                };

                ConnectionMultiplexer.ErrorMessage += (sender, args) =>
                {
                    _logger.LogError("Redis 错误消息: {EndPoint}, {Error}",
                        args.EndPoint, args.Message);
                };

                ConnectionMultiplexer.InternalError += (sender, args) =>
                {
                    _logger.LogError(args.Exception, "Redis 内部错误: {EndPoint}, {Origin}",
                        args.EndPoint, args.Origin);
                };
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                ConnectionMultiplexer?.Dispose();
                _disposed = true;
            }
        }
    }

    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "127.0.0.1";
        public int DatabaseId { get; set; } = 0;

        // 超时配置
        public int SyncTimeout { get; set; } = 5000; // 同步超时（毫秒）
        public int AsyncTimeout { get; set; } = 5000; // 异步超时（毫秒）
        public int ConnectTimeout { get; set; } = 5000; // 连接超时（毫秒）

        // 连接池配置
        public int MaxPoolSize { get; set; } = 50; // 最大连接池大小（降低以避免资源浪费）
        public int ConnectRetry { get; set; } = 3; // 连接重试次数

        // 性能优化配置
        public bool AbortOnConnectFail { get; set; } = false; // 连接失败时不中止
        public int KeepAlive { get; set; } = 60; // 保活时间（秒）
        public bool AllowAdmin { get; set; } = false; // 是否允许管理命令
        public string SslHost { get; set; } = null; // SSL主机名（如需要）
    }
}