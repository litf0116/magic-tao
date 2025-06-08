<template>
    <div class="flex-1 flex">
        <div class="chat-container">
            <div class="px-4 z-10 h-65px bg-[#E5D9D9] text-[#82615F] flex items-center">
                <img :src="friend.avatar" class="chat-avatar" />
                <div class="font-700 text-18px">{{ friend.name }}</div>
            </div>
            <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
        </div>
    </div>
    <div
        class="min-w-260px md:w-260px h-full flex flex-col border-0 border-l-1 md:border-solid md:border-gray-300 min-h-700px"
    >
        <GroupList :item="friend" />
    </div>
</template>

<script setup lang="ts">
import chatMain from '@/components/Chat/chatMain.vue'
import { ChatMessageType } from '@/api/appService'
import api from '@/api'

const chatStore = useChatStore()
const route = useRoute()
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)
const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${friend.id}`) || []
})

const friend = reactive({
    id: 0,
    name: '',
    avatar: '',
	qq:"",
	weChat:""
})

onMounted(() => {
    console.log('onMounted', route.params, route.query)
    const id = parseInt(route.params.id + '')
    if (isNaN(id)) {
        console.error('Invalid friend id:', route.params.id)
        return
    }
    friend.id = id
    friend.name = route.query.name as string
    friend.avatar = route.query.avatar as string

    api.user.get({ id: friend.id }).then((res) => {
        friend.name = res.name;
        friend.avatar = res.headImgUrl;
		friend.qq=res.qq;
		friend.weChat=res.wx;
    })

    chatStore.addChatList(friend.id, friend.name, friend.avatar)
    chatStore.SetCurrentChatId(friend.id)

    loadHistoryMessage(true)
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
        chatStore.sendMsg(friend.id, friend.name, friend.avatar, '[图片]', ChatMessageType.Image, e.data).then(() => {
            //todo
        })
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendMsg(friend.id, friend.name, friend.avatar, e.data as string).then(() => {
            //todo
        })
    }
}
</script>
