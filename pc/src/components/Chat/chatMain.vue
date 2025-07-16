<template>
    <div ref="scrollView" class="chat-main">
        <div ref="messageList" class="message-list">
            <!-- {{ history }} -->

            <div v-if="history.loading" class="history-loading">
                <img src="@/assets/images/pending.gif" />
            </div>

            <div
                v-else
                class="my-2"
                :class="history.allLoaded ? 'history-loaded' : 'load'"
                @click="loadHistoryMessage()"
            >
                {{ history.allLoaded ? '已经没有更多的历史消息' : '获取历史消息' }}
            </div>

            <div v-for="(message, index) in historyMsgs" :key="index" v-motion-pop-visible-once>
                <div class="time-tips">{{ renderMessageDate(message, index) }}</div>

                <div class="message-item" @contextmenu.prevent>
                    <div class="message-item-content" :class="{ self: message.from === userStore.user.id }">
                        <div
                            v-if="message.type === ChatMessageType.Welcome"
                            class="text-center flex-1 text-gray-600 text-sm"
                        >
                            {{ message.fromName }} 加入群聊
                        </div>

                        <div
                            v-else-if="message.type === ChatMessageType.BanUser"
                            class="text-center flex-1 text-gray-600 text-sm"
                        >
                            {{ message.msg }}
                        </div>
                        <div
                            v-else-if="message.type === ChatMessageType.Backout"
                            class="text-center flex-1 text-gray-600 text-sm"
                        >
                            {{ message.msg }}
                        </div>

                        <template v-else>
                            <div class="sender-info" @click.right="showActionPopup(message)">
                                <img
                                    v-if="userStore.user.id === message.from && message.fromTag != '拍卖师'"
                                    :src="getImgUrl2(userStore.user.headImgUrl!, true)"
                                    class="sender-avatar object-cover"
                                />

                                <img v-else :src="getImgUrl2(message.avatar)" class="sender-avatar object-cover" />
                            </div>

                            <div class="message-content">
                                <div
                                    @click="showGroupChatRules = true"
                                    class="message-fromName"
                                    style="display: flex; cursor: pointer"
                                    :class="[message.fromAdmin ? ' !text-red-500' : '']"
                                >
                                    <span
                                        v-if="message.fromAdmin && message.fromTag"
                                        :class="[message.tagClass ? message.tagClass : '']"
                                    >
                                        {{ message.fromTag }}</span
                                    >
                                    <div
                                        v-else-if="message.userChatLevel && isPrivateChat === false"
                                        class="tag_AuctionManager"
                                        :style="{
                                            background: `linear-gradient(90deg,${message.userChatLevel.borderColor},${message.userChatLevel.rightBorderColor})`,
                                        }"
                                        style="margin-right: 5px; color: #fff"
                                    >
                                        {{ message.userChatLevel.name }}
                                    </div>
                                    {{ message.fromName }}
                                    <!-- {{ dayjs(message.time).format('MM-DD HH:mm') }} -->
                                </div>

                                <div class="message-payload mt-1">
                                    <!-- 文本消息 -->
                                    <TextMessage
                                        v-if="message.type === ChatMessageType.Text"
                                        :message="message"
                                        @action="showDetails"
                                    />
                                    <!-- 图片消息 -->
                                    <ImageMessage
                                        v-if="message.type === ChatMessageType.Image"
                                        :message="message"
                                        @action="showImagePreviewPopup"
                                        @contextMenu="showActionPopup(message, true)"
                                    />
                                    <!-- 开始拍卖 -->
                                    <AuctionStartMessage
                                        v-if="message.type === ChatMessageType.AuctionStart"
                                        :message="message"
                                        @action="catchAction"
                                    />
                                    <!-- 出价 -->
                                    <AuctionBidMessage
                                        v-if="message.type === ChatMessageType.AuctionBid"
                                        :message="message"
                                        @action="showDetails"
                                    />
                                    <!-- 公布得主 -->
                                    <AuctionEndMessage
                                        v-if="message.type === ChatMessageType.AuctionEnd"
                                        :message="message"
                                        @action="onAuctionEndAction"
                                    />
                                    <!-- 交易通知 -->
                                    <AuctionDealMessage
                                        v-if="message.type === ChatMessageType.AuctionDeal"
                                        :message="message"
                                        @action="onAuctionDealAction"
                                    />
                                    <!-- 卡秒状态变更消息 -->
                                    <KasecStatusMessage
                                        v-if="message.type === ChatMessageType.KasecStatusChanged"
                                        :message="message"
                                        @action="showDetails"
                                    />
                                </div>
                            </div>
                        </template>
                    </div>
                </div>
            </div>
            <div class="h-10"></div>
        </div>
    </div>
    <div class="chat-footer">
        <div class="action-box">
            <div class="action-bar flex items-center space-x-2">
                <el-popover v-model:visible="emojiVisible" placement="top-end" trigger="click" width="450">
                    <template #reference>
                        <i class="i-carbon:face-activated size-6 text-gray-500 hover:text-rose-600"></i>
                    </template>
                    <el-tabs type="card">
                        <el-tab-pane label="系统">
                            <img
                                v-for="(emojiItem, emojiKey, index) in emojiStore.emojiMap"
                                :key="index"
                                class="emoji-item"
                                :src="emojiStore.emojiUrl + emojiItem"
                                @click="chooseEmoji(emojiKey)"
                            />
                        </el-tab-pane>
                        <el-tab-pane label="收藏">
                            <div class="grid grid-cols-6 gap-2">
                                <div v-for="(e, index) in emojiStore.userEmoji" :key="index" class="relative">
                                    <img
                                        class="w-full hover:scale-110 object-contain"
                                        :src="getImgUrl2(e.url, true)"
                                        @click="chooseUserEmoji(e)"
                                    />
                                    <i
                                        class="i-mdi:close-circle text-red-500 absolute top-0 right-0 cursor-pointer hover:scale-120"
                                        @click="emojiStore.removeEmoji(e)"
                                    />
                                </div>
                            </div>
                        </el-tab-pane>
                    </el-tabs>
                </el-popover>

                <!-- 图片 -->
                <div class="action-item">
                    <tt-upload :file-size="2048" :multiple="false" @on-uploaded="sendImageMessage2">
                        <div>
                            <i class="iconfont icon-picture" title="图片"></i>
                        </div>
                    </tt-upload>
                </div>
                <!-- <div class="action-item" @click="screenshotStatus = true">
					<div>
						<img style="width:20px;height:20px;margin-top:5px;" src="../../assets/screenshot.png" />
					</div>
				</div> -->
            </div>

            <div class="input-box">
                <chat-input
                    v-model="text"
                    @focus="emojiVisible = false"
                    @file-uploaded="sendImageMessage"
                    @onPressEnter="sendTextMessage"
                />
            </div>
            <div class="send-box">
                <el-button color="#f4835a" class="zoom-in" :disabled="!text.trim()" @click="sendTextMessage">
                    <span class="text-white">发送 Enter</span>
                </el-button>
            </div>
        </div>
    </div>
    <userInfoDialog ref="userInfoDialogRef" />

    <!-- //ANCHOR - 消息弹窗 -->
    <div v-if="actionPopup.visible" class="action-popup" @click="actionPopup.visible = false">
        <div class="action-popup-main">
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
        </div>
    </div>
    <!-- 群聊等级规则 -->
    <div v-if="showGroupChatRules" class="action-popup" @click="actionPopup.visible = false">
        <div class="action-popup-main" style="width: 280px; padding: 10px">
            <div>群等级制度，根据成交价金额自动累计</div>
            <div v-for="(x, index) in groupChatLevel" :key="index">
                {{ x.level }}级:消费满{{ x.amountRequired }} {{ x.name }}
            </div>
            <!-- <div> 1级:消费满100 实习生熊男 </div>
            <div> 2级:消费满1000 试用期露比 </div>
            <div> 3级:消费满5000 转正神兽 </div>
            <div> 4级:消费满1W 经理犹大 </div>
            <div> 5级:消费满5W 总经理双王 </div>
            <div> 6级:消费满10W 董事长里雍 </div> -->
            <div class="action-item" style="border-bottom: 0px solid #efefef" @click="showGroupChatRules = false">
                取消
            </div>
        </div>
    </div>

    <auction-item-detail ref="detailRef" />

    <!-- <screen-short v-if="screenshotStatus" @destroy-component="destroyComponent" @get-image-data="getImg">
	</screen-short> -->
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { convertAuctionPayload } from '@/utils/propertyConverter'
import { orderBy, uniqBy, last } from 'lodash'
import api from '@/api'
import chatInput from '@/components/Chat/ChatInput.vue'
import ttUpload from '@/components/tt-upload/index.vue'
import { ChatEmojiDto, ChatMessage, ChatMessageType } from '@/api/appService'
import userInfoDialog from './userInfoDialog.vue'
import dayjs from 'dayjs'
import { useDebounceFn } from '@vueuse/core'
import { ElMessageBox } from 'element-plus'
import { useEventBus } from '@vueuse/core'
import auctionItemDetail from '@/components/Chat/auctionItemDetail.vue'
import { GetList } from '@/api/groupChatLevel'
import AuctionEndMessage from '@/components/Chat/AuctionEndMessage.vue'
import AuctionBidMessage from '@/components/Chat/AuctionBidMessage.vue'
import AuctionStartMessage from '@/components/Chat/AuctionStartMessage.vue'
import KasecStatusMessage from '@/components/Chat/KasecStatusMessage.vue'
import AuctionDealMessage from '@/components/Chat/AuctionDealMessage.vue'
import ImageMessage from '@/components/Chat/ImageMessage.vue'
import TextMessage from '@/components/Chat/TextMessage.vue'
import { useChatStore } from '@/stores/chatStore'
import { useUserStore } from '@/stores/userStore'
import { useAuctionStore } from '@/stores/auctionStore'
import { useEmojiStore } from '@/stores/emojiStore'
import { Tips } from '@/composables'
import { getImgUrl as getImgUrl2 } from '@/composables'

