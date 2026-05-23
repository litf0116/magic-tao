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
using SqlSugar;
using TtWork.Project.Web.Controllers;

namespace TtWork.Project.Jobs;

public class UserDepositJob(
    ILogger<UserDepositJob> logger,
    IRepository<User, long> userRepository,
    IRepository<UserDepositLog, Ulid> userDepositLogRepository,
    UserManager userManager,
    IMediator mediator,
    IUnitOfWorkManager unitOfWorkManager,
    ISqlSugarClient sqlSugarClient,
    GroupChatLevelSettingsService groupChatLevelSettingsService) : IAsyncBackgroundJob<UserDepositLog>, ITransientDependency {
    [UnitOfWork]
    public virtual async Task ExecuteAsync(UserDepositLog log) {
        logger.LogDebug("[UserDepositJob]开始执行 LogId={LogId}, CreatorUserId={UserId}, Amount={Amount}, Type={Type}",
            log.Id, log.CreatorUserId, log.Amount, log.Type);

        using (unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant)) {
            var entity = await userDepositLogRepository.GetAll().AsNoTracking().Where(x => x.Id == log.Id).FirstOrDefaultAsync();
            if (entity == null)
            {
                logger.LogError("[UserDepositJob]保证金记录不存在 LogId={LogId}", log.Id);
                return;
            }

            logger.LogDebug("[UserDepositJob]查询到的记录 IsSuccess={IsSuccess}, Reason={Reason}", entity.IsSuccess, entity.Reason);

            if (entity.IsSuccess) {
                logger.LogWarning($"[UserDepositJob]用户:{log.CreatorUserId}保证金操作已经成功,无需重复操作{log.Id}");
                return;
            }

            if (!string.IsNullOrEmpty(entity.Reason) && entity.Reason.Contains("保证金支付:"))
            {
                var outTradeNo = entity.Reason.Replace("保证金支付:", "");
                var existingSuccessLog = await userDepositLogRepository.GetAll()
                    .Where(x => x.Id != log.Id && x.IsSuccess)
                    .Where(x => x.Reason != null && x.Reason.Contains(outTradeNo))
                    .FirstOrDefaultAsync();

                if (existingSuccessLog != null)
                {
                    logger.LogWarning($"[UserDepositJob]该订单已有其他成功记录，跳过: {outTradeNo}");
                    return;
                }
            }

            var doAmount = log.Type switch {
                BalanceLogType.支付 => log.Amount,
                BalanceLogType.扣除 => -log.Amount,
                BalanceLogType.退还 => -log.Amount,
                _ => throw new Exception("[UserDepositJob]未知的余额操作模式")
            };
            logger.LogDebug("[UserDepositJob]余额操作 doAmount={DoAmount}", doAmount);

            #region 余额操作
            logger.LogDebug("[UserDepositJob]更新用户余额 UserId={UserId}, doAmount={DoAmount}", log.CreatorUserId, doAmount);
            var cnt = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId).ExecuteUpdateAsync(setter =>
                setter.SetProperty(b => b.DepositBalance, b => b.DepositBalance + doAmount));

            if (cnt != 0) {
                var afterAmount = await userRepository.GetAll().Where(x => x.Id == log.CreatorUserId)
                    .Select(x => x.DepositBalance).FirstOrDefaultAsync();
                logger.LogDebug("[UserDepositJob]余额更新成功，更新后余额={AfterAmount}", afterAmount);

                await userDepositLogRepository.GetAll().Where(x => x.Id == log.Id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(b => b.IsSuccess, true)
                        .SetProperty(b => b.SuccessTime, DateTime.Now)
                        .SetProperty(b => b.AfterAmount, afterAmount)
                    );
                logger.LogInformation($"[UserDepositJob]用户:{log.CreatorUserId}保证金操作成功,当前保证金{afterAmount}");
            }
            else {
                logger.LogError("[UserDepositJob]用户余额操作失败 UserId={UserId}, cnt=0", log.CreatorUserId);
                throw new Exception("[UserDepositJob]用户余额操作失败");
            }
            #endregion

            #region 加权限和更新等级
            logger.LogDebug("[UserDepositJob]开始角色操作 GetUserByIdAsync UserId={UserId}", log.CreatorUserId);
            User user = null;
            try
            {
                user = await userManager.GetUserByIdAsync(log.CreatorUserId!.Value);
                logger.LogDebug("[UserDepositJob]获取用户成功 UserId={UserId}, UserName={UserName}, Name={Name}",
                    user.Id, user.UserName, user.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UserDepositJob]GetUserByIdAsync 异常 UserId={UserId}", log.CreatorUserId);
                throw;
            }

            try
            {
                logger.LogDebug("[UserDepositJob]执行 AddToRoleAsync Role={Role}", ProjectRoles.竞拍用户);
                var addRoleResult = await userManager.AddToRoleAsync(user, ProjectRoles.竞拍用户);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => $"[{e.Code}]{e.Description}"));
                    logger.LogError("[UserDepositJob]AddToRoleAsync 失败 UserId={UserId}, Errors={Errors}",
                        log.CreatorUserId, errors);
                }
                else
                {
                    logger.LogDebug("[UserDepositJob]AddToRoleAsync 成功 UserId={UserId}", log.CreatorUserId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UserDepositJob]AddToRoleAsync 异常 UserId={UserId}, Role={Role}",
                    log.CreatorUserId, ProjectRoles.竞拍用户);
                throw;
            }

            #endregion

            #region 自动升级群聊等级
            logger.LogDebug("[UserDepositJob]开始群聊等级操作 UserId={UserId}", log.CreatorUserId);
            try
            {
                var userGroupLevel = await sqlSugarClient.Queryable<TtWork.Abp.Entity.UserGroupLevelEntity>()
                    .LeftJoin<TtWork.Abp.Entity.GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                    .Where((a, b) => a.UserId == log.CreatorUserId)
                    .Select((a, b) => new { a.UserId, b.Level, a.CumulativeAmount })
                    .FirstAsync();
                int userLevel = userGroupLevel?.Level ?? 0;
                logger.LogDebug("[UserDepositJob]当前群聊等级 UserLevel={UserLevel}", userLevel);

                if (userLevel == 0) {
                    decimal newCumulative = userGroupLevel?.CumulativeAmount >= 88 ? userGroupLevel.CumulativeAmount : 88;
                    var groupChatLevelSettings = await sqlSugarClient.Queryable<TtWork.Abp.Entity.GroupChatLevelSettingsEntity>()
                        .Where(w => w.AmountRequired <= newCumulative)
                        .OrderByDescending(o => o.AmountRequired)
                        .FirstAsync();
                    if (userGroupLevel != null && userGroupLevel.UserId != 0) {
                        await sqlSugarClient.Updateable<TtWork.Abp.Entity.UserGroupLevelEntity>()
                            .SetColumns(u => u.CumulativeAmount == newCumulative)
                            .SetColumns(u => u.GroupChatId == groupChatLevelSettings.Id)
                            .Where(u => u.UserId == log.CreatorUserId)
                            .ExecuteCommandAsync();
                    } else {
                        await sqlSugarClient.Insertable(new TtWork.Abp.Entity.UserGroupLevelEntity {
                            UserId = log.CreatorUserId.Value,
                            CumulativeAmount = newCumulative,
                            GroupChatId = groupChatLevelSettings.Id
                        }).ExecuteCommandAsync();
                    }
                    logger.LogInformation($"[UserDepositJob]用户:{log.CreatorUserId}保证金到账后自动累计金额并升级等级");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UserDepositJob]群聊等级操作异常 UserId={UserId}", log.CreatorUserId);
            }
            #endregion

            logger.LogDebug("[UserDepositJob]发布缓存清除事件 UserId={UserId}", log.CreatorUserId);
            await mediator.Publish(new MyCountCacheClear(log.CreatorUserId));
            logger.LogDebug("[UserDepositJob]执行完毕 UserId={UserId}", log.CreatorUserId);
        }
    }
}