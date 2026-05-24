using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Abp.Identity
{
    /// <summary>
    /// 放宽的 UserName 验证器：不限制 UserName 字符集（如中文、特殊符号等）
    /// ASP.NET Core Identity 默认的 UserValidator 只允许 ASCII 字符，导致中文用户名更新失败
    /// </summary>
    public class RelaxedUserValidator : IUserValidator<User>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidUserName",
                    Description = "用户名不能为空"
                }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
