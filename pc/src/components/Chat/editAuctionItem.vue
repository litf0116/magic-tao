<template>
    <el-dialog
        v-model="dialogVisible"
        title="拍卖物品编辑"
        width="800px"
        draggable
        destroy-on-close
        append-to-body
        :close-on-click-modal="false"
    >
        <el-form
            ref="ruleFormRef"
            :model="form"
            :rules="rules"
            style="max-width: 800px"
            label-width="auto"
            class="demo-ruleForm"
            status-icon
        >
            <div class="grid grid-cols-4 gap-2 items-center">
                <el-form-item label="名称" prop="name" class="col-span-2">
                    <el-input v-model="form.name" />
                </el-form-item>
                <el-form-item label="图片" prop="imageUrl">
                    <tt-upload
                        v-model="form.imageUrl"
                        css-class="avatar-uploader"
                        :file-size="2048"
                        :multiple="false"
                        @on-uploaded="handleUploaded"
                    >
                        <img v-if="form.imageUrl" :src="form.imageUrl" class="size-12" />
                        <div v-else class="border-2 border-dashed border-blue-300">
                            <div class="i-mdi:add text-gray-200 size-12"></div>
                        </div>
                    </tt-upload>
                </el-form-item>
                <el-form-item label="是否直接上架">
                    <el-switch
                        v-model="form.status"
                        class="ml-2"
                        style="--el-switch-on-color: #13ce66; --el-switch-off-color: #ff4949"
                        :active-value="1"
                        :inactive-value="0"
                    />
                </el-form-item>
            </div>
            <div class="grid grid-cols-2 gap-2 items-center">
                <el-form-item label="卖家信息" prop="sellerInfo">
                    <el-input v-model="form.sellerInfo" type="textarea" />
                </el-form-item>
                <el-form-item label="排序[正序]" prop="order">
                    <el-input v-model="form.order" type="number" />
                </el-form-item>
            </div>
            <el-form-item label="介绍" prop="description">
                <!-- {{ form.description }} -->
                <div
                    ref="contentTarget"
                    class="min-h-64 w-full border-2 border-solid border-blue-500"
                    contenteditable
                    @input="onInput"
                    @paste="onPaste"
                ></div>
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
import { AuctionItemCreateOrUpdateDto } from '@/api/appService'
import api from '@/api'

import cache from '@/utils/cache'
import base64 from '@/utils/base64'
import axios from 'axios'
import DOMPURIFY from 'dompurify'
const signature = ref('')
const imgUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME
const policy = ref('')
const actionUrl = computed(() => `https://v0.api.upyun.com/${bucketName}`)

const userStore = useUserStore()
const ruleFormRef = ref<FormInstance>()
const contentTarget = ref<HTMLElement | null>(null)
const form = ref<AuctionItemCreateOrUpdateDto>({
    name: '',
})

const emit = defineEmits(['onSaved', 'onEdit'])

const rules = reactive<FormRules<AuctionItemCreateOrUpdateDto>>({
    name: [{ required: true, message: '请输入', trigger: ['change', 'blur'] }],
    imageUrl: [{ required: true, message: '请上传图片', trigger: ['change', 'blur'] }],
    sellerInfo: [{ required: true, message: '必填', trigger: ['change', 'blur'] }],
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
    // 调试：检查提交时的description字段
    console.log('提交时的form数据:', form.value)
    console.log('提交时的description字段:', form.value.description)

    let _api
    if (form.value.id) _api = api.auctionItem.update
    else _api = api.auctionItem.create

    _api({ body: form.value })
        .then((res) => {
            console.log('保存成功，返回数据:', res)
            Tips.success('成功')
            emit('onSaved')
            // userStore.getUserInfo()
            dialogVisible.value = false
        })
        .catch((error) => {
            console.error('保存失败:', error)
            Tips.error('保存失败')
        })
}

function handleUploaded(e: { url: string }) {
    form.value = { ...form.value, imageUrl: `${e.url}!w300` }
}

const dialogVisible = ref(false)
const show = (e: boolean, id: number) => {
    dialogVisible.value = e
    if (e) {
        api.auctionItem.getForEdit({ id: id }).then((res) => {
            form.value = res.data!
            // 修复：正确处理description字段，避免null或undefined，使用 DOMPurify 防止 XSS
            const description = res.data!.description || ''
            const sanitizedHtml = DOMPURIFY.sanitize(description, { USE_PROFILES: { html: true } })
            contentTarget.value!.innerHTML = sanitizedHtml
            console.log('获取到的description字段:', description)
        })
    }
}
defineExpose({
    show,
})

function onInput(e) {
    console.log('onInput事件:', e.target.innerHTML)
    // 修复：确保description字段正确保存
    form.value.description = e.target.innerHTML || ''
    console.log('保存的description字段:', form.value.description)
}

async function onPaste(e: ClipboardEvent) {
    // console.log(e)
    e.preventDefault()
    e.stopPropagation()
    if (!e.clipboardData) return

    const paste = (e.clipboardData || window.clipboardData).getData('text/plain')
    if (paste) {
        // 新建元素标签
        var newNode = document.createElement('span')
        newNode.innerHTML = paste
        // 获取当前光标位置，插入元素
        window.getSelection().getRangeAt(0).insertNode(newNode)
        form.value.description = contentTarget.value!.innerHTML
    }

    const items = e.clipboardData.items
    if (items.length) {
        console.log(items)

        let file = null
        for (let i = 0; i < items.length; i++) {
            if (items[i].type.indexOf('image') !== -1) {
                file = items[i].getAsFile()
                await handleChange(file).then(() => {
                    form.value.description = contentTarget.value!.innerHTML
                }) //上传
            }
        }
    }

    // let cbPayload = [...(e.clipboardData || e.originalEvent.clipboardData).items] // Capture the ClipboardEvent's eventData payload as an array
    // cbPayload = cbPayload.filter((i) => /image/.test(i.type)) // Strip out the non-image bits
    // if (!cbPayload.length || cbPayload.length === 0) return false // If no image was present in the collection, bail.

    // let reader = new FileReader() // Instantiate a FileReader...
    // reader.onload = (e) => {
    //     console.log(e)(
    //         (contentTarget.value!.innerHTML = `<img style="max-width:200px;max-height:200px;" src="${e.target.result}">`)
    //     ) // ... set its onLoad to render the event target's payload
    // }
    // reader.readAsDataURL(cbPayload[0].getAsFile()) // ... then read in the pasteboard image data as Base64
}

function sleep(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms))
}

function handleChange(file: any) {
    console.log(file)
    return new Promise<void>((resolve) => {
        let formData = new FormData()
        formData.append('authorization', `UPYUN ${userName}:${signature.value}`)
        formData.append('policy', policy.value)
        formData.append('file', file.raw || file)
        axios
            .post(actionUrl.value, formData)
            .then(async (res) => {
                console.log('upload result', res)
                const url = `${imgUrl}${res.data.url}`
                if (!form.value.imageUrl) {
                    form.value.imageUrl = url
                }
                var newNode = document.createElement('img')
                newNode.dataset.url = url
                newNode.style.maxWidth = '200px'
                newNode.style.maxHeight = '200px'
                newNode.src = url + '!w300'
                // newNode.innerHTML = paste
                window.getSelection().getRangeAt(0).insertNode(newNode)
                return resolve()
            })
            .catch((err) => {
                console.log(err)
                Tips.error('上传失败')
            })
    })
}

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
