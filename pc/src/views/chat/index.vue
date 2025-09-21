<template>
    <div class="w-full h-full relative flex flex-col md:flex-row text-true-gray-800">
        <div class="min-w-220px md:w-220px">
            <div class="conversation-list-container">
                <div class="conversation-list-content">
                    <div v-if="chatStore.chatList.length">
                        <router-link
                            v-for="(x, key) in orderBy(chatStore.chatList, ['order'], ['desc'])"
                            :key="key"
                            replace
                            :to="chatLocation(x)"
                            @click="chatListItemClick(x)"
                        >
                            <div>
                                <div
                                    class="conversation border-solid border-1 border-white hover:border-sky-500 hover:rounded-lg"
                                    :class="[x.id === chatStore.currentChat.id ? 'bg-[#faf1f0]' : 'bg-white']"
                                    @contextmenu.prevent="(e) => showRightClickMenu(e, x)"
                                >
                                    <div class="avatar">
                                        <img v-if="x.id && x.id > 0" :src="x.avatar" class="object-cover" />

                                        <template v-else>
                                            <div
                                                v-if="x.id === 0"
                                                class="bg-blue-600 text-white font-bold w-full h-full flex flex-center"
                                            >
                                                大厅
                                            </div>

                                            <div
                                                v-else-if="x.id === -1"
                                                class="bg-green-600 text-white font-bold w-full h-full flex flex-center"
                                            >
                                                拍卖
                                            </div>

                                            <div
                                                v-else
                                                class="bg-black text-white font-bold w-full h-full flex flex-center"
                                                :style="{ backgroundColor: getRandomColor(x.name) }"
                                            >
                                                组队
                                            </div>
                                        </template>
                                        <div v-if="x.unread" class="unread-count">
                                            <span class="unread">{{ x.unread }}</span>
                                        </div>
                                    </div>
                                    <div class="conversation-message">
                                        <div class="conversation-top">
                                            <span
                                                class="conversation-name text-left"
                                                :class="[
                                                    x.name === 'lobby'
                                                        ? 'text-blue-600 !font-bold'
                                                        : x.name === 'auction'
                                                        ? 'text-green-600 !font-bold'
                                                        : 'text-gray-700',
                                                ]"
                                                >{{
                                                    x.name === 'lobby'
                                                        ? '勇者招募所'
                                                        : x.name === 'auction'
                                                        ? '拍卖行'
                                                        : x.name
                                                }}</span
                                            >
                                            <div class="conversation-time">
                                                {{ dayjs(x.time).format('MM-DD HH:mm') }}
                                            </div>
                                        </div>
                                        <div class="conversation-bottom">
                                            <div class="line-clamp-1">
                                                {{ x.lastMsg }}
                                            </div>
                                        </div>
                                        <!-- <div class="conversation-bottom">
                                            <div class="conversation-content" v-if="conversation.lastMessage.recalled">
                                                <div v-if="conversation.type === 'private'">
                                                    {{
                                                        conversation.lastMessage.senderId === currentUser.id
                                                            ? '你'
                                                            : `"${conversation.data.name}"`
                                                    }}撤回了一条消息
                                                </div>
                                                <div v-if="conversation.type === 'group'">
                                                    {{
                                                        conversation.lastMessage.senderId === currentUser.id
                                                            ? '你'
                                                            : `"${conversation.lastMessage.senderData.name}"`
                                                    }}撤回了一条消息
                                                </div>
                                            </div>
                                            <div class="conversation-content" v-else>
                                                <div
                                                    class="unread-text"
                                                    v-if="
                                                        conversation.lastMessage.read === false &&
                                                        conversation.lastMessage.senderId === currentUser.id
                                                    "
                                                >
                                                    [未读]
                                                </div>
                                                <div v-if="conversation.type === 'private'">
                                                    {{
                                                        conversation.lastMessage.senderId === currentUser.id
                                                            ? '我'
                                                            : conversation.data.name
                                                    }}:
                                                </div>
                                                <div v-else>
                                                    {{
                                                        conversation.lastMessage.senderId === currentUser.id
                                                            ? '我'
                                                            : conversation.lastMessage.senderData.name
                                                    }}:
                                                </div>
                                                <span class="text" v-if="conversation.lastMessage.type === 'text'">{{
                                                    conversation.lastMessage.payload.text
                                                }}</span>
                                                <span v-else-if="conversation.lastMessage.type === 'image'"
                                                    >[图片消息]</span
                                                >
                                            </div>
                                        </div> -->
                                    </div>
                                </div>
                            </div>
                        </router-link>
                    </div>
                    <div v-else class="no-conversation">- 当前没有会话 -</div>
                </div>
            </div>
            <template v-if="rightClickMenu.visible">
                <teleport to="body">
                    <div
                        v-motion-fade
                        class="absolute z-50 bg-white shadow-lg rounded-md ring-orange-500 ring-2 overflow-hidden"
                        :style="{ left: rightClickMenu.x + 'px', top: rightClickMenu.y + 'px' }"
                    >
                        <!-- <div v-if="rightClickMenu.conversation" class="action-item" @click="topConversation">
    
        {{ rightClickMenu.conversation.top ? '取消置顶' : '置顶' }}
    
    </div> -->

                        <div
                            class="px-4 py-2 hover:bg-orange-500 hover:text-white hover:font-bold"
                            @click="deleteConversation"
                        >
                            删除聊天
                        </div>
                    </div>
                </teleport>
            </template>
        </div>
        <router-view :key="route.params.id + ''" />
        <el-image-viewer
            v-if="showViewer"
            :initial-index="viewerIndex"
            :url-list="urlList"
            @switch="viewerIndex = $event"
            @close="showViewer = false"
        />
    </div>
