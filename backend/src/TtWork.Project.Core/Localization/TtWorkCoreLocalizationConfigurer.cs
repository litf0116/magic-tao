using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Json;
using TtWork.Project;

namespace TtWork.Project.Core.Localization
{
    public static class TtWorkCoreLocalizationConfigurer {
        public static void Configure(ILocalizationConfiguration localizationConfiguration) {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(
                    CoreConsts.LocalizationSourceName,
                    new JsonEmbeddedFileLocalizationDictionaryProvider(
                        Assembly.GetExecutingAssembly(),
                        "TtWork.Project.Core.Localization.JsonSources"
                    )
                )
            );
        }
    }
}