<template>
    <el-dialog v-model="dialogVisible" title="用户设置" width="600px">
        <!-- 只读信息区域 -->
        <div class="user-info-display">
            <span class="info-label">用户编号：</span>
            <span class="info-value">{{ userId }}</span>
            <span class="info-label">诚信履约金：</span>
            <span class="info-value">¥{{ depositBalance }}</span>
            <el-button type="success" size="small" @click="handleDeposit"> 充值 </el-button>
            <el-button type="primary" size="small" @click="withdrawDialogVisible = true"> 提现 </el-button>
        </div>

        <el-form
            ref="ruleFormRef"
            style="max-width: 600px"
            :model="form"
            :rules="rules"
            label-width="auto"
            class="demo-ruleForm"
            status-icon
        >
            <el-form-item label="头像" prop="headImgUrl">
                <tt-upload
                    v-model="form.headImgUrl"
                    css-class="avatar-uploader"
                    :file-size="2048"
                    :multiple="false"
                    @on-uploaded="handleUploaded"
                >
                    <img v-if="form.headImgUrl" :src="form.headImgUrl" class="avatar" />
                    <div v-else class="i-carbon:plus size-6 text-gray-500"></div>
                </tt-upload>
            </el-form-item>

            <el-form-item label="昵称" prop="name">
                <el-input v-model="form.name" />
            </el-form-item>
            <el-form-item label="qq" prop="qq">
                <el-input v-model="form.qq" />
            </el-form-item>
            <el-form-item label="微信号" prop="wx">
                <el-input v-model="form.wx" />
            </el-form-item>
            <el-form-item label="登录用户名" prop="userName">
                <el-input v-model="form.userName" />
            </el-form-item>
            <el-form-item label="密码" prop="password">
                <el-input v-model="form.password" placeholder="修改密码请填入,不修改留空" />
            </el-form-item>
            <div>
                <el-button type="primary" @click="submitForm"> 更新 </el-button>
                <el-button @click="dialogVisible = false">关闭</el-button>
            </div>
        </el-form>
    </el-dialog>

    <withdrawDialog v-model:show="withdrawDialogVisible" title="提示" :show-cancel="false" @confirm="handleConfirm">
        <div>平台提现功能尚未完善，诚信履约金退款，请加管理员老淡QQ：383875411，微信：18845639111，私信扫码退款。</div>
    </withdrawDialog>
</template>

<script setup lang="ts">
import TtUpload from '@/components/tt-upload/index.vue'
import type { FormInstance, FormRules } from 'element-plus'
import { UserEditDto } from '@/api/appService'
import withdrawDialog from '@/components/CustomModal.vue'
import api from '@/api'
import { useRouter } from 'vue-router'

const userStore = useUserStore()
const router = useRouter()
const ruleFormRef = ref<FormInstance>()

// 只读字段，不参与表单提交
const userId = ref<number>(0)
const depositBalance = ref<number>(0)

const form = ref<Omit<UserEditDto, 'id' | 'depositBalance'>>({
    name: '',
    userName: '',
    password: '',
    headImgUrl: '',
    qq: '',
    wx: '',
})

const rules = reactive<FormRules<UserEditDto>>({
    name: [
        { required: true, message: '请输入昵称', trigger: ['change', 'blur'] },
        { min: 2, max: 24, message: '长度不能小于2个字符', trigger: ['change', 'blur'] },
    ],
    userName: [
        { required: true, message: '请输入登录用户名', trigger: ['change', 'blur'] },
        { min: 4, max: 32, message: '长度不能小于4个字符', trigger: ['change', 'blur'] },
    ],
    qq: [{ required: true, message: '请输入QQ号', trigger: ['change', 'blur'] }],
})

const submitForm = async () => {
    if (!ruleFormRef.value) return
    await ruleFormRef.value.validate((valid: boolean, _fields: object) => {
        if (valid) {
            debounce(realSave, 300)()
        } else {
            Tips.error('请检查表单错误!')
        }
    })
}

async function realSave() {
    try {
        await api.user.update({ body: form.value })
        Tips.success('更新成功')
        userStore.getUserInfo()
        dialogVisible.value = false
    } catch (err) {
        Tips.error((err as Error).message || '更新失败')
    }
}

