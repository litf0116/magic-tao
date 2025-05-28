<template>
    <el-dialog v-model="dialogVisible" title="招募信息发布" width="500" destroy-on-close append-to-body>
        <el-form ref="ruleFormRef" :model="form" :rules="rules" label-width="auto" class="demo-ruleForm" status-icon>
            <el-form-item label="标题" prop="title" required>
                <el-input v-model="form.title" />
            </el-form-item>
            <el-form-item label="限定人数" prop="limit" required>
                <el-input v-model="form.limit" type="number" />
            </el-form-item>
        </el-form>
        <template #footer>
            <div class="dialog-footer">
                <el-button @click="dialogVisible = false">取消</el-button>
                <el-button type="primary" @click="submit"> 确定 </el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import { ChatGroupCreateOrUpdateDto } from '@/api/appService'
import api from '@/api'
import { FormInstance, FormRules } from 'element-plus'

const emit = defineEmits(['onSaved', 'onEdit'])
const dialogVisible = ref(false)
const ruleFormRef = ref<FormInstance>()
const form = ref<ChatGroupCreateOrUpdateDto | null>({})
const rules = reactive<FormRules<ChatGroupCreateOrUpdateDto>>({
    title: [{ required: true, message: '必填', trigger: ['change', 'blur'] }],
    limit: [{ required: true, message: '必填', trigger: ['change', 'blur'] }],
})

const show = (e: boolean) => {
    dialogVisible.value = e
    if (e) {
        api.chatGroup.getForEdit({}).then((res) => {
            form.value = res.data!
        })
    }
}

const submit = () => {
    ruleFormRef.value?.validate(async (valid) => {
        if (valid) {
            await api.chatGroup.create({ body: form.value })
            dialogVisible.value = false
            emit('onSaved')
        }
    })
}

defineExpose({
    show,
})
</script>

<style scoped></style>
