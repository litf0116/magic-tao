using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Core.Editions;

namespace TtWork.Abp.Core.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore)
        {
        }
    }
}
