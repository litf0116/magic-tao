<template>
    <div>
        <el-table :data="tableData" style="width: 100%">
            <el-table-column label="名称" prop="name" />
            <el-table-column label="等级" prop="level" />
            <el-table-column label="所需金额" prop="amountRequired" />
            <el-table-column label="左边框颜色" prop="borderColor" />
            <el-table-column label="右边框颜色" prop="rightBorderColor" />
            <el-table-column label="操作" prop="statusStr" />
            <el-table-column align="right">
                <template #header>
                    <el-button size="small" @click="handleAdd">新增</el-button>
                </template>
                <template #default="scope">
                    <el-button size="small" @click="handleEdit(scope.row)">编辑</el-button>
                    <el-button size="small" type="danger" @click="handleDelete(scope.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <el-dialog v-model="dialogVisibleForm" :title="dialogTitle" width="600px" draggable destroy-on-close
            append-to-body :close-on-click-modal="false">
            <el-form ref="formRef" :model="formData" :rules="formRules" style="max-width: 800px" label-width="auto"
                class="demo-ruleForm" status-icon>
                <el-form-item v-for="field in formFields" :key="field.prop" :label="field.label" :prop="field.prop">
                    <el-input v-if="field.prop != 'borderColor' && field.prop != 'rightBorderColor'"
                        v-model.number="formData[field.prop]" :type="field.type || 'text'"
                        :placeholder="'请输入' + field.label">
                    </el-input>
                    <el-color-picker v-else v-model="formData[field.prop]" />
                </el-form-item>
                <div>
                    <el-button type="primary" @click="submitForm">确定</el-button>
                    <el-button @click="dialogVisibleForm = false">关闭</el-button>
                </div>
            </el-form>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { FormInstance, ElMessageBox, ElMessage } from 'element-plus'

import { GetList, AddGroupChatLevelSettings, EditGroupChatLevelSetting, DeleteGroupChatLevelSetting } from '@/api/groupChatLevel'

const tableData = ref([])
const formRef = ref<FormInstance>()
const dialogVisibleForm = ref(false)
const dialogTitle = ref('新增')

// 定义表单字段配置
const formFields = [
    { label: '名称', prop: 'name', type: 'text' },
    { label: '等级', prop: 'level', type: 'number' },
    { label: '所需金额', prop: 'amountRequired', type: 'number' },
    { label: '左边框颜色', prop: 'borderColor', type: 'text' },
    { label: '右边框颜色', prop: 'rightBorderColor', type: 'text' }
]

// 表单数据对象
const formData = reactive({
    id: 0,
    name: '',
    level: 0,
    amountRequired: 0,
    borderColor: ''
})

// 表单验证规则
const formRules = computed(() => {
    const rules = {}
    formFields.forEach(field => {
        rules[field.prop] = [
            { required: true, message: `请输入${field.label}`, trigger: 'blur' }
        ]
    })
    return rules
})

// 新增按钮处理
const handleAdd = () => {
    dialogTitle.value = '新增'
    resetForm()
    dialogVisibleForm.value = true
}

// 编辑按钮处理
const handleEdit = (row) => {
    dialogTitle.value = '编辑'
    Object.assign(formData, row)
    dialogVisibleForm.value = true
}

// 删除按钮处理
const handleDelete = async (row) => {
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(async () => {
        var res = await DeleteGroupChatLevelSetting(row.id)
        if (res.status === 200) {
            pageList()
        }
        ElMessage({ type: 'success', message: '删除成功!' })
    }).catch(() => {
        ElMessage({ type: 'info', message: '已取消删除' })
    })
}

// 重置表单
const resetForm = () => {
    if (formRef.value) {
        formRef.value.resetFields()
    }
    Object.keys(formData).forEach(key => {
        formData[key] = ''
    })
}

// 提交表单
const submitForm = () => {
    if (!formRef.value) return
    formRef.value.validate(async (valid) => {
        if (valid) {
            // 实现提交逻辑
            console.log('表单数据：', formData)
            formData.id = formData.id == 0 ? 0 : parseInt(formData.id.toString())
            var res;
            if (formData.id == 0) {
                res = await AddGroupChatLevelSettings(formData)
            } else {
                res = await EditGroupChatLevelSetting(formData)
            }
            if (res.status === 200) {
                // 成功处理
                dialogVisibleForm.value = false
                pageList()
            }
        }
    })
}

// 获取列表数据
const pageList = async () => {
    try {
        const res = await GetList()
        if (res.status === 200) {
            tableData.value = res.data
        }
    } catch (error) {
        console.error('获取列表失败：', error)
    }
}

// 组件挂载时获取数据
onMounted(() => {
    pageList()
})
</script>