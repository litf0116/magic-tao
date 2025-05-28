using System.Collections.Generic;
using Abp.Dependency;

namespace TtWork.Project.Authentication.External {
    public class ExternalAuthConfiguration : ISingletonDependency {
        public List<ExternalLoginProviderInfo> Providers { get; } = new();
    }
}