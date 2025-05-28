using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Json;

namespace TtWork.Project.Localization {
    public static class ProjectLocalizationConfigurer {
        public static void Configure(ILocalizationConfiguration localizationConfiguration) {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(
                    AppConsts.LocalizationSourceName,
                    new JsonEmbeddedFileLocalizationDictionaryProvider(
                        Assembly.GetExecutingAssembly(),
                        $"TtWork.Project.Localization.JsonSources"
                    )
                )
            );
        }
    }
}