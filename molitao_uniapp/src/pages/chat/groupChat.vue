<template>
    <view>
        <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"/>
    </view>
</template>
<script setup lang="ts">
import {onLoad, onShow} from '@dcloudio/uni-app'
import chatMain from '@/components/chat/chatMain.vue'
import {ChatMessageType} from '@/composables/types'

const chatStore = useChatStore()
const chan = ref('')
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)

onShow(() => {
})

onLoad(async (pamams: any) => {
    if (pamams != null) {
        const t = pamams.id + ''
        chan.value = t
        chatStore.connectServer().then(async () => {
            if (chatStore.hasGroup(t)) {
                await chatStore.SetCurrentChatId(parseInt(t.split('_')[0]), t.split('_')[1], true).then(() => {
                    initGroup()
                })
            } else {
                Tips.error('未找到该群聊')
                uni.redirectTo({url: '/pages/chat/index'})
            }
        })
    }
})

const initGroup = async () => {
    await loadHistoryMessage(true)
}

const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    try {
        if (force) {
            await chatStore.joinChannel(chan.value)
        }

        chatRef.value!.history.loading = true
        const name = chan.value
        let lastTime = 0
        if (!force)
            if (historyMsgs.value && historyMsgs.value.length) {
                lastTime = historyMsgs.value[0].time!
            }
        
        const res = await chatStore.getGroupHistory(name, lastTime, force)
        chatRef.value!.history.loading = false
        if (res.length < 20) {
            chatRef.value!.history.allLoaded = true
        }
    } catch (e) {
        console.error('[loadHistoryMessage] 错误:', e)
        chatRef.value!.history.loading = false
        Tips.error('加载历史消息失败，请重试')
    }
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
</script>

<route lang="json">
{
"style": {
"navigationBarTitleText": "群聊"
}
}
</route>
