<template>
    <view>
        <view v-if="item" class="sticky top-0 left-0 right-0 z-99">
            <uv-notice-bar
                mode="link"
                bgColor="#f4835a"
                color="#ffffff"
                :text="announceContent"
                :url="`/pages/announce/list?id=1`"
            ></uv-notice-bar>
        </view>

        <view class="fixed right-0 top-100rpx" @click.stop="showGroups">
            <view class="text-30rpx w-48rpx py-2 bg-[#ff7144] text-white font-700 rounded-l-lg text-center">
                组队房间
            </view>
        </view>

        <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage" />
        <uv-popup ref="popupRef" mode="right">
            <view class="w-65vw h-100vh">
                <div class="shadow h-10 grid grid-cols-3">
                    <div class="bg-gray-500 text-white cursor-pointer flex flex-center" @click="chatStore.getGropus">
                        刷新列表
                    </div>
                    <div
                        v-if="myGroup"
                        class="col-span-2 bg-orange-300 text-white cursor-pointer flex flex-center"
                        @click="deleteMyGroup"
                    >
                        删除我的组队频道
                    </div>
                    <div
                        v-else
                        class="col-span-2 bg-[#f4835a] text-white cursor-pointer flex flex-center"
                        @click="createGroup"
                    >
                        创建组队聊天
                    </div>
                </div>
                <div class="space-y-2 overflow-y-scroll" style="height: calc(100vh - 80rpx)">
                    <view class="p-2">
                        <div
                            v-for="(group, k) in chatStore.groups
                                .filter(
                                    (x) =>
                                        x.chan !== '0_lobby' &&
                                        x.chan !== '-1_auction' &&
                                        x.chan &&
                                        x.chan.indexOf(filterText) > -1
                                )
                                .reverse()"
                            :key="k"
                        >
                            <div
                                class="px-2 rounded h-8 flex flex-center text-true-gray-8 shadow-md cursor-pointer relative text-sm flex items-center justify-between"
                                :class="[
                                    myGroup && myGroup.chan === group.chan ? 'bg-blue-500 text-white' : 'bg-gray-100',
                                ]"
                                @click="Goto.group({ id: group.chan })"
                            >
                                <div class="flex-1 line-clamp-1">{{ group.chan.split('_')[1] }}</div>
                                <div class="text-#82615F">{{ group.online }}人</div>
                            </div>
                        </div>
                    </view>
                </div>
            </view>
        </uv-popup>
        <!-- 公告弹窗 -->
        <uv-popup ref="popupShowRef" type="message">
            <view v-if="item" class="popup-content">
                <text class="popup-title">公告</text>
                <img v-if="item.imageUrl" :src="item.imageUrl" mode="aspectFit" class="popup-image" />
                <text class="popup-text">{{ item.content }}</text>
                <view class="popup-view">
                    <button class="popup-button" @tap="onConfirm">确定</button>
                </view>
            </view>
        </uv-popup>
    </view>
</template>
<script setup lang="ts">
import { onLoad, onShow } from '@dcloudio/uni-app'
import chatMain from '@/components/chat/chatMain.vue'
import { nextTick } from 'vue'
import { ChatMessageType, type AnnounceDto } from '@/composables/types'
import api from '@/utils/api'
import { Goto } from '@/composables/goto'
const chatStore = useChatStore()
const userStore = useUserStore()
const chan = ref('')
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)
const item = ref<AnnounceDto | null>(null)
const filterText = ref('')
const popupRef = ref(null as any)
const popupShowRef = ref(null as any)
onLoad((pamams: any) => {
    console.log('onLoad', pamams)
    const t = pamams.id + ''
    console.log(t)
    chan.value = t
    init()
})

const userId = computed(() => {
    return userStore.user.id
})

watch(
    () => userId.value,
    (val) => {
        if (val) {
            init()
        }
    }
)

const init = () => {
    chatStore.connectServer().then(async () => {
        await chatStore.getGropus().then(async () => {
            if (chatStore.hasGroup(chan.value)) {
                await chatStore.SetCurrentChatId(0).then(() => {
                    initGroup(chan.value)
                })
            } else {
                Tips.error('未找到该群聊')
                uni.redirectTo({ url: '/pages/chat/index' })
            }
        })
    })
    //获取最新公告
    api.announce.getLatest({ id: 1 }).then((res) => {
        item.value = res
        nextTick(() => {
            var noticeInfo = uni.getStorageSync('lobbyNotice')
            if (noticeInfo === '' || noticeInfo.id != res.id) {
                popupShowRef.value.open()
            }
        })
    })
}
//关闭公告弹窗
const onConfirm = () => {
    uni.setStorageSync('lobbyNotice', item.value)
    popupShowRef.value.close()
}
const announceContent = computed(() => {
    //如果长度大于40，截取前40个字符
    if (item.value && item.value.content && item.value.content.length > 18) {
        return item.value.content.slice(0, 18) + '...'
    } else if (item.value && item.value.content) {
        return item.value.content
    }
    return ''
})

const myGroup = computed(() => {
    const _r = chatStore.hasMyGroup()
    return _r ? _r : ''
})

const initGroup = async (name: string) => {
    await loadHistoryMessage(true)
}

const historyMsgs = computed(() => {
    console.log('historyMsgs', chatStore.chatMap)
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    if (force) {
        await chatStore.joinChannel(chan.value)
    }

    chatRef.value!.history.loading = true
    const name = `${chatStore.currentChat.id}_${chatStore.currentChat.name}`
    let lastTime = new Date().getTime()
    if (!force)
        if (historyMsgs.value && historyMsgs.value.length) {
            lastTime = historyMsgs.value[0].time!
        }
    await chatStore.getGroupHistory(name, lastTime, force).then((res) => {
        chatRef.value!.history.loading = false
        if (res.length < 20) {
            chatRef.value!.history.allLoaded = true
        }
    })
}

//LINK[epic=消息发送] - 群消息发送逻辑
function send(e: { type: ChatMessageType; data: string | object }) {
    if (e.type === ChatMessageType.Image) {
        chatStore.sendChannelMsg('[图片]', '', ChatMessageType.Image, e.data).then(() => {
            //
        })
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendChannelMsg(e.data as string, '', ChatMessageType.Text).then(() => {
            //
        })
    }
}

function showGroups() {
    console.log('showGroups', popupRef.value)
    chatStore.getGropus().then(() => {
        popupRef.value.open()
    })
}

function deleteMyGroup() {
    console.log(myGroup.value)
    if (myGroup.value) {
        deleteChannel(myGroup.value.chan)
    }
}

function deleteChannel(name: string) {
    if (name.split('_')[0] !== `-${userStore.user.id}`) return

    //确定删除该组队聊天吗
    uni.showModal({
        title: '确定删除该组队聊天吗',
        content: '删除后将无法恢复',
        success: (res) => {
            if (res.confirm) {
                chatStore.deleteChannel(name).then(() => {
                    Tips.success('删除成功')
                    chatStore.getGropus()
                })
            }
        },
    })
}

function createGroup() {
    //弹出输入框
    uni.showModal({
        title: '创建组队聊天',
        content: '',
        editable: true,
        placeholderText: '请输入组队聊天名称',
        success: (res) => {
            if (res.confirm) {
                chatStore
                    .createChannel(res.content)
                    .then(() => {
                        Tips.success('创建成功')
                        chatStore.getGropus()
                    })
                    .catch((e) => {
                        Tips.error(e)
                    })
            }
        },
    })
}
</script>
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
        "navigationBarTitleText": "勇者招募所"
    }
}
</route>
