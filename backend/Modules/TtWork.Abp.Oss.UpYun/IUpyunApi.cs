using System.Net.Http;
using System.Threading.Tasks;

namespace TtWork.Abp.Oss.UpYun {
    public interface IUpyunApi {
        Task<byte[]> GetBytesAsync(string imgUrl);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);
    }

    public class UpyunApi : IUpyunApi {
        private readonly System.Net.Http.HttpClient _client;

        public UpyunApi(System.Net.Http.HttpClient client) {
            _client = client;
        }

        public virtual async Task<byte[]> GetBytesAsync(string imgUrl) {
            return await _client.GetByteArrayAsync(imgUrl);
        }


        public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message) {
            return await _client.SendAsync(message);
        }
    }
}