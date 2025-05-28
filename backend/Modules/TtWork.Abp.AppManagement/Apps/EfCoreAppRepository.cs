using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.AppManagement.Domain;

namespace TtWork.Abp.AppManagement.Apps
{
    public class EfCoreAppRepository : IAppRepository , ITransientDependency
    {
        private readonly IRepository<App, Guid> _appRepository;

        public EfCoreAppRepository(IRepository<App, Guid> appRepository)
        {
            _appRepository = appRepository;
        }

        public virtual async Task<App> FindAsync(string name, string providerName, string providerKey)
        {
            return await _appRepository
                .FirstOrDefaultAsync(
                    s => s.Name == name && s.ProviderName == providerName && s.ProviderKey == providerKey
                );
        }

        public virtual async Task<List<App>> GetListAsync(string providerName, string providerKey)
        {
            return await _appRepository.GetAll()
                .Where(
                    s => s.ProviderName == providerName && s.ProviderKey == providerKey
                ).ToListAsync();
        }

        public Task<App> InsertAsync(App app)
        {
            return _appRepository.InsertAsync(app);
        }

        public Task<App> UpdateAsync(App app)
        {
            return _appRepository.UpdateAsync(app);
        }

        public Task DeleteAsync(App app)
        {
            return _appRepository.DeleteAsync(app);
        }
    }
}