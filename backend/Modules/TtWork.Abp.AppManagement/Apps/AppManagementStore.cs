using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Runtime.Caching;
using TtWork.Abp.AppManagement.Domain;

namespace TtWork.Abp.AppManagement.Apps
{
    public class AppManagementStore : IAppManagementStore, ITransientDependency
    {
        private readonly ICacheManager _cacheManager;

        protected IAppRepository AppRepository { get; }
        protected IGuidGenerator GuidGenerator { get; }

        public AppManagementStore(
            ICacheManager cacheManager,
            IAppRepository appRepository,
            IGuidGenerator guidGenerator
        )
        {
            _cacheManager = cacheManager;
            AppRepository = appRepository;
            GuidGenerator = guidGenerator;
        }

        public virtual async Task<Dictionary<string, string>> GetOrNullAsync(string name, string providerName, string providerKey)
        {
            var cacheItem = await GetCacheItemAsync(name, providerName, providerKey);
            return cacheItem.Value;
        }

        public virtual async Task SetAsync(string name, string clientName, Dictionary<string, string> value, string providerName, string providerKey)
        {
            var setting = await AppRepository.FindAsync(name, providerName, providerKey);
            if (setting == null)
            {
                setting = new App(GuidGenerator.Create(), name, clientName, value, providerName, providerKey);
                await AppRepository.InsertAsync(setting);
            }
            else
            {
                setting.Value = value;
                await AppRepository.UpdateAsync(setting);
                var cacheKey = CalculateCacheKey(name, providerName, providerKey);

                var cache = _cacheManager.GetCache("AppManagement");

                await cache.RemoveAsync(
                    cacheKey
                );
            }
        }

        public virtual async Task<List<AppValue>> GetListAsync(string providerName, string providerKey)
        {
            var apps = await AppRepository.GetListAsync(providerName, providerKey);
            return apps.Select(s => new AppValue(s.Name, s.Value)).ToList();
        }

        public virtual async Task DeleteAsync(string name, string providerName, string providerKey)
        {
            var setting = await AppRepository.FindAsync(name, providerName, providerKey);
            if (setting != null)
            {
                await AppRepository.DeleteAsync(setting);
            }
        }

        protected virtual async Task<AppCacheItem> GetCacheItemAsync(string name, string providerName, string providerKey)
        {
            var cacheKey = CalculateCacheKey(name, providerName, providerKey);
            var cache = _cacheManager.GetCache("AppManagement");

            var cacheItem = await cache.GetAsync(cacheKey, async (x) =>
            {
                var app = await AppRepository.FindAsync(name, providerName, providerKey);

                var c = new AppCacheItem(app?.Value);

                return c;
            });

            return cacheItem as AppCacheItem;

            // if (cacheItem != null)
            // {
            //     return cacheItem;
            // }
        }

        protected virtual string CalculateCacheKey(string name, string providerName, string providerKey)
        {
            return AppCacheItem.CalculateCacheKey(name, providerName, providerKey);
        }
    }
}