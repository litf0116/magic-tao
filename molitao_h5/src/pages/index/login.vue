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
                <template v-if="true">
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
                </template>

                <template v-else>
                    <view class="input-wrap">
                        <input
                            v-model="smsForm.phoneNumber"
                            placeholder="请输入手机号"
                            type="number"
                            maxlength="11"
                            class="input"
                            placeholder-class="input-placeholder"
                            @focus="focusField = 'phone'"
                            @blur="focusField = ''"
                        />
                        <view class="input-underline" :class="{ active: focusField === 'phone' }"></view>
                    </view>

                    <view class="input-wrap sms-input-wrap">
                        <input
                            v-model="smsForm.code"
                            placeholder="请输入验证码"
                            type="number"
                            maxlength="6"
                            class="input sms-input"
                            placeholder-class="input-placeholder"
                            @focus="focusField = 'code'"
                            @blur="focusField = ''"
                        />
                        <view
                            class="sms-btn"
                            :class="{ disabled: smsCountdown > 0 || !isValidPhone }"
                            @tap="sendSmsCode"
                        >
                            <text>{{ smsCountdown > 0 ? `${smsCountdown}s` : '获取验证码' }}</text>
                        </view>
                        <view class="input-underline" :class="{ active: focusField === 'code' }"></view>
                    </view>
                </template>

                <view class="login-btn" :class="{ disabled: isLoading || !agreePrivacy }" @tap="handleLogin">
                    <text class="login-btn-text">{{ isLoading ? '登录中' : '登录' }}</text>
                </view>

                <view class="qrcode-login-btn" @tap="toQrcodeScanner">
                    <text class="qrcode-icon">📷</text>
                    <text class="qrcode-text">扫码登录</text>
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
import { ref, computed, onUnmounted } from 'vue'
import api from '@/utils/api'

const userStore = useUserStore()
const isLoading = ref(false)
const focusField = ref('')
const agreePrivacy = ref(false)
const loginTab = ref<'password' | 'sms'>('password')

const { toHome, toForgotPassword } = useTo()

const form = ref({
    userNameOrEmailAddress: '',
    password: '',
})

const smsForm = ref({
    phoneNumber: '',
    code: '',
})

const smsCountdown = ref(0)
let smsTimer: number | undefined = undefined

const SMS_COUNTDOWN_KEY = 'sms_countdown_end_time'

const restoreSmsCountdown = () => {
    const endTime = uni.getStorageSync(SMS_COUNTDOWN_KEY)
    if (endTime) {
        const remaining = Math.floor((parseInt(endTime) - Date.now()) / 1000)
        if (remaining > 0) {
            smsCountdown.value = remaining
            startSmsTimer()
        } else {
            uni.removeStorageSync(SMS_COUNTDOWN_KEY)
        }
    }
}

const startSmsTimer = () => {
    smsTimer = setInterval(() => {
        smsCountdown.value--
        if (smsCountdown.value <= 0) {
            clearInterval(smsTimer)
            uni.removeStorageSync(SMS_COUNTDOWN_KEY)
        }
    }, 1000)
}

const isValidPhone = computed(() => /^1[3-9]\d{9}$/.test(smsForm.value.phoneNumber))

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

const sendSmsCode = async () => {
    if (smsCountdown.value > 0 || !isValidPhone.value) return

    try {
        await api.tokenAuth.sendSmsCode({ phoneNumber: smsForm.value.phoneNumber })
        uni.showToast({ title: '验证码已发送', icon: 'success' })
        smsCountdown.value = 60
        uni.setStorageSync(SMS_COUNTDOWN_KEY, (Date.now() + 60000).toString())
        startSmsTimer()
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '发送验证码失败',
            icon: 'none',
        })
    }
}

const handleLogin = async () => {
    if (isLoading.value) return

    if (!checkPrivacyAgreement()) return

    if (loginTab.value === 'password') {
        await handlePasswordLogin()
    } else {
        await handleSmsLogin()
    }
}

const handlePasswordLogin = async () => {
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

const handleSmsLogin = async () => {
    if (!isValidPhone.value) {
        uni.showToast({ title: '请输入正确的手机号', icon: 'none' })
        return
    }

    if (!smsForm.value.code || smsForm.value.code.length !== 6) {
        uni.showToast({ title: '请输入6位验证码', icon: 'none' })
        return
    }

    isLoading.value = true

    try {
        const res = (await api.tokenAuth.phoneAuthenticate({
            phoneNumber: smsForm.value.phoneNumber,
            code: smsForm.value.code,
        })) as any

        if (res?.accessToken) {
            uni.setStorageSync('token', res.accessToken)
            await userStore.getUserInfo()
            uni.showToast({ title: '登录成功', icon: 'success' })
            uni.$emit('refreshView')
            setTimeout(() => {
                navigateAfterLogin()
            }, 500)
        }
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

const toQrcodeScanner = () => {
    uni.navigateTo({ url: '/pages/auth/qrcode-scanner' })
}

onUnmounted(() => {
    if (smsTimer) {
        clearInterval(smsTimer)
    }
})

restoreSmsCountdown()
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

.tabs {
    display: flex;
    margin-bottom: 40rpx;
    border-bottom: 2rpx solid #ebebeb;
}

.tab-item {
    flex: 1;
    text-align: center;
    padding: 24rpx 0;
    font-size: 30rpx;
    color: #999999;
    position: relative;

    &.active {
        color: #f4835a;
        font-weight: 500;

        &::after {
            content: '';
            position: absolute;
            bottom: -2rpx;
            left: 50%;
            transform: translateX(-50%);
            width: 60rpx;
            height: 4rpx;
            background: #f4835a;
            border-radius: 2rpx;
        }
    }
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

.sms-input-wrap {
    display: flex;
    align-items: center;
}

.sms-input {
    flex: 1;
}

.sms-btn {
    padding: 16rpx 24rpx;
    background: #f4835a;
    border-radius: 8rpx;
    font-size: 26rpx;
    color: #ffffff;
    white-space: nowrap;

    &.disabled {
        background: #cccccc;
    }
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

.qrcode-login-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 12rpx;
    margin-top: 32rpx;
    padding: 24rpx 0;
    border: 2rpx solid #f4835a;
    border-radius: 48rpx;

    &:active {
        background: rgba(244, 131, 90, 0.1);
    }
}

.qrcode-icon {
    font-size: 36rpx;
}

.qrcode-text {
    font-size: 28rpx;
    color: #f4835a;
    font-weight: 500;
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
