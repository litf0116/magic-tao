using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Definitions;
using TtWork.HttpClient.Weixin;
using TtWork.Lib;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.Core.Users.Dto;
using TtWork.Project.Applications.Users.Dto;
using TtWork.Project.Domains.Pays;
using TtWork.Project.Roles.Dto;
using TtWork.Project.Users.Dto;
using TtWork.Abp.Entity;
using SqlSugar;

namespace TtWork.Project.Applications.Core.Users
{
    /// <summary>
    /// 用户API
    /// </summary>
    public class UserAppService :
        AbpAsyncCrudAppService<User, UserDto, long, AppResultRequestDto, CreateUserDto, UserEditDto>
    {
        private readonly IWeixinApi _weixinApi;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly UserCache _userCache;
        private readonly ITenantCache _tenantCache;
        private readonly IRedisClient _redisClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserAppService> _logger;
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly ChatUserCache _chatUserCache;
        private readonly IOptions<WechatSettings> _wechatSettings;

        public UserAppService(
            IRedisClient redisClient,
            IRepository<User, long> repository,
            IocManager iocManager,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            UserCache userCache,
            ITenantCache tenantCache,
            IWeixinApi weixinApi,
            IHttpClientFactory httpClientFactory,
            ILogger<UserAppService> logger,
            ISqlSugarClient sqlSugarClient,
            ChatUserCache chatUserCache,
            IOptions<WechatSettings> wechatSettings
        )
            : base(repository, iocManager)
        {
            _redisClient = redisClient;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _userCache = userCache;
            _tenantCache = tenantCache;
            _weixinApi = weixinApi;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _sqlSugarClient = sqlSugarClient;
            _chatUserCache = chatUserCache;
            _wechatSettings = wechatSettings;

            base.GetAllPermissionName = AppPermissions.Administration;
            base.DeletePermissionName = AppPermissions.Administration;
            base.UpdatePermissionName = AppPermissions.Administration;
            base.CreatePermissionName = AppPermissions.Administration;
        }

        [AbpAuthorize]
        public override async Task<UserDto> GetAsync(EntityDto<long> input)
        {
            var userId = _abpSession.UserId!;
            if (input.Id != userId)
            {
                //限制1秒内查看次数
                var key = $"GetUser_{userId}";
                var count = await _redisClient.Database.StringGetAsync(key);
                if (count.HasValue)
                {
                    throw new UserFriendlyException("操作频繁,请稍后再试!");
                }
                else
                {
                    await _redisClient.Database.StringSetAsync(key, 1, TimeSpan.FromSeconds(1));
                }
            }

            var result = await base.GetAsync(input);
            if (!result.IsActive)
            {
                // 禁用的用户管理员以外人的看不到头像和昵称
                if (!await base.IsAdminAsync())
                {
                    result.Name = "用户已封号";
                    result.HeadImgUrl = AppConsts.UserDefaultAvatar;
                }
            }

            return result;
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);
            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = false;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
            CheckErrors(await _userManager.CreateAsync(user, input.Password));
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapToEntityDto(user);
        }

