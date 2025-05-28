using Abp.Modules;
using Abp.Reflection.Extensions;

namespace TtWork.ProjectName.Core.Share
{
    public class ProjectNameAbpCoreSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectNameAbpCoreSharedModule).GetAssembly());
        }
    }
}