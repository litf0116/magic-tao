<template>
    <el-upload
        :class="cssClass"
        :multiple="false"
        :drag="drag"
        :action="actionUrl"
        :file-list="fileList"
        :show-file-list="showFileList"
        :data="uploadData"
        :on-success="handleSuccess"
        :before-upload="beforeUpload"
        :list-type="listType"
        :on-remove="handleRemove"
        :on-preview="handlePictureCardPreview"
    >
        <slot />
        <template #file="{ file }">
            <slot name="file" :file="file" />
        </template>
    </el-upload>
    <el-dialog v-model="dialogVisible">
        <img w-full :src="dialogImageUrl" alt="Preview Image" />
    </el-dialog>
</template>
<script setup lang="ts">
import { IUploadObject } from '@/types'
import cache from '@/utils/cache'
import base64 from '@/utils/base64'
import api from '@/api'
const props = defineProps({
    modelValue: {
        type: [String, Array],
        default: '',
    },
    cssClass: {
        type: String,
        default: '',
    },
    multiple: {
        type: Boolean,
        default: false,
    },
    drag: {
        type: Boolean,
        default: false,
    },
    fileSize: {
        type: Number,
        default: -1,
    },
    showFileList: {
        type: Boolean,
        default: false,
    },
    listType: {
        type: String,
        default: 'text',
    },
})
const emit = defineEmits(['update:modelValue', 'onUploaded', 'change', 'input'])

const fileList = computed(() => {
    if (!props.modelValue) return []
    if (Array.isArray(props.modelValue))
        return (props.modelValue as string[]).map((item, index) => {
            return {
                name: index,
                url: item,
            }
        })
    return [{ name: '1', url: props.modelValue as string }]
})

const listObj = ref<{ [key: string]: IUploadObject }>({})

const signature = ref('')
const imgUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME
const policy = ref('')
const uploadData = ref<{ authorization: string; file: string; policy: string }>({
    authorization: '',
    file: '',
    policy: '',
})

onMounted(() => {
    // console.log('onMounted')
    getAuth()
})

const actionUrl = `https://v0.api.upyun.com/${bucketName}`

const handleSuccess = (res: any, file: any) => {
    console.log('handleSuccess', res, file)
    if (res.message === 'ok') {
        const fileName = `${imgUrl}${res.url}`
        emit('onUploaded', { url: fileName, file: file })
        emit('input', fileName)
        //  console.log("response", res);
        //  console.log("file", file);
        const uid = file.uid
        const objKeyArr = Object.keys(listObj.value)
        for (let i = 0, len = objKeyArr.length; i < len; i++) {
            if (listObj.value[objKeyArr[i]].uid === uid) {
                listObj.value[objKeyArr[i]].url = fileName
                listObj.value[objKeyArr[i]].hasSuccess = true
                return
            }
        }
    }
}

const getAuth = async () => {
    const cachedata = cache.getWithExpiry('upyun')
    if (cachedata && cachedata.policy && cachedata.signature) {
        signature.value = cachedata.signature
        policy.value = cachedata.policy
    } else {
        // @ts-ignore
        const date = new Date().toGMTString()
        const opts = {
            'save-key': `/${userName}/{year}-{mon}-{day}/upload_{random32}{.suffix}`,
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

const beforeUpload = async (file: any) => {
    // 验证文件类型 - MIME 类型
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
    const isAllowedType = allowedTypes.includes(file.type)

    // 验证文件扩展名
    const fileName = file.name.toLowerCase()
    const fileExtension = fileName.split('.').pop()
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp']
    const hasAllowedExtension = fileExtension && allowedExtensions.includes(fileExtension)

    if (!isAllowedType || !hasAllowedExtension) {
        Tips.error('只支持 JPG、PNG、GIF、WEBP 格式的图片!')
        return false
    }

    if (props.fileSize > 0) {
        if (file.size / 1024 > props.fileSize) {
            Tips.error(`图片大小不能超过 ${props.fileSize}KB!`)
            return false
        }
    }
    const fileUid = file.uid
    const img = new Image()
    img.src = window.URL.createObjectURL(file)
    img.onload = () => {
        listObj.value[fileUid] = {
            hasSuccess: false,
            uid: file.uid,
            url: '',
            width: img.width,
            height: img.height,
        }
    }

    if (!signature.value || !policy.value) {
        getAuth()
    }

    uploadData.value = {
        file: file.name,
        authorization: `UPYUN ${userName}:${signature.value}`,
        policy: policy.value,
    }
}
const authorization = computed(() => {
    return `UPYUN ${userName}:${signature.value}`
})

defineExpose({ actionUrl, authorization, policy })

const handleRemove = (uploadFile: any, uploadFiles: any) => {
    console.log(uploadFile, uploadFiles)
    emit(
        'update:modelValue',
        uploadFiles.map((item: any) => item.url)
    )
}
const dialogImageUrl = ref('')
const dialogVisible = ref(false)
const handlePictureCardPreview = (uploadFile: any) => {
    dialogImageUrl.value = uploadFile.url!
    dialogVisible.value = true
}
</script>
<style></style>
