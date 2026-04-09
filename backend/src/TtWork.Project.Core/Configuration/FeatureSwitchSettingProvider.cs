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
                AppSettings.FeatureSwitch.ShowAuctionMaxVersionMpWeixin,
                "",
                scopes: SettingScopes.Application,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                AppSettings.FeatureSwitch.ShowTradingPostMaxVersionMpWeixin,
                "",
                scopes: SettingScopes.Application,
                isVisibleToClients: true
            )
        };
    }
}
