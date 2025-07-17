<template>
    <view>
        <view v-if="item" class="sticky top-0 left-0 right-0 z-99">
            <uv-notice-bar
                mode="link"
                bgColor="#f4835a"
                color="#ffffff"
                :text="announceContent"
                :url="`/pages/announce/list?id=2`"
            ></uv-notice-bar>
        </view>

        <view :class="{ show: showUnread }" class="message-notification fixed right-0 top-140rpx z-100">
            <view class="text-30rpx w-170rpx py-2 bg-[#ff7144] text-white font-700 rounded-l-lg text-center">
                {{ unread }}条新消息
            </view>
        </view>

        <view class="fixed right-0 top-290rpx z-100" @click.stop="showGoods">
            <view class="text-30rpx w-48rpx py-2 bg-[#ff7144] text-white font-700 rounded-l-lg text-center">
                待拍品
            </view>
        </view>

        <view v-if="onAuctionItem && onAuctionItem.id" class="fixed right-2 bottom-500rpx z-100">
            <view
                class="flex text-sm justify-center underline text-white bg-[#ff7144] py-2 mb-2 rounded-lg opacity-80"
                @click.stop="showonAuctionDetail"
                >拍品详情</view
            >
            <view class="py-4 px-3 bg-red-500 rounded-lg text-center opacity-80" @click.stop="bid">
                <view class="text-sm text-white mb-2">拍卖中</view>
                <view class="text-32rpx text-white font-700 underline">出价</view>
            </view>
        </view>
        <chatMain
            ref="chatRef"
            :options="{ chatType: 'auction' }"
            @onSend="send"
            @loadHistoryMessage="loadHistoryMessage"
            @showDetail="showDetail"
        >
        </chatMain>
    </view>

    <uv-popup ref="popupRef" mode="right">
        <view class="w-65vw h-100vh">
            <auctionList @showDetail="showDetail" />
        </view>
    </uv-popup>

    <uv-popup ref="popup" @change="popChange">
        <view v-if="showItem" class="p-4">
            <view class="flex flex-row items-center overflow-hidden cursor-pointer">
                <!-- <image :src="getImgUrl(showItem.imageUrl!, true)" class="w-16 h-16 rounded" mode="aspectFill"
					@click="showImgPreview(showItem.imageUrl!)" /> -->
                <view class="text-wrap px-2 flex-1 flex flex-col">
                    <view class="text-[#ff7144] line-clamp-3">{{ showItem.name }}</view>
                </view>
                <view v-if="showItem.status === '上架'">
                    <uv-button type="success" @click="sub(showItem)">开拍通知</uv-button>
                </view>
            </view>
            <div
                class="mt-2 min-w-200px max-h-50vh overflow-scroll"
                v-html="getStartContent(showItem!)"
                @tap="catchImage"
            ></div>
            <view class="h-8"></view>
        </view>
    </uv-popup>
    <!-- 公告弹窗 -->
    <uv-popup ref="popupShowRef" type="message">
        <view v-if="item" class="popup-content">
            <text class="popup-title">公告</text>
            <img v-if="item.imageUrl" :src="item.imageUrl" mode="aspectFit" class="popup-image" />
            <text class="popup-text">{{ item.content }}</text>
            <view class="popup-view">
                <button @tap="onConfirm" class="popup-button">确定</button>
            </view>
        </view>
    </uv-popup>

    <!-- 出价规则弹窗 -->
    <BidRulesModal
        v-model:show="bidRulesModalVisible"
        :message="bidRulesMessage"
        :currentPrice="bidRulesCurrentPrice"
        :minBidPrice="bidRulesMinPrice"
        @confirm="onBidRulesConfirm"
    />
</template>

<script setup lang="ts">
import chatMain from '@/components/chat/chatMain.vue'
import { getImgUrl, Tips } from '@/composables'
import type { AnnounceDto, AuctionItemDto } from '@/composables/types'
import auctionList from '@/components/chat/auctionList.vue'
import BidRulesModal from '@/components/BidRulesModal.vue'
import api from '@/utils/api'
import { calculateMinBidPrice } from '@/utils/auction'
import { onLoad, onShow, onReady } from '@dcloudio/uni-app'
import { ChatMessageType } from '@/composables/types'
import { nextTick, onUnmounted } from 'vue'

