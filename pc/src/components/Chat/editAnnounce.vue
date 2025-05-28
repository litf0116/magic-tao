<template>
    <el-dialog
        v-model="dialogVisible"
        width="600px"
        draggable
        destroy-on-close
        append-to-body
        :close-on-click-modal="false"
    >
        <el-form
            ref="ruleFormRef"
            style="max-width: 600px"
            :model="form"
            :rules="rules"
            label-width="auto"
            class="demo-ruleForm"
            status-icon
        >
            <el-form-item label="版块" prop="categoryId">
                <el-select v-model="form.categoryId" placeholder="请选择">
                    <el-option label="勇者招募" :value="1" />
                    <el-option label="拍卖行" :value="2" />
                </el-select>
            </el-form-item>
            <el-form-item label="图片" prop="imageUrl">
                <tt-upload
                    v-model="form.imageUrl"
                    css-class="avatar-uploader"
                    :file-size="2048"
                    :multiple="false"
                    @on-uploaded="handleUploaded"
                >
                    <img v-if="form.imageUrl" :src="form.imageUrl" class="size-24" />
                    <div v-else class="border-2 border-dashed border-blue-300">
                        <div class="i-mdi:add text-gray-200 size-24"></div>
                    </div>
                </tt-upload>
            </el-form-item>
            <el-form-item label="内容" prop="content">
                <el-input v-model="form.content" type="textarea" :rows="12" />
            </el-form-item>
            <el-form-item label="排序[数字越大越前]" prop="sort">
                <el-input v-model="form.sort" type="number" />
            </el-form-item>
            <div>
                <el-button type="primary" @click="submitForm"> 更新 </el-button>
                <el-button @click="dialogVisible = false">关闭</el-button>
            </div>
        </el-form>
    </el-dialog>
</template>

<script setup lang="ts">
import TtUpload from '@/components/tt-upload/index.vue'
import type { FormInstance, FormRules } from 'element-plus'
import { AnnounceCreateOrUpdateDto } from '@/api/appService'
import api from '@/api'

import cache from '@/utils/cache'
import base64 from '@/utils/base64'

const signature = ref('')
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME
const policy = ref('')
const actionUrl = computed(() => `https://v0.api.upyun.com/${bucketName}`)

const ruleFormRef = ref<FormInstance>()
const form = ref<AnnounceCreateOrUpdateDto>({
    imageUrl: '',
    content: '',
})

const emit = defineEmits(['onSaved', 'onEdit'])

const rules = reactive<FormRules<AnnounceCreateOrUpdateDto>>({
    // imageUrl: [{ required: true, message: '必填', trigger: ['change', 'blur'] }],
    content: [{ required: true, message: '必填', trigger: ['change', 'blur'] }],
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
    let _api
    if (form.value.id) _api = api.announce.update
    else _api = api.announce.create

    _api({ body: form.value }).then((res) => {
        Tips.success('成功')
        emit('onSaved')
        // userStore.getUserInfo()
        dialogVisible.value = false
    })
}

function handleUploaded(e: { url: string }) {
    form.value = { ...form.value, imageUrl: `${e.url}` }
}

const dialogVisible = ref(false)
const show = (e: boolean, id: number, categoryId: number) => {
    dialogVisible.value = e
    if (e) {
        api.announce.getForEdit({ id: id }).then((res) => {
            form.value = res.data!
            form.value.categoryId = categoryId
        })
    }
}
defineExpose({
    show,
})

onMounted(() => {
    getAuth()
})

const getAuth = async () => {
    const cachedata = cache.getWithExpiry('upyun')
    if (cachedata && cachedata.policy && cachedata.signature) {
        signature.value = cachedata.signature
        policy.value = cachedata.policy
    } else {
        // @ts-ignore
        const date = new Date().toGMTString()
        const opts = {
            'save-key': `/{year}{mon}{day}/{random32}{.suffix}`,
            bucket: bucketName,
            expiration: Math.round(new Date().getTime() / 1000) + 43200, //12hour
            date: date,
        }
        policy.value = base64.encode(JSON.stringify(opts))
        const data = ['POST', '/' + bucketName, date, policy.value].join('&')
        await api.upload.getSignature({ data: data }).then((res) => {
            signature.value = res.signature
            cache.setWithExpiry('upyun', { signature: signature.value, policy: policy.value }, 600)
            // emit("onKeyReady", { url: actionUrl, key: authorization.value, policy: policy });
        })
    }
}
</script>
<style scoped>
.avatar-uploader .avatar {
    width: 96px;
    height: 96px;
    display: block;
}
</style>
