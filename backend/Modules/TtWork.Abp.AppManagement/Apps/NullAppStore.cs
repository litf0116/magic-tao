using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TtWork.Abp.AppManagement.Apps
{
    public class NullAppStore : IAppStore, ISingletonDependency
    {
        public ILogger<NullAppStore> Logger { get; set; }

        public NullAppStore()
        {
            Logger = NullLogger<NullAppStore>.Instance;
        }

        public Task<Dictionary<string, string>> GetOrNullAsync(string name, string providerName, string providerKey)
        {
            return Task.FromResult<Dictionary<string, string>>(null);
        }
    }
}