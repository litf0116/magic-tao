<template>
    <view class="message-list">
        <view v-if="history.loading" class="history-loaded">
            <image src="../../static/images/loading.svg" />
        </view>
        <view v-else :class="history.allLoaded ? 'history-loaded' : 'load'" @click="$emit('loadHistory', false)">
            <view>{{ history.allLoaded ? '已经没有更多的历史消息' : '点击获取历史消息' }}</view>
        </view>

        <view v-for="(message, index) in messages" :key="message.id">
            <TimeDivider v-if="shouldShowTime(message, index)" :time="formatTime(message.time)" />

            <MessageItem
                :message="message"
                :previous-message="messages[index - 1]"
                :current-user-id="currentUserId"
                :chat-type="chatType"
                @show-action="$emit('showAction', message)"
                @show-detail="$emit('showDetail', message)"
                @show-image="$emit('showImage', message)"
                @auction-action="$emit('auctionAction', $event)"
            />
        </view>
    </view>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import dayjs from 'dayjs'
import { ChatMessage } from '@/composables/types'
import TimeDivider from './TimeDivider.vue'
import MessageItem from './MessageItem.vue'

const props = defineProps<{
    messages: ChatMessage[]
    history: {
        loading: boolean
        allLoaded: boolean
    }
    currentUserId: number
    chatType: 'private' | 'group'
}>()

const emit = defineEmits<{
    loadHistory: [scrollToBottom: boolean]
    showAction: [message: ChatMessage]
    showDetail: [message: ChatMessage]
    showImage: [message: ChatMessage]
    auctionAction: [data: any]
}>()

const shouldShowTime = (message: ChatMessage, index: number): boolean => {
    if (index === 0) return true
    const prevMessage = props.messages[index - 1]
    const timeDiff = message.time - prevMessage.time
    return timeDiff > 5 * 60 * 1000 // 5分钟间隔显示时间
}

const formatTime = (timestamp: number): string => {
    return dayjs(timestamp).format('HH:mm')
}
</script>

<style scoped>
.message-list {
    flex: 1;
    overflow-y: auto;
    padding: 20rpx;
}

.history-loaded {
    text-align: center;
    padding: 20rpx;
    color: #999;
    font-size: 24rpx;
}

.load {
    text-align: center;
    padding: 20rpx;
    color: #0066cc;
    font-size: 24rpx;
    cursor: pointer;
}

.load:hover {
    color: #0052a3;
}
</style>
