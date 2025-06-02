<template>
    <view class="text-content">
        <text>{{ message.msg || '' }}</text>
    </view>
</template>

<script setup lang="ts">
import type { ChatMessage } from '@/composables/types'
import { defineProps } from 'vue'
const props = defineProps<{ message: ChatMessage }>()
const { message } = props
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
