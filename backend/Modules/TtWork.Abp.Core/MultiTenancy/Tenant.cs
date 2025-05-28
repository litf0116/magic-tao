using Abp.MultiTenancy;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Core.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
