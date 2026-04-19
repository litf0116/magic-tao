using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Dependency;
using Castle.Core.Logging;
using TtWork.Abp.Core.Net.Sms;

namespace TtWork.Project.Net.Sms
{
    public class SmsSender : ISmsSender, ITransientDependency
    {
        public ILogger Logger { get; set; }

        public SmsSender()
        {
            Logger = NullLogger.Instance;
        }

        public Task SendAsync(string number, string message)
        {
            var codeMatch = Regex.Match(message, @"(\d{6})");
            if (codeMatch.Success)
            {
                var code = codeMatch.Groups[1].Value;
                Logger.Info($"[测试模式] 短信验证码已发送");
                Logger.Info($"[测试模式] 手机号: {number}");
                Logger.Info($"[测试模式] 验证码: {code} (5分钟内有效)");
                Logger.Info($"[测试模式] 完整消息: {message}");
            }
            else
            {
                Logger.Info($"[测试模式] 短信已发送");
                Logger.Info($"[测试模式] 手机号: {number}");
                Logger.Info($"[测试模式] 消息内容: {message}");
            }

            return Task.FromResult(0);
        }
    }
}