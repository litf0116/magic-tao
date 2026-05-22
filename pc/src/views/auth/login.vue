<template>
    <div class="flex flex-col items-center justify-center space-y-4 p-4 text-center">
        <div class="p-8 bg-white rounded-lg">
            <template v-if="loginType === 1">
                <div class="relative size-240px">
                    <img
                        v-if="chatStore.pubQrUrl"
                        class="size-240px"
                        :class="[qrTimeout ? 'blur' : '']"
                        :src="chatStore.pubQrUrl"
                    />
                    <div
                        v-if="chatStore.qrLoading && !chatStore.pubQrUrl"
                        class="size-240px flex flex-col flex-center text-gray-400"
                    >
                        <div class="i-carbon:loading size-16 animate-spin" />
                        <div class="mt-4">加载中...</div>
                    </div>
                    <div
                        v-if="chatStore.qrError && !chatStore.pubQrUrl"
                        class="size-240px flex flex-col flex-center text-red-400 text-xl"
                    >
                        <div class="i-carbon:error size-16" />
                        <div class="mt-4">{{ chatStore.qrError }}</div>
                        <div class="mt-2">
                            <span class="cursor-pointer text-blue-400 underline" @click.stop="initQrLogin()">重试</span>
                        </div>
                    </div>
                    <div
                        v-if="qrTimeout"
                        class="absolute top-0 left-0 size-240px flex flex-col flex-center text-white text-xl"
                    >
                        <div class="i-carbon:error size-16 text-amber" />
                        <div class="mt-4">二维码已过期</div>
                        <div>
                            请
                            <span class="cursor-pointer text-blue-400 underline" @click.stop="initQrLogin()">刷新</span>
                            后重试
                        </div>
                    </div>
                </div>
                <div class="text-gray-800">扫码登录魔力淘</div>
                <div class="text-gray text-sm mt-4">扫码关注即可快捷注册/登录</div>
                <div class="flex justify-center gap-4 text-blue-400 text-sm mt-4">
                    <span class="cursor-pointer" @click="loginType = 2">密码/验证码登录</span>
                </div>
            </template>
            <template v-else>
                <div>
                    <img
                        class="w-72"
                        :src="'https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300'"
                    />
                </div>

                <div class="mt-6">
                    <div class="flex border-b border-gray-200 mb-6">
                        <div
                            class="px-4 py-2 cursor-pointer text-sm"
                            :class="
                                passwordTab === 'password'
                                    ? 'text-blue-500 border-b-2 border-blue-500'
                                    : 'text-gray-500'
                            "
                            @click="passwordTab = 'password'"
                        >
                            密码登录
                        </div>
                        <div
                            class="px-4 py-2 cursor-pointer text-sm"
                            :class="
                                passwordTab === 'sms' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-500'
                            "
                            @click="passwordTab = 'sms'"
                        >
                            验证码登录
                        </div>
                    </div>

                    <template v-if="passwordTab === 'password'">
                        <div>
                            <el-input v-model="form.username" placeholder="请输入账号" />
                        </div>
                        <div class="mt-4">
                            <el-input v-model="form.password" type="password" placeholder="请输入密码" show-password />
                        </div>
                        <div class="mt-6">
                            <el-button
                                :loading="loading"
                                class="w-full"
                                type="primary"
                                :disabled="submitDisabled"
                                @click="login"
                                >登录</el-button
                            >
                        </div>
                    </template>

                    <template v-else>
                        <div>
                            <el-input v-model="smsForm.phoneNumber" placeholder="请输入手机号" maxlength="11">
                                <template #prefix>
                                    <span class="text-gray-400">+86</span>
                                </template>
                            </el-input>
                        </div>
                        <div class="mt-4 flex gap-2">
                            <el-input v-model="smsForm.code" placeholder="请输入验证码" maxlength="6" class="flex-1" />
                            <el-button
                                :disabled="smsCountdown > 0 || !isValidPhone"
                                :loading="sendingSms"
                                @click="sendSmsCode"
                            >
                                {{ smsCountdown > 0 ? `${smsCountdown}s` : '获取验证码' }}
                            </el-button>
                        </div>
                        <div class="mt-6">
                            <el-button
                                :loading="loading"
                                class="w-full"
                                type="primary"
                                :disabled="smsSubmitDisabled"
                                @click="phoneLogin"
                                >登录</el-button
                            >
                        </div>
                    </template>
                </div>

                <div class="flex justify-center gap-4 text-blue-400 text-sm mt-6">
                    <span class="cursor-pointer" @click="loginType = 1">微信扫码登录</span>
                    <span class="text-gray-300">|</span>
                    <span class="cursor-pointer" @click="loginType = 2">密码/验证码登录</span>
                </div>
            </template>
        </div>
    </div>

    <ProfileCompletionGuide ref="profileGuideRef" />
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage } from 'element-plus'
import ProfileCompletionGuide from '@/components/ProfileCompletionGuide.vue'

const userStore = useUserStore()
const chatStore = useChatStore()
const route = useRoute()

const loginType = ref(1) // 1: 微信扫码, 2: 密码/验证码
const passwordTab = ref('password')
const loading = ref(false)
const qrTimeout = ref(false)
const form = reactive({
    username: '',
    password: '',
    rememberClient: false,
})

