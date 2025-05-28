using System.Threading.Tasks;
using Abp.Dependency;
using TtWork.Project.Authentication.External;

namespace TtWork.Project.Web.Authentication.External {
    public abstract class ExternalAuthProviderApi : ITransientDependency {
        public ExternalLoginProviderInfo ProviderInfo { get; set; }

        public void Initialize(ExternalLoginProviderInfo providerInfo) {
            ProviderInfo = providerInfo;
        }

        public async Task<bool> IsValidUser(string userId, string accessCode, string appid, string appsec) {
            var userInfo = await GetUserInfo(accessCode, appid, appsec);
            return userInfo.ProviderKey == userId;
        }

        public abstract Task<ExternalAuthUserInfo> GetUserInfo(string accessCode, string appid, string appsec);
    }
}