using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Entities.Caching;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
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
    string cacheName = CacheNames.UserCacheName)
    : EntityCache<User, UserDto, long>(cacheManager, repository,
        unitOfWorkManager, cacheName), ITransientDependency {
    /// <summary>
    /// 当缓存不存在时从数据库取
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected override async Task<UserDto> GetCacheItemFromDataSourceAsync(long id) {
        var user = await base.GetEntityFromDataSourceAsync(id);
        if (user != null) {
            var userDto = MapToCacheItem(user);

            // 初始化 Permissions 列表
            userDto.Permissions = new List<string>();
            
            var roles = await userManager.GetRolesAsync(user);
            userDto.RoleNames = roles.ToArray();

            foreach (var role in userDto.RoleNames) {
                var grantedPermissions = await roleManager.GetGrantedPermissionsAsync(role);
                foreach (var p in grantedPermissions) {
                    if (userDto.Permissions.All(z => z != p.Name)) {
                        userDto.Permissions.Add(p.Name);
                    }
                }
            }

            return userDto;
        }

        return null;
    }
}
