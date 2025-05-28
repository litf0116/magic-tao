using System.Collections.Generic;
using System.Linq;
using Abp.Authorization.Roles;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Definitions;
using TtWork.Project.Core;

namespace TtWork.Project.EntityFrameworkCore.Seed.Tenants {
    public class ProjectRoleAndUserBuilder {
        private readonly AbpDbContext _context;
        private readonly int _tenantId;

        public ProjectRoleAndUserBuilder(AbpDbContext context, int tenantId) {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create() {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers() {
            List<string> permissionList = [
                AppPermissions.Pages.Auction,
                AppPermissions.Pages.Default,
                AppPermissions.Pages.ChatManager
            ];

            CreateRole(ProjectRoles.竞拍用户, nameof(ProjectRoles.竞拍用户), [AppPermissions.Pages.Auction], isStatic: true,
                isDefault: false);

            CreateRole(ProjectRoles.聊天室管理, nameof(ProjectRoles.聊天室管理), permissionList, isStatic: true,
                isDefault: false);

            CreateRole(ProjectRoles.拍卖师, nameof(ProjectRoles.拍卖师),
                [..permissionList, AppPermissions.Pages.AuctionManager], isStatic: true, isDefault: false);
        }

        /// <summary>
        /// CreateRole
        /// </summary>
        /// <param name="roleName">角色名</param>
        /// <param name="displayName">显示名称</param>
        /// <param name="permissionList">初始权限列表</param>
        /// <param name="isStatic">是否能修改</param>
        /// <param name="isDefault">是否默认</param>
        private void CreateRole(string roleName, string displayName, IEnumerable<string> permissionList,
            bool isStatic = true, bool isDefault = false) {
            var role = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == roleName);
            if (role == null) {
                role = _context.Roles.Add(new Role(_tenantId, roleName, displayName)
                    { IsStatic = isStatic, IsDefault = isDefault }).Entity;
                _context.SaveChanges();
                foreach (var p in permissionList)

                    _context.Permissions.Add(
                        new RolePermissionSetting {
                            TenantId = _tenantId,
                            Name = p,
                            IsGranted = true,
                            RoleId = role.Id
                        });
                _context.SaveChanges();
            }
            else {
                // Grant all permissions to admin role
                // 数据库中已有权限
                var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                    .OfType<RolePermissionSetting>()
                    .Where(p => p.TenantId == _tenantId
                                && p.RoleId == role.Id
                    )
                    .Select(p => p.Name)
                    .ToList();

                // 和数据库的差集
                var permissions = permissionList
                    .Where(p => !grantedPermissions.Contains(p))
                    .ToList();

                if (permissions.Any()) {
                    _context.Permissions.AddRange(
                        permissions.Select(permission => new RolePermissionSetting {
                            TenantId = _tenantId,
                            Name = permission,
                            IsGranted = false,
                            RoleId = role.Id
                        })
                    );
                    _context.SaveChanges();
                }
            }
        }
    }
}