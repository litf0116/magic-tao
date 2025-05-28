using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using TtWork.Abp.AppManagement.Apps;
using TtWork.Abp.AppManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.UI;
using JetBrains.Annotations;
using TtWork.Abp.Definitions;


namespace TtWork.Abp.AppManagement.Applications {
    namespace TT.Abp.AppManagement.Application {
        public interface IAppAppService : IAsyncCrudAppService<AppDto, Guid, PagedAndSortedResultRequestDto,
            AppCreateOrUpdateDto, AppCreateOrUpdateDto> {
        }

        public class AppAppService :
            AsyncCrudAppService<App, AppDto, Guid, PagedAndSortedResultRequestDto, AppCreateOrUpdateDto,
                AppCreateOrUpdateDto>, IAppAppService {
            private readonly IAppDefinitionManager _appDefinitionManager;
            private readonly IAppProvider _appProvider;

            public AppAppService(
                IRepository<App, Guid> repository,
                IAppDefinitionManager appDefinitionManager,
                IAppProvider appProvider
            ) : base(repository) {
                _appDefinitionManager = appDefinitionManager;
                _appProvider = appProvider;
                base.GetAllPermissionName = AppPermissions.Administration;
                base.GetPermissionName = AppPermissions.Administration;
                base.CreatePermissionName = AppPermissions.Administration;
                base.UpdatePermissionName = AppPermissions.Administration;
                base.DeletePermissionName = AppPermissions.Administration;
            }

            protected override IQueryable<App> CreateFilteredQuery(PagedAndSortedResultRequestDto input) {
                var query = Repository.GetAll()
                        .WhereIf(AbpSession.TenantId.HasValue,
                            x => x.ProviderName == "T" && x.ProviderKey == AbpSession.TenantId.Value.ToString())
                        .WhereIf(!AbpSession.TenantId.HasValue, x => x.ProviderName == "T" && x.ProviderKey == null)
                    ;

                return query;
            }

            [AbpAuthorize(AppPermissions.Administration)]
            public async Task<ListResultDto<AppDto>> GetPublishList() {
                var list = _appDefinitionManager.GetAll();

                List<AppDto> result = new List<AppDto>();
                foreach (var s in list) {
                    result.Add(
                        new AppDto {
                            Name = s.Name,
                            ClientName = s.ClientName,
                            ClientType = s.ClientType,
                            Value = await _appProvider.GetOrNullAsync(s.Name)
                        });
                }

                return new ListResultDto<AppDto>(result); // await Task.FromResult(list);
            }
        }


        /// <summary>
        /// <see cref="App"/>
        /// </summary>
        public class AppDto : EntityDto<Guid> {
            public string Name { get; set; }

            public string ClientName { get; set; }

            public string ClientType { get; set; }

            public Dictionary<string, string> Value { get; set; } = new();

            public string ProviderName { get; set; }

            public string ProviderKey { get; set; }

            public string TryGetValue(string key) {
                return Value.ContainsKey(key) ? Value[key] : null;
            }

            public string GetValue(string key) {
                return Value[key] ?? throw new UserFriendlyException($"App:{Name} {key}未设置");
            }

            public T GetValue<T>(string key, T defaultValue) {
                if (Value[key] == null)
                    return defaultValue;
                try {
                    return (T)Convert.ChangeType(Value[key], typeof(T));
                }
                catch {
                    return defaultValue;
                }
            }
        }

        public class AppCreateOrUpdateDto : EntityDto<Guid> {
            [NotNull] public string Name { get; set; }

            [NotNull] public string ClientName { get; set; }

            [NotNull] public string ProviderName { get; set; }

            [CanBeNull] public string ProviderKey { get; set; }

            public Dictionary<string, string> Value { get; set; } = new();
        }
    }
}