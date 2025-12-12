using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TtWork.Lib
{
    /// <summary>
    /// HttpClient 扩展方法 - 使用单例 HttpClientService 避免资源泄漏
    /// </summary>
    public static class HttpEx
    {
        private static IHttpClientService _httpClientService;

        /// <summary>
        /// 初始化 HttpClientService
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _httpClientService = serviceProvider.GetRequiredService<IHttpClientService>();
        }

        /// <summary>
        /// 确保 HttpClientService 已初始化
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_httpClientService == null)
            {
                throw new InvalidOperationException("HttpClientService 未初始化。请先调用 HttpEx.Initialize() 方法。");
            }
        }

        public static async Task<T> GetJsonAsync<T>(string url)
        {
            EnsureInitialized();
            return await _httpClientService.GetJsonAsync<T>(url);
        }

        public static async Task<T> PostAsync<T>(string url, object data) where T : class, new()
        {
            EnsureInitialized();
            return await _httpClientService.PostAsync<T>(url, data);
        }

        public static string PostHtml(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded")
        {
            EnsureInitialized();
            return _httpClientService.PostHtml(url, strPostdata, encoding, stringType);
        }

        public static byte[] PostGotImageByte(string url, object obj)
        {
            EnsureInitialized();
            return _httpClientService.PostGotImageByte(url, obj);
        }
    }
}