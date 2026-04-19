<template>
    <view class="account-security">
        <view class="section">
            <view class="section-title">
                <text>登录方式</text>
            </view>

            <view v-if="loading" class="loading-wrap">
                <text>加载中...</text>
            </view>

            <view v-else-if="bindings.length === 0" class="empty-wrap">
                <text>暂无登录方式</text>
            </view>

            <view v-else class="binding-list">
                <view 
                    v-for="binding in bindings" 
                    :key="binding.loginProvider"
                    class="binding-item"
                >
                    <view class="binding-left">
                        <view class="binding-icon">
                            <text v-if="binding.loginProvider === 'Phone'">📱</text>
                            <text v-else-if="binding.loginProvider.startsWith('WeChat')">💬</text>
                            <text v-else>👤</text>
                        </view>
                        <view class="binding-info">
                            <text class="binding-name">{{ binding.providerDisplayName }}</text>
                            <text class="binding-key">{{ maskProviderKey(binding) }}</text>
                        </view>
                    </view>
                    <view class="binding-right">
                        <view 
                            v-if="canUnbind(binding)"
                            class="unbind-btn"
                            @tap="handleUnbind(binding)"
                        >
                            <text>解绑</text>
                        </view>
                    </view>
                </view>
            </view>
        </view>

        <view class="section">
            <view class="section-title">
                <text>添加登录方式</text>
            </view>
            <view class="add-btn" @tap="showBindPhoneDialog = true">
                <text class="add-btn-icon">📱</text>
                <text class="add-btn-text">绑定手机号</text>
            </view>
        </view>

        <view v-if="showBindPhoneDialog" class="dialog-mask" @tap="showBindPhoneDialog = false">
            <view class="dialog" @tap.stop>
                <view class="dialog-header">
                    <text class="dialog-title">绑定手机号</text>
                    <view class="dialog-close" @tap="showBindPhoneDialog = false">
                        <text>✕</text>
                    </view>
                </view>
                <view class="dialog-body">
                    <view class="form-item">
                        <text class="form-label">手机号</text>
                        <input
                            v-model="bindPhoneForm.phoneNumber"
                            type="number"
                            maxlength="11"
                            placeholder="请输入手机号"
                            class="form-input"
                        />
                    </view>
                    <view class="form-item">
                        <text class="form-label">验证码</text>
                        <view class="sms-row">
                            <input
                                v-model="bindPhoneForm.code"
                                type="number"
                                maxlength="6"
                                placeholder="请输入验证码"
                                class="form-input sms-input"
                            />
                            <view 
                                class="sms-btn"
                                :class="{ disabled: bindPhoneCountdown > 0 || !isValidBindPhone }"
                                @tap="sendBindSmsCode"
                            >
                                <text>{{ bindPhoneCountdown > 0 ? `${bindPhoneCountdown}s` : '获取验证码' }}</text>
                            </view>
                        </view>
                    </view>
                </view>
                <view class="dialog-footer">
                    <view class="dialog-btn cancel" @tap="showBindPhoneDialog = false">
                        <text>取消</text>
                    </view>
                    <view 
                        class="dialog-btn confirm"
                        :class="{ disabled: !canBindPhone }"
                        @tap="handleBindPhone"
                    >
                        <text>确认绑定</text>
                    </view>
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api, { type LoginBindingDto } from '@/utils/api'

const loading = ref(false)
const bindings = ref<LoginBindingDto[]>([])

const showBindPhoneDialog = ref(false)
const bindPhoneForm = ref({
    phoneNumber: '',
    code: ''
})
const bindPhoneCountdown = ref(0)
let bindPhoneTimer: number | undefined = undefined

const isValidBindPhone = computed(() => /^1[3-9]\d{9}$/.test(bindPhoneForm.value.phoneNumber))
const canBindPhone = computed(() => isValidBindPhone.value && bindPhoneForm.value.code.length === 6)

onMounted(() => {
    loadBindings()
})

async function loadBindings() {
    loading.value = true
    try {
        const res = await api.account.getLoginBindings()
        bindings.value = res?.items || []
    } catch (error: any) {
        uni.showToast({ title: error?.message || '获取登录方式失败', icon: 'none' })
    } finally {
        loading.value = false
    }
}

function maskProviderKey(binding: LoginBindingDto): string {
    if (binding.loginProvider === 'Phone') {
        const phone = binding.providerKey
        if (phone && phone.length === 11) {
            return phone.replace(/(\d{3})\d{4}(\d{4})/, '$1****$2')
        }
        return phone
    }
    if (binding.providerKey && binding.providerKey.length > 6) {
        return binding.providerKey.slice(0, 3) + '***' + binding.providerKey.slice(-3)
    }
    return binding.providerKey
}

function canUnbind(binding: LoginBindingDto): boolean {
    return bindings.value.length > 1
}

async function handleUnbind(binding: LoginBindingDto) {
    if (!canUnbind(binding)) {
        uni.showToast({ title: '至少需要保留一种登录方式', icon: 'none' })
        return
    }

    uni.showModal({
        title: '解绑确认',
        content: `确定要解绑${binding.providerDisplayName}吗？解绑后将无法使用该方式登录。`,
        success: async (res) => {
            if (res.confirm) {
                try {
                    await api.account.unbindLogin({
                        loginProvider: binding.loginProvider,
                        providerKey: binding.providerKey
                    })
                    uni.showToast({ title: '解绑成功', icon: 'success' })
                    await loadBindings()
                } catch (error: any) {
                    uni.showToast({ title: error?.message || '解绑失败', icon: 'none' })
                }
            }
        }
    })
}

