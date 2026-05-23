using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Applications.Auth.Dto;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications.Auth;

public class QrCodeAuthService : AbpAppServiceBase, IQrCodeAuthService, ITransientDependency
{
    private readonly IRepository<AuthRequest, long> _authRequestRepository;
    private readonly IRepository<User, long> _userRepository;

    private const int QrCodeExpiresInSeconds = 60;
    private const string QrCodeContentFormat = "https://www.molitao.top/h5/pages/auth/qrcode-confirm?code={0}";

    public QrCodeAuthService(
        IRepository<AuthRequest, long> authRequestRepository,
        IRepository<User, long> userRepository)
    {
        _authRequestRepository = authRequestRepository;
        _userRepository = userRepository;
    }

    public async Task<QrCodeGenerateOutputDto> GenerateQrCodeAsync(long userId)
    {
        var code = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.Now.AddSeconds(QrCodeExpiresInSeconds);

        var authRequest = new AuthRequest
        {
            Code = code,
            UserId = userId,
            Status = AuthRequestStatus.Pending,
            ExpiresAt = expiresAt
        };

        await _authRequestRepository.InsertAsync(authRequest);
        await CurrentUnitOfWork.SaveChangesAsync();

        return new QrCodeGenerateOutputDto
        {
            Code = code,
            QrContent = string.Format(QrCodeContentFormat, code),
            ExpiresIn = QrCodeExpiresInSeconds
        };
    }

    public async Task<QrCodeUserInfoDto> GetUserInfoByCodeAsync(string code)
    {
        var authRequest = await GetValidAuthRequestAsync(code, AuthRequestStatus.Pending);
        authRequest.MarkAsScanned();
        await _authRequestRepository.UpdateAsync(authRequest);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetUserDtoAsync(authRequest.UserId);
    }

    public async Task<QrCodeLoginResultDto> ConfirmLoginAsync(string code)
    {
        var authRequest = await GetValidAuthRequestAsync(code, AuthRequestStatus.Scanned);

        authRequest.MarkAsConfirmed();
        await _authRequestRepository.UpdateAsync(authRequest);
        await CurrentUnitOfWork.SaveChangesAsync();

        var user = await _userRepository.GetAsync(authRequest.UserId);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        if (!user.IsActive)
        {
            throw new UserFriendlyException("用户已被禁用");
        }

        return new QrCodeLoginResultDto
        {
            Token = null,
            TokenType = "Bearer",
            ExpiresIn = 0,
            User = await GetUserDtoAsync(authRequest.UserId)
        };
    }

    public async Task<QrCodeStatusDto> GetStatusAsync(string code)
    {
        var authRequest = await _authRequestRepository
            .GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code);

        if (authRequest == null || authRequest.Status == AuthRequestStatus.Expired)
        {
            return new QrCodeStatusDto { Status = "expired" };
        }

        if (DateTime.Now > authRequest.ExpiresAt)
        {
            return new QrCodeStatusDto { Status = "expired" };
        }

        var status = authRequest.Status switch
        {
            AuthRequestStatus.Pending => "pending",
            AuthRequestStatus.Scanned => "scanned",
            AuthRequestStatus.Confirmed => "confirmed",
            _ => "expired"
        };

        var result = new QrCodeStatusDto { Status = status };

        if (authRequest.Status == AuthRequestStatus.Confirmed)
        {
            result.User = await GetUserDtoAsync(authRequest.UserId);
        }

        return result;
    }

    private async Task<AuthRequest> GetValidAuthRequestAsync(string code, AuthRequestStatus expectedStatus)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new UserFriendlyException("二维码code不能为空");
        }

        var authRequest = await _authRequestRepository
            .GetAll()
            .FirstOrDefaultAsync(x => x.Code == code);

        if (authRequest == null)
        {
            throw new UserFriendlyException("二维码不存在");
        }

        if (DateTime.Now > authRequest.ExpiresAt)
        {
            authRequest.MarkAsExpired();
            await _authRequestRepository.UpdateAsync(authRequest);
            await CurrentUnitOfWork.SaveChangesAsync();
            throw new UserFriendlyException("二维码已过期");
        }

        if (authRequest.Status != expectedStatus)
        {
            var errorMessage = authRequest.Status switch
            {
                AuthRequestStatus.Scanned => "二维码已被扫描",
                AuthRequestStatus.Confirmed => "二维码已被使用",
                AuthRequestStatus.Expired => "二维码已过期",
                _ => "二维码状态异常"
            };
            throw new UserFriendlyException(errorMessage);
        }

        return authRequest;
    }

    public async Task<AuthRequest> GetAuthRequestByCodeAsync(string code)
    {
        return await _authRequestRepository
            .GetAll()
            .FirstOrDefaultAsync(x => x.Code == code);
    }

    private async Task<QrCodeUserInfoDto> GetUserDtoAsync(long userId)
    {
        var user = await _userRepository.GetAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new QrCodeUserInfoDto
        {
            UserId = user.Id,
            Nickname = user.Name ?? user.UserName,
            Avatar = user.HeadImgUrl,
            Phone = MaskPhoneNumber(user.PhoneNumber)
        };
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
        {
            return phone;
        }
        return phone.Substring(0, 3) + "****" + phone.Substring(phone.Length - 4);
    }
}
