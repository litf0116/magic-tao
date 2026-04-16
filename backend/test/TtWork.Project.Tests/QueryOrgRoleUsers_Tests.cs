using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Moq;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Events;

namespace TtWork.Project.Tests;

public class QueryOrgRoleUsers_Tests
{
    [Fact]
    public async Task Should_Return_Empty_List_When_OrganizationId_Is_Zero()
    {
        var userRepositoryMock = new Mock<IRepository<User, long>>();
        var userOrgUnitRepositoryMock = new Mock<IRepository<UserOrganizationUnit, long>>();
        var userRoleRepositoryMock = new Mock<IRepository<UserRole, long>>();

        var handler = new QueryOrgRoleUsersHandler(
            userRepositoryMock.Object,
            userOrgUnitRepositoryMock.Object,
            userRoleRepositoryMock.Object
        );

        var query = new QueryOrgRoleUsers { OrganizationId = 0, RoleId = 1 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_RoleId_Is_Zero()
    {
        var userRepositoryMock = new Mock<IRepository<User, long>>();
        var userOrgUnitRepositoryMock = new Mock<IRepository<UserOrganizationUnit, long>>();
        var userRoleRepositoryMock = new Mock<IRepository<UserRole, long>>();

        var handler = new QueryOrgRoleUsersHandler(
            userRepositoryMock.Object,
            userOrgUnitRepositoryMock.Object,
            userRoleRepositoryMock.Object
        );

        var query = new QueryOrgRoleUsers { OrganizationId = 1, RoleId = 0 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_Both_Are_Negative()
    {
        var userRepositoryMock = new Mock<IRepository<User, long>>();
        var userOrgUnitRepositoryMock = new Mock<IRepository<UserOrganizationUnit, long>>();
        var userRoleRepositoryMock = new Mock<IRepository<UserRole, long>>();

        var handler = new QueryOrgRoleUsersHandler(
            userRepositoryMock.Object,
            userOrgUnitRepositoryMock.Object,
            userRoleRepositoryMock.Object
        );

        var query = new QueryOrgRoleUsers { OrganizationId = -1, RoleId = -1 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
