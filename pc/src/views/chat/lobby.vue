<template>
    <div class="flex-1 flex">
        <div class="chat-container">
            <div class="px-4 z-10 h-65px bg-[#E5D9D9] text-[#82615F] flex items-center justify-between">
                <div class="font-700 text-18px">{{ chatStore.getCurrentName() }}</div>
                <div>
                    <el-button
                        v-if="chatStore.currentChat.id! !== -userStore.user.id! && chan !== '0_lobby'"
                        @click="chatStore.leaveChannel(chan)"
                        >退出</el-button
                    >
                </div>
            </div>
            <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
        </div>
    </div>
    <div
        class="min-w-260px md:w-260px h-full flex flex-col border-0 border-l-1 md:border-solid md:border-gray-300 min-h-700px"
    >
        <GroupList />
    </div>
</template>

<script setup lang="ts">
import { ChatMessageType } from '@/api/appService'
import chatMain from '@/components/Chat/chatMain.vue'

import GroupList from '@/components/Chat/GroupList.vue'

const chatStore = useChatStore()
const userStore = useUserStore()
const route = useRoute()
const router = useRouter()

const chan = ref('')
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)

onMounted(async () => {
    chan.value = '0_lobby'
    await chatStore.getGropus().then(async () => {
        if (chatStore.hasGroup(chan.value)) {
            await chatStore.SetCurrentChatId(0, chan.value.split('_')[1], true).then(() => {
                initGroup(chan.value)
            })
        } else {
            Tips.error('未找到该群聊')
            router.push({ name: 'chatIndex', replace: true })
        }
    })
})

const initGroup = async (name: string) => {
    await loadHistoryMessage(true)
}

const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    if (force) {
        await chatStore.joinChannel(chan.value)
    }

    chatRef.value!.history.loading = true
    // const name = `${chatStore.currentChat.id}_${chatStore.currentChat.name}`
    const name = chan.value
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

// watch(
//     () => route.params.id,
//     async (newId) => {
//         console.log('watch params newId', newId)
//         if (newId) {
//             await initGroup(newId + '')
//         }
//     }
// )
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
