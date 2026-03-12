<template>
    <tui-page>
        <view class="settings-container p-4 bg-gray-50 min-h-screen">
            <view class="bg-white rounded-lg p-4 mb-4">
                <view class="flex items-center justify-between mb-4">
                    <text class="text-lg font-bold text-gray-800">密码登录设置</text>
                    <switch
                        :checked="canUsePasswordLogin"
                        :disabled="isLoading"
                        @change="onToggle"
                        color="#f4835a"
                    />
                </view>

                <view class="text-sm text-gray-600 mb-4">
                    开启密码登录后，您可以使用用户名和密码登录系统。
                </view>

                <template v-if="canUsePasswordLogin">
                    <view class="border-t border-gray-200 pt-4">
                        <text class="text-base font-semibold text-gray-700 mb-4 block">修改密码</text>

                        <view class="space-y-4">
                            <view>
                                <text class="text-sm text-gray-600 mb-2 block">当前密码</text>
                                <input
                                    v-model="form.currentPassword"
                                    type="password"
                                    placeholder="请输入当前密码"
                                    class="w-full border border-gray-300 rounded px-3 py-2 focus:border-[#f4835a] outline-none"
                                />
                            </view>

                            <view>
                                <text class="text-sm text-gray-600 mb-2 block">新密码</text>
                                <input
                                    v-model="form.newPassword"
                                    type="password"
                                    placeholder="请输入新密码（至少6位）"
                                    class="w-full border border-gray-300 rounded px-3 py-2 focus:border-[#f4835a] outline-none"
                                />
                            </view>

                            <view>
                                <text class="text-sm text-gray-600 mb-2 block">确认新密码</text>
                                <input
                                    v-model="form.confirmPassword"
                                    type="password"
                                    placeholder="请再次输入新密码"
                                    class="w-full border border-gray-300 rounded px-3 py-2 focus:border-[#f4835a] outline-none"
                                />
                            </view>

                            <button
                                class="w-full bg-[#f4835a] text-white rounded py-3 font-bold active:opacity-80 transition-opacity"
                                :disabled="isLoading"
                                @tap="handleChangePassword"
                            >
                                {{ isLoading ? '保存中...' : '保存新密码' }}
                            </button>
                        </view>
                    </view>
                </template>

                <template v-else>
                    <view class="border-t border-gray-200 pt-4">
                        <text class="text-base font-semibold text-gray-700 mb-4 block">设置密码</text>
                        <view class="text-sm text-gray-600 mb-4">
                            首次开启密码登录需要设置登录密码
                        </view>

                        <view class="space-y-4">
                            <view>
                                <text class="text-sm text-gray-600 mb-2 block">新密码</text>
                                <input
                                    v-model="form.newPassword"
                                    type="password"
                                    placeholder="请输入新密码（至少6位）"
                                    class="w-full border border-gray-300 rounded px-3 py-2 focus:border-[#f4835a] outline-none"
                                />
                            </view>

                            <view>
                                <text class="text-sm text-gray-600 mb-2 block">确认新密码</text>
                                <input
                                    v-model="form.confirmPassword"
                                    type="password"
                                    placeholder="请再次输入新密码"
                                    class="w-full border border-gray-300 rounded px-3 py-2 focus:border-[#f4835a] outline-none"
                                />
                            </view>

                            <button
                                class="w-full bg-[#f4835a] text-white rounded py-3 font-bold active:opacity-80 transition-opacity"
                                :disabled="isLoading"
                                @tap="handleEnablePasswordLogin"
                            >
                                {{ isLoading ? '设置中...' : '开启密码登录' }}
                            </button>
                        </view>
                    </view>
                </template>
            </view>
        </view>
    </tui-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

const userStore = useUserStore()
const isLoading = ref(false)
const canUsePasswordLogin = ref(false)

const form = ref({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
})

onMounted(async () => {
    await checkPasswordLoginStatus()
})

async function checkPasswordLoginStatus() {
    try {
        canUsePasswordLogin.value = await api.account.canUsePasswordLogin()
    } catch (error) {
        console.error('获取密码登录状态失败', error)
    }
}

async function onToggle(e: any) {
    if (e.detail.value) {
        uni.showModal({
            title: '开启密码登录',
            content: '开启后您可以使用用户名和密码登录系统，确定要开启吗？',
            success: (res) => {
                if (res.confirm) {
                    showEnablePasswordDialog()
                }
            }
        })
    } else {
        uni.showModal({
            title: '关闭密码登录',
            content: '关闭后将无法使用密码登录，确定要关闭吗？',
            success: async (res) => {
                if (res.confirm) {
                    await handleDisablePasswordLogin()
                }
            }
        })
    }
}

function showEnablePasswordDialog() {
    form.value.newPassword = ''
    form.value.confirmPassword = ''
}

async function handleEnablePasswordLogin() {
    if (!form.value.newPassword?.trim()) {
        uni.showToast({
            title: '请输入新密码',
            icon: 'none'
        })
        return
    }

    if (form.value.newPassword.length < 6) {
        uni.showToast({
            title: '密码至少需要6位',
            icon: 'none'
        })
        return
    }

    if (form.value.newPassword !== form.value.confirmPassword) {
        uni.showToast({
            title: '两次输入的密码不一致',
            icon: 'none'
        })
        return
    }

    isLoading.value = true

    try {
        await api.account.enablePasswordLogin(form.value.newPassword)
        uni.showToast({
            title: '密码登录已开启',
            icon: 'success'
        })
        canUsePasswordLogin.value = true
        form.value.newPassword = ''
        form.value.confirmPassword = ''
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '开启失败',
            icon: 'none'
        })
    } finally {
        isLoading.value = false
    }
}

async function handleChangePassword() {
    if (!form.value.currentPassword?.trim()) {
        uni.showToast({
            title: '请输入当前密码',
            icon: 'none'
        })
        return
    }

    if (!form.value.newPassword?.trim()) {
        uni.showToast({
            title: '请输入新密码',
            icon: 'none'
        })
        return
    }

    if (form.value.newPassword.length < 6) {
        uni.showToast({
            title: '密码至少需要6位',
            icon: 'none'
        })
        return
    }

    if (form.value.newPassword !== form.value.confirmPassword) {
        uni.showToast({
            title: '两次输入的密码不一致',
            icon: 'none'
        })
        return
    }

    if (form.value.currentPassword === form.value.newPassword) {
        uni.showToast({
            title: '新密码不能与当前密码相同',
            icon: 'none'
        })
        return
    }

    isLoading.value = true

    try {
        await api.account.changePassword(form.value.currentPassword, form.value.newPassword)
        uni.showToast({
            title: '密码修改成功',
            icon: 'success'
        })
        form.value.currentPassword = ''
        form.value.newPassword = ''
        form.value.confirmPassword = ''
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '修改失败',
            icon: 'none'
        })
    } finally {
        isLoading.value = false
    }
}

async function handleDisablePasswordLogin() {
    isLoading.value = true

    try {
        await api.account.disablePasswordLogin()
        uni.showToast({
            title: '密码登录已关闭',
            icon: 'success'
        })
        canUsePasswordLogin.value = false
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '关闭失败',
            icon: 'none'
        })
    } finally {
        isLoading.value = false
    }
}
</script>

<style lang="scss" scoped>
.settings-container {
    input:focus {
        outline: none;
    }

    button:disabled {
        opacity: 0.6;
    }
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "账户设置",
        "navigationBarBackgroundColor": "#f4835a",
        "navigationBarTextStyle": "white"
    }
}
</route>