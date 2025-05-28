using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.MultiTenancy;

namespace TtWork.Project.Applications.Sessions.Dto {
    [AutoMapFrom(typeof(TenantCacheItem))]
    public class TenantLoginInfoDto : EntityDto {
        public string TenancyName { get; set; }

        public string Name { get; set; }
    }
}