const smsForm = reactive({
    phoneNumber: '',
    code: '',
})

const sendingSms = ref(false)
const smsCountdown = ref(0)
let smsTimer: number | undefined = undefined

const SMS_COUNTDOWN_KEY = 'sms_countdown_end_time'

function restoreSmsCountdown() {
    const endTime = sessionStorage.getItem(SMS_COUNTDOWN_KEY)
    if (endTime) {
        const remaining = Math.floor((parseInt(endTime) - Date.now()) / 1000)
        if (remaining > 0) {
            smsCountdown.value = remaining
            startSmsTimer()
        } else {
            sessionStorage.removeItem(SMS_COUNTDOWN_KEY)
        }
    }
}

function startSmsTimer() {
    smsTimer = window.setInterval(() => {
        smsCountdown.value--
        if (smsCountdown.value <= 0) {
            clearInterval(smsTimer)
            sessionStorage.removeItem(SMS_COUNTDOWN_KEY)
        }
    }, 1000)
}

let interVal: number | undefined = undefined
let expiredTimer: number | undefined = undefined

const profileGuideRef = ref()

const isValidPhone = computed(() => /^1[3-9]\d{9}$/.test(smsForm.phoneNumber))

const submitDisabled = computed(() => {
    return !form.username || !form.password
})

const smsSubmitDisabled = computed(() => {
    return !isValidPhone.value || !smsForm.code || smsForm.code.length !== 6
})

onMounted(() => {
    initQrLogin()
    restoreSmsCountdown()
})

async function initQrLogin() {
    clearTimeout(expiredTimer)
    clearInterval(interVal)
    qrTimeout.value = false
    try {
        const res = await chatStore.init()
        await chatStore.initQr(res)
        expiredTimer = setTimeout(() => {
            qrTimeout.value = true
            clearInterval(interVal)
        }, 60_000)

        interVal = setInterval(() => {
            api.tokenAuth
                .qrToken({ key: res })
                .then(async (accessToken) => {
                    if (accessToken) {
                        clearTimeout(expiredTimer)
                        await userStore.SET_TOKEN(accessToken)
                        const userInfo = await userStore.getUserInfo()
                        clearInterval(interVal)
                        if (userInfo?.user?.needProfileCompletion) {
                            profileGuideRef.value?.show(userInfo.user.id)
                        } else {
                            navigateToHome()
                        }
                    }
                })
                .catch((err) => {
                    console.error('[DEBUG initQrLogin] 轮询二维码登录状态失败:', err)
                })
        }, 2000)
    } catch (err) {
        console.error('[DEBUG initQrLogin] 初始化二维码登录失败:', err)
    }
}

async function login() {
    if (!form.username) {
        ElMessage({ type: 'error', message: '请输入账号' })
        return
    }
    if (!form.password) {
        ElMessage({ type: 'error', message: '请输入密码' })
        return
    }
    loading.value = true
    try {
        await userStore.login(form)
        loading.value = false
        ElMessage({ type: 'success', message: '登录成功' })
        await userStore.getUserInfo()
        clearInterval(interVal)
        navigateToHome()
    } catch (error: any) {
        loading.value = false
        ElMessage({
            type: 'error',
            dangerouslyUseHTMLString: true,
            message: `<strong>${error?.message || '登录失败'}</strong><br/><p class="py-4">${error?.details || ''}</p>`,
        })
    }
}

async function sendSmsCode() {
    if (!isValidPhone.value) {
        ElMessage({ type: 'error', message: '请输入正确的手机号' })
        return
    }

    sendingSms.value = true
    try {
        await api.tokenAuth.sendSmsCode({
            body: { phoneNumber: smsForm.phoneNumber },
        })
        ElMessage({ type: 'success', message: '验证码已发送' })
        smsCountdown.value = 60
        sessionStorage.setItem(SMS_COUNTDOWN_KEY, (Date.now() + 60000).toString())
        startSmsTimer()
    } catch (error: any) {
        ElMessage({ type: 'error', message: error.message || '发送验证码失败' })
    } finally {
        sendingSms.value = false
    }
}

async function phoneLogin() {
    if (!isValidPhone.value) {
        ElMessage({ type: 'error', message: '请输入正确的手机号' })
        return
    }
    if (!smsForm.code || smsForm.code.length !== 6) {
        ElMessage({ type: 'error', message: '请输入6位验证码' })
        return
    }

    loading.value = true
    try {
        const res = await api.tokenAuth.phoneAuthenticate({
            body: { phoneNumber: smsForm.phoneNumber, code: smsForm.code },
        })
        if (res.accessToken) {
            await userStore.SET_TOKEN(res.accessToken)
            await userStore.getUserInfo()
            clearInterval(interVal)
            ElMessage({ type: 'success', message: '登录成功' })
            navigateToHome()
        }
    } catch (error: any) {
        ElMessage({ type: 'error', message: error.message || '登录失败，请重试' })
    } finally {
        loading.value = false
    }
}

function navigateToHome() {
    if (route.query.redirect) window.location.href = '/index.html#' + route.query.redirect
    else window.location.href = '/index.html'
}

onUnmounted(() => {
    clearInterval(interVal)
    clearInterval(smsTimer)
    stopAppQrPolling()
    stopAppQrCountdown()
})
</script>
