<template>
    <chatMain
        ref="chatRef"
        :options="chatOptions"
        @onSend="send"
        @loadHistoryMessage="loadHistoryMessage"
        @showDetail="showDetail"
    ></chatMain>

    <!-- 拍品详情弹窗 -->
    <uv-popup ref="popup" @change="popChange">
        <view v-if="showItem" class="p-4">
            <view class="flex flex-row items-center overflow-hidden cursor-pointer">
                <view class="text-wrap px-2 flex-1 flex flex-col">
                    <view class="text-[#ff7144] line-clamp-3">{{ showItem.name }}</view>
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
</template>

<script setup lang="ts">
import chatMain from '@/components/chat/chatMain.vue'
import { onLoad, onUnload } from '@dcloudio/uni-app'
import { ChatMessageType, type UserDto, type AuctionItemDto } from '@/composables/types'
import api from '@/utils/api'
import type { ChatOptions } from '@/components/chat/types'
import { nextTick } from 'vue'
import { getImgUrl } from '@/composables'

const chatOptions: ChatOptions = {
    enableAudio: false,
    enableEmoji: true,
    enableImage: true,
    maxTextLength: 500,
    chatType: 'private',
    showUserInfo: true,
    enableLongPress: true,
    autoScroll: true,
    historyLoadSize: 20,
}

const chatStore = useChatStore()
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)
const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${friend.id}`) || []
})

const friend = reactive({
    id: 0,
    name: '',
    avatar: '',
})

const user = ref<UserDto | null>(null)

// 拍品详情相关
const showItem = ref<AuctionItemDto | null>(null)
const popup = ref(null as any)

onLoad((query: any) => {
    if (query != undefined) {
        const id = parseInt(query.id + '')
        if (isNaN(id)) {
            return
        }
        friend.id = id
        friend.name = decodeURIComponent(query.name)
        friend.avatar = decodeURIComponent(query.avatar)

        api.user.get({ id: friend.id }).then((res: UserDto) => {
            user.value = res
            friend.name = res.name!
            friend.avatar = res.headImgUrl!
        }).catch((e) => {
            console.error('获取用户信息失败:', e)
        })

        chatStore.connectServer().then(() => {
            chatStore.addChatList(friend.id, friend.name, friend.avatar)
            chatStore.SetCurrentChatId(friend.id)

            nextTick(() => {
                loadHistoryMessage(true)
            })
        }).catch((e) => {
            console.error('连接聊天服务器失败:', e)
        })
    }
})

onUnload(() => {
    chatStore.closeChat(friend.id)
})

async function loadHistoryMessage(force = false) {
    if (!chatRef.value) {
        return
    }

    chatRef.value.history.loading = true
    let lastTime = 0
    if (!force)
        if (historyMsgs.value && historyMsgs.value.length) {
            lastTime = historyMsgs.value[0].time!
        }
    await chatStore.getPrivateHistory(friend.id, lastTime, force).then((res) => {
        if (chatRef.value) {
            chatRef.value.history.loading = false
            if (res.length < 20) {
                chatRef.value.history.allLoaded = true
            }
        }
    })
}

// LINK[epic=消息发送] - 私聊消息发送逻辑
function send(e: { type: ChatMessageType; data: string | object }) {
    if (e.type === ChatMessageType.Image) {
        chatStore.sendMsg(friend.id, friend.name, friend.avatar, '[图片]', ChatMessageType.Image, e.data).then(() => {})
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendMsg(friend.id, friend.name, friend.avatar, e.data as string).then(() => {})
    }
}

// 拍品详情相关函数
function showDetail(e: AuctionItemDto) {
    showItem.value = convertFields(e)
    popup.value.open('bottom')
}

// 检测首字母是否大写
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

function popChange(e: { show: boolean; type: string }) {
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
    } catch (error) {
        // Image preview error handling
    }
}
</script>
<route lang="json">
{
    "style": {
        "navigationBarTitleText": "私聊"
    }
}
</route>
