<template>
    <view class="chatInterface" @contextmenu.prevent="">
        <view id="scrollview" class="scroll-view">
            <image v-if="history.loading" class="history-loaded" src="/static/images/loading.svg" />
            <view v-else :class="history.allLoaded ? 'history-loaded' : 'load'" @click="loadHistoryMessage(false)">
                <view>{{ history.allLoaded ? '已经没有更多的历史消息' : '点击获取历史消息' }}</view>
            </view>

            <view v-for="(message, index) in historyMsgs" :key="message.id">
                <!--时间显示，类似于微信，隔5分钟不发言，才显示时间-->
                <view class="time-lag">
                    {{ renderMessageDate(message, index) }}
                </view>
                <view
                    class="message-item"
                    :class="{
                        'system-center':
                            message.type === ChatMessageType.Welcome ||
                            message.type === ChatMessageType.BanUser ||
                            message.type === ChatMessageType.Backout,
                    }"
                >
                    <WelcomeMessage v-if="message.type === ChatMessageType.Welcome" :fromName="message.fromName" />
                    <SystemMessage
                        v-else-if="message.type === ChatMessageType.BanUser"
                        :type="'BanUser'"
                        :msg="message.msg"
                    />
                    <SystemMessage
                        v-else-if="message.type === ChatMessageType.Backout"
                        :type="'Backout'"
                        :msg="message.msg"
                    />

                    <view v-else class="message-item-content" :class="{ self: message.from === userStore.user.id }">
                        <view class="avatar" @tap="showActionPopup(message)">
                            <image :src="getImgUrl2(message.avatar, true)" mode="aspectFill"></image>
                        </view>

                        <view class="content">
                            <!-- 如果当前群聊是私聊 则不展示用户等级信息 -->
                            <div
                                class="message-fromName"
                                style="display: flex"
                                :class="[message.fromAdmin ? ' !text-red-500' : '']"
                                @tap="showGroupChatRules = true"
                            >
                                <div v-if="chatOptions?.chatType === 'private'">
                                    <!-- 私有聊天是特有的 标签显示规则 只显示自己的标签 他人的标签不显示-->
                                    <span
                                        v-if="message.fromAdmin && message.fromTag"
                                        :class="[message.tagClass ? message.tagClass : '']"
                                        >{{ message.fromTag }}
                                    </span>
                                </div>
                                <div v-else>
                                    <span
                                        v-if="message.fromAdmin && message.fromTag"
                                        :class="[message.tagClass ? message.tagClass : '']"
                                        >{{ message.fromTag }}
                                    </span>
                                    <div
                                        v-else-if="message.userChatLevel"
                                        class="tag_AuctionManager"
                                        :style="{
                                            background: `linear-gradient(90deg,${message.userChatLevel.borderColor},${message.userChatLevel.rightBorderColor})`,
                                        }"
                                        style="margin-right: 5px; color: #fff"
                                    >
                                        {{ message.userChatLevel.name }}
                                    </div>
                                </div>
                                {{ message.fromName }}
                            </div>
                            <view class="message-payload mt-1">
                                <!-- 文本 -->
                                <TextMessage
                                    v-if="message.type === ChatMessageType.Text"
                                    :message="message"
                                    @tap="goShowDetails(message)"
                                />
                                <!-- 图片 -->
                                <ImageMessage
                                    v-else-if="message.type === ChatMessageType.Image"
                                    :message="message"
                                    :showImageFullScreen="showImageFullScreen"
                                    :showActionPopup="showActionPopup"
                                />

                                <!-- 开始拍卖 -->
                                <AuctionStartMessage
                                    v-else-if="message.type === ChatMessageType.AuctionStart && message.payload"
                                    :message="message"
                                    :catchImage="catchImage"
                                    @action="onAuctionStartAction"
                                />

                                <!-- 出价 -->
                                <AuctionBidMessage
                                    v-else-if="message.type === ChatMessageType.AuctionBid && message.payload"
                                    :message="message"
                                    @action="onAuctionBidAction"
                                />

                                <!-- 公布得主 -->
                                <AuctionEndMessage
                                    v-else-if="message.type === ChatMessageType.AuctionEnd && message.payload"
                                    :message="message"
                                    @action="onAuctionEndAction"
                                />
                                <!-- 交易通知 -->
                                <AuctionDealMessage
                                    v-else-if="message.type === ChatMessageType.AuctionDeal"
                                    :message="message"
                                    @action="onAuctionDealAction"
                                />
                                <!-- 卡秒状态变更消息 -->
                                <KasecStatusMessage
                                    v-if="message.type === ChatMessageType.KasecStatusChanged"
                                    :message="message"
                                    @tap="goShowDetails(message)"
                                />
                            </view>
                        </view>
                    </view>
                </view>
            </view>
            <view class="h-12"></view>
        </view>
        <view class="action-box">
            <view class="action-top">
                <input
                    v-model="text"
                    class="consult-input"
                    maxlength="700"
                    placeholder="发送消息"
                    type="text"
                    @confirm="sendTextMessage"
                />
                <view @click="switchEmojiKeyboard">
                    <image v-if="emoji.visible" class="more" src="/static/images/jianpan.png"></image>
                    <image v-else class="more" src="/static/images/emoji.png"></image>
                </view>
                <view>
                    <image class="more" src="/static/images/more.png" @click="showOtherTypesMessagePanel()" />
                </view>
                <view v-if="text" class="send-btn-box">
                    <text class="btn" @click="sendTextMessage">发送</text>
                </view>
            </view>
            <view v-if="emoji.visible" class="action-bottom action-bottom-emoji">
                <view class="flex flex-col">
                    <view class="flex items-center">
                        <view
                            class="px-4 py-1"
                            :class="[emojiIndex === 0 ? 'bg-#f4835a text-white' : ' text-gray-600']"
                            @click="emojiIndex = 0"
                            >系统</view
                        >
                        <view
                            class="px-4 py-1"
                            :class="[emojiIndex === 1 ? 'bg-#f4835a text-white' : ' text-gray-600']"
                            @click="emojiIndex = 1"
                            >收藏</view
                        >
                    </view>
                    <view v-if="emojiIndex === 0" class="max-h-300rpx overflow-y-scroll pb-8">
                        <image
                            v-for="(emojiItem, key, index) in emoji.map"
                            :key="index"
                            class="emoji-item"
                            :src="emoji.url + emojiItem"
                            @click="chooseEmoji(key)"
                        ></image>
                    </view>
                    <view v-else class="max-h-300rpx overflow-y-scroll pb-8">
                        <image
                            v-for="(emojiItem, index) in emojiStore.userEmoji"
                            :key="index"
                            class="!size-16 mr-2"
                            mode="aspectFit"
                            :src="getImgUrl2(emojiItem.url, true)"
                            :data-url="getImgUrl2(emojiItem.url, false)"
                            @tap.stop="chooseUserEmoji(emojiItem)"
                            @longpress.stop="emojiStore.removeEmoji(emojiItem)"
                        ></image>
                    </view>
                </view>
            </view>
            <!--其他类型消息面板-->
            <view v-if="otherTypesMessagePanelVisible" class="action-bottom relative">
                <view class="absolute right-1 top-1" @click="otherTypesMessagePanelVisible = false">
                    <view class="size-6 text-red i-mdi:close"></view>
                </view>
                <view class="more-icon">
                    <image class="operation-icon" src="/static/images/picture.png" @click="sendImageMessage2()"></image>
                    <view class="operation-title">图片</view>
                </view>
            </view>
        </view>

        <uv-popup ref="userInfoDialogRef" mode="top">
            <view v-if="viewUserInfoId" class="min-h-20vh">
                <userProfile :userId="viewUserInfoId" />
            </view>
        </uv-popup>

        <view v-if="actionPopup.visible" class="action-popup" @touchmove.stop.prevent>
            <view class="layer"></view>
            <view class="action-list">
                <div v-if="actionPopup.isImg" class="action-item" @click="addEmoji()">收藏至表情</div>
                <div
                    v-if="userStore.isChatAdmin || (selectMessage?.from && selectMessage.from === userStore.user.id)"
                    class="action-item"
                    @click="backout()"
                >
                    撤销
                </div>
                <div
                    v-if="
                        (userStore.isChatAdmin || (selectMessage && selectMessage.fromAdmin)) &&
                        selectMessage?.from &&
                        selectMessage.from !== userStore.user.id
                    "
                    class="action-item"
                    @click="adminSend()"
                >
                    私聊
                </div>
                <div
                    v-if="selectMessage?.from && selectMessage.from !== userStore.user.id"
                    class="action-item"
                    @click="addFriend()"
                >
                    加为好友
                </div>
                <div
                    v-if="selectMessage?.from && selectMessage.from !== userStore.user.id"
                    class="action-item"
                    @click="viewUserInfo(selectMessage.from)"
                >
                    查看资料
                </div>
                <div v-if="userStore.isChatAdmin" class="action-item" @click="ban()">禁言</div>
                <div class="action-item" @click="actionPopup.visible = false">取消</div>
            </view>
        </view>
        <!-- 群聊等级规则 -->
        <view v-if="showGroupChatRules" class="action-popup" @touchmove.stop.prevent>
            <view class="action-list" style="width: 275px; padding: 10px; color: #fff">
                <div>群等级制度，根据成交价金额自动累计</div>
                <div v-for="x in groupChatLevel" :key="x.level">
                    {{ x.level }}级:消费满{{ x.amountRequired }} {{ x.name }}
                </div>
                <div class="action-item" @click="showGroupChatRules = false">取消</div>
            </view>
        </view>

        <uv-popup ref="popupDetailRef">
            <view v-if="showItem" class="p-4">
                <view class="flex flex-row items-center overflow-hidden cursor-pointer">
                    <view class="text-wrap px-2 flex-1 flex flex-col">
                        <view class="text-[#ff7144] line-clamp-3">{{ showItem.Name }}</view>
                        <div
                            @tap="catchImage($event, showItem)"
                            v-html="showItem.description || showItem.Description"
                        ></div>
                    </view>
                </view>
            </view>
        </uv-popup>
    </view>
