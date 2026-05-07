using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TtWork.Abp.DomianServices;
using TtWork.HttpClient.Weixin;
using Consts = TtWork.Abp.Consts;

namespace TtWork.Project.Web.Authentication.External {
    /// <summary>
    /// 微信公众号网页授权登录
    /// </summary>
    public class WechatPubAuthProviderApi(
        ILogger<WechatPubAuthProviderApi> logger,
        IWeixinApi weixinApi)
        : ExternalAuthProviderApi {
        private string _providerName = Consts.LoginProvider.WeChatPubOpenid;
        private string _providerKey;

        public override async Task<ExternalAuthUserInfo>
            GetUserInfo(string code, string appid, string appsec) //因为需要获取微信放进User.Name 
        {
            //通过，用code换取access_token
            var oAuth2Token = await weixinApi.GetOAuth2Token(appid, appsec, code);
            var wxUser = await weixinApi.SnsUserInfo(oAuth2Token.access_token, oAuth2Token.openid);

            _providerKey = wxUser.openid;
            Dictionary<string, string> userLogins = new Dictionary<string, string>();
            userLogins.Add(_providerName, _providerKey);

            if (wxUser.unionid != null) {
                _providerName = Consts.LoginProvider.WeChatUnionId;
                _providerKey = wxUser.unionid;
                userLogins.Add(_providerName, _providerKey);
            }

            var rndname = $"玩家{Random.Shared.Next(10000, 99999)}";

            var authUserInfo = new ExternalAuthUserInfo {
                ProviderKey = _providerKey,
                Provider = _providerName,

                UserName = _providerKey,

                Name = rndname,
                Surname = rndname,

                EmailAddress = _providerKey + "@molitao.top",
                HeadImgUrl = AppConsts.UserDefaultAvatar,

                FromClient = FromClient.WechatPublic,
                Extension = wxUser,
                UserLogins = userLogins
            };
            return authUserInfo;
        }
    }
}