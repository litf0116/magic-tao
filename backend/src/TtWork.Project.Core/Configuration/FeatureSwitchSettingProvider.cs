using System.Collections.Generic;
using Abp.Configuration;

namespace TtWork.Project.Core.Configuration;

public class FeatureSwitchSettingProvider : SettingProvider
{
    public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
    {
        return new[]
        {
            new SettingDefinition(
                AppSettings.FeatureSwitch.ReviewVersionMpWeixin,
                "",
                scopes: SettingScopes.Application,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                AppSettings.FeatureSwitch.ReviewVersionAppPlus,
                "",
                scopes: SettingScopes.Application,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                AppSettings.FeatureSwitch.ReviewVersionH5,
                "",
                scopes: SettingScopes.Application,
                isVisibleToClients: true
            )
        };
    }
}
