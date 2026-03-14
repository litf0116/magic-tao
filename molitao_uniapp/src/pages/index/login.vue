<template>
    <tui-page>
        <view class="login-container h-[100vh] px-4 relative flex flex-col">
            <view class="flex-1 flex flex-col items-center justify-center pt-12">
                <image
                    src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
                    class="h-[15vh]"
                    mode="aspectFit"
                />
            </view>

            <view class="flex-1 flex flex-col justify-center w-full max-w-md mx-auto">
                <view class="form-container bg-white rounded-2xl shadow-lg p-6 mb-6">
                    <text class="text-2xl font-bold text-center mb-6 text-gray-800">欢迎登录</text>

                    <button
                        class="w-full bg-green-500 text-white rounded-lg mb-6 py-4 font-bold active:opacity-80 transition-opacity flex items-center justify-center"
                        :disabled="isLoading"
                        @tap="wxLogin(false)"
                    >
                        <text class="mr-2 text-xl">📱</text>
                        <text class="text-lg">微信快捷登录</text>
                    </button>

                    <view class="relative my-6">
                        <view class="absolute inset-0 flex items-center">
                            <view class="w-full border-t border-gray-300"></view>
                        </view>
                        <view class="relative flex justify-center text-sm">
                            <text class="px-2 bg-white text-gray-500">或使用账号密码</text>
                        </view>
                    </view>

                    <view class="space-y-4">
                        <input
                            v-model="form.userNameOrEmailAddress"
                            placeholder="账号/邮箱/手机号"
                            class="w-full border border-gray-300 rounded-lg px-4 py-3 bg-white focus:border-[#f4835a] transition-colors"
                            placeholder-class="text-gray-400"
                        />
                        <input
                            v-model="form.password"
                            placeholder="密码"
                            type="password"
                            class="w-full border border-gray-300 rounded-lg px-4 py-3 bg-white focus:border-[#f4835a] transition-colors"
                            placeholder-class="text-gray-400"
                        />
                    </view>

                    <view class="mt-4 mb-6 text-right">
                        <text class="text-sm text-gray-500" @tap="toForgotPassword">忘记密码？</text>
                    </view>

                    <button
                        class="w-full bg-[#f4835a] text-white rounded-lg py-3 font-bold active:opacity-80 transition-opacity"
                        :disabled="isLoading"
                        @tap="handleLogin"
                    >
                        {{ isLoading ? '登录中...' : '登录' }}
                    </button>
                </view>

                <view class="flex justify-center mt-6">
                    <button
                        class="text-gray-500 py-3 font-bold active:text-gray-700 transition-colors"
                        :disabled="isLoading"
                        @tap="toHome"
                    >
                        返回首页
                    </button>
                </view>
            </view>
        </view>
    </tui-page>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const userStore = useUserStore()
const isLoading = ref(false)

const { toHome, toForgotPassword } = useTo()

const form = ref({
    userNameOrEmailAddress: '',
    password: '',
})

const handleLogin = async () => {
    if (!form.value.userNameOrEmailAddress?.trim()) {
        uni.showToast({
            title: '请输入账号/邮箱/手机号',
            icon: 'none'
        })
        return
    }

    if (!form.value.password?.trim()) {
        uni.showToast({
            title: '请输入密码',
            icon: 'none'
        })
        return
    }

    isLoading.value = true

    try {
        await userStore.login(
            form.value.userNameOrEmailAddress.trim(),
            form.value.password.trim()
        )
        uni.showToast({
            title: '登录成功',
            icon: 'success'
        })

        uni.$emit('refreshView')
        uni.navigateBack()
    } catch (error: any) {
        const errorMsg = error?.message || error || '登录失败，请检查账号和密码'
        uni.showToast({
            title: errorMsg,
            icon: 'none',
            duration: 2000
        })
    } finally {
        isLoading.value = false
    }
}

const wxLogin = (back: boolean) => {
    // #ifdef MP-WEIXIN
    userStore.wxLogin().then(() => {
        if (back) {
            uni.$emit('refreshView')
            uni.navigateBack()
        }
    }).catch((error: any) => {
        uni.showToast({
            title: error?.message || '微信登录失败',
            icon: 'none'
        })
    })
    // #endif

    // #ifdef APP-PLUS
    userStore.appWxLogin().then(() => {
        if (back) {
            uni.$emit('refreshView')
            uni.navigateBack()
        }
    }).catch((error: any) => {
        uni.showToast({
            title: error?.message || '微信登录失败',
            icon: 'none'
        })
    })
    // #endif
}
</script>

<style lang="scss" scoped>
.login-container {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.form-container {
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

input {
    &:focus {
        outline: none;
    }
}

button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "用户登录",
        "navigationBarBackgroundColor": "#f4835a",
        "navigationBarTextStyle": "white"
    }
}
</route>
