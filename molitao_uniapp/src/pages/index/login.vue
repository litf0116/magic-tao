<template>
    <tui-page>
        <view class="h-[100vh] px-4 relative flex flex-col">
            <view class="flex-1 flex flex-col items-center flex-center">
                <image
                    src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
                    class="h-[15vh]"
                    mode="aspectFit"
                />
            </view>

            <!-- 账号密码登录表单 -->
            <view class="mb-4">
                <input
                    v-model="form.userNameOrEmailAddress"
                    placeholder="账号/邮箱/手机号"
                    class="w-full border border-gray-300 rounded-lg px-4 py-3 mb-3 bg-white"
                    placeholder-class="text-gray-400"
                />
                <input
                    v-model="form.password"
                    placeholder="密码"
                    type="password"
                    class="w-full border border-gray-300 rounded-lg px-4 py-3 mb-3 bg-white"
                    placeholder-class="text-gray-400"
                />
            </view>

            <!-- 登录按钮 -->
            <button
                class="w-full bg-[#f4835a] text-white rounded-lg mb-4 py-3 font-bold"
                :disabled="isLoading"
                @tap="handleLogin"
            >
                {{ isLoading ? '登录中...' : '登录' }}
            </button>

            <!-- 小程序端：微信登录 -->
            <!-- #ifdef MP-WEIXIN -->
            <button
                class="w-full bg-green-500 text-white rounded-lg mb-4 py-3 font-bold"
                :disabled="isLoading"
                @tap="wxLogin(false)"
            >
                微信登录
            </button>
            <!-- #endif -->

            <!-- App 端：微信登录（暂时隐藏，后续补充）-->
            <!-- #ifdef APP-PLUS -->
            <!-- <button
                class="w-full bg-green-500 text-white rounded-lg mb-4 py-3 font-bold"
                :disabled="isLoading"
                @tap="wxLogin(false)"
            >
                微信登录
            </button> -->
            <!-- #endif -->

            <button
                class="w-full mb-32 rounded-lg py-3 text-gray-500"
                :disabled="isLoading"
                @tap="toHome"
            >
                返回
            </button>
        </view>
    </tui-page>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const userStore = useUserStore()
const isLoading = ref(false)

const { toHome } = useTo()

const form = ref({
    userNameOrEmailAddress: '',
    password: '',
})

// 账号密码登录
async function handleLogin() {
    // 验证输入
    if (!form.value.userNameOrEmailAddress || !form.value.userNameOrEmailAddress.trim()) {
        uni.showToast({
            title: '请输入账号/邮箱/手机号',
            icon: 'none'
        })
        return
    }

    if (!form.value.password || !form.value.password.trim()) {
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

        // 发送事件通知
        uni.$emit('refreshView')
        uni.navigateBack({})
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

// 微信登录
function wxLogin(back: boolean) {
    userStore.wxLogin().then(() => {
        if (back) {
            // 发送事件通知
            uni.$emit('refreshView')
            uni.navigateBack({})
        }
    })
}
</script>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "用户登录"
    }
}
</route>
