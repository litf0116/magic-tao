<template>
    <view class="update-modal-mask" v-if="visible" @click="handleMaskClick">
        <view class="update-modal" @click.stop>
            <view class="update-header">
                <text class="update-title">发现新版本</text>
                <text class="update-version">V{{ versionInfo?.latestVersionName }}</text>
            </view>

            <view class="update-body">
                <scroll-view scroll-y class="update-content">
                    <text class="update-description">{{ versionInfo?.description || '版本更新' }}</text>
                    <view class="update-info">
                        <text class="info-item">文件大小: {{ fileSize }}</text>
                    </view>
                </scroll-view>

                <view v-if="downloading" class="progress-section">
                    <view class="progress-bar">
                        <view class="progress-fill" :style="{ width: progress + '%' }"></view>
                    </view>
                    <text class="progress-text">{{ progress }}%</text>
                </view>
            </view>

            <view class="update-footer">
                <button v-if="!isForceUpdate && !downloading" class="btn-cancel" @click="handleCancel">暂不更新</button>
                <button
                    class="btn-confirm"
                    :class="{ 'btn-full': isForceUpdate || downloading }"
                    :disabled="downloading"
                    @click="handleUpdate"
                >
                    {{ downloading ? '下载中...' : '立即更新' }}
                </button>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

interface UpdateInfo {
    hasUpdate: boolean
    latestVersionCode: number
    latestVersionName: string
    description: string
    downloadUrl: string
    fileName: string
    fileSize: number
    isForceUpdate: boolean
    releaseDate: string
}

interface Props {
    visible: boolean
    versionInfo: UpdateInfo | null
    downloading?: boolean
    progress?: number
}

interface Emits {
    (e: 'update:visible', value: boolean): void
    (e: 'confirm'): void
    (e: 'cancel'): void
}

const props = withDefaults(defineProps<Props>(), {
    downloading: false,
    progress: 0
})

const emit = defineEmits<Emits>()

const isForceUpdate = computed(() => props.versionInfo?.isForceUpdate || false)
const fileSize = computed(() => {
    if (!props.versionInfo?.fileSize) return '未知'
    const size = props.versionInfo.fileSize
    if (size < 1024) return size + ' B'
    if (size < 1024 * 1024) return (size / 1024).toFixed(2) + ' KB'
    return (size / (1024 * 1024)).toFixed(2) + ' MB'
})

const handleMaskClick = () => {
    if (!isForceUpdate.value && !props.downloading) {
        emit('update:visible', false)
    }
}

const handleCancel = () => {
    emit('update:visible', false)
    emit('cancel')
}

const handleUpdate = () => {
    emit('confirm')
}
</script>

<style scoped>
.update-modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
}

.update-modal {
    width: 600rpx;
    max-height: 800rpx;
    background-color: #fff;
    border-radius: 24rpx;
    overflow: hidden;
}

.update-header {
    padding: 40rpx 32rpx;
    text-align: center;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.update-title {
    display: block;
    font-size: 36rpx;
    font-weight: bold;
    color: #fff;
    margin-bottom: 8rpx;
}

.update-version {
    display: block;
    font-size: 28rpx;
    color: rgba(255, 255, 255, 0.9);
}

.update-body {
    padding: 32rpx;
}

.update-content {
    max-height: 300rpx;
    margin-bottom: 32rpx;
}

.update-description {
    display: block;
    font-size: 28rpx;
    color: #333;
    line-height: 1.6;
    white-space: pre-wrap;
}

.update-info {
    margin-top: 16rpx;
    padding-top: 16rpx;
    border-top: 1rpx solid #eee;
}

.info-item {
    display: block;
    font-size: 24rpx;
    color: #999;
}

.progress-section {
    margin-top: 24rpx;
}

.progress-bar {
    height: 8rpx;
    background-color: #f0f0f0;
    border-radius: 4rpx;
    overflow: hidden;
    margin-bottom: 16rpx;
}

.progress-fill {
    height: 100%;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    transition: width 0.3s;
}

.progress-text {
    display: block;
    text-align: center;
    font-size: 24rpx;
    color: #666;
}

.update-footer {
    display: flex;
    padding: 0 32rpx 32rpx;
    gap: 24rpx;
}

.btn-cancel,
.btn-confirm {
    flex: 1;
    height: 88rpx;
    line-height: 88rpx;
    text-align: center;
    border-radius: 16rpx;
    font-size: 32rpx;
    border: none;
}

.btn-cancel {
    background-color: #f5f5f5;
    color: #666;
}

.btn-confirm {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: #fff;
}

.btn-confirm:disabled {
    opacity: 0.6;
}

.btn-full {
    flex: 1;
}
</style>