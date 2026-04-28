using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.UI;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TtWork.Project.Domains;
using TtWork.Project.Services.Cache;

namespace TtWork.Project.Events.Commands;

public class RollBackAuctionEvent(AuctionItemDto payload) : IRequest<AuctionItemDto> {
    public AuctionItemDto Payload { get; } = payload;

    public class RollBackAuctionEventHandler(
        IRepository<AuctionItem, long> repository,
        IRepository<BidHistory, long> bitHistoryRepository,
        IAuctionItemCacheService cacheService,
        IObjectMapper objectMapper)
        : IRequestHandler<RollBackAuctionEvent, AuctionItemDto> {
        [UnitOfWork]
        public virtual async Task<AuctionItemDto> Handle(RollBackAuctionEvent notification, CancellationToken cancellationToken) {
            var auctionItem = await repository.GetAsync(notification.Payload.Id);
            if (auctionItem != null) {
                if (auctionItem.Status == AuctionStatusEnum.已成交)
                    throw new UserFriendlyException("商品已成交,不能撤回拍卖记录");

                var bidHistorys = await bitHistoryRepository.GetAll()
                    .Where(x => x.AuctionItemId == auctionItem.Id && !x.IsRollBack)
                    .OrderByDescending(x => x.BidPrice).ToListAsync(cancellationToken: cancellationToken);

                BidHistory previousBid = null;
                for (var index = 0; index < bidHistorys.Count; index++) {
                    var t = bidHistorys[index];
                    if (t.BidPrice == notification.Payload.CurrentPrice) {
                        t.IsRollBack = true;
                        if (index + 1 < bidHistorys.Count)
                            previousBid = bidHistorys[index + 1];
                        await bitHistoryRepository.UpdateAsync(t);
                        break;
                    }
                }

                if (auctionItem.CurrentPrice == notification.Payload.CurrentPrice) {
                    auctionItem.RollBack(previousBid);

                    if (previousBid == null) {
                        auctionItem.CurrentPrice = auctionItem.StartingPrice;
                        auctionItem.CurrentPriceUserId = null;
                        auctionItem.CurrentPriceUserName = null;
                    }

                    await cacheService.ClearAuctionDetailCacheAsync(auctionItem.Id);
                    await cacheService.ClearAuctionListCacheAsync(AuctionStatusEnum.拍卖中);
                    await cacheService.ClearCurrentAuctionCacheAsync();

                    return objectMapper.Map<AuctionItemDto>(auctionItem);
                }
            }
            return null;
        }
    }
}
