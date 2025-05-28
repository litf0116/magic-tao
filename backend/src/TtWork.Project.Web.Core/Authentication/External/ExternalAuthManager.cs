using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using TtWork.Project.Authentication.External;

namespace TtWork.Project.Web.Authentication.External {
    public class ExternalAuthManager(
        IIocResolver iocResolver,
        ExternalAuthConfiguration externalAuthConfiguration
    )
        : ITransientDependency {
        public Task<ExternalAuthUserInfo> GetUserInfo(string provider, string accessCode, string appid, string appsec) {
            using var providerApi = CreateProviderApi(provider);
            return providerApi.Object.GetUserInfo(accessCode, appid, appsec);
        }

        private IDisposableDependencyObjectWrapper<ExternalAuthProviderApi> CreateProviderApi(string provider) {
            var providerInfo = externalAuthConfiguration.Providers.FirstOrDefault(p => p.Name == provider);
            if (providerInfo == null) {
                throw new Exception("Unknown external auth provider: " + provider);
            }

            var providerApi = iocResolver.ResolveAsDisposable<ExternalAuthProviderApi>(providerInfo.ProviderApiType);
            providerApi.Object.Initialize(providerInfo);
            return providerApi;
        }
    }
}