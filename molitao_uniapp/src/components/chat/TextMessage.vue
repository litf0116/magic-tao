<template>
    <view class="text-content" @tap="handleTap">
        <rich-text :nodes="renderedText"></rich-text>
    </view>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { defineProps, defineEmits } from 'vue'
import { useChatEmojiStore } from '@/stores/chatEmojiStore'
import EmojiDecoder from '@/composables/emojiDecoder'
import type { ChatMessage } from '@/composables/types' // 路径以实际为准

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
</script>

<style scoped>
.text-content {
    padding: 16rpx;
    border-radius: 12rpx;
    color: #000000;
    background: #ffffff;
    word-break: break-all;
    text-align: left;
    vertical-align: center;
    display: block;
}
</style>
