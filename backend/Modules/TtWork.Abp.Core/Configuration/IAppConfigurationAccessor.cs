using Microsoft.Extensions.Configuration;

namespace TtWork.Abp.Configuration {
    public interface IAppConfigurationAccessor {
        IConfigurationRoot Configuration { get; }
    }
}