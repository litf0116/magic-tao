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
        return items.map(item => {
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
        console.log('=== 开始更新拍品信息 ===')
        console.log('原始出价消息payload:', bidMessagePayload)
        console.log('payload类型:', typeof bidMessagePayload)

        if (!bidMessagePayload) {
            console.warn('出价消息payload为空')
            return
        }

        // 使用convertAuctionPayload进行格式转换，处理PascalCase到camelCase的转换
        const convertedPayload = convertAuctionPayload(bidMessagePayload)
        console.log('转换后的payload:', convertedPayload)

        if (!convertedPayload || !convertedPayload.id) {
            console.warn('出价消息payload中缺少拍品ID')
            console.log('payload中的所有字段:', Object.keys(convertedPayload || {}))
            return
        }

        const auctionItemId = convertedPayload.id
        console.log('拍品ID:', auctionItemId, '类型:', typeof auctionItemId)

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
                displayIndex: item.displayIndex // 确保不覆盖原有的 displayIndex
            }
        }

        console.log('要更新的数据:', updatedData)
        console.log('当前list长度:', list.value.length)
        console.log('当前auctionMid长度:', auctionMid.value.length)
        console.log('当前list4长度:', list4.value.length)

        // 更新list中的拍品（待拍卖列表）
        const listIndex = list.value.findIndex((item) => item.id === auctionItemId)
        if (listIndex !== -1) {
            console.log('找到待拍卖列表中的拍品，索引:', listIndex)
            console.log('更新前:', list.value[listIndex])
            list.value[listIndex] = updateItem(list.value[listIndex])
            console.log('更新后:', list.value[listIndex])
        } else {
            console.log('在待拍卖列表中未找到拍品ID:', auctionItemId)
        }

        // 更新auctionMid中的拍品（拍卖中列表）
        const auctionMidIndex = auctionMid.value.findIndex((item) => item.id === auctionItemId)
        if (auctionMidIndex !== -1) {
            console.log('找到拍卖中列表中的拍品，索引:', auctionMidIndex)
            console.log('更新前:', auctionMid.value[auctionMidIndex])
            auctionMid.value[auctionMidIndex] = updateItem(auctionMid.value[auctionMidIndex])
            console.log('更新后:', auctionMid.value[auctionMidIndex])
        } else {
            console.log('在拍卖中列表中未找到拍品ID:', auctionItemId)
            console.log(
                '拍卖中列表的所有拍品ID:',
                auctionMid.value.map((item) => item.id)
            )
        }

        // 更新list4中的拍品（已完成列表）
        const list4Index = list4.value.findIndex((item) => item.id === auctionItemId)
        if (list4Index !== -1) {
            console.log('找到已完成列表中的拍品，索引:', list4Index)
            console.log('更新前:', list4.value[list4Index])
            list4.value[list4Index] = updateItem(list4.value[list4Index])
            console.log('更新后:', list4.value[list4Index])
        } else {
            console.log('在已完成列表中未找到拍品ID:', auctionItemId)
        }

        console.log('=== 拍品信息更新完成 ===')
    }

    // 设置卡秒状态（管理员）
    async function setKasec(auctionItemId, kasec) {
        console.log('=== setKasec 方法开始 ===')
        console.log('输入参数:', { auctionItemId, kasec })

        const maxRetries = 2
        let retryCount = 0

        while (retryCount <= maxRetries) {
            try {
                console.log(`第${retryCount + 1}次尝试执行卡秒操作`)

                // 获取拍品基本信息（只检查拍品是否存在和状态）
                console.log('获取拍品基本信息...')
                const detailRes = await GetDetail(auctionItemId)
                const auctionItem = detailRes.data || detailRes
                console.log('拍品基本信息:', auctionItem)

                // 只检查拍品是否存在和状态
                if (!auctionItem) {
                    console.log('找不到指定的拍卖品')
                    ElMessage.error('找不到指定的拍卖品')
                    return false
                }

                if (auctionItem.status !== '拍卖中') {
                    console.log(`拍品状态不正确: ${auctionItem.status}`)
                    ElMessage.error(`当前拍卖品状态为"${auctionItem.status}"，无法进行卡秒操作`)
                    return false
                }

                console.log('拍品状态检查通过，直接执行卡秒设置...')

                // 直接调用API设置卡秒状态，不检查当前状态
                console.log('准备调用 setKasecStatus API...')
                const res = await setKasecStatus(auctionItemId, kasec)
                console.log('setKasec API调用结果:', res)

                // API调用成功后，立即更新前端状态
                if (res) {
                    console.log('卡秒操作API调用成功，立即更新前端状态...')
                    isKasec.value = kasec
                    console.log('前端状态已更新，当前状态:', isKasec.value)
                    console.log('=== setKasec 方法成功结束 ===')
                    return true
                } else {
                    console.log('API返回失败')
                    throw new Error('API返回失败')
                }
            } catch (error) {
                retryCount++
                console.error(`卡秒操作失败 (第${retryCount}次尝试):`, error)

                // 如果是最后一次重试，抛出错误
                if (retryCount > maxRetries) {
                    console.log('已达到最大重试次数，抛出错误')
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
                    console.log('=== setKasec 方法失败结束 ===')
                    throw error
                }

                // 等待一段时间后重试
                console.log(`等待${1000 * retryCount}ms后重试...`)
                await new Promise((resolve) => setTimeout(resolve, 1000 * retryCount))
                console.log(`准备第${retryCount + 1}次重试...`)
            }
        }

        console.log('=== setKasec 方法异常结束 ===')
        return false
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
                // 注意：出价消息已由后端发送，前端无需重复发送
                Tips.success('出价成功')
            })
    }

    function getList(status: number | undefined = undefined, maxResultCount: number = 100) {
        return new Promise<void>((resolve) => {
            //待拍卖
            if (!status) {
                api.auctionItem.getPublicList({ maxResultCount }).then((res) => {
                    console.log('=== getList 待拍卖列表 API响应 ===')
                    console.log('返回数据条数:', res.items?.length)
                    console.log('第一条数据ID:', res.items?.[0]?.id)
                    console.log('最后一条数据ID:', res.items?.[res.items?.length - 1]?.id)

                    // 处理图片URL并计算序号
                    const itemsWithImages = convertObjectImageUrlsArray(res.items!, ['imageUrl'])
                    list.value = calculateDisplayIndices(itemsWithImages)
                    console.log('拍卖列表已刷新，实际数据条数:', list.value.length)
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            }
            //拍卖中
            if (status === 2) {
                GetAuctionMidList({ status, maxResultCount }).then((res) => {
                    if (res.status == 200) {
                        console.log('=== getList 拍卖中列表 API响应 ===')
                        console.log('返回数据条数:', res.data?.items?.length)

                        auctionMid.value.length = 0
                        // 处理图片URL并计算序号
                        const itemsWithImages = convertObjectImageUrlsArray(res.data.items, ['imageUrl'])
                        auctionMid.value = calculateDisplayIndices(itemsWithImages)
                        console.log('拍卖中列表已刷新，实际数据条数:', auctionMid.value.length)
                    }
                    // Tips.success('拍卖列表已刷新')
                    return resolve()
                })
            }
            //已完成
            if (status === 4) {
                api.auctionItem.getPublicList({ status, maxResultCount }).then((res) => {
                    console.log('=== getList 已完成列表 API响应 ===')
                    console.log('返回数据条数:', res.items?.length)
                    console.log('第一条数据ID:', res.items?.[0]?.id)

                    // 处理图片URL并计算序号
                    const itemsWithImages = convertObjectImageUrlsArray(res.items!, ['imageUrl'])
                    list4.value = calculateDisplayIndices(itemsWithImages)
                    console.log('已完成列表已刷新，实际数据条数:', list4.value.length)
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
        updateAuctionItemFromBidMessage, // 新增方法
    }
})
