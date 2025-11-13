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
                <!-- 开发调试登录 -->
                <div v-if="isDev" class="mt-4">
                    <div class="text-gray-500 text-sm mb-2">开发调试登录：</div>
                    <div class="flex flex-wrap gap-2">
                        <el-button
                            :loading="devLoading"
                            size="small"
                            type="warning"
                            @click="devLoginAsUser(1)"
                        >
                            用户1登录
                        </el-button>
                        <el-button
                            :loading="devLoading"
                            size="small"
                            type="warning"
                            @click="devLoginAsUser(2)"
                        >
                            用户2登录
                        </el-button>
                        <el-button
                            :loading="devLoading"
                            size="small"
                            type="warning"
                            @click="devLoginAsUser(3)"
                        >
                            用户3登录
                        </el-button>
                        <el-button
                            :loading="devLoading"
                            size="small"
                            type="warning"
                            @click="devLoginAsUser(14)"
                        >
                            用户14登录
                        </el-button>
                    </div>
                </div>
                <div class="text-blue-400 text-sm mt-6 cursor-pointer" @click="loginType = 1">使用扫码登录</div>
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
const devLoading = ref(false)
const qrTimeout = ref(false)
const form = reactive({
    username: '',
    password: '',
    rememberClient: false,
})

// 检查是否为开发环境
const isDev = computed(() => {
    return import.meta.env.MODE === 'development'
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

// 开发调试登录
async function devLoginAsUser(userId: number) {
    devLoading.value = true
    try {
        await userStore.devLogin(userId)
        await userStore.getUserInfo()
        clearInterval(interVal)
        if (route.query.redirect) window.location.href = '/index.html#' + route.query.redirect
        else window.location.href = '/index.html'
    } catch (error: any) {
        console.error('开发调试登录失败:', error)
        ElMessage({
            type: 'error',
            message: `开发调试登录失败: ${error.message}`,
        })
    } finally {
        devLoading.value = false
    }
}

onUnmounted(() => {
    clearInterval(interVal)
})
</script>