</template>

<script setup lang="ts">
import userProfile from './userProfile.vue'
import AuctionEndMessage from './AuctionEndMessage.vue'
import AuctionStartMessage from './AuctionStartMessage.vue'
import AuctionBidMessage from './AuctionBidMessage.vue'
import AuctionDealMessage from './AuctionDealMessage.vue'
import KasecStatusMessage from './KasecStatusMessage.vue'
import WelcomeMessage from './WelcomeMessage.vue'
import SystemMessage from './SystemMessage.vue'
import TextMessage from './TextMessage.vue'
import ImageMessage from './ImageMessage.vue'
import { upload } from '@/utils/upload'
import dayjs from 'dayjs'
import { useDebounceFn } from '@vueuse/core'
import { useEventBus } from '@vueuse/core'
import api from '@/utils/api'
import { orderBy, uniqBy, last } from 'lodash'
import { onLoad, onUnload } from '@dcloudio/uni-app'
import { ChatListItemType, ChatMessageType, type ChatEmojiDto, type ChatMessage } from '@/composables/types'
import type { AuctionItemDto } from '@/composables/types'
import { getImgUrl as getImgUrl2, Tips } from '@/composables'
import type { ChatOptions } from './types'
import { computed, defineEmits, defineProps, reactive, ref, watch } from 'vue'
import { Goto } from '@/composables/goto'
import { convertAuctionPayload } from '@/utils/propertyConverter'
import { convertImageUrl } from '@/utils/imageUrlConverter'

