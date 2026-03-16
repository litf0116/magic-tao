<template>
    <view class="login-page">
        <view class="login-content">
            <view class="logo-wrap">
                <image
                    src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
                    class="logo"
                    mode="aspectFit"
                />
            </view>

            <!-- 微信小程序：只显示微信登录 -->
            <!-- #ifdef MP-WEIXIN -->
            <view class="form-card">
                <view class="welcome-text">
                    <text class="welcome-title">欢迎来到魔力淘</text>
                    <text class="welcome-desc">登录后享受更多精彩服务</text>
                </view>

                <view class="wx-login-btn" :class="{ disabled: isLoading }" @tap="handleWxLogin">
                    <text class="wx-login-text">微信一键登录</text>
                </view>

                <view class="agreement">
                    <text class="agreement-text">登录即表示同意</text>
                    <text class="agreement-link" @tap="toAgreement">《用户协议》</text>
                </view>
            </view>
            <!-- #endif -->

            <!-- APP：账号密码 + 微信OAuth -->
            <!-- #ifdef APP-PLUS -->
            <view class="form-card">
                <view class="input-wrap">
                    <input
                        v-model="form.userNameOrEmailAddress"
                        placeholder="请输入账号"
                        class="input"
                        placeholder-class="input-placeholder"
                        @focus="focusField = 'account'"
                        @blur="focusField = ''"
                    />
                    <view class="input-underline" :class="{ active: focusField === 'account' }"></view>
                </view>

                <view class="input-wrap">
                    <input
                        v-model="form.password"
                        placeholder="请输入密码"
                        type="password"
                        class="input"
                        placeholder-class="input-placeholder"
                        @focus="focusField = 'password'"
                        @blur="focusField = ''"
                    />
                    <view class="input-underline" :class="{ active: focusField === 'password' }"></view>
                </view>

                <view class="action-row">
                    <text class="forgot-link" @tap="toForgotPassword">忘记密码？</text>
                </view>

                <view class="login-btn" :class="{ disabled: isLoading }" @tap="handleLogin">
                    <text class="login-btn-text">{{ isLoading ? '登录中' : '登录' }}</text>
                </view>

                <view class="divider">
                    <view class="divider-line"></view>
                    <text class="divider-label">其他登录方式</text>
                    <view class="divider-line"></view>
                </view>

                <view class="oauth-row">
                    <view class="oauth-item" @tap="handleWxOAuth">
                        <view class="oauth-icon wx-icon">
                            <text class="oauth-icon-text">微信</text>
                        </view>
                        <text class="oauth-label">微信登录</text>
                    </view>
                </view>

                <view class="home-link" @tap="toHome">
                    <text class="home-link-text">返回首页</text>
                </view>
            </view>
            <!-- #endif -->

            <!-- H5：账号密码 + 微信扫码 -->
            <!-- #ifdef H5 -->
            <view class="form-card">
                <view class="tab-row">
                    <view class="tab-item" :class="{ active: loginMode === 'password' }" @tap="loginMode = 'password'">
                        <text class="tab-text">账号登录</text>
                    </view>
                    <view class="tab-item" :class="{ active: loginMode === 'qrcode' }" @tap="switchToQrcode">
                        <text class="tab-text">扫码登录</text>
                    </view>
                </view>

                <!-- 账号密码登录 -->
                <view v-if="loginMode === 'password'" class="password-form">
                    <view class="input-wrap">
                        <input
                            v-model="form.userNameOrEmailAddress"
                            placeholder="请输入账号"
                            class="input"
                            placeholder-class="input-placeholder"
                            @focus="focusField = 'account'"
                            @blur="focusField = ''"
                        />
                        <view class="input-underline" :class="{ active: focusField === 'account' }"></view>
                    </view>

                    <view class="input-wrap">
                        <input
                            v-model="form.password"
                            placeholder="请输入密码"
                            type="password"
                            class="input"
                            placeholder-class="input-placeholder"
                            @focus="focusField = 'password'"
                            @blur="focusField = ''"
                        />
                        <view class="input-underline" :class="{ active: focusField === 'password' }"></view>
                    </view>

                    <view class="action-row">
                        <text class="forgot-link" @tap="toForgotPassword">忘记密码？</text>
                    </view>

                    <view class="login-btn" :class="{ disabled: isLoading }" @tap="handleLogin">
                        <text class="login-btn-text">{{ isLoading ? '登录中' : '登录' }}</text>
                    </view>
                </view>

                <!-- 微信扫码登录 -->
                <view v-if="loginMode === 'qrcode'" class="qrcode-form">
                    <view class="qrcode-wrap">
                        <image v-if="qrcodeUrl" :src="qrcodeUrl" class="qrcode-image" mode="aspectFit" />
                        <view v-else class="qrcode-loading">
                            <text class="qrcode-loading-text">二维码加载中...</text>
                        </view>
                    </view>
                    <text class="qrcode-tip">请使用微信扫码登录</text>
                    <text class="qrcode-refresh" @tap="refreshQrcode">刷新二维码</text>
                </view>

                <view class="home-link" @tap="toHome">
                    <text class="home-link-text">返回首页</text>
                </view>
            </view>
            <!-- #endif -->
        </view>

        <view class="footer">
            <text class="footer-text">登录即表示同意《用户协议》和《隐私政策》</text>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import api from '@/utils/api'

