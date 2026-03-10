using System.Threading.Tasks;
using TtWork.HttpClient.Weixin.WeixiinResult;

namespace TtWork.HttpClient.Weixin {
    public interface IWeixinApi {
        Task<WeixinTokenResult> GetToken(string appid, string appSecret);
        Task<string> GetAccessTokenAsync(string appid = null, string appSecret = null);
        Task<WeixinUserInfoResult> GetUserInfo(string token, string openid);
        Task<MiniSessionResult> Mini_Code2Session(string code, string appid, string appSeret);
        Task<byte[]> WxacodeGet(string token, string path, int width = 430, bool is_hyaline = false);

        Task<byte[]> WxacodeGetUnlimit(string token, string scene, string page = "pages/index/index", int width = 430,
            bool is_hyaline = false);

        Task<TicketResult> GetTicket(string token);

        Task<OAuth2Result> GetOAuth2Token(string appid, string appsec, string code);

        Task<WeixinUserInfoResult> SnsUserInfo(string access_token, string openid);

        Task<BaseWeChatReulst> CustomSend(string accessToken, string openid, string msgtype, object body);

        Task<string> GetQrCode(string accessToken, string sceneStr, QrCodeType type = QrCodeType.QR_STR_SCENE,
            int expireSeconds = 604800);

        Task<MediaCheckResult> MediaCheckAsync(string accessToken, string mediaUrl, int mediaType = 1);

        Task<MsgSecCheckResult> MsgSecCheck(string accessToken, string content, int version = 1, int scene = 1, string openid = "", string title = "");

        Task<BaseWeChatReulst> ImgSecCheck(string accessToken, byte[] imageBuffer);
    }
}