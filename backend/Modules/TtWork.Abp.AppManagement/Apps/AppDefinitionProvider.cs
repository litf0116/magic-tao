using Abp.Dependency;

namespace TtWork.Abp.AppManagement.Apps
{
    public abstract class AppDefinitionProvider : IAppDefinitionProvider, ITransientDependency
    {
        public abstract void Define(IAppDefinitionContext context);
    }
}