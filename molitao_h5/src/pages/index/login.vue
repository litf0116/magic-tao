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
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const userStore = useUserStore()
const isLoading = ref(false)
const focusField = ref('')
const agreePrivacy = ref(false)

const { toHome, toForgotPassword } = useTo()

const checkPrivacyAgreement = () => {
    if (!agreePrivacy.value) {
        uni.showToast({ title: '请先阅读并同意用户协议和隐私政策', icon: 'none', duration: 2000 })
        return false
    }
    return true
}

const togglePrivacy = () => {
    agreePrivacy.value = !agreePrivacy.value
}

const navigateAfterLogin = () => {
    uni.redirectTo({ url: '/pages/tabbar/index' })
}

const form = ref({
    userNameOrEmailAddress: '',
    password: '',
})

const handleLogin = async () => {
    if (isLoading.value) return

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
        await userStore.login(form.value.userNameOrEmailAddress.trim(), form.value.password.trim())
        uni.showToast({ title: '登录成功', icon: 'success' })
        uni.$emit('refreshView')
        setTimeout(() => {
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
    display: flex;
    flex-direction: column;
    padding: 80rpx 48rpx 48rpx;
}

.logo-wrap {
    display: flex;
    justify-content: center;
    margin-bottom: 60rpx;
}

.logo {
    width: 200rpx;
    height: 200rpx;
}

.form-card {
    background: #ffffff;
    border-radius: 24rpx;
    padding: 48rpx 40rpx 40rpx;
    box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.04);
}

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
