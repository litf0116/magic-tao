using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TtWork.HttpClient.Weixin.Helpers;
using TtWork.HttpClient.Weixin.Models;

namespace TtWork.HttpClient.Weixin.Security;

public class WeChatPayAuthorizationGenerator : IWeChatPayAuthorizationGenerator {
    
    
    /// <summary>
    /// 授权(Authorization)标头的认证类型。
    /// </summary>
    public const string AuthorizationScheme = "WECHATPAY2-SHA256-RSA2048";
    
    public async Task<string> GenerateAuthorizationAsync(HttpMethod method, string url, string body, string mchId, string p12Path) {
        var timeStamp = DateTimeHelper.GetNowTimeStamp().ToString();
        var nonceStr = RandomStringHelper.GetRandomString();

        var requestModel = new WeChatPayApiRequestModel(method, url, body, timeStamp, nonceStr);
        var pendingSignature = requestModel.GetPendingSignatureString();

        WeChatPayCertificate certificate = new WeChatPayCertificate(mchId, File.ReadAllBytes(p12Path), mchId);
        
        var signString = RsaSign(pendingSignature, certificate);

        return
            $"{AuthorizationScheme} mchid=\"{mchId}\",nonce_str=\"{nonceStr}\",timestamp=\"{timeStamp}\",serial_no=\"{certificate.X509Certificate.SerialNumber}\",signature=\"{signString}\"";
    }

    private string RsaSign(string pendingSignature, WeChatPayCertificate certificate) {
        var privateKey = certificate.X509Certificate.GetRSAPrivateKey();
        var signDataBytes = privateKey!.SignData(Encoding.UTF8.GetBytes(pendingSignature), HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signDataBytes);
    }
}