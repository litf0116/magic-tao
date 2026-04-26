using System;
using System.Collections.Generic;
using System.Linq;
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

    [Fact]
    public async Task ConfirmLoginAsync_ShouldThrowWhenUserIdMismatch()
    {
        var code = "mismatch-code";
        var authRequest = new AuthRequest
        {
            Id = 1,
            Code = code,
            UserId = 1L,
            Status = AuthRequestStatus.Scanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        SetupAuthRequestSingle(authRequest);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ConfirmLoginAsync(code, 999L));
        
        exception.Message.ShouldBe("用户身份不匹配");
    }

    [Fact]
    public async Task ConfirmLoginAsync_ShouldThrowWhenUserNotFound()
    {
        var code = "user-not-found-code";
        var userId = 999L;
        var authRequest = new AuthRequest
        {
            Id = 1,
            Code = code,
            UserId = userId,
            Status = AuthRequestStatus.Scanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        SetupAuthRequestSingle(authRequest);
        _userRepoMock.Setup(x => x.GetAsync(userId)).ReturnsAsync((User)null!);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ConfirmLoginAsync(code, userId));
        
        exception.Message.ShouldBe("用户不存在");
    }

    [Fact]
    public async Task ConfirmLoginAsync_ShouldThrowWhenUserInactive()
    {
        var code = "inactive-user-code";
        var userId = 1L;
        var user = CreateTestUser(userId, isActive: false);
        var authRequest = new AuthRequest
        {
            Id = 1,
            Code = code,
            UserId = userId,
            Status = AuthRequestStatus.Scanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        SetupAuthRequestSingle(authRequest);
        _userRepoMock.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ConfirmLoginAsync(code, userId));
        
        exception.Message.ShouldBe("用户已被禁用");
    }

    [Fact]
    public async Task ConfirmLoginAsync_ShouldThrowWhenCodeAlreadyConfirmed()
    {
        var code = "already-confirmed-code";
        var authRequest = new AuthRequest
        {
            Id = 1,
            Code = code,
            UserId = 1L,
            Status = AuthRequestStatus.Confirmed,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        SetupAuthRequestSingle(authRequest);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => _service.ConfirmLoginAsync(code, 1L));
        
        exception.Message.ShouldBe("二维码已被使用");
    }

    private void SetupAuthRequestSingle(AuthRequest authRequest)
    {
        var list = new List<AuthRequest> { authRequest }.AsQueryable();
        var mockSet = new Mock<DbSet<AuthRequest>>();
        
        mockSet.As<IQueryable<AuthRequest>>()
            .Setup(m => m.Provider)
            .Returns(list.Provider);

        mockSet.As<IQueryable<AuthRequest>>()
            .Setup(m => m.Expression)
            .Returns(list.Expression);

        mockSet.As<IQueryable<AuthRequest>>()
            .Setup(m => m.ElementType)
            .Returns(list.ElementType);

        mockSet.As<IQueryable<AuthRequest>>()
            .Setup(m => m.GetEnumerator())
            .Returns(list.GetEnumerator());

        _authRequestRepoMock.Setup(x => x.GetAll()).Returns(mockSet.Object);
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
