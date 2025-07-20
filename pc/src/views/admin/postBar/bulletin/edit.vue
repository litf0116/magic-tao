<template>
    <el-dialog
        v-model="isShow"
        :title="title"
        :width="width"
        :close-on-click-modal="false"
        append-to-body
        destroy-on-close
    >
        <el-form ref="formRef" :rules="formRules" :model="form" label-position="top">
            <el-form-item label="标题" prop="title">
                <el-input v-model="form.title" />
            </el-form-item>
            <el-form-item label="内容" prop="content">
                <!-- <el-input v-model="form.content" type="textarea" /> -->
                <div style="border: 1px solid #eeeff0; border-radius: 5px; overflow: hidden; width: 100%">
                    <Toolbar
                        style="border-bottom: 1px solid #eeeff0"
                        :editor="editorRef"
                        :default-config="toolbarConfig"
                        :mode="mode"
                    />
                    <Editor
                        v-model="form.content"
                        style="height: 300px; overflow-y: hidden"
                        :default-config="editorConfig"
                        :mode="mode"
                        @on-change="handleChange"
                        @onCreated="handleCreated"
                    />
                </div>
            </el-form-item>
        </el-form>
        <template #footer>
            <el-button type="default" @click="isShow = false">取消</el-button>
            <el-button type="primary" @click="handleSave">确定</el-button>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import { Editor, Toolbar } from '@wangeditor/editor-for-vue'
import { IToolbarConfig } from '@wangeditor/editor'
import '@wangeditor/editor/dist/css/style.css'
import { shallowRef } from 'vue'
import { Add, Edit } from '@/api/PostBulletinAPI'

const toolbarConfig: Partial<IToolbarConfig> = {
    toolbarKeys: [
        'headerSelect',
        'bold',
        'italic',
        'underline',
        'through',
        'bulletedList',
        'justifyLeft',
        'justifyCenter',
        'justifyRight',
        'undo',
        'redo',
        'insertLink',
    ],
}
const editorConfig = {
    // 添加以下配置
    onblur: function (editor: any) {
        return false // 返回 false 阻止默认失焦行为
    },
}

const editorRef = shallowRef()
const mode = 'default'
const handleCreated = (editor: any) => {
    editorRef.value = editor
}
//富文本框值更改
const handleChange = (editor: any) => {
    form.value.content = editor.isEmpty() ? '' : editor.getHtml()
    // 手动触发表单验证
    if (formRef.value) {
        formRef.value.validateField('content')
    }
}

defineProps({
    width: {
        type: String,
        default: '60%',
    },
})
const emit = defineEmits(['change'])

const form = ref({
    id: 0,
    content: '',
    title: '',
})
// 表单验证规则
const formRules = computed(() => {
    const rules = {
        title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
        content: [{ required: true, message: '请输入内容', trigger: 'blur' }],
    }
    return rules
})
const formRef = ref(null)
const isShow = ref(false)

const title = computed(() => {
    return form.value.id != 0 ? '编辑' : '新增'
})

const show = (dto) => {
    isShow.value = true
    form.value = {
        id: 0,
        content: '',
        title: '',
    }
    if (dto != null) {
        form.value = dto
    }
}

const handleSave = () => {
    if (!formRef.value) return
    formRef.value.validate(async (valid) => {
        if (valid) {
            let _api
            if (form.value.id && form.value.id != 0) {
                _api = Edit
            } else {
                _api = Add
            }
            _api(form.value)
                .then(() => {
                    isShow.value = false
                    emit('change', form.value)
                    Tips.success('成功')
                })
                .catch((err: any) => {
                    Tips.error('服务器异常！')
                })
        }
    })
}

defineExpose({ show })
</script>
