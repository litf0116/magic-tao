<template>
    <div>
        <el-card>
            <template #header>
                <div class="flex justify-between items-center">
                    <span>应用版本管理</span>
                    <el-button type="primary" @click="showUploadDialog">发布新版本</el-button>
                </div>
            </template>

            <el-tabs v-model="activePlatform" @tab-change="loadData">
                <el-tab-pane label="Android" name="android" />
                <el-tab-pane label="iOS" name="ios" />
            </el-tabs>

            <el-table :data="list" v-loading="loading">
                <el-table-column type="index" width="50" align="center" />
                <el-table-column label="版本号" prop="versionName" width="120" />
                <el-table-column label="版本码" prop="versionCode" width="80" />
                <el-table-column label="平台" prop="platform" width="80" />
                <el-table-column label="强制更新" prop="isForceUpdate" width="100">
                    <template #default="{ row }">
                        <el-tag :type="row.isForceUpdate ? 'danger' : 'success'">
                            {{ row.isForceUpdate ? '是' : '否' }}
                        </el-tag>
                    </template>
                </el-table-column>
                <el-table-column label="激活状态" prop="isActive" width="100">
                    <template #default="{ row }">
                        <el-tag :type="row.isActive ? 'success' : 'info'">
                            {{ row.isActive ? '当前版本' : '历史版本' }}
                        </el-tag>
                    </template>
                </el-table-column>
                <el-table-column label="文件大小" prop="fileSize" width="100">
                    <template #default="{ row }">
                        {{ formatFileSize(row.fileSize) }}
                    </template>
                </el-table-column>
                <el-table-column label="发布时间" prop="releaseDate" width="180" />
                <el-table-column label="描述" prop="description" show-overflow-tooltip />
                <el-table-column label="操作" width="180" align="center">
                    <template #default="{ row }">
                        <el-button type="primary" size="small" link @click="toggleActive(row)">
                            {{ row.isActive ? '设为历史' : '激活' }}
                        </el-button>
                        <el-button type="danger" size="small" link @click="handleDelete(row)">
                            删除
                        </el-button>
                    </template>
                </el-table-column>
            </el-table>
        </el-card>

        <el-dialog v-model="uploadVisible" title="发布新版本" width="550px" :close-on-click-modal="false">
            <el-form ref="formRef" :model="form" label-width="100px">
                <el-form-item label="版本名称" prop="versionName">
                    <el-input v-model="form.versionName" placeholder="如: 1.0.0" />
                </el-form-item>
                <el-form-item label="版本号" prop="versionCode">
                    <el-input-number v-model="form.versionCode" :min="1" />
                </el-form-item>
                <el-form-item label="平台" prop="platform">
                    <el-select v-model="form.platform">
                        <el-option label="Android" value="android" />
                        <el-option label="iOS" value="ios" />
                    </el-select>
                </el-form-item>
                <el-form-item label="强制更新">
                    <el-switch v-model="form.isForceUpdate" />
                </el-form-item>
                <el-form-item label="版本说明" prop="description">
                    <el-input v-model="form.description" type="textarea" rows="3" />
                </el-form-item>
                <el-form-item label="安装包">
                    <div class="w-full">
                        <el-upload
                            ref="uploadRef"
                            :auto-upload="false"
                            :limit="1"
                            :on-change="handleFileChange"
                            :file-list="fileList"
                            :accept="form.platform === 'android' ? '.apk,.wgt' : '.ipa,.wgt'"
                            :disabled="uploading"
                        >
                            <el-button :disabled="uploading">
                                <el-icon class="mr-1"><Upload /></el-icon>
                                选择文件
                            </el-button>
                        </el-upload>
                        <div v-if="selectFile" class="mt-2 text-sm text-gray-500">
                            文件: {{ selectFile.name }} ({{ formatFileSize(selectFile.size || 0) }})
                        </div>
                        <el-progress
                            v-if="uploadProgress > 0 && uploadProgress < 100"
                            :percentage="uploadProgress"
                            :status="uploadProgress === 100 ? 'success' : undefined"
                            class="mt-2"
                        />
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="uploadVisible = false" :disabled="uploading">取消</el-button>
                <el-button type="primary" :loading="uploading" @click="handleUpload">发布</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Upload } from '@element-plus/icons-vue'
import cache from '@/utils/cache'
import base64 from '@/utils/base64'
import axios from 'axios'

