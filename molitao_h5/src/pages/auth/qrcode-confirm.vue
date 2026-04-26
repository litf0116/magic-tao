<template>
    <view class="qrcode-confirm-page">
        <view class="confirm-content">
            <!-- Logo 区域 -->
            <view class="logo-wrap">
                <image
                    src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
                    class="logo"
                    mode="aspectFit"
                />
            </view>

            <!-- 加载状态 -->
            <view v-if="isLoading" class="loading-wrap">
                <text class="loading-text">正在获取登录信息...</text>
            </view>

            <!-- 用户信息展示 -->
            <view v-else-if="userInfo" class="user-info-card">
                <view class="title-wrap">
                    <text class="title">确认登录以下账号？</text>
                </view>

                <view class="avatar-wrap">
                    <image
                        :src="userInfo.avatarUrl || 'https://image.molitao.top/default-avatar.png'"
                        class="avatar"
                        mode="aspectFill"
                    />
                </view>

                <view class="user-detail">
                    <text class="nickname">{{ userInfo.name || '魔力淘用户' }}</text>
                    <text class="phone">{{ maskPhone(userInfo.phoneNumber) }}</text>
                </view>

                <!-- 操作按钮 -->
                <view class="action-buttons">
                    <view
                        class="confirm-btn"
                        :class="{ disabled: isConfirming }"
                        @tap="handleConfirm"
                    >
                        <text class="confirm-btn-text">{{ isConfirming ? '确认中...' : '确认登录' }}</text>
                    </view>

                    <view class="cancel-btn" @tap="handleCancel">
                        <text class="cancel-btn-text">取消</text>
                    </view>
                </view>
            </view>

            <!-- 错误状态 -->
            <view v-else class="error-wrap">
                <text class="error-text">{{ errorMessage || '获取登录信息失败' }}</text>
                <view class="retry-btn" @tap="fetchUserInfo">
                    <text class="retry-btn-text">重试</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/utils/api'

interface UserInfo {
    id?: number
    name?: string
    phoneNumber?: string
    avatarUrl?: string
}

const code = ref('')
const isLoading = ref(true)
const isConfirming = ref(false)
const userInfo = ref<UserInfo | null>(null)
const errorMessage = ref('')

// 脱敏手机号
const maskPhone = (phone?: string) => {
    if (!phone) return ''
    if (phone.length >= 11) {
        return phone.replace(/(\d{3})\d{4}(\d{4})/, '$1****$2')
    }
    return phone
}

// 获取用户信息
const fetchUserInfo = async () => {
    if (!code.value) {
        errorMessage.value = '无效的登录二维码'
        isLoading.value = false
        return
    }

    isLoading.value = true
    errorMessage.value = ''

    try {
        // 调用 API 获取二维码对应的用户信息
        // 注意：此 API 需要在 Task 7 中实现
        const res = (await api.tokenAuth.qrToken(code.value)) as any

        if (res && res.user) {
            userInfo.value = res.user
        } else {
            errorMessage.value = '二维码已过期或无效'
        }
    } catch (error: any) {
        console.error('获取用户信息失败:', error)
        errorMessage.value = error?.message || '获取登录信息失败'
    } finally {
        isLoading.value = false
    }
}

// 确认登录
const handleConfirm = async () => {
    if (isConfirming.value || !code.value) return

    isConfirming.value = true

    try {
        // 调用 API 确认登录
        // 注意：此 API 需要在 Task 7 中实现
        const res = (await api.tokenAuth.qrToken(code.value)) as any

        if (res) {
            // 如果返回的是 token 字符串，说明已确认成功
            if (typeof res === 'string' && res) {
                uni.showToast({ title: '登录成功', icon: 'success' })
                // 延迟关闭页面
                setTimeout(() => {
                    // 返回首页或关闭页面
                    uni.switchTab({ url: '/pages/tabbar/index' })
                }, 1000)
            } else if (res.accessToken) {
                // 存储 token
                uni.setStorageSync('token', res.accessToken)
                uni.showToast({ title: '登录成功', icon: 'success' })
                setTimeout(() => {
                    uni.switchTab({ url: '/pages/tabbar/index' })
                }, 1000)
            }
        }
    } catch (error: any) {
        console.error('确认登录失败:', error)
        uni.showToast({
            title: error?.message || '确认登录失败',
            icon: 'none',
            duration: 2000,
        })
    } finally {
        isConfirming.value = false
    }
}

// 取消登录
const handleCancel = () => {
    uni.showModal({
        title: '提示',
        content: '确定要取消登录吗？',
        success: (res) => {
            if (res.confirm) {
                // 返回首页
                uni.switchTab({ url: '/pages/tabbar/index' })
            }
        },
    })
}

// 页面加载
onMounted(() => {
    // 获取 URL 参数中的 code
    const pages = getCurrentPages()
    const currentPage = pages[pages.length - 1] as any
    const options = currentPage?.options || {}

    code.value = options.code || ''

    if (code.value) {
        fetchUserInfo()
    } else {
        errorMessage.value = '缺少登录参数'
        isLoading.value = false
    }
})
</script>

<style lang="scss" scoped>
.qrcode-confirm-page {
    min-height: 100vh;
    background: #f6f6f6;
    display: flex;
    flex-direction: column;
}

.confirm-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 80rpx 48rpx 48rpx;
}

.logo-wrap {
    display: flex;
    justify-content: center;
    margin-bottom: 60rpx;
}

.logo {
    width: 160rpx;
    height: 160rpx;
}

.loading-wrap {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 100rpx 0;
}

.loading-text {
    font-size: 28rpx;
    color: #999999;
}

.user-info-card {
    width: 100%;
    background: #ffffff;
    border-radius: 24rpx;
    padding: 48rpx 40rpx 40rpx;
    box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.04);
}

.title-wrap {
    text-align: center;
    margin-bottom: 48rpx;
}

.title {
    font-size: 36rpx;
    font-weight: 500;
    color: #333333;
}

.avatar-wrap {
    display: flex;
    justify-content: center;
    margin-bottom: 32rpx;
}

.avatar {
    width: 160rpx;
    height: 160rpx;
    border-radius: 50%;
    background: #f0f0f0;
}

.user-detail {
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-bottom: 48rpx;
}

.nickname {
    font-size: 32rpx;
    font-weight: 500;
    color: #333333;
    margin-bottom: 16rpx;
}

.phone {
    font-size: 28rpx;
    color: #999999;
}

.action-buttons {
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.confirm-btn {
    height: 96rpx;
    background: #f4835a;
    border-radius: 48rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: opacity 0.2s;

    &:active {
        opacity: 0.85;
    }

    &.disabled {
        opacity: 0.5;
    }
}

.confirm-btn-text {
    font-size: 32rpx;
    color: #ffffff;
    font-weight: 500;
    letter-spacing: 4rpx;
}

.cancel-btn {
    height: 96rpx;
    background: #ffffff;
    border: 2rpx solid #e5e5e5;
    border-radius: 48rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.2s;

    &:active {
        background: #f5f5f5;
    }
}

.cancel-btn-text {
    font-size: 32rpx;
    color: #666666;
    font-weight: 500;
}

.error-wrap {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 100rpx 0;
}

.error-text {
    font-size: 28rpx;
    color: #999999;
    margin-bottom: 32rpx;
}

.retry-btn {
    padding: 20rpx 60rpx;
    background: #f4835a;
    border-radius: 40rpx;
}

.retry-btn-text {
    font-size: 28rpx;
    color: #ffffff;
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "扫码登录确认",
        "navigationBarBackgroundColor": "#f6f6f6",
        "navigationBarTextStyle": "black"
    }
}
</route>
