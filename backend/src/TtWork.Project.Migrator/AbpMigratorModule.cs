using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using TtWork.Project.Core;
using TtWork.Project.EntityFrameworkCore;
using TtWork.Project.Core.Configuration;
using TtWork.Project.Migrator.DependencyInjection;

namespace TtWork.Project.Migrator
{
    [DependsOn(typeof(ProjectEntityFrameworkModule))]
    public class AbpMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public AbpMigratorModule(ProjectEntityFrameworkModule projectProjectNameEntityFrameworkModule)
        {
            projectProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(AbpMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                CoreConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus),
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
