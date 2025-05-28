import type { AuctionItemDto } from '@/composables/types'
import api from '@/utils/api'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAuctionStore = defineStore('auction', () => {
    const chatStore = useChatStore()
    const userStore = useUserStore()
    const list = ref<AuctionItemDto[]>([])
    const list4 = ref<AuctionItemDto[]>([])
	const auctionMidList=ref<AuctionItemDto[]>([])

    function end(auctionItemId: number) {
        api.auctionItem.endAuction({ id: auctionItemId }).then((res) => {
            chatStore.sendChannelMsg('', '-1_auction', ChatMessageType.AuctionEnd, res)
        })
    }

    //出价
    function bid(auctionItemId: number, bidPrice: number) {
        api.auctionItem
            .bid({
                auctionItemId,
                bidPrice,
                bidUserName: userStore.user.name,
                bidUserAvatar: userStore.user.headImgUrl,
            })
            .then((res) => {
                // chatStore.sendChannelMsg(`${res.currentPrice}`, '-1_auction', ChatMessageType.AuctionBid, res)
                Tips.success('出价成功')
            })
    }

    function getList(status: number | undefined = undefined) {
        return new Promise<void>((resolve) => {
            if (!status) {
                api.auctionItem.getPublicList({ MaxResultCount: 100 }).then((res) => {
                    list.value = res.items!
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            } else if (status === 4) {
                api.auctionItem.getPublicList({ status, MaxResultCount: 100 }).then((res) => {
                    list4.value = res.items!
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            }
        })

        // fetch auctions from the server
    }

    function startAuction(id: number) {
        api.auctionItem.startAuction({ id: id }).then((res) => {
            // TODO:通知拍卖房间的人刷新拍品列表
            chatStore.sendChannelMsg('', '-1_auction', ChatMessageType.AuctionStart, res)
        })
    }

    function startNotify(id: number) {
        return new Promise<void>((resolve) => {
            api.auctionItem.subStartNotify({ auctionItemId: id, openid: userStore.openid }).then(() => {
                return resolve()
            })
        })
    }

    return {
        list,
        list4,
        bid,
        end,
        startAuction,
        startNotify,
        getList,
		auctionMidList
    }
})
