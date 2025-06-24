import { defineStore } from 'pinia'
// import { useLocalStorage } from '@vueuse/core'
import { ref } from 'vue'
import { AuctionItemDto, ChatMessageType } from '@/api/appService'
import api from '@/api'
import { GetAuctionMidList } from '@/api/auctionMidAPI'
import { setKasecStatus, getKasecStatus, GetDetail } from '@/api/auctionItemAPI'
import { ElMessage } from 'element-plus'

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
        try {
            // 获取最新的拍卖品状态
            const detailRes = await GetDetail(auctionItemId)
            const auctionItem = detailRes.data || detailRes

            // 检查拍卖品状态
            if (!auctionItem) {
                ElMessage.error('找不到指定的拍卖品')
                return false
            }

            if (auctionItem.status !== '拍卖中') {
                ElMessage.error(`当前拍卖品状态为"${auctionItem.status}"，无法进行卡秒操作`)
                return false
            }

            console.log('卡秒操作前状态检查通过:', {
                auctionItemId,
                status: auctionItem.status,
                kasec,
            })

            const res = await setKasecStatus(auctionItemId, kasec)
            console.log('setKasec', res)
            isKasec.value = kasec
            return res
        } catch (error) {
            console.error('卡秒操作失败:', error)
            ElMessage.error('卡秒操作失败，请稍后重试')
            return false
        }
    }

    // 获取卡秒状态
    async function syncKasecStatus(auctionItemId) {
        console.log('开始获取卡秒状态，拍品ID:', auctionItemId)
        const res = await getKasecStatus(auctionItemId)
        console.log('API返回的完整响应:', res)
        console.log('API返回的data:', res.data)
        // 由于axios拦截器处理了ABP响应，res.data就是result值
        const kasecStatus = !!res.data
        console.log('解析后的卡秒状态:', kasecStatus)
        isKasec.value = kasecStatus
        console.log('设置isKasec.value为:', isKasec.value)
    }

    //结束竞拍
    async function end(auctionItemId: number) {
        try {
            // 获取最新的拍卖品状态
            const detailRes = await GetDetail(auctionItemId)
            const auctionItem = detailRes.data || detailRes

            // 检查拍卖品状态
            if (!auctionItem) {
                ElMessage.error('找不到指定的拍卖品')
                return false
            }

            if (auctionItem.status !== '拍卖中') {
                ElMessage.error(`当前拍卖品状态为"${auctionItem.status}"，无法结束拍卖`)
                return false
            }

            console.log('结束拍卖前状态检查通过:', {
                auctionItemId,
                status: auctionItem.status,
                name: auctionItem.name,
            })

            const res = await api.auctionItem.endAuction({ id: auctionItemId })

            if (res.toUserMsg === '已成交商品不能再次拍卖') {
                ElMessage.warning('该商品已经成交，无法重复操作')
                return false
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

            ElMessage.success('拍卖结束成功')
            return true
        } catch (error) {
            console.error('结束拍卖失败:', error)
            ElMessage.error('结束拍卖失败，请稍后重试')
            return false
        }
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
                api.auctionItem.getPublicList({ status, maxResultCount: 100 }).then((res) => {
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
