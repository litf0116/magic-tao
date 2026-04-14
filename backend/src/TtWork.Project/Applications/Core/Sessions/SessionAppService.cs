using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.MultiTenancy;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using TtWork.Abp;
using TtWork.Abp.Caches;
using TtWork.Abp.Core;
using TtWork.Lib;
using TtWork.Project.Applications.Dto;
using TtWork.Project.Applications.Sessions.Dto;

// ReSharper disable once CheckNamespace
namespace TtWork.Project.Applications {
    public class SessionAppService(
        UserCache userCache,
        ITenantCache tenantCache
    ) : AbpAppServiceBase {
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
                var cacheItem = await tenantCache.GetAsync(AbpSession.TenantId.Value);
                output.Tenant =
                    ObjectMapper.Map<TenantLoginInfoDto>(cacheItem);
            }

            if (AbpSession.UserId.HasValue) {
                var cacheUser = await userCache.GetAsync(AbpSession.UserId.Value);

                if (!cacheUser.IsActive) {
                    throw new UserFriendlyException(1, AppConsts.UserBanText);
                }


                var roles = cacheUser.RoleNames;
                output.User = ObjectMapper.Map<UserLoginInfoDto>(cacheUser);
                output.Roles = roles.ToList();
                output.Permissions = cacheUser.Permissions;
                
                output.User.NeedProfileCompletion = string.IsNullOrEmpty(cacheUser.PhoneNumber) 
                                                     && !cacheUser.SkipProfileCompletion;
                output.User.SkipProfileCompletion = cacheUser.SkipProfileCompletion;
            }


            return output;
        }
    }
}