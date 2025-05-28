using System.Threading.Tasks;
using System.Xml;

namespace TtWork.HttpClient.Weixin.Infrastructure
{
    public interface IWeChatPayApiRequester
    {
        Task<XmlDocument> RequestAsync(string url, string body);
    }
}