const userStore = useUserStore()
const isLoading = ref(false)
const focusField = ref('')
const loginMode = ref<'password' | 'qrcode'>('password')
const qrcodeUrl = ref('')
const qrcodeState = ref('')
const qrcodeTimer = ref<any>(null)
const qrcodeExpireTimer = ref<any>(null)

const { toHome, toForgotPassword } = useTo()

// 登录成功后跳转处理
const navigateAfterLogin = () => {
    console.log('[navigateAfterLogin] 开始执行')

    // #ifdef H5
    // H5 环境：直接跳转到首页
    console.log('[navigateAfterLogin] H5模式 - 直接跳转到首页')
    uni.redirectTo({ url: '/pages/tabbar/index' })
    // #endif

    // #ifndef H5
    // 非 H5 环境：检查页面栈
    const pages = uni.getCurrentPages()
    const hasPages = pages && pages.length > 1

    console.log('[navigateAfterLogin] 非 H5 模式', {
        pagesLength: pages ? pages.length : 0,
        hasPages,
    })

    // 如果有上一页，则返回上一页
    if (hasPages) {
        console.log('[navigateAfterLogin] 返回上一页')
        uni.navigateBack()
    } else {
        // 否则关闭所有页面并跳转到首页
        console.log('[navigateAfterLogin] 跳转到首页 /pages/tabbar/index')
        uni.reLaunch({ url: '/pages/tabbar/index' })
    }
    // #endif
}

const form = ref({
    userNameOrEmailAddress: '',
    password: '',
})

// 账号密码登录
const handleLogin = async () => {
    if (isLoading.value) return

    if (!form.value.userNameOrEmailAddress?.trim()) {
        uni.showToast({ title: '请输入账号', icon: 'none' })
        return
    }

    if (!form.value.password?.trim()) {
        uni.showToast({ title: '请输入密码', icon: 'none' })
        return
    }

    isLoading.value = true

    try {
        console.log('[handleLogin] 开始登录', form.value.userNameOrEmailAddress.trim())
        await userStore.login(form.value.userNameOrEmailAddress.trim(), form.value.password.trim())
        console.log('[handleLogin] 登录API调用成功')
        uni.showToast({ title: '登录成功', icon: 'success' })
        uni.$emit('refreshView')
        setTimeout(() => {
            console.log('[handleLogin] 准备执行 navigateAfterLogin')
            navigateAfterLogin()
        }, 500)
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '登录失败',
            icon: 'none',
            duration: 2000,
        })
    } finally {
        isLoading.value = false
    }
}

// 微信小程序登录
const handleWxLogin = () => {
    if (isLoading.value) return

    userStore
        .wxLogin()
        .then(() => {
            uni.$emit('refreshView')
            setTimeout(() => {
                navigateAfterLogin()
            }, 500)
        })
        .catch((error: any) => {
            uni.showToast({ title: error?.message || '微信登录失败', icon: 'none' })
        })
}

