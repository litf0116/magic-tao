using System.Globalization;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.FluentValidation;
using Abp.Modules;
using Abp.Reflection.Extensions;
using FluentValidation;
using TtWork.Abp.Core.Extensions;
using TtWork.Abp.Definitions;
using TtWork.Abp.Extensions;
using TtWork.Abp.Localization;
using TtWork.Abp.Organizations;

namespace TtWork.Abp {
    /// <summary>
    /// 
    /// </summary>
    [DependsOn(
        typeof(AbpFluentValidationModule),
        typeof(AbpAutoMapperModule)
    )]
    // ReSharper disable once InconsistentNaming
    public class TtWorkAbpCoreModule : AbpModule {
        /// <summary>
        /// 
        /// </summary>
        public override void PreInitialize() {
            Configuration.Authorization.Providers.Add<AbpAuthorizationProvider>();

            if (!IocManager.IsRegistered<ICurrentOrganizationAccessor>()) {
                IocManager.Register<ICurrentOrganizationAccessor, AsyncLocalCurrentShopAccessor>(DependencyLifeStyle
                    .Singleton);
            }

            TtWorkLocalizationConfigurer.Configure(Configuration.Localization);
        }

        public override void PostInitialize() {
            // ValidatorOptions.Global.LanguageManager.Enabled = true;
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("zh-CN");
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(TtWorkAbpCoreModule).GetAssembly());
            IocManager.RegisterMediatRAssembly<TtWorkAbpCoreModule>();
        }
    }
}