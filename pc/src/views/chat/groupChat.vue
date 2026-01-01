<template>
    <div class="flex-1 flex">
        <div class="chat-container">
            <div class="px-4 z-10 h-65px bg-[#E5D9D9] text-[#82615F] flex items-center justify-between">
                <div class="font-700 text-18px">{{ chatGroup.title }}</div>
                <div>
                    <!-- <el-button v-if="!isMyGroup" @click="chatStore.leaveChannel(chatGroup.chan)">退出</el-button> -->
                </div>
            </div>
            <chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage"></chatMain>
        </div>
    </div>
    <div
        class="min-w-260px md:w-260px h-full flex flex-col border-0 border-l-1 md:border-solid md:border-gray-300 min-h-700px"
    >
        <div class="flex-1" style="max-height: calc(700px - 2.5rem)" @contextmenu.prevent>
            <div id="ps_container" style="max-height: calc(700px - 12rem)" class="space-y-2 overflow-hidden h-full p-2">
                <div>
                    <div class="flex items-center justify-between">
                        <div class="font-600 text-16px">群聊成员 {{ users.length }} / {{ chatGroup.limit }}</div>
                        <div>
                            <el-switch
                                v-if="isMyGroup"
                                v-model="chatGroup.isHidden"
                                size="small"
                                active-text="公开"
                                inactive-text="隐藏"
                                :active-value="false"
                                :inactive-value="true"
                                @change="toggleHidden"
                            />
                        </div>
                    </div>
                    <div class="h-1"></div>
                </div>
                <div v-for="(u, k) in users" :key="k">
                    <div v-motion-fade-visible class="flex items-center justify-between">
                        <div class="flex items-center">
                            <img :src="getImgUrl(u.headImgUrl)" class="w-8 h-8 rounded" />
                            <div class="ml-2 text-gray-600">{{ u.name }}</div>
                        </div>

                        <div
                            v-if="isMyGroup && u.id !== userStore.user.id"
                            class="text-sm underline text-red"
                            @click="kick(u.id)"
                        >
                            踢出
                        </div>
                    </div>
                </div>
                <div class="h-16"></div>
            </div>
        </div>
        <div class="shadow h-10 grid grid-cols-2">
            <div class="bg-gray-500 text-white cursor-pointer flex flex-center" @click="getUsers">刷新用户列表</div>
            <div
                v-if="isMyGroup"
                class="bg-orange-300 text-white cursor-pointer flex flex-center"
                @click="deleteChannel"
            >
                删除频道
            </div>
            <div
                v-else
                class="bg-orange-300 text-white cursor-pointer flex flex-center"
                @click="chatStore.leaveChannel(chatGroup.chan)"
            >
                退出频道
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ChatGroupDto, ChatMessageType, UserDto } from '@/api/appService'
import chatMain from '@/components/Chat/chatMain.vue'
import { getImgUrl } from '@/composables'
import { useEventBus } from '@vueuse/core'
import { ElMessageBox } from 'element-plus'

const bus = useEventBus(onmessageKey)
//LINK[epic=处理收到消息] - ChatMain处理收到消息
const unsubscribe = bus.on(async (msg: any) => {
    console.log('groupChat onmessageKey', msg)
    if (msg.type === 'Welcome' && msg.chan === chan.value) {
        getUsers()
    }
})

onUnmounted(() => {
    unsubscribe()
})

const chatStore = useChatStore()
const userStore = useUserStore()
const route = useRoute()
const router = useRouter()
const chan = ref('')
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)

const groupId = ref(0)
const chatGroup = ref<ChatGroupDto>({})

onMounted(async () => {
    console.log('onMounted', route.params.id)
    if (route.params.id) {
        const t = route.params.id + ''
        groupId.value = Math.abs(parseInt(t.split('_')[0]))
        chan.value = t

        api.chatGroup
            .get({ id: groupId.value })
            .then((res) => {
                chatStore.SetCurrentChatId(-res.id, res.title, true).then(() => {
                    initGroup(t)
                })
            })
            .catch(() => {
                Tips.error('频道不存在')
                router.push({ name: 'chatIndex', replace: true })
            })
    }
})

function deleteChannel() {
    ElMessageBox.confirm('确定删除该组队聊天吗?', '删除组队聊天', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            await chatStore.deleteChannel(chatGroup.value).then(() => {
                Tips.success('删除成功')
                router.push({ name: 'chatIndex', replace: true })
            })
        })
        .catch(() => {
            Tips.info('取消删除')
        })
}

function toggleHidden(e: boolean) {
    console.log('toggleHidden', e)
    // api.chatGroup.update({ id: chatGroup.value.id, isHidden: e }).then(() => {
    //     chatGroup.value.isHidden = e
    // })
    api.chatGroup.toggleHidden({ id: chatGroup.value.id }).then((res) => {
        Tips.success('修改成功')
        chatGroup.value = res
    })
}

const users = ref<UserDto[]>([])

const initGroup = async (name: string) => {
    await getChatGroup()
    await loadHistoryMessage(true)
    // getUsers()
}

const isMyGroup = computed(() => {
    return chatGroup.value?.creatorUserId === userStore.user.id
})

const getChatGroup = () => {
    api.chatGroup.get({ id: groupId.value }).then((res) => {
        chatGroup.value = res
    })
}

function getUsers() {
    users.value = []
    api.chatGroup.getGroupUser({ chan: chan.value }).then((res) => {
        users.value = res.items!
    })
}

// 踢出频道
function kick(userId: number) {
    console.log('kick', userId)
    api.chatGroup.kickUser({ id: chatGroup.value.id, userId }).then(() => {
        getUsers()
    })
}

const historyMsgs = computed(() => {
    return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
    if (force) {
        await chatStore.joinChannel(chan.value)
    }

    chatRef.value!.history.loading = true
    let lastTime = 0
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
