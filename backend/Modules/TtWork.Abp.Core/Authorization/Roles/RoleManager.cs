using System.Collections.Generic;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Authorization.Roles {
    public class RoleManager(
        RoleStore store,
        IEnumerable<IRoleValidator<Role>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<AbpRoleManager<Role, User>> logger,
        IPermissionManager permissionManager,
        ICacheManager cacheManager,
        IUnitOfWorkManager unitOfWorkManager,
        IRoleManagementConfig roleManagementConfig,
        IRepository<OrganizationUnit, long> organizationUnitRepository,
        IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
        : AbpRoleManager<Role, User>(store,
            roleValidators,
            keyNormalizer,
            errors, logger,
            permissionManager,
            cacheManager,
            unitOfWorkManager,
            roleManagementConfig,
            organizationUnitRepository,
            organizationUnitRoleRepository);
}