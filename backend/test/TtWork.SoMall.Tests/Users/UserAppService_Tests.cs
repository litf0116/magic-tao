using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TTWork.Abp.Core.Applications.Dtos;
using TtWork.SoMall.Applications.Users;
using TtWork.SoMall.Applications.Users.Dto;
using Xunit;

namespace TtWork.SoMall.Tests.Users
{
    public class UserAppService_Tests : AbpTestBase
    {
        private readonly IUserAppService _userAppService;

        public UserAppService_Tests()
        {
            _userAppService = Resolve<IUserAppService>();
        }

        [Fact]
        public async Task GetUsers_Test()
        {
            await UsingDbContextAsync(async context =>
            {
                var list = await context.Users.IgnoreQueryFilters().OrderBy(X => X.Id).ToListAsync();

                list[0].TenantId.ShouldBe(null);
                list[0].UserName.ShouldBe("admin");
                list[0].Id.ShouldBe(1);
                list[1].TenantId.ShouldBe(1);
                list[1].UserName.ShouldBe("admin");
                list[1].Id.ShouldBe(2);

                (await context.Users.IgnoreQueryFilters().CountAsync()).ShouldBe(4);
            });


            // Act
            var output = await _userAppService.GetAllAsync(new AppResultRequestDto {MaxResultCount = 20, SkipCount = 0});

            // Assert
            output.Items.Count.ShouldBe(3);
        }

        [Fact]
        public async Task CreateUser_Test()
        {
            // Act
            await _userAppService.CreateAsync(
                new CreateUserDto
                {
                    EmailAddress = "john@volosoft.com",
                    IsActive = true,
                    Name = "John",
                    Surname = "Nash",
                    Password = "123qwe",
                    UserName = "john.nash"
                });

            await UsingDbContextAsync(async context =>
            {
                var johnNashUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "john.nash");
                johnNashUser.ShouldNotBeNull();
            });
        }
    }
}