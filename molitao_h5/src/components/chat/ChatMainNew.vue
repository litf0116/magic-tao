<template>
    <view class="chat-interface" @contextmenu.prevent="">
        <MessageList
            :messages="historyMsgs"
            :history="history"
            :current-user-id="userStore.user.id"
            :chat-type="chatOptions?.chatType || 'group'"
            @load-history="loadHistoryMessage"
            @show-action="showActionPopup"
            @show-detail="goShowDetails"
            @show-image="showImageFullScreen"
            @auction-action="onAuctionStartAction"
        />

        <ChatInput
            @send-message="sendTextMessage"
            @send-image="handleImageUpload"
            @send-file="handleFileUpload"
            @send-location="handleLocationSend"
        />

        <!-- 原有的弹窗和状态保持不变 -->
        <userProfile ref="popupDetailRef" :show-item="showItem" :chat-options="chatOptions" />

        <!-- 群聊规则弹窗 -->
        <view v-if="showGroupChatRules" class="modal-mask" @tap="showGroupChatRules = false">
            <view class="modal-content" @tap.stop>
                <view class="modal-header">
                    <text>群聊规则</text>
                    <view class="close-btn" @tap="showGroupChatRules = false">✕</view>
                </view>
                <view class="modal-body">
                    <text>这里是群聊规则内容...</text>
                </view>
            </view>
        </view>

        <!-- 动作弹窗 -->
        <ActionPopup
            v-if="showActionPopupVisible"
            :message="selectedMessage"
            :current-user-id="userStore.user.id"
            @close="showActionPopupVisible = false"
            @action="handleAction"
        />
    </view>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'
import { useEventBus } from '@vueuse/core'
import { useUserStore } from '@/stores/userStore'
import { useChatStore } from '@/stores/chatStore'
import MessageList from './MessageList.vue'
import ChatInput from './ChatInput.vue'
import userProfile from './userProfile.vue'
import ActionPopup from './ActionPopup.vue'
import type { ChatOptions } from './types'
import type { ChatMessage } from '@/composables/types'

// Props 定义
const props = defineProps<{
    options: ChatOptions
}>()

// Emits 定义
const emit = defineEmits<{
    loadHistoryMessage: [scrollToBottom: boolean]
    onSend: [message: any]
    showDetail: [message: any]
}>()

// Store 引用
const userStore = useUserStore()
const chatStore = useChatStore()

// 组件状态
const showItem = ref<any>(null)
const popupDetailRef = ref<any>(null)
const showGroupChatRules = ref(false)
const showActionPopupVisible = ref(false)
const selectedMessage = ref<ChatMessage | null>(null)

// 计算属性
const chatOptions = computed(() => props.options)
const historyMsgs = computed(() => chatStore.currentHistoryMsgs)
const history = computed(() => ({
    loading: chatStore.historyLoading,
    allLoaded: chatStore.historyAllLoaded,
}))

// 方法定义
const loadHistoryMessage = (scrollToBottom: boolean) => {
    emit('loadHistoryMessage', scrollToBottom)
}

const sendTextMessage = (text: string) => {
    emit('onSend', { type: 'text', content: text })
}

const handleImageUpload = () => {
    // 处理图片上传逻辑
    console.log('处理图片上传')
}

const handleFileUpload = () => {
    // 处理文件上传逻辑
    console.log('处理文件上传')
}

const handleLocationSend = () => {
    // 处理位置发送逻辑
    console.log('处理位置发送')
}

const showActionPopup = (message: ChatMessage) => {
    selectedMessage.value = message
    showActionPopupVisible.value = true
}

const goShowDetails = (message: ChatMessage) => {
    emit('showDetail', message)
}

const showImageFullScreen = (message: ChatMessage) => {
    // 处理图片全屏显示
    console.log('显示图片全屏', message)
}

const onAuctionStartAction = (data: any) => {
    // 处理拍卖开始动作
    console.log('拍卖开始动作', data)
}

const handleAction = (action: string) => {
    // 处理动作弹窗的回调
    console.log('处理动作', action)
    showActionPopupVisible.value = false
}

// 监听消息变化，自动滚动到底部
watch(
    historyMsgs,
    () => {
        nextTick(() => {
            // 滚动到最新消息的逻辑
            const scrollView = document.getElementById('scrollview')
            if (scrollView) {
                scrollView.scrollTop = scrollView.scrollHeight
            }
        })
    },
    { deep: true }
)
</script>

<style scoped>
.chat-interface {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background: #f5f5f5;
}

.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 999;
}

.modal-content {
    background: white;
    border-radius: 16rpx;
    width: 80%;
    max-width: 600rpx;
    max-height: 80vh;
    overflow-y: auto;
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 32rpx;
    border-bottom: 1rpx solid #eee;
}

.modal-header text {
    font-size: 32rpx;
    font-weight: bold;
}

.close-btn {
    font-size: 36rpx;
    color: #999;
    cursor: pointer;
}

.modal-body {
    padding: 32rpx;
}
</style>
