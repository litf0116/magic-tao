using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TtWork.HttpClient.Weixin.WeixiinResult;
using TtWork.Lib;

namespace TtWork.HttpClient.Weixin {
    public class WeixinApi(
        System.Net.Http.HttpClient client,
        IDistributedCache cache,
        ILogger<WeixinApi> logger)
        : IWeixinApi {
        /// <summary>
        ///     <see cref="https://developers.weixin.qq.com/doc/offiaccount/Basic_Information/Get_access_token.html" />
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        public async Task<WeixinTokenResult> GetToken(string appid, string appSecret) {
            var requestData = new JObject
            {
                { "grant_type", "client_credential" },
                { "appid", appid },
                { "secret", appSecret }
            };

            var content = new StringContent(requestData.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("cgi-bin/stable_token", content);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // ip error: {"errcode":40164,"errmsg":"invalid ip 114.220.209.25 ipv6 ::ffff:114.220.209.25, not in whitelist hint: [eS4JRA00075263]"}
            // secret error :{"errcode":40013,"errmsg":"invalid appid"}
            // access_token invalid: {"errcode":40001,"errmsg":"invalid credential, access_token is invalid or not latest"}
            // success return {"access_token":"ACCESS_TOKEN","expires_in":7200}

            var result = JsonConvert.DeserializeObject<WeixinTokenResult>(jsonResponse);
            return result;
        }

        /// <summary>
        /// 取得公众号AccessToken(带缓存)
        /// 缓存时间15分钟，剩余时间少于5分钟时自动刷新
        /// </summary>
        public async Task<string> GetAccessTokenAsync(string appid = null, string appSecret = null) {
            var key = $"accesstoken:{appid}";
            var cacheValue = await cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cacheValue)) {
                return cacheValue;
            }

            var token = await GetToken(appid, appSecret);
            logger.LogInformation("请求appid: AccessToken:{@AccessTokenResult}", JsonConvert.SerializeObject(token));
            if (token == null || token.errcode != 0) {
                throw new Exception($"AccessToken获取失败 {token.errmsg}");
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(900)
            };
            await cache.SetStringAsync(key, token.access_token, options);
            return token.access_token;
        }

        /// <summary>
        /// 公众号获取用户信息
        /// <see cref="https://developers.weixin.qq.com/doc/offiaccount/User_Management/Get_users_basic_information_UnionID.html#UinonId"/>
        public async Task<WeixinUserInfoResult> GetUserInfo(string token, string openid) {
            var response =
                await client.GetAsync($"/cgi-bin/user/info?access_token={token}&openid={openid}&lang=zh_CN");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WeixinUserInfoResult>(jsonResponse);
            logger.LogInformation("{@GetUserInfo}", result);
            return result;
        }


        public async Task<MiniSessionResult> Mini_Code2Session(string code, string appid, string appSeret) {
            var response = await client.GetAsync($"sns/jscode2session?appid={appid}&secret={appSeret}&grant_type=authorization_code&js_code={code}");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            logger.LogInformation("{@jscode2session}", JsonConvert.SerializeObject(jsonResponse));
            var result = JsonConvert.DeserializeObject<MiniSessionResult>(jsonResponse);
            return result;
        }


