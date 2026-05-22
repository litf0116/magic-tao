import { defineStore } from 'pinia'
// import { useLocalStorage } from '@vueuse/core'
import { ref } from 'vue'
import { AuctionItemDto, ChatMessageType } from '@/api/appService'
import api from '@/api'
import { GetAuctionMidList } from '@/api/auctionMidAPI'
import { setKasecStatus, getKasecStatus, GetDetail } from '@/api/auctionItemAPI'
import { ElMessage } from 'element-plus'
import { convertAuctionPayload } from '@/utils/propertyConverter'
import { convertObjectImageUrlsArray } from '@/utils/imageUrlConverter'

export const useAuctionStore = defineStore('auction', () => {
    const list = ref<AuctionItemDto[]>([])
    const list4 = ref<AuctionItemDto[]>([])
    const auctionMid = ref<AuctionItemDto[]>([])
    // 卡秒状态
    const isKasec = ref(false)

    const userStore = useUserStore()
    const chatStore = useChatStore()

    // 辅助函数：为拍卖品列表计算显示序号
    function calculateDisplayIndices(items: AuctionItemDto[]): AuctionItemDto[] {
        let currentIndex = 1
        return items.map((item) => {
            // 空降商品不显示序号
            if (item.name?.includes('空降')) {
                return { ...item, displayIndex: '' }
            }
            // 其他商品从1开始连续编号
            return { ...item, displayIndex: currentIndex++ }
        })
    }

    // 新增：从出价消息直接更新拍品信息
    function updateAuctionItemFromBidMessage(bidMessagePayload: any) {
        if (!bidMessagePayload) {
            console.warn('出价消息payload为空')
            return
        }

        // 使用convertAuctionPayload进行格式转换，处理PascalCase到camelCase的转换
        const convertedPayload = convertAuctionPayload(bidMessagePayload)

        if (!convertedPayload || !convertedPayload.id) {
            console.warn('出价消息payload中缺少拍品ID')
            return
        }

        const auctionItemId = convertedPayload.id

        const updatedData = {
            currentPrice: convertedPayload.currentPrice,
            currentPriceUserId: convertedPayload.currentPriceUserId,
            currentPriceUserName: convertedPayload.currentPriceUserName,
        }

        // 辅助函数：更新项目时保持 displayIndex
        const updateItem = (item: AuctionItemDto) => {
            return {
                ...item,
                ...updatedData,
                displayIndex: item.displayIndex, // 确保不覆盖原有的 displayIndex
            }
        }

        // 更新list中的拍品（待拍卖列表）
        const listIndex = list.value.findIndex((item) => item.id === auctionItemId)
        if (listIndex !== -1) {
            list.value[listIndex] = updateItem(list.value[listIndex])
        } else {
            // 更新auctionMid中的拍品（拍卖中列表）
            const auctionMidIndex = auctionMid.value.findIndex((item) => item.id === auctionItemId)
            if (auctionMidIndex !== -1) {
                auctionMid.value[auctionMidIndex] = updateItem(auctionMid.value[auctionMidIndex])
            } else {
                // 更新list4中的拍品（已完成列表）
                const list4Index = list4.value.findIndex((item) => item.id === auctionItemId)
                if (list4Index !== -1) {
                    list4.value[list4Index] = updateItem(list4.value[list4Index])
                }
            }
        }
    }

    // 设置卡秒状态（管理员）
    async function setKasec(auctionItemId, kasec) {
        const maxRetries = 2
        let retryCount = 0

        while (retryCount <= maxRetries) {
            try {
                // 获取拍品基本信息（只检查拍品是否存在和状态）
                const detailRes = await GetDetail(auctionItemId)
                const auctionItem = detailRes

                // 只检查拍品是否存在和状态
                if (!auctionItem) {
                    ElMessage.error('找不到指定的拍卖品')
                    return false
                }

                if (auctionItem.status !== '拍卖中') {
                    ElMessage.error(`当前拍卖品状态为"${auctionItem.status}"，无法进行卡秒操作`)
                    return false
                }

                // 直接调用API设置卡秒状态，不检查当前状态
                const res = await setKasecStatus(auctionItemId, kasec)

                // API调用成功后，立即更新前端状态
                if (res) {
                    isKasec.value = kasec
                    return true
                } else {
                    throw new Error('API返回失败')
                }
            } catch (error) {
                retryCount++
                console.error(`卡秒操作失败 (第${retryCount}次尝试):`, error)

                // 如果是最后一次重试，抛出错误
                if (retryCount > maxRetries) {
                    // 根据错误类型提供更详细的错误信息
                    if (error.code === 'NETWORK_ERROR' || error.message?.includes('Network Error')) {
                        error.message = '网络连接异常，请检查网络后重试'
                    } else if (error.code === 'TIMEOUT') {
                        error.message = '操作超时，请稍后重试'
                    } else if (error.message?.includes('状态冲突')) {
                        error.message = '拍品状态已发生变化，请刷新页面后重试'
                    } else if (!error.message) {
                        error.message = '操作失败，请重试'
                    }
                    throw error
                }

                // 等待一段时间后重试
                await new Promise((resolve) => setTimeout(resolve, 1000 * retryCount))
            }
        }

        return false
    }

    // 获取卡秒状态
    async function syncKasecStatus(auctionItemId) {
        const res = await getKasecStatus(auctionItemId)
        // 由于axios拦截器处理了ABP响应，res就是result值
        const kasecStatus = !!res
        isKasec.value = kasecStatus
    }

    //结束竞拍
    async function end(auctionItemId: number) {
        try {
            // 获取最新的拍卖品状态
            const detailRes = await GetDetail(auctionItemId)
            const auctionItem = detailRes

            // 检查拍卖品状态
            if (!auctionItem) {
                ElMessage.error('找不到指定的拍卖品')
                return false
            }

            if (auctionItem.status !== '拍卖中') {
                ElMessage.error(`当前拍卖品状态为"${auctionItem.status}"，无法结束拍卖`)
                return false
            }

            const res = await api.auctionItem.endAuction({ id: auctionItemId })

            if (res.toUserMsg === '已成交商品不能再次拍卖') {
                ElMessage.warning('该商品已经成交，无法重复操作')
                return false
            }

            // 注意：拍卖结束和成交消息已由后端统一发送，前端无需重复发送

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
    async function bid(auctionItemId: number, bidPrice: number) {
        try {
            await api.auctionItem.bid({
                body: {
                    auctionItemId,
                    bidPrice,
                    bidUserName: userStore.user.name,
                    bidUserAvatar: userStore.user.headImgUrl,
                },
            })
            Tips.success('出价成功')
        } catch (error) {
            console.error('【Bid 出价失败】', {
                auctionItemId,
                bidPrice,
                error,
                errorMessage: error?.message,
                errorCode: error?.code,
                errorResponse: error,
                userAgent: navigator.userAgent,
                timestamp: new Date().toISOString(),
            })
            const msg = error?.message || error?.error?.message || '网络异常，请稍后重试'
            ElMessage.error(msg)
        }
    }

    function getList(status: number | undefined = undefined, maxResultCount = 100) {
        return new Promise<void>((resolve) => {
            //待拍卖
            if (!status) {
                api.auctionItem.getPublicList({ maxResultCount }).then((res) => {
                    // 处理图片URL并计算序号
                    const itemsWithImages = convertObjectImageUrlsArray(res.items!, ['imageUrl'])
                    list.value = calculateDisplayIndices(itemsWithImages)
                    return resolve()
                })
            }
            //拍卖中
            if (status === 2) {
                GetAuctionMidList({ status, maxResultCount }).then((res) => {
                    auctionMid.value.length = 0
                    // 处理图片URL并计算序号
                    const itemsWithImages = convertObjectImageUrlsArray(res.items, ['imageUrl'])
                    auctionMid.value = calculateDisplayIndices(itemsWithImages)
                    return resolve()
                })
            }
            //已完成
            if (status === 4) {
                api.auctionItem.getPublicList({ status, maxResultCount }).then((res) => {
                    // 处理图片URL并计算序号
                    const itemsWithImages = convertObjectImageUrlsArray(res.items!, ['imageUrl'])
                    list4.value = calculateDisplayIndices(itemsWithImages)
                    return resolve()
                })
            }
        })
    }

    function startAuction(id: number) {
        api.auctionItem.startAuction({ id: id }).then((res) => {
            // TODO:通知拍卖房间的人刷新拍品列表
            chatStore.sendChannelMsg('', '-1_auction', ChatMessageType.AuctionStart, res)
        })
    }

    function startNotify() {
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
        updateAuctionItemFromBidMessage, // 新增方法
    }
})
