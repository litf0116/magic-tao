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
                    :maxSize="5 * 1024 * 1024"
                    width="200rpx"
                    height="200rpx"
                    :previewFullImage="true"
                    :deletable="true"
                    :disabled="isSaving"
                    uploadIcon="camera-fill"
                    uploadIconColor="#909399"
                    uploadText="点击上传"
                    @after-read="afterRead"
                    @delete="deletePic"
                    @oversize="onOversize"
                    @error="onUploadError"
                ></uv-upload>
                <view class="text-xs text-gray-500 mt-2"> 支持JPG、PNG、GIF、WEBP格式，建议尺寸200x200像素 </view>
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
            <!-- 提交按钮：上传中或正在保存时禁用 -->
            <uv-button
                type="primary"
                text="提交"
                customStyle="margin-top: 10px"
                :disabled="isSaving || isUploading"
                @click="submit"
            ></uv-button>
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

// 判断是否有文件正在上传
const isUploading = computed(() => {
    return fileList1.value.some((file: any) => file.status === 'uploading')
})

function submit() {
    // 智能头像处理：新用户需要上传头像，老用户可以保留或更换头像
    if (!form.value.headImgUrl && !fileList1.value.length) {
        return Tips.info('请上传头像')
    }

    // 如果上传了新头像，验证上传状态和URL格式
    if (fileList1.value.length > 0) {
        const fileItem = fileList1.value[0]

        // 检查上传状态
        if (fileItem.status !== 'success') {
            Tips.info('图片正在上传中，请稍候...')
            return
        }

        // 检查URL格式，必须是CDN地址
        if (!fileItem.url || !fileItem.url.startsWith('http://image.molitao.top')) {
            Tips.info('头像上传未完成，请重新上传')
            return
        }

        form.value.headImgUrl = fileItem.url
    }
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
    api.user
        .get({ id: userStore.user.id })
        .then((res: any) => {
            form.value = res
            // 确保现有头像正确加载到fileList中，支持用户更换头像
            if (res.headImgUrl) {
                fileList1.value = [
                    {
                        url: res.headImgUrl,
                        status: 'success',
                        message: '',
                        deletable: true, // 允许删除现有头像
                    },
                ]
            } else {
                // 如果没有头像，清空fileList，允许用户上传新头像
                fileList1.value = []
            }
        })
        .catch((err) => {
            Tips.error('获取用户信息失败，请重试')
        })
}

const afterRead = async (event: any) => {
    // 防止重复上传
    if (isSaving.value) {
        Tips.info('正在上传中，请稍候...')
        return
    }

    isSaving.value = true
    let lists: any = [].concat(event.file)

    // 如果用户已有头像，先清空再添加新头像
    if (fileList1.value.length > 0) {
        fileList1.value = []
    }
    // 重置索引，确保从 0 开始
    let fileListLen = 0

    lists.map((item: any) => {
        fileList1.value.push({
            ...item,
            status: 'uploading',
            message: '上传中',
        })
    })

    for (let i = 0; i < lists.length; i++) {
        try {
            const uploadResult = await uploadImage(lists[i].url)

            // 图片上传成功后，调用审核接口
            try {
                const auditResult = await api.imageAudit.check({ url: uploadResult })

                // 审核不通过，删除图片并提示
                if (!auditResult.pass) {
                    fileList1.value.splice(fileListLen, 1)
                    Tips.error(auditResult.message || '图片内容不合规，请更换头像')
                    fileListLen++
                    continue
                }
            } catch (auditErr: any) {
                // 审核接口调用失败时，跳过审核直接使用图片
                console.warn('审核接口调用失败，跳过审核:', auditErr)
                // 不 return，继续执行
            }

            console.log('设置上传状态为 success, fileListLen:', fileListLen, 'uploadResult:', uploadResult)
            let item = fileList1.value[fileListLen]
            // 确保索引有效
            if (!item) {
                console.error('item is undefined, fileListLen:', fileListLen, 'length:', fileList1.value.length)
                Tips.error('头像上传失败，请重试')
                continue
            }
            fileList1.value.splice(
                fileListLen,
                1,
                Object.assign(item, {
                    status: 'success',
                    message: '上传成功',
                    url: uploadResult,
                    deletable: true,
                })
            )
            fileListLen++
            Tips.success('头像上传成功')
        } catch (err: any) {
            Tips.error(`头像上传失败：${err}`)
            fileList1.value.splice(fileListLen, 1)
            fileListLen++
        }
    }
    isSaving.value = false
}

const deletePic = (event: any) => {
    uni.showModal({
        title: '提示',
        content: '确定要删除当前头像吗？',
        success: (res) => {
            if (res.confirm) {
                fileList1.value.splice(event.index, 1)
                // 同时清空表单中的头像地址，让用户可以重新上传
                form.value.headImgUrl = ''
                Tips.success('头像已删除，请重新上传新头像')
            }
        },
    })
}

// 处理文件大小超限
const onOversize = (file: any) => {
    Tips.error('图片大小不能超过5MB，请压缩后重新上传')
}

// 处理上传错误
const onUploadError = (error: any) => {
    Tips.error(`上传失败：${error.errMsg || '未知错误'}`)
}
</script>

<style lang="scss" scoped>
/* 自定义头像删除按钮样式，让删除按钮更大更容易点击 */
:deep(.uv-upload__deletable) {
    width: 24px !important;
    height: 24px !important;
    background-color: rgba(245, 63, 63, 0.9) !important;
    border-bottom-left-radius: 12px !important;
    top: -2px !important;
    right: -2px !important;

    /* 增加触摸区域 */
    &::before {
        content: '';
        position: absolute;
        top: -8px;
        left: -8px;
        right: -8px;
        bottom: -8px;
        z-index: -1;
    }

    .uv-upload__deletable__icon {
        transform: scale(1) !important;

        /* 调整图标位置 */
        :deep(.uv-icon) {
            font-size: 14px !important;
        }
    }

    /* 悬停效果 */
    &:active {
        background-color: rgba(220, 38, 38, 1) !important;
        transform: scale(0.95);
    }
}

/* 确保删除按钮在最上层 */
:deep(.uv-upload__wrap__preview) {
    .uv-upload__deletable {
        z-index: 10 !important;
    }
}

/* 优化头像预览区域的视觉效果 */
:deep(.uv-upload__wrap__preview) {
    border-radius: 8px;
    overflow: hidden;

    .uv-upload__wrap__preview__image {
        border-radius: 8px;
    }
}
</style>
