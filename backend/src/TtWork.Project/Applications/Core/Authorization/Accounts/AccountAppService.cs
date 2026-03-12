using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Applications.Authorization.Accounts.Dto;
using TtWork.Project.Authorization.Accounts.Dto;

namespace TtWork.Project.Applications.Core.Authorization.Accounts {
    [AbpAuthorize]
    public class AccountAppService : AbpAppServiceBase {
        public const string PasswordRegex =
            "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;

        public AccountAppService(UserRegistrationManager userRegistrationManager) {
            _userRegistrationManager = userRegistrationManager;
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

        [AbpAuthorize("Admin")]
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
            
            await UserManager.InitializePasswordAsync(user);
            CheckErrors(await UserManager.ChangePasswordAsync(user, newPassword));
            
            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<bool> ChangePassword(string currentPassword, string newPassword) {
            var user = await GetCurrentUserAsync();
            
            if (await UserManager.CheckPasswordAsync(user, currentPassword)) {
                CheckErrors(await UserManager.ChangePasswordAsync(user, newPassword));
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

        [AbpAuthorize("Admin")]
        public async Task<RegisterOutput> Register(RegisterInput input) {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                input.PhoneNumber,
                true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
            );

            var isEmailConfirmationRequiredForLogin =
                await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                    .IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
                { CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin) };
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

        public async Task<RegisterOutput> Register(RegisterInput input) {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                input.PhoneNumber,
                true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
            );

            var isEmailConfirmationRequiredForLogin =
                await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                    .IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
                { CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin) };
        }


        [HttpPost]
        public async Task<UserDto> UpdatePhone([FromBody] string phone) {
            var user = await GetCurrentUserAsync();
            CheckErrors(await UserManager.SetPhoneNumberAsync(user, phone));
            CheckErrors(await UserManager.SetUserNameAsync(user, phone));
            //TODO:暂时为密码自动修改为手机号码后6位
            CheckErrors(await UserManager.ChangePasswordAsync(user,
                Regex.Matches(phone, @"\d+(\d{6})\b")[0].Groups[1].Value));
            await CurrentUnitOfWork.SaveChangesAsync();
            var roles = await UserManager.GetRolesAsync(user);
            var userDto = ObjectMapper.Map<UserDto>(user);
            userDto.RoleNames = roles.ToArray();
            //var roles = await _roleManager.Roles.Where(r => roles.Any(ur => ur.RoleId == r.Id)).Select(r => r.NormalizedName).ToListAsync();
            return userDto;
        }
    }
}