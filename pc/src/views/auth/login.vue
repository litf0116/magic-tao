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
                    <span class="cursor-pointer" @click="loginType = 3">APP 扫码登录</span>
                    <span class="text-gray-300">|</span>
                    <span class="cursor-pointer" @click="loginType = 2">密码/验证码登录</span>
                </div>
            </template>
            <template v-else-if="loginType === 3">
                <!-- APP 扫码登录 -->
                <div class="relative size-240px">
                    <img
                        v-if="appQrDataUrl"
                        class="size-240px"
                        :class="[appQrStatus === 'expired' ? 'blur' : '']"
                        :src="appQrDataUrl"
                    />
                    <div
                        v-if="appQrLoading && !appQrDataUrl"
                        class="size-240px flex flex-col flex-center text-gray-400"
                    >
                        <div class="i-carbon:loading size-16 animate-spin" />
                        <div class="mt-4">生成二维码中...</div>
                    </div>
                    <div
                        v-if="appQrError && !appQrDataUrl"
                        class="size-240px flex flex-col flex-center text-red-400 text-xl"
                    >
                        <div class="i-carbon:error size-16" />
                        <div class="mt-4">{{ appQrError }}</div>
                        <div class="mt-2">
                            <span class="cursor-pointer text-blue-400 underline" @click.stop="initAppQrLogin()"
                                >重试</span
                            >
                        </div>
                    </div>
                    <div
                        v-if="appQrStatus === 'expired'"
                        class="absolute top-0 left-0 size-240px flex flex-col flex-center text-white text-xl"
                    >
                        <div class="i-carbon:error size-16 text-amber" />
                        <div class="mt-4">二维码已过期</div>
                        <div>
                            请
                            <span class="cursor-pointer text-blue-400 underline" @click.stop="initAppQrLogin()"
                                >刷新</span
                            >
                            后重试
                        </div>
                    </div>
                </div>
                <div class="text-gray-800 mt-4">APP 扫码登录</div>
                <div class="text-gray text-sm mt-2">使用魔力淘 APP 扫码登录</div>
                <div class="text-gray text-sm mt-2">
                    剩余时间: <span :class="[appQrCountdown <= 10 ? 'text-red-400' : '']">{{ appQrCountdown }}</span> 秒
                    <span v-if="appQrStatus === 'expired'" class="ml-2 cursor-pointer text-blue-400" @click="refreshAppQr"
                        >刷新</span
                    >
                </div>
                <div class="text-gray text-sm mt-2">
                    状态:
                    <span v-if="appQrStatus === 'pending'" class="text-gray-500">等待扫码...</span>
                    <span v-else-if="appQrStatus === 'scanned'" class="text-blue-400">已扫码，等待确认...</span>
                    <span v-else-if="appQrStatus === 'confirmed'" class="text-green-500">登录成功</span>
                    <span v-else-if="appQrStatus === 'expired'" class="text-red-400">已过期</span>
                </div>
                <div class="flex justify-center gap-4 text-blue-400 text-sm mt-4">
                    <span class="cursor-pointer" @click="loginType = 1">微信扫码登录</span>
                    <span class="text-gray-300">|</span>
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
                    <span class="cursor-pointer" @click="loginType = 3">APP 扫码登录</span>
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
import QRCode from 'qrcode'
import { generateQrCode, getQrCodeStatus } from '@/api/qrcode'
import type { QrCodeStatusDto } from '@/api/qrcode'

const userStore = useUserStore()
const chatStore = useChatStore()
const route = useRoute()

const loginType = ref(1) // 1: 微信扫码, 2: 密码/验证码, 3: APP扫码
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

// APP 扫码登录状态
const appQrDataUrl = ref('')
const appQrLoading = ref(false)
const appQrError = ref('')
const appQrCode = ref('')
const appQrCountdown = ref(60)
const appQrStatus = ref<QrCodeStatusDto['status']>('pending')
let appQrTimer: number | undefined = undefined
let appQrCountdownTimer: number | undefined = undefined

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

// 监听 loginType 变化，自动初始化 APP 扫码登录
watch(loginType, (newType) => {
    if (newType === 3) {
        initAppQrLogin()
    } else {
        // 离开 APP 扫码页面时停止轮询
        stopAppQrPolling()
        stopAppQrCountdown()
    }
})

