using TtWork.Abp.Core.Organizations;

namespace TtWork.Abp.Organizations
{
    public interface ICurrentOrganizationAccessor
    {
        BasicOrganizationInfo Current { get; set; }
    }
}