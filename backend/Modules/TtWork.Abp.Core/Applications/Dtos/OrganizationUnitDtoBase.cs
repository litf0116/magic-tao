using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Organizations;

namespace TtWork.Abp.Applications.Dtos
{
    [AutoMapFrom(typeof(OrganizationUnit))]
    public class OrganizationUnitDtoBase : EntityDto<long>
    {
        public string DisplayName { get; set; }
    }
}