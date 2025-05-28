using System.Threading.Tasks;

namespace TtWork.Abp.Core.Security
{
    public interface IRecaptchaValidator
    {
        Task ValidateAsync(string captchaResponse);
    }
}