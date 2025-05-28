using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AutoMapper;
using TtWork.Abp.AppManagement.Applications.TT.Abp.AppManagement.Application;
using TtWork.Abp.AppManagement.Apps;
using TtWork.Abp.AppManagement.Domain;
using TtWork.Abp.Extensions;

namespace TtWork.Abp.AppManagement
{
    [DependsOn(
        typeof(AbpAutoMapperModule)
    )]
    public class AppManagementModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.Register<AppOptions>();
            Configuration.Modules.AppModule().ValueProviders.Add<DefaultValueAppValueProvider>();
            Configuration.Modules.AppModule().ValueProviders.Add<ConfigurationAppValueProvider>();
            Configuration.Modules.AppModule().ValueProviders.Add<GlobalAppValueProvider>();
            Configuration.Modules.AppModule().ValueProviders.Add<TenantAppValueProvider>();

            // this line move to Web.Core , then test will fine
            // Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(typeof(AppManagementModule).Assembly, moduleName: "AppManagement", useConventionalHttpVerbs: true);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(AppManagementMapper.CreateMappings);
        }


        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AppManagementModule).GetAssembly());
            IocManager.RegisterMediatRAssembly<AppManagementModule>();
        }
    }

    public class AppManagementMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<App, AppCreateOrUpdateDto>()
                .ReverseMap();

            configuration.CreateMap<App, AppDto>();
        }
    }

    public static class AppConfigurationExtensions
    {
        public static AppOptions AppModule(this IModuleConfigurations moduleConfigurations)
        {
            return moduleConfigurations.AbpConfiguration.Get<AppOptions>();
        }
    }
}