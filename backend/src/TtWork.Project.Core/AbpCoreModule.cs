using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using TtWork.Abp;
using TtWork.Abp.AppManagement;
using TtWork.Abp.Extensions;
using TtWork.Project.Core.Localization;
using TtWork.Project.Timing;

namespace TtWork.Project.Core {
    [DependsOn(
        // typeof(AbpZeroCoreModule),
        typeof(AppManagementModule),
        typeof(TtWorkAbpCoreModule)
    )]
    public class ProjectCoreModule : AbpModule {
        public override void PreInitialize() {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            // Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            // Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            // Configuration.Modules.Zero().EntityTypes.User = typeof(User);
            // Configure roles
            // AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);


            // Enable this line to create a multi-tenant application.
            // 多租户开启状态
            Configuration.MultiTenancy.IsEnabled = CoreConsts.MultiTenancyEnabled;
            
            Configuration.MultiTenancy.TenantIdResolveKey =
                "Abp.Tenantid"; //兼容处理。Axios Http2.0 header会把 Abp.TenantId 转成 Abp.Tenantid

            TtWorkCoreLocalizationConfigurer.Configure(Configuration.Localization);
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectCoreModule).GetAssembly());
            IocManager.RegisterMediatRAssembly<ProjectCoreModule>();
        }

        public override void PostInitialize() {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}