// import AuctionList from '@/components/chat/AuctionList.vue'
const chatStore = useChatStore()
const userStore = useUserStore()
const auctionStore = useAuctionStore()
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)
const popupRef = ref(null as any)
//公告信息跟弹窗
const item = ref<AnnounceDto | null>(null)
const popupShowRef = ref(null as any)

// 出价规则弹窗相关
const bidRulesModalVisible = ref(false)
const bidRulesMessage = ref('')
const bidRulesCurrentPrice = ref(0)
const bidRulesMinPrice = ref(0)

//未读消息条数
const unread = ref('')
const showUnread = ref(false)

onLoad(() => {
    chatStore.connectServer().then(async () => {
        //todo
        console.log('onShow')
        init('-1_auction')
    })
    //获取最新公告
    api.announce.getLatest({ id: 2 }).then((res) => {
        item.value = res
        nextTick(() => {
            var noticeInfo = uni.getStorageSync('auctionNotice')
            if (noticeInfo === '' || noticeInfo.id != res.id) {
                popupShowRef.value.open()
            }
        })
    })

    uni.$on('eventUnread', () => {
        unread.value = uni.getStorageSync('unreadCount')
        showUnread.value = true
        setTimeout(() => {
            showUnread.value = false
        }, 3000)
    })

    // 监听出价规则弹窗事件
    uni.$on('showBidRulesModal', (data: { message: string; needPriceInfo?: boolean }) => {
        bidRulesMessage.value = data.message

        // 如果需要价格信息，从当前拍卖商品获取
        if (data.needPriceInfo && onAuctionItem.value) {
            bidRulesCurrentPrice.value = onAuctionItem.value.currentPrice || onAuctionItem.value.startingPrice || 0
            // 计算最低出价
            bidRulesMinPrice.value = calculateMinBidPrice(
                bidRulesCurrentPrice.value,
                auctionStore.isKasec
            )
        }

        bidRulesModalVisible.value = true
    })
})

// 组件销毁时移除监听
onUnmounted(() => {
    uni.$off('eventUnread')
    uni.$off('showBidRulesModal')
})

// 出价规则弹窗确认处理
function onBidRulesConfirm() {
    bidRulesModalVisible.value = false
}
//关闭公告弹窗
const onConfirm = () => {
    uni.setStorageSync('auctionNotice', item.value)
    popupShowRef.value.close()
}
//获取用户信息
const userId = computed(() => {
    return userStore.user.id
})

watch(
    () => userId.value,
    (val) => {
        if (val) {
            init('-1_auction')
        }
    }
)

const announceContent = computed(() => {
    //如果长度大于40，截取前40个字符
    if (item.value && item.value.content && item.value.content.length > 18) {
        return item.value.content.slice(0, 18) + '...'
    } else if (item.value && item.value.content) {
        return item.value.content
    }
    return ''
})

const onAuctionItem = computed(() => {
    return auctionStore.list.find((item) => item.status === '拍卖中') || null
})

const init = async (name: string) => {
    console.log('Init')

    chatStore.SetCurrentChatId(-1, 'auction', true)
    await loadHistoryMessage(true)
}

function sub(e: AuctionItemDto) {
    const msgId = 'ZuYTYzw2cM0LVhF5ybH5iATMaDl6lZ82OC6cczsglEA'
    uni.requestSubscribeMessage({
        tmplIds: [msgId],
        success: (res) => {
            // console.log(res);
        },
        fail(res) {
            console.log('requestSubscribeMessage fail', res)
        },
        complete: (res: any) => {
            console.log(res)
            if (res[msgId] !== 'reject') {
                auctionStore.startNotify(e.id!).then(() => {
                    Tips.success('订阅成功')
                })

                // chatStore.sendChannelMsg('订阅成功', '', ChatMessageType.Text).then(() => {})
            } else {
                Tips.info('请允许接受通知')
            }
        },
    })
}

const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    if (force) {
        await chatStore.joinChannel('-1_auction')
    }

    console.log('loadHistoryMessage')

    chatRef.value!.history.loading = true

    let lastTime = new Date().getTime()
    if (!force) {
        if (historyMsgs.value && historyMsgs.value.length) {
            lastTime = historyMsgs.value[0].time!
        }
    }
    await chatStore.getGroupHistory('-1_auction', lastTime, force).then((res) => {
        chatRef.value!.history.loading = false
        if (res.length < 20) {
            chatRef.value!.history.allLoaded = true
        }
    })
}

