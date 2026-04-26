using System.Threading.Tasks;
using TtWork.Project.Applications.Auth.Dto;

namespace TtWork.Project.Applications.Auth;

public interface IQrCodeAuthService
{
    Task<QrCodeGenerateOutputDto> GenerateQrCodeAsync(long userId);
    Task<QrCodeUserInfoDto> GetUserInfoByCodeAsync(string code);
    Task<QrCodeLoginResultDto> ConfirmLoginAsync(string code, long userId);
    Task<QrCodeStatusDto> GetStatusAsync(string code);
}