// 接收 options 属性
const props = defineProps<{
    options: ChatOptions
}>()

// 使用 options 属性
const chatOptions = computed(() => props.options)

const showItem: any = ref<AuctionItemDto | null>(null)
const popupDetailRef = ref(null as any)

const emit = defineEmits(['loadHistoryMessage', 'onSend', 'showDetail'])

const isLoadingHistory = ref(false)
//是否展示'其他消息类型面板'
const otherTypesMessagePanelVisible = ref(false)

function loadHistoryMessage(isScrollToBottom: boolean) {
    isLoadingHistory.value = true
    emit('loadHistoryMessage', isScrollToBottom)
}

const emojiStore = useChatEmojiStore()
const chatStore = useChatStore()
const userStore = useUserStore()
const auctionStore = useAuctionStore()
const recorderManager = uni.getRecorderManager()
//显示群聊规则
const showGroupChatRules = ref(false)
//群聊等级信息
const groupChatLevel: any = ref([])
const emojiIndex = ref(0)

onLoad(async () => {
    delayloadhistory()
    scrollToBottom(true)
    //获取群等级信息
    var res: any = await api.groupChatLevelSettings.getList()
    if (res) {
        groupChatLevel.value = res
    }
})

onUnload(() => {
    // console.log('ChatMain onUnload')
    // 发送事件通知
    uni.$emit('refreshView')
    unsubscribe()
    clearInterval(timeId)
})

let timeId: any = null

// LINK - 延迟加载服务器最新消息,不同则重连服务器
function delayloadhistory() {
    // console.log('delayloadhistory')
    clearInterval(timeId)
    timeId = setInterval(() => {
        // console.log('delayloadhistory执行')
        //检查最后一条消息是否是historyMsgs的最后一条
        chatStore.getServerLastId().then((res) => {
            const hisLast = last(
                historyMsgs.value.filter((x) => x.type !== 'Welcome' && x.type !== 'BanUser' && x.type !== 'Backout')
            )
            // console.log('chatStore.getServerLastId', res, hisLast)
            if (res != api.guid && res !== hisLast?.id) {
                // console.log('服务器消息与本地不同')
                chatStore.connectServer(true).then(() => {
                    loadHistoryMessage(true)
                    if (chatStore.currentChat.id === -1) {
                        auctionStore.getList()
                    }
                })
            } else {
                // console.log('服务器消息与本地相同')
            }
        })
    }, 20_000)
}

const bus = useEventBus(onmessageKey)
//LINK[epic=处理收到消息] - ChatMain处理收到消息
const unsubscribe = bus.on((msg: any) => {
    // console.log('chatMain onmessageKey', msg)
    // scrollToBottom()
    // console.log('msg', msg)
    if (msg.type === 'AuctionStart' || msg.type === 'AuctionEnd') {
        auctionStore.getList()
    }
    if (msg.type === 'AuctionBid') {
        // auctionStore.getList()
    }

    if (msg.type === 'Backout' && msg.chan === '-1_auction') {
        // console.log('拍卖行撤回')
        auctionStore.getList()
    }

    if (msg.type === 'Backout') {
        // console.log('Backout', msg)
        if (msg.id) {
            chatStore.removeMessage(msg.id)
        }
    }
})

const history = ref<{ allLoaded: boolean; loading: boolean }>({
    // messages: [],
    allLoaded: false,
    loading: true,
})
defineExpose({ history })

const historyMsgs: any = computed(() => {
    return [...orderBy(uniqBy(chatStore.chatMap.get(`${chatStore.currentChat.id}`) || [], 'id'), ['time'], ['asc'])]
})

