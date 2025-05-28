using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using MediatR;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Core.Events.Queries
{
    public class UserQuery : IRequest<User>
    {
        public UserQuery(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }
    }

    public class UserQueryHandle : IRequestHandler<UserQuery, User>
    {
        private readonly IRepository<User, long> _repository;

        public UserQueryHandle(IRepository<User, long> repository)
        {
            _repository = repository;
        }

        [UnitOfWork]
        public virtual async Task<User> Handle(UserQuery request, CancellationToken cancellationToken)
        {
            return await _repository.FirstOrDefaultAsync(x => x.Id == request.UserId);
        }
    }
}