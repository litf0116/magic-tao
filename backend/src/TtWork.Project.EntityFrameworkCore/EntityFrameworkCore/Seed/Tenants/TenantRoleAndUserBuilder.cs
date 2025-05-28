using System.Collections.Generic;
using System.Linq;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Definitions;
using TtWork.Project.EntityFrameworkCore;

namespace TtWork.Project.EntityFrameworkCore.Seed.Tenants {
    public class TenantRoleAndUserBuilder {
        private readonly AbpDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(AbpDbContext context, int tenantId) {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create() {
            CreateRolesAndUsers();
        }

        private void CreateRole(string roleName, string displayName, IEnumerable<string> permission) {
            var role = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == roleName);
            if (role == null) {
                role = _context.Roles.Add(new Role(_tenantId, roleName, displayName) { IsStatic = true }).Entity;
                _context.SaveChanges();
                foreach (var p in permission)

                    _context.Permissions.Add(
                        new RolePermissionSetting {
                            TenantId = _tenantId,
                            Name = p,
                            IsGranted = true,
                            RoleId = role.Id
                        });
                _context.SaveChanges();
            }
        }


        private void CreateRolesAndUsers() {
            #region admin角色初始化

            var adminRole = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null) {
                adminRole = _context.Roles
                    .Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin_CN)
                        { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            // Grant all permissions to admin role
            // 数据库中已有权限
            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId
                            && p.RoleId == adminRole.Id
                )
                .Select(p => p.Name)
                .ToList();

            var allPermissions = PermissionFinder
                .GetAllPermissions(
                    new AbpAuthorizationProvider());

            // 和数据库的差集
            var permissions = allPermissions.Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
                                                        !grantedPermissions.Contains(p.Name))
                .ToList();

            if (permissions.Any()) {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id
                    })
                );
                _context.SaveChanges();
            }


            var adminUser = _context.Users.IgnoreQueryFilters()
                .FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null) {
                adminUser = User.CreateTenantAdminUser(_tenantId, "admin@wujiangapp.com");
                adminUser.Password =
                    new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                        .HashPassword(adminUser, "321ewq");
                adminUser.IsEmailConfirmed = true;
                adminUser.IsActive = true;

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }

            #endregion

            #region 机构角色初始化

            // CreateRole(StaticRoleNames.Tenants.Organize, "单位角色", new[] {
            //     AppPermissions.Pages.Default,
            //     AppPermissions.Pages.Organization.Default
            // });

            #endregion

            #region User角色初始化

            // var userRole = _context.Roles.IgnoreQueryFilters()
            //     .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.User);
            // if (userRole == null) {
            //     userRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.User, "用户角色")
            //         { IsStatic = true, IsDefault = true }).Entity;
            //     _context.SaveChanges();
            //
            //     foreach (var p in new[] {
            //                  AppPermissions.Pages.Default
            //              }) {
            //         _context.Permissions.Add(
            //             new RolePermissionSetting() {
            //                 TenantId = _tenantId,
            //                 Name = p,
            //                 IsGranted = true,
            //                 RoleId = userRole.Id
            //             });
            //     }
            //
            //     _context.SaveChanges();
            // }
            // else {
            //     if (!userRole.IsDefault) {
            //         userRole.IsDefault = true;
            //         _context.SaveChanges();
            //     }
            // }

            #endregion
        }
    }
}