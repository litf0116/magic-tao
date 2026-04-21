using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.UI;
using Microsoft.Extensions.Logging;
using TtWork.Abp;
using TtWork.Abp.Caches;
using TtWork.Abp.Core;
using TtWork.Abp.Authorization.Users;
using TtWork.Lib;
using TtWork.Project.Applications.Dto;
using TtWork.Project.Applications.Sessions.Dto;

// ReSharper disable once CheckNamespace
namespace TtWork.Project.Applications {
    public class SessionAppService : AbpAppServiceBase, ITransientDependency {
        private readonly UserCache _userCache;
        private readonly ITenantCache _tenantCache;

        public SessionAppService(
            UserCache userCache,
            ITenantCache tenantCache) {
            _userCache = userCache;
            _tenantCache = tenantCache;
        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations() {
            var output = new GetCurrentLoginInformationsOutput {
                Application = new ApplicationInfoDto {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>()
                }
            };

            if (AbpSession.TenantId.HasValue) {
                var cacheItem = await _tenantCache.GetAsync(AbpSession.TenantId.Value);
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(cacheItem);
            }

            if (AbpSession.UserId.HasValue) {
                var userId = AbpSession.UserId.Value;
                var cacheUser = await _userCache.GetAsync(userId);
                
                if (!cacheUser.IsActive) {
                    throw new UserFriendlyException(1, AppConsts.UserBanText);
                }

                output.User = ObjectMapper.Map<UserLoginInfoDto>(cacheUser);
                output.User.NeedProfileCompletion = string.IsNullOrEmpty(cacheUser.PhoneNumber) && !cacheUser.SkipProfileCompletion;
                output.User.SkipProfileCompletion = cacheUser.SkipProfileCompletion;
                
                // 从缓存获取角色和权限
                output.Roles = cacheUser.RoleNames?.ToList() ?? new List<string>();
                output.Permissions = cacheUser.Permissions ?? new List<string>();
            }

            return output;
        }
    }
}
