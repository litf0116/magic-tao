<template>
    <tui-page>
        <view class="h-[100vh] px-4 relative flex flex-col">
            <view class="flex-1 flex flex-col items-center flex-center">
                <image
                    src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png"
                    class="h-[10vh]"
                    mode="aspectFit"
                />
                <text class="font-bold text-lg mt-4">完善信息</text>
                <text class="text-gray-500 text-sm mt-2">绑定手机号后即可使用完整功能</text>
            </view>

            <view class="w-full">
                <view class="bg-white rounded-lg p-4 mb-4">
                    <view class="flex items-center border-b border-gray-100 pb-3">
                        <text class="text-gray-600 w-20">手机号</text>
                        <input
                            v-model="form.phoneNumber"
                            type="number"
                            maxlength="11"
                            placeholder="请输入手机号"
                            class="flex-1 text-base"
                        />
                    </view>
                    <view class="flex items-center pt-3">
                        <text class="text-gray-600 w-20">密码</text>
                        <input
                            v-model="form.password"
                            :password="!showPassword"
                            placeholder="请设置密码"
                            class="flex-1 text-base"
                        />
                        <view @tap="showPassword = !showPassword">
                            <text class="text-gray-400">{{ showPassword ? '隐藏' : '显示' }}</text>
                        </view>
                    </view>
                </view>

                <button class="w-full bg-[#f4835a] text-white rounded-lg mb-4" :disabled="isLoading" @click="handleBind">
                    <text v-if="!isLoading">确认绑定</text>
                    <text v-else>绑定中...</text>
                </button>

                <view class="text-center">
                    <text class="text-gray-400 text-xs">绑定后可以使用手机号和密码登录</text>
                </view>
            </view>
        </view>
    </tui-page>
</template>

<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'

const userStore = useUserStore()

const form = ref({
    phoneNumber: '',
    password: '',
})

const showPassword = ref(false)
const isLoading = ref(false)

let bindToken = ''
let userId = 0
let userName = ''

onLoad((options: any) => {
    if (options.bindToken) {
        bindToken = options.bindToken
    }
    if (options.userId) {
        userId = Number(options.userId)
    }
    if (options.userName) {
        userName = options.userName
    }
})

async function handleBind() {
    if (!/^1\d{10}$/.test(form.value.phoneNumber)) {
        uni.showToast({ icon: 'none', title: '请输入正确的手机号码' })
        return
    }

    if (!/^[^ ]{6,32}$/.test(form.value.password)) {
        uni.showToast({ icon: 'none', title: '密码长度6-32位，不能包含空格' })
        return
    }

    isLoading.value = true
    try {
        await userStore.bindPhoneWithPassword(form.value.phoneNumber, form.value.password, bindToken)
        uni.showToast({ icon: 'success', title: '绑定成功' })
        setTimeout(() => {
            uni.$emit('refreshView')
            uni.navigateBack({})
        }, 1500)
    } catch (err: any) {
        uni.showToast({ icon: 'none', title: err.error || '绑定失败' })
    } finally {
        isLoading.value = false
    }
}
</script>

<route lang="json">
{
    "style": {
        "navigationBarTitleText": "完善信息"
    }
}
</route>