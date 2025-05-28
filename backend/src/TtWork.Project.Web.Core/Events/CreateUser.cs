using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Nest;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Web.Events;

public class CreateUser : MediatR.IRequest<User> {
    public User User { get; }

    public CreateUser(User user) {
        User = user;
    }

    public class CreateUserHandler(
        IRepository<User, long> userRepository,
        IPasswordHasher<User> passwordHasher
    ) : IRequestHandler<CreateUser, User> {
        [UnitOfWork]
        public virtual async Task<User> Handle(CreateUser request, CancellationToken cancellationToken) {
            request.User.TenantId = 1;
            request.User.Password = passwordHasher.HashPassword(request.User, User.CreateRandomPassword());
            request.User.SetNormalizedNames();
            await userRepository.InsertAsync(request.User);
            return request.User;
        }
    }
}