async function sendBindSmsCode() {
    if (bindPhoneCountdown.value > 0 || !isValidBindPhone.value) return

    try {
        await api.tokenAuth.sendSmsCode({ 
            phoneNumber: bindPhoneForm.value.phoneNumber, 
            purpose: 'bindphone' 
        })
        uni.showToast({ title: '验证码已发送', icon: 'success' })
        bindPhoneCountdown.value = 60
        bindPhoneTimer = setInterval(() => {
            bindPhoneCountdown.value--
            if (bindPhoneCountdown.value <= 0) {
                clearInterval(bindPhoneTimer)
            }
        }, 1000)
    } catch (error: any) {
        uni.showToast({ title: error?.message || '发送验证码失败', icon: 'none' })
    }
}

async function handleBindPhone() {
    if (!canBindPhone.value) return

    try {
        await api.account.bindPhone({
            phoneNumber: bindPhoneForm.value.phoneNumber,
            code: bindPhoneForm.value.code
        })
        uni.showToast({ title: '绑定成功', icon: 'success' })
        showBindPhoneDialog.value = false
        bindPhoneForm.value = { phoneNumber: '', code: '' }
        await loadBindings()
    } catch (error: any) {
        uni.showToast({ title: error?.message || '绑定失败', icon: 'none' })
    }
}
</script>

<style lang="scss" scoped>
.account-security {
    min-height: 100vh;
    background: #f6f6f6;
    padding: 24rpx;
}

.section {
    background: #ffffff;
    border-radius: 16rpx;
    margin-bottom: 24rpx;
    padding: 32rpx;
}

.section-title {
    font-size: 32rpx;
    font-weight: 500;
    color: #333333;
    margin-bottom: 32rpx;
}

.loading-wrap,
.empty-wrap {
    text-align: center;
    padding: 48rpx 0;
    color: #999999;
    font-size: 28rpx;
}

.binding-list {
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.binding-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 24rpx;
    background: #f9f9f9;
    border-radius: 12rpx;
}

.binding-left {
    display: flex;
    align-items: center;
    gap: 20rpx;
}

.binding-icon {
    width: 72rpx;
    height: 72rpx;
    background: #f4835a;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 36rpx;
}

.binding-info {
    display: flex;
    flex-direction: column;
    gap: 8rpx;
}

.binding-name {
    font-size: 28rpx;
    font-weight: 500;
    color: #333333;
}

.binding-key {
    font-size: 24rpx;
    color: #999999;
}

.unbind-btn {
    padding: 12rpx 24rpx;
    background: #ff4d4f;
    border-radius: 8rpx;

    text {
        font-size: 24rpx;
        color: #ffffff;
    }
}

.add-btn {
    display: flex;
    align-items: center;
    gap: 16rpx;
    padding: 32rpx;
    background: #f9f9f9;
    border-radius: 12rpx;
    border: 2rpx dashed #ddd;
}

.add-btn-icon {
    font-size: 40rpx;
}

.add-btn-text {
    font-size: 28rpx;
    color: #666666;
}

.dialog-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 999;
}

.dialog {
    width: 600rpx;
    background: #ffffff;
    border-radius: 16rpx;
    overflow: hidden;
}

.dialog-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 32rpx;
    border-bottom: 2rpx solid #f0f0f0;
}

.dialog-title {
    font-size: 32rpx;
    font-weight: 500;
    color: #333333;
}

.dialog-close {
    font-size: 32rpx;
    color: #999999;
    padding: 8rpx;
}

.dialog-body {
    padding: 32rpx;
}

.form-item {
    margin-bottom: 24rpx;
}

.form-label {
    font-size: 28rpx;
    color: #333333;
    margin-bottom: 16rpx;
    display: block;
}

.form-input {
    width: 100%;
    height: 88rpx;
    padding: 0 24rpx;
    background: #f9f9f9;
    border-radius: 12rpx;
    font-size: 28rpx;
}

.sms-row {
    display: flex;
    gap: 16rpx;
}

.sms-input {
    flex: 1;
}

.sms-btn {
    padding: 0 24rpx;
    height: 88rpx;
    background: #f4835a;
    border-radius: 12rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    white-space: nowrap;

    text {
        font-size: 26rpx;
        color: #ffffff;
    }

    &.disabled {
        background: #cccccc;
    }
}

.dialog-footer {
    display: flex;
    border-top: 2rpx solid #f0f0f0;
}

.dialog-btn {
    flex: 1;
    height: 96rpx;
    display: flex;
    align-items: center;
    justify-content: center;

    text {
        font-size: 30rpx;
    }

    &.cancel {
        border-right: 2rpx solid #f0f0f0;

        text {
            color: #666666;
        }
    }

    &.confirm {
        text {
            color: #f4835a;
            font-weight: 500;
        }

        &.disabled {
            text {
                color: #cccccc;
            }
        }
    }
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "账号与安全",
        "navigationBarBackgroundColor": "#f6f6f6",
        "navigationBarTextStyle": "black"
    }
}
</route>
