using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tt.HttpClient.Weixin.Extensions;
using TtWork.HttpClient.Weixin.Extensions;
using TtWork.HttpClient.Weixin.Models;
using TtWork.HttpClient.Weixin.Security;

namespace Tt.HttpClient.Weixin;

public interface IV3PayApi {
    public Task<CreateH5OrderResponse> CreateH5OrderAsync(CreateOrderRequest request, string certPath);
    public Task<CreateOrderResponse> CreateJsOrderAsync(CreateOrderRequest request, string certPath);
    public Task<CreateNativeOrderResponse> CreateNativeOrderAsync(CreateNativeOrderRequest request, string certPath);

    public Task<GetJsSdkWeChatPayParametersResult> GetJsSdkWeChatPayParametersAsync(
        GetJsSdkWeChatPayParametersInput input, string certPath);

    public Task<GetPlatformCertificatesResponse> GetPlatformCertificatesAsync(string mchId, string certPath);

    public Task<QueryOrderResponse> QueryOrderAsync(string mchId, string outTradeNo, string certPath);
}

public class V3PayApi(
    ILogger<V3PayApi> logger,
    System.Net.Http.HttpClient client,
    IWeChatPayAuthorizationGenerator weChatPayAuthorizationGenerator
) : IV3PayApi {
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        NullValueHandling = NullValueHandling.Ignore
    };


    public async Task<GetJsSdkWeChatPayParametersResult> GetJsSdkWeChatPayParametersAsync(
        GetJsSdkWeChatPayParametersInput input, string certPath) {
        if (string.IsNullOrEmpty(input.PrepayId)) {
            return new GetJsSdkWeChatPayParametersResult("请传入有效的预支付订单 Id。");
        }

        const string signType = "RSA";
        var nonceStr = RandomStringHelper.GetRandomString();
        var timeStamp = DateTimeHelper.GetNowTimeStamp();
        var package = $"prepay_id={input.PrepayId}";

        var waitSignString = new StringBuilder();
        waitSignString.Append(input.AppId).Append('\n')
            .Append(timeStamp).Append('\n')
            .Append(nonceStr).Append('\n')
            .Append("prepay_id=").Append(input.PrepayId).Append('\n');


        var certificate = await GetCertificateAsync(input.MchId, certPath);
        var paySign = GetSignature(waitSignString.ToString(), certificate);

        return new GetJsSdkWeChatPayParametersResult(nonceStr, timeStamp, package, signType, paySign);
    }


    public const string CreateH5OrderUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/h5";
    public const string CreateOrderUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/jsapi";
    public const string CreateNativeOrderUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/native";
    public const string CertificatesUrl = "https://api.mch.weixin.qq.com/v3/certificates";
    public const string QueryOrderBaseUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/out-trade-no";

    public Task<CreateH5OrderResponse> CreateH5OrderAsync(CreateOrderRequest request, string certPath) {
        return RequestAsync<CreateH5OrderResponse>(HttpMethod.Post, CreateH5OrderUrl, request, request.MchId, certPath);
    }

    public Task<CreateOrderResponse> CreateJsOrderAsync(CreateOrderRequest request, string certPath) {
        return RequestAsync<CreateOrderResponse>(HttpMethod.Post, CreateOrderUrl, request, request.MchId, certPath);
    }

    public Task<CreateNativeOrderResponse> CreateNativeOrderAsync(CreateNativeOrderRequest request, string certPath) {
        return RequestAsync<CreateNativeOrderResponse>(HttpMethod.Post, CreateNativeOrderUrl, request, request.MchId, certPath);
    }

    public virtual Task<GetPlatformCertificatesResponse> GetPlatformCertificatesAsync(string mchId, string certPath) {
        return RequestAsync<GetPlatformCertificatesResponse>(HttpMethod.Get, CertificatesUrl, null, mchId, certPath);
    }

    public Task<QueryOrderResponse> QueryOrderAsync(string mchId, string outTradeNo, string certPath) {
        var url = $"{QueryOrderBaseUrl}/{outTradeNo}?mchid={mchId}";
        return RequestAsync<QueryOrderResponse>(HttpMethod.Get, url, null, mchId, certPath);
    }

    public Task<TResponse> RequestAsync<TResponse>(HttpMethod method, string url, object body, string mchId = null,
        string certPath = null) {
        return RequestAsync<TResponse>(method, url, HandleRequestObject(method, body), mchId, certPath);
    }

    public async Task<TResponse> RequestAsync<TResponse>(HttpMethod method, string url, string body,
        string mchId = null, string certPath = null) {
        var responseString = await RequestAsync(method, url, body, mchId, certPath);

        return JsonConvert.DeserializeObject<TResponse>(responseString);
    }

    public async Task<string> RequestAsync(HttpMethod method, string url, string body, string mchId = null,
        string certPath = null) {
        var response = await RequestRawAsync(method, url, body, mchId, certPath);
        await LogFailureResponseAsync(response);

        return await response.Content.ReadAsStringAsync();
    }


    public async Task<HttpResponseMessage> RequestRawAsync(HttpMethod method, string url, string body = null,
        string mchId = null, string certPath = null) {
        var request = CreateRequest(method, url, body);

        // Setting the request header for the http client.
        // var options = await _optionsProvider.GetAsync(mchId);
        // var language = options.AcceptLanguage ?? ApiLanguages.DefaultLanguage;
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("TtWork.WeChat.Pay", "1.0.0"));

        var Authorization = await weChatPayAuthorizationGenerator.GenerateAuthorizationAsync(method, url, body, mchId, certPath);
        request.Headers.Add("Authorization", Authorization);

        // Sending the request.
        // var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request);
    }

    private string HandleRequestObject(HttpMethod method, object body) {
        if (method == HttpMethod.Post || method == HttpMethod.Put) {
            return JsonConvert.SerializeObject(body, JsonSerializerSettings);
        }

        if (method != HttpMethod.Get) return null;
        if (body is string bodyStr) {
            return bodyStr;
        }

        // Convert the object to query string.
        return WeChatReflectionHelper.ConvertToQueryString(body);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string body) {
        if (method == HttpMethod.Post || method == HttpMethod.Put) {
            return new HttpRequestMessage(method, url) {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        if (method == HttpMethod.Get && !string.IsNullOrEmpty(body)) {
            return new HttpRequestMessage(HttpMethod.Get, $"{url}?{body}");
        }

        return new HttpRequestMessage(method, url);
    }

    protected virtual async Task LogFailureResponseAsync(HttpResponseMessage responseMessage) {
        switch (responseMessage.StatusCode) {
            case HttpStatusCode.OK:
            case HttpStatusCode.Accepted:
            case HttpStatusCode.NoContent:
                return;
            default:
                logger.LogError("微信支付接口调用失败，HTTP状态码：{StatusCode}，返回内容：{Content}",
                    responseMessage.StatusCode, await responseMessage.Content.ReadAsStringAsync());
                break;
        }
    }

    public async Task<WeChatPayCertificate> GetCertificateAsync(string mchId, string certPath) {
        var certificate = new WeChatPayCertificate(mchId, await File.ReadAllBytesAsync(certPath), mchId);
        return certificate;
    }

    public string GetSignature(string pendingSignature, WeChatPayCertificate certificate) {
        var privateKey = certificate.X509Certificate.GetRSAPrivateKey();
        var signDataBytes = privateKey.SignData(Encoding.UTF8.GetBytes(pendingSignature), HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signDataBytes);
    }
}