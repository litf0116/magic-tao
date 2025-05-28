
using Abp.Collections;

namespace TtWork.Abp.AppManagement.Apps
{
    public class AppOptions
    {
        public ITypeList<IAppDefinitionProvider> DefinitionProviders { get; }

        public ITypeList<IAppValueProvider> ValueProviders { get; }

        public AppOptions()
        {
            DefinitionProviders = new TypeList<IAppDefinitionProvider>();
            ValueProviders = new TypeList<IAppValueProvider>();
        }
    }
}