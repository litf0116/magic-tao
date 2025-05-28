using Abp.Authorization;
using Abp.Domain.Uow;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Core.MultiTenancy;
using TtWork.Project.Identity;

namespace TtWork.Abp.Core.Identity {
    public class SecurityStampValidator : AbpSecurityStampValidator<Tenant, Role, User> {
        public SecurityStampValidator(
            IOptions<SecurityStampValidatorOptions> options,
            SignInManager signInManager,
            ILoggerFactory loggerFactory,
            IUnitOfWorkManager unitOfWorkManager
        )
            : base(
                options,
                signInManager,
                loggerFactory,
                unitOfWorkManager) {
        }
    }
}