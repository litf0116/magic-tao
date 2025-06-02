<template>
    <view class="p-4 bg-white">
        <uv-form ref="formRef" labelPosition="top" :model="form" :rules="rules">
            <uv-form-item :labelWidth="220" label="用户编号" prop="id" borderBottom>
                {{ form.id }}
            </uv-form-item>
            <uv-form-item :labelWidth="220" label="头像" prop="headImgUrl" borderBottom>
                <uv-upload
                    accept="image"
                    :fileList="fileList1"
                    :multiple="false"
                    :maxCount="1"
                    width="200rpx"
                    height="200rpx"
                    @after-read="afterRead"
                    @delete="deletePic"
                ></uv-upload>
            </uv-form-item>
            <uv-form-item :labelWidth="220" label="昵称" prop="name" borderBottom>
                <uv-input v-model="form.name" placeholder="请输入昵称"></uv-input>
            </uv-form-item>
            <template v-if="form.id">
                <uv-form-item
                    v-for="x in extItems"
                    :key="x.prop"
                    :labelWidth="220"
                    :label="x.label"
                    :prop="x.prop"
                    borderBottom
                >
                    <uv-input v-model="form[x.prop]" placeholder="请输入"></uv-input>
                </uv-form-item>
            </template>
            <uv-button type="primary" text="提交" customStyle="margin-top: 10px" @click="submit"></uv-button>
        </uv-form>
    </view>
</template>
<script setup lang="ts">
import api from '@/utils/api'
import { onLoad } from '@dcloudio/uni-app'
import { uploadImage } from '@/utils/upload'

const userStore = useUserStore()

let eventChannel: any = null
onLoad(() => {
    const pages = getCurrentPages()
    const page: any = pages[pages.length - 1]
    eventChannel = page.getOpenerEventChannel()
    fetchData()
})

const extItems = computed(() => {
    if (!userStore.user.id) {
        return []
    } else {
        return [
            {
                label: '扣.扣',
                prop: 'qq',
            },
            {
                label: 'w.x',
                prop: 'wx',
            },
        ]
    }
})

const rules = {
    name: [{ required: true, message: '昵称不能为空', trigger: ['blur', 'change'] }],
    qq: [{ required: true, message: '必填', trigger: ['blur', 'change'] }],
    // phoneNumber: [
    //     { required: true, message: '手机号不能为空', trigger: ['blur', 'change'] },
    //     {
    //         pattern: /^1[0-9]{10}$/g,
    //         // 正则检验前先将值转为字符串
    //         transform(value) {
    //             return String(value)
    //         },
    //         message: '必须为11位手机号码',
    //         trigger: ['blur', 'change'],
    //     },
    // ],
}
const formRef = ref<any>(null)
const form = ref({ id: 0, headImgUrl: '', name: '', qq: '', wx: '' })
const isSaving = ref(false)
const fileList1 = ref([] as any[])

function submit() {
    if (!fileList1.value.length) return Tips.info('请上传头像')

    form.value.headImgUrl = fileList1.value[0].url

    formRef.value
        .validate()
        .then(() => {
            debounce(realSave, 300)()
        })
        .catch(() => {
            Tips.info('请填写完整信息')
        })
}

function realSave() {
    isSaving.value = true
    api.user
        .update(form.value)
        .then(() => {
            Tips.noCancelModal('修改成功').then(() => {
                uni.redirectTo({
                    url: '/pages/index/my',
                })
            })
        })
        .finally(() => {
            isSaving.value = false
            userStore.checkLogin()
        })
}

function fetchData() {
    api.user.get({ id: userStore.user.id }).then((res: any) => {
        form.value = res
        if (res.headImgUrl) {
            fileList1.value = [
                {
                    url: res.headImgUrl,
                    status: 'success',
                    message: '',
                },
            ]
        }
    })
}

const afterRead = async (event: any) => {
    console.log('afterRead', event)
    isSaving.value = true
    let lists: any = [].concat(event.file)
    let fileListLen = fileList1.value.length
    lists.map((item: any) => {
        fileList1.value.push({
            ...item,
            status: 'uploading',
            message: '上传中',
        })
    })
    console.log('lists', lists)

    for (let i = 0; i < lists.length; i++) {
        await uploadImage(lists[i].url)
            .then((res: any) => {
                console.log('uploadImage', res)
                let item = fileList1.value[fileListLen]
                fileList1.value.splice(
                    fileListLen,
                    1,
                    Object.assign(item, {
                        status: 'success',
                        message: '',
                        url: res,
                    })
                )
                fileListLen++
            })
            .catch((err) => {
                console.error(err)
                Tips.noCancelModal(err)
                fileList1.value.splice(fileListLen, 1)
                fileListLen++
            })
    }
    isSaving.value = false
}

const deletePic = (event: any) => {
    fileList1.value.splice(event.index, 1)
}
</script>
