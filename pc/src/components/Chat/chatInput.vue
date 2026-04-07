<template>
    <el-input
        :model-value="modelValue"
        type="textarea"
        :max="700"
        :rows="3"
        @update:model-value="$emit('update:modelValue', $event)"
        @keyup.enter.prevent="emit('onPressEnter', $event)"
        @focus="focus"
        @paste.capture="pasting"
    >
    </el-input>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElInput } from 'element-plus'
import cache from '@/utils/cache'
import base64 from '@/utils/base64'
import api from '@/api'
import axios from 'axios'

const props = defineProps<{
    modelValue?: string
}>()

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void
    (e: 'focus'): void
    (e: 'fileUploaded', data: { url: string; data: any }): void
    (e: 'onPressEnter', event: KeyboardEvent): void
}>()

const signature = ref('')
const imgUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME
const policy = ref('')

const actionUrl = computed(() => `https://v0.api.upyun.com/${bucketName}`)

function focus() {
    emit('focus')
}

// 监听粘贴事件
async function pasting(event: ClipboardEvent) {
    if (!event.clipboardData) return
    const items = event.clipboardData.items
    if (items.length) {
        let file = null
        for (let i = 0; i < items.length; i++) {
            if (items[i].type.indexOf('image') !== -1) {
                file = items[i].getAsFile()
                handleChange(file) //上传
                break
            }
        }
    }
}

function handleChange(file: any) {
    console.log(file)
    let formData = new FormData()
    formData.append('authorization', `UPYUN ${userName}:${signature.value}`)
    formData.append('policy', policy.value)
    formData.append('file', file.raw || file)
    axios
        .post(actionUrl.value, formData)
        .then((res) => {
            console.log('upload result', res)
            emit('fileUploaded', { url: `${imgUrl}${res.data.url}`, data: res.data })
        })
        .catch((err) => {
            console.log(err)
            Tips.error('上传失败')
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
