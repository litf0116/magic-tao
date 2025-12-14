using System;
using System.Threading.Tasks;

namespace TtWork.Lib.Http
{
    /// <summary>
    /// HttpClient 服务接口
    /// </summary>
    public interface IHttpClientService
    {
        Task<T> GetJsonAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data) where T : class, new();
        string PostHtml(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded");
        byte[] PostGotImageByte(string url, object obj);
    }
}