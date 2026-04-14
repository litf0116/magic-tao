using Abp.Dependency;
using Abp.Hangfire;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Caching.Redis;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TtWork.Abp;
using TtWork.Project.Web.Core;
using TtWork.Project.Authentication.External;
using TtWork.Project.Configuration;
using TtWork.Project.Web.Authentication.External;
using TTWork.WeiXinMiddleware;
using TtWork.Project.Web.Host.HealthChecks;

namespace TtWork.Project.Web.Host {
    [DependsOn(typeof(ProjectWebCoreModule)
        // , typeof(AbpRedisCacheModule)
    )]
    public class ProjectWebHostModule(IConfiguration appConfiguration) : AbpModule {
        public override void PreInitialize() {
            // Configuration.Caching.UseRedis(options => {
            //     options.ConnectionString = appConfiguration["Redis:ConnectionString"];
            //     options.DatabaseId = appConfiguration.GetValue<int>("Redis:DatabaseId");
            // });

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectWebHostModule).GetAssembly());
            
            // 注册健康检查服务
            IocManager.Register<DatabaseHealthCheck>(DependencyLifeStyle.Transient);
            IocManager.Register<RedisHealthCheck>(DependencyLifeStyle.Transient);
            
            ConfigureTokenAuth();
        }

        private void ConfigureTokenAuth() {
            var externalAuthConfig = IocManager.Resolve<ExternalAuthConfiguration>();

            externalAuthConfig.Providers.Add(new ExternalLoginProviderInfo(
                Consts.LoginProvider.WeChatPub,
                "", //set in tenantSetting
                "", //set in tenantSetting
                typeof(WechatPubAuthProviderApi)));


            externalAuthConfig.Providers.Add(new ExternalLoginProviderInfo(
                Consts.LoginProvider.WeChatMiniOpenid,
                "", //set in tenantSetting
                "", //set in tenantSetting
                typeof(WechatMiniOpenidProviderApi)));

            
            externalAuthConfig.Providers.Add(new ExternalLoginProviderInfo(
                Consts.LoginProvider.WeChatPubOpenid,
                "", //set in tenantSetting
                "", //set in tenantSetting
                typeof(WechatPubOpenidProviderApi)));
        }
    }
}