//LINK[epic=消息发送] - 拍卖消息发送逻辑
function send(e: { type: ChatMessageType; data: string | object }) {
    if (e.type === ChatMessageType.Image) {
        chatStore.sendChannelMsg('[图片]', '', ChatMessageType.Image, e.data).then(() => {})
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendChannelMsg(e.data as string, '', ChatMessageType.Text).then(() => {
            //
        })
    }
}

function showGoods() {
    popupRef.value.open()
}

function doPayment(
    params: { amount: number; type: string; from: string },
    callback: { success: () => void; fail: () => void }
) {
    api.client
        .payDeposit({ openid: userStore.openid, amount: params.amount })
        .then((res: any) => {
            wx.requestPayment({
                provider: 'wxpay',
                timeStamp: `${res.timeStamp}`,
                nonceStr: res.nonceStr,
                package: res.package,
                signType: res.signType,
                paySign: res.paySign,
                success: async (res) => {
                    console.log('支付成功:', JSON.stringify(res))

                    // 清除支付状态
                    uni.removeStorageSync('depositStatus')

                    // 更新用户信息
                    try {
                        await userStore.checkLogin(false, true)
                        console.log('用户信息更新成功')
                    } catch (error) {
                        console.error('更新用户信息失败:', error)
                    }

                    callback.success()
                    Tips.success('支付成功，保证金已到账')

                    // 支付成功后，询问用户是否立即出价
                    setTimeout(() => {
                        uni.showModal({
                            title: '支付成功',
                            content: '保证金已到账，是否立即出价？',
                            showCancel: true,
                            confirmText: '立即出价',
                            cancelText: '稍后出价',
                            success: (modalRes) => {
                                if (modalRes.confirm) {
                                    // 延迟一下再调用出价，确保用户信息已更新
                                    setTimeout(() => {
                                        bid()
                                    }, 500)
                                }
                            },
                        })
                    }, 1500)
                },
                fail: (err) => {
                    console.log('支付失败:', JSON.stringify(err))

                    // 清除支付状态
                    uni.removeStorageSync('depositStatus')

                    callback.fail()
                    Tips.info('用户取消支付')
                },
            })
        })
        .catch((error) => {
            console.error('获取支付参数失败:', error)

            // 清除支付状态
            uni.removeStorageSync('depositStatus')

            callback.fail()
            Tips.error('获取支付参数失败，请重试')
        })
}

