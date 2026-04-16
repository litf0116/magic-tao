<template>
    <view class="action-popup" v-if="visible">
        <view class="popup-mask" @click="close"></view>
        <view class="popup-content">
            <view class="popup-header">
                <text>操作</text>
                <view class="close-btn" @click="close">✕</view>
            </view>
            <view class="popup-body">
                <view 
                    v-for="action in actions" 
                    :key="action.key"
                    class="action-item"
                    @click="handleAction(action.key)"
                >
                    <text>{{ action.label }}</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Action {
    key: string
    label: string
}

const props = defineProps<{
    message: any
    currentUserId: number
}>()

const emit = defineEmits<{
    close: []
    action: [key: string]
}>()

const visible = ref(true)

const actions = ref<Action[]>([
    { key: 'copy', label: '复制' },
    { key: 'delete', label: '删除' },
    { key: 'reply', label: '回复' },
    { key: 'forward', label: '转发' }
])

const close = () => {
    visible.value = false
    emit('close')
}

const handleAction = (key: string) => {
    emit('action', key)
    close()
}
</script>

<style scoped>
.action-popup {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 999;
}

.popup-mask {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
}

.popup-content {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background: white;
    border-radius: 16rpx 16rpx 0 0;
    max-height: 70vh;
}

.popup-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 32rpx;
    border-bottom: 1rpx solid #eee;
}

.popup-header text {
    font-size: 32rpx;
    font-weight: bold;
}

.close-btn {
    font-size: 36rpx;
    color: #999;
    cursor: pointer;
}

.popup-body {
    padding: 16rpx 0;
}

.action-item {
    padding: 32rpx;
    border-bottom: 1rpx solid #f5f5f5;
    cursor: pointer;
    transition: background-color 0.2s;
}

.action-item:hover {
    background-color: #f8f8f8;
}

.action-item:last-child {
    border-bottom: none;
}

.action-item text {
    font-size: 28rpx;
    color: #333;
}
</style>