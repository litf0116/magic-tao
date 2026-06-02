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
        public async Task<BindPhoneResult> BindPhoneWithPassword([FromBody] BindPhoneWithPasswordInput input)
        {
            if (string.IsNullOrWhiteSpace(input.PhoneNumber) ||
                !System.Text.RegularExpressions.Regex.IsMatch(input.PhoneNumber, @"^1[3-9]\d{9}$"))
            {
                throw new UserFriendlyException("请输入正确的手机号");
            }

            if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 8)
            {
                throw new UserFriendlyException("密码至少8位");
            }

            var user = await GetCurrentUserAsync();

            // 检查手机号是否已被其他用户绑定
            var existingBinding = await _userLoginRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.LoginProvider == ProjectConsts.LoginProvider.Phone &&
                    x.ProviderKey == input.PhoneNumber &&
                    x.UserId != user.Id);

            if (existingBinding != null)
            {
                throw new UserFriendlyException("该手机号已被注册，请使用手机号密码登录");
            }

            var existingUserWithPhone = await UserManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == input.PhoneNumber && x.Id != user.Id);

            if (existingUserWithPhone != null)
            {
                throw new UserFriendlyException("该手机号已被注册，请使用手机号密码登录");
            }

            // 如果当前用户的手机号已绑定（从abpuserlogins查到），只更新密码
            var currentUserBinding = await _userLoginRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.LoginProvider == ProjectConsts.LoginProvider.Phone &&
                    x.ProviderKey == input.PhoneNumber &&
                    x.UserId == user.Id);

            var hashedPassword = _passwordHasher.HashPassword(user, input.Password);
            user.Password = hashedPassword;
            user.PhoneNumber = input.PhoneNumber;
            user.IsPhoneNumberConfirmed = true;
            await UserManager.UpdateAsync(user);

            // 只有新绑定手机号时才插入 login 记录
            if (currentUserBinding == null)
            {
                await _userLoginRepository.InsertAsync(new UserLogin(
                    user.TenantId, user.Id,
                    ProjectConsts.LoginProvider.Phone, input.PhoneNumber));
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return new BindPhoneResult
            {
                UserId = user.Id,
                UserName = user.UserName
            };
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

            var appleBinding = bindings.FirstOrDefault(x => x.LoginProvider == Consts.LoginProvider.Apple);
            if (appleBinding != null)
            {
                result.Add(new LoginBindingDto
                {
                    LoginProvider = Consts.LoginProvider.Apple,
                    ProviderKey = appleBinding.ProviderKey ?? "",
                    DisplayName = "Apple",
                    Icon = "apple",
                    IsBound = true,
                    BoundTime = null
                });
            }

            return new LoginBindingListOutput
            {
                Items = result
            };
        }

        [HttpPost]
        public async Task<bool> UnbindLogin([FromBody] UnbindLoginInput input)
        {
            if (input.LoginProvider == ProjectConsts.LoginProvider.Phone)
            {
                throw new UserFriendlyException("手机号不支持解绑，如需更换手机号请使用更换功能");
            }

            if (input.LoginProvider == Consts.LoginProvider.Password)
            {
                throw new UserFriendlyException("密码登录无法解绑，如需关闭请设置空密码");
            }

            var user = await GetCurrentUserAsync();
            var bindings = await _userLoginRepository.GetAll()
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            var binding = bindings.FirstOrDefault(x => x.LoginProvider == input.LoginProvider);
            if (binding == null)
            {
                return true;
            }

            var hasPassword = !string.IsNullOrEmpty(user.Password);
            var hasPhoneBinding = bindings.Any(x => x.LoginProvider == ProjectConsts.LoginProvider.Phone);
            var hasOtherBinding = bindings.Any(x => 
                x.LoginProvider != input.LoginProvider && 
                x.LoginProvider != ProjectConsts.LoginProvider.Phone);

            if (!hasPassword && !hasPhoneBinding && !hasOtherBinding)
            {
                throw new UserFriendlyException("至少需要保留一种登录方式");
            }

            await _userLoginRepository.DeleteAsync(binding);
            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 注销账号 - 清除个人信息并禁用登录
        /// </summary>
        [HttpPost]
        public async Task<bool> DeleteAccount([FromBody] DeleteAccountInput input)
        {
            var user = await GetCurrentUserAsync();

            // 1. 验证密码
            if (!await UserManager.CheckPasswordAsync(user, input.Password))
            {
                throw new UserFriendlyException("密码错误");
            }

            // 2. 匿名化个人信息
            var anonymousSuffix = $"_{user.Id}_{DateTime.Now:yyyyMMdd}";
            user.Name = "已注销";
            user.Surname = "已注销";
            user.UserName = $"deleted{anonymousSuffix}";
            user.EmailAddress = $"deleted{anonymousSuffix}@molitao.top";
            user.PhoneNumber = null;
            user.HeadImgUrl = null;
            user.Qq = null;
            user.Wx = null;
            user.Password = null;
            user.IsActive = false;

            await UserManager.UpdateAsync(user);
            await CurrentUnitOfWork.SaveChangesAsync();

            return true;
        }
    }
}