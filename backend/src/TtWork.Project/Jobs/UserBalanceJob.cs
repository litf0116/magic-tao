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
using TtWork.Project.Domains.Pays;
using TtWork.Project.Events.Commands;

namespace TtWork.Project.Jobs;

public class UserBalanceJob(
    ILogger<UserBalanceJob> logger,
    IMediator mediator,
    IRepository<User, long> userRepository,
    IRepository<UserBalanceLog, Ulid> userBalanceLogRepository,
    IUnitOfWorkManager unitOfWorkManager) : IAsyncBackgroundJob<UserBalanceLog>, ITransientDependency {
    [UnitOfWork]
    public virtual async Task ExecuteAsync(UserBalanceLog log) {
        using (unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant)) {
            var entity = await userBalanceLogRepository.GetAll().AsNoTracking().Where(x => x.Id == log.Id).FirstOrDefaultAsync();
            if (entity.IsSuccess) {
                logger.LogWarning($"[UserBalanceJob]用户:{log.CreatorUserId}余额操作已经成功,无需重复操作{log.Id}");
                return;
            }

            var doAmount = log.Type switch {
                BalanceLogType.支付 => log.Amount,
                BalanceLogType.扣除 => -log.Amount,
                BalanceLogType.退还 => -log.Amount,
                _ => throw new Exception("[UserBalanceJob]未知的余额操作模式")
            };

            var cnt = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId).ExecuteUpdateAsync(setter =>
                setter.SetProperty(b => b.Balance, b => b.Balance + doAmount));

            if (cnt != 0) {
                var afterAmount = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId).Select(x => x.Balance).FirstOrDefaultAsync();
                await userBalanceLogRepository.GetAll().Where(x => x.Id == log.Id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(b => b.IsSuccess, true)
                        .SetProperty(b => b.SuccessTime, DateTime.Now)
                        .SetProperty(b => b.AfterAmount, afterAmount)
                    );
                logger.LogInformation($"[UserBalanceJob]用户:{log.CreatorUserId}余额操作成功,当前余额{afterAmount}");

                await mediator.Publish(new MyCountCacheClear(log.CreatorUserId));
            }
            else {
                throw new Exception("[UserBalanceJob]用户余额操作失败");
            }
        }
    }
}