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
        /// 缓存时间15分钟，剩余时间少于5分钟时自动刷新
        /// </summary>
        public virtual async Task<string> GetAccessTokenAsync(string appid = null,
            string appSeret = null) {
            var key = $"accesstoken:{appid}";
            var cache = await redisClient.Database.StringGetAsync(key);
            
            if (cache.HasValue)
            {
                var ttl = await redisClient.Database.KeyTimeToLiveAsync(key);
                
                // 如果剩余时间少于5分钟，异步刷新（不阻塞当前请求）
                if (ttl.HasValue && ttl.Value.TotalSeconds < 300)
                {
                    _ = Task.Run(async () => {
                        try
                        {
                            var newToken = await weixinApi.GetToken(appid, appSeret);
                            if (newToken?.errcode == 0)
                            {
                                await redisClient.Database.StringSetAsync(key, newToken.access_token, TimeSpan.FromSeconds(900));
                                logger.LogInformation("Access token提前刷新成功: {AppId}", appid);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Access token提前刷新失败: {AppId}", appid);
                        }
                    });
                }
                
                return cache.ToString();
            }

            var token = await weixinApi.GetToken(appid, appSeret);
            logger.LogInformation("请求appid: AccessToken:{@AccessTokenResult}", JsonConvert.SerializeObject(token));
            if (token == null || token.errcode != 0) throw new UserFriendlyException($"AccessToken获取失败 {token.errmsg}");
            
            await redisClient.Database.StringSetAsync(key, token.access_token, TimeSpan.FromSeconds(900));
            return token.access_token;
        }
    }
}