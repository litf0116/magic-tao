import type { AuctionItemDto } from '@/composables/types'
import api from '@/utils/api'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAuctionStore = defineStore('auction', () => {
    const chatStore = useChatStore()
    const userStore = useUserStore()
    const list = ref<AuctionItemDto[]>([])
    const list4 = ref<AuctionItemDto[]>([])
    const auctionMidList = ref<AuctionItemDto[]>([])
    // 当前秒杀品ID
    const currentAuctionId = ref<number | null>(null)
    // 卡秒状态
    const isKasec = ref(false)

    function setCurrentAuctionId(auctionId: number) {
        currentAuctionId.value = auctionId
        syncKasecStatus(auctionId)
    }

    async function syncKasecStatus(auctionId: number) {
        try {
            const res = await api.auctionItem.getKasecStatus(auctionId)
            const kasecStatus = !!res
            isKasec.value = kasecStatus
            return kasecStatus
        } catch (error) {
            console.error('获取卡秒状态失败:', error)
            // 如果获取失败，默认为false
            isKasec.value = false
            return false
        }
    }

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
                    // Tips.success('秒杀列表已刷新')
                    return resolve()
                })
            } else if (status === 4) {
                api.auctionItem.getPublicList({ status, MaxResultCount: 100 }).then((res) => {
                    list4.value = res.items!
                    // Tips.success('秒杀列表已刷新')
                    return resolve()
                })
            }
        })

        // fetch auctions from the server
    }

    function startAuction(id: number) {
        api.auctionItem.startAuction({ id: id }).then((res) => {
            // TODO:通知秒杀房间的人刷新秒品列表
            chatStore.sendChannelMsg('', '-1_auction', ChatMessageType.AuctionStart, res)
        })
    }

    function startNotify(id: number, platform: 'miniprogram' | 'app' = 'miniprogram', openid?: string) {
        return new Promise<void>((resolve, reject) => {
            const data: { auctionItemId: number; openid?: string; platform: string } = {
                auctionItemId: id,
                platform,
            }

            if (platform === 'miniprogram') {
                data.openid = openid || userStore.openid
            }

            api.auctionItem
                .subStartNotify(data)
                .then(() => {
                    return resolve()
                })
                .catch((error: any) => {
                    return reject(error)
                })
        })
    }

    return {
        list,
        list4,
        auctionMidList,
        bid,
        end,
        startAuction,
        startNotify,
        getList,
        currentAuctionId,
        setCurrentAuctionId,
        isKasec,
        syncKasecStatus,
    }
})
