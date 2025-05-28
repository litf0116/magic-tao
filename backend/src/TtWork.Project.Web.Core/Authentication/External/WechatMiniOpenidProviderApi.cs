using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.UI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tt.HttpClient.Weixin;
using TtWork.Abp.DomianServices;
using TtWork.Abp.DomianServices.Weixin;
using TtWork.HttpClient.Weixin;
using TtWork.Lib;
using TtWork.Project.Authentication.External;
using TtWork.Project.Web.Models.TokenAuth;
using Consts = TtWork.Abp.Consts;

namespace TtWork.Project.Web.Authentication.External {
    public class WechatMiniOpenidProviderApi(
        ILogger<WechatMiniOpenidProviderApi> logger,
        IWeixinApi weixinApi
    )
        : ExternalAuthProviderApi {
        private string _providerName = Consts.LoginProvider.WeChatMiniOpenid;
        private string _providerKey;

        public override async Task<ExternalAuthUserInfo>
            GetUserInfo(string code, string appid, string appSec) //因为需要获取微信放进User.Name 
        {
            var authModel = JsonConvert.DeserializeObject<WeChatMiniProgramAuthenticateModel>(code);

            var wxUser = await weixinApi.Mini_Code2Session(authModel.code, appid, appSec);
            if (wxUser.unionid.IsNullOrEmptyOrWhiteSpace())
                throw new UserFriendlyException("解密小程序Code失败,请重试");

            _providerKey = wxUser.openid;
            Dictionary<string, string> userLogins = new Dictionary<string, string>();
            userLogins.Add(_providerName, _providerKey);

            if (wxUser.unionid != null) {
                _providerName = Consts.LoginProvider.WeChatUnionId;
                _providerKey = wxUser.unionid;
                userLogins.Add(_providerName, _providerKey);
            }

            var rndname = $"玩家{new Random().Next(10000, 99999)}";

            var authUserInfo = new ExternalAuthUserInfo {
                ProviderKey = _providerKey,
                Provider = _providerName,

                UserName = _providerKey,
                Name = rndname,
                Surname = rndname,
                EmailAddress = _providerKey + "@molitao.top",
                HeadImgUrl = AppConsts.UserDefaultAvatar,
                FromClient = FromClient.WechatMini,
                PhoneNumber = "18012341234",
                Extension = wxUser,
                UserLogins = userLogins
            };
            return authUserInfo;
        }
    }
}