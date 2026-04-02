<template>
    <div class="flex h-screen overflow-hidden bg-[#F3F3F3]">
        <!-- 保证金提示条 -->
        <div v-if="isDepositInsufficient" class="deposit-warning">
            <div class="flex items-center gap-2">
                <span class="text-[#856404]">参与竞拍需缴纳保证金 (50 元)</span>
            </div>
            <el-button type="warning" size="small" @click="goToDepositPayment">立即缴纳</el-button>
        </div>

        <div class="chat-container">
            <div class="px-4 z-10 h-65px bg-[#E5D9D9] text-[#82615F] flex items-center">
                <div class="font-700 text-18px">拍卖行</div>
            </div>
            <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
        </div>
        <div
            class="min-w-260px md:w-260px h-full flex flex-col border-0 border-l-1 md:border-solid md:border-gray-300 min-h-700px"
        >
            <!-- <GroupList /> -->
            <AuctionList />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ChatMessageType } from '@/api/appService'
import chatMain from '@/components/Chat/chatMain.vue'
import AuctionList from '@/components/Chat/AuctionList.vue'
import { ElMessage } from 'element-plus'

const chatStore = useChatStore()
const userStore = useUserStore()
const router = useRouter()

// 检查保证金是否充足
const isDepositInsufficient = computed(() => {
    const deposit = userStore.user.depositBalance || 0
    const userLevel = userStore.user.userLevel || 0
    return userLevel === 0 && deposit < 50
})

// 跳转到支付页面
const goToDepositPayment = () => {
    router.push('/deposit-payment')
}

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
    // 检查保证金是否充足
    const deposit = userStore.user.depositBalance || 0
    const userLevel = userStore.user.userLevel || 0
    if (userLevel === 0 && deposit < 50) {
        ElMessage.warning('新用户参与竞拍需要缴纳保证金 (50 元)')
        goToDepositPayment()
        return
    }

    if (e.type === ChatMessageType.Image) {
        chatStore.sendChannelMsg('[图片]', '', ChatMessageType.Image, e.data).then(() => {})
    } else if (e.type === ChatMessageType.Text) {
        chatStore.sendChannelMsg(e.data as string, '', ChatMessageType.Text).then(() => {
            //
        })
    }
}
</script>

<style scoped>
.deposit-warning {
    @apply bg-[#fffbe6] border border-[#ffe58f] px-4 py-3 rounded mx-4 mt-2;
    @apply flex justify-between items-center;
}
</style>