// APP 扫码登录 - 生成二维码
async function initAppQrLogin() {
    console.log('[DEBUG initAppQrLogin] 开始初始化 APP 扫码登录')
    stopAppQrPolling()
    appQrLoading.value = true
    appQrError.value = ''
    appQrDataUrl.value = ''
    appQrCountdown.value = 60
    appQrStatus.value = 'pending'

    try {
        const res = await generateQrCode()
        console.log('[DEBUG initAppQrLogin] generateQrCode 返回:', res)
        appQrCode.value = res.code

        // 使用 qrcode 库生成二维码图片
        const dataUrl = await QRCode.toDataURL(res.qrContent, {
            width: 240,
            margin: 2,
            color: {
                dark: '#000000',
                light: '#ffffff',
            },
        })
        appQrDataUrl.value = dataUrl
        appQrLoading.value = false

        // 开始倒计时
        startAppQrCountdown(res.expiresIn)

        // 开始轮询状态
        startAppQrPolling()
    } catch (err: any) {
        console.error('[DEBUG initAppQrLogin] 生成二维码失败:', err)
        appQrLoading.value = false
        appQrError.value = err?.message || '生成二维码失败，请重试'
    }
}

// 开始倒计时
function startAppQrCountdown(expiresIn: number) {
    appQrCountdown.value = expiresIn
    appQrCountdownTimer = window.setInterval(() => {
        appQrCountdown.value--
        if (appQrCountdown.value <= 0) {
            stopAppQrCountdown()
            appQrStatus.value = 'expired'
            stopAppQrPolling()
        }
    }, 1000)
}

// 停止倒计时
function stopAppQrCountdown() {
    if (appQrCountdownTimer) {
        clearInterval(appQrCountdownTimer)
        appQrCountdownTimer = undefined
    }
}

// 开始轮询状态
function startAppQrPolling() {
    appQrTimer = window.setInterval(async () => {
        if (!appQrCode.value) return

        try {
            const status = await getQrCodeStatus(appQrCode.value)
            console.log('[DEBUG appQrPolling] 状态:', status)
            appQrStatus.value = status.status

            if (status.status === 'confirmed' && status.user) {
                // 登录成功
                stopAppQrPolling()
                stopAppQrCountdown()
                ElMessage({ type: 'success', message: `欢迎回来，${status.user.nickname}` })
                // TODO: 后端需要返回 token，这里暂时用用户信息
                // 等后端 API 完善
                await userStore.getUserInfo()
                navigateToHome()
            } else if (status.status === 'expired') {
                stopAppQrPolling()
                stopAppQrCountdown()
            }
        } catch (err) {
            console.error('[DEBUG appQrPolling] 轮询失败:', err)
        }
    }, 2000)
}

// 停止轮询
function stopAppQrPolling() {
    if (appQrTimer) {
        clearInterval(appQrTimer)
        appQrTimer = undefined
    }
}

// 刷新二维码
function refreshAppQr() {
    initAppQrLogin()
}

async function initQrLogin() {
    console.log('[DEBUG initQrLogin] 开始初始化扫码登录')
    clearTimeout(expiredTimer)
    clearInterval(interVal)
    qrTimeout.value = false
    try {
        const res = await chatStore.init()
        console.log('[DEBUG initQrLogin] chatStore.init() 返回:', res)
        console.log('[DEBUG initQrLogin] 调用 chatStore.initQr()')
        await chatStore.initQr(res)
        console.log('[DEBUG initQrLogin] chatStore.initQr() 完成')
        expiredTimer = setTimeout(() => {
            console.log('[DEBUG initQrLogin] 二维码过期定时器触发')
            qrTimeout.value = true
            clearInterval(interVal)
        }, 60_000)

        interVal = setInterval(() => {
            console.log('[DEBUG initQrLogin] 轮询 QrToken, key:', res)
            api.tokenAuth
                .qrToken({ key: res })
                .then(async (accessToken) => {
                    console.log('[DEBUG initQrLogin] QrToken 返回:', accessToken)
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
    await userStore.login(form).then(
        async () => {
            loading.value = false
            ElMessage({ type: 'success', message: '登录成功' })
            await userStore.getUserInfo()
            clearInterval(interVal)
            navigateToHome()
        },
        async (error: any) => {
            loading.value = false
            ElMessage({
                type: 'error',
                dangerouslyUseHTMLString: true,
                message: `<strong>${error.message}</strong><br/><p class="py-4">${error.details}</p>`,
            })
        }
    )
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
