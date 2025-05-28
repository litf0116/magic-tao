using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Identity;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.MultiTenancy;

namespace TtWork.Abp {
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class AbpAppServiceBase : ApplicationService {
        public TenantManager TenantManager { get; set; }
        public UserManager UserManager { get; set; }

        /// <summary>
        /// AbpAppServiceBase
        /// </summary>
        protected AbpAppServiceBase() {
            LocalizationSourceName = AbpConsts.LocalizationSourceName;
            PermissionChecker = NullPermissionChecker.Instance;
        }

        /// <summary>
        /// GetCurrentUserAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<User> GetCurrentUserAsync() {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null) {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identityResult"></param>
        protected virtual void CheckErrors(IdentityResult identityResult) {
            identityResult.CheckErrors(LocalizationManager);
        }

        /// <summary>
        /// IsInRole
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        protected async Task<bool> IsInRole(long userId, string roleName) {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var roles = await UserManager.GetRolesAsync(user);
            return roles.Any(x => String.Equals(x, roleName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// IsInRoles
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleNames"></param>
        /// <returns></returns>
        protected async Task<bool> IsInRoles(long userId, List<string> roleNames) {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var roles = await UserManager.GetRolesAsync(user);
            return roleNames.Any(roleName => roles.Any(x => String.Equals(x, roleName, StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}