//出价
async function bid() {
    const userId = userStore.user.id
    console.log('开始出价流程 - 用户ID:', userId)

    try {
        // 首先获取当前拍卖商品ID
        if (!onAuctionItem.value || !onAuctionItem.value.id) {
            console.log('没有正在拍卖的商品')
            Tips.error('没有正在拍卖的商品')
            return
        }

        const auctionItemId = onAuctionItem.value.id
        console.log('当前拍卖商品ID:', auctionItemId)

        // 获取实时的拍卖商品信息
        console.log('正在获取实时拍卖商品信息...')
        const auctionItemDetail = await api.auctionItem.getDetail(auctionItemId)
        console.log('拍卖商品详情获取成功:', auctionItemDetail)

        // 验证商品状态
        if (auctionItemDetail.status !== '拍卖中') {
            console.log('商品不在拍卖中，状态:', auctionItemDetail.status)
            Tips.error('商品不在拍卖中')
            return
        }

        // 从商品详情中获取卡秒状态
        const isKasecMode = !!auctionItemDetail.isKasec
        console.log('卡秒状态:', isKasecMode)

        // 同步到store（可选，用于UI显示）
        auctionStore.isKasec = isKasecMode

        // 获取实时用户信息
        console.log('正在获取实时用户信息...')
        const currentUser = await api.user.get({ id: userId })
        console.log('用户信息获取成功:', currentUser)
        const deposit = currentUser?.depositBalance || 0
        console.log('用户信息获取成功:', {
            userId: currentUser?.id,
            userName: currentUser?.userName,
            depositBalance: deposit,
            isActive: currentUser?.isActive,
        })

        // 获取用户等级信息
        console.log('正在获取用户等级信息...')
        const levelResponse = await api.userGroupLevel.getUserLevelInfo(userId!)
        const levelInfo = levelResponse.data
        const userLevel = levelInfo?.levelSettings?.level ?? 0
        const cumulativeAmount = levelInfo?.userLevel?.cumulativeAmount ?? 0
        console.log('用户等级信息:', {
            userLevel,
            cumulativeAmount,
            levelSettings: levelInfo?.levelSettings,
            userLevelInfo: levelInfo?.userLevel,
        })

        // 新用户且保证金不足的情况
        if (userLevel === 0 && deposit < 50) {
            console.log('新用户保证金不足:', { userLevel, deposit })
            // 先显示一个提示
            uni.showToast({
                title: '新用户需要缴纳保证金',
                icon: 'none',
                duration: 2000,
            })

            console.log('准备显示保证金弹窗...')
            try {
                await new Promise((resolve, reject) => {
                    uni.showModal({
                        title: '出价须知',
                        content:
                            '新用户参与拍卖，需要缴纳51元（50元保证金+1元提现手续费）\n老用户回归参与拍卖，需向拍卖师-老淡，提供以往QQ群成交聊天记录截图',
                        showCancel: true,
                        confirmText: '去缴纳',
                        cancelText: '提供记录',
                        success: (res) => {
                            console.log('保证金弹窗结果:', res)
                            if (res.confirm) {
                                // 保存状态
                                const depositStatus = {
                                    status: 'pending',
                                    timestamp: new Date().getTime(),
                                    userId: userStore.user.id,
                                    from: 'auction',
                                }
                                uni.setStorageSync('depositStatus', depositStatus)

                                // 直接跳转到保证金支付页面
                                doPayment(
                                    {
                                        amount: 51,
                                        type: 'deposit',
                                        from: 'auction',
                                    },
                                    {
                                        success: () => {
                                            console.log('支付成功')
                                        },
                                        fail: () => {
                                            console.log('支付失败')
                                        },
                                    }
                                )
                            } else if (res.cancel) {
                                navigateToAdminChat('record_provided')
                            }
                            resolve(res)
                        },
                        fail: (err: any) => {
                            console.error('保证金弹窗失败:', err)
                            reject(err)
                        },
                    })
                })
            } catch (error) {
                console.error('显示保证金弹窗出错:', error)
                Tips.error('显示弹窗失败，请重试')
            }
            return
        }

        // 满足条件，弹出原有出价输入框

        // 使用工具方法计算最低出价（基于实时获取的商品信息）
        const minPrice = calculateMinBidPrice(auctionItemDetail.currentPrice || auctionItemDetail.startingPrice, isKasecMode)

        let title = '出价'
        let placeholderText = `请输入出价金额(最低出价${minPrice})`

        if (isKasecMode) {
            title = '卡秒出价'
            placeholderText = `卡秒模式-需三倍加价(最低出价${minPrice})`
        }

        console.log('准备显示出价弹窗...')
        // 先显示一个提示
        uni.showToast({
            title: '请输入出价金额',
            icon: 'none',
            duration: 2000,
        })

        try {
            await new Promise((resolve, reject) => {
                uni.showModal({
                    title: title,
                    content: '', // 清空content，避免作为默认值显示在输入框中
                    editable: true,
                    placeholderText: placeholderText,
                    success: (res) => {
                        console.log('出价弹窗结果:', res)
                        if (res.confirm) {
                            const value = Number(res.content)
                            if (!value) {
                                Tips.noCancelModal('请输入数字')
                                return
                            }
                            console.log('用户输入出价金额:', value)
                            auctionStore.bid(auctionItemId, value)
                        }
                        resolve(res)
                    },
                    fail: (err: any) => {
                        console.error('出价弹窗失败:', err)
                        reject(err)
                    },
                })
            })
        } catch (error) {
            console.error('显示出价弹窗出错:', error)
            Tips.error('显示弹窗失败，请重试')
        }
    } catch (error) {
        console.error('出价过程发生错误:', error)
        uni.showToast({
            title: '获取商品信息失败，请稍后重试',
            icon: 'none',
        })
    }
}

function showonAuctionDetail() {
    if (!onAuctionItem) return
    showDetail(onAuctionItem.value!)
}

