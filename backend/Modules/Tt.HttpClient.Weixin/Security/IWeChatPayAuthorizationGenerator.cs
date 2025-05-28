using System.Net.Http;
using System.Threading.Tasks;

namespace TtWork.HttpClient.Weixin.Security;

public interface IWeChatPayAuthorizationGenerator {
    Task<string> GenerateAuthorizationAsync(HttpMethod method, string url, string body, string mchId, string p12Path);
}