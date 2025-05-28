using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TtWork.Abp.Core.Oss;
using TtWork.Project;

namespace TtWork.Abp.Oss.UpYun {
    public class UpYunModule : AbpModule {
        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(GetType().GetAssembly());

            IocManager.Register<IOssClient, UpYunClient>(DependencyLifeStyle.Singleton);

            Configuration.Settings.Providers.Add<UpYunOssSettingProvider>();
        }
    }
}