using System;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Core.MultiTenancy;

namespace TtWork.Abp.Core
{
    public class AppDomainServicebase : DomainService, IIocManagerAccessor
    {
        public TenantManager TenantManager { get; set; }
        public UserManager UserManager { get; set; }

        protected readonly IHttpContextAccessor _httpContext;
        
        protected readonly IAbpSession _abpSession;
        
        protected readonly ICacheManager _cacheManager;
        
        protected readonly IUnitOfWork _unitOfWork;

        public IIocManager IocManager { get; }

        public AppDomainServicebase(IIocManager iocManager)
        {
            IocManager = iocManager;
            _httpContext = IocManager.Resolve<IHttpContextAccessor>();
            _cacheManager = IocManager.Resolve<ICacheManager>();
            _abpSession = IocManager.Resolve<IAbpSession>();
            _unitOfWork = IocManager.Resolve<IUnitOfWork>();
            LocalizationSourceName = AbpConsts.LocalizationSourceName;
        }

        protected virtual Task<User> GetCurrentUserAsync()
        {
            var user = UserManager.FindByIdAsync(_abpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(_abpSession.GetTenantId());
        }
    }
}