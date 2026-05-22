using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;
using Castle.Core.Logging;
using Newtonsoft.Json;

namespace TtWork.Abp.Core.Net.Sms
{
    public class SmsSender : ISmsSender, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly SmsSettings _smsSettings;

        public SmsSender()
        {
            Logger = NullLogger.Instance;
            _smsSettings = new SmsSettings();
        }

        public async Task SendAsync(string number, string message)
        {
            try
            {
                var codeMatch = Regex.Match(message, @"(\d{6})");
                if (!codeMatch.Success)
                {
                    Logger.Warn($"[阿里云短信] 消息中未找到6位验证码: {message}");
                    throw new UserFriendlyException("短信模板格式错误");
                }

                var code = codeMatch.Groups[1].Value;

                if (string.IsNullOrEmpty(_smsSettings.AccessKeyId) ||
                    string.IsNullOrEmpty(_smsSettings.AccessKeySecret))
                {
                    Logger.Info($"[测试模式] 短信验证码已发送");
                    Logger.Info($"[测试模式] 手机号: {number}");
                    Logger.Info($"[测试模式] 验证码: {code} (5分钟内有效)");
                    return;
                }

                var response = await SendSmsAsync(number, code);
                Logger.Info($"[阿里云短信] 发送结果 - Code: {response.Code}, Message: {response.Message}");

                if (response.Code != "OK")
                {
                    throw new UserFriendlyException($"短信发送失败: {response.Message}");
                }

                Logger.Info($"[阿里云短信] 短信发送成功 - 手机号: {number}, 验证码: {code}");
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"[阿里云短信] 发送异常: {ex.Message}", ex);
                throw new UserFriendlyException($"短信发送异常: {ex.Message}");
            }
        }

        private async Task<SendSmsResponse> SendSmsAsync(string phoneNumber, string code)
        {
            var parameters = new SortedDictionary<string, string>
            {
                { "AccessKeyId", _smsSettings.AccessKeyId },
                { "Action", "SendSms" },
                { "Format", "JSON" },
                { "PhoneNumbers", phoneNumber },
                { "SignName", _smsSettings.SignName },
                { "SignatureMethod", "HMAC-SHA256" },
                { "SignatureNonce", Guid.NewGuid().ToString() },
                { "SignatureVersion", "1.0" },
                { "TemplateCode", _smsSettings.TemplateCode },
                { "TemplateParam", $"{{\"code\":\"{code}\"}}" },
                { "Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "Version", "2017-05-25" }
            };

            var signature = ComputeSignature(
                parameters,
                _smsSettings.AccessKeySecret,
                "GET",
                "dysmsapi.aliyuncs.com",
                "/"
            );
            parameters["Signature"] = signature;

            var queryString = BuildQueryString(parameters);
            var requestUrl = $"https://dysmsapi.aliyuncs.com/?{queryString}";

            using var httpClient = new System.Net.Http.HttpClient();
            var response = await httpClient.GetAsync(requestUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            Logger.Info($"[阿里云短信] API响应: {responseContent}");

            return JsonConvert.DeserializeObject<SendSmsResponse>(responseContent)
                ?? new SendSmsResponse { Code = "ERROR", Message = "Failed to parse response" };
        }

        private static string ComputeSignature(
            SortedDictionary<string, string> parameters,
            string accessKeySecret,
            string method,
            string host,
            string path)
        {
            var canonicalizedQuery = BuildCanonicalizedQuery(parameters);
            var stringToSign = $"{method}\n{host}\n{path}\n{canonicalizedQuery}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(accessKeySecret + "&"));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hash);
        }

        private static string BuildCanonicalizedQuery(SortedDictionary<string, string> parameters)
        {
            var result = new StringBuilder();
            foreach (var param in parameters)
            {
                if (result.Length > 0)
                    result.Append("&");

                result.Append(Uri.EscapeDataString(param.Key));
                result.Append("=");
                result.Append(Uri.EscapeDataString(param.Value));
            }
            return result.ToString();
        }

        private static string BuildQueryString(SortedDictionary<string, string> parameters)
        {
            return BuildCanonicalizedQuery(parameters);
        }
    }

    public class SendSmsResponse
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
        public string RequestId { get; set; } = "";
        public string BizId { get; set; } = "";
    }

    public class SmsSettings
    {
        public string AccessKeyId { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_ACCESSKEYID") ?? "";

        public string AccessKeySecret { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_ACCESSKEYSECRET") ?? "";

        public string SignName { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_SIGNNAME") ?? "魔力淘";

        public string TemplateCode { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_TEMPLATE_CODE") ?? "SMS_333905928";
    }
}