const text = ref('')
//定义表情列表
const emoji = ref({
    url: emojiStore.emojiUrl,
    map: emojiStore.emojiMap,
    visible: false,
    decoder: new emojiDecoder(emojiStore.emojiUrl, emojiStore.emojiMap),
})
const audio = reactive({
    startTime: null,
    //语音录音中
    recording: false,
    //录音按钮展示
    visible: false,
})

let audioPlayer = reactive({
    innerAudioContext: null,
    audio: {},
    playingMessage: null,
})

// 展示消息删除弹出框
const actionPopup = ref({
    visible: false,
    message: null,
    recallable: false,
    isImg: false,
})

function catchImage(e: any, payload: any) {
    try {
        const convertedPayload = convertAuctionPayload(payload)
        const { description } = convertedPayload
        if (!description) {
            return
        }
        const list = []
        //从 string中img标签中获取data-url的属性放入数组中
        const reg = /<img.*?data-url=['"](.*?)['"].*?>/g
        let result
        while ((result = reg.exec(description)) !== null) {
            // 对每个图片URL进行转换处理
            const convertedUrl = convertImageUrl(result[1])
            list.push(convertedUrl)
        }

        if (list.length === 0) return
        wx.previewImage({
            current: list[0], // 当前显示图片的http链接
            urls: list, // 需要预览的图片http链接列表
        })

        // console.log('catchImage', list)
    } catch (e) {
        // console.log('catchImage', e)
    }
}

//语音录制按钮和键盘输入的切换

function switchAudioKeyboard() {
    audio.visible = !audio.visible
    if (uni.authorize) {
        uni.authorize({
            scope: 'scope.record',
            fail: () => {
                uni.showModal({
                    title: '获取录音权限失败',
                    content: '请先授权才能发送语音消息！',
                })
            },
        })
    }
}

function onRecordStart() {
    try {
        recorderManager.start({})
    } catch (e) {
        uni.showModal({
            title: '录音错误',
            content: '请在app和小程序端体验录音，Uni官方明确H5不支持getRecorderManager, 详情查看Uni官方文档',
        })
    }
}

function onRecordEnd() {
    try {
        recorderManager.stop()
    } catch (e) {
        // console.log(e)
    }
}

function showImageFullScreen(e: any) {
    let imagesUrl = [e.currentTarget.dataset.url]
    uni.previewImage({
        urls: imagesUrl,
    })
}

watch(
    () => historyMsgs.value.length,
    () => {
        const l = historyMsgs.value.length
        delayloadhistory()
        if (l > 5) {
            // console.log('watchEffect', historyMsgs.value.length)
            scrollToBottom(false)
        }
    }
)
// LINK - 滚动到底部
function scrollToBottom(t = true) {
    // console.log('scrollToBottom')
    // nextTick(() => {})
    let query = uni.createSelectorQuery()
    query.selectViewport().scrollOffset()
    query.selectViewport().boundingClientRect()
    query.exec((res: any) => {
        if (!t) if (res[0].scrollTop + res[1].height < res[0].scrollHeight - 200) return
        setTimeout(() => {
            uni.pageScrollTo({
                scrollTop: 2_000_000,
                duration: 0,
            })
        }, 100)
    })
}

//解析消息
// function renderTextMessage(message: ChatMessage) {
// 	var html = `<div class="text-content">`;
// 	if (typeof message.payload === 'string') message.payload = JSON.parse(message.payload!)
// 	if (message.payload.Status != undefined || message.payload.Status === 4 || message.payload.status === '已成交') {
// 		html += `<div class="text-sm">商品名称: ${message.payload.name === undefined ? message.payload.Name : message.payload.name}</div>
//         <div>${message.payload.description === undefined ? message.payload.Description : message.payload.description}</div>`
// 	}
// 	if (message && message.msg) {
// 		return (
// 			html += '<span >' + emoji.value.decoder.decode(message.msg.replaceAll('\n', '<br/>')) + '</span>'
// 		)
// 	}
// 	return '</div>'
// }
function renderTextMessage(message: ChatMessage) {
    if (message && message.msg) {
        return (
            '<span class="text-content">' +
            emoji.value.decoder.decode(message.msg.replaceAll('\n', '<br/>')) +
            '</span>'
        )
    }

    return ''
}

function renderMessageDate(message: ChatMessage, index: number) {
    if (index === 0) {
        return dayjs(message.time).format('YYYY-MM-DD HH:mm:ss')
    } else {
        if (message.time! - historyMsgs.value[index - 1].time! > 3 * 60 * 1000) {
            return dayjs(message.time).format('YYYY-MM-DD HH:mm:ss')
        }
    }
    return ''
}

function showActionPopup(msg: ChatMessage, isImg = false) {
    //停止向上传播
    // e.stopPropagation()
    // console.log('showActionPopup', msg)
    actionPopup.value.visible = true
    actionPopup.value.isImg = isImg
    selectMessage.value = msg
}

function closeActionPopup() {
    actionPopup.value.visible = false
}

watch(
    () => actionPopup.value.visible,
    (newVal) => {
        if (newVal) {
        } else {
            selectMessage.value = null
            actionPopup.value.isImg = false
        }
    }
)

function chooseEmoji(emojiKey: string) {
    text.value += emojiKey
    emoji.value.visible = false
}
function chooseUserEmoji(e: ChatEmojiDto) {
    // console.log('chooseUserEmoji', e)
    emoji.value.visible = false

    emit('onSend', { type: ChatMessageType.Image, data: { url: e.url } })
}

function switchEmojiKeyboard() {
    emoji.value.visible = !emoji.value.visible
    otherTypesMessagePanelVisible.value = false
    if (emoji.value.visible) {
        emojiStore.reload()
    }
}

function showOtherTypesMessagePanel() {
    otherTypesMessagePanelVisible.value = !otherTypesMessagePanelVisible.value

    emoji.value.visible = false
}

function sendImageMessage2() {
    upload(1).then((res) => {
        // console.log('sendImageMessage2', res)
        emit('onSend', { type: ChatMessageType.Image, data: res })
        otherTypesMessagePanelVisible.value = false
    })
}

const sendTextMessage = useDebounceFn(() => {
    if (!text.value.trim()) {
        Tips.info('输入为空')
        text.value = ''
        return
    }
    emit('onSend', { type: ChatMessageType.Text, data: text.value })
    text.value = ''
}, 200)

const selectMessage = ref<ChatMessage | null>(null)

//ANCHOR - 添加好友
function addFriend() {
    // console.log(selectMessage.value)
    if (selectMessage.value) {
        api.userFriend.addFriend({ id: selectMessage.value.from }).then(() => {
            Tips.success('添加好友成功, 请等待对方同意')
            closeActionPopup()
        })
    }
}

const userInfoDialogRef = ref<null | any>(null)
const viewUserInfoId = ref(0)
// ANCHOR - 查看用户信息
function viewUserInfo(userId: number) {
    // console.log('viewUserInfo', userId)
    // dialogVisible.value = true
    viewUserInfoId.value = userId
    userInfoDialogRef.value.open()
    closeActionPopup()
}
//ANCHOR - 添加表情
function addEmoji() {
    emojiStore.addToEmoji(selectMessage.value!).then(() => {
        Tips.success('添加成功')
        closeActionPopup()
    })
}
//ANCHOR - 撤回消息
function backout() {
    // console.log(selectMessage.value)
    if (selectMessage.value && selectMessage.value.id) {
        api.ws.backout(selectMessage.value).then(() => {
            Tips.success('撤回成功')
            closeActionPopup()
        })
    }
}

//ANCHOR - 管理员私聊
function adminSend() {
    // console.log(selectMessage.value)
    if (selectMessage.value && selectMessage.value.from) {
        let chat = chatStore.chatList.find((item) => item.id === selectMessage.value!.from)

        if (!chat) {
            chat = {
                id: selectMessage.value.from!,
                name: selectMessage.value.fromName!,
                type: ChatListItemType.user,
                avatar: selectMessage.value.avatar!,
                unread: 0,
                order: 0,
            }
            chatStore.addChatList(chat.id!, chat.name, chat.avatar!)
        }
        chatStore.SetCurrentChat(chat)

        Goto.private({
            id: `${chat.id}`,
            name: chat.name,
            avatar: chat.avatar || convertImageUrl('https://cdn.molitao.top/avater.png'),
        })
        closeActionPopup()
    }
}

//ANCHOR - 禁言
function ban() {
    if (!selectMessage.value) return
    // console.log('ban', selectMessage.value)

    const chan = selectMessage.value.chan
    const userId = selectMessage.value.from as number
    if (selectMessage.value && selectMessage.value.from) {
        uni.showModal({
            title: '请输入禁言时间(分钟)',
            content: '60',
            editable: true,
            placeholderText: '请输入禁言时间(分钟)',
            success: (res) => {
                if (res.confirm) {
                    const value = Number(res.content)
                    if (!value) {
                        Tips.noCancelModal('请输入数字')
                        return
                    }
                    api.ws.banUser({ userId, minutes: Number(value), chan }).then(() => {
                        Tips.success('禁言成功')
                        closeActionPopup()
                    })
                }
            },
        })
    }
}

//显示详情
function showDetails(item: any) {
    // console.log('showDetails', item)
    if (typeof item.payload === 'string') {
        item.payload = JSON.parse(item.payload!)
    }
    emit('showDetail', item.payload)
}
//跳转到详情
function goShowDetails(item: any) {
    // console.log('goShowDetails', item)
    const convertedPayload = convertAuctionPayload(item.payload)
    if (convertedPayload.status != undefined && convertedPayload.status == '已成交') {
        popupDetailRef.value.open('bottom')
        showItem.value = convertedPayload
    }
}

async function onAuctionEndAction({ message, payload }: { message: any; payload: any }) {
    // console.log('onAuctionEndAction', { message, payload })

    try {
        // 转换payload，兼容老旧消息的PascalCase属性
        const convertedPayload = convertAuctionPayload(payload)

        // 从 payload 中获取拍品ID
        const auctionItemId = convertedPayload.id
        if (!auctionItemId) {
            // console.error('AuctionEnd payload 中缺少拍品ID')
            Tips.error('无法获取拍品信息')
            return
        }

        // 通过 API 获取完整的拍品信息
        // console.log('正在获取拍品详情，ID:', auctionItemId)
        const auctionItemDetail = await api.auctionItem.getDetail(auctionItemId)
        // console.log('拍品详情获取成功:', auctionItemDetail)

        // 调用 showDetail 显示详情
        showDetails({ message, payload: auctionItemDetail })
    } catch (error) {
        // console.error('获取拍品详情失败:', error)
        Tips.error('获取拍品详情失败，请重试')

        // 如果API调用失败，尝试使用转换后的payload数据（降级处理）
        // console.log('使用转换后的payload数据作为降级处理')
        const convertedPayload = convertAuctionPayload(payload)
        showDetails({ message, payload: convertedPayload })
    }
}

async function onAuctionStartAction({ message, payload }: { message: any; payload: any }) {
    // console.log('onAuctionStartAction', { message, payload })

    try {
        // 转换payload，兼容老旧消息的PascalCase属性
        const convertedPayload = convertAuctionPayload(payload)

        // 从 payload 中获取拍品ID
        const auctionItemId = convertedPayload.id
        if (!auctionItemId) {
            Tips.error('无法获取拍品信息')
            return
        }

        // 通过 API 获取完整的拍品信息
        // console.log('正在获取拍品详情，ID:', auctionItemId)
        const auctionItemDetail = await api.auctionItem.getDetail(auctionItemId)
        // console.log('拍品详情获取成功:', auctionItemDetail)

        // 调用 showDetail 显示详情
        showDetails({ message, payload: auctionItemDetail })
    } catch (error) {
        // console.error('获取拍品详情失败:', error)
        Tips.error('获取拍品详情失败，请重试')

        // 如果API调用失败，尝试使用转换后的payload数据（降级处理）
        // console.log('使用转换后的payload数据作为降级处理')
        const convertedPayload = convertAuctionPayload(payload)
        showDetails({ message, payload: convertedPayload })
    }
}

async function onAuctionBidAction({ message, payload }: { message: any; payload: any }) {
    // console.log('onAuctionBidAction', { message, payload })

    try {
        // 转换payload，兼容老旧消息的PascalCase属性
        const convertedPayload = convertAuctionPayload(payload)

        // 从 payload 中获取拍品ID
        const auctionItemId = convertedPayload.id
        if (!auctionItemId) {
            Tips.error('无法获取拍品信息')
            return
        }

        // 通过 API 获取完整的拍品信息
        // console.log('正在获取拍品详情，ID:', auctionItemId)
        const auctionItemDetail = await api.auctionItem.getDetail(auctionItemId)
        // console.log('拍品详情获取成功:', auctionItemDetail)

        // 调用 showDetail 显示详情
        showDetails({ message, payload: auctionItemDetail })
    } catch (error) {
        // console.error('获取拍品详情失败:', error)
        Tips.error('获取拍品详情失败，请重试')

        // 如果API调用失败，尝试使用转换后的payload数据（降级处理）
        // console.log('使用转换后的payload数据作为降级处理')
        const convertedPayload = convertAuctionPayload(payload)
        showDetails({ message, payload: convertedPayload })
    }
}

async function onAuctionDealAction({ message, payload }: { message: any; payload: any }) {
    // console.log('onAuctionDealAction', { message, payload })

    try {
        // 转换payload，兼容老旧消息的PascalCase属性
        const convertedPayload = convertAuctionPayload(payload)

        // 从 payload 中获取拍品ID
        const auctionItemId = convertedPayload.id
        if (!auctionItemId) {
            Tips.error('无法获取拍品信息')
            return
        }

        // 通过 API 获取完整的拍品信息
        // console.log('正在获取拍品详情，ID:', auctionItemId)
        const auctionItemDetail = await api.auctionItem.getDetail(auctionItemId)
        // console.log('拍品详情获取成功:', auctionItemDetail)

        // 调用 showDetail 显示详情
        showDetails({ message, payload: auctionItemDetail })
    } catch (error) {
        // console.error('获取拍品详情失败:', error)
        Tips.error('获取拍品详情失败，请重试')

        // 如果API调用失败，尝试使用转换后的payload数据（降级处理）
        // console.log('使用转换后的payload数据作为降级处理')
        const convertedPayload = convertAuctionPayload(payload)
        showDetails({ message, payload: convertedPayload })
    }
}
</script>

<style lang="scss" scoped>
.chatInterface {
    height: 100%;
    background-color: #faf1f0;

    .scroll-view {
        padding-left: 20rpx;
        padding-right: 20rpx;
        box-sizing: border-box;
        -webkit-overflow-scrolling: touch;
        padding-bottom: 150rpx;
        background-color: #faf1f0;

        .history-loaded {
            font-size: 24rpx;
            height: 60rpx;
            line-height: 60rpx;
            width: 100%;
            text-align: center;
            color: #cccccc;
        }

        .load {
            font-size: 24rpx;
            height: 60rpx;
            line-height: 60rpx;
            width: 100%;
            text-align: center;
            color: #d02129;
        }

        .message-item {
            display: flex;
            margin: 20rpx 0;

            .message-item-checkbox {
                height: 80rpx;
                display: flex;
                align-items: center;
            }

            .message-item-content {
                flex: 1;
                overflow: hidden;
                display: flex;
            }

            .message-item-content.self {
                overflow: hidden;
                display: flex;
                justify-content: flex-start;
                flex-direction: row-reverse;
            }

            .avatar {
                width: 80rpx;
                height: 80rpx;
                flex-shrink: 0;
                flex-grow: 0;

                image {
                    width: 100%;
                    height: 100%;
                }
            }
        }

        .content {
            font-size: 34rpx;
            line-height: 44rpx;
            margin: 0 20rpx;
            max-width: 520rpx;
        }

        .message-fromName {
            @apply text-xs text-gray-600;
        }

        .message-payload {
            display: flex;
            flex-direction: row;
            align-items: center;
        }

        .self .message-payload {
            justify-content: flex-end;
        }

        .self .message-fromName {
            justify-content: flex-end;
            text-align: right;
        }

        .content .pending {
            background: url('/static/images/pending.gif') no-repeat center;
            background-size: 30rpx;
            width: 30rpx;
            height: 30rpx;
            margin-right: 10rpx;
        }

        .content .send-fail {
            background: url('/static/images/failed.png') no-repeat center;
            background-size: 30rpx;
            width: 30rpx;
            height: 30rpx;
            margin-right: 10rpx;
        }
    }

    .action-box {
        display: flex;
        backdrop-filter: blur(0.27rpx);
        width: 100%;
        position: fixed;
        bottom: 0rpx;
        left: 0;
        right: 0;
        flex-direction: column;
        background-color: #f1f1f1;
        padding-bottom: 34rpx;
    }

    .action-top {
        display: flex;
        align-items: center;
        box-sizing: border-box;
        background: #f6f6f6;
        backdrop-filter: blur(27.1828px);
        border-top: 1px solid #ececec;
        padding: 0 20rpx;
    }

    .consult-input {
        flex: 1;
        height: 80rpx;
        padding-left: 20rpx;
        margin: 20rpx;
        margin-left: 0;
        border: none;
        outline: none;
        box-sizing: border-box;
        border-radius: 6px;
        background: #ffffff;
        font-size: 32rpx;
    }

    .more {
        width: 62rpx;
        height: 62rpx;
        margin-right: 10rpx;
        display: flex;
    }

    .send-btn-box {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 110rpx;
        height: 60rpx;
        border-radius: 10rpx;
        background: #d02129;
    }

    .send-btn-box .btn {
        color: #ffffff;
        font-size: 28rpx;
    }

    .action-bottom {
        height: 360rpx;
        width: 100%;
        padding: 20rpx;
        box-sizing: border-box;
        display: flex;
    }

    .action-bottom-emoji {
        @apply justify-start flex-wrap overflow-y-scroll;
    }

    .action-bottom image {
        width: 72rpx;
        height: 72rpx;
    }

    .action-box .action-bottom .more-icon {
        display: flex;
        align-items: center;
        flex-direction: column;
        padding: 0 30rpx;
    }

    .action-box .action-bottom .operation-icon {
        width: 60rpx;
        height: 60rpx;
        min-width: 60rpx;
        min-height: 60rpx;
        padding: 25rpx;
        border-radius: 20rpx;
        background: #ffffff;
    }

    .action-box .action-bottom .operation-title {
        font-size: 24rpx;
        line-height: 50rpx;
        color: #82868e;
    }

    .action-box .action-top .record-input {
        flex: 1;
        width: 480rpx;
        height: 80rpx;
        line-height: 80rpx;
        padding-left: 20rpx;
        margin: 20rpx;
        margin-left: 0;
        border: none;
        outline: none;
        box-sizing: border-box;
        border-radius: 6px;
        background: #cccccc;
        color: #ffffff;
        font-size: 28rpx;
        text-align: center;
    }

    .messageSelector-box {
        display: flex;
        justify-content: center;
        align-items: center;
        backdrop-filter: blur(0.27rpx);
        width: 100%;
        position: fixed;
        bottom: 0;
        left: 0;
        border-radius: 12rpx;
        background: #f6f6f6;
        height: 150rpx;
        padding: 20rpx 0;
        font-size: 32rpx;
    }

    .messageSelector-box .messageSelector-btn {
        width: 60rpx;
        height: 60rpx;
    }

    uni-checkbox:not([disabled]) .uni-checkbox-input:hover {
        border-color: #d1d1d1 !important;
    }

    uni-checkbox .uni-checkbox-input {
        border-radius: 50% !important;
    }

    /* #ifdef MP-WEIXIN */
    checkbox .wx-checkbox-input {
        border-radius: 50% !important;
    }

    checkbox .wx-checkbox-input.wx-checkbox-input-checked {
        color: #007aff !important;
    }

    /* #endif */

    .action-popup {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .action-popup .layer {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(51, 51, 51, 0.5);
        z-index: 999;
    }

    .action-popup .action-list {
        width: 350rpx;
        background: #434343;
        position: relative;
        z-index: 1000;
        border-radius: 20rpx;
        overflow: hidden;
    }

    .action-popup .action-item {
        text-align: center;
        height: 100rpx;
        line-height: 100rpx;
        font-size: 34rpx;
        color: #ffffff;
        border-bottom: 1px solid #efefef;
    }

    .action-popup .action-item:last-child {
        border: none;
    }

    .record-loading {
        position: fixed;
        top: 50%;
        left: 50%;
        width: 300rpx;
        height: 300rpx;
        margin: -150rpx -150rpx;
        background: #262628;
        background: url('/static/images/recording-loading.gif') no-repeat center;
        background-size: 100%;
        border-radius: 40rpx;
    }

    .img-layer {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: #000000;
        z-index: 9999;
        padding: 6rpx;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .img-layer uni-image {
        height: 100% !important;
    }

    .img-layer {
        height: 100% !important;
        width: 100% !important;
    }

    .order-list {
        position: fixed;
        top: 0;
        bottom: 0;
        z-index: 10;
        width: 100vw;
        height: 100vh;
        background: rgba(0, 0, 0, 0.5);
    }

    .orders-content {
        position: absolute;
        width: 100%;
        bottom: 0;
        background: #f1f1f1;
        z-index: 200;
    }

    .title {
        font-weight: 600;
        font-size: 30rpx;
        color: #000000;
        margin-left: 20rpx;
        margin-right: 20rpx;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .close {
        font-size: 50rpx;
    }

    .order-item {
        padding: 20rpx;
        background: #ffffff;
        margin: 20rpx;
        border-radius: 20rpx;
    }

    .order-id {
        font-size: 24rpx;
        color: #666666;
        margin-bottom: 10rpx;
    }

    .order-body {
        display: flex;
        font-size: 28rpx;
    }

    .order-img {
        width: 120rpx;
        height: 120rpx;
        border-radius: 10rpx;
    }

    .order-name {
        margin-left: 20rpx;
        width: 270rpx;
    }

    .order-right {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    .order-price {
        font-weight: bold;
    }

    .order-count {
        font-size: 24rpx;
        color: #666666;
    }

    .video-snapshot {
        position: relative;
    }

    .video-snapshot video {
        max-height: 300rpx;
        max-width: 400rpx;
    }

    .video-snapshot .video-play-icon {
        position: absolute;
        width: 40rpx;
        height: 40rpx;
        border-radius: 20rpx;
        background: url('/static/images/play.png') no-repeat center;
        background-size: 100%;
        top: 50%;
        left: 50%;
        margin: -20rpx;
    }

    .group-icon {
        right: 20rpx;
        width: 60rpx;
        height: 60rpx;
        top: 14rpx;
        position: fixed;
        right: 20rpx;
        top: 120rpx;
        background-color: #c4c4c4;
        z-index: 2;
        border-radius: 60rpx;
    }

    .uni-toast {
        background-color: #ffffff !important;
    }

    .time-lag {
        font-size: 20rpx;
        text-align: center;
    }

    .audio-content {
        height: 86rpx;
        -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
    }

    .audio-facade {
        min-width: 20rpx;
        padding: 6rpx 10rpx;
        line-height: 72rpx;
        background: #ffffff;
        font-size: 24rpx;
        border-radius: 14rpx;
        color: #000000;
        display: flex;
    }

    .audio-facade-bg {
        background: url('/static/images/voice.png') no-repeat center;
        background-size: 30rpx;
        width: 40rpx;
    }

    .audio-facade-bg.play-icon {
        background: url('/static/images/play.gif') no-repeat center;
        background-size: 30rpx;
    }

    .order-content {
        border-radius: 20rpx;
        background: #ffffff;
        padding: 16rpx;
        display: flex;
        flex-direction: column;
    }

    .scroll-view .content .order-id {
        color: #333333;
    }

    .scroll-view .content .order-body {
        padding: 10rpx;
    }

    .scroll-view .content .order-name {
        font-weight: normal;
    }

    .scroll-view .content .order-info {
        display: flex;
        justify-content: space-between;
        padding: 10rpx;
    }

    .scroll-view .content .order-info .order-price {
        font-weight: normal;
    }

    .message-read {
        color: grey;
        font-size: 24rpx;
        text-align: end;
        height: 36rpx;
    }

    .message-unread {
        color: #d02129;
        font-size: 24rpx;
        text-align: end;
        height: 36rpx;
    }

    .message-recalled {
        display: flex;
        align-items: center;
        justify-content: center;
        line-height: 56rpx;
        font-size: 26rpx;
        text-align: center;
        color: grey;
    }

    .message-recalled .message-recalled-self {
        display: flex;
    }

    .message-recalled .message-recalled-self span {
        margin-left: 10rpx;
        color: #d02129;
    }
}

.message-item.system-center {
    display: flex;
    justify-content: center;
}
</style>

<style>
.image-content {
    border-radius: 12rpx;
    width: 300rpx;
    height: 300rpx;
}

.text-content {
    padding: 16rpx;
    border-radius: 12rpx;
    color: #000000;
    background: #ffffff;
    word-break: break-all;
    text-align: left;
    vertical-align: center;
    display: block;
}

.text-content img {
    width: 50rpx;
    height: 50rpx;
}
</style>
