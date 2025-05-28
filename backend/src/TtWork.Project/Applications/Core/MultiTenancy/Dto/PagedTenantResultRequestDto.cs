using Abp.Application.Services.Dto;

namespace TtWork.Project.MultiTenancy.Dto {
    public class PagedTenantResultRequestDto : PagedResultRequestDto {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}