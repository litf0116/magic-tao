using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TtWork.Lib.Http
{
    /// <summary>
    /// HttpClient 服务实现 - 使用单例模式避免资源泄漏
    /// </summary>
    public class HttpClientService : IHttpClientService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;
        private bool _disposed = false;

        public HttpClientService(ILogger<HttpClientService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MagicTao/1.0");
        }

        public async Task<T> GetJsonAsync<T>(string url)
        {
            EnsureNotDisposed();

            try
            {
                _logger.LogDebug("GET JSON: {Url}", url);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(content);

                _logger.LogDebug("GET JSON 成功: {Url}", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET JSON 失败: {Url}", url);
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string url, object data) where T : class, new()
        {
            EnsureNotDisposed();

            try
            {
                _logger.LogDebug("POST JSON: {Url}", url);
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(responseContent);

                _logger.LogDebug("POST JSON 成功: {Url}", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST JSON 失败: {Url}", url);
                throw;
            }
        }

        public string PostHtml(string url, string strPostdata, string encoding = "utf-8", string stringType = "application/x-www-form-urlencoded")
        {
            EnsureNotDisposed();

            try
            {
                _logger.LogDebug("POST HTML: {Url}", url);
                var content = new StringContent(strPostdata, Encoding.GetEncoding(encoding), stringType);

                var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _logger.LogDebug("POST HTML 成功: {Url}", url);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST HTML 失败: {Url}", url);
                throw;
            }
        }

        public byte[] PostGotImageByte(string url, object obj)
        {
            EnsureNotDisposed();

            try
            {
                _logger.LogDebug("POST IMAGE: {Url}", url);
                var json = JsonConvert.SerializeObject(obj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                _logger.LogDebug("POST IMAGE 成功: {Url}, Size: {Size} bytes", url, result.Length);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST IMAGE 失败: {Url}", url);
                throw;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HttpClientService));
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
                _logger.LogInformation("HttpClientService 已释放");
            }
        }
    }
}