const activePlatform = ref('android')
const list = ref<any[]>([])
const loading = ref(false)
const uploadVisible = ref(false)
const uploading = ref(false)
const uploadProgress = ref(0)
const formRef = ref()
const uploadRef = ref()
const fileList = ref<any[]>([])
const selectFile = ref<File | null>(null)

const imgUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME

const form = ref({
    versionName: '',
    versionCode: 1,
    platform: 'android',
    isForceUpdate: false,
    description: '',
})

function formatFileSize(bytes: number) {
    if (!bytes) return '0 B'
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}

async function loadData() {
    loading.value = true
    try {
        const res = await api.appRelease.getHistory(activePlatform.value)
        list.value = res?.data?.items || []
    } catch (e) {
        ElMessage.error('加载失败')
    } finally {
        loading.value = false
    }
}

function showUploadDialog() {
    form.value = {
        versionName: '',
        versionCode: getNextVersionCode(),
        platform: activePlatform.value,
        isForceUpdate: false,
        description: '',
    }
    selectFile.value = null
    fileList.value = []
    uploadProgress.value = 0
    uploadVisible.value = true
}

function getNextVersionCode() {
    const codes = list.value.map((x) => x.versionCode)
    return codes.length ? Math.max(...codes) + 1 : 1
}

function handleFileChange(file: any) {
    selectFile.value = file.raw
}

async function getOssSignature(): Promise<{ signature: string; policy: string }> {
    const cachedata = cache.getWithExpiry('upyun_app')
    if (cachedata && cachedata.policy && cachedata.signature) {
        return { signature: cachedata.signature, policy: cachedata.policy }
    }

    const date = new Date().toGMTString()
    const opts = {
        'save-key': `/apps/${userName}/{year}-{mon}-{day}/{random32}{.suffix}`,
        bucket: bucketName,
        expiration: Math.round(new Date().getTime() / 1000) + 43200,
        date: date,
    }
    const policy = base64.encode(JSON.stringify(opts))
    const data = ['POST', '/' + bucketName, date, policy].join('&')

    const res = await api.upload.getSignature({ data })
    cache.setWithExpiry('upyun_app', { signature: res.signature, policy }, 600)
    return { signature: res.signature, policy }
}

async function uploadToOss(file: File): Promise<string> {
    const { signature, policy } = await getOssSignature()

    const formData = new FormData()
    formData.append('file', file)
    formData.append('policy', policy)
    formData.append('authorization', `UPYUN ${userName}:${signature}`)

    const response = await axios.post(`https://v0.api.upyun.com/${bucketName}`, formData, {
        onUploadProgress: (progressEvent) => {
            if (progressEvent.total) {
                uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total)
            }
        },
    })

    if (response.data?.message === 'ok' && response.data?.url) {
        return `${imgUrl}${response.data.url}`
    }

    throw new Error('上传失败')
}

async function handleUpload() {
    if (!selectFile.value) {
        ElMessage.warning('请选择文件')
        return
    }
    if (!form.value.versionName) {
        ElMessage.warning('请输入版本名称')
        return
    }

    uploading.value = true
    uploadProgress.value = 0

    try {
        const downloadUrl = await uploadToOss(selectFile.value)

        await api.appRelease.publishByUrl({
            versionName: form.value.versionName,
            versionCode: form.value.versionCode,
            description: form.value.description || '',
            downloadUrl: downloadUrl,
            fileName: selectFile.value.name,
            fileSize: selectFile.value.size,
            isForceUpdate: form.value.isForceUpdate,
            platform: form.value.platform,
        })

        ElMessage.success('发布成功')
        uploadVisible.value = false
        loadData()
    } catch (e: any) {
        ElMessage.error(e.message || '发布失败')
    } finally {
        uploading.value = false
        uploadProgress.value = 0
    }
}

async function toggleActive(row: any) {
    try {
        await ElMessageBox.confirm(
            row.isActive ? '确定将此版本设为历史版本？' : '确定激活此版本？'
        )
        await api.appRelease.toggle(row.id)
        ElMessage.success('操作成功')
        loadData()
    } catch (e: any) {
        if (e !== 'cancel') {
            ElMessage.error('操作失败')
        }
    }
}

async function handleDelete(row: any) {
    try {
        await ElMessageBox.confirm('确定删除此版本？', '警告', { type: 'warning' })
        await api.appRelease.delete(row.id)
        ElMessage.success('删除成功')
        loadData()
    } catch (e: any) {
        if (e !== 'cancel') {
            ElMessage.error('删除失败')
        }
    }
}

onMounted(() => {
    loadData()
})
</script>