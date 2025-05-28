using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;

namespace TtWork.Project.Events.Queries;

public class QueryUserDtoBase : IRequest<UserDtoBase> {
    public long? UserId { get; }

    public bool FromCache { get; }

    public QueryUserDtoBase(long? userId, bool fromCache = true) {
        UserId = userId;
        FromCache = fromCache;
    }


    public class QueryUserDtoBaseHandle(
        IRepository<User, long> userRepo,
        UserCache userCache,
        IObjectMapper objectMapper)
        : IRequestHandler<QueryUserDtoBase, UserDtoBase> {
        [UnitOfWork]
        public virtual async Task<UserDtoBase> Handle(QueryUserDtoBase request, CancellationToken cancellationToken) {
            if (request.UserId.HasValue) {
                if (request.FromCache) {
                    var cache = await userCache.GetAsync(request.UserId!.Value);
                    if (cache != null)
                        return objectMapper.Map<UserDtoBase>(cache);
                }
                else {
                    var user = await userRepo.GetAll().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == request.UserId!.Value, cancellationToken: cancellationToken);

                    return objectMapper.Map<UserDtoBase>(user);
                }
            }

            return null;
        }
    }
}