<template>
    <el-dialog
        v-model="dialogVisible"
        title="消息配置"
        width="600px"
        draggable
        destroy-on-close
        append-to-body
        :close-on-click-modal="false"
    >
        <el-form
            ref="formRef"
            :model="formFields"
            :rules="formRules"
            style="max-width: 800px"
            label-width="auto"
            class="demo-ruleForm"
            status-icon
        >
            <template v-for="(field, index) in formFields" :key="index">
                <el-form-item :label="field.label" :prop="`${index}.msg`">
                    <el-input v-model="field.msg" type="text"></el-input>
                </el-form-item>
            </template>
            <div>
                <el-button type="primary" @click="submitForm"> 更新 </el-button>
                <el-button @click="dialogVisible = false">关闭</el-button>
            </div>
        </el-form>
    </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import type { FormInstance } from 'element-plus'
import { GetList, Add } from '@/api/msgConfiguration'
import { Tips } from '@/composables'

const emit = defineEmits(['onSaved', 'onEdit'])
const formRef = ref<FormInstance>()
//类型 1、新用户出价提示 2、提现提示
const formFields = reactive([
    {
        id: 0,
        type: 1,
        msg: '',
        label: '新用户出价诚信履约金余额不足提示',
        prop: 'msg',
    },
])
// 定义验证规则
const formRules = computed(() => {
    const fieldRules = {}
    formFields.forEach((field, index) => {
        fieldRules[`${index}.msg`] = [{ required: true, message: `请输入${field.label}`, trigger: 'blur' }]
    })
    return fieldRules
})
onMounted(() => undefined)
//提交数据
const submitForm = () => {
    if (!formRef.value) return
    formRef.value.validate(async (valid) => {
        if (valid) {
            await Add(formFields)
            Tips.success('添加成功')
            emit('onSaved')
            dialogVisible.value = false
        }
    })
}

const dialogVisible = ref(false)
const show = async (e: boolean, id: number) => {
    dialogVisible.value = e
    if (e) {
        var res: any = await GetList()
        if (res && Array.isArray(res)) {
            res.forEach((item, index) => {
                formFields[index].id = item.id
                formFields[index].msg = item.msg
            })
        }
    }
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
