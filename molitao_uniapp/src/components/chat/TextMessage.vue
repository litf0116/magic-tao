<template>
    <view class="text-content" @tap="handleTap">
        <text>{{ message.msg || '' }}</text>
    </view>
</template>

<script setup lang="ts">
import type { ChatMessage } from '@/composables/types'
import { defineProps, defineEmits } from 'vue'
const props = defineProps<{ message: ChatMessage }>()
const emit = defineEmits(['tap'])
const { message } = props

function handleTap(e) {
    emit('tap', props.message)
}
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
