using System.Threading.Tasks;
using Abp.Dependency;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TtWork.Lib;

namespace TtWork.Abp.Extensions {
    /// <summary>
    /// IWxSubscribeMessageSender
    /// </summary>
    public interface IWxSubscribeMessageSender {
        /// <summary>
        /// <see cref="https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/subscribe-message/subscribeMessage.send.html"/>
        /// </summary>
        /// <param name="openids">接收者（用户）的 openid</param>
        /// <param name="accessToken"></param>
        /// <param name="templateId">所需下发的订阅模板id</param>
        /// <param name="data">模板内容，格式形如 { "key1": { "value": any }, "key2": { "value": any } }</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转。</param>
        /// <param name="miniprogram_state">跳转小程序类型：developer为开发版；trial为体验版；formal为正式版；默认为正式版</param>
        /// <param name="lang">进入小程序查看”的语言类型，支持zh_CN(简体中文)、en_US(英文)、zh_HK(繁体中文)、zh_TW(繁体中文)，默认为zh_CN</param>
        /// <returns></returns>
        Task<(bool, string)> SendAsync(string[] openids,
            string accessToken,
            string templateId,
            object data,
            string page = "",
            string miniprogram_state = "formal",
            string lang = "zh_CN");
    }


    /// <summary>
    /// WxSubscribeMessageSender
    /// </summary>
    public class WxSubscribeMessageSender : IWxSubscribeMessageSender, ITransientDependency {
        private readonly ILogger<WxSubscribeMessageSender> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public WxSubscribeMessageSender(ILogger<WxSubscribeMessageSender> logger) {
            _logger = logger;
        }

        public async Task<(bool, string)> SendAsync(string[] openids, string accessToken, string template_id,
            object data, string page = "", string miniprogram_state = "formal",
            string lang = "zh_CN") {
            var _result = (true, "");
            foreach (var openid in openids) {
                var apiurl = $"https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token={accessToken}";

                if (openid.IsNullOrEmptyOrWhiteSpace())
                    break;

                object postData = new {
                    touser = openid,
                    template_id = template_id,
                    data = data,
                    page = page,
                    miniprogram_state = miniprogram_state,
                    lang = lang
                };


                var result = await HttpEx.PostAsync<dynamic>(apiurl, postData);
                string wx_result = JsonConvert.SerializeObject(result);

                if (result.errmsg != "ok") {
                    _logger.LogError("小程序模版消息发送失败!{@wx_result} {@request_data}", wx_result,
                        JsonConvert.SerializeObject(postData));
                    _result.Item1 = false;
                    _result.Item2 += result.errmsg;
                }
                else {
                    _logger.LogInformation("小程序模版消息发送成功!{@wx_result} {@request_data}", wx_result,
                        JsonConvert.SerializeObject(postData));
                }
            }

            return _result;
        }
    }
}