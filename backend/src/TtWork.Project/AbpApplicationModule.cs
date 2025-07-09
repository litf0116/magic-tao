using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TtWork.Abp.AppManagement;
using TtWork.Abp.Extensions;
using TtWork.Abp.Oss.UpYun;
using TtWork.Project.Core;
using TtWork.Project.Definitions;
using TtWork.Project.Localization;
using TtWork.Project.Services;
using TtWork.Project.Services.Cache;

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
            
            // 手动注册拍卖品缓存服务接口
            IocManager.Register<IAuctionItemCacheService, AuctionItemCacheManager>(
                DependencyLifeStyle.Transient);

            // 注册出价资格检查服务
            IocManager.Register<IBidEligibilityService, BidEligibilityService>(
                DependencyLifeStyle.Transient);
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