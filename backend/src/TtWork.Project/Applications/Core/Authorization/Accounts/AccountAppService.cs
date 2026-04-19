using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Definitions;
using TtWork.Project.Applications.Core.Authorization.Accounts.Dto;
using TtWork.Project.Applications.Authorization.Accounts.Dto;
using TtWork.Project.Authorization.Accounts.Dto;
using TtWork.Project.Domains;
using TtWork.Project.Services;
using ProjectConsts = TtWork.Abp.Consts;
using TtWork.Project.Services;
using Consts = TtWork.Abp.Consts;

namespace TtWork.Project.Applications.Core.Authorization.Accounts {
    [AbpAuthorize]
    public class AccountAppService : AbpAppServiceBase {
        public const string PasswordRegex =
            "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<UserLogin, long> _userLoginRepository;
        private readonly ISmsVerificationCodeService _smsVerificationCodeService;

        public AccountAppService(
            UserRegistrationManager userRegistrationManager,
            IPasswordHasher<User> passwordHasher,
            IRepository<UserLogin, long> userLoginRepository,
            ISmsVerificationCodeService smsVerificationCodeService)
        {
            _userRegistrationManager = userRegistrationManager;
            _passwordHasher = passwordHasher;
            _userLoginRepository = userLoginRepository;
            _smsVerificationCodeService = smsVerificationCodeService;
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
        public async Task<bool> EnablePasswordLogin([FromBody] string newPassword) {
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
            // Database column doesn't allow null, use empty string instead
            user.Password = string.Empty;
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

        [HttpPost]
        public async Task<bool> BindPhone([FromBody] BindPhoneInput input)
        {
            var user = await GetCurrentUserAsync();
            var purpose = SmsCodePurpose.BindPhone;

            var isValid = await _smsVerificationCodeService.VerifyCodeAsync(input.PhoneNumber, input.Code, purpose);
            if (!isValid)
            {
                throw new UserFriendlyException("验证码错误或已过期");
            }

            var existingBinding = await _userLoginRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.LoginProvider == ProjectConsts.LoginProvider.Phone &&
                    x.ProviderKey == input.PhoneNumber);

            if (existingBinding != null)
            {
                if (existingBinding.UserId == user.Id)
                {
                    throw new UserFriendlyException("该手机号已绑定当前账号");
                }

                throw new UserFriendlyException("该手机号已被其他账号绑定，请使用该手机号登录后在设置中合并账号");
            }

            var currentPhoneBinding = await _userLoginRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.LoginProvider == ProjectConsts.LoginProvider.Phone);

            if (currentPhoneBinding != null)
            {
                await _userLoginRepository.DeleteAsync(currentPhoneBinding);
            }

            await _userLoginRepository.InsertAsync(new UserLogin(
                user.TenantId, user.Id,
                ProjectConsts.LoginProvider.Phone, input.PhoneNumber));

            user.PhoneNumber = input.PhoneNumber;
            user.IsPhoneNumberConfirmed = true;
            await UserManager.UpdateAsync(user);

            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        [HttpGet]
        public async Task<LoginBindingListOutput> GetLoginBindings()
        {
            var user = await GetCurrentUserAsync();
            var bindings = await _userLoginRepository.GetAll()
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            var result = new List<LoginBindingDto>();

            var phoneBinding = bindings.FirstOrDefault(x => x.LoginProvider == ProjectConsts.LoginProvider.Phone);
            result.Add(new LoginBindingDto
            {
                LoginProvider = ProjectConsts.LoginProvider.Phone,
                ProviderKey = user.PhoneNumber ?? "",
                DisplayName = "手机号",
                Icon = "phone",
                IsBound = phoneBinding != null || !string.IsNullOrEmpty(user.PhoneNumber),
                BoundTime = null
            });

            var wechatBinding = bindings.FirstOrDefault(x =>
                x.LoginProvider == Consts.LoginProvider.WeChatUnionId ||
                x.LoginProvider == Consts.LoginProvider.WeChatApp ||
                x.LoginProvider.StartsWith("WeChat"));
            if (wechatBinding != null)
            {
                result.Add(new LoginBindingDto
                {
                    LoginProvider = Consts.LoginProvider.WeChatUnionId,
                    ProviderKey = wechatBinding.ProviderKey ?? "",
                    DisplayName = "微信",
                    Icon = "wechat",
                    IsBound = true,
                    BoundTime = null
                });
            }

            result.Add(new LoginBindingDto
            {
                LoginProvider = Consts.LoginProvider.Password,
                ProviderKey = "",
                DisplayName = "登录密码",
                Icon = "lock",
                IsBound = !string.IsNullOrEmpty(user.Password),
                BoundTime = null
            });

            return new LoginBindingListOutput
            {
                Items = result
            };
        }

        [HttpPost]
        public async Task<bool> UnbindLogin([FromBody] UnbindLoginInput input)
        {
            var user = await GetCurrentUserAsync();
            var bindings = await _userLoginRepository.GetAll()
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            if (input.LoginProvider == Consts.LoginProvider.Password)
            {
                throw new UserFriendlyException("密码登录无法解绑，如需关闭请设置空密码");
            }

            var hasPassword = !string.IsNullOrEmpty(user.Password);
            var hasPhoneBinding = bindings.Any(x => x.LoginProvider == ProjectConsts.LoginProvider.Phone);
            var hasWechatBinding = bindings.Any(x =>
                x.LoginProvider == Consts.LoginProvider.WeChatUnionId ||
                x.LoginProvider == Consts.LoginProvider.WeChatApp ||
                x.LoginProvider.StartsWith("WeChat"));
            var effectiveBindingCount = (hasPhoneBinding ? 1 : 0) + (hasWechatBinding ? 1 : 0);

            if (!hasPassword && effectiveBindingCount <= 1)
            {
                throw new UserFriendlyException("至少需要保留一种登录方式，请先设置密码后再解绑");
            }

            var binding = bindings.FirstOrDefault(x => x.LoginProvider == input.LoginProvider);
            if (binding != null)
            {
                await _userLoginRepository.DeleteAsync(binding);
            }

            if (input.LoginProvider == ProjectConsts.LoginProvider.Phone && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.PhoneNumber = null;
                await UserManager.UpdateAsync(user);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }
    }
}