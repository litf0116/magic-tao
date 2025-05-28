using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TtWork.Abp.Authorization;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Core.Editions;
using TtWork.Abp.Core.MultiTenancy;
using TtWork.Project.Identity;
using SecurityStampValidator = TtWork.Abp.Core.Identity.SecurityStampValidator;

namespace TtWork.Abp.Identity
{
    public static class IdentityRegistrar
    {
        public static IdentityBuilder Register(IServiceCollection services)
        {
            // services.AddLogging();

            return services.AddAbpIdentity<Tenant, User, Role>(option =>
                {
                    option.Password.RequireUppercase = false;
                    option.Password.RequiredLength = 6;
                    option.Password.RequireLowercase = false;
                    option.Password.RequireDigit = false;
                    option.Password.RequiredUniqueChars = 0;
                    option.Password.RequireNonAlphanumeric = false;
                })
                .AddAbpTenantManager<TenantManager>()
                .AddAbpUserManager<UserManager>()
                .AddAbpRoleManager<RoleManager>()
                .AddAbpEditionManager<EditionManager>()
                .AddAbpUserStore<UserStore>()
                .AddAbpRoleStore<RoleStore>()
                .AddAbpLogInManager<LogInManager>()
                .AddAbpSignInManager<SignInManager>()
                .AddAbpSecurityStampValidator<SecurityStampValidator>()
                .AddAbpUserClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
                .AddPermissionChecker<PermissionChecker>()
                .AddDefaultTokenProviders();
        }
    }
}
