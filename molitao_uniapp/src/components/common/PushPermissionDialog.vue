<template>
    <view class="push-permission-dialog">
        <view class="dialog-overlay" @tap="onCancel"></view>
        <view class="dialog-content">
            <view class="dialog-header">
                <text class="dialog-title">开启推送通知</text>
            </view>
            <view class="dialog-body">
                <view class="permission-icon">
                    <text class="icon-bell">🔔</text>
                </view>
                <view class="permission-desc">
                    <text class="desc-text">开启推送通知后，您可以及时收到以下信息：</text>
                    <view class="feature-list">
                        <view class="feature-item">
                            <text class="feature-icon">✓</text>
                            <text class="feature-text">拍卖开始提醒</text>
                        </view>
                        <view class="feature-item">
                            <text class="feature-icon">✓</text>
                            <text class="feature-text">出价被超越通知</text>
                        </view>
                        <view class="feature-item">
                            <text class="feature-icon">✓</text>
                            <text class="feature-text">拍卖结果通知</text>
                        </view>
                        <view class="feature-item">
                            <text class="feature-icon">✓</text>
                            <text class="feature-text">系统消息提醒</text>
                        </view>
                    </view>
                </view>
                <view class="permission-note">
                    <text class="note-text">您可以随时在设置中关闭推送通知</text>
                </view>
            </view>
            <view class="dialog-footer">
                <button class="btn-cancel" @tap="onCancel">暂不开启</button>
                <button class="btn-confirm" @tap="onConfirm">立即开启</button>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits<{
    confirm: []
    cancel: []
}>()

const onCancel = () => {
    emit('cancel')
}

const onConfirm = () => {
    emit('confirm')
}
</script>

<style lang="scss" scoped>
.push-permission-dialog {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
}

.dialog-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
}

.dialog-content {
    position: relative;
    width: 560rpx;
    background-color: #fff;
    border-radius: 24rpx;
    overflow: hidden;
    animation: dialog-in 0.3s ease-out;
}

@keyframes dialog-in {
    from {
        opacity: 0;
        transform: scale(0.9);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}

.dialog-header {
    padding: 48rpx 48rpx 24rpx;
    text-align: center;
}

.dialog-title {
    font-size: 36rpx;
    font-weight: 600;
    color: #1a1a1a;
}

.dialog-body {
    padding: 24rpx 48rpx 32rpx;
}

.permission-icon {
    text-align: center;
    margin-bottom: 32rpx;

    .icon-bell {
        font-size: 120rpx;
    }
}

.permission-desc {
    margin-bottom: 24rpx;
}

.desc-text {
    display: block;
    font-size: 28rpx;
    color: #666;
    line-height: 1.6;
    margin-bottom: 24rpx;
}

.feature-list {
    margin-top: 24rpx;
}

.feature-item {
    display: flex;
    align-items: center;
    margin-bottom: 16rpx;

    &:last-child {
        margin-bottom: 0;
    }
}

.feature-icon {
    width: 40rpx;
    height: 40rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f4835a;
    color: #fff;
    border-radius: 50%;
    font-size: 24rpx;
    margin-right: 16rpx;
    flex-shrink: 0;
}

.feature-text {
    font-size: 28rpx;
    color: #333;
    line-height: 1.5;
}

.permission-note {
    padding: 16rpx;
    background-color: #f5f5f5;
    border-radius: 12rpx;
}

.note-text {
    font-size: 24rpx;
    color: #999;
    text-align: center;
    display: block;
}

.dialog-footer {
    display: flex;
    border-top: 1rpx solid #eee;
}

.btn-cancel,
.btn-confirm {
    flex: 1;
    height: 96rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 32rpx;
    border: none;
    background: none;
    border-radius: 0;
    padding: 0;
    margin: 0;

    &::after {
        border: none;
    }
}

.btn-cancel {
    color: #666;
    border-right: 1rpx solid #eee;
}

.btn-confirm {
    color: #f4835a;
    font-weight: 600;
}
</style>
