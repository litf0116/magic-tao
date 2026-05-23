using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Entities.Caching;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Caches;

/// <summary>
/// 用户缓存
/// </summary>
public class UserCache(
    ICacheManager cacheManager,
    IRepository<User, long> repository,
    IUnitOfWorkManager unitOfWorkManager,
    UserManager userManager,
    RoleManager roleManager,
    ILogger<UserCache> logger,
    string cacheName = CacheNames.UserCacheName)
    : EntityCache<User, UserDto, long>(cacheManager, repository,
        unitOfWorkManager, cacheName), ITransientDependency {
    /// <summary>
    /// 当缓存不存在时从数据库取
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected override async Task<UserDto> GetCacheItemFromDataSourceAsync(long id) {
        logger.LogDebug("[UserCache]缓存未命中，从数据库加载 UserId={UserId}", id);
        var user = await base.GetEntityFromDataSourceAsync(id);
        if (user != null) {
            logger.LogDebug("[UserCache]获取用户成功 UserId={UserId}, UserName={UserName}",
                user.Id, user.UserName);
            var userDto = MapToCacheItem(user);

            // 初始化 Permissions 列表
            userDto.Permissions = new List<string>();
            
            try
            {
                logger.LogDebug("[UserCache]调用 GetRolesAsync UserId={UserId}", user.Id);
                var roles = await userManager.GetRolesAsync(user);
                userDto.RoleNames = roles.ToArray();
                logger.LogDebug("[UserCache]GetRolesAsync 成功 UserId={UserId}, Roles={Roles}",
                    user.Id, string.Join(",", roles));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UserCache]GetRolesAsync 异常 UserId={UserId}", user.Id);
                throw;
            }

            foreach (var role in userDto.RoleNames) {
                var grantedPermissions = await roleManager.GetGrantedPermissionsAsync(role);
                foreach (var p in grantedPermissions) {
                    if (userDto.Permissions.All(z => z != p.Name)) {
                        userDto.Permissions.Add(p.Name);
                    }
                }
            }

            logger.LogDebug("[UserCache]缓存加载完成 UserId={UserId}, RoleCount={RoleCount}",
                user.Id, userDto.RoleNames.Length);
            return userDto;
        }

        logger.LogWarning("[UserCache]用户不存在 UserId={UserId}", id);
        return null;
    }
}
