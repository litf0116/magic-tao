using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TtWork.Abp;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Applications.Auth;
using TtWork.Project.Applications.Auth.Dto;
using TtWork.Project.Authentication.JwtBearer;
using TtWork.Project.Controllers;

namespace TtWork.Project.Web.Controllers;

[Route("api/auth/qrcode")]
public class QrCodeAuthController : AbpControllerBase
{
    private readonly IQrCodeAuthService _qrCodeAuthService;
    private readonly UserManager _userManager;
    private readonly TokenAuthConfiguration _tokenAuthConfiguration;
    private readonly ICacheManager _cacheManager;
    private readonly AbpUserClaimsPrincipalFactory<User, Role> _claimsPrincipalFactory;
    private readonly IdentityOptions _identityOptions;

    public QrCodeAuthController(
        IQrCodeAuthService qrCodeAuthService,
        UserManager userManager,
        TokenAuthConfiguration tokenAuthConfiguration,
        ICacheManager cacheManager,
        AbpUserClaimsPrincipalFactory<User, Role> claimsPrincipalFactory,
        IOptions<IdentityOptions> identityOptions)
    {
        _qrCodeAuthService = qrCodeAuthService;
        _userManager = userManager;
        _tokenAuthConfiguration = tokenAuthConfiguration;
        _cacheManager = cacheManager;
        _claimsPrincipalFactory = claimsPrincipalFactory;
        _identityOptions = identityOptions.Value;
    }

    /// <summary>
    /// 生成二维码（PC端调用，需要认证）
    /// </summary>
    [HttpPost]
    [AbpAuthorize]
    public async Task<QrCodeGenerateOutputDto> Generate()
    {
        var userId = AbpSession.GetUserId();
        return await _qrCodeAuthService.GenerateQrCodeAsync(userId);
    }

    /// <summary>
    /// 扫码获取用户信息（移动端调用，无需认证）
    /// </summary>
    /// <param name="code">二维码code</param>
    [HttpGet("{code}")]
    [AllowAnonymous]
    public async Task<QrCodeUserInfoDto> GetUserInfo(string code)
    {
        return await _qrCodeAuthService.GetUserInfoByCodeAsync(code);
    }

    /// <summary>
    /// 确认登录（移动端调用，无需认证）
    /// </summary>
    /// <param name="input">确认登录请求</param>
    [HttpPost("confirm")]
    [AllowAnonymous]
    public async Task<QrCodeLoginResultDto> Confirm([FromBody] ConfirmLoginInputDto input)
    {
        var result = await _qrCodeAuthService.ConfirmLoginAsync(input.Code);

        // 获取二维码绑定的用户ID生成Token
        var authRequest = await _qrCodeAuthService.GetAuthRequestByCodeAsync(input.Code);
        if (authRequest == null)
        {
            throw new Exception("二维码不存在");
        }

        var tokenResult = await GenerateTokenAsync(authRequest.UserId);
        result.Token = tokenResult.Token;
        result.ExpiresIn = tokenResult.ExpiresIn;

        return result;
    }

    /// <summary>
    /// 轮询状态（PC端调用，无需认证）
    /// </summary>
    /// <param name="code">二维码code</param>
    [HttpGet("{code}/status")]
    public async Task<QrCodeStatusDto> GetStatus(string code)
    {
        return await _qrCodeAuthService.GetStatusAsync(code);
    }

    private async Task<(string Token, int ExpiresIn)> GenerateTokenAsync(long userId)
    {
        var user = await _userManager.GetUserAsync(new UserIdentifier(AbpSession.TenantId, userId));
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        var principal = await _claimsPrincipalFactory.CreateAsync(user);
        var identity = principal.Identity as ClaimsIdentity;

        // 生成刷新令牌
        var refreshToken = CreateRefreshToken(
            await CreateJwtClaims(identity, user, TokenType.RefreshToken)
        );

        // 生成访问令牌
        var accessToken = CreateAccessToken(
            await CreateJwtClaims(identity, user, TokenType.AccessToken, refreshToken.key)
        );

        return (accessToken, (int)_tokenAuthConfiguration.AccessTokenExpiration.TotalSeconds);
    }

    private string CreateAccessToken(IEnumerable<Claim> claims)
    {
        return CreateToken(claims, _tokenAuthConfiguration.AccessTokenExpiration);
    }

    private (string token, string key) CreateRefreshToken(IEnumerable<Claim> claims)
    {
        var claimsList = new List<Claim>(claims);
        return (CreateToken(claimsList, _tokenAuthConfiguration.RefreshTokenExpiration),
            claimsList.First(c => c.Type == AppConsts.TokenValidityKey).Value);
    }

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan expiration)
    {
        var now = DateTime.Now;

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _tokenAuthConfiguration.Issuer,
            audience: _tokenAuthConfiguration.Audience,
            claims: claims,
            notBefore: now,
            signingCredentials: _tokenAuthConfiguration.SigningCredentials,
            expires: now.Add(expiration)
        );
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    private async Task<IEnumerable<Claim>> CreateJwtClaims(
        ClaimsIdentity identity,
        User user,
        TokenType tokenType,
        string refreshTokenKey = null)
    {
        var tokenValidityKey = Guid.NewGuid().ToString();
        var claims = new List<Claim>(identity.Claims);

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

        var expiration = tokenType == TokenType.AccessToken
            ? _tokenAuthConfiguration.AccessTokenExpiration
            : _tokenAuthConfiguration.RefreshTokenExpiration;

        _cacheManager
            .GetCache(AppConsts.TokenValidityKey)
            .Set(tokenValidityKey, "", expiration);

        await _userManager.AddTokenValidityKeyAsync(
            user,
            tokenValidityKey,
            DateTime.Now.Add(expiration)
        );

        return claims;
    }
}
