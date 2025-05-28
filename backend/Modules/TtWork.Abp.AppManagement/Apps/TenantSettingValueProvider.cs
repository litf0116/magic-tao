using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Runtime.Session;

namespace TtWork.Abp.AppManagement.Apps
{
    public class TenantAppValueProvider : AppValueProvider
    {
        private readonly IAbpSession _session;
        public const string ProviderName = "T";
        public override string Name => ProviderName;


        public TenantAppValueProvider(IAppStore appStore, IAbpSession session)
            : base(appStore)
        {
            _session = session;
        }

        public override async Task<Dictionary<string, string>> GetOrNullAsync(AppDefinition setting)
        {
            return await AppStore.GetOrNullAsync(setting.Name, Name, _session.TenantId?.ToString());
        }
    }
}