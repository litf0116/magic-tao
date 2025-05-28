<template>
    <div v-if="form && form.user" class="grid gap-4 grid-cols-2">
        <el-form-item label="Id" prop="user.id">
            <el-input v-model="form.user.id" disabled />
        </el-form-item>
        <el-form-item label="用户名" prop="user.userName">
            <el-input v-model="form.user.userName" />
        </el-form-item>
        <el-form-item label="姓名" prop="user.surname">
            <el-input v-model="form.user.surname" />
        </el-form-item>
        <el-form-item label="邮箱" prop="user.emailAddress">
            <el-input v-model="form.user.emailAddress" />
        </el-form-item>
        <el-form-item label="手机" prop="user.phoneNumber">
            <el-input v-model="form.user.phoneNumber" type="number" maxlength="11" />
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
        const form = inject('form', {})
        onMounted(() => {
            console.log(form)
        })
        return { form }
    },
})
</script>
