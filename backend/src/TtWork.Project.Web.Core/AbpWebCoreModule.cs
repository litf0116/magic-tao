using System;
using System.Text;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Extensions;
using Abp.Hangfire;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TtWork.Abp.AppManagement;
using TtWork.Abp.Extensions;
using TtWork.Project.Authentication.JwtBearer;
using TtWork.Project.Core;
using TtWork.Project;

namespace TtWork.Project.Web {
    [DependsOn(
        typeof(AbpAspNetCoreModule),
        typeof(ProjectCoreModule), // 确保 ProjectCoreModule 存在并被正确引用
        typeof(AbpHangfireAspNetCoreModule),
        typeof(ProjectEntityFrameworkModule),
        typeof(AbpApplicationModule) // 添加此行
    )]
    public class ProjectWebCoreModule(IWebHostEnvironment env, IConfiguration configuration) : AbpModule {
        public override void PreInitialize() {
            Clock.Provider = ClockProviders.Local;
            // Clock.Provider = ClockProviders.Utc;

            Configuration.DefaultNameOrConnectionString = configuration.GetConnectionString(
                CoreConsts.ConnectionStringName
            );

            // 扫描 AppManagement 程序集的应用服务
            Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(typeof(AppManagementModule).Assembly,
                moduleName: "AppManagement", useConventionalHttpVerbs: true);

            // 扫描 TtWork.Project.Web.Core 程序集的传统 MVC 控制器 (如 QrCodeAuthController, TokenAuthController)
            Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(
                typeof(ProjectWebCoreModule).Assembly,
                moduleName: "Project",
                useConventionalHttpVerbs: true);

            // 使用netcore AddNewtonsoftJson中的时间格式
            Configuration.Modules.AbpAspNetCore().UseMvcDateTimeFormatForAppServices = true;

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            ConfigureTokenAuth();
        }

        private void ConfigureTokenAuth() {
            IocManager.Register<TokenAuthConfiguration>();

            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey =
                new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(configuration["Authentication:JwtBearer:SecurityKey"] ??
                                            new Guid().ToString().ToMd5())
                );
            tokenAuthConfig.Issuer = configuration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = configuration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials =
                new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.Expiration =
                TimeSpan.FromDays(Convert.ToDouble(configuration["Authentication:JwtBearer:Day"]));
            tokenAuthConfig.AccessTokenExpiration =
                TimeSpan.FromDays(int.Parse(configuration["Authentication:JwtBearer:Day"] ?? "7"));

            tokenAuthConfig.RefreshTokenExpiration = AppConsts.RefreshTokenExpiration;
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectWebCoreModule).GetAssembly());
            IocManager.RegisterMediatRAssembly<ProjectWebCoreModule>();
        }
    }
}