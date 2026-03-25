<template>
    <div class="app-download-page">
        <!-- 标题区域 -->
        <div class="page-header">
            <h1 class="page-title">App 下载</h1>
            <p class="page-subtitle">随时随地，畅享魔力淘</p>
        </div>

        <!-- 主内容区域 -->
        <div class="content-wrapper">
            <!-- App 信息卡片 -->
            <div class="app-card">
                <div class="app-icon">
                    <img :src="logoImage" alt="魔力淘" />
                </div>
                <div class="app-info">
                    <h2 class="app-name">魔力淘 App</h2>
                    <div class="version-badge">
                        <span class="label">最新版本</span>
                        <span class="value">{{ latestVersion?.latestVersionName || '加载中...' }}</span>
                    </div>
                </div>
            </div>

            <!-- 版本描述 -->
            <div class="description-card" v-if="latestVersion?.description">
                <h3 class="card-title">更新内容</h3>
                <div class="description-content">{{ latestVersion.description }}</div>
            </div>

            <!-- 下载按钮 -->
            <div class="download-section">
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

                <el-button size="large" @click="showHistoryDialog = true">
                    <template #icon>
                        <el-icon><Clock /></el-icon>
                    </template>
                    历史版本
                </el-button>
            </div>

            <!-- 二维码区域 -->
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

        <!-- 历史版本弹窗 -->
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
import { Download, Iphone, Clock } from '@element-plus/icons-vue'
import appReleaseAPI from '@/api/appRelease'
import logoImage from '@/assets/images/logo.png'

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

// 监听弹窗打开时加载历史版本
import { watch } from 'vue'
watch(showHistoryDialog, async (val) => {
    if (val) {
        await loadHistoryVersions()
    }
})
</script>

<style lang="scss" scoped>
.app-download-page {
    width: 100%;
    max-width: 900px;
    margin: 0 auto;
    padding: 30px 20px;
}

.page-header {
    text-align: center;
    margin-bottom: 30px;

    .page-title {
        font-size: 28px;
        font-weight: 600;
        color: #833a00;
        margin: 0 0 8px 0;
    }

    .page-subtitle {
        font-size: 14px;
        color: #666;
        margin: 0;
    }
}

.content-wrapper {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.app-card {
    display: flex;
    align-items: center;
    gap: 24px;
    padding: 24px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);

    .app-icon {
        width: 100px;
        height: 100px;
        border-radius: 20px;
        overflow: hidden;
        flex-shrink: 0;

        img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }
    }

    .app-info {
        flex: 1;

        .app-name {
            font-size: 24px;
            font-weight: 600;
            color: #333;
            margin: 0 0 12px 0;
        }

        .version-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 6px 12px;
            background: #f0f9eb;
            border-radius: 20px;

            .label {
                font-size: 12px;
                color: #999;
            }

            .value {
                font-size: 14px;
                font-weight: 500;
                color: #67c23a;
            }
        }
    }
}

.description-card {
    padding: 20px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);

    .card-title {
        font-size: 16px;
        font-weight: 500;
        color: #333;
        margin: 0 0 12px 0;
    }

    .description-content {
        font-size: 14px;
        color: #666;
        line-height: 1.8;
        white-space: pre-wrap;
    }
}

.download-section {
    display: flex;
    gap: 16px;
    justify-content: center;
    flex-wrap: wrap;
}

.qr-section {
    text-align: center;
    padding: 24px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);

    .qr-title {
        font-size: 14px;
        color: #666;
        margin-bottom: 16px;
    }

    .qr-code {
        width: 180px;
        height: 180px;
        margin: 0 auto;
        border: 1px solid #eee;
        border-radius: 8px;
        overflow: hidden;

        img {
            width: 100%;
            height: 100%;
            object-fit: contain;
        }
    }
}

@media (max-width: 768px) {
    .app-download-page {
        padding: 20px 15px;
    }

    .app-card {
        flex-direction: column;
        text-align: center;
    }

    .download-section {
        flex-direction: column;

        .el-button {
            width: 100%;
        }
    }
}
</style>