using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;
using Castle.Core.Logging;
using AlibabaCloud.SDK.Dysmsapi20170525;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using AlibabaCloud.OpenApiClient.Models;
using Tea;

namespace TtWork.Abp.Core.Net.Sms
{
    public class SmsSender : ISmsSender, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly SmsSettings _smsSettings;

        private Client _client;

        public SmsSender()
        {
            Logger = NullLogger.Instance;
            _smsSettings = new SmsSettings();
        }

        private Client EnsureClient()
        {
            if (_client != null)
                return _client;

            var config = new Config
            {
                AccessKeyId = _smsSettings.AccessKeyId,
                AccessKeySecret = _smsSettings.AccessKeySecret,
                Endpoint = "dysmsapi.aliyuncs.com",
                ConnectTimeout = 5000,
                ReadTimeout = 10000
            };

            _client = new Client(config);
            return _client;
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

                var client = EnsureClient();
                var request = new SendSmsRequest
                {
                    PhoneNumbers = number,
                    SignName = _smsSettings.SignName,
                    TemplateCode = _smsSettings.TemplateCode,
                    TemplateParam = $"{{\"code\":\"{code}\"}}"
                };

                var response = await client.SendSmsAsync(request);

                Logger.Info($"[阿里云短信] 发送结果 - Code: {response.Body.Code}, Message: {response.Body.Message}, BizId: {response.Body.BizId}");

                if (response.Body.Code != "OK")
                {
                    throw new UserFriendlyException($"短信发送失败: {response.Body.Message}");
                }

                Logger.Info($"[阿里云短信] 短信发送成功 - 手机号: {number}, 验证码: {code}");
            }
            catch (TeaException ex)
            {
                Logger.Error($"[阿里云短信] SDK 异常: {ex.Message}, Code: {ex.Code}");
                throw new UserFriendlyException($"短信发送异常: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[阿里云短信] 发送异常: {ex.Message}", ex);
                throw new UserFriendlyException($"短信发送异常: {ex.Message}");
            }
        }
    }

    public class SmsSettings
    {
        public string AccessKeyId { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_ACCESSKEYID") ?? "";

        public string AccessKeySecret { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_ACCESSKEYSECRET") ?? "";

        public string SignName { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_SIGNNAME") ?? "黑龙江省魔淡网络科技";

        public string TemplateCode { get; set; } =
            Environment.GetEnvironmentVariable("ALIYUN_SMS_TEMPLATE_CODE") ?? "SMS_506845124";
    }
}