        [AbpAuthorize]
        public async Task<GetUserForEditOutput> GetCurrentUser()
        {
            _logger.LogDebug("[GetCurrentUser]入口 UserId={UserId}", _abpSession.UserId);
            var output = new GetUserForEditOutput();

            try
            {
                var user = await _userManager.GetUserByIdAsync(_abpSession.UserId!.Value);
                _logger.LogDebug("[GetCurrentUser]获取用户成功 UserId={UserId}, UserName={UserName}, Name={Name}",
                    user.Id, user.UserName, user.Name);

                output.User = ObjectMapper.Map<UserEditDto>(user);
                output.HeadImgUrl = user.HeadImgUrl;

                var organizationUnits = await _userManager.GetOrganizationUnitsAsync(user);
                output.MemberedOrganizationUnits = organizationUnits.Select(ou => ou.Code).ToList();
                _logger.LogDebug("[GetCurrentUser]完成 UserId={UserId}", _abpSession.UserId);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetCurrentUser]异常 UserId={UserId}", _abpSession.UserId);
                throw;
            }
        }

        [AbpAuthorize(AppPermissions.Administration)]
        public async Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input)
        {
            //Getting all available roles
            var roleDtoList = await _roleManager.Roles
                .OrderBy(r => r.DisplayName)
                .Select(r => new UserRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDisplayName = r.DisplayName
                })
                .ToArrayAsync();

            var output = new GetUserForEditOutput
            {
                Roles = roleDtoList,
                MemberedOrganizationUnits = new List<string>()
            };

            if (!input.Id.HasValue || input.Id == 0)
            {
                //Creating a new user
                output.User = new UserEditDto
                {
                    IsActive = true,
                    //ShouldChangePasswordOnNextLogin = true, 
                };

                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    var defaultUserRole = roleDtoList.FirstOrDefault(ur => ur.RoleName == defaultRole.Name);
                    if (defaultUserRole != null)
                    {
                        defaultUserRole.IsAssigned = true;
                    }
                }
            }
            else
            {
                //Editing an existing user
                var user = await _userManager.GetUserByIdAsync(input.Id.Value);

                output.User = ObjectMapper.Map<UserEditDto>(user);
                output.HeadImgUrl = user.HeadImgUrl;

                foreach (var userRoleDto in roleDtoList)
                {
                    userRoleDto.IsAssigned = await _userManager.IsInRoleAsync(user, userRoleDto.RoleName);
                }

                var organizationUnits = await _userManager.GetOrganizationUnitsAsync(user);
                output.MemberedOrganizationUnits = organizationUnits.Select(ou => ou.Code).ToList();
            }

            return output;
        }


        [HttpPost]
        [AbpAuthorize(AppPermissions.Administration)]
        public async Task CreateOrUpdateUser(CreateOrUpdateUserInput input)
        {
            if (input.User.Id > 0)
            {
                await UpdateUserAsync(input);
            }
            else
            {
                await CreateUserAsync(input);
            }
        }


        [AbpAuthorize(AppPermissions.Administration)]
        protected virtual async Task CreateUserAsync(CreateOrUpdateUserInput input)
        {
            // if (AbpSession.TenantId.HasValue)
            // {
            //     await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            // }

            var user = ObjectMapper.Map<User>(input.User); //Passwords is not mapped (see mapping configuration)

            user.EmailAddress = user.UserName + "@molitao.top";
            user.Surname = user.Name[..1];
            user.PhoneNumber = user.UserName;
            user.IsEmailConfirmed = false;
            user.IsPhoneNumberConfirmed = true;
            user.TenantId = AbpSession.TenantId;

            //Set password
            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                // foreach (var validator in _passwordValidators)
                // {
                //     CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                // }

                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }
            else
            {
                // user.Password = _passwordHasher.HashPassword(user, User.DefaultPassword);
                user.Password = _passwordHasher.HashPassword(user, user.PhoneNumber[5..]); //手机号后六位
            }

            // user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.AssignedRoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            }

            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

            //Notifications
            // await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            // await _appNotifier.WelcomeToTheApplicationAsync(user);

            //Organization Units
            // await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());
        }

        [AbpAuthorize(AppPermissions.Administration)]
        protected virtual async Task UpdateUserAsync(CreateOrUpdateUserInput input)
        {
            Debug.Assert(input.User.Id != null, "input.User.Id should be set.");

            var user = await _userManager.FindByIdAsync(input.User.Id.ToString());

            if (user == null)
                throw new UserFriendlyException($"user {input.User.Id} is null");

            //Update user properties
            ObjectMapper.Map(input.User, user); //Passwords is not mapped (see mapping configuration)

            if (!input.User.Password.IsNullOrEmpty())
            {
                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
                CheckErrors(await _userManager.ChangePasswordAsync(user, input.User.Password));
            }

            CheckErrors(await _userManager.UpdateAsync(user));

            //Update roles
            CheckErrors(await _userManager.SetRolesAsync(user, input.AssignedRoleNames));

            //update organization units
            //await _userManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());
        }


        /// <summary>
        /// 个人修改资料
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize]
        public override async Task<UserDto> UpdateAsync(UserEditDto input)
        {
            if (input.Id <= 0)
                throw new UserFriendlyException(1, "id is null");
            var user = await _userManager.GetUserByIdAsync(input.Id);
            if (_abpSession.UserId != user.Id)
                throw new UserFriendlyException(1, "无权修改其他用户信息!");

            if (await Repository.GetAll().AsNoTracking().AnyAsync(x => x.Name == input.Name && x.Id != input.Id))
                throw new UserFriendlyException(1, "此昵称已存在!");

            if (await Repository.GetAll().AsNoTracking()
                    .AnyAsync(x => x.UserName == input.UserName && x.Id != input.Id))
                throw new UserFriendlyException(1, "登录用户名已存在!");

            if (!string.IsNullOrEmpty(input.HeadImgUrl) &&
                input.HeadImgUrl != user.HeadImgUrl)
            {
                ValidateHeadImgUrl(input.HeadImgUrl);
            }

            // 🔐 头像安全检查
            if (!string.IsNullOrEmpty(input.HeadImgUrl) && input.HeadImgUrl != user.HeadImgUrl)
            {
                try
                {
                    _logger.LogInformation("开始检查头像安全性: {HeadImgUrl}", input.HeadImgUrl);

                    // 1. 从CDN下载图片
                    var imageBytes = await DownloadImageAsync(input.HeadImgUrl);
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        _logger.LogWarning("无法下载头像图片，跳过审核");
                    }
                    else
                    {
                        // 2. 验证文件大小 (微信限制: ≤1MB)
                        if (imageBytes.Length > 1024 * 1024)
                        {
                            _logger.LogWarning("图片大小超过1MB，跳过审核");
                        }
                        else
                        {
                            // 3. 获取 access token (带缓存)
                            var accessToken = await _weixinApi.GetAccessTokenAsync(_wechatSettings.Value.AppId, _wechatSettings.Value.AppSecret);

                            // 4. imgSecCheck 图片审核
                            var checkResult = await _weixinApi.ImgSecCheck(accessToken, imageBytes);
                            _logger.LogInformation("头像审核结果: errcode={Errcode}, errmsg={Errmsg}",
                                checkResult.errcode, checkResult.errmsg);

                            if (checkResult.errcode == 87014)
                            {
                                // 违规内容仍需阻止
                                _logger.LogWarning("头像包含违规内容: UserId={UserId}", user.Id);
                                throw new UserFriendlyException(87014, "你所发布的内容含有违规信息，请修改后再试。");
                            }

                            // 其他错误只记录日志，不阻止保存
                            if (checkResult.errcode != 0)
                            {
                                _logger.LogWarning("头像审核失败: errcode={Errcode}, errmsg={Errmsg}，允许保存",
                                    checkResult.errcode, checkResult.errmsg);
                            }
                            else
                            {
                                _logger.LogInformation("头像审核通过: UserId={UserId}", user.Id);
                            }
                        }
                    }
                }
                catch (UserFriendlyException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // 审核异常不影响保存，记录日志继续
                    _logger.LogError(ex, "头像审核异常，允许保存");
                }
            }

            // 保存旧头像URL用于历史记录
            string oldAvatarUrl = user.HeadImgUrl;
            bool avatarChanged = !string.IsNullOrEmpty(input.HeadImgUrl) && input.HeadImgUrl != oldAvatarUrl;

            user.HeadImgUrl = input.HeadImgUrl;
            user.Name = input.Name;
            // user.EmailAddress = input.EmailAddress;
            user.PhoneNumber = input.PhoneNumber;
            user.Qq = input.Qq;
            user.Wx = input.Wx;

            user.UserName = input.UserName;
            if (!input.Password.IsNullOrEmptyOrWhiteSpace())
            {
                user.Password = _passwordHasher.HashPassword(user, input.Password);
            }

            CheckErrors(await _userManager.UpdateAsync(user));

            _chatUserCache.ClearUserCache(user.Id);

            return ObjectMapper.Map<UserDto>(user);
        }

        /// <summary>
        /// 从URL下载图片
        /// </summary>
        private async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("下载图片失败: {Url}, StatusCode={StatusCode}",
                        imageUrl, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载图片异常: {Url}", imageUrl);
                return null;
            }
        }

        private static readonly string[] BlockedHeadImgUrlPrefixes =
        {
            "wxfile://",
            "http://tmp_",
            "file://"
        };

        private static readonly string[] AllowedHeadImgUrlPrefixes =
        {
            "https://cdn.molitao.top",
            "http://image.molitao.top",
            "https://image.molitao.top",
            "https://thirdwx.qlogo.cn",
            "https://wx.qlogo.cn"
        };

        private void ValidateHeadImgUrl(string headImgUrl)
        {
            if (string.IsNullOrEmpty(headImgUrl))
                return;

            foreach (var prefix in BlockedHeadImgUrlPrefixes)
            {
                if (headImgUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UserFriendlyException($"头像地址格式错误: {headImgUrl}");
                }
            }

            foreach (var prefix in AllowedHeadImgUrlPrefixes)
            {
                if (headImgUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            throw new UserFriendlyException($"头像地址不正确: {headImgUrl}，请使用CDN地址或微信头像地址");
        }

        /// <summary>
        /// 获取微信配置
        /// </summary>
        private (string appId, string appSecret) GetWeixinConfig()
        {
            return (_wechatSettings.Value.AppId, _wechatSettings.Value.AppSecret);
        }

        [AbpAuthorize(AppPermissions.Administration)]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        [AbpAuthorize(AppPermissions.Administration)]
        public async Task<ListResultDto<UserDto>> GetUsersInRole(EntityDto<long> input)
        {
            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            if (role == null)
                throw new UserFriendlyException($"Role {input.Id} 不存在!");
            var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);
            return new ListResultDto<UserDto>(ObjectMapper.Map<List<UserDto>>(users));
        }

        [AbpAuthorize(AppPermissions.Administration)]
        public async Task ClearUserCacheAsync(long userId)
        {
            _chatUserCache.ClearUserCache(userId);
            _logger.LogInformation("管理员清除了用户缓存: UserId={UserId}", userId);
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserEditDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            var roleIds = user.Roles != null ? user.Roles.Select(x => x.RoleId).ToArray() : new int[] { };

            var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

            var userDto = base.MapToEntityDto(user);
            userDto.RoleNames = roles.ToArray();

            return userDto;
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, AppResultRequestDto input)
        {
            return query.OrderBy(input.Sorting);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("请先登录");
            }

            long userId = _abpSession.UserId.Value;

            var user = await _userManager.GetUserByIdAsync(userId);

            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword,
                GetTenancyNameOrNull(), shouldLockout: false);

            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("原密码不正确");
            }

            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);

            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }


        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("修改密码前请先登录.");
            }

            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            var loginAsync =
                await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);

            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException(
                    "原密码不正确,请重试!");
            }

            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("只有管理员可以重设密码.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return true;
        }

        /// <summary>
        /// 跳过完善个人信息引导
        /// </summary>
        public async Task SkipProfileCompletion()
        {
            var userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);
            user.SkipProfileCompletion = true;
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 完善个人信息（绑定手机号、设置用户名和密码）
        /// </summary>
        public async Task CompleteProfile(CompleteProfileInput input)
        {
            var userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);

            // 检查用户名是否已被使用
            var existingUserName = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == input.UserName && x.Id != userId);
            if (existingUserName != null)
            {
                throw new UserFriendlyException("该用户名已被使用，请选择其他用户名");
            }

            // 检查手机号是否已被其他用户使用
            var existingPhone = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == input.PhoneNumber && x.Id != userId);
            if (existingPhone != null)
            {
                throw new UserFriendlyException("该手机号已被其他账号绑定，请使用其他手机号");
            }

            // 更新用户信息
            user.PhoneNumber = input.PhoneNumber;
            user.UserName = input.UserName;
            user.Password = _passwordHasher.HashPassword(user, input.Password);

            await CurrentUnitOfWork.SaveChangesAsync();
        }


        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        protected override IQueryable<User> CreateFilteredQuery(AppResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                    .WhereIf(!input.Keyword.IsNullOrEmptyOrWhiteSpace(),
                        x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
                    .WhereIf(input.Status is 1, x => x.IsActive == true)
                    .WhereIf(input.Status is 0, x => x.IsActive == false)
                    .WhereIf(input.Pid.HasValue, x => x.Roles.Any(y => y.RoleId == input.Pid))
                ;
        }

        public override async Task<PagedResultDto<UserDto>> GetAllAsync(AppResultRequestDto input)
        {
            var result = await base.GetAllAsync(input);
            
            if (result.Items.Any())
            {
                var userIds = result.Items.Select(x => x.Id).ToList();
                var groupLevels = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                    .Where(x => userIds.Contains(x.UserId))
                    .ToListAsync();
                
                var groupLevelDict = groupLevels.ToDictionary(x => x.UserId);
                
                foreach (var item in result.Items)
                {
                    if (groupLevelDict.TryGetValue(item.Id, out var groupLevel))
                    {
                        item.CumulativeAmount = groupLevel.CumulativeAmount;
                    }
                }
            }
            
            return result;
        }
    }
}