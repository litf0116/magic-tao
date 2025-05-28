using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TtWork.Abp.DomianServices;
using Consts = TtWork.Abp.Consts;

namespace TtWork.Project.Web.Authentication.External {
    public class WechatOpenidProviderApi(
        ILogger<WechatOpenidProviderApi> logger)
        : ExternalAuthProviderApi {
        private const string ProviderName = Consts.LoginProvider.WeChatPub;

        public override async Task<ExternalAuthUserInfo>
            GetUserInfo(string code, string appid, string appsec) //因为需要获取微信放进User.Name 
        {
            var openid = code;
            var rndname = $"玩家{new Random().Next(10000, 99999)}";
            var authUserInfo = new ExternalAuthUserInfo {
                ProviderKey = openid,
                Provider = ProviderName,
                
                UserName = openid,
                Name = rndname,
                Surname = rndname,
                EmailAddress = openid + "@molitao.top",
                HeadImgUrl = AppConsts.UserDefaultAvatar,
                FromClient = FromClient.WechatPublic,
            };
            return authUserInfo;
        }
    }
}