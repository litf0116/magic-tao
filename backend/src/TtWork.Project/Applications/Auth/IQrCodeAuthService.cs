using System.Threading.Tasks;
using TtWork.Project.Applications.Auth.Dto;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications.Auth;

public interface IQrCodeAuthService
{
    Task<QrCodeGenerateOutputDto> GenerateQrCodeAsync(long userId);
    Task<QrCodeUserInfoDto> GetUserInfoByCodeAsync(string code);
    Task<QrCodeLoginResultDto> ConfirmLoginAsync(string code);
    Task<QrCodeStatusDto> GetStatusAsync(string code);
    Task<AuthRequest> GetAuthRequestByCodeAsync(string code);
}
