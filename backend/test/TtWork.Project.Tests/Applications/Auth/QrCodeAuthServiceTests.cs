using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Applications.Auth;
using TtWork.Project.Domains;
using Xunit;

namespace TtWork.Project.Tests.Applications.Auth;

public class QrCodeAuthServiceTests
{
    private readonly Mock<IRepository<AuthRequest, long>> _authRequestRepoMock;
    private readonly Mock<IRepository<User, long>> _userRepoMock;
    private readonly QrCodeAuthService _service;

    public QrCodeAuthServiceTests()
    {
        _authRequestRepoMock = new Mock<IRepository<AuthRequest, long>>();
        _userRepoMock = new Mock<IRepository<User, long>>();
        
        _service = new QrCodeAuthService(
            _authRequestRepoMock.Object,
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task GetUserInfoByCodeAsync_ShouldThrowWhenCodeIsEmpty()
    {
        await Assert.ThrowsAsync<UserFriendlyException>(() => _service.GetUserInfoByCodeAsync(""));
        await Assert.ThrowsAsync<UserFriendlyException>(() => _service.GetUserInfoByCodeAsync(null));
        await Assert.ThrowsAsync<UserFriendlyException>(() => _service.GetUserInfoByCodeAsync("   "));
    }

    private static User CreateTestUser(long id, bool isActive = true) => new()
    {
        Id = id,
        UserName = "testuser",
        Name = "Test User",
        HeadImgUrl = "http://example.com/avatar.jpg",
        PhoneNumber = "13812345678",
        IsActive = isActive,
        EmailAddress = "test@example.com"
    };
}
