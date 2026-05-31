using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Controllers;

namespace TtWork.Project.Web.Core.Controllers
{
    [Route("api/localdev")]
    public class LocalDevController : AbpControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<LocalDevController> _logger;

        public LocalDevController(
            UserManager userManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<LocalDevController> logger)
        {
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordInput input)
        {
            if (!IsLocalRequest())
            {
                return Forbid();
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user == null)
            {
                return NotFound($"用户ID {input.UserId} 不存在");
            }

            // 使用 UserManager.ChangePasswordAsync 正确设置密码
            // 这会同时更新 Password 和 SecurityStamp
            var result = await _userManager.ChangePasswordAsync(user, input.NewPassword);
            result.CheckErrors(LocalizationManager);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            _logger.LogInformation("本地开发接口: 用户ID {UserId} 密码已重置", input.UserId);

            return Ok(new { message = $"用户ID {input.UserId} 密码已重置为 {input.NewPassword}" });
        }

        private bool IsLocalRequest()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return remoteIp == "127.0.0.1" || remoteIp == "::1" || remoteIp == "::ffff:127.0.0.1";
        }
    }

    public class ResetPasswordInput
    {
        public long UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