function handleUploaded(e: { url: string }) {
    form.value.headImgUrl = `${e.url}!w300`
}

const dialogVisible = ref(false)
const show = async (e: boolean) => {
    dialogVisible.value = e
    if (e) {
        try {
            const res = await api.user.getCurrentUser()
            if (res.user) {
                userId.value = res.user.id ?? 0
                depositBalance.value = res.user.depositBalance ?? 0
                form.value = {
                    id: res.user.id ?? 0,
                    name: res.user.name ?? '',
                    userName: res.user.userName ?? '',
                    password: '',
                    headImgUrl: res.user.headImgUrl ?? '',
                    qq: res.user.qq ?? '',
                    wx: res.user.wx ?? '',
                }
            }
        } catch (err) {
            Tips.error((err as Error).message || '获取用户信息失败')
        }
    }
}

const withdrawDialogVisible = ref(false)
const handleConfirm = () => {
    withdrawDialogVisible.value = false
}

// 导航到统一支付页面
const handleDeposit = () => {
    dialogVisible.value = false
    router.push({
        path: '/payment',
        query: {
            type: 'deposit',
            returnUrl: '/index',
        },
    })
}

defineExpose({
    show,
})
</script>
<style lang="scss" scoped>
// 系统主色调
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$border-color: #ae6f4d;

// 弹窗容器样式
:deep(.el-dialog) {
    border-radius: 20px;
    border: 3px solid $border-color;
    box-shadow: 0 8px 32px rgba(131, 58, 0, 0.25);
    overflow: hidden;
}

// 标题栏样式
:deep(.el-dialog__header) {
    background: linear-gradient(135deg, $primary-color 0%, $primary-light 100%);
    margin: 0;
    padding: 0;

    .el-dialog__title {
        color: #fff;
        font-weight: 600;
        font-size: 18px;
    }

    .el-dialog__headerbtn {
        top: 12px;
        right: 16px;

        .el-dialog__close {
            color: #fff;
            font-size: 20px;

            &:hover {
                color: #fff2e8;
            }
        }
    }
}

// 弹窗 body 背景
:deep(.el-dialog__body) {
    background: $bg-light;
    padding: 32px;
}

// 表单样式
:deep(.el-form-item) {
    margin-bottom: 24px;

    .el-form-item__label {
        color: $primary-color;
        font-weight: 600;
    }
}

// 输入框样式
:deep(.el-input) {
    .el-input__wrapper {
        border-radius: 8px;
        border: 2px solid transparent;
        transition: all 0.3s ease;

        &:hover {
            border-color: $primary-light;
        }

        &.is-focus {
            border-color: $primary-color;
            box-shadow: 0 0 0 3px rgba(131, 58, 0, 0.15);
        }
    }
}

// 按钮样式
:deep(.el-button--primary) {
    background: linear-gradient(135deg, $primary-color 0%, $primary-light 100%);
    border: none;
    border-radius: 8px;
    padding: 12px 32px;
    font-weight: 600;

    &:hover {
        opacity: 0.9;
        transform: translateY(-1px);
    }
}

:deep(.el-button) {
    border-radius: 8px;
    padding: 12px 24px;
}

// 头像上传样式
.avatar-uploader {
    :deep(.avatar) {
        width: 96px;
        height: 96px;
        border-radius: 50%;
        border: 3px solid $border-color;
        object-fit: cover;
    }
}

// 弹窗内容布局
.demo-ruleForm {
    max-width: 500px;
    margin: 0 auto;
}

// 只读信息区域
.user-info-display {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 12px 16px;
    background: rgba(131, 58, 0, 0.08);
    border-radius: 8px;
    margin-bottom: 24px;
    flex-wrap: wrap;

    .info-label {
        color: $primary-color;
        font-weight: 600;
        font-size: 14px;
    }

    .info-value {
        color: $primary-light;
        font-weight: 600;
        font-size: 14px;
        margin-right: 8px;
    }

    .el-button {
        padding: 8px 16px;
        font-size: 12px;
    }
}
</style>