// APP 微信 OAuth 登录
const handleWxOAuth = () => {
    if (isLoading.value) return

    userStore
        .appWxLogin()
        .then(() => {
            uni.$emit('refreshView')
            setTimeout(() => {
                navigateAfterLogin()
            }, 500)
        })
        .catch((error: any) => {
            uni.showToast({ title: error?.message || '微信登录失败', icon: 'none' })
        })
}

// H5 微信扫码登录
const generateState = () => {
    return 'qr_' + Date.now() + '_' + Math.random().toString(36).substring(2, 10)
}

const getWxQrcode = async () => {
    try {
        qrcodeState.value = generateState()
        const url = await api.tokenAuth.pubQrLogin(qrcodeState.value)
        qrcodeUrl.value = url

        startQrcodePolling()
        startQrcodeExpireTimer()
    } catch (error: any) {
        uni.showToast({ title: error?.message || '获取二维码失败', icon: 'none' })
        qrcodeUrl.value = ''
    }
}

const startQrcodePolling = () => {
    stopQrcodePolling()
    qrcodeTimer.value = setInterval(async () => {
        try {
            const token = await api.tokenAuth.qrToken(qrcodeState.value)
            if (token) {
                stopQrcodePolling()
                stopQrcodeExpireTimer()

                uni.setStorageSync('token', token)
                uni.$emit('refreshView')
                uni.showToast({ title: '登录成功', icon: 'success' })

                setTimeout(() => {
                    navigateAfterLogin()
                }, 500)
            }
        } catch (error) {
            // 轮询失败继续
        }
    }, 2000)
}

const startQrcodeExpireTimer = () => {
    stopQrcodeExpireTimer()
    qrcodeExpireTimer.value = setTimeout(() => {
        stopQrcodePolling()
        qrcodeUrl.value = ''
        uni.showToast({ title: '二维码已过期，请刷新', icon: 'none' })
    }, 60000)
}

const stopQrcodePolling = () => {
    if (qrcodeTimer.value) {
        clearInterval(qrcodeTimer.value)
        qrcodeTimer.value = null
    }
}

const stopQrcodeExpireTimer = () => {
    if (qrcodeExpireTimer.value) {
        clearTimeout(qrcodeExpireTimer.value)
        qrcodeExpireTimer.value = null
    }
}

const refreshQrcode = () => {
    stopQrcodePolling()
    stopQrcodeExpireTimer()
    getWxQrcode()
}

const switchToQrcode = () => {
    loginMode.value = 'qrcode'
    getWxQrcode()
}

const toAgreement = () => {
    // TODO: 跳转用户协议页面
}

// #ifdef H5
onMounted(() => {
    // 扫码模式下已由 getWxQrcode 启动轮询
})

onUnmounted(() => {
    stopQrcodePolling()
    stopQrcodeExpireTimer()
})
// #endif
</script>

<style lang="scss" scoped>
.login-page {
    min-height: 100vh;
    background: #f6f6f6;
    display: flex;
    flex-direction: column;
}

.login-content {
    flex: 1;
    padding: 0 48rpx;
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.logo-wrap {
    display: flex;
    justify-content: center;
    margin-bottom: 64rpx;
}

.logo {
    width: 240rpx;
    height: 160rpx;
}

.form-card {
    background: #ffffff;
    border-radius: 24rpx;
    padding: 48rpx 40rpx 40rpx;
    box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.04);
}

/* 微信小程序样式 */
.welcome-text {
    text-align: center;
    margin-bottom: 48rpx;
}

.welcome-title {
    display: block;
    font-size: 40rpx;
    font-weight: 600;
    color: #333333;
    margin-bottom: 16rpx;
}

.welcome-desc {
    display: block;
    font-size: 28rpx;
    color: #999999;
}

