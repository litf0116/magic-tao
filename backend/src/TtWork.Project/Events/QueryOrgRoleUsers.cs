using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Events {
    public class QueryOrgRoleUsers : IRequest<List<User>> {
        public long OrganizationId { get; set; }

        public int RoleId { get; set; }
    }


    public class QueryOrgRoleUsersHandler : IRequestHandler<QueryOrgRoleUsers, List<User>> {
        public Task<List<User>> Handle(QueryOrgRoleUsers request, CancellationToken cancellationToken) {
            throw new System.NotImplementedException();
        }
    }
}