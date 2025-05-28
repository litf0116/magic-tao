using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Shouldly;
using TTWork.Abp.AuditManagement.Applications;
using TTWork.Abp.AuditManagement.Applications.Dtos;
using TTWork.Abp.AuditManagement.Domain;
using TtWork.Project.Application.Definitions;
using Xunit;

namespace TtWork.SoMall.Tests.Modules
{
    public class AuditFlowTests : AbpTestBase
    {
        private IAuditFlowAppService _service;
        private IRepository<AuditFlow, Guid> _auditFlowRepository;

        public AuditFlowTests()
        {
            _auditFlowRepository = Resolve<IRepository<AuditFlow, Guid>>();
            _service = Resolve<IAuditFlowAppService>();
        }

        [Fact]
        public async Task GetForEdit_Must_Return_DefineAuditList()
        {
            AbpSession.TenantId = 1;
            AbpSession.UserId = 2;

            var editDto = await _service.GetForEdit(new EntityDto<Guid>() {Id = new Guid()});

            var auditList = editDto.Schema["auditDefinitions"] as JArray;
            auditList?.Count.ShouldBe(5);

            editDto.Data.AuditName = auditList?.First["value"].ToString();
            editDto.Data.AuditName = OrganizationAudit.ApplyAprove;
            editDto.Data.Enable = true;
            editDto.Data.ProviderName = "G";
            editDto.Data.AuditNodes = new List<AuditNodeCreateOrEditDto>()
            {
                new AuditNodeCreateOrEditDto()
                {
                    Index = 0,
                    UserName = "admin",
                    UserId = 2
                },
                new AuditNodeCreateOrEditDto()
                {
                    Index = 0,
                    UserName = "tt",
                    UserId = 3
                },
                new AuditNodeCreateOrEditDto()
                {
                    Index = 1,
                    UserName = "admin",
                    UserId = 2
                }
            };

            await _service.CreateAsync(editDto.Data);


            await UsingDbContextAsync(async context =>
            {
                var firstDto = await context.AuditFlows.Include(x => x.AuditNodes).FirstOrDefaultAsync();
                firstDto.ProviderName.ShouldBe("G");
                firstDto.AuditName.ShouldBe(OrganizationAudit.ApplyAprove);
                firstDto.AuditNodes.Count.ShouldBe(3);
                firstDto.NodesMaxIndex = 1;
            });
        }
    }
}