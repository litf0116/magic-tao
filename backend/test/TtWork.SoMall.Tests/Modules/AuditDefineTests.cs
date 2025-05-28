using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TTWork.Abp.AuditManagement.Audits;
using TTWork.Abp.AuditManagement.Domain;
using TtWork.Project.Application.Definitions;
using Xunit;

namespace TtWork.SoMall.Tests.Modules
{
    public class AuditTests : AbpTestBase
    {
        private IAuditDefinitionManager _auditDefinitionManager;
        private IAuditValueProviderManager _auditValueProvider;
        private IRepository<AuditFlow, Guid> _auditFlowRepository;

        private IAuditProvider _auditProvider;

        // private IAuditManagementAppService _appService;

        public AuditTests()
        {
            _auditDefinitionManager = Resolve<IAuditDefinitionManager>();
            _auditValueProvider = Resolve<IAuditValueProviderManager>();
            _auditFlowRepository = Resolve<IRepository<AuditFlow, Guid>>();
            _auditProvider = Resolve<IAuditProvider>();
            // _appService = Resolve<IAuditManagementAppService>();
        }

        [Fact]
        public async Task DefinitionTest()
        {
            var audits = _auditDefinitionManager.GetAll();

            // define in mall Module
            audits.Count.ShouldBe(5);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task ValueProviderTest()
        {
            var providers = _auditValueProvider.Providers;

            // define in mall Module
            providers.Count.ShouldBe(3);

            await Task.CompletedTask;
        }


        [Fact]
        public async Task AuditFlowTests()
        {
            AbpSession.UserId = 2;
            AbpSession.TenantId = 1;


            var gEntity = new AuditFlow(
                OrganizationAudit.ApplyAprove,
                true,
                "G",
                null);

            var tEntity = new AuditFlow(
                OrganizationAudit.ApplyAprove,
                true,
                "T",
                "1");


            await UsingDbContextAsync(async context =>
            {
                await context.AuditFlows.AddAsync(gEntity);

                await context.AuditFlows.AddAsync(tEntity);

                // await context.SaveChangesAsync();

                var list = await context.AuditFlows.ToListAsync();

                list.Count.ShouldBe(2);
            });


            //Act
            var result = await _auditProvider.GetOrNullAsync(OrganizationAudit.ApplyAprove);

            //Assert
            gEntity.Enable.ShouldBe(true);
            gEntity.ProviderName.ShouldBe("G");
            gEntity.AuditName.ShouldBe(OrganizationAudit.ApplyAprove);

            result.ShouldNotBeNull();
            result.ShouldBe(tEntity.Id);

            await Task.CompletedTask;
        }
    }
}