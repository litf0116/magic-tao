<template>
    <view class="kasec-status-message" :class="kasecStatusClass" @tap="handleTap">
        <view class="kasec-icon">⚡</view>
        <view class="kasec-content">
            <rich-text :nodes="renderedText"></rich-text>
        </view>
    </view>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { defineProps, defineEmits } from 'vue'
import { useChatEmojiStore } from '@/stores/chatEmojiStore'
import EmojiDecoder from '@/composables/emojiDecoder'
import type { ChatMessage } from '@/composables/types'

const props = defineProps<{ message: ChatMessage }>()
const emit = defineEmits(['tap'])

function handleTap(e) {
    emit('tap', props.message)
}

// 获取 emoji 配置
const emojiStore = useChatEmojiStore()
const decoder = new EmojiDecoder(emojiStore.emojiUrl, emojiStore.emojiMap)

const renderedText = computed(() => {
    if (!props.message?.msg) return ''
    // 解析 emoji 并处理换行
    return decoder.decode(props.message.msg.replace(/\n/g, '<br/>'))
})

// 根据payload中的isKasec状态确定样式类
const kasecStatusClass = computed(() => {
    let payload = props.message.payload
    if (typeof payload === 'string') {
        payload = JSON.parse(payload)
    }
    const isKasec = payload?.isKasec
    return isKasec ? 'kasec-enabled' : 'kasec-disabled'
})
</script>

<style scoped>
.kasec-status-message {
    display: flex;
    align-items: center;
    padding: 24rpx;
    border-radius: 20rpx;
    color: #000000;
    word-break: break-all;
    text-align: left;
    vertical-align: center;
    margin: 12rpx 0;
    border: 4rpx solid;
    font-weight: 600;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.1);
}

.kasec-enabled {
    background: linear-gradient(135deg, #fff5f5 0%, #fed7d7 100%);
    border-color: #e53e3e;
    color: #c53030;
}

.kasec-disabled {
    background: linear-gradient(135deg, #f0fff4 0%, #c6f6d5 100%);
    border-color: #38a169;
    color: #2f855a;
}

.kasec-icon {
    font-size: 36rpx;
    margin-right: 16rpx;
    animation: pulse 2s infinite;
}

.kasec-enabled .kasec-icon {
    color: #e53e3e;
}

.kasec-disabled .kasec-icon {
    color: #38a169;
}

.kasec-content {
    flex: 1;
}

@keyframes pulse {
    0% {
        transform: scale(1);
    }
    50% {
        transform: scale(1.1);
    }
    100% {
        transform: scale(1);
    }
}
</style> 