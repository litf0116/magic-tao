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
            <el-form-item label="版块" prop="categoryId" :error="formErrors['categoryId']">
                <el-select v-model="form.categoryId" placeholder="请选择">
                    <el-option label="勇者招募" :value="1" />
                    <el-option label="拍卖行" :value="2" />
                </el-select>
            </el-form-item>
            <el-form-item label="图片" prop="titleImageUrl">
                <tt-upload
                    v-model="form.imageUrl"
                    css-class="avatar-uploader"
                    :file-size="2048"
                    @onUploaded="handleUploaded"
                >
                    <img v-if="form.imageUrl" :src="form.imageUrl" class="size-32" />
                    <el-icon v-else class="avatar-uploader-icon"><Plus /></el-icon>
                </tt-upload>
            </el-form-item>
            <el-form-item label="内容" prop="content" :error="formErrors['content']">
                <el-input v-model="form.content" type="textarea" rows="8" />
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
import { AnnounceCreateOrUpdateDto } from '@/api/appService'
import { ElMessage } from 'element-plus'

defineProps({
    width: {
        type: String,
        default: '60%',
    },
})
const emit = defineEmits(['change'])

const form = ref({} as AnnounceCreateOrUpdateDto)
const formError = ref('')
const formErrors = ref({} as any)
const formRef = ref(null)
const isShow = ref(false)

const title = computed(() => {
    return form.value.id ? '编辑' : '新增'
})

const show = (dto: AnnounceCreateOrUpdateDto) => {
    formErrors.value = {}
    isShow.value = true
    form.value = dto
}

const handleSave = () => {
    let _api
    if (form.value.id && form.value.id != 0) {
        _api = api.announce.update
    } else {
        _api = api.announce.create
    }
    _api({ body: Object.assign({}, form.value) })
        .then(() => {
            ElMessage.success({ message: '成功' })
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

function handleUploaded(e: any) {
    form.value.titleImageUrl = e.url
}

defineExpose({ show })
</script>
