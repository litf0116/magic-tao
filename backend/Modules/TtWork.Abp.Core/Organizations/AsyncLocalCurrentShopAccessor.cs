using System.Threading;
using Abp.Dependency;
using TtWork.Abp.Core.Organizations;

namespace TtWork.Abp.Organizations {
    public class AsyncLocalCurrentShopAccessor : ICurrentOrganizationAccessor, ISingletonDependency {
        public BasicOrganizationInfo Current {
            get => _currentScope.Value;
            set => _currentScope.Value = value;
        }

        private readonly AsyncLocal<BasicOrganizationInfo> _currentScope;

        public AsyncLocalCurrentShopAccessor() {
            _currentScope = new AsyncLocal<BasicOrganizationInfo>();
        }
    }
}