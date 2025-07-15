using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Domains;
using TtWork.Project.Events.Queries;
using static System.Guid;

namespace TtWork.Abp {
    public class AbpAsyncCrudAppService<TEntity, TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput> :
        AsyncCrudAppService<TEntity, TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput>
        where TEntity : class, IEntity<TPrimaryKey>
        where TEntityDto : IEntityDto<TPrimaryKey>
        where TUpdateInput : IEntityDto<TPrimaryKey> {
        protected bool EnableGetEdit = false;
        protected bool UpdateCheckOwner = false;
        protected bool GetCreatorUser = false;
        protected bool GetUser = false;
        public UserManager UserManager { get; set; }
        public IMediator _mediator { get; set; }

        public AbpAsyncCrudAppService(
            IRepository<TEntity, TPrimaryKey> repository,
            IocManager iocManager
        )
            : base(repository) {
            LocalizationSourceName = AbpConsts.LocalizationSourceName;
            Logger = NullLogger.Instance;
            _mediator = iocManager.Resolve<IMediator>();
        }

        public virtual async Task<GetForEditOutput<TCreateInput>> GetForEdit(EntityDto<TPrimaryKey> input) {
            if (!EnableGetEdit)
                throw new UserFriendlyException("GetForEdit is not enable");

            var entity = input.Id switch {
                (int and > 0) or (long and > 0) => await GetEntityByIdAsync(input.Id),
                (Guid b) => (b == Empty ? null : await GetEntityByIdAsync(input.Id)),
                _ => null
            };

            var schema = JToken.FromObject(new { });

            var result = new GetForEditOutput<TCreateInput>(
                entity != null
                    ? ObjectMapper.Map<TCreateInput>(entity)
                    : Activator.CreateInstance<TCreateInput>(),
                schema);
            
            // 调试：检查GetForEdit返回的description字段
            if (entity != null && result.Data is dynamic dynamicData)
            {
                try
                {
                    var description = dynamicData.Description;
                    Logger.Debug($"GetForEdit返回的description字段: {description}");
                }
                catch (Exception ex)
                {
                    Logger.Debug($"GetForEdit检查description字段时出错: {ex.Message}");
                }
            }

            return result;
        }

        public override async Task<TEntityDto> GetAsync(EntityDto<TPrimaryKey> input) {
            var dto = await base.GetAsync(input);
            if (GetCreatorUser && dto is IHaveCreatorUser c)
                c.CreatorUser = await _mediator.Send(new QueryUserDtoBase(c.CreatorUserId));

            if (GetUser && dto is IHaveUser b)
                b.User = await _mediator.Send(new QueryUserDtoBase(b.UserId));
            return dto;
        }

        /// <summary>
        /// GetAllAsync
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<PagedResultDto<TEntityDto>> GetAllAsync(TGetAllInput input) {
            var result = await base.GetAllAsync(input);
            if (result.Items.Count > 0 && GetCreatorUser) {
                foreach (var dto in result.Items) {
                    if (dto is IHaveCreatorUser c)
                        c.CreatorUser = await _mediator.Send(new QueryUserDtoBase(c.CreatorUserId));
                }
            }

            if (result.Items.Count > 0 && GetUser) {
                foreach (var dto in result.Items) {
                    if (dto is IHaveUser c)
                        c.User = await _mediator.Send(new QueryUserDtoBase(c.UserId));
                }
            }

            return result;
        }


        public override async Task<TEntityDto> UpdateAsync(TUpdateInput input) {
            CheckUpdatePermission();
            TEntity entity = await GetEntityByIdAsync(input.Id);

            if (entity != null && UpdateCheckOwner && entity is ICreationAudited c) {
                if (AbpSession.UserId == null)
                    throw new UserFriendlyException("请先登录");

                if (c.CreatorUserId != AbpSession.UserId!.Value) {
                    throw new UserFriendlyException("无权编辑");
                }
            }

            MapToEntity(input, entity);
            Repository.GetDbContext().Entry(entity).State = EntityState.Modified;
            await Repository.UpdateAsync(entity);
            //如果不加这句,更新时含有json convention的会不更新
            await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            TEntityDto entityDto = MapToEntityDto(entity);
            entity = default(TEntity);
            return entityDto;
        }

        protected async Task<bool> IsInRoleAsync(long userId, string roleName) {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            var roles = await UserManager.GetRolesAsync(user);
            return roles.Any(x => String.Equals(x, roleName,
                StringComparison.CurrentCultureIgnoreCase));
        }

        protected async Task<bool> IsAdminAsync() {
            if (!AbpSession.UserId.HasValue)
                return false;
            return await IsInRoleAsync(AbpSession.UserId.Value, StaticRoleNames.Tenants.Admin);
        }

        protected async Task<User> GetCurrentUserAsync() {
            return await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
        }
    }
}