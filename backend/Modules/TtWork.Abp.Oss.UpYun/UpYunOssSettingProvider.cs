using System.Collections.Generic;
using Abp.Configuration;
using Microsoft.Extensions.Configuration;
using TtWork.Abp.Configuration;

namespace TtWork.Abp.Oss.UpYun {
    public class UpYunOssSettingProvider(IConfiguration configuration) : SettingProvider {
        // private readonly IConfigurationRoot _appConfiguration;
        //
        // public UpYunOssSettingProvider(IConfiguration configuration) {
        //     _appConfiguration = appConfiguration.Configuration;
        // }

        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context) {
            return new[] {
                new SettingDefinition(OssSetting.Upyun.BucketName,
                    GetFromAppSettings(OssSetting.Upyun.BucketName, ""),
                    scopes: SettingScopes.All, isVisibleToClients: true),

                new SettingDefinition(OssSetting.Upyun.UserName,
                    GetFromAppSettings(OssSetting.Upyun.UserName, ""),
                    scopes: SettingScopes.All,
                    isVisibleToClients: true),

                new SettingDefinition(OssSetting.Upyun.Password,
                    GetFromAppSettings(OssSetting.Upyun.Password, ""),
                    scopes: SettingScopes.All, isVisibleToClients: false
                ),

                new SettingDefinition(OssSetting.Upyun.DomainHost,
                    GetFromAppSettings(OssSetting.Upyun.DomainHost, ""),
                    scopes: SettingScopes.All, isVisibleToClients: true),
            };
        }

        private string GetFromAppSettings(string name, string defaultValue = null) {
            return GetFromSettings("App:" + name, defaultValue);
        }

        private string GetFromSettings(string name, string defaultValue = null) {
            return configuration[name] ?? defaultValue;
        }
    }
}