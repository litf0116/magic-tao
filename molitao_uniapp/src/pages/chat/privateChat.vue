<template>
    <chatMain ref="chatRef" :options="chatOptions" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
</template>

<script setup lang="ts">
import chatMain from '@/components/chat/chatMain.vue'
import { onLoad } from '@dcloudio/uni-app'
import { ChatMessageType, type UserDto } from '@/composables/types'
import api from '@/utils/api'
import type { ChatOptions } from '@/components/chat/types'

const chatOptions: ChatOptions = {
    enableAudio: false,
    enableEmoji: true,
    enableImage: true,
    maxTextLength: 500,
    chatType: 'private',
    showUserInfo: true,
    enableLongPress: true,
    autoScroll: true,
    historyLoadSize: 20
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

onLoad((query: any) => {
    if (query != undefined) {
        const id = parseInt(query.id + '')
        if (isNaN(id)) {
            console.error('Invalid friend id:', query.id)
            return
        }
        friend.id = id
        friend.name = decodeURIComponent(query.name)
        friend.avatar = decodeURIComponent(query.avatar)

        api.user.get({ id: friend.id }).then((res: UserDto) => {
            user.value = res
            friend.name = res.name!
            friend.avatar = res.headImgUrl!
        })

        chatStore.connectServer().then(() => {
            chatStore.addChatList(friend.id, friend.name, friend.avatar)
            chatStore.SetCurrentChatId(friend.id)

            loadHistoryMessage(true)
        })
    }
})

async function loadHistoryMessage(force = false) {
    chatRef.value!.history.loading = true
    let lastTime = new Date().getTime()
    if (!force)
        if (historyMsgs.value && historyMsgs.value.length) {
            lastTime = historyMsgs.value[0].time!
        }
    await chatStore.getPrivateHistory(friend.id, lastTime, force).then((res) => {
        chatRef.value!.history.loading = false
        if (res.length < 20) {
            chatRef.value!.history.allLoaded = true
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
</script>
<route lang="json">
{
    "style": {
        "navigationBarTitleText": "私聊"
    }
}
</route>
