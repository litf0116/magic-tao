<template>
    <view class="action-panel" v-if="visible">
        <view class="panel-header">
            <text>更多功能</text>
            <view class="close-btn" @click="close">✕</view>
        </view>
        <view class="panel-content">
            <view class="action-grid">
                <view class="action-item" @click="handleImage">
                    <view class="action-icon">📷</view>
                    <text class="action-label">图片</text>
                </view>
                <view class="action-item" @click="handleFile">
                    <view class="action-icon">📄</view>
                    <text class="action-label">文件</text>
                </view>
                <view class="action-item" @click="handleLocation">
                    <view class="action-icon">📍</view>
                    <text class="action-label">位置</text>
                </view>
                <view class="action-item" @click="handleVideo">
                    <view class="action-icon">🎥</view>
                    <text class="action-label">视频</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
    visible: boolean
}>()

const emit = defineEmits<{
    sendImage: []
    sendFile: []
    sendLocation: []
    close: []
}>()

const handleImage = () => {
    emit('sendImage')
    close()
}

const handleFile = () => {
    emit('sendFile')
    close()
}

const handleLocation = () => {
    emit('sendLocation')
    close()
}

const handleVideo = () => {
    // 预留视频功能
    console.log('Video feature coming soon')
    close()
}

const close = () => {
    emit('close')
}
</script>

<style scoped>
.action-panel {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    background: white;
    border-radius: 16rpx 16rpx 0 0;
    z-index: 999;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 32rpx;
    border-bottom: 1rpx solid #eee;
}

.panel-header text {
    font-size: 32rpx;
    font-weight: bold;
}

.close-btn {
    font-size: 36rpx;
    color: #999;
    cursor: pointer;
}

.panel-content {
    padding: 32rpx;
}

.action-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 32rpx;
}

.action-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 24rpx;
    border-radius: 16rpx;
    cursor: pointer;
    transition: background-color 0.2s;
}

.action-item:hover {
    background-color: #f8f8f8;
}

.action-icon {
    font-size: 48rpx;
    margin-bottom: 16rpx;
}

.action-label {
    font-size: 24rpx;
    color: #666;
}
</style>