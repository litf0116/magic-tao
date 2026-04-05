using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TtWork.Abp;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Dapper;
using TtWork.Lib;
using TtWork.Lib.Redis;
using TtWork.Project.Authentication.JwtBearer;
using TTWork.WeiXinMiddleware;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TtWork.Project.Web.Host {
    public class AbpWeiXinProvider(
        IRepository<User, long> userRepository,
        IRepository<UserLogin, long> userLoginRepository,
        ILogger<AbpWeiXinProvider> logger,
        IUnitOfWorkManager unitOfWorkManager,
        ISqlConnectionFactory sqlConnectionFactory,
        ICacheManager cacheManager,
        TokenAuthConfiguration tokenAuthConfiguration,
        IPasswordHasher<User> passwordHasher,
        IRedisClient redisClient)
        : WeiXinProvide(logger) {
        public override async Task OnUnsubscribe(WeiXinContext context) {
            await base.OnUnsubscribe(context);
        }


        private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null) {
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

        private async Task<IEnumerable<Claim>> CreateJwtClaims(long userId, TimeSpan? expiration = null) {
            var tokenValidityKey = Guid.NewGuid().ToString();
            List<Claim> claims = [];
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, $"{userId}"));

            var userIdentifier = new UserIdentifier(1, userId);
            claims.AddRange(new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(AppConsts.TokenValidityKey, tokenValidityKey),
                new Claim(AppConsts.UserIdentifier, userIdentifier.ToUserIdentifierString()),
                new Claim(AppConsts.TokenType, "0")
            });

            if (!expiration.HasValue) {
                expiration = tokenAuthConfiguration.AccessTokenExpiration;
            }

            cacheManager
                .GetCache(AppConsts.TokenValidityKey)
                .Set(tokenValidityKey, "", expiration);

            using var conn = sqlConnectionFactory.GetOpenConnection();

            var count = conn.Execute(
                "insert abpusertokens(TenantId,UserId,LoginProvider,Name,Value,ExpireDate) values (@a,@b,@c,@d,@e,@f)",
                new[] {
                    new {
                        a = 1, b = userId, c = AppConsts.TokenValidityKey, d = tokenValidityKey,
                        e = "", f = DateTime.UtcNow.Add(expiration.Value)
                    }
                }
            );

            return claims;
        }

        //生成TOKEN到redis
        private async Task DoOpenid(string openid, string state) {
            const string QrTokenKey = "Molitao:QrToken:";
            var cache = await redisClient.Database.StringGetAsync(QrTokenKey + state);
            if (cache.HasValue) {
                return;
            }

            using var uow = unitOfWorkManager.Begin();
            var userLogin = await userLoginRepository.FirstOrDefaultAsync(x =>
                x.TenantId == 1 &&
                x.ProviderKey == openid &&
                x.LoginProvider == Consts.LoginProvider.WeChatPub);
            if (userLogin == null) {
                var dbUser = await userRepository.GetAll()
                    .Include(x => x.Logins).FirstOrDefaultAsync(x =>
                        x.UserName == openid);

                if (dbUser != null) {
                    userLogin = new UserLogin() {
                        TenantId = 1,
                        ProviderKey = openid,
                        LoginProvider = Consts.LoginProvider.WeChatPub
                    };

                    dbUser.Logins.Add(userLogin);
                    await unitOfWorkManager.Current.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                else {
                    //注册用户
                    dbUser = new User {
                        Name = $"玩家{new Random().Next(10000, 99999)}",
                        Surname = openid,
                        EmailAddress = openid + "@molitao.top",
                        IsActive = true,
                        UserName = openid,
                        IsEmailConfirmed = false,
                        IsPhoneNumberConfirmed = false,
                        PhoneNumber = "",
                        Roles = new List<UserRole>(),
                        HeadImgUrl = AppConsts.UserDefaultAvatar,
                        TenantId = 1
                    };
                    dbUser.Password = passwordHasher.HashPassword(dbUser, User.CreateRandomPassword());
                    dbUser.SetNormalizedNames();
                    await userRepository.InsertAsync(dbUser);


                    userLogin = new UserLogin() {
                        TenantId = 1,
                        ProviderKey = openid,
                        LoginProvider = Consts.LoginProvider.WeChatPub
                    };
                    dbUser.Logins = [userLogin];
                }

                await unitOfWorkManager.Current.SaveChangesAsync();
                await uow.CompleteAsync();
            }

            var accessToken = CreateToken(await CreateJwtClaims(userLogin.UserId));
            await redisClient.Database.StringSetAsync(QrTokenKey + state, accessToken,
                TimeSpan.FromHours(1));
        }


        public override async Task OnSubscribe(WeiXinContext context) {
            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.Response.ContentType = "text/xml";
            var textResult = @"欢迎关注我们的公众号";

            if (!context.EventKey.IsNullOrEmptyOrWhiteSpace()) {
                await DoOpenid(context.FromUserName, context.EventKey.Replace("qrscene_", ""));

                // await tokenAuthController.OpenidEvent(context.FromUserName, context.EventKey.Replace("qrscene_", ""));
            }

            var resultText = $@"<xml>
<ToUserName><![CDATA[{context.FromUserName}]]></ToUserName>
<FromUserName><![CDATA[{context.ToUserName}]]></FromUserName>
<CreateTime>{context.CreateTime}</CreateTime>
<MsgType><![CDATA[text]]></MsgType>
<Content><![CDATA[{textResult}]]></Content>
</xml>";
            await context.HttpContext.Response.WriteAsync(resultText);
        }


        public override async Task OnScan(WeiXinContext context) {
            //TODO:根据eventkey判断返回
            var textResult = "扫码登录";
            await DoOpenid(context.FromUserName, context.EventKey);
            // await tokenAuthController.OpenidEvent(context.FromUserName, context.EventKey);
            var resultText = $@"<xml>
<ToUserName><![CDATA[{context.FromUserName}]]></ToUserName>
<FromUserName><![CDATA[{context.ToUserName}]]></FromUserName>
<CreateTime>{context.CreateTime}</CreateTime>
<MsgType><![CDATA[text]]></MsgType>
<Content><![CDATA[{textResult}]]></Content>
</xml>";

            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.Response.ContentType = "text/xml";
            await context.HttpContext.Response.WriteAsync(resultText);
        }

        public override async Task OnRecieveMessage(WeiXinContext context) {
        }

        public override void LogTask(List<int> tenantIds, WeiXinMessage recieve, string body) {
            if (Options.MutilTenant)
                foreach (var tenantId in tenantIds) {
                    redisClient.Database.HashSet($"WeiXinProvider:{tenantId}:{recieve.FromUserName}",
                        $"{recieve.CreateTime}", body);
                }
            else
                redisClient.Database.HashSet($"WeiXinProvider:{recieve.FromUserName}", $"{recieve.CreateTime}", body);
        }
    }
}