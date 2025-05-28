using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TtWork.Project.Domains;

namespace TtWork.Project.Events.Commands;

public class RollBackAuctionEvent(AuctionItemDto payload) : MediatR.INotification {
    public AuctionItemDto Payload { get; } = payload;

    public class RollBackAuctionEventHandler(
        IRepository<AuctionItem, long> repository,
        IRepository<BidHistory, long> bitHistoryRepository)
        : INotificationHandler<RollBackAuctionEvent> {
        [UnitOfWork]
        public virtual async Task Handle(RollBackAuctionEvent notification, CancellationToken cancellationToken) {
            var auctionItem = await repository.GetAsync(notification.Payload.Id);
            if (auctionItem != null) {
                if (auctionItem.Status == AuctionStatusEnum.已成交)
                    throw new UserFriendlyException("商品已成交,不能撤回拍卖记录");

                var bidHistorys = await bitHistoryRepository.GetAll()
                    .Where(x => x.AuctionItemId == auctionItem.Id && !x.IsRollBack)
                    .OrderByDescending(x => x.BidPrice).ToListAsync(cancellationToken: cancellationToken);

                BidHistory previousBid = null;
                //从高价到低价搜索
                for (var index = 0; index < bidHistorys.Count; index++) {
                    var t = bidHistorys[index];
                    if (t.BidPrice == notification.Payload.CurrentPrice) {
                        //找到当前价格的出价记录
                        t.IsRollBack = true;
                        if (index + 1 < bidHistorys.Count)
                            previousBid = bidHistorys[index + 1];
                        await bitHistoryRepository.UpdateAsync(t);
                        break;
                    }
                }

                if (auctionItem.CurrentPrice == notification.Payload.CurrentPrice) {
                    //如果当前价格和传入的价格一致,则回滚到上一次的价格
                    auctionItem.RollBack(previousBid);
                }
            }
        }
    }
}