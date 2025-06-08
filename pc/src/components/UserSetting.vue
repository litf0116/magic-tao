<template>
    <el-dialog v-model="dialogVisible" title="用户设置" width="600px">
        <el-form
            ref="ruleFormRef"
            style="max-width: 600px"
            :model="form"
            :rules="rules"
            label-width="auto"
            class="demo-ruleForm"
            status-icon
        >
            <el-form-item label="用户编号" prop="id">
                {{ form.id }}
            </el-form-item>
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

                <div style="width: 75%; text-align: center">
                    保证金：{{ form.depositBalance }}
                    <el-button style="margin-left: 20px" type="primary" @click="withdrawDialogVisible = true">
                        提现
                    </el-button>
                </div>
            </el-form-item>

            <el-form-item label="昵称" prop="name">
                <el-input v-model="form.name" />
            </el-form-item>
            <!-- <el-form-item label="手机号码" prop="phoneNumber">
                <el-input v-model="form.phoneNumber" />
            </el-form-item> -->
            <el-form-item label="qq" prop="qq">
                <el-input v-model="form.qq" />
            </el-form-item>
            <el-form-item label="微信号" prop="wx">
                <el-input v-model="form.wx" />
            </el-form-item>
            <!-- <el-form-item label="邮箱" prop="emailAddress">
                <el-input v-model="form.emailAddress" />
            </el-form-item> -->
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

    <withdrawDialog v-model:show="withdrawDialogVisible" title="提示" :showCancel="false" @confirm="handleConfirm">
        <div>平台提现功能尚未完善，保证金退款，请加管理员老淡QQ：383875411，微信：18845639111，私信扫码退款。</div>
    </withdrawDialog>
</template>

<script setup lang="ts">
import TtUpload from '@/components/tt-upload/index.vue'
import type { FormInstance, FormRules } from 'element-plus'
import { UserEditDto } from '@/api/appService'
import withdrawDialog from '@/components/CustomModal.vue'
import api from '@/api'
const userStore = useUserStore()
const ruleFormRef = ref<FormInstance>()
const form = ref<UserEditDto>({
    id: -1,
    name: '',
    userName: '',
    password: '',
    headImgUrl: '',
    qq: '',
    wx: '',
    depositBalance: 0,
} as UserEditDto)

const rules = reactive<FormRules<UserEditDto>>({
    name: [
        { required: true, message: '请输入昵称', trigger: ['change', 'blur'] },
        { min: 2, max: 24, message: '长度不能小于2个字符', trigger: ['change', 'blur'] },
    ],
    userName: [
        { required: true, message: '请输入登录用户名', trigger: ['change', 'blur'] },
        { min: 4, max: 32, message: '长度不能小于4个字符', trigger: ['change', 'blur'] },
    ],
    // phoneNumber: [
    //     { required: true, message: '请输入正确的手机号码', trigger: ['change', 'blur'] },
    //     { min: 11, max: 11, message: '请输入正确的手机号码', trigger: ['change', 'blur'] },
    // ],
    qq: [{ required: true, message: '请输入QQ号', trigger: ['change', 'blur'] }],
    // emailAddress: [{ type: 'email', required: true, message: '请输入正确的邮箱', trigger: ['change', 'blur'] }],
})

const submitForm = async () => {
    if (!ruleFormRef.value) return
    await ruleFormRef.value.validate((valid: boolean, fields: object) => {
        console.log(valid, fields)
        // console.log(typeof valid, typeof fields)
        if (valid) {
            debounce(realSave, 300)()
        } else {
            Tips.error("请检查表单错误!'")
            // console.log('error submit!', fields)
        }
    })
}

function realSave() {
    api.user.update({ body: form.value }).then((res) => {
        Tips.success('更新成功')
        userStore.getUserInfo()
        dialogVisible.value = false
    })
}

function handleUploaded(e: { url: string }) {
    form.value = { ...form.value, headImgUrl: `${e.url}!w300` }
}

const dialogVisible = ref(false)
const show = (e: boolean) => {
    dialogVisible.value = e
    if (e) {
        api.user.getCurrentUser().then((res) => {
            if (res.user) form.value = { ...res.user! }
        })
    }
}

const withdrawDialogVisible = ref(false)
const handleConfirm = () => {
    withdrawDialogVisible.value = false
}

defineExpose({
    show,
})
</script>
<style scoped>
.avatar-uploader .avatar {
    width: 96px;
    height: 96px;
    display: block;
}
</style>
