using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TtWork.Lib
{
    /// <summary>
    /// HttpClient 单例服务，避免资源泄漏
    /// </summary>
    public interface IHttpClientService
    {
        Task<T> GetJsonAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data) where T : class, new();
        string PostHtml(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded");
        byte[] PostGotImageByte(string url, object obj);
    }

    public class HttpClientService : IHttpClientService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;
        private bool _disposed = false;

        public HttpClientService(ILogger<HttpClientService> logger)
        {
            _logger = logger;

            _httpClient = new HttpClient();

            // 设置默认请求头
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 设置超时时间
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // 设置 User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MagicTao/1.0");
        }

        public async Task<T> GetJsonAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                responseBody = responseBody.Replace("\uFEFF", "");

                return JsonConvert.DeserializeObject<T>(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP GET请求失败: {Url}", url);
                throw;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "HTTP GET请求超时: {Url}", url);
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string url, object data) where T : class, new()
        {
            try
            {
                string content = JsonConvert.SerializeObject(data);
                var buffer = Encoding.UTF8.GetBytes(content);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync(url, byteContent).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("HTTP POST请求返回非成功状态码: {StatusCode}, URL: {Url}, Response: {Response}",
                        response.StatusCode, url, result);
                    return new T();
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP POST请求失败: {Url}", url);
                throw;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "HTTP POST请求超时: {Url}", url);
                throw;
            }
        }

        public async Task<string> PostHtmlAsync(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded")
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                request.Content = new StringContent(strPostdata, Encoding.GetEncoding(encoding), stringType);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP POST HTML请求失败: {Url}", url);
                throw;
            }
        }

        public string PostHtml(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded")
        {
            return PostHtmlAsync(url, strPostdata, encoding, stringType).GetAwaiter().GetResult();
        }

        public async Task<byte[]> PostGotImageByteAsync(string url, object obj)
        {
            try
            {
                var postData = JsonConvert.SerializeObject(obj);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentType.MediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new Exception($"Expected image but received JSON: {json}");
                }

                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP POST 图片请求失败: {Url}", url);
                throw;
            }
        }

        public byte[] PostGotImageByte(string url, object obj)
        {
            return PostGotImageByteAsync(url, obj).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}