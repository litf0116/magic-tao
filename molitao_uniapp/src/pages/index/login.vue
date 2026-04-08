<template>
    <view class="login-page">
        <view class="login-content">
            <view class="logo-wrap">
                <image
                    src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
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

                <view class="wx-login-btn" :class="{ disabled: isLoading || !agreePrivacy }" @tap="handleWxLogin">
                    <text class="wx-login-text">微信一键登录</text>
                </view>

                <view class="agreement">
                    <view class="checkbox-wrap" @tap="togglePrivacy">
                        <view class="checkbox" :class="{ checked: agreePrivacy }">
                            <text v-if="agreePrivacy" class="check-icon">✓</text>
                        </view>
                        <text class="agreement-text">
                            我已阅读并同意
                            <text class="agreement-link" @tap.stop="toAgreement">《用户协议》</text>
                            和
                            <text class="agreement-link" @tap.stop="toPrivacy">《隐私政策》</text>
                        </text>
                    </view>
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

                <view class="login-btn" :class="{ disabled: isLoading || !agreePrivacy }" @tap="handleLogin">
                    <text class="login-btn-text">{{ isLoading ? '登录中' : '登录' }}</text>
                </view>

                <view class="agreement">
                    <view class="checkbox-wrap" @tap="togglePrivacy">
                        <view class="checkbox" :class="{ checked: agreePrivacy }">
                            <text v-if="agreePrivacy" class="check-icon">✓</text>
                        </view>
                        <text class="agreement-text">
                            我已阅读并同意
                            <text class="agreement-link" @tap.stop="toAgreement">《用户协议》</text>
                            和
                            <text class="agreement-link" @tap.stop="toPrivacy">《隐私政策》</text>
                        </text>
                    </view>
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

            <!-- H5：账号密码登录 -->
            <!-- #ifdef H5 -->
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

                <view class="login-btn" :class="{ disabled: isLoading || !agreePrivacy }" @tap="handleLogin">
                    <text class="login-btn-text">{{ isLoading ? '登录中' : '登录' }}</text>
                </view>

                <view class="agreement">
                    <view class="checkbox-wrap" @tap="togglePrivacy">
                        <view class="checkbox" :class="{ checked: agreePrivacy }">
                            <text v-if="agreePrivacy" class="check-icon">✓</text>
                        </view>
                        <text class="agreement-text">
                            我已阅读并同意
                            <text class="agreement-link" @tap.stop="toAgreement">《用户协议》</text>
                            和
                            <text class="agreement-link" @tap.stop="toPrivacy">《隐私政策》</text>
                        </text>
                    </view>
                </view>

                <view class="home-link" @tap="toHome">
                    <text class="home-link-text">返回首页</text>
                </view>
            </view>
            <!-- #endif -->
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import api from '@/utils/api'

const userStore = useUserStore()
const isLoading = ref(false)
const focusField = ref('')

// 隐私政策同意状态
const agreePrivacy = ref(false)

const { toHome, toForgotPassword } = useTo()

// 检查隐私政策同意
const checkPrivacyAgreement = () => {
    if (!agreePrivacy.value) {
        uni.showToast({ title: '请先阅读并同意用户协议和隐私政策', icon: 'none', duration: 2000 })
        return false
    }
    return true
}

// 切换隐私政策同意状态
const togglePrivacy = () => {
    agreePrivacy.value = !agreePrivacy.value
}

// 登录成功后跳转处理
const navigateAfterLogin = () => {
    console.log('[navigateAfterLogin] 开始执行')

    // #ifdef H5
    uni.redirectTo({ url: '/pages/tabbar/index' })
    // #endif

    // #ifdef APP-PLUS
    uni.reLaunch({ url: '/pages/tabbar/index' })
    // #endif

    // #ifdef MP-WEIXIN
    const pages = getCurrentPages()
    if (pages && pages.length > 1) {
        uni.navigateBack()
    } else {
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

    // 检查隐私政策同意
    if (!checkPrivacyAgreement()) return

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

    // 检查隐私政策同意
    if (!checkPrivacyAgreement()) return

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
const handleWxOAuth = async () => {
    if (isLoading.value) return

    // 检查隐私政策同意
    if (!checkPrivacyAgreement()) return

    isLoading.value = true
    console.log('[handleWxOAuth] 开始微信登录')

    try {
        const res = await userStore.appWxLogin()
        console.log('[handleWxOAuth] 登录成功', res)
        uni.$emit('refreshView')
        uni.showToast({ title: '登录成功', icon: 'success' })

        // 强制跳转
        setTimeout(() => {
            console.log('[handleWxOAuth] 执行跳转')
            // #ifdef APP-PLUS
            uni.reLaunch({ url: '/pages/tabbar/index' })
            // #endif
        }, 800)
    } catch (error: any) {
        console.log('[handleWxOAuth] 登录失败', error)
        uni.showToast({ title: error?.message || error || '微信登录失败', icon: 'none' })
    } finally {
        isLoading.value = false
    }
}

const toAgreement = () => {
    uni.navigateTo({ url: '/pages/protocol/agreement' })
}

const toPrivacy = () => {
    uni.navigateTo({ url: '/pages/protocol/privacy' })
}
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
    margin-top: 48rpx;
    padding-bottom: 24rpx;
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

/* 隐私政策勾选框 */
.checkbox-wrap {
    display: flex;
    align-items: center;
    gap: 12rpx;
    padding: 12rpx 0;
}

.checkbox {
    width: 36rpx;
    height: 36rpx;
    border: 2rpx solid #cccccc;
    border-radius: 6rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    transition: all 0.2s;

    &.checked {
        background: #07c160;
        border-color: #07c160;
    }
}

.check-icon {
    color: #ffffff;
    font-size: 22rpx;
    font-weight: bold;
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
