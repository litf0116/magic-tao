using System;
using System.Threading.Tasks;
using Abp;
using Abp.Configuration;
using Abp.Extensions;
using JetBrains.Annotations;

namespace TtWork.Abp.AppManagement.Apps
{
    public static class AppProviderExtensions
    {
        public static async Task<T> GetAsync<T>([NotNull] this ISettingManager settingProvider, [NotNull] string name, T defaultValue = default)
            where T : struct
        {
            Check.NotNull(settingProvider, nameof(settingProvider));
            Check.NotNull(name, nameof(name));

            var value = await settingProvider.GetSettingValueAsync(name);
            return value?.To<T>() ?? defaultValue;
        }
    }
}