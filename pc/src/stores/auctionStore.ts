import { defineStore } from 'pinia'
// import { useLocalStorage } from '@vueuse/core'
import { ref } from 'vue'
import { AuctionItemDto, ChatMessageType } from '@/api/appService'
import api from '@/api'
import { GetAuctionMidList } from '@/api/auctionMidAPI'
import { setKasecStatus, getKasecStatus } from '@/api/auctionItemAPI'

export const useAuctionStore = defineStore('auction', () => {
    const chatStore = useChatStore()
    const userStore = useUserStore()
    /**
     * 待拍卖的无
     */
    const list = ref<AuctionItemDto[]>([])
    /**
     * 已完成拍卖的物品
     */
    const list4 = ref<AuctionItemDto[]>([])
    /**
     * 拍卖中的物品
     */
    const auctionMid = ref<AuctionItemDto[]>([])
    // 卡秒状态
    const isKasec = ref(false)

    // 设置卡秒状态（管理员）
    async function setKasec(auctionItemId, kasec) {
        await setKasecStatus(auctionItemId, kasec)
        isKasec.value = kasec
    }
    // 获取卡秒状态
    async function syncKasecStatus(auctionItemId) {
        const res = await getKasecStatus(auctionItemId)
        isKasec.value = !!res
    }

    //结束竞拍
    function end(auctionItemId: number) {
        api.auctionItem.endAuction({ id: auctionItemId }).then((res) => {
            if (res.toUserMsg === '已成交商品不能再次拍卖') {
                return
            }
            chatStore.sendChannelMsg('', '-1_auction', ChatMessageType.AuctionEnd, res)
            if (res.dealUserId) {
                chatStore.sendMsg(
                    res.dealUserId,
                    res.dealUserName,
                    res.dealUserAvatar,
                    res.toUserMsg,
                    ChatMessageType.AuctionDeal,
                    res
                )
            }
            // 结束后自动关闭卡秒
            isKasec.value = false
        })
    }

    //出价
    function bid(auctionItemId: number, bidPrice: number) {
        api.auctionItem
            .bid({
                body: {
                    auctionItemId,
                    bidPrice,
                    bidUserName: userStore.user.name,
                    bidUserAvatar: userStore.user.headImgUrl,
                },
            })
            .then((res) => {
                // chatStore.sendChannelMsg(`${res.currentPrice}`, '-1_auction', ChatMessageType.AuctionBid, res)
                Tips.success('出价成功')
            })
    }

    function getList(status: number | undefined = undefined) {
        return new Promise<void>((resolve) => {
            //待拍卖
            if (!status) {
                api.auctionItem.getPublicList({ maxResultCount: 20 }).then((res) => {
                    list.value = res.items!
                    console.log('拍卖列表已刷新')
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            }
            //拍卖中
            else if (status === 2) {
                GetAuctionMidList({ status, maxResultCount: 20 }).then((res) => {
                    if (res.status == 200) {
                        auctionMid.value.length = 0
                        auctionMid.value = res.data.items
                    }
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            }
            //已完成
            else if (status === 4) {
                api.auctionItem.getPublicList({ status, maxResultCount: 20 }).then((res) => {
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
            return resolve()
            // api.auctionItem.subStartNotify({ auctionItemId, openid: userStore.openid }).then((res) => {
        })
    }

    return {
        list,
        list4,
        auctionMid,
        bid,
        end,
        startNotify,
        startAuction,
        getList,
        isKasec,
        setKasec,
        syncKasecStatus,
    }
})
