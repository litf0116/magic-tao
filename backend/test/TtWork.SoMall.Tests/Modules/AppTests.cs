using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TTWork.Abp.AppManagement.Applications.TT.Abp.AppManagement.Application;
using TTWork.Abp.AppManagement.Apps;
using TTWork.Abp.AppManagement.Domain;
using TTWork.Definitions;
using Xunit;

namespace TtWork.SoMall.Tests.Modules
{
        public class AppTests : AbpTestBase
    {
        private IAppDefinitionManager _appDefinitionManager;
        private IAppValueProviderManager _appValueProvider;
        private IRepository<App, Guid> _appRepository;

        private IAppProvider _appProvider;

        private IAppAppService _service;

        // private IAuditManagementAppService _appService;

        public AppTests()
        {
            _appDefinitionManager = Resolve<IAppDefinitionManager>();
            _appValueProvider = Resolve<IAppValueProviderManager>();
            _appRepository = Resolve<IRepository<App, Guid>>();
            _appProvider = Resolve<IAppProvider>();

            _service = Resolve<IAppAppService>();


            // _appService = Resolve<IAuditManagementAppService>();
        }

        [Fact]
        public async Task DefinitionTest()
        {
            var appDefinitions = _appDefinitionManager.GetAll();

            // define in mall Module
            appDefinitions.Count.ShouldBe(1);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task ValueProviderTest()
        {
            var providers = _appValueProvider.Providers;

            // define in mall Module
            providers.Count.ShouldBe(4);

            await Task.CompletedTask;
        }


        [Fact]
        public async Task GetForEdit_Must_Return_DefineAuditList()
        {
            AbpSession.TenantId = 1;
            AbpSession.UserId = 2;

            var appName = ProjectApp.WechatWork;
            
            await _service.CreateAsync(new AppCreateOrUpdateDto()
            {
                Name = appName,
                ClientName = ProjectApp.WechatWork,
                ProviderName = "T",
                ProviderKey = "1",
                Value = new Dictionary<string, string>()
                {
                    { "appid", "123" },
                    { "appsec", "321" }
                }
            });

            await UsingDbContextAsync(async context =>
            {
                var firstDto = await context.Apps.FirstOrDefaultAsync();
                firstDto.ProviderName.ShouldBe("T");
                firstDto.Name.ShouldBe(appName);
                firstDto.ProviderName.ShouldBe("T");
                firstDto.ProviderKey.ShouldBe("1");
            });

            var p = await _appProvider.GetOrNullAsync(appName);
            p["appid"].ShouldBe("123");
            p["appsec"].ShouldBe("321");
        }
    }
}