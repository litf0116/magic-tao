using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Definitions;
using TtWork.Project.Applications.Core.Authorization.Accounts.Dto;
using TtWork.Project.Applications.Authorization.Accounts.Dto;
using TtWork.Project.Authorization.Accounts.Dto;

namespace TtWork.Project.Applications.Core.Authorization.Accounts {
    [AbpAuthorize]
    public class AccountAppService : AbpAppServiceBase {
        public const string PasswordRegex =
            "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountAppService(
            UserRegistrationManager userRegistrationManager,
            IPasswordHasher<User> passwordHasher) {
            _userRegistrationManager = userRegistrationManager;
            _passwordHasher = passwordHasher;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input) {
            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
            if (tenant == null) {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            if (!tenant.IsActive) {
                return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
            }

            return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
        }

        [AbpAuthorize(AppPermissions.Administration)]
        public async Task<RegisterOutput> Register(RegisterInput input) {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                input.PhoneNumber,
                true
            );

            var isEmailConfirmationRequiredForLogin =
                await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                    .IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
                { CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin) };
        }

        [HttpPost]
        public async Task<bool> EnablePasswordLogin(string newPassword) {
            var user = await GetCurrentUserAsync();

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            await UserManager.UpdateAsync(user);

            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<bool> ChangePassword([FromBody] ChangePasswordInput input) {
            var user = await GetCurrentUserAsync();

            if (await UserManager.CheckPasswordAsync(user, input.CurrentPassword)) {
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.NewPassword));
                await CurrentUnitOfWork.SaveChangesAsync();
                return true;
            }

            throw new UserFriendlyException("当前密码错误");
        }

        [HttpPost]
        public async Task<bool> DisablePasswordLogin() {
            var user = await GetCurrentUserAsync();
            user.Password = null;
            await UserManager.UpdateAsync(user);
            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        [HttpGet]
        public async Task<bool> CanUsePasswordLogin() {
            var user = await GetCurrentUserAsync();
            return !string.IsNullOrEmpty(user.Password);
        }

        [HttpPost]
        public async Task<UserDto> UpdatePhone([FromBody] string phone) {
            var user = await GetCurrentUserAsync();
            CheckErrors(await UserManager.SetPhoneNumberAsync(user, phone));
            CheckErrors(await UserManager.SetUserNameAsync(user, phone));
            CheckErrors(await UserManager.ChangePasswordAsync(user,
                Regex.Matches(phone, @"\d+(\d{6})\b")[0].Groups[1].Value));
            await CurrentUnitOfWork.SaveChangesAsync();
            var roles = await UserManager.GetRolesAsync(user);
            var userDto = ObjectMapper.Map<UserDto>(user);
            userDto.RoleNames = roles.ToArray();
            return userDto;
        }
    }
}