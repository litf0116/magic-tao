<template>
    <el-dialog
        v-model="dialogVisible"
        title="完善账号信息"
        width="500px"
        :close-on-click-modal="false"
        :close-on-press-escape="false"
        :show-close="false"
    >
        <div class="text-center mb-6">
            <div class="i-carbon:warning size-16 text-amber mx-auto mb-4"></div>
            <p class="text-gray-600">为方便您使用更多登录方式，请完善以下信息</p>
        </div>

        <el-form ref="ruleFormRef" :model="form" :rules="rules" label-width="80px" status-icon>
            <el-form-item label="手机号" prop="phoneNumber">
                <el-input v-model="form.phoneNumber" placeholder="请输入手机号" maxlength="11" />
            </el-form-item>

            <el-form-item label="用户名" prop="userName">
                <el-input v-model="form.userName" placeholder="请输入用户名（用于登录）" />
            </el-form-item>

            <el-form-item label="密码" prop="password">
                <el-input v-model="form.password" type="password" placeholder="请设置密码（至少6位）" show-password />
            </el-form-item>
        </el-form>

        <template #footer>
            <div class="flex justify-between">
                <el-button @click="handleSkip">跳过</el-button>
                <div class="flex gap-2">
                    <el-button @click="handleSkip">稍后完善</el-button>
                    <el-button type="primary" :loading="loading" @click="handleComplete">完成</el-button>
                </div>
            </div>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import api from '@/api'

const dialogVisible = ref(false)
const loading = ref(false)

const ruleFormRef = ref<FormInstance>()

const form = ref({
    phoneNumber: '',
    userName: '',
    password: '',
})

const validatePhone = (rule: object, value: string, callback: any) => {
    if (!value) {
        callback(new Error('请输入手机号'))
    } else if (!/^1[3-9]\d{9}$/.test(value)) {
        callback(new Error('请输入正确的手机号'))
    } else {
        callback()
    }
}

const validateUserName = (rule: object, value: string, callback: any) => {
    if (!value) {
        callback(new Error('请输入用户名'))
    } else if (value.length < 2) {
        callback(new Error('用户名至少2位'))
    } else {
        callback()
    }
}

const validatePassword = (rule: object, value: string, callback: any) => {
    if (!value) {
        callback(new Error('请输入密码'))
    } else if (value.length < 6) {
        callback(new Error('密码至少6位'))
    } else {
        callback()
    }
}

const rules = reactive<FormRules>({
    phoneNumber: [{ required: true, validator: validatePhone, trigger: ['change', 'blur'] }],
    userName: [{ required: true, validator: validateUserName, trigger: ['change', 'blur'] }],
    password: [{ required: true, validator: validatePassword, trigger: ['change', 'blur'] }],
})

const show = () => {
    dialogVisible.value = true
    form.value = {
        phoneNumber: '',
        userName: '',
        password: '',
    }
}

const handleComplete = async () => {
    if (!ruleFormRef.value) return

    await ruleFormRef.value.validate((valid) => {
        if (!valid) return

        loading.value = true
        api.user
            .completeProfile({
                body: {
                    phoneNumber: form.value.phoneNumber,
                    userName: form.value.userName,
                    password: form.value.password,
                },
            })
            .then(
                () => {
                    ElMessage.success('信息完善成功')
                    dialogVisible.value = false
                    navigateToHome()
                },
                (error: any) => {
                    ElMessage.error(error.message || '完善信息失败')
                }
            )
            .finally(() => {
                loading.value = false
            })
    })
}

const handleSkip = () => {
    dialogVisible.value = false
    api.user.skipProfileCompletion().then(
        () => {
            navigateToHome()
        },
        () => {
            navigateToHome()
        }
    )
}

const navigateToHome = () => {
    window.location.href = '/index.html'
}

defineExpose({
    show,
})
</script>
