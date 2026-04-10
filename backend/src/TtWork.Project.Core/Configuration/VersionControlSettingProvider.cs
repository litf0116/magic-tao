using System.Collections.Generic;
using Abp.Configuration;

namespace TtWork.Project.Core.Configuration
{
    /// <summary>
    /// 版本控制设置定义提供者
    /// </summary>
    public class VersionControlSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
            {
                new SettingDefinition(
                    AppSettings.VersionControl.LatestStableVersion,
                    "20260410@1.2.1",
                    scopes: SettingScopes.Application,
                    isVisibleToClients: false
                )
            };
        }
    }
}