const emit = defineEmits(['loadHistoryMessage', 'onSend'])
//显示群聊规则
const showGroupChatRules = ref(false)
//商品详情
const detailRef = ref<InstanceType<typeof auctionItemDetail> | null>(null)
const userInfoDialogRef = ref<InstanceType<typeof userInfoDialog> | null>(null)
const isLoadingHistory = ref(false)

//LINK - loadHistoryMessage
function loadHistoryMessage(force = false) {
    isLoadingHistory.value = true
    emit('loadHistoryMessage', force)
}

// LINK - 表情

const emojiStore = useEmojiStore()

const chatStore = useChatStore()
const userStore = useUserStore()
const auctionStore = useAuctionStore()
const scrollView = ref(null as any)
const firstLoad = ref(true)
//群聊等级信息
const groupChatLevel = ref([])

const route = useRoute()
//是否是私聊页面
const isPrivateChat = ref(false)

/**截图组件 结束**/

onMounted(async () => {
    delayloadhistory()
    scrollToBottom(true)
    //查询群聊等级信息
    var res = await GetList()
    if (res.data) {
        groupChatLevel.value = res.data
    }
    //判断是否是私聊
    if (route.path.includes('privateChat')) {
        isPrivateChat.value = true
    } else {
        isPrivateChat.value = false
    }
})

