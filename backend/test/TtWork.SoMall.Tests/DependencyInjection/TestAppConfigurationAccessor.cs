using Abp.Dependency;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;
using TtWork.SoMall.Configuration;

namespace TtWork.SoMall.Tests.DependencyInjection
{
    public class TestAppConfigurationAccessor : IAppConfigurationAccessor, ISingletonDependency
    {
        public IConfigurationRoot Configuration { get; }

        public TestAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(
                typeof(AbpTestModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }
    }
}