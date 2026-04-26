<template>
    <view class="qrcode-scanner-page">
        <!-- 顶部导航 -->
        <view class="scanner-header">
            <view class="back-btn" @tap="handleBack">
                <text class="back-icon">‹</text>
                <text class="back-text">返回</text>
            </view>
            <text class="header-title">扫描二维码</text>
            <view class="placeholder"></view>
        </view>

        <!-- 扫码区域 -->
        <view class="scanner-container">
            <!-- 相机预览 -->
            <view id="qr-reader" class="qr-reader"></view>

            <!-- 扫描提示 -->
            <view class="scan-hint">
                <text class="hint-text">将二维码放入框内，即可自动扫描</text>
            </view>

            <!-- 加载状态 -->
            <view v-if="isInitializing" class="loading-overlay">
                <text class="loading-text">正在启动相机...</text>
            </view>

            <!-- 错误状态 -->
            <view v-if="errorMessage" class="error-overlay">
                <text class="error-text">{{ errorMessage }}</text>
                <view class="retry-btn" @tap="initScanner">
                    <text class="retry-text">重试</text>
                </view>
            </view>
        </view>

        <!-- 底部操作 -->
        <view class="scanner-footer">
            <view class="footer-tip">
                <text class="tip-text">扫描 PC 端生成的二维码完成登录</text>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Html5Qrcode } from 'html5-qrcode'
import api from '@/utils/api'

const userStore = useUserStore()

const isInitializing = ref(true)
const errorMessage = ref('')
const scannedCode = ref('')
let html5QrCode: Html5Qrcode | null = null

const initScanner = async () => {
    isInitializing.value = true
    errorMessage.value = ''

    try {
        html5QrCode = new Html5Qrcode('qr-reader')

        const config = {
            fps: 10,
            qrbox: { width: 250, height: 250 },
            aspectRatio: 1.0,
            focusMode: 'continuous' as const,
        }

        await html5QrCode.start({ facingMode: 'environment' }, config, onScanSuccess, onScanFailure)

        isInitializing.value = false
    } catch (err: unknown) {
        console.error('启动相机失败:', err)
        isInitializing.value = false

        const error = err as { name?: string; message?: string }
        if (error.name === 'NotAllowedError') {
            errorMessage.value = '请允许访问相机权限'
        } else if (error.name === 'NotFoundError') {
            errorMessage.value = '未找到可用的相机设备'
        } else if (error.name === 'NotReadableError') {
            errorMessage.value = '相机被其他应用占用'
        } else {
            errorMessage.value = '启动相机失败，请重试'
        }
    }
}

const onScanSuccess = async (decodedText: string) => {
    if (scannedCode.value === decodedText) return

    scannedCode.value = decodedText
    console.log('扫码结果:', decodedText)

    // 解析二维码内容，提取 code 参数
    const code = parseQrCode(decodedText)
    if (!code) {
        uni.showToast({ title: '无效的二维码', icon: 'none' })
        return
    }

    // 停止扫描
    if (html5QrCode) {
        await html5QrCode.stop()
    }

    // 跳转到确认页面
    uni.navigateTo({
        url: `/pages/auth/qrcode-confirm?code=${code}`,
    })
}

const onScanFailure = (error: string) => {
    // 扫描失败是正常的（没有检测到二维码），不需要处理
    console.debug('扫描中...', error)
}

const parseQrCode = (rawValue: string): string | null => {
    // 支持多种格式：
    // 1. H5 URL: https://www.molitao.top/h5/pages/auth/qrcode-confirm?code=xxx
    // 2. App 深度链接: molitao://qrcode?code=xxx
    // 3. 纯 code

    try {
        const url = new URL(rawValue)
        const code = url.searchParams.get('code')
        if (code) return code
    } catch {
        // 不是 URL 格式
    }

    // 如果是纯 code（长度大于 5 且不包含特殊字符）
    if (rawValue.length > 5 && !rawValue.includes('://') && !rawValue.includes('/')) {
        return rawValue
    }

    return null
}

const handleBack = () => {
    stopScanner()
    uni.navigateBack()
}

const stopScanner = async () => {
    if (html5QrCode) {
        try {
            const isRunning = html5QrCode.isScanning
            if (isRunning) {
                await html5QrCode.stop()
            }
            html5QrCode.clear()
        } catch (err) {
            console.error('停止扫描失败:', err)
        }
        html5QrCode = null
    }
}

onMounted(() => {
    // 只在 H5 环境下启用扫码
    // #ifdef H5
    initScanner()
    // #endif

    // #ifndef H5
    isInitializing.value = false
    errorMessage.value = '请在 H5 环境下使用扫码功能'
    // #endif
})

onUnmounted(() => {
    stopScanner()
})
</script>

<style lang="scss" scoped>
.qrcode-scanner-page {
    min-height: 100vh;
    background: #000;
    display: flex;
    flex-direction: column;
}

.scanner-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 24rpx 32rpx;
    padding-top: calc(24rpx + env(safe-area-inset-top));
    background: rgba(0, 0, 0, 0.8);
}

.back-btn {
    display: flex;
    align-items: center;
    gap: 8rpx;
    padding: 16rpx 24rpx;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 32rpx;
}

.back-icon {
    font-size: 40rpx;
    color: #fff;
    line-height: 1;
}

.back-text {
    font-size: 28rpx;
    color: #fff;
}

.header-title {
    font-size: 32rpx;
    font-weight: 500;
    color: #fff;
}

.placeholder {
    width: 120rpx;
}

.scanner-container {
    flex: 1;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

.qr-reader {
    width: 100%;
    max-width: 500rpx;
    aspect-ratio: 1;
    border-radius: 24rpx;
    overflow: hidden;

    :deep(video) {
        width: 100% !important;
        height: 100% !important;
        object-fit: cover;
        border-radius: 24rpx;
    }

    :deep(#qr-reader-shader) {
        border-radius: 24rpx;
    }
}

.scan-hint {
    margin-top: 48rpx;
    text-align: center;
}

.hint-text {
    font-size: 28rpx;
    color: rgba(255, 255, 255, 0.7);
}

.loading-overlay,
.error-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.8);
    gap: 24rpx;
}

.loading-text {
    font-size: 28rpx;
    color: #fff;
}

.error-text {
    font-size: 28rpx;
    color: #ff6b6b;
    text-align: center;
    padding: 0 48rpx;
}

.retry-btn {
    padding: 20rpx 48rpx;
    background: #f4835a;
    border-radius: 40rpx;
}

.retry-text {
    font-size: 28rpx;
    color: #fff;
}

.scanner-footer {
    padding: 32rpx;
    padding-bottom: calc(32rpx + env(safe-area-inset-bottom));
    background: rgba(0, 0, 0, 0.8);
}

.footer-tip {
    text-align: center;
}

.tip-text {
    font-size: 24rpx;
    color: rgba(255, 255, 255, 0.5);
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "扫描二维码",
        "navigationBarBackgroundColor": "#000000",
        "navigationBarTextStyle": "white",
        "navigationStyle": "custom"
    }
}
</route>