onUnmounted(() => {
    clearInterval(timeId)
    unsubscribe()
})

let timeId = null

// LINK - 延迟加载服务器最新消息,不同则重连服务器
function delayloadhistory() {
    console.log('delayloadhistory')
    clearInterval(timeId)
    timeId = setInterval(() => {
        console.log('delayloadhistory执行')
        //检查最后一条消息是否是historyMsgs的最后一条
        chatStore.getServerLastId().then((res) => {
            const hisLast = last(
                historyMsgs.value.filter((x) => x.type !== 'Welcome' && x.type !== 'BanUser' && x.type !== 'Backout')
            )
            console.log('chatStore.getServerLastId', res, hisLast)
            if (res != api.guid && res !== hisLast?.id) {
                console.log('服务器消息与本地不同')
                chatStore.connectServer(true).then(() => {
                    loadHistoryMessage(true)
                    if (chatStore.currentChat.id === -1) {
                        auctionStore.getList()
                    }
                })
            } else {
                console.log('服务器消息与本地相同')
            }
        })
    }, 30_000)
}

const bus = useEventBus(onmessageKey)
//LINK[epic=处理收到消息] - ChatMain处理收到消息
const unsubscribe = bus.on((msg: any) => {
    console.log('chatMain onmessageKey', msg)
    if (msg.type === 'AuctionStart' || msg.type === 'AuctionEnd') {
        auctionStore.getList()
    }

    if (msg.type === 'Backout' && msg.chan === '-1_auction') {
        console.log('拍卖行撤回')
        auctionStore.getList()
    }

    if (msg.type === 'Backout') {
        console.log('Backout', msg)
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

const historyMsgs = computed(() => {
    const realData = orderBy(
        uniqBy(chatStore.chatMap.get(`${chatStore.currentChat.id}`) || [], 'id'),
        [(msg) => msg.time], // 使用时间戳排序
        ['asc']
    )
    return [...realData]
})

const text = ref('')

// 展示消息删除弹出框
const actionPopup = ref({
    visible: false,
    message: null,
    recallable: false,
    isImg: false,
})

watch(
    () => historyMsgs.value.length,
    () => {
        const l = historyMsgs.value.length
        delayloadhistory()
        if (l > 5) {
            console.log('watchEffect', historyMsgs.value.length)
            if (firstLoad.value) {
                firstLoad.value = false
                scrollToBottom(true)
            } else scrollToBottom(false)
        }
    }
)

function scrollToBottom(force = true) {
    console.log('force', force)
    console.log('scrollTop', scrollView.value.scrollTop)
    console.log('clientHeight', scrollView.value.clientHeight)
    console.log('scrollHeight', scrollView.value.scrollHeight)
    setTimeout(() => {
        if (scrollView.value && scrollView.value.scrollHeight) {
            if (!force) {
                console.log('if (!force)')
                if (scrollView.value.scrollTop + scrollView.value.clientHeight < scrollView.value.scrollHeight - 400) {
                    console.log(
                        'if (scrollView.value.scrollTop + scrollView.value.clientHeight < scrollView.value.scrollHeight - 400)'
                    )
                    return
                }
            }

            // if (isLoadingHistory.value) {
            //     isLoadingHistory.value = false
            //     return
            // }
            // console.log('scrollHeight', scrollView.value.scrollHeight)
            setTimeout(() => {
                scrollView.value.scrollTop = scrollView.value.scrollHeight // scrollView.value.scrollHeight
                console.log('scrollView.value.scrollTop', scrollView.value.scrollTop)
            }, 50)
        }
    }, 100)
}
//消息
// function renderTextMessage(message: ChatMessage) {
// 	var html = "";
// 	if (typeof message.payload === 'string') message.payload = JSON.parse(message.payload!)
// 	if (message.payload.Status != undefined || message.payload.Status === 4 || message.payload.status === '已成交') {
// 		html += `<div class="text-sm">商品名称: ${message.payload.name === undefined ? message.payload.Name : message.payload.name}</div>
//         <div>${message.payload.description === undefined ? message.payload.Description : message.payload.description}</div>`
// 	}
// 	if (message && message.msg)
// 		return html += '<span class="text-content">' + emoji.value.decoder.decode(message.msg) + '</span>'
// 	return ''
// }
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
    console.log('showActionPopup', msg)
    actionPopup.value.visible = true
    actionPopup.value.isImg = isImg
    selectMessage.value = msg
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

const showImageViewer = inject('showImageViewer') as (list: string[]) => void

function showImagePreviewPopup(message: ChatMessage) {
    if (typeof message.payload === 'string') message.payload = JSON.parse(message.payload!)
    // imagePreview.value.visible = true
    // imagePreview.value.url = `${import.meta.env.VITE_APP_UPYUN_IMG_URL}${message.payload.url}`
    let url = ''
    if (message.payload.url.startsWith('http')) url = message.payload.url
    else url = `${import.meta.env.VITE_APP_UPYUN_IMG_URL}${message.payload.url}`
    showImageViewer([url])
}

function chooseEmoji(emojiKey: string) {
    text.value += emojiKey
    emojiVisible.value = false
}

const emojiVisible = ref(false)
//ANCHOR - 选择用户表情
function chooseUserEmoji(e: ChatEmojiDto) {
    console.log('chooseUserEmoji', e)
    emojiVisible.value = false
    emit('onSend', { type: ChatMessageType.Image, data: { url: e.url } })
}

function sendImageMessage2(e: any) {
    console.log('sendImageMessage2', e)
    //close emoji popup

    emit('onSend', { type: ChatMessageType.Image, data: e })
}

function sendImageMessage(e: { url: string; data: any }) {
    // console.log('sendImageMessage', e)
    emit('onSend', { type: ChatMessageType.Image, data: e.data })
}

const sendTextMessage = useDebounceFn((e: KeyboardEvent | PointerEvent) => {
    console.log(e)
    if (!text.value.trim()) {
        Tips.info('输入为空')
        text.value = ''
        return
    }
    emit('onSend', { type: ChatMessageType.Text, data: text.value })
    text.value = ''
}, 200)

const selectMessage = ref<ChatMessage | null>(null)

function addFriend() {
    console.log(selectMessage.value)
    if (selectMessage.value) {
        api.userFriend.addFriend({ id: selectMessage.value.from }).then(() => {
            Tips.success('添加好友成功, 请等待对方同意')
        })
    }
}

//ANCHOR - 查看用户信息
function viewUserInfo(userId: number) {
    console.log('viewUserInfo', userId)
    userInfoDialogRef.value.show(true, userId)
}

const router = useRouter()
//ANCHOR - 添加表情
function addEmoji() {
    emojiStore.addToEmoji(selectMessage.value!).then(() => {
        Tips.success('添加成功')
    })
}

//ANCHOR - 撤回消息
function backout() {
    console.log(selectMessage.value)
    if (selectMessage.value && selectMessage.value.id) {
        api.ws.backout({ body: selectMessage.value }).then(() => {
            Tips.success('撤回成功')
        })
    }
}

//ANCHOR - 管理员私聊
function adminSend() {
    // console.log(
    console.log(selectMessage.value)
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
        router.push({
            path: `/chat/index/privateChat/${chat.id}`,
            query: { name: chat.name, avatar: chat.avatar || 'https://cdn.molitao.top/avater.png' },
        })
    }
}

//ANCHOR - 禁言
function ban() {
    console.log('ban', selectMessage.value)

    const chan = selectMessage.value.chan
    const userId = selectMessage.value.from
    if (selectMessage.value && selectMessage.value.from) {
        //输入禁言多少时间
        ElMessageBox.prompt('请输入禁言时间(分钟)', '禁言', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            inputPattern: /\d+/,
            inputValue: '60',
            inputType: 'number',
            inputErrorMessage: '请输入正确的数字',
        }).then(({ value }) => {
            console.log('ban', value)
            api.ws
                .banUser({
                    body: { userId, minutes: Number(value), chan },
                })
                .then(() => {
                    Tips.success('禁言成功')
                })
        })
    }
}
//查看图片
function catchImage(e: ChatMessage) {
    console.log('catchImage', e)
    try {
        const convertedPayload = convertAuctionPayload(e.payload)
        var description = convertedPayload.description
        if (!description) return
        const list = []
        //从 string中img标签中获取data-url的属性放入数组中
        const reg = /<img.*?data-url=['"](.*?)['"].*?>/g
        let result
        while ((result = reg.exec(description)) !== null) {
            list.push(result[1])
        }

        if (list.length === 0) return
        console.log('catchImage', list)
        // wx.previewImage({
        //     current: list[0], // 当前显示图片的http链接
        //     urls: list, // 需要预览的图片http链接列表
        // })

        showImageViewer(list)
    } catch (e) {
        console.log('catchImage', e)
    }
}

function catchAction({
    message,
    payload,
    type,
    imageUrl,
}: {
    message: ChatMessage
    payload: any
    type?: string
    imageUrl?: string
}) {
    showDetails({ message, payload })
}
//显示详情
function showDetails(e) {
    console.log('showDetails', e)
    if (JSON.stringify(e.payload) === '{}' || e.payload === '{}') {
        return
    }
    const convertedPayload = convertAuctionPayload(e.payload)
    var id = convertedPayload.id
    detailRef.value?.show(true, id)
}

function onAuctionEndAction({ message, payload }) {
    showDetails({ message, payload })
}

function onAuctionDealAction({ message, payload }) {
    showDetails({ message, payload })
}
</script>

<style scoped>
.el-tabs {
    --el-tabs-header-height: 30px;
}

.chat-main {
    display: flex;
    flex-direction: column;
    overflow-y: auto;
    flex: 1;
    scrollbar-width: thin;
}

.chat-main::-webkit-scrollbar {
    width: 0;
}

.chat-main .history-loaded {
    text-align: center;
    font-size: 12px;
    color: #cccccc;
    line-height: 20px;
}

.chat-main .load {
    text-align: center;
    font-size: 12px;
    color: #d02129;
    line-height: 20px;
    cursor: pointer;
}

.history-loading {
    width: 100%;
    text-align: center;
}

.time-tips {
    color: #999;
    text-align: center;
    font-size: 12px;
}

.message-list {
    padding: 0 10px;
}

.message-item {
    display: flex;
}

.message-item-checkbox {
    height: 50px;
    margin-right: 15px;
    display: flex;
    align-items: center;
}

.input-checkbox {
    position: relative;
}

.message-item-checkbox input[type='checkbox']::before,
.message-item-checkbox input[type='checkbox']:checked::before {
    content: '';
    position: absolute;
    top: -3px;
    left: -3px;
    background: #ffffff;
    width: 18px;
    height: 18px;
    border: 1px solid #cccccc;
    border-radius: 50%;
}

.message-item-checkbox input[type='checkbox']:checked::before {
    content: '\2713';
    background-color: #d02129;
    width: 18px;
    color: #ffffff;
    text-align: center;
    font-weight: bold;
}

.message-item-content {
    flex: 1;
    margin: 5px 0;
    overflow: hidden;
    display: flex;
}

.sender-info {
    margin: 0 5px;
}

.sender-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
}

.message-content {
    max-width: calc(100% - 100px);
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

.pending {
    background: url('@/assets/images/pending.gif') no-repeat center;
    background-size: 13px;
    width: 15px;
    height: 15px;
}

.send-fail {
    background: url('@/assets/images/failed.png') no-repeat center;
    background-size: 15px;
    width: 18px;
    height: 18px;
    margin-right: 3px;
}

.content-image {
    display: block;
    cursor: pointer;
}

.content-image img {
    height: 100%;
}

.content-audio {
    -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
}

.content-audio .audio-facade {
    min-width: 12px;
    background: #eeeeee;
    border-radius: 7px;
    display: flex;
    font-size: 14px;
    padding: 8px;
    margin: 5px 0;
    line-height: 25px;
    cursor: pointer;
}

.content-audio .audio-facade-bg {
    background: url('@/assets/images/voice.png') no-repeat center;
    background-size: 15px;
    width: 20px;
}

.content-audio .audio-facade-bg.play-icon {
    background: url('@/assets/images/play.gif') no-repeat center;
    background-size: 20px;
}

.content-order {
    border-radius: 5px;
    border: 1px solid #eeeeee;
    padding: 8px;
    display: flex;
    flex-direction: column;
}

.content-order .order-id {
    font-size: 12px;
    color: #666666;
    margin-bottom: 5px;
}

.content-order .order-body {
    display: flex;
    font-size: 13px;
    padding: 5px;
}

.content-order .order-img {
    width: 70px;
    height: 70px;
    border-radius: 5px;
}

.content-order .order-name {
    margin-left: 10px;
    width: 135px;
    color: #606164;
}

.content-order .order-count {
    font-size: 12px;
    color: #666666;
    flex: 1;
}

.content-file {
    width: 240px;
    height: 65px;
    font-size: 15px;
    background: #ffffff;
    border: 1px solid #eeeeee;
    display: flex;
    align-items: center;
    padding: 10px;
    border-radius: 5px;
    cursor: pointer;
}

.content-file:hover {
    background: #f1f1f1;
}

.file-info {
    width: 194px;
    text-align: left;
}

.file-name {
    text-overflow: ellipsis;
    overflow: hidden;
    display: -webkit-box;
    word-break: break-all;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
}

.file-size {
    font-size: 12px;
    color: #ccc;
}

.file-img {
    width: 50px;
    height: 50px;
}

.message-item .self {
    overflow: hidden;
    display: flex;
    justify-content: flex-start;
    flex-direction: row-reverse;
}

.message-item .self .audio-facade {
    flex-direction: row-reverse;
}

.message-item .self .audio-facade-bg {
    background: url('@/assets/images/voice.png') no-repeat center;
    background-size: 15px;
    width: 20px;
    -moz-transform: rotate(180deg);
    -webkit-transform: rotate(180deg);
    -o-transform: rotate(180deg);
    transform: rotate(180deg);
}

.message-item .self .play-icon {
    background: url('@/assets/images/play.gif') no-repeat center;
    background-size: 20px;
    -moz-transform: rotate(180deg);
    -webkit-transform: rotate(180deg);
    -o-transform: rotate(180deg);
    transform: rotate(180deg);
}

.message-recalled {
    display: flex;
    align-items: center;
    justify-content: center;
    line-height: 28px;
    font-size: 13px;
    text-align: center;
    color: grey;
    margin-top: 10px;
}

.message-recalled-self {
    display: flex;
}

.message-recalled-self span {
    margin-left: 5px;
    color: #d02129;
    cursor: pointer;
}

.chat-footer {
    border-top: 1px solid #dcdfe6;
    width: 100%;
    height: 170px;
    background: #ffffff;
}

.action-delete {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 100%;
    background-color: #ffffff;
}

.delete-btn {
    width: 25px;
    height: 25px;
    padding: 10px;
    background: #f5f5f5;
    border-radius: 50%;
    cursor: pointer;
    margin-bottom: 10px;
}

.action-box {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
}

.action-bar {
    display: flex;
    flex-direction: row;
    padding: 0 10px;
    position: relative;
}

.action-bar .action-item {
    text-align: left;
    padding: 10px 0;
    position: relative;
}

.action-bar .action-item .iconfont {
    font-size: 22px;
    margin: 0 10px;
    z-index: 3;
    color: #606266;
    cursor: pointer;
}

.action-bar .action-item .iconfont:focus {
    outline: none;
}

.action-bar .action-item .iconfont:hover {
    color: #d02129;
}

.emoji-box {
    width: 360px;
    position: absolute;
    top: -151px;
    left: -11px;
    z-index: 2007;
    background: #fff;
    border: 1px solid #ebeef5;
    padding: 12px;
    text-align: justify;
    font-size: 14px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
    word-break: break-all;
    border-radius: 4px;
}

.emoji-item {
    width: 28px;
    height: 28px;
    margin: 0 2px;
}

.input-box {
    padding: 0 10px;
    flex: 1;
}

.input-content {
    border: none;
    resize: none;
    display: block;
    padding: 5px 15px;
    box-sizing: border-box;
    width: 100%;
    color: #606266;
    outline: none;
    background: #ffffff;
    word-break: break-all;
}

.send-box {
    padding: 5px 10px;
    text-align: right;
}

.action-popup {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    right: 0;
    background: rgba(51, 51, 51, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
}

.action-popup-main {
    width: 150px;
    /* height: 120px; */
    background: #ffffff;
    z-index: 100;
    border-radius: 10px;
    overflow: hidden;
}

.action-popup-main .action-item {
    text-align: center;
    line-height: 40px;
    font-size: 15px;
    color: #262628;
    border-bottom: 1px solid #efefef;
    cursor: pointer;
}

.order-box {
    width: 848px;
    position: absolute;
    left: -281px;
    right: 0;
    top: 0;
    bottom: 0;
    z-index: 2007;
    font-size: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(33, 33, 33, 0.7);
}

.order-list {
    width: 300px;
    background: #f1f1f1;
    border-radius: 5px;
}

.order-list .title {
    font-weight: 600;
    font-size: 15px;
    color: #000000;
    margin-left: 10px;
    margin-right: 10px;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.order-list .title span {
    font-size: 28px;
    font-weight: 400;
    cursor: pointer;
}

.order-list .order-item {
    padding: 10px;
    background: #ffffff;
    margin: 10px;
    border-radius: 5px;
    cursor: pointer;
}

.order-list .order-id {
    font-size: 12px;
    color: #666666;
    margin-bottom: 5px;
}

.order-list .order-body {
    display: flex;
    font-size: 13px;
    justify-content: space-between;
}

.order-list .order-img {
    width: 50px;
    height: 50px;
    border-radius: 5px;
}

.order-list .order-name {
    width: 160px;
}

.order-list .order-count {
    font-size: 12px;
    color: #666666;
    flex: 1;
}
</style>
