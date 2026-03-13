<template>
    <div class="app-download-container">
        <div class="app-card">
            <div class="app-icon">
                <img src="/logo.png" alt="魔力淘" />
            </div>

            <div class="app-info">
                <h1 class="app-title">魔力淘 App</h1>
                <div class="app-version">
                    <span class="version-label">最新版本</span>
                    <span class="version-number">{{ latestVersion?.latestVersionName || '加载中...' }}</span>
                </div>
            </div>

            <div class="app-description">
                <div class="description-content">
                    {{ latestVersion?.description || '暂无版本描述' }}
                </div>
            </div>

            <div class="download-actions">
                <el-button type="primary" size="large" :loading="downloading.android" @click="downloadApk('android')">
                    <template #icon>
                        <el-icon><Download /></el-icon>
                    </template>
                    {{ downloading.android ? '下载中...' : 'Android 下载' }}
                </el-button>

                <el-button size="large" :loading="downloading.ios" @click="downloadApk('ios')">
                    <template #icon>
                        <el-icon><Iphone /></el-icon>
                    </template>
                    {{ downloading.ios ? '下载中...' : 'iOS 下载' }}
                </el-button>

                <el-button size="large" @click="showHistoryDialog = true"> 历史版本 </el-button>
            </div>

            <div class="qr-section">
                <div class="qr-title">手机扫码快速下载</div>
                <div class="qr-code">
                    <img
                        src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300"
                        alt="扫码下载"
                    />
                </div>
            </div>
        </div>

        <el-dialog v-model="showHistoryDialog" title="历史版本" width="800px">
            <el-table :data="historyVersions" style="width: 100%">
                <el-table-column prop="versionName" label="版本号" width="120" />
                <el-table-column prop="releaseDate" label="发布时间" width="180" />
                <el-table-column prop="description" label="描述" show-overflow-tooltip />
                <el-table-column prop="fileSize" label="文件大小" width="100" />
                <el-table-column label="操作" width="120">
                    <template #default="{ row }">
                        <el-button
                            type="primary"
                            size="small"
                            link
                            :loading="downloadingVersionId === row.id"
                            @click="downloadVersion(row)"
                        >
                            下载
                        </el-button>
                    </template>
                </el-table-column>
            </el-table>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import api from '@/api'
import appReleaseAPI from '@/api/appRelease'

interface VersionInfo {
    latestVersionCode: number
    latestVersionName: string
    description: string
    downloadUrl: string
    fileName: string
    fileSize: number
    isForceUpdate: boolean
    releaseDate: string
}

interface HistoryVersion {
    id: number
    versionName: string
    versionCode: number
    description: string
    fileName: string
    fileSize: number
    isForceUpdate: boolean
    platform: string
    releaseDate: string
    isActive: boolean
    downloadUrl: string
}

const latestVersion = ref<VersionInfo | null>(null)
const historyVersions = ref<HistoryVersion[]>([])
const showHistoryDialog = ref(false)
const downloading = ref({ android: false, ios: false })
const downloadingVersionId = ref<number | null>(null)

onMounted(async () => {
    await loadLatestVersion()
})

async function loadLatestVersion() {
    try {
        const result = await appReleaseAPI.checkUpdate(0, 'android')
        latestVersion.value = result
    } catch (error) {
        console.error('加载版本信息失败', error)
        latestVersion.value = {
            latestVersionCode: 0,
            latestVersionName: '1.0.0',
            description: '暂无版本描述',
            downloadUrl: '',
            fileName: '',
            fileSize: 0,
            isForceUpdate: false,
            releaseDate: '',
        }
    }
}

async function loadHistoryVersions() {
    if (historyVersions.value.length > 0) return

    try {
        const result = await appReleaseAPI.getHistory('android')
        historyVersions.value = result.items || []
    } catch (error) {
        console.error('加载历史版本失败', error)
        ElMessage.error('加载历史版本失败')
    }
}

async function downloadApk(platform: string) {
    if (!latestVersion.value?.downloadUrl) {
        ElMessage.warning('暂无可用下载链接')
        return
    }

    downloading.value[platform] = true

    try {
        const url = latestVersion.value.downloadUrl.startsWith('http')
            ? latestVersion.value.downloadUrl
            : window.location.origin + latestVersion.value.downloadUrl

        window.open(url, '_blank')

        ElMessage.success('下载已开始')
    } catch (error) {
        console.error('下载失败', error)
        ElMessage.error('下载失败')
    } finally {
        setTimeout(() => {
            downloading.value[platform] = false
        }, 1000)
    }
}

async function downloadVersion(version: HistoryVersion) {
    downloadingVersionId.value = version.id

    try {
        const url = version.downloadUrl.startsWith('http')
            ? version.downloadUrl
            : window.location.origin + version.downloadUrl

        window.open(url, '_blank')

        ElMessage.success('下载已开始')
    } catch (error) {
        console.error('下载失败', error)
        ElMessage.error('下载失败')
    } finally {
        setTimeout(() => {
            downloadingVersionId.value = null
        }, 1000)
    }
}

async function openHistoryDialog() {
    showHistoryDialog.value = true
    await loadHistoryVersions()
}

defineExpose({
    openHistoryDialog,
})
</script>

<style scoped>
.app-download-container {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 20px;
}

.app-card {
    width: 800px;
    max-width: 100%;
    background: white;
    border-radius: 16px;
    padding: 48px 40px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 32px;
}

.app-icon {
    width: 160px;
    height: 160px;
    border-radius: 32px;
    overflow: hidden;
    border: 2px solid #e5e7eb;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.app-icon img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.app-info {
    text-align: center;
}

.app-title {
    font-size: 28px;
    font-weight: 600;
    color: #1f2937;
    margin: 0;
}

.app-version {
    margin-top: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
}

.version-label {
    font-size: 14px;
    color: #9ca3af;
}

.version-number {
    font-size: 16px;
    color: #6b7280;
    font-weight: 500;
}

.app-description {
    width: 100%;
    max-height: 200px;
    overflow-y: auto;
    background: #f9fafb;
    border-radius: 12px;
    padding: 20px;
}

.description-content {
    font-size: 14px;
    color: #6b7280;
    line-height: 1.6;
    white-space: pre-wrap;
}

.download-actions {
    display: flex;
    gap: 16px;
    flex-wrap: wrap;
    justify-content: center;
}

.qr-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
}

.qr-title {
    font-size: 14px;
    color: #6b7280;
}

.qr-code {
    width: 200px;
    height: 200px;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    overflow: hidden;
}

.qr-code img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

@media (max-width: 768px) {
    .app-card {
        padding: 32px 24px;
        gap: 24px;
    }

    .app-icon {
        width: 120px;
        height: 120px;
        border-radius: 24px;
    }

    .app-title {
        font-size: 24px;
    }

    .download-actions {
        flex-direction: column;
        width: 100%;
    }

    .download-actions .el-button {
        width: 100%;
    }
}
</style>
