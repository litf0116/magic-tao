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
        <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage" @showDetail="showDetail">
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
                @tap="catchImage"
                v-html="getStartContent(showItem!)"
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
</template>

<script setup lang="ts">
import chatMain from '@/components/chat/chatMain.vue'
import { getImgUrl } from '@/composables'
import type { AnnounceDto, AuctionItemDto } from '@/composables/types'
import auctionList from '@/components/chat/auctionList.vue'
import api from '@/utils/api'
import { onLoad, onShow, onReady } from '@dcloudio/uni-app'
import { ChatMessageType } from '@/composables/types'

// import AuctionList from '@/components/chat/AuctionList.vue'
const chatStore = useChatStore()
const userStore = useUserStore()
const auctionStore = useAuctionStore()
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)
const popupRef = ref(null as any)
//公告信息跟弹窗
const item = ref<AnnounceDto | null>(null)
const popupShowRef = ref(null as any)

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
})

// 组件销毁时移除监听
onUnmounted(() => {
    uni.$off('eventUnread')
})
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
//出价
function bid() {
    auctionStore.getList().then(() => {
        if (!onAuctionItem.value) {
            Tips.error('没有正在拍卖的商品')
            return
        }

        let minPrice = 0
        if (onAuctionItem.value.currentPrice) {
            // 算法：
            // 100以内，1R一加
            // 100~1000，5R一加
            // 1000-2000，10R一加
            // 2000-5000，20R一加
            // 50000-1W，50一加
            // 1W以上，100一加
            if (onAuctionItem.value.currentPrice < 100) {
                minPrice = onAuctionItem.value.currentPrice + 1
            } else if (onAuctionItem.value.currentPrice < 1000) {
                minPrice = onAuctionItem.value.currentPrice + 5
            } else if (onAuctionItem.value.currentPrice < 2000) {
                minPrice = onAuctionItem.value.currentPrice + 10
            } else if (onAuctionItem.value.currentPrice < 5000) {
                minPrice = onAuctionItem.value.currentPrice + 20
            } else if (onAuctionItem.value.currentPrice < 10000) {
                minPrice = onAuctionItem.value.currentPrice + 50
            } else {
                minPrice = onAuctionItem.value.currentPrice + 100
            }
        }

        uni.showModal({
            title: `请输入出价金额(最低出价${minPrice})`,
            // content: minPrice ? minPrice.toString() : '',
            content: '',
            editable: true,
            placeholderText: '请输入出价金额',
            success: (res) => {
                if (res.confirm) {
                    const value = Number(res.content)
                    if (!value) {
                        Tips.noCancelModal('请输入数字')
                        return
                    }
                    // if (value < minPrice) {
                    //     Tips.noCancelModal(
                    //         '100以内，1R一加。100~1000，5R一加。1000-2000，10R一加。2000-5000，20R一加。50000-1W，50一加。1W以上，100一加',
                    //         '不能低于最低出价'
                    //     )
                    //     return
                    // }
                    auctionStore.bid(onAuctionItem.value!.id!, value)
                }
            },
        })
    })
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
    return `<div>${item.description}</div>`
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
