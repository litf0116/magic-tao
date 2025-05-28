using System.Threading;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Core.Events.Queries
{
    /// <summary>
    /// 获取用户指定LoginPrivider的LoginKey,找不到返回null
    /// </summary>
    public class UserLoginKeyQuery : IRequest<string>
    {
        public long UserId { get; }
        public string LoginProvider { get; }

        public UserLoginKeyQuery(long userId, string loginProvider)
        {
            UserId = userId;
            LoginProvider = loginProvider;
        }


        public class UserLoginKeyQueryHandle : IRequestHandler<UserLoginKeyQuery, string>
        {
            private readonly IRepository<UserLogin, long> _userLoginRepositoryRepository;

            public UserLoginKeyQueryHandle(IRepository<UserLogin, long> userLoginRepository)
            {
                _userLoginRepositoryRepository = userLoginRepository;
            }

            [UnitOfWork]
            public virtual async Task<string> Handle(UserLoginKeyQuery request, CancellationToken cancellationToken)
            {
                var entityWithNoTrack = await _userLoginRepositoryRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LoginProvider == request.LoginProvider, cancellationToken: cancellationToken);
                
                return entityWithNoTrack?.ProviderKey;
            }
        }
    }
}