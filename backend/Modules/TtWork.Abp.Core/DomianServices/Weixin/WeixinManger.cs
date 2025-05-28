using System;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tt.HttpClient.Weixin;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core;
using TtWork.HttpClient.Weixin;
using TtWork.Lib;
using TtWork.Lib.Redis;

namespace TtWork.Abp.DomianServices.Weixin {
    /// <summary>
    /// 微信管理
    /// </summary>
    public class WeixinManger(
        IWeixinApi weixinApi,
        ILogger<WeixinManger> logger,
        IRedisClient redisClient,
        IIocManager iocManager) : AppDomainServicebase(iocManager) {
        /// <summary>
        /// 取得公众号AccessToken(带缓存)
        /// </summary>
        public virtual async Task<string> GetAccessTokenAsync(string appid = null,
            string appSeret = null) {
            var key = $"accesstoken:{appid}";
            var cache = await redisClient.Database.StringGetAsync(key);
            if (cache.HasValue) {
                return cache.ToString();
            }

            var token = await weixinApi.GetToken(appid, appSeret);
            logger.LogInformation("请求appid: AccessToken:{@AccessTokenResult}", JsonConvert.SerializeObject(token));
            if (token == null || token.errcode != 0) throw new UserFriendlyException($"AccessToken获取失败 {token.errmsg}");
            await redisClient.Database.StringSetAsync(key, token.access_token, TimeSpan.FromSeconds(7200));
            return token.access_token;
        }
    }
}