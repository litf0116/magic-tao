using System.Linq;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace TtWork.Lib.Redis
{
    public interface IRedisClient
    {
        IDatabase Database { get; }

        void DeleteKeysWithPartten(string parten);
    }

    public class RedisClient : IRedisClient
    {
        private readonly IOptionsSnapshot<RedisOptions> _optionsAccessor;
        public IDatabase Database { get; }

        private ConnectionMultiplexer ConnectionMultiplexer { get; }

        public void DeleteKeysWithPartten(string pattern)
        {
            foreach (var ep in ConnectionMultiplexer.GetEndPoints())
            {
                var server = ConnectionMultiplexer.GetServer(ep);
                var keys = server.Keys(database: _optionsAccessor.Value.DatabaseId, pattern: pattern).ToArray();
                Database.KeyDeleteAsync(keys);
            }
        }

        public RedisClient(IOptionsSnapshot<RedisOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(optionsAccessor.Value.ConnectionString);

            Database = ConnectionMultiplexer.GetDatabase(optionsAccessor.Value.DatabaseId);
        }
    }

    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "127.0.0.1,connectTimeout=1000,syncTimeout=1000";
        public int DatabaseId { get; set; } = -1;
    }
}