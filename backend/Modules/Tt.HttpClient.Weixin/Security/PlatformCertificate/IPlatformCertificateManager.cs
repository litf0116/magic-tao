using System.Threading.Tasks;

namespace TtWork.HttpClient.Weixin.Security.PlatformCertificate;

public interface IPlatformCertificateManager {
    Task<PlatformCertificateEntity> GetPlatformCertificateAsync(string mchId, string serialNo,
        string ApiV3Key, string certPath);
}