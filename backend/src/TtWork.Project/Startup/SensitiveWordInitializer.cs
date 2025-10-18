using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Runtime.Session;
using MediatR;
using TtWork.Project.Events;

namespace TtWork.Project.Startup
{
    /// <summary>
    /// 违禁词缓存初始化器
    /// 在系统启动时自动初始化违禁词缓存
    /// </summary>
    public class SensitiveWordInitializer : ITransientDependency
    {
        private readonly ILogger<SensitiveWordInitializer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEventBus _eventBus;

        public SensitiveWordInitializer(
            ILogger<SensitiveWordInitializer> logger,
            IServiceScopeFactory serviceScopeFactory,
            IEventBus eventBus)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _eventBus = eventBus;
        }

        /// <summary>
        /// 初始化违禁词缓存
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("开始初始化违禁词缓存...");

            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                // 强制重建缓存
                var words = await mediator.Send(new QueryCacheWords(true));

                _logger.LogInformation("🎉 违禁词缓存初始化完成，共加载 {Count} 个违禁词", words.Length);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "违禁词缓存初始化失败");
                throw;
            }
        }
    }

    }