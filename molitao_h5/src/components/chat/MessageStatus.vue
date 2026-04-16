<template>
    <view class="message-status">
        <view v-if="message.status === ChatMessageStatus.Sending" class="message-sending">
            <text>发送中...</text>
        </view>
        <view v-else-if="message.status === ChatMessageStatus.Failed" class="message-failed">
            <text>发送失败</text>
        </view>
        <view v-else-if="message.isRead" class="message-read">
            <text>已读</text>
        </view>
        <view v-else-if="message.from === currentUserId" class="message-unread">
            <text>未读</text>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ChatMessage, ChatMessageStatus } from '@/composables/types'

const props = defineProps<{
    message: ChatMessage
    currentUserId: number
}>()
</script>

<style scoped>
.message-status {
    margin-top: 8rpx;
    text-align: right;
}

.message-sending,
.message-failed,
.message-read,
.message-unread {
    font-size: 24rpx;
    height: 36rpx;
}

.message-sending {
    color: #999;
}

.message-failed {
    color: #ff4444;
}

.message-read {
    color: #999;
}

.message-unread {
    color: #d02129;
}
</style>