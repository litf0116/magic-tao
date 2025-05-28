using System;
using Abp;
using Abp.Dependency;
using TtWork.Abp.Organizations;

namespace TtWork.Abp.Core.Organizations
{
    public class CurrentOrganization : ICurrentOrganization, ITransientDependency
    {
        public virtual bool IsAvailable => Id.HasValue;
        public virtual long? Id => _currentShopAccessor.Current?.OrganizationId;

        public string DisplayName => _currentShopAccessor.Current?.DisplayName;

        private readonly ICurrentOrganizationAccessor _currentShopAccessor;

        public CurrentOrganization(ICurrentOrganizationAccessor currentShopAccessor)
        {
            _currentShopAccessor = currentShopAccessor;
        }

        public IDisposable Change(long? id, string name = null)
        {
            return SetCurrent(id, name);
        }

        private IDisposable SetCurrent(long? ouId, string name = null)
        {
            var parentScope = _currentShopAccessor.Current;
            _currentShopAccessor.Current = new BasicOrganizationInfo(ouId, name);
            return new DisposeAction(() => { _currentShopAccessor.Current = parentScope; });
        }
    }
}