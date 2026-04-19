<template>
    <div class="account-security p-6">
        <el-card class="mb-6">
            <template #header>
                <div class="flex items-center justify-between">
                    <span class="text-lg font-medium">登录方式</span>
                </div>
            </template>

            <div v-if="loading" class="py-8 text-center text-gray-400">
                <div class="i-carbon:loading size-8 animate-spin mx-auto" />
                <div class="mt-2">加载中...</div>
            </div>

            <div v-else-if="bindings.length === 0" class="py-8 text-center text-gray-400">暂无登录方式</div>

            <div v-else class="space-y-4">
                <div
                    v-for="binding in bindings"
                    :key="binding.loginProvider"
                    class="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
                >
                    <div class="flex items-center gap-3">
                        <div class="size-10 rounded-full bg-blue-100 flex items-center justify-center">
                            <span
                                v-if="binding.loginProvider === 'Phone'"
                                class="i-carbon:phone text-blue-500 text-xl"
                            />
                            <span
                                v-else-if="binding.loginProvider.startsWith('WeChat')"
                                class="i-carbon:logo-wechat text-green-500 text-xl"
                            />
                            <span v-else class="i-carbon:user text-gray-500 text-xl" />
                        </div>
                        <div>
                            <div class="font-medium">{{ binding.providerDisplayName }}</div>
                            <div class="text-sm text-gray-500">{{ maskProviderKey(binding) }}</div>
                        </div>
                    </div>
                    <div class="flex items-center gap-2">
                        <span v-if="binding.bindTime" class="text-xs text-gray-400">
                            绑定于 {{ formatDate(binding.bindTime) }}
                        </span>
                        <el-button
                            v-if="canUnbind(binding)"
                            type="danger"
                            text
                            size="small"
                            @click="handleUnbind(binding)"
                        >
                            解绑
                        </el-button>
                    </div>
                </div>
            </div>
        </el-card>

        <el-card>
            <template #header>
                <span class="text-lg font-medium">添加登录方式</span>
            </template>

            <div class="flex gap-4">
                <el-button @click="showBindPhoneDialog = true">
                    <span class="i-carbon:phone mr-2" />
                    绑定手机号
                </el-button>
            </div>
        </el-card>

        <el-dialog v-model="showBindPhoneDialog" title="绑定手机号" width="400px">
            <el-form :model="bindPhoneForm" label-width="80px">
                <el-form-item label="手机号">
                    <el-input v-model="bindPhoneForm.phoneNumber" placeholder="请输入手机号" maxlength="11">
                        <template #prefix>
                            <span class="text-gray-400">+86</span>
                        </template>
                    </el-input>
                </el-form-item>
                <el-form-item label="验证码">
                    <div class="flex gap-2 w-full">
                        <el-input
                            v-model="bindPhoneForm.code"
                            placeholder="请输入验证码"
                            maxlength="6"
                            class="flex-1"
                        />
                        <el-button
                            :disabled="bindPhoneCountdown > 0 || !isValidBindPhone"
                            :loading="sendingBindSms"
                            @click="sendBindSmsCode"
                        >
                            {{ bindPhoneCountdown > 0 ? `${bindPhoneCountdown}s` : '获取验证码' }}
                        </el-button>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="showBindPhoneDialog = false">取消</el-button>
                <el-button type="primary" :loading="binding" :disabled="!canBindPhone" @click="handleBindPhone">
                    确认绑定
                </el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { LoginBindingDto } from '@/api/appService'

const loading = ref(false)
const bindings = ref<LoginBindingDto[]>([])

const showBindPhoneDialog = ref(false)
const bindPhoneForm = reactive({
    phoneNumber: '',
    code: '',
})
const sendingBindSms = ref(false)
const bindPhoneCountdown = ref(0)
const binding = ref(false)

let bindPhoneTimer: number | undefined = undefined

const isValidBindPhone = computed(() => /^1[3-9]\d{9}$/.test(bindPhoneForm.phoneNumber))
const canBindPhone = computed(() => isValidBindPhone.value && bindPhoneForm.code.length === 6)

onMounted(() => {
    loadBindings()
})

async function loadBindings() {
    loading.value = true
    try {
        const res = await api.account.getLoginBindings()
        bindings.value = res.items || []
    } catch (error: any) {
        ElMessage.error(error.message || '获取登录方式失败')
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

function formatDate(dateStr: string): string {
    if (!dateStr) return ''
    const date = new Date(dateStr)
    return date.toLocaleDateString('zh-CN')
}

function canUnbind(binding: LoginBindingDto): boolean {
    return bindings.value.length > 1
}

async function handleUnbind(binding: LoginBindingDto) {
    if (!canUnbind(binding)) {
        ElMessage.warning('至少需要保留一种登录方式')
        return
    }

    try {
        await ElMessageBox.confirm(
            `确定要解绑${binding.providerDisplayName}吗？解绑后将无法使用该方式登录。`,
            '解绑确认',
            { type: 'warning' }
        )

        await api.account.unbindLogin({
            body: {
                loginProvider: binding.loginProvider,
                providerKey: binding.providerKey,
            },
        })

        ElMessage.success('解绑成功')
        await loadBindings()
    } catch (error: any) {
        if (error !== 'cancel') {
            ElMessage.error(error.message || '解绑失败')
        }
    }
}

async function sendBindSmsCode() {
    if (!isValidBindPhone.value) {
        ElMessage.error('请输入正确的手机号')
        return
    }

    sendingBindSms.value = true
    try {
        await api.tokenAuth.sendSmsCode({
            body: { phoneNumber: bindPhoneForm.phoneNumber, purpose: 'bindphone' },
        })
        ElMessage.success('验证码已发送')
        bindPhoneCountdown.value = 60
        bindPhoneTimer = window.setInterval(() => {
            bindPhoneCountdown.value--
            if (bindPhoneCountdown.value <= 0) {
                clearInterval(bindPhoneTimer)
            }
        }, 1000)
    } catch (error: any) {
        ElMessage.error(error.message || '发送验证码失败')
    } finally {
        sendingBindSms.value = false
    }
}

async function handleBindPhone() {
    if (!canBindPhone.value) return

    binding.value = true
    try {
        await api.account.bindPhone({
            body: {
                phoneNumber: bindPhoneForm.phoneNumber,
                code: bindPhoneForm.code,
            },
        })

        ElMessage.success('绑定成功')
        showBindPhoneDialog.value = false
        bindPhoneForm.phoneNumber = ''
        bindPhoneForm.code = ''
        await loadBindings()
    } catch (error: any) {
        ElMessage.error(error.message || '绑定失败')
    } finally {
        binding.value = false
    }
}

onUnmounted(() => {
    clearInterval(bindPhoneTimer)
})
</script>
