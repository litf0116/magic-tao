using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tt.HttpClient.Weixin.Models;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Caches;
using TtWork.HttpClient.Weixin.Models;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Jobs;

public class TenPayNotifyArgs {
    public WechatPaymentNotification Notification { get; protected set; }

    public TenPayNotifyArgs(WechatPaymentNotification notification) {
        Notification = notification;
    }
}

public class TenPayNotifyJob(
    UserManager userManager,
    UserCache userCache,
    ILogger<TenPayNotifyJob> logger,
    IUnitOfWorkManager unitOfWorkManager,
    IRepository<PayOrder, Ulid> payOrderRepository,
    IRepository<UserBalanceLog, Ulid> userBalanceLogRepository,
    IRepository<UserDepositLog, Ulid> userDepositLogRepository)
    : IAsyncBackgroundJob<TenPayNotifyArgs>, ITransientDependency {
    [UnitOfWork]
    public virtual async Task ExecuteAsync(TenPayNotifyArgs args) {
        try {
            using (unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant)) {
                var data = JsonConvert.DeserializeObject<WeChatPayPaidEventModel>(args.Notification.RawData);

                // 业务逻辑
                var payOrder = await payOrderRepository.FirstOrDefaultAsync(x => x.OutTradeNo == args.Notification.OutTradeNo);

                if (payOrder == null) {
                    logger.LogError($"支付订单不存在: {args.Notification.OutTradeNo}");
                    BackgroundJob.Enqueue<SendWxWorkJob>(
                        z => z.SendMarkdown($"""
                                             <font color="warning">收到支付成功通知，但订单不存在</font>
                                             >OutTradeNo: {args.Notification.OutTradeNo}
                                             """, AppConsts.WorkWxKeys.支付成功通知群, true, "[TenPayNotifyJob]")
                    );
                    // throw new Exception("收到支付成功通知，但订单不存在");
                    return;
                }

                payOrder.SuccessPay(args.Notification.Id.ToString(), args.Notification.SuccessTime);

                if (payOrder.HostType == OrderType.充值) {
                    var log = new UserBalanceLog(BalanceLogType.支付, payOrder.Total / 100m) {
                        CreatorUserId = payOrder.CreatorUserId,
                        TenantId = payOrder.TenantId,
                    };

                    await userBalanceLogRepository.InsertAsync(log);
                    await unitOfWorkManager.Current.SaveChangesAsync();

                    BackgroundJob.Enqueue<UserBalanceJob>(b => b.ExecuteAsync(log));
                }
                else if (payOrder.HostType == OrderType.保证金) {
                    var log = new UserDepositLog(BalanceLogType.支付, payOrder.Total / 100m) {
                        CreatorUserId = payOrder.CreatorUserId,
                        TenantId = payOrder.TenantId,
                    };
                    await userDepositLogRepository.InsertAsync(log);
                    await unitOfWorkManager.Current.SaveChangesAsync();

                    BackgroundJob.Enqueue<UserDepositJob>(b => b.ExecuteAsync(log));
                }

                try {
                    var user = await userCache.GetAsync(payOrder.CreatorUserId!.Value);
                    BackgroundJob.Enqueue<SendWxWorkJob>(
                        z => z.SendMarkdown($$"""
                                              <font color="info">订单支付成功</font>，请相关同事注意。
                                              >OutTradeNo: {{payOrder.OutTradeNo}}
                                              >支付类型:{{payOrder.HostType}}
                                              >订单金额: {{(Convert.ToDecimal(data.Amount.Total) / 100m):F2}}元 {{data.Amount.Currency}}
                                              >实际支付: {{(Convert.ToDecimal(data.Amount.PayerTotal) / 100m):F2}}元
                                              >用户昵称: {{user.Name}}
                                              """, AppConsts.WorkWxKeys.支付成功通知群, true, "[TenPayNotifyJob]")
                    );
                }
                catch (Exception e) {
                    //ignored
                }

                await Task.CompletedTask;
            }
        }
        catch (Exception e) {
            //Console.WriteLine(e);
            throw;
        }
    }
}