<template>
    <div class="flex flex-col items-center justify-center space-y-4 p-4 text-center">
        <!-- <div>微信网页登录</div>
        <div>{{ chatStore.qrUrl }}</div>
        <div>
            <img v-if="chatStore.qrUrl" class="size-240px" :src="chatStore.qrUrl" />
        </div> -->
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
                        v-if="qrTimeout"
                        class="absolute top-0 left-0 size-240px flex flex-col flex-center text-white text-xl"
                    >
                        <div class="i-carbon:error size-16 text-amber"></div>
                        <div class="mt-4">二维码已过期</div>
                        <div>
                            请
                            <span class="cursor-pointer text-blue-400 underline" @click.stop="init()">刷新</span> 后重试
                        </div>
                    </div>
                </div>
                <div class="text-gray-800">扫码登录魔力淘</div>
                <div class="text-gray text-sm mt-4">扫码关注即可快捷注册/登录</div>
                <div class="text-blue-400 text-sm mt-4 cursor-pointer" @click="loginType = 2">使用密码/验证码登录</div>
            </template>
            <template v-else>
                <div>
                    <img
                        class="w-72"
                        :src="'https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300'"
                    />
                </div>
                <div class="mt-6">
                    <el-input v-model="form.username" placeholder="请输入用户名" />
                </div>
                <div class="mt-6">
                    <el-input v-model="form.password" type="password" placeholder="请输入密码" />
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
                <div class="text-blue-400 text-sm mt-6 cursor-pointer" @click="loginType = 1">使用扫码登录</div>

                <!-- Token登录入口 -->
                <div v-if="isDevMode" class="mt-6 p-4 bg-yellow-50 border border-yellow-200 rounded">
                    <div class="text-yellow-600 font-semibold mb-2">🔧 Token直接登录</div>
                    <div class="space-y-3">
                        <div>
                            <el-input
                                v-model="tokenInput"
                                type="textarea"
                                :rows="3"
                                placeholder="粘贴完整的access token"
                                class="w-full"
                                size="small"
                            />
                        </div>
                        <div class="flex gap-2">
                            <el-button
                                type="warning"
                                size="small"
                                @click="tokenLogin"
                                :loading="loading"
                            >
                                使用Token登录
                            </el-button>
                            <el-button
                                type="info"
                                size="small"
                                plain
                                @click="clearToken"
                            >
                                清空
                            </el-button>
                        </div>
                    </div>
                    <div class="text-xs text-yellow-500 mt-2">
                        提示：直接使用有效的access token，无需用户名密码验证
                    </div>
                </div>
            </template>
        </div>
    </div>
</template>

<script lang="ts" setup>
import api from '@/api'
import { ElMessage } from 'element-plus'

const userStore = useUserStore()
const chatStore = useChatStore()
const route = useRoute()

const loginType = ref(1)
const loading = ref(false)
const qrTimeout = ref(false)
const form = reactive({
    username: '',
    password: '',
    rememberClient: false,
})
const tokenInput = ref('') // token输入

// 判断是否为开发模式
const isDevMode = computed(() => {
    return import.meta.env.DEV
})
let interVal: number | undefined = undefined

let expiredTimer: number | undefined = undefined

onMounted(() => {
    init()
})

function init() {
    debounce(() => {
        chatStore.init().then(async (res) => {
            qrTimeout.value = false
            await chatStore.initQr(res)
            expiredTimer = setTimeout(() => {
                qrTimeout.value = true
                clearInterval(interVal)
            }, 60_000) // 1 minute

            interVal = setInterval(() => {
                api.tokenAuth.qrToken({ key: res }).then(async (res) => {
                    if (res) {
                        clearTimeout(expiredTimer)
                        await userStore.SET_TOKEN(res)
                        await userStore.getUserInfo()
                        clearInterval(interVal)
                        if (route.query.redirect) window.location.href = '/index.html#' + route.query.redirect
                        else window.location.href = '/index.html'
                    }
                })
            }, 2000)
        })
    }, 200)()
}

const submitDisabled = computed(() => {
    return !form.username || !form.password
})

async function login() {
    if (!form.username) {
        ElMessage({
            type: 'error',
            message: `请输入用户名`,
        })
        return
    }
    if (!form.password) {
        ElMessage({
            type: 'error',
            message: `请输入密码`,
        })
        return
    }
    loading.value = true
    await userStore.login(form).then(
        async () => {
            loading.value = false

            // Just to simulate the time of the request
            //  console.log('login result ', token)
            await userStore.getUserInfo()
            clearInterval(interVal)
            if (route.query.redirect) window.location.href = '/index.html#' + route.query.redirect
            else window.location.href = '/index.html'
        },
        async (error: any) => {
            loading.value = false

            ElMessage({
                type: 'error',
                dangerouslyUseHTMLString: true,
                message: `<strong>${error.message}</strong><br/>
              <p class="py-4">${error.details}</p>`,
            })
        }
    )
}

// Token直接登录
async function tokenLogin() {
    if (!tokenInput.value.trim()) {
        ElMessage({
            type: 'warning',
            message: '请输入token',
        })
        return
    }

    loading.value = true

    try {
        // 直接设置token
        userStore.SET_TOKEN(tokenInput.value.trim())

        // 验证token是否已设置
        console.log('Token已设置:', localStorage.getItem('token'))

        // 验证token并获取用户信息
        await userStore.getUserInfo()
        clearInterval(interVal)

        loading.value = false

        ElMessage({
            type: 'success',
            message: 'Token登录成功！',
        })

        // 延迟跳转，确保状态已更新
        setTimeout(() => {
            if (route.query.redirect) {
                window.location.href = '/index.html#' + route.query.redirect
            } else {
                window.location.href = '/index.html'
            }
        }, 500)
    } catch (error: any) {
        loading.value = false

        // 清除无效的token
        userStore.RESET_TOKEN()

        ElMessage({
            type: 'error',
            message: `Token无效或已过期: ${error.message || '请检查token是否正确'}`,
        })
    }
}

// 清空token输入
function clearToken() {
    tokenInput.value = ''
}

onUnmounted(() => {
    clearInterval(interVal)
})
</script>
