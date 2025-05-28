using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Applications;
using TtWork.Project.Core;
using TtWork.Project.Domains.Pays;
using TtWork.Project.Events.Commands;

namespace TtWork.Project.Jobs;

public class UserDepositJob(
    ILogger<UserDepositJob> logger,
    IRepository<User, long> userRepository,
    IRepository<UserDepositLog, Ulid> userDepositLogRepository,
    UserManager userManager,
    IMediator mediator,
    IUnitOfWorkManager unitOfWorkManager) : IAsyncBackgroundJob<UserDepositLog>, ITransientDependency {
    [UnitOfWork]
    public virtual async Task ExecuteAsync(UserDepositLog log) {
        using (unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant)) {
            var entity = await userDepositLogRepository.GetAll().AsNoTracking().Where(x => x.Id == log.Id).FirstOrDefaultAsync();
            if (entity.IsSuccess) {
                logger.LogWarning($"[UserBalanceJob]用户:{log.CreatorUserId}保证金操作已经成功,无需重复操作{log.Id}");
                return;
            }

            var doAmount = log.Type switch {
                BalanceLogType.支付 => log.Amount,
                BalanceLogType.扣除 => -log.Amount,
                BalanceLogType.退还 => -log.Amount,
                _ => throw new Exception("[UserDepositJob]未知的余额操作模式")
            };

            #region 余额操作

            var cnt = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId).ExecuteUpdateAsync(setter =>
                setter.SetProperty(b => b.DepositBalance, b => b.DepositBalance + doAmount));

            if (cnt != 0) {
                var afterAmount = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId)
                    .Select(x => x.DepositBalance).FirstOrDefaultAsync();

                await userDepositLogRepository.GetAll().Where(x => x.Id == log.Id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(b => b.IsSuccess, true)
                        .SetProperty(b => b.SuccessTime, DateTime.Now)
                        .SetProperty(b => b.AfterAmount, afterAmount)
                    );
                logger.LogInformation($"[UserDepositJob]用户:{log.CreatorUserId}保证金操作成功,当前保证金{afterAmount}");
            }
            else {
                throw new Exception("[UserDepositJob]用户余额操作失败");
            }

            #endregion

            #region 加权限

            var user = await userManager.GetUserByIdAsync(log.CreatorUserId!.Value);
            await userManager.AddToRoleAsync(user, ProjectRoles.竞拍用户);
            // await unitOfWorkManager.Current.SaveChangesAsync();

            #endregion

            await mediator.Publish(new MyCountCacheClear(log.CreatorUserId));
        }
    }
}