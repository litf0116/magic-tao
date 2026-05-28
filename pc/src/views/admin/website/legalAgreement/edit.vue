<template>
    <el-dialog
        v-model="isShow"
        :title="title"
        :width="width"
        :close-on-click-modal="false"
        append-to-body
        destroy-on-close
    >
        <el-form ref="formRef" :model="form" label-position="top">
            <el-form-item label="标题" prop="title" :error="formErrors['title']">
                <el-input v-model="form.title" />
            </el-form-item>
            <el-form-item label="内容" prop="content">
                <el-input v-model="form.content" type="textarea" :rows="20" />
            </el-form-item>
            <el-form-item label="是否发布" prop="status">
                <el-switch
                    v-model="form.status"
                    active-color="#13ce66"
                    inactive-color="#ff4949"
                    :active-value="'已发布'"
                    :inactive-value="'草稿'"
                />
            </el-form-item>
            <el-form-item label="排序[数字越大越前]" prop="sort">
                <el-input v-model="form.sort" type="number" />
            </el-form-item>
        </el-form>
        <template #footer>
            <el-button type="default" @click="isShow = false">取消</el-button>
            <el-button type="primary" @click="handleSave">确定</el-button>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import api from '@/api'
import { CmsArticleCreateOrUpdateDto } from '@/api/appService'
import { ElMessage } from 'element-plus'

const props = defineProps({
    pid: {
        type: Number,
        default: 2,
    },
    width: {
        type: String,
        default: '80%',
    },
})
const emit = defineEmits(['change'])

const form = ref({} as CmsArticleCreateOrUpdateDto)
const formError = ref('')
const formErrors = ref({} as any)
const formRef = ref(null)
const isShow = ref(false)

const title = computed(() => {
    return form.value.id ? '编辑' : '新增'
})

const show = (dto: CmsArticleCreateOrUpdateDto) => {
    formErrors.value = {}
    isShow.value = true
    // 确保 categoryId 固定为法律协议分类
    form.value = {
        ...dto,
        categoryId: props.pid,
    } as CmsArticleCreateOrUpdateDto
}

const handleSave = () => {
    let _api
    if (form.value.id && form.value.id != 0) {
        _api = api.cmsArticle.update
    } else {
        _api = api.cmsArticle.create
    }
    _api({ body: Object.assign({}, form.value) })
        .then(() => {
            ElMessage.success({ message: '保存成功' })
            isShow.value = false
            emit('change', form.value)
            clearErrors()
        })
        .catch((err: any) => {
            formError.value = err.details
            getErrors(err.validationErrors)
        })
}

function getErrors(errors: ValidationError[]) {
    formErrors.value = {}
    errors.forEach((x) => {
        if (x.members && x.members.length) {
            x.members.forEach((y) => {
                formErrors.value = { ...formErrors.value, [y]: x.message }
            })
        }
    })
}

function clearErrors() {
    formError.value = ''
    formErrors.value = {}
}

defineExpose({ show })
</script>