</template>

<script setup lang="ts">
import { ChatListItem } from '@/stores/chatStore'
import { ElMessageBox } from 'element-plus'
import { orderBy } from 'lodash'
import dayjs from 'dayjs'
import { convertImageUrl } from '@/utils/imageUrlConverter'
const chatStore = useChatStore()
const route = useRoute()

const showViewer = ref(false)
const viewerIndex = ref(0)
const urlList = ref([])

function showImageViewer(urls: string[]) {
    console.log('showImageViewer', urls)
    urlList.value = []
    nextTick(() => {
        viewerIndex.value = 0
        urlList.value = urls
        showViewer.value = true
    })
}

provide('showImageViewer', showImageViewer)

onMounted(() => {
    //隐藏Conversation右键菜单
    document.addEventListener('click', () => {
        hideRightClickMenu()
    })
})

const rightClickMenu = ref({
    conversation: null,
    visible: false,
    x: null,
    y: null,
})

function chatListItemClick(chat: ChatListItem) {
    console.log('chatListItemClick')
    chatStore.SetCurrentChat(chat)
    chatStore.SetUnread(chat, 0)
}

function showRightClickMenu(e: any, conversation: any) {
    console.log('showRightClickMenu', e)
    rightClickMenu.value.conversation = conversation
    rightClickMenu.value.visible = true
    rightClickMenu.value.x = e.pageX
    rightClickMenu.value.y = e.pageY
}

function hideRightClickMenu() {
    rightClickMenu.value.visible = false
}

function deleteConversation() {
    ElMessageBox.confirm('确认要删除这条会话吗？', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(() => {
        let conversation: ChatListItem = rightClickMenu.value.conversation!
        console.log('删除会话', conversation)
        chatStore.deleteChat(conversation)
    })
}

function chatLocation(chat: ChatListItem) {
    // console.log('chatLocation', chat)
    if (chat.type === ChatListItemType.user) {
        return {
            path: `/chat/index/privateChat/${chat.id}`,
            query: { name: chat.name, avatar: convertImageUrl(chat.avatar) || 'http://image.molitao.top/avater.png' },
        }
    } else if (chat.id === 0) {
        return { path: `/chat/index/lobby` }
    } else if (chat.id === -1) {
        return { path: `/chat/auction/auction` }
    } else {
        return { path: `/chat/index/groupChat/${chat.id}_${chat.name}` }
    }
}

//根据传入的string不同,获得不同的随机颜色
const getRandomColor = (str: string) => {
    if (!str) return '#000'
    let hash = 0
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash)
    }
    let color = '#'
    for (let i = 0; i < 3; i++) {
        let value = (hash >> (i * 8)) & 0xff
        color += ('00' + value.toString(16)).slice(-2)
    }
    return color
}
</script>

<style>
.conversations {
    width: 100%;
    height: 100%;
    position: relative;
    display: flex;
    color: #333333;
}

.conversation-list {
    width: 220px;
}

.conversation-list-container {
    height: 700px;
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
    /* background-color: white; */
    border-right: #dbd6d6 1px solid;
}

.conversation-list-content {
    flex: 1;
    overflow-y: auto;
    padding: 10px 0;
    scrollbar-width: none;
    -ms-overflow-style: none;
}

.conversation-list-content::-webkit-scrollbar {
    display: none;
}

.no-conversation {
    text-align: center;
    color: #666666;
}

.conversation {
    display: flex;
    padding: 10px 5px;
    cursor: pointer;
}

.unread-count {
    position: absolute;
    top: -10px;
    left: 30px;
    width: 18px;
    height: 18px;
    border-radius: 50%;
    color: white;
    background: #d02129;
}

.unread-count .unread {
    display: block;
    font-size: 12px;
    text-align: center;
    line-height: 18px;
}

.conversation-message {
    flex: 1;
    padding-left: 5px;
    display: flex;
    width: 160px;
    flex-direction: column;
    justify-content: space-around;
}

.conversation-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    text-align: right;
}

.conversation-name {
    font-size: 12px;
    font-weight: 500;
}

.conversation-time {
    /* width: 75px; */
    color: #b9b9b9;
    display: flex;
    flex-direction: column;
    font-size: 12px;
}

.conversation-bottom {
    @apply text-sm;
    display: flex;
    color: #666666;
}

.conversation-content {
    display: flex;
    width: 160px;
    color: #b3b3b3;
}

.conversation-content .text {
    overflow: hidden;
    text-overflow: ellipsis;
    flex: 1;
    white-space: nowrap;
    word-break: break-all;
}

.conversation-bottom .unread-text {
    color: #d02129;
    width: 35px !important;
}

.conversation .avatar {
    width: 40px;
    height: 40px;
    position: relative;
}

.conversation .avatar img {
    width: 100%;
    border-radius: 10%;
}

.router-link-active {
    background: #eeeeee;
}
</style>

<style>
.chat-container {
    @apply flex-1 flex flex-col max-h-700px h-ull flex flex-col relative;
}

.chat-title {
    @apply h-10 px-4 flex items-center text-lg;
}

.chat-avatar {
    @apply size-9;
}

.chat-name {
    @apply text-lg font-bold ml-2 overflow-hidden line-clamp-1;
}
</style>
