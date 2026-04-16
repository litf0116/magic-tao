using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Events {
    public class QueryOrgRoleUsers : IRequest<List<User>> {
        public long OrganizationId { get; set; }

        public int RoleId { get; set; }
    }


    public class QueryOrgRoleUsersHandler : IRequestHandler<QueryOrgRoleUsers, List<User>> {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;

        public QueryOrgRoleUsersHandler(
            IRepository<User, long> userRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<UserRole, long> userRoleRepository) {
            _userRepository = userRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _userRoleRepository = userRoleRepository;
        }

        public virtual async Task<List<User>> Handle(QueryOrgRoleUsers request, CancellationToken cancellationToken) {
            if (request.OrganizationId <= 0 || request.RoleId <= 0) {
                return new List<User>();
            }

            var users = await (
                from u in _userRepository.GetAll()
                join uou in _userOrganizationUnitRepository.GetAll() on u.Id equals uou.UserId
                join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                where uou.OrganizationUnitId == request.OrganizationId
                   && ur.RoleId == request.RoleId
                select u
            ).Distinct().ToListAsync(cancellationToken);

            return users;
        }
    }
}