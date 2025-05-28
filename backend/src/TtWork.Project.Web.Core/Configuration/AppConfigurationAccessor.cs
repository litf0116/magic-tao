using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using TtWork.Abp.Configuration;

namespace TtWork.Project.Configuration {
    public class AppConfigurationAccessor(IWebHostEnvironment env) : IAppConfigurationAccessor, ISingletonDependency {
        public IConfigurationRoot Configuration { get; } //= env.GetAppConfiguration();
    }
}