        /// <summary>
        ///     获取小程序码，适用于需要的码数量较少的业务场景。通过该接口生成的小程序码，永久有效，有数量限制，详见获取二维码。
        ///     <see cref="https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/qr-code/wxacode.get.html" />
        /// </summary>
        /// <param name="token">接口调用凭证</param>
        /// <param name="path">扫码进入的小程序页面路径，最大长度 128 字节，不能为空；对于小游戏，可以只传入 query 部分，来实现传参效果，如：传入 "?foo=bar"，即可在 wx.getLaunchOptionsSync 接口中的 query 参数获取到 {foo:"bar"}。</param>
        /// <param name="width">二维码的宽度，单位 px。最小 280px，最大 1280px</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序码</param>
        /// <returns></returns>
        public async Task<byte[]> WxacodeGet(string token, string path,
            int width = 430, bool is_hyaline = false) {
            var postData = JsonConvert.SerializeObject(new { path, width, is_hyaline });

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData)));

            var response =
                await client.PostAsync($"wxa/getwxacode?access_token={token}", hc);

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        /// <summary>
        ///     获取小程序码，适用于需要的码数量极多的业务场景。通过该接口生成的小程序码，永久有效，数量暂无限制。 更多用法详见 获取二维码。
        ///     <see cref="https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/qr-code/wxacode.getUnlimited.html#HTTPS%20%E8%B0%83%E7%94%A8" />
        /// </summary>
        /// <param name="token">接口调用凭证</param>
        /// <param name="scene">最大32个可见字符，只支持数字，大小写英文以及部分特殊字符：!#$&'()*+,/:;=?@-._~，其它字符请自行编码为合法字符（因不支持%，中文无法使用 urlencode 处理，请使用其他编码方式）</param>
        /// <param name="page">必须是已经发布的小程序存在的页面（否则报错），例如 pages/index/index, 根路径前不要填加 /,不能携带参数（参数请放在scene字段里），如果不填写这个字段，默认跳主页面</param>
        /// <param name="width">二维码的宽度，单位 px，最小 280px，最大 1280px</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序</param>
        /// <returns></returns>
        public async Task<byte[]> WxacodeGetUnlimit(string token, string scene, string page = null,
            int width = 430, bool is_hyaline = false) {
            var postData = "";
            if (page.IsNullOrEmptyOrWhiteSpace() || page == "pages/index/index")
                postData = JsonConvert.SerializeObject(new { scene, width, is_hyaline });
            else
                postData = JsonConvert.SerializeObject(new { scene, page, width, is_hyaline });

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData)));

            var response = await client.PostAsync($"wxa/getwxacodeunlimit?access_token={token}", hc);
            var strResult = await response.Content.ReadAsStringAsync();
            var result = strResult.TryConvert<BaseWeChatReulst>();
            if (result != null) throw new Exception(result.errmsg);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes;
        }


        /// <summary>
        ///     公众号获取JS-SDK
        ///     <see cref="https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/JS-SDK.html" />
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<TicketResult> GetTicket(string token) {
            var strResponse = await client.GetStringAsync($"cgi-bin/ticket/getticket?access_token={token}&type=jsapi");
            var jsonReuslt = strResponse.TryConvert<TicketResult>();
            return await Task.FromResult(jsonReuslt);
        }

        /// <summary>
        ///     <see cref="https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/Wechat_webpage_authorization.html" />
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsec"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<OAuth2Result> GetOAuth2Token(string appid, string appsec, string code) {
            var strResponse = await client.GetStringAsync(
                $"sns/oauth2/access_token?appid={appid}&secret={appsec}&code={code}&grant_type=authorization_code");

            logger.LogDebug($"GetOAuth2Token Result:{strResponse}");
            var jsonReuslt = strResponse.TryConvert<OAuth2Result>();
            return await Task.FromResult(jsonReuslt);
        }

        /// <summary>
        ///     <see cref="https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/Wechat_webpage_authorization.html" />
        /// </summary>
        public async Task<WeixinUserInfoResult> SnsUserInfo(string access_token, string openid) {
            var strResponse =
                await client.GetStringAsync($"sns/userinfo?access_token={access_token}&openid={openid}&lang=zh_CN");

            logger.LogDebug($"SnsUserInfo Result:{strResponse}");
            var jsonReuslt = strResponse.TryConvert<WeixinUserInfoResult>();
            return await Task.FromResult(jsonReuslt);
        }

        public async Task<BaseWeChatReulst> CustomSend(string accessToken, string openid, string msgtype, object body) {
            var postData = new JObject
                { { "touser", openid }, { "msgtype", msgtype }, { msgtype, JObject.FromObject(body) } };

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData.ToString())));

            var strResponse =
                await client.PostAsync($"cgi-bin/message/custom/send?access_token={accessToken}", hc);

            var result = await strResponse.Content.ReadAsStringAsync();

            return result.TryConvert<BaseWeChatReulst>();
        }

        public async Task<string> GetQrCode(string accessToken, string sceneStr,
            QrCodeType type = QrCodeType.QR_STR_SCENE, int expireSeconds = 604800) {
            var postData = new JObject {
                { "action_name", type.ToString() },
                { "action_info", new JObject { { "scene", new JObject { { "scene_str", sceneStr } } } } }
            };
            if (type == QrCodeType.QR_STR_SCENE)
                postData["expire_seconds"] = expireSeconds;

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData.ToString())));
            var strResponse =
                await client.PostAsync($"cgi-bin/qrcode/create?access_token={accessToken}", hc);

            var result = await strResponse.Content.ReadAsStringAsync();
            var errResult = result.TryConvert<BaseWeChatReulst>();
            if (errResult != null && !errResult.errmsg.IsNullOrEmptyOrWhiteSpace()) {
                logger.LogError("GetQrCode Error {@result}", result);
                throw new Exception(errResult.errmsg);
            }

            var jsonReuslt = result.TryConvert<GetQrCodeResult>();
            return $"https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={jsonReuslt.ticket}";
        }

        public async Task<MediaCheckResult> MediaCheckAsync(string accessToken, string mediaUrl, int mediaType = 1) {
            var postData = new JObject {
                { "media_url", mediaUrl },
                { "media_type", mediaType }
            };

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData.ToString())));
            hc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync($"wxa/media_check_async?access_token={accessToken}", hc);
            var result = await response.Content.ReadAsStringAsync();

            logger.LogInformation("MediaCheck Result: {@result}", result);

            return result.TryConvert<MediaCheckResult>();
        }

        public async Task<MsgSecCheckResult> MsgSecCheck(string accessToken, string content, int version = 1, int scene = 1, string openid = "", string title = "") {
            var postData = new JObject {
                { "content", content },
                { "version", version },
                { "scene", scene },
                { "openid", openid },
                { "title", title }
            };

            HttpContent hc = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(postData.ToString())));
            hc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync($"wxa/msg_sec_check?access_token={accessToken}", hc);
            var result = await response.Content.ReadAsStringAsync();

            logger.LogInformation("MsgSecCheck Result: {@result}", result);

            return result.TryConvert<MsgSecCheckResult>();
        }

        /// <summary>
        ///     图片安全检测
        ///     <see cref="https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/safety/safety风险的图片检测接口.html" />
        /// </summary>
        /// <param name="accessToken">接口调用凭证</param>
        /// <param name="imageBuffer">图片二进制数据</param>
        /// <returns></returns>
        public async Task<BaseWeChatReulst> ImgSecCheck(string accessToken, byte[] imageBuffer) {
            using var content = new MultipartFormDataContent();
            using var imageContent = new ByteArrayContent(imageBuffer);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                Name = "\"media\""
            };
            content.Add(imageContent);

            var response = await client.PostAsync($"wxa/img_sec_check?access_token={accessToken}", content);
            var result = await response.Content.ReadAsStringAsync();

            logger.LogInformation("ImgSecCheck Result: {@result}", result);

            return result.TryConvert<BaseWeChatReulst>();
        }

        public async Task<OAuth2Result> GetOpenPlatformAccessTokenAsync(string appid, string secret, string code) {
            var url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={appid}&secret={secret}&code={code}&grant_type=authorization_code";
            logger.LogInformation("请求微信开放平台 OAuth2 接口: {Url}", url);

            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            logger.LogInformation("微信开放平台 OAuth2 响应: {Result}", result);

            var oauthResult = result.TryConvert<OAuth2Result>();

            if (oauthResult == null || !string.IsNullOrEmpty(oauthResult.errmsg)) {
                logger.LogError("微信开放平台 OAuth2 认证失败: {ErrorCode} - {ErrorMessage}",
                    oauthResult?.errcode, oauthResult?.errmsg);
                throw new Exception($"微信开放平台认证失败: {oauthResult?.errmsg}");
            }

            logger.LogInformation("微信开放平台 OAuth2 认证成功: OpenId={OpenId}, UnionId={UnionId}",
                oauthResult.openid, oauthResult.unionid);

            return oauthResult;
        }
    }

    public class GetQrCodeResult {
        public string ticket { get; set; }
        public int expire_seconds { get; set; }
        public string url { get; set; }
    }

    public enum QrCodeType {
        QR_STR_SCENE,
        QR_LIMIT_STR_SCENE
    }


    public class OAuth2Result {
        public string access_token { get; set; }

        public int expires_in { get; set; }

        public string refresh_token { get; set; }

        public string openid { get; set; }

        public string scope { get; set; }
    }

    public class MediaCheckResult : BaseWeChatReulst {
        public MediaTraceResult trace_id { get; set; }
        public int errcode { get; set; }
        public string errmsg { get; set; }
    }

    public class MediaTraceResult {
        public string trace_id { get; set; }
        public MediaDetailResult detail { get; set; }
    }

    public class MediaDetailResult {
        public MediaCheckTraceInfo[] trace { get; set; }
    }

    public class MediaCheckTraceInfo {
        public string media_id { get; set; }
        public string media_type { get; set; }
        public string suggest { get; set; }
        public string label { get; set; }
        public int probability { get; set; }
    }

    public class MsgSecCheckResult : BaseWeChatReulst {
        public Detail[] detail { get; set; }
    }

    public class Detail {
        public string strategy { get; set; }
        public int errcode { get; set; }
        public string suggest { get; set; }
    }
}