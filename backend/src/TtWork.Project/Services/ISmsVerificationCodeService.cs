using System.Threading.Tasks;
using TtWork.Project.Domains;

namespace TtWork.Project.Services;

public interface ISmsVerificationCodeService
{
    Task SendCodeAsync(string phoneNumber, SmsCodePurpose purpose);
    Task<bool> VerifyCodeAsync(string phoneNumber, string code, SmsCodePurpose purpose);
}