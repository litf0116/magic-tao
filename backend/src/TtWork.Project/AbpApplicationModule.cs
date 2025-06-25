using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TtWork.Abp.AppManagement;
using TtWork.Abp.Extensions;
using TtWork.Abp.Oss.UpYun;
using TtWork.Project.Core;
using TtWork.Project.Definitions;
using TtWork.Project.Localization;

namespace TtWork.Project {
    [DependsOn(
        typeof(ProjectCoreModule),
        typeof(UpYunModule),
        typeof(AbpAutoMapperModule),
        typeof(AbpAspNetCoreSignalRModule)
    )]
    public class AbpApplicationModule : AbpModule {
        public override void PreInitialize() {
            Configuration.Authorization.Providers.Add<ProjectNameAuthorizationProvider>();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);

            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(typeof(AbpApplicationModule).GetAssembly());

            ProjectLocalizationConfigurer.Configure(Configuration.Localization);
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(AbpApplicationModule).GetAssembly());
            IocManager.RegisterMediatRAssembly<AbpApplicationModule>();
            
            // 缓存服务会自动注册，因为实现了ITransientDependency
            // 事件处理器会自动注册，因为实现了ITransientDependency
        }

        public override void PostInitialize() {
            // Audit模块确保注入
            // Configuration.Modules.AuditModule().DefinitionProviders.AddIfNotContains(
            //     typeof(ProjectNameAuditDefinitionProvider));

            // App模块确保注入
            Configuration.Modules.AppModule().DefinitionProviders.AddIfNotContains(
                typeof(ProjectAppProvider));
        }
    }
}