.wx-login-btn {
    height: 96rpx;
    background: #07c160;
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

.wx-login-text {
    font-size: 32rpx;
    color: #ffffff;
    font-weight: 500;
    letter-spacing: 2rpx;
}

.agreement {
    display: flex;
    justify-content: center;
    margin-top: 32rpx;
}

.agreement-text {
    font-size: 24rpx;
    color: #999999;
}

.agreement-link {
    font-size: 24rpx;
    color: #f4835a;
}

/* 输入框样式 */
.input-wrap {
    margin-bottom: 32rpx;
    position: relative;
}

.input {
    width: 100%;
    height: 88rpx;
    padding: 0 8rpx;
    font-size: 30rpx;
    color: #333333;
    background: transparent;
}

.input-placeholder {
    color: #cccccc;
}

.input-underline {
    position: absolute;
    left: 0;
    right: 0;
    bottom: 0;
    height: 2rpx;
    background: #ebebeb;
    transition: background 0.2s;

    &.active {
        background: #f4835a;
    }
}

.action-row {
    display: flex;
    justify-content: flex-end;
    margin-bottom: 40rpx;
}

.forgot-link {
    font-size: 26rpx;
    color: #999999;

    &:active {
        color: #f4835a;
    }
}

.login-btn {
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

.login-btn-text {
    font-size: 32rpx;
    color: #ffffff;
    font-weight: 500;
    letter-spacing: 4rpx;
}

/* 分隔线 */
.divider {
    display: flex;
    align-items: center;
    margin: 40rpx 0;
}

.divider-line {
    flex: 1;
    height: 1rpx;
    background: #ebebeb;
}

.divider-label {
    padding: 0 32rpx;
    font-size: 24rpx;
    color: #bbbbbb;
}

/* OAuth 登录 */
.oauth-row {
    display: flex;
    justify-content: center;
    gap: 64rpx;
}

.oauth-item {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.oauth-icon {
    width: 88rpx;
    height: 88rpx;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 16rpx;

    &:active {
        opacity: 0.8;
    }
}

.wx-icon {
    background: #07c160;
}

.oauth-icon-text {
    font-size: 24rpx;
    color: #ffffff;
    font-weight: 500;
}

.oauth-label {
    font-size: 24rpx;
    color: #666666;
}

.home-link {
    display: flex;
    justify-content: center;
    margin-top: 32rpx;
    padding: 16rpx 0;
}

.home-link-text {
    font-size: 28rpx;
    color: #999999;

    &:active {
        color: #f4835a;
    }
}

/* H5 Tab 切换 */
.tab-row {
    display: flex;
    margin-bottom: 40rpx;
    border-bottom: 2rpx solid #f0f0f0;
}

.tab-item {
    flex: 1;
    height: 80rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;

    &.active {
        .tab-text {
            color: #f4835a;
            font-weight: 500;
        }

        &::after {
            content: '';
            position: absolute;
            left: 50%;
            bottom: 0;
            transform: translateX(-50%);
            width: 48rpx;
            height: 4rpx;
            background: #f4835a;
            border-radius: 2rpx;
        }
    }
}

.tab-text {
    font-size: 30rpx;
    color: #999999;
}

/* 二维码 */
.qrcode-form {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.qrcode-wrap {
    width: 320rpx;
    height: 320rpx;
    background: #f6f6f6;
    border-radius: 16rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 24rpx;
}

.qrcode-image {
    width: 280rpx;
    height: 280rpx;
}

.qrcode-loading {
    display: flex;
    align-items: center;
    justify-content: center;
}

.qrcode-loading-text {
    font-size: 26rpx;
    color: #999999;
}

.qrcode-tip {
    font-size: 28rpx;
    color: #666666;
    margin-bottom: 16rpx;
}

.qrcode-refresh {
    font-size: 26rpx;
    color: #f4835a;
}

.footer {
    padding: 32rpx 0 48rpx;
    text-align: center;
}

.footer-text {
    font-size: 22rpx;
    color: #bbbbbb;
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "登录",
        "navigationBarBackgroundColor": "#f6f6f6",
        "navigationBarTextStyle": "black"
    }
}
</route>
