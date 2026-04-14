using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Hangfire;
using Microsoft.Extensions.Logging;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Jobs;

public class CleanExpiredPayOrderJob(
    ILogger<CleanExpiredPayOrderJob> logger,
    IUnitOfWorkManager unitOfWorkManager,
    IRepository<PayOrder, Ulid> payOrderRepository)
    : IAsyncBackgroundJob<object>, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task ExecuteAsync(object args)
    {
        var cutoffTime = DateTime.Now.AddHours(-24);
        
        var expiredOrders = payOrderRepository.GetAll()
            .Where(x => x.State == PayState.未支付 && x.CreationTime < cutoffTime)
            .ToList();

        if (expiredOrders.Count == 0)
        {
            logger.LogDebug("No expired unpaid orders to clean");
            return;
        }

        foreach (var order in expiredOrders)
        {
            order.State = PayState.取消;
        }

        await unitOfWorkManager.Current.SaveChangesAsync();
        
        logger.LogInformation("Cleaned {Count} expired unpaid orders (older than 24h)", expiredOrders.Count);
    }
}
