<template>
    <div class="flex-1 flex">
        <div class="chat-container">
            <div class="px-4 z-10 h-65px bg-[#E5D9D9] text-[#82615F] flex items-center">
                <div class="font-700 text-18px">拍卖行</div>
            </div>
            <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
        </div>
    </div>
    <div
        class="min-w-260px md:w-260px h-full flex flex-col border-0 border-l-1 md:border-solid md:border-gray-300 min-h-700px">
        <!-- <GroupList /> -->
        <AuctionList />
    </div>
</template>

<script setup lang="ts">
import { ChatMessageType } from '@/api/appService'
import chatMain from '@/components/Chat/chatMain.vue'
import AuctionList from '@/components/Chat/AuctionList.vue'
const chatStore = useChatStore()

const chatRef = ref<InstanceType<typeof chatMain> | null>(null)

onMounted(() => {
    init('-1_auction')
})

const init = async (name: string) => {
    chatStore.SetCurrentChatId(-1, 'auction', true)
    await loadHistoryMessage(true)
}

const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    if (force) {
        await chatStore.joinChannel('-1_auction')
    }
    chatRef.value!.history.loading = true

    let lastTime = new Date().getTime()
    if (!force)
        if (historyMsgs.value && historyMsgs.value.length) {
            lastTime = historyMsgs.value[0].time!
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
        chatStore.sendChannelMsg('[图片]', '', ChatMessageType.Image, e.data).then(() => { })
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendChannelMsg(e.data as string, '', ChatMessageType.Text).then(() => {
            //
        })
    }
}
</script>
