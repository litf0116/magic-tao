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

        <el-dialog v-model="uploadVisible" title="发布新版本" width="500px">
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
                    <el-input v-model="form.description" type="textarea" rows="4" />
                </el-form-item>
                <el-form-item label="APK文件" prop="file">
                    <el-upload
                        ref="uploadRef"
                        :auto-upload="false"
                        :limit="1"
                        :on-change="handleFileChange"
                        :file-list="fileList"
                        accept=".apk,.wgt"
                    >
                        <el-button>选择文件</el-button>
                    </el-upload>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="uploadVisible = false">取消</el-button>
                <el-button type="primary" :loading="uploading" @click="handleUpload">发布</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const activePlatform = ref('android')
const list = ref<any[]>([])
const loading = ref(false)
const uploadVisible = ref(false)
const uploading = ref(false)
const formRef = ref()
const uploadRef = ref()
const fileList = ref<any[]>([])
const selectFile = ref<File | null>(null)

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
        list.value = res.items || []
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
    uploadVisible.value = true
}

function getNextVersionCode() {
    const codes = list.value.map((x) => x.versionCode)
    return codes.length ? Math.max(...codes) + 1 : 1
}

function handleFileChange(file: any) {
    selectFile.value = file.raw
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
    try {
        await api.appRelease.publish({
            versionName: form.value.versionName,
            versionCode: form.value.versionCode,
            description: form.value.description,
            isForceUpdate: form.value.isForceUpdate,
            platform: form.value.platform,
            file: selectFile.value,
        })
        ElMessage.success('发布成功')
        uploadVisible.value = false
        loadData()
    } catch (e) {
        ElMessage.error('发布失败')
    } finally {
        uploading.value = false
    }
}

async function toggleActive(row: any) {
    await ElMessageBox.confirm(
        row.isActive ? '确定将此版本设为历史版本？' : '确定激活此版本？'
    )
    await api.appRelease.toggle(row.id)
    ElMessage.success('操作成功')
    loadData()
}

async function handleDelete(row: any) {
    await ElMessageBox.confirm('确定删除此版本？', '警告', { type: 'warning' })
    await api.appRelease.delete(row.id)
    ElMessage.success('删除成功')
    loadData()
}

onMounted(() => {
    loadData()
})
</script>
