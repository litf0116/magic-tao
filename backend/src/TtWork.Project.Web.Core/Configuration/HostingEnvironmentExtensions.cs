using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TtWork.Project.Core.Configuration;

namespace TtWork.Project.Web.Configuration {
    public static class HostingEnvironmentExtensions {
        public static IConfigurationRoot GetAppConfiguration(this IWebHostEnvironment env) {
            return AppConfigurations.Get(env.ContentRootPath, env.EnvironmentName, env.IsDevelopment());
        }
    }
}