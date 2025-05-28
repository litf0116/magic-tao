using Xunit;

namespace TtWork.SoMall.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute()
        {
            if (!ProjectNameConsts.MultiTenancyEnabled)
            {
                //Skip = "MultiTenancy is disabled.";
            }
        }
    }
}