function showDetail(e: AuctionItemDto) {
    console.log('showDetail', e)
    showItem.value = convertFields(e)
    popup.value.open('bottom')
}
//检测首字母是否大写
const convertFields = (obj: any) => {
    const newObj: any = {}
    Object.keys(obj).forEach((key) => {
        const firstChar = key.charAt(0)
        if (firstChar === firstChar.toUpperCase()) {
            const newKey = firstChar.toLowerCase() + key.slice(1)
            newObj[newKey] = obj[key]
        } else {
            newObj[key] = obj[key]
        }
    })
    return newObj
}

function showImgPreview(url: string) {
    url = getImgUrl(url, false)
    uni.previewImage({
        current: url, // 当前显示图片的http链接
        urls: [url], // 需要预览的图片http链接列表
    })
}

const showItem = ref<AuctionItemDto | null>(null)
const popup = ref(null as any)

function popChange(e: { show: boolean; type: string }) {
    console.log(e)
    if (e.show === false) {
        showItem.value = null
    }
}

function getStartContent(item: AuctionItemDto) {
    const description = item.description
    if (!description || description.trim() === '') {
        // 与 PC 端保持一致，显示拍品图片
        return `<div class="flex justify-center py-4">
            <img
                src="${item.imageUrl}"
                class="w-full h-48 object-cover rounded cursor-pointer"
                alt="${item.name}"
            />
        </div>`
    }
    return `<div>${description}</div>`
}

function catchImage(e: any) {
    console.log('catchImage', e)
    try {
        const description = showItem.value?.description
        if (!description) return
        const list = []
        //从 string中img标签中获取data-url的属性放入数组中
        const reg = /<img.*?data-url=['"](.*?)['"].*?>/g
        let result
        while ((result = reg.exec(description)) !== null) {
            list.push(result[1])
        }

        if (list.length === 0) return
        uni.previewImage({
            current: list[0], // 当前显示图片的http链接
            urls: list, // 需要预览的图片http链接列表
        })

        console.log('catchImage', list)
    } catch (e) {
        console.log('catchImage', e)
    }
}

// 添加路由跳转函数
const navigateToAdminChat = (status: 'pending' | 'record_provided') => {
    // 保存状态
    const depositStatus = {
        status,
        timestamp: new Date().getTime(),
        userId: userStore.user.id,
        from: 'auction',
    }
    uni.setStorageSync('depositStatus', depositStatus)

    // 构建路由参数
    const params = {
        id: 14,
        name: encodeURIComponent('管理员'),
        avatar: encodeURIComponent('/images/admin_avatar.png'),
        from: 'auction',
        status,
    }

    // 构建完整URL
    const url = `/pages/chat/privateChat?id=${params.id}&name=${params.name}&avatar=${params.avatar}&from=${params.from}&status=${params.status}`

    // 执行跳转
    uni.navigateTo({
        url,
        success: () => {
            console.log('跳转到管理员私信页面成功', { params })
        },
        fail: (err: any) => {
            console.error('跳转失败:', err)
            Tips.error('跳转失败，请重试')
            // 清除状态
            uni.removeStorageSync('depositStatus')
        },
    })
}
</script>
<style lang="scss" scoped>
.message-notification {
    position: fixed;
    right: 0;
    top: 140rpx;
    z-index: 100;
    transform: translateX(100%);
    transition: transform 0.3s ease-in-out;
}

.message-notification.show {
    transform: translateX(0);
}
</style>
<style lang="scss" scoped>
.popup-content {
    background-color: #fff;
    border-radius: 10px;
    padding: 20px;
    width: 80%;
    max-width: 300px;
}

::v-deep .uv-safe-bottom {
    display: none;
}

.popup-image {
    width: 100%;
    height: 200px;
}

.popup-title {
    font-size: 18px;
    font-weight: bold;
    margin-top: 10px;
    text-align: center;
    display: block;
}

.popup-text {
    margin-top: 10px;
    text-align: center;
    display: block;
    overflow-y: auto;
    max-height: 120px;
}

.popup-view {
    display: flex;
}

.popup-button {
    margin-top: 20px;
    width: 100px;
}
</style>
<route lang="json">
{
    "style": {
        "navigationBarTitleText": "拍卖行"
    }
}
</route>
