using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.UI;
using Microsoft.IdentityModel.Tokens;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization;
using TtWork.Abp.Definitions;
using TtWork.Project.Authentication.JwtBearer;

namespace TtWork.Project.Web.Authentication.JwtBearer {
    public class AbpZeroTemplateAsyncJwtSecurityTokenHandler : IAsyncSecurityTokenValidator {
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AbpZeroTemplateAsyncJwtSecurityTokenHandler() {
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        public bool CanReadToken(string securityToken) {
            return _tokenHandler.CanReadToken(securityToken);
        }

        public async Task<(ClaimsPrincipal, SecurityToken)> ValidateToken(string securityToken,
            TokenValidationParameters validationParameters) {
            var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out var validatedToken);

            if (!HasTokenType(principal, TokenType.AccessToken)) {
                throw new SecurityTokenException("invalid token type");
            }

            return await ValidateTokenInternal(principal, validatedToken);
        }

        public async Task<(ClaimsPrincipal, SecurityToken)> ValidateRefreshToken(string securityToken,
            TokenValidationParameters validationParameters) {
            var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out var validatedToken);

            if (!HasTokenType(principal, TokenType.RefreshToken)) {
                throw new SecurityTokenException("invalid token type");
            }

            return await ValidateTokenInternal(principal, validatedToken);
        }

        private async Task<(ClaimsPrincipal, SecurityToken)> ValidateTokenInternal(ClaimsPrincipal principal,
            SecurityToken validatedToken) {
            var cacheManager = IocManager.Instance.Resolve<ICacheManager>();
            await ValidateSecurityStampAsync(principal);

            var tokenValidityKeyClaim = principal.Claims.First(c => c.Type == AppConsts.TokenValidityKey);
            if (await TokenValidityKeyExistsInCache(tokenValidityKeyClaim, cacheManager)) {
                return (principal, validatedToken);
            }

            var userIdentifierString = principal.Claims.First(c => c.Type == AppConsts.UserIdentifier);
            var userIdentifier = UserIdentifier.Parse(userIdentifierString.Value);

            if (!await ValidateTokenValidityKey(tokenValidityKeyClaim, userIdentifier)) {
                throw new SecurityTokenException("invalid");
            }

            var tokenAuthConfiguration = IocManager.Instance.Resolve<TokenAuthConfiguration>();

            await cacheManager.GetCache(AppConsts.TokenValidityKey).SetAsync(
                tokenValidityKeyClaim.Value, "",
                absoluteExpireTime: new DateTimeOffset(
                    Clock.Now.AddMinutes(tokenAuthConfiguration.AccessTokenExpiration.TotalMinutes)
                )
            );

            return (principal, validatedToken);
        }

        private async Task<bool> ValidateTokenValidityKey(Claim tokenValidityKeyClaim, UserIdentifier userIdentifier) {
            bool isValid;

            using (var unitOfWorkManager = IocManager.Instance.ResolveAsDisposable<IUnitOfWorkManager>()) {
                using (var uow = unitOfWorkManager.Object.Begin()) {
                    using (unitOfWorkManager.Object.Current.SetTenantId(userIdentifier.TenantId)) {
                        using (var userManager = IocManager.Instance.ResolveAsDisposable<UserManager>()) {
                            var userManagerObject = userManager.Object;
                            var user = await userManagerObject.GetUserAsync(userIdentifier);
                            isValid = await userManagerObject.IsTokenValidityKeyValidAsync(
                                user,
                                tokenValidityKeyClaim.Value
                            );

                            await uow.CompleteAsync();
                        }
                    }
                }
            }

            return isValid;
        }

        private static async Task<bool> TokenValidityKeyExistsInCache(Claim tokenValidityKeyClaim,
            ICacheManager cacheManager) {
            var tokenValidityKeyInCache = await cacheManager
                .GetCache(AppConsts.TokenValidityKey)
                .GetOrDefaultAsync(tokenValidityKeyClaim.Value);

            return tokenValidityKeyInCache != null;
        }

        private static async Task ValidateSecurityStampAsync(ClaimsPrincipal principal) {
            using (var securityStampHandler = IocManager.Instance.ResolveAsDisposable<IJwtSecurityStampHandler>()) {
                if (!await securityStampHandler.Object.Validate(principal)) {
                    throw new SecurityTokenException("invalid");
                }
            }
        }

        private bool HasTokenType(ClaimsPrincipal principal, TokenType tokenType) {
            return principal.Claims.FirstOrDefault(x => x.Type == AppConsts.TokenType)?.Value ==
                   tokenType.To<int>().ToString();
        }

    
    }
}