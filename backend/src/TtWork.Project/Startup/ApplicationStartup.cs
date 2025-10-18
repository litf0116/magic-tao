using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection;
using System.Reflection;
using TtWork.Project.Core;
using TtWork.Project.Startup;

namespace TtWork.Project
{
    /// <summary>
    /// 应用启动器
    /// 在应用启动时自动初始化违禁词缓存
    /// </summary>
    [DependsOn(typeof(ProjectCoreModule))]
    public class ApplicationStartup : AbpModule
    {
        private readonly ILogger<ApplicationStartup> _logger;

        public ApplicationStartup(ILogger<ApplicationStartup> logger)
        {
            _logger = logger;
        }

        public override void PreInitialize()
        {
            // AutoMapper配置
            // Configuration.Modules.AbpAutoMapper().Configurators.Add(mapper =>
            // {
            //     // AutoMapper配置
            // });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ApplicationStartup).Assembly);
        }

        public override void PostInitialize()
        {
            // 在应用启动完成后初始化违禁词缓存
            Task.Run(async () =>
            {
                await Task.Delay(5000); // 等待5秒确保所有服务都已启动

                try
                {
                    using var scope = IocManager.CreateScope();
                    var initializer = scope.Resolve<SensitiveWordInitializer>();
                    await initializer.InitializeAsync();

                    _logger.LogInformation("🎉 违禁词缓存自动初始化成功");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "❌ 违禁词缓存自动初始化失败");
                }
            });
        }

        public override void Shutdown()
        {
            _logger.LogInformation("应用正在关闭...");
            base.Shutdown();
        }
    }
}