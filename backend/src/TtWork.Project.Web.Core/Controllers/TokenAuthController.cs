using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Abp;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Json;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.UI;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Nest;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using TtWork.Abp.AppManagement.Events;
using TtWork.Abp.Authorization;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.MultiTenancy;
using TtWork.Abp.Definitions;
using TtWork.Abp.DomianServices.Weixin;
using TtWork.HttpClient.Weixin;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.Core.Authorization;
using TtWork.Project.Applications.Core.Authorization.Accounts.Dto;
using TtWork.Project.Authentication.JwtBearer;
using TtWork.Project.Controllers;
using TtWork.Project.Definitions;
using TtWork.Project.Models.TokenAuth;
using TtWork.Project.Web.Authentication.External;
using TtWork.Project.Web.Authentication.JwtBearer;
using TtWork.Project.Web.Core.Identity;
using TtWork.Project.Web.Core.Models.TokenAuth;
using TtWork.Project.Web.Models.TokenAuth;
using Consts = TtWork.Abp.Consts;

namespace TtWork.Project.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController(
        IMediator mediator,
        IRedisClient redisClient,
        LogInManager logInManager,
        ITenantCache tenantCache,
        WeixinManger weixinManger,
        IWeixinApi weixinApi,
        AbpLoginResultTypeHelper abpLoginResultTypeHelper,
        TokenAuthConfiguration tokenAuthConfiguration,
        IOptions<AsyncJwtBearerOptions> _jwtOptions,
        ExternalAuthManager externalAuthManager,
        ICacheManager cacheManager,
        UserManager userManager,
        IUnitOfWorkManager unitOfWorkManager,
        UserRegistrationManager userRegistrationManager,
        IOptions<IdentityOptions> identityOptions,
        IRepository<User, long> userRepository,
        IRepository<UserLogin, long> userLoginRepository,
        AbpUserClaimsPrincipalFactory<User, Role> claimsPrincipalFactory,
         IPasswordHasher<User> passwordHasher
    )
        : AbpControllerBase
    {
        private readonly IdentityOptions _identityOptions = identityOptions.Value;

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress,
            string password, string tenancyName)
        {
            var loginResult = await logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);
         //   var password1 = passwordHasher.HashPassword(loginResult.User, "123456");
            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result,
                        usernameOrEmailAddress, tenancyName);
            }
        }

        private const string QrTokenKey = "Molitao:QrToken:";


        [HttpGet]
        [DisableAuditing]
        public async Task<string> QrToken(string key)
        {
            var data = await redisClient.Database.StringGetAsync(QrTokenKey + key);
            return data.HasValue ? data : "";
        }

        /// <summary>
        /// 获取公众号事件二维码
        /// </summary>
        /// <param name="state"></param>
        [HttpGet]
        public async Task<string> PubQrLogin(string state)
        {
            var app = await mediator.Send(new QueryApp("pub"));
            var (appid, appSec) = (app.GetValue("appid"), app.GetValue("appsec"));
            var token = await weixinManger.GetAccessTokenAsync(appid, appSec);
            try
            {
                var str = await weixinApi.GetQrCode(token, state);
                return str;
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }


        /// <summary>
        /// 公众号网页登录
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> QrLogin(string code = "", string state = "", int tenantId = 1)
        {
            using (AbpSession.Use(tenantId, null))
            {
                UnitOfWorkManager.Current.SetTenantId(tenantId);
                var cache = await redisClient.Database.StringGetAsync(QrTokenKey + state);
                var app = await mediator.Send(new QueryApp("pub"));
                var (appid, appSec) = (app.GetValue("appid"), app.GetValue("appsec"));
                if (!cache.HasValue)
                {
                    if (string.IsNullOrEmpty(code))
                    {
                        var redirectUri = Request.GetEncodedUrl();
                        return Redirect(
                            $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appid}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code&scope=snsapi_userinfo&state={state}#wechat_redirect");
                    }

                    try
                    {
                        code = Request.Query["code"];
                        var authUserInfo = await externalAuthManager.GetUserInfo(Consts.LoginProvider.WeChatPub,
                            code, appid, appSec);
                        ExternalAuthenticateModel model = new()
                            { AuthProvider = authUserInfo.Provider, ProviderKey = authUserInfo.ProviderKey };
                        var (externalUser, loginResult) = await ExternalLogin(model, authUserInfo);
                        var jwtResult = await ExternalAuthenticateResultModel(loginResult, externalUser, model);
                        await redisClient.Database.StringSetAsync(QrTokenKey + state, jwtResult.AccessToken,
                            TimeSpan.FromHours(1));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        return Content(e.Message);
                    }
                }

                if (!string.IsNullOrEmpty(code))
                {
                    return RedirectToAction("QrLogin", "TokenAuth", new { state });
                }

                return Ok("ok");
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var pwd = model.Password;
            GenerateHashedPassword(pwd);
            //获取用户信息
            var loginResult =
                await GetLoginResultAsync(model.UserNameOrEmailAddress, model.Password, GetTenancyNameOrNull());

            var returnUrl = model.ReturnUrl;

            if (model.SingleSignIn.HasValue && model.SingleSignIn.Value &&
                loginResult.Result == AbpLoginResultType.Success)
            {
                // loginResult.User.SetSignInToken();
                // returnUrl = AddSingleSignInParametersToReturnUrl(model.ReturnUrl, loginResult.User.SignInToken, loginResult.User.Id, loginResult.User.TenantId);
            }

            await userManager.InitializeOptionsAsync(loginResult.Tenant?.Id);

            // string twoFactorRememberClientToken = null;

            var refreshToken = CreateRefreshToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    tokenType: TokenType.RefreshToken
                )
            );

            var accessToken = CreateAccessToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    refreshTokenKey: refreshToken.key
                )
            );


            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                ExpireInSeconds = (int)tokenAuthConfiguration.AccessTokenExpiration.TotalSeconds,
                RefreshToken = refreshToken.token,
                RefreshTokenExpireInSeconds = (int)tokenAuthConfiguration.RefreshTokenExpiration.TotalSeconds,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                // TwoFactorRememberClientToken = twoFactorRememberClientToken,
                UserId = loginResult.User.Id,
                // ReturnUrl = returnUrl
            };
        }


        [HttpPost]
        public async Task<RefreshTokenResult> RefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var (isRefreshTokenValid, principal) = await IsRefreshTokenValid(refreshToken);
            if (!isRefreshTokenValid)
            {
                throw new ValidationException("Refresh token is not valid!");
            }

            try
            {
                var user = await userManager.GetUserAsync(
                    UserIdentifier.Parse(principal.Claims.First(x => x.Type == AppConsts.UserIdentifier).Value)
                );

                if (user == null)
                {
                    throw new UserFriendlyException("Unknown user or user identifier");
                }

                principal = await claimsPrincipalFactory.CreateAsync(user);

                var accessToken = CreateAccessToken(
                    await CreateJwtClaims(principal.Identity as ClaimsIdentity, user)
                );

                return await Task.FromResult(new RefreshTokenResult(
                    accessToken,
                    GetEncryptedAccessToken(accessToken),
                    (int)tokenAuthConfiguration.AccessTokenExpiration.TotalSeconds)
                );
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ValidationException("Refresh token is not valid!", e);
            }
        }

        private async Task<(bool isValid, ClaimsPrincipal principal)> IsRefreshTokenValid(string refreshToken)
        {
            ClaimsPrincipal principal = null;

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = tokenAuthConfiguration.Audience,
                    ValidIssuer = tokenAuthConfiguration.Issuer,
                    IssuerSigningKey = tokenAuthConfiguration.SecurityKey
                };

                foreach (var validator in _jwtOptions.Value.AsyncSecurityTokenValidators)
                {
                    if (!validator.CanReadToken(refreshToken))
                    {
                        continue;
                    }

                    try
                    {
                        (principal, _) = await validator.ValidateRefreshToken(refreshToken, validationParameters);
                        return (true, principal);
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex.ToString(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString(), ex);
            }

            return (false, principal);
        }


        [HttpGet]
        [AbpAuthorize]
        public async Task LogOut()
        {
            if (AbpSession.UserId != null)
            {
                var tokenValidityKeyInClaims = User.Claims.First(c => c.Type == AppConsts.TokenValidityKey);
                await RemoveTokenAsync(tokenValidityKeyInClaims.Value);

                var refreshTokenValidityKeyInClaims =
                    User.Claims.FirstOrDefault(c => c.Type == AppConsts.RefreshTokenValidityKey);
                if (refreshTokenValidityKeyInClaims != null)
                {
                    await RemoveTokenAsync(refreshTokenValidityKeyInClaims.Value);
                }
            }
        }

        private async Task RemoveTokenAsync(string tokenKey)
        {
            await userManager.RemoveTokenValidityKeyAsync(
                await userManager.GetUserAsync(AbpSession.ToUserIdentifier()), tokenKey
            );

            await cacheManager.GetCache(AppConsts.TokenValidityKey).RemoveAsync(tokenKey);
        }


        /// <summary>
        /// 微信公众号openid授权登录
        /// </summary>
        [NonAction]
        public async Task<ExternalAuthenticateResultModel> WeixinPubAuthenticate(string openid)
        {
            var app = await mediator.Send(new QueryApp(ProjectApp.pub));

            var authUserInfo = await externalAuthManager.GetUserInfo(Consts.LoginProvider.WeChatPubOpenid,
                openid, app.GetValue("appid"), app.GetValue("appsec"));

            ExternalAuthenticateModel model = new()
                { AuthProvider = authUserInfo.Provider, ProviderKey = authUserInfo.ProviderKey };

            var (externalUser, loginResult) = await ExternalLogin(model, authUserInfo);

            return await ExternalAuthenticateResultModel(loginResult, externalUser, model);
        }


        /// <summary>
        /// 小程序微信授权登录
        /// </summary>
        /// <param name="loginModel"></param>
        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> WeixinMiniAuthenticate(
            [FromBody] WeChatMiniProgramAuthenticateModel loginModel
        )
        {
            try
            {
                Logger.Info($"[WeixinMiniAuthenticate] Request: {loginModel.ToJsonString()}");
                
                var app = await mediator.Send(new QueryApp());
                var authUserInfo =
                    await externalAuthManager.GetUserInfo(Consts.LoginProvider.WeChatMiniOpenid,
                        loginModel.ToJsonString(), app.GetValue("appid"), app.GetValue("appsec"));
                Logger.Info($"[WeixinMiniAuthenticate] AuthUserInfo: Provider={authUserInfo.Provider}, ProviderKey={authUserInfo.ProviderKey}, Name={authUserInfo.Name}");
                
                ExternalAuthenticateModel model = new()
                    { AuthProvider = authUserInfo.Provider, ProviderKey = authUserInfo.ProviderKey };

                var (externalUser, loginResult) = await ExternalLogin(model, authUserInfo);
                Logger.Info($"[WeixinMiniAuthenticate] LoginResult: Result={loginResult.Result}, UserId={loginResult.User?.Id}");

                return await ExternalAuthenticateResultModel(loginResult, externalUser, model);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                if (e is UserFriendlyException)
                    throw new UserFriendlyException(e.Message);
                throw new UserFriendlyException("登录失败,请重新登录");
            }
        }


        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> WeixinMiniPhoneAuthenticate(
            [FromBody] WeChatMiniProgramAuthenticateModel loginModel)
        {
            try
            {
                var app = await mediator.Send(new QueryApp());

                var authUserInfo =
                    await externalAuthManager.GetUserInfo(Consts.LoginProvider.WeChatMiniPhone,
                        loginModel.ToJsonString(), app.GetValue("appid"), app.GetValue("appsec"));

                ExternalAuthenticateModel model = new()
                    { AuthProvider = authUserInfo.Provider, ProviderKey = authUserInfo.ProviderKey };

                var (externalUser, loginResult) = await ExternalLogin(model, authUserInfo);

                return await ExternalAuthenticateResultModel(loginResult, externalUser, model);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                if (e is UserFriendlyException) throw new UserFriendlyException(e.Message);
                throw new UserFriendlyException("登录失败,请重新登录");
            }
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> AuthenticateWeixinApp(
            [FromBody] WeixinAppAuthenticateModel loginModel)
        {
            try
            {
                var app = await mediator.Send(new QueryApp("app"));
                
                string openid, unionid, accessToken;

                if (!string.IsNullOrEmpty(loginModel.AccessToken) && !string.IsNullOrEmpty(loginModel.Openid))
                {
                    openid = loginModel.Openid;
                    unionid = loginModel.Unionid;
                    accessToken = loginModel.AccessToken;
                    Logger.Info($"[AuthenticateWeixinApp] New way - openid={openid}, unionid={unionid}, accessToken={accessToken.Substring(0, 20)}...");
                }
                else
                {
                    Logger.Info($"[AuthenticateWeixinApp] Old way - code={loginModel.AuthCode}");
                    var weixinResult = await weixinApi.GetOpenPlatformAccessTokenAsync(
                        app.GetValue("appid"),
                        app.GetValue("appsec"),
                        loginModel.AuthCode
                    );
                    openid = weixinResult.openid;
                    unionid = weixinResult.unionid;
                    accessToken = weixinResult.access_token;
                }

                var authUserInfo = new ExternalAuthUserInfo
                {
                    ProviderKey = openid,
                    ProviderName = Consts.LoginProvider.WeChatApp,
                    UserName = openid,
                    Name = $"玩家{new Random().Next(10000, 99999)}",
                    Surname = $"玩家{new Random().Next(10000, 99999)}",
                    EmailAddress = $"{openid}@molitao.top",
                    HeadImgUrl = "https://cdn.wujiangapp.com.cn/PicGo/202411061606451.png",
                    UserLogins = new Dictionary<string, string>
                    {
                        [Consts.LoginProvider.WeChatApp] = openid
                    }
                };

                if (!string.IsNullOrEmpty(unionid))
                {
                    authUserInfo.UnionId = unionid;
                    authUserInfo.UserLogins[Consts.LoginProvider.WeChatUnionId] = unionid;
                }

                AbpLoginResult<Tenant, User> loginResult;

                if (!string.IsNullOrEmpty(unionid))
                {
                    loginResult = await logInManager.LoginAsync(
                        new UserLoginInfo(Consts.LoginProvider.WeChatUnionId, unionid, Consts.LoginProvider.WeChatUnionId),
                        GetTenancyNameOrNull());

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        Logger.Info($"[AuthenticateWeixinApp] User found by unionid, linking openid");
                        var existingLogins = await userLoginRepository.GetAllListAsync(x => x.UserId == loginResult.User.Id);
                        if (!existingLogins.Any(x => x.LoginProvider == Consts.LoginProvider.WeChatApp && x.ProviderKey == openid))
                        {
                            await TryAddUserLogin(new UserLogin(loginResult.User.TenantId, loginResult.User.Id, Consts.LoginProvider.WeChatApp, openid));
                        }
                        
                        await UpdateUserInfoFromWeixin(loginResult.User, accessToken, openid);
                        
                        return await ExternalAuthenticateResultModel(loginResult, authUserInfo, new ExternalAuthenticateModel
                        {
                            AuthProvider = Consts.LoginProvider.WeChatApp,
                            ProviderKey = openid
                        });
                    }
                    
                    Logger.Info($"[AuthenticateWeixinApp] User not found by unionid, trying openid (may be old mini program user without unionid)");
                }

                loginResult = await logInManager.LoginAsync(
                    new UserLoginInfo(Consts.LoginProvider.WeChatApp, openid, Consts.LoginProvider.WeChatApp),
                    GetTenancyNameOrNull());

                if (loginResult.Result == AbpLoginResultType.Success)
                {
                    Logger.Info($"[AuthenticateWeixinApp] User found by openid, linking unionid if available");
                    if (!string.IsNullOrEmpty(unionid))
                    {
                        var existingLogins = await userLoginRepository.GetAllListAsync(x => x.UserId == loginResult.User.Id);
                        if (!existingLogins.Any(x => x.LoginProvider == Consts.LoginProvider.WeChatUnionId && x.ProviderKey == unionid))
                        {
                            await TryAddUserLogin(new UserLogin(loginResult.User.TenantId, loginResult.User.Id, Consts.LoginProvider.WeChatUnionId, unionid));
                            Logger.Info($"[AuthenticateWeixinApp] Unionid linked to existing user");
                        }
                    }
                    
                    await UpdateUserInfoFromWeixin(loginResult.User, accessToken, openid);
                    
                    return await ExternalAuthenticateResultModel(loginResult, authUserInfo, new ExternalAuthenticateModel
                    {
                        AuthProvider = Consts.LoginProvider.WeChatApp,
                        ProviderKey = openid
                    });
                }

                if (!string.IsNullOrEmpty(unionid))
                {
                    Logger.Warn($"[AuthenticateWeixinApp] User not found by unionid or openid. May be old mini program user without unionid. Rejecting to prevent duplicate accounts.");
                    throw new UserFriendlyException("检测到您已使用小程序账号登录，为了保护您的账号资产，请先打开小程序完成登录验证，之后即可使用APP直接登录。");
                }

                Logger.Info($"[AuthenticateWeixinApp] New user, creating account");
                var newUser = await RegisterExternalUserAsync(authUserInfo);
                loginResult = await logInManager.LoginAsync(
                    new UserLoginInfo(Consts.LoginProvider.WeChatApp, openid, Consts.LoginProvider.WeChatApp),
                    GetTenancyNameOrNull());
                await UpdateUserInfoFromWeixin(loginResult.User, accessToken, openid);
                
                return await ExternalAuthenticateResultModel(loginResult, authUserInfo, new ExternalAuthenticateModel
                {
                    AuthProvider = Consts.LoginProvider.WeChatApp,
                    ProviderKey = openid
                });
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                if (e is UserFriendlyException) throw new UserFriendlyException(e.Message);
                throw new UserFriendlyException("微信登录失败,请重试");
            }
        }

        private async Task UpdateUserInfoFromWeixin(User user, string accessToken, string openid)
        {
            try
            {
                var wxUserInfo = await weixinApi.SnsUserInfo(accessToken, openid);
                if (wxUserInfo != null && !string.IsNullOrEmpty(wxUserInfo.nickname))
                {
                    Logger.Info($"[AuthenticateWeixinApp] Updating user info - nickname={wxUserInfo.nickname}, headimgurl={wxUserInfo.headimgurl}");
                    
                    user.Name = wxUserInfo.nickname;
                    user.Surname = wxUserInfo.nickname;
                    user.HeadImgUrl = wxUserInfo.headimgurl;
                    
                    await userManager.UpdateAsync(user);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    
                    Logger.Info($"[AuthenticateWeixinApp] User info updated successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[AuthenticateWeixinApp] Failed to update user info: {ex.Message}");
            }
        }

        [UnitOfWork]
        protected virtual async Task<(ExternalAuthUserInfo, AbpLoginResult<Tenant, User>)> ExternalLogin(
            ExternalAuthenticateModel model, ExternalAuthUserInfo externalUser)
        {
            var loginResult = await logInManager.LoginAsync(
                new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            if (loginResult.Result == AbpLoginResultType.Success)
            {
                var notDefualtUserLogin = externalUser.UserLogins.Where(x => x.Key != model.AuthProvider).ToList();
                if (notDefualtUserLogin.Any())
                {
                    foreach (var u in notDefualtUserLogin)
                    {
                        await TryAddUserLogin(new UserLogin(loginResult.User.TenantId, loginResult.User.Id, u.Key,
                            u.Value));
                    }
                }
            }

            return (externalUser, loginResult);
        }

        private async Task TryAddUserLogin(UserLogin userLogin)
        {
            try
            {
                using var uow = unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew);
                // using var uow = unitOfWorkManager.Begin();
                if (!await userLoginRepository.GetAll().AsNoTracking()
                        .AnyAsync(x => x.ProviderKey == userLogin.ProviderKey && x.TenantId == userLogin.TenantId))
                {
                    await userLoginRepository.InsertAsync(userLogin);
                }

                await uow.CompleteAsync();
            }
            catch (Exception e)
            {
                if (e.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
                {
                    //已插入过不处理
                    return;
                }

                Log.Error(e.Message);
            }
        }


        private async Task<ExternalAuthenticateResultModel> WeixinExtAuthResult(
            AbpLoginResult<Tenant, User> loginResult, ExternalAuthUserInfo externalUser)
        {
            var refreshToken = CreateRefreshToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    tokenType: TokenType.RefreshToken));

            var accessToken = CreateAccessToken(
                await CreateJwtClaims(
                    loginResult.Identity,
                    loginResult.User,
                    refreshTokenKey: refreshToken.key
                )
            );


            // var roles = await userManager.GetRolesAsync(loginResult.User);
            //
            // externalUser.PhoneNumber = loginResult.User.PhoneNumber;
            // externalUser.Id = loginResult.User.Id;

            return new ExternalAuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncrpyedAccessToken(accessToken),
                ExpireInSeconds = (int)tokenAuthConfiguration.Expiration.TotalSeconds,
                RefreshToken = refreshToken.key,
                Extension = externalUser.Extension
                // User = externalUser,
                // RoleNames = roles.ToArray()
            };
        }

        private async Task<ExternalAuthenticateResultModel> ExternalAuthenticateResultModel(
            AbpLoginResult<Tenant, User> loginResult,
            ExternalAuthUserInfo externalUser,
            ExternalAuthenticateModel model)
        {
            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                {
                    loginResult.AddWeixinClaim(externalUser);
                    return await WeixinExtAuthResult(loginResult, externalUser);
                }
                case AbpLoginResultType.UnknownExternalLogin:
                {
                    var newUser = await RegisterExternalUserAsync(externalUser);
                    if (!newUser.IsActive)
                    {
                        return new ExternalAuthenticateResultModel { WaitingForActivation = true };
                    }

                    // Try to login again with newly registered user!
                    loginResult = await logInManager.LoginAsync(
                        new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider),
                        GetTenancyNameOrNull());


                    if (loginResult.Result != AbpLoginResultType.Success)
                    {
                        throw abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }

                    loginResult.AddWeixinClaim(externalUser);
                    return await WeixinExtAuthResult(loginResult, externalUser);
                }
                default:
                {
                    throw abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                        loginResult.Result,
                        model.ProviderKey,
                        GetTenancyNameOrNull()
                    );
                }
            }
        }


        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var dbUser = await userRepository.GetAll()
                .Include(x => x.Logins).FirstOrDefaultAsync(x =>
                    x.UserName == externalUser.ProviderKey);
            //如果数据库中不存在相同名称的用户,自动重新注册
            if (dbUser == null)
            {
                dbUser = await userRegistrationManager.RegisterAsync(
                    externalUser.Name,
                    externalUser.Surname,
                    externalUser.EmailAddress,
                    externalUser.UserName,
                    TtWork.Abp.Authorization.Users.User.CreateRandomPassword(),
                    externalUser.PhoneNumber,
                    false,
                    externalUser.IsPhoneNumberConfirmed,
                    externalUser.HeadImgUrl,
                    externalUser.FromClient
                );
                // await CurrentUnitOfWork.SaveChangesAsync();
            }

            dbUser.Logins = externalUser.UserLogins.Select(x => new UserLogin
            {
                LoginProvider = x.Key,
                ProviderKey = x.Value,
                TenantId = dbUser.TenantId
            }).ToList();

            await CurrentUnitOfWork.SaveChangesAsync();
            return dbUser;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }


        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            return CreateToken(claims, expiration ?? tokenAuthConfiguration.AccessTokenExpiration);
        }

        private (string token, string key) CreateRefreshToken(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();
            return (CreateToken(claimsList, AppConsts.RefreshTokenExpiration),
                claimsList.First(c => c.Type == AppConsts.TokenValidityKey).Value);
        }

        private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: tokenAuthConfiguration.Issuer,
                audience: tokenAuthConfiguration.Audience,
                claims: claims,
                notBefore: now,
                signingCredentials: tokenAuthConfiguration.SigningCredentials,
                expires: now.Add(expiration ?? tokenAuthConfiguration.Expiration)
            );
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private async Task<IEnumerable<Claim>> CreateJwtClaims(ClaimsIdentity identity, User user,
            TimeSpan? expiration = null, TokenType tokenType = TokenType.AccessToken, string refreshTokenKey = null)
        {
            var tokenValidityKey = Guid.NewGuid().ToString();
            var claims = identity.Claims.ToList();

            var nameIdClaim = claims.First(c => c.Type == _identityOptions.ClaimsIdentity.UserIdClaimType);
            if (_identityOptions.ClaimsIdentity.UserIdClaimType != JwtRegisteredClaimNames.Sub)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value));
            }

            var userIdentifier = new UserIdentifier(AbpSession.TenantId, Convert.ToInt64(nameIdClaim.Value));
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(AppConsts.TokenValidityKey, tokenValidityKey),
                new Claim(AppConsts.UserIdentifier, userIdentifier.ToUserIdentifierString()),
                new Claim(AppConsts.TokenType, tokenType.To<int>().ToString())
            });

            if (!string.IsNullOrEmpty(refreshTokenKey))
            {
                claims.Add(new Claim(AppConsts.RefreshTokenValidityKey, refreshTokenKey));
            }

            if (!expiration.HasValue)
            {
                expiration = tokenType == TokenType.AccessToken
                    ? tokenAuthConfiguration.AccessTokenExpiration
                    : tokenAuthConfiguration.RefreshTokenExpiration;
            }

            cacheManager
                .GetCache(AppConsts.TokenValidityKey)
                .Set(tokenValidityKey, "", expiration);

            await userManager.AddTokenValidityKeyAsync(
                user,
                tokenValidityKey,
                DateTime.UtcNow.Add(expiration.Value)
            );

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }

        private string GetEncrpyedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }

        [HttpGet]
        public string GenerateHashedPassword(string plainPassword = "123456")
        {
            var user = new User(); // 创建一个空的 User 实例
            var hashedPassword = passwordHasher.HashPassword(user, plainPassword);
            Log.Information($"Hashed password: {hashedPassword}");
            return hashedPassword;
        }

        /// <summary>
        /// 为指定用户生成token的请求模型
        /// </summary>
        public class GenerateTokenForUserInput
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            public long UserId { get; set; }
        }

        /// <summary>
        /// 为指定用户生成token的返回结果
        /// </summary>
        public class GenerateTokenForUserResult
        {
            /// <summary>
            /// 访问令牌
            /// </summary>
            public string AccessToken { get; set; }

            /// <summary>
            /// 加密的访问令牌
            /// </summary>
            public string EncryptedAccessToken { get; set; }

            /// <summary>
            /// 过期时间（秒）
            /// </summary>
            public int ExpireInSeconds { get; set; }

            /// <summary>
            /// 刷新令牌
            /// </summary>
            public string RefreshToken { get; set; }

            /// <summary>
            /// 刷新令牌过期时间（秒）
            /// </summary>
            public int RefreshTokenExpireInSeconds { get; set; }

            /// <summary>
            /// 用户ID
            /// </summary>
            public long UserId { get; set; }

            /// <summary>
            /// 用户名
            /// </summary>
            public string UserName { get; set; }
        }

        /// <summary>
        /// 为指定用户生成token（管理员权限，仅限本地访问）
        /// </summary>
        /// <param name="input">生成token请求</param>
        /// <returns>token信息</returns>
        [HttpPost]
        public async Task<GenerateTokenForUserResult> GenerateTokenForUser([FromBody] GenerateTokenForUserInput input)
        {
            try
            {
                // 获取客户端IP地址
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                // 检查是否为本地IP
                bool isLocalIp = false;
                if (!string.IsNullOrEmpty(clientIp))
                {
                    // 检查IPv4本地地址
                    if (clientIp == "127.0.0.1" || clientIp.StartsWith("192.168.") ||
                        clientIp.StartsWith("10.") || clientIp.StartsWith("172."))
                    {
                        isLocalIp = true;
                    }
                    // 检查IPv6本地地址
                    else if (clientIp == "::1" || clientIp.StartsWith("::ffff:127.0.0.1") ||
                             clientIp.StartsWith("fe80::"))
                    {
                        isLocalIp = true;
                    }
                }

                // 限制只能从本地访问
                if (!isLocalIp)
                {
                    Logger.Warn($"非法IP尝试访问GenerateTokenForUser接口: {clientIp}");
                    throw new UserFriendlyException("此接口仅允许本地访问");
                }

                Logger.Info($"本地IP {clientIp} 正在为用户ID {input.UserId} 生成token");

                // 验证输入参数
                if (input.UserId <= 0)
                {
                    throw new UserFriendlyException("用户ID无效");
                }

                var userIdentifier = new UserIdentifier(1, input.UserId);
                // 获取用户信息
                var user = await userManager.GetUserAsync(userIdentifier);
                if (user == null)
                {
                    throw new UserFriendlyException("用户不存在");
                }

                // 检查用户是否激活
                if (!user.IsActive)
                {
                    throw new UserFriendlyException("用户已被禁用");
                }

                // 创建用户身份
                var principal = await claimsPrincipalFactory.CreateAsync(user);
                var identity = principal.Identity as ClaimsIdentity;

                // 生成刷新令牌
                var refreshToken = CreateRefreshToken(
                    await CreateJwtClaims(
                        identity,
                        user,
                        tokenType: TokenType.RefreshToken
                    )
                );

                // 生成访问令牌
                var accessToken = CreateAccessToken(
                    await CreateJwtClaims(
                        identity,
                        user,
                        refreshTokenKey: refreshToken.key
                    )
                );

                Logger.Info($"成功为用户 {user.UserName} (ID: {user.Id}) 生成token");

                return new GenerateTokenForUserResult
                {
                    AccessToken = accessToken,
                    EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                    ExpireInSeconds = (int)tokenAuthConfiguration.AccessTokenExpiration.TotalSeconds,
                    RefreshToken = refreshToken.token,
                    RefreshTokenExpireInSeconds = (int)tokenAuthConfiguration.RefreshTokenExpiration.TotalSeconds,
                    UserId = user.Id,
                    UserName = user.UserName
                };
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("为用户生成token失败，用户ID: " + input.UserId, ex);
                throw new UserFriendlyException("生成token失败: " + ex.Message);
            }
        }
    }
}