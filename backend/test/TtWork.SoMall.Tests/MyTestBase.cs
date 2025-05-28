using Abp.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TTWork.Abp.Core.Authorization.Roles;
using TTWork.Abp.Core.Authorization.Users;
using TTWork.Domains.Organizations;
using TtWork.SoMall.Applications.Organizations.Dtos;

namespace TtWork.SoMall.Tests
{
    public partial class AbpTestBase
    {
        protected void AddTestBase(int _tenantId)
        {
            //add a id:3 user
            UsingDbContext(_context =>
            {
                var userRole = _context.Roles.FirstOrDefaultAsync(x => x.Name == StaticRoleNames.Tenants.User);

                var user3 = User.CreateTenantAdminUser(_tenantId, "tt1");
                user3.Password =
                    new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                        .HashPassword(user3, "123qwe");
                user3.IsEmailConfirmed = true;
                user3.IsActive = true;

                _context.Users.Add(user3);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, user3.Id, userRole.Id));
                var user4 = User.CreateTenantAdminUser(_tenantId, "tt2");
                user4.Password =
                    new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                        .HashPassword(user4, "123qwe");
                user4.IsEmailConfirmed = true;
                user4.IsActive = true;

                _context.Users.Add(user4);
                _context.SaveChanges();
                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, user4.Id, userRole.Id));
                _context.SaveChanges();
            });


            UsingDbContext(_context =>
            {
                var ou1 = new AppOrganizationUnit()
                {
                    DisplayName = "OU1",
                    Detail = new OrganizationUnitDetailCreateDto()
                    {
                        Address = "address",
                        Desc = "desc",
                        LogoImgUrl = "d",
                        HeadmanPhone = "1",
                        HeadmanRealName = "1",
                    },
                    Code = "001",
                    CreatorUserId = 3,
                    TenantId = 1,
                };
                _context.SoMallOrganizationUnits.Add(ou1);
                _context.SaveChanges();

                //用户3为机构1的管理
                _context.UserOrganizationUnits.Add(new UserOrganizationUnit
                {
                    UserId = 3,
                    OrganizationUnitId = ou1.Id,
                    TenantId = 1
                });
                _context.SaveChanges();
            });
        }
    }
}