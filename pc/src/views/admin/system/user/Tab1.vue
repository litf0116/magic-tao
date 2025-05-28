<template>
    <div v-if="form && form.user" class="grid gap-4 grid-cols-2">
        <el-form-item label="Id" prop="user.id">
            <el-input v-model="form.user.id" disabled />
        </el-form-item>
        <el-form-item label="用户名" prop="user.userName" required>
            <el-input v-model="form.user.userName" />
        </el-form-item>
        <el-form-item label="姓名" prop="user.name" required>
            <el-input v-model="form.user.name" @change="change" />
        </el-form-item>
        <el-form-item label="姓" prop="user.surname" required>
            <el-input v-model="form.user.surname" />
        </el-form-item>
        <el-form-item label="邮箱" prop="user.emailAddress" required>
            <el-input v-model="form.user.emailAddress" />
        </el-form-item>
        <el-form-item label="手机" prop="user.phoneNumber">
            <el-input v-model="form.user.phoneNumber" type="number" maxlength="11" />
        </el-form-item>
        <el-form-item label="保证金" prop="user.depositBalance">
            <el-input v-model="form.user.depositBalance" type="number" maxlength="11" />
        </el-form-item>
        <el-form-item v-if="form.user.detail" label="企业微信号" prop="user.detail.WechatWorkAcctId">
            <el-input v-model="form.user.detail.wechatWorkAcctId" />
        </el-form-item>
        <el-form-item label="密码" prop="user.password">
            <el-input v-model="form.user.password" type="password" />
        </el-form-item>
        <el-form-item label="是否启用" prop="user.isActive">
            <el-switch v-model="form.user.isActive" active-color="#13ce66" inactive-color="#ff4949" />
        </el-form-item>
    </div>
</template>

<script lang="ts">
export default defineComponent({
    setup() {
        const form = inject('form', {} as any)
        onMounted(() => {
            console.log(form)
        })

        const data = reactive({
            change: (e: string) => {
                if (e && e.length > 0) {
                    form.value.user.surname = e[0]
                }
            },
        })

        return { form, ...toRefs(data) }
    },
})
</script>
