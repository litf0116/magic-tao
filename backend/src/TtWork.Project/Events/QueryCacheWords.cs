using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TtWork.Lib.Redis;
using TtWork.Project.Domains;

namespace TtWork.Project.Events;

public class QueryCacheWords : IRequest<string[]> {
    public bool Rebuild { get; set; }

    public QueryCacheWords(bool rebuild = false) {
        Rebuild = rebuild;
    }

    public class QueryCacheWordsHandle(
        IRedisClient redisClient,
        IRepository<SensitiveWord, long> repository,
        ILogger<QueryCacheWordsHandle> logger) : IRequestHandler<QueryCacheWords, string[]> {
        [UnitOfWork]
        public virtual async Task<string[]> Handle(QueryCacheWords request, CancellationToken cancellationToken) {
            const string key = AppConsts.SensitiveWordsCacheKey;
            if (request.Rebuild)
                await redisClient.Database.KeyDeleteAsync(key);

            var cache = await redisClient.Database.StringGetAsync(key);
            if (cache.HasValue) {
                try {
                    return cache.ToString().FromJsonString<string[]>();
                }
                catch (Exception e) {
                    logger.LogError("获取敏感词缓存失败 {@e}", e);
                }
            }

            var list = await repository.GetAll().AsNoTracking().Select(x => x.Content)
                .ToListAsync(cancellationToken: cancellationToken);

            await redisClient.Database.StringSetAsync(key, list.ToArray().ToJsonString());

            return list.ToArray();
        }
    }
}