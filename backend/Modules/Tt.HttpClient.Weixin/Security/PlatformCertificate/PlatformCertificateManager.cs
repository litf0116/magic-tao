using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tt.HttpClient.Weixin;
using TtWork.HttpClient.Weixin.Security.Extensions;

namespace TtWork.HttpClient.Weixin.Security.PlatformCertificate;

public class PlatformCertificateManager : IPlatformCertificateManager {
    public static string PlatformCertificatesCacheItemKey { get; set; } = nameof(PlatformCertificatesCacheItemKey);

    private readonly ILogger<PlatformCertificateManager> _logger;
    private readonly IV3PayApi _v3PayApi;
    private readonly ConcurrentDictionary<string, PlatformCertificateEntity> _certificatesCache = new();

    public PlatformCertificateManager(
        ILogger<PlatformCertificateManager> logger,
        IV3PayApi v3PayApi
    ) {
        _logger = logger;
        _v3PayApi = v3PayApi;
    }

    /// <summary>
    /// 获取平台证书
    /// https://pay.weixin.qq.com/docs/merchant/development/interface-rules/wechatpay-certificates-rotation.html
    /// </summary>
    /// <param name="mchId"></param>
    /// <param name="serialNo"></param>
    /// <param name="ApiV3Key"></param>
    /// <param name="certPath"></param>
    /// <returns></returns>
    public virtual async Task<PlatformCertificateEntity> GetPlatformCertificateAsync(string mchId, string serialNo,
        string ApiV3Key, string certPath) {
        // Check.NotNullOrWhiteSpace(mchId, nameof(mchId));
        // Check.NotNullOrWhiteSpace(serialNo, nameof(serialNo));

        var cacheItem = _certificatesCache.GetValueOrDefault(serialNo);
        if (cacheItem != null) return cacheItem;

        try {
            var certificates = await _v3PayApi.GetPlatformCertificatesAsync(mchId, certPath);

            foreach (var certificate in certificates.Data) {
                var certificateString = WeChatPaySecurityUtility.AesGcmDecrypt(ApiV3Key,
                    certificate.EncryptCertificateData.AssociatedData,
                    certificate.EncryptCertificateData.Nonce,
                    certificate.EncryptCertificateData.Ciphertext);

                _certificatesCache.TryAdd(
                    certificate.SerialNo,
                    new PlatformCertificateEntity(certificate.SerialNo, certificateString, certificate.EffectiveTime,
                        certificate.ExpireTime));
            }
        }
        catch (Exception e) {
            _logger.LogWarning("Fail to get and cache the platform certificates");
            _logger.LogError(e, e.Message);
        }

        return _certificatesCache[serialNo];
    }
}