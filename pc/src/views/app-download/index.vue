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
            <div v-if="latestVersion?.description" class="description-card">
                <h3 class="card-title">更新内容</h3>
                <div class="description-content">{{ latestVersion.description }}</div>
            </div>

            <!-- 平台选择 -->
            <div class="platform-tabs">
                <div
                    class="tab-item"
                    :class="{ active: currentPlatform === 'android' }"
                    @click="switchPlatform('android')"
                >
                    <svg class="platform-icon" viewBox="0 0 24 24" fill="currentColor">
                        <path
                            d="M17.6 11.48V8a1.52 1.52 0 0 0-3 0v3.48h-5.2V8a1.52 1.52 0 0 0-3 0v3.48H4v6.4h16v-6.4h-2.4zM7.6 16.4a1.2 1.2 0 1 1 0-2.4 1.2 1.2 0 0 1 0 2.4zm8.8 0a1.2 1.2 0 1 1 0-2.4 1.2 1.2 0 0 1 0 2.4z"
                        />
                        <path d="M6.4 5.2l1.4 2.4h8.4l1.4-2.4-2.2-2.2.8-.8-1-.8-.8.8H10l-.8-.8-1 .8.8.8L6.4 5.2z" />
                    </svg>
                    <span>Android</span>
                </div>
                <div class="tab-item" :class="{ active: currentPlatform === 'ios' }" @click="switchPlatform('ios')">
                    <svg class="platform-icon" viewBox="0 0 24 24" fill="currentColor">
                        <path
                            d="M18.71 19.5c-.83 1.24-1.71 2.45-3.05 2.47-1.34.03-1.77-.79-3.29-.79-1.53 0-2 .77-3.27.82-1.31.05-2.3-1.32-3.14-2.53C4.25 17 2.94 12.45 4.7 9.39c.87-1.52 2.43-2.48 4.12-2.51 1.28-.02 2.5.87 3.29.87.78 0 2.26-1.07 3.81-.91.65.03 2.47.26 3.64 1.98-.09.06-2.17 1.28-2.15 3.81.03 3.02 2.65 4.03 2.68 4.04-.03.07-.42 1.44-1.38 2.83M13 3.5c.73-.83 1.94-1.46 2.94-1.5.13 1.17-.34 2.35-1.04 3.19-.69.85-1.83 1.51-2.95 1.42-.15-1.15.41-2.35 1.05-3.11z"
                        />
                    </svg>
                    <span>iOS</span>
                </div>
            </div>

            <!-- 二维码区域（移动端 Android 隐藏，直接显示下载按钮） -->
            <div v-if="!(currentPlatform === 'android' && isMobile)" class="qr-section">
                <div class="qr-title">
                    <template v-if="currentPlatform === 'android'"> 扫码打开下载页 </template>
                    <template v-else> 扫码访问 H5 网页 </template>
                </div>
                <div class="qr-code">
                    <img :src="currentQrCode" :alt="currentPlatform === 'android' ? 'Android 下载页' : 'iOS H5 访问'" />
                </div>
                <div class="qr-hint">
                    <template v-if="currentPlatform === 'android'"> 手机扫码可下载 Android App </template>
                    <template v-else> 扫码在 Safari 中打开，可添加到桌面 </template>
                </div>
            </div>

            <!-- iOS 添加到桌面说明 -->
            <div v-if="currentPlatform === 'ios'" class="ios-guide">
                <h3 class="guide-title">
                    <el-icon><InfoFilled /></el-icon>
                    如何将网页添加到桌面
                </h3>
                <div class="guide-steps">
                    <div class="step">
                        <div class="step-number">1</div>
                        <div class="step-content">
                            <div class="step-title">在 Safari 中打开</div>
                            <div class="step-desc">使用 iPhone 自带的 Safari 浏览器打开本页面</div>
                        </div>
                    </div>
                    <div class="step">
                        <div class="step-number">2</div>
                        <div class="step-content">
                            <div class="step-title">点击分享按钮</div>
                            <div class="step-desc">点击底部工具栏的「分享」按钮</div>
                        </div>
                    </div>
                    <div class="step">
                        <div class="step-number">3</div>
                        <div class="step-content">
                            <div class="step-title">选择「添加到主屏幕」</div>
                            <div class="step-desc">在弹出的菜单中找到并点击「添加到主屏幕」选项</div>
                        </div>
                    </div>
                    <div class="step">
                        <div class="step-number">4</div>
                        <div class="step-content">
                            <div class="step-title">确认添加</div>
                            <div class="step-desc">点击右上角「添加」完成操作，桌面上会出现魔力淘图标</div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- 下载按钮 -->
            <div class="download-section">
                <el-button
                    v-if="currentPlatform === 'android'"
                    type="primary"
                    size="large"
                    :loading="downloading"
                    @click="downloadAndroid"
                >
                    <template #icon>
                        <el-icon><Download /></el-icon>
                    </template>
                    {{ downloading ? '下载中...' : '直接下载 APK' }}
                </el-button>

                <el-button v-else type="primary" size="large" @click="openH5">
                    <template #icon>
                        <el-icon><Link /></el-icon>
                    </template>
                    在浏览器中打开 H5
                </el-button>

                <el-button size="large" @click="showHistoryDialog = true">
                    <template #icon>
                        <el-icon><Clock /></el-icon>
                    </template>
                    历史版本
                </el-button>
            </div>

            <!-- 其他平台提示 -->
            <div class="other-platforms">
                <span class="other-platforms-text">下载遇到了问题？</span>
                <a href="https://www.molitao.top/h5/" target="_blank" class="other-platforms-link">打开 H5 网页</a>
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
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Clock, Download, InfoFilled, Link } from '@element-plus/icons-vue'
import logoImage from '@/assets/images/logo.png'

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

const currentPlatform = ref<'android' | 'ios'>('android')
const latestVersion = ref<VersionInfo | null>(null)
const historyVersions = ref<HistoryVersion[]>([])
const showHistoryDialog = ref(false)
const downloading = ref(false)
const downloadingVersionId = ref<number | null>(null)

// 移动端检测
const isMobile = ref(window.innerWidth < 768)
function onResize() {
    isMobile.value = window.innerWidth < 768
}

// Android 下载页二维码（指向独立 H5 下载页）
const androidQrCode = computed(() => {
    const pageUrl = 'https://www.molitao.top/app-download/'
    return `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(pageUrl)}`
})

// iOS H5 访问二维码
const iosQrCode = computed(() => {
    const h5Url = 'https://www.molitao.top/h5/'
    return `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(h5Url)}`
})

// 当前显示的二维码
const currentQrCode = computed(() => {
    return currentPlatform.value === 'android' ? androidQrCode.value : iosQrCode.value
})

const switchPlatform = (platform: 'android' | 'ios') => {
    currentPlatform.value = platform
}

const downloadAndroid = () => {
    if (!latestVersion.value?.downloadUrl) {
        ElMessage.warning('暂无可用下载链接')
        return
    }

    downloading.value = true

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
            downloading.value = false
        }, 1000)
    }
}

const openH5 = () => {
    window.open('https://www.molitao.top/h5/', '_blank')
}

onMounted(async () => {
    await loadLatestVersion()
    window.addEventListener('resize', onResize)
})

onUnmounted(() => {
    window.removeEventListener('resize', onResize)
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
watch(showHistoryDialog, async (val) => {
    if (val) {
        await loadHistoryVersions()
    }
})
</script>

<style lang="scss" scoped>
// 网站主色调
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

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
        color: $primary-color;
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
    border: 2px solid $border-color;
    border-radius: 12px;

    .app-icon {
        width: 100px;
        height: 100px;
        border-radius: 20px;
        overflow: hidden;
        flex-shrink: 0;
        border: 2px solid $border-color;

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
            color: $primary-color;
            margin: 0 0 12px 0;
        }

        .version-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 6px 12px;
            background: $bg-light;
            border: 1px solid $border-color;
            border-radius: 20px;

            .label {
                font-size: 12px;
                color: $primary-light;
            }

            .value {
                font-size: 14px;
                font-weight: 500;
                color: $primary-color;
            }
        }
    }
}

.description-card {
    padding: 20px;
    background: #fff;
    border: 2px solid $border-color;
    border-radius: 12px;

    .card-title {
        font-size: 16px;
        font-weight: 500;
        color: $primary-color;
        margin: 0 0 12px 0;
    }

    .description-content {
        font-size: 14px;
        color: #666;
        line-height: 1.8;
        white-space: pre-wrap;
    }
}

.platform-tabs {
    display: flex;
    gap: 12px;
    justify-content: center;

    .tab-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 12px 32px;
        background: #fff;
        border: 2px solid $border-color;
        border-radius: 12px;
        cursor: pointer;
        transition: all 0.3s;
        font-size: 16px;
        font-weight: 500;
        color: $primary-color;

        &:hover {
            background: $bg-card;
        }

        &.active {
            background: $primary-color;
            border-color: $primary-color;
            color: #fff;

            .platform-icon {
                filter: brightness(0) invert(1);
            }
        }

        .platform-icon {
            width: 24px;
            height: 24px;
            transition: filter 0.3s;
        }
    }
}

.qr-section {
    text-align: center;
    padding: 24px;
    background: #fff;
    border: 2px solid $border-color;
    border-radius: 12px;

    .qr-title {
        font-size: 16px;
        font-weight: 500;
        color: $primary-color;
        margin-bottom: 16px;
    }

    .qr-code {
        width: 200px;
        height: 200px;
        margin: 0 auto;
        border: 2px solid $border-color;
        border-radius: 8px;
        overflow: hidden;
        background: #fff;

        img {
            width: 100%;
            height: 100%;
            object-fit: contain;
        }
    }

    .qr-hint {
        margin-top: 12px;
        font-size: 13px;
        color: $primary-light;
    }
}

.ios-guide {
    padding: 20px;
    background: $bg-light;
    border: 2px solid $border-color;
    border-radius: 12px;

    .guide-title {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 16px;
        font-weight: 500;
        color: $primary-color;
        margin: 0 0 16px 0;
    }

    .guide-steps {
        display: flex;
        flex-direction: column;
        gap: 16px;
    }

    .step {
        display: flex;
        gap: 16px;
        align-items: flex-start;

        .step-number {
            width: 28px;
            height: 28px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: $primary-color;
            color: #fff;
            border-radius: 50%;
            font-size: 14px;
            font-weight: 600;
            flex-shrink: 0;
        }

        .step-content {
            flex: 1;

            .step-title {
                font-size: 14px;
                font-weight: 500;
                color: $primary-color;
                margin-bottom: 4px;
            }

            .step-desc {
                font-size: 13px;
                color: $primary-light;
                line-height: 1.5;
            }
        }
    }
}

.download-section {
    display: flex;
    gap: 16px;
    justify-content: center;
    flex-wrap: wrap;

    .el-button--primary {
        background: $primary-color;
        border-color: $primary-color;

        &:hover {
            background: darken($primary-color, 10%);
            border-color: darken($primary-color, 10%);
        }
    }
}

.other-platforms {
    text-align: center;
    padding: 16px;
    background: #fff;
    border: 2px solid $border-color;
    border-radius: 12px;

    .other-platforms-text {
        font-size: 14px;
        color: #666;
        margin-right: 8px;
    }

    .other-platforms-link {
        font-size: 14px;
        color: $primary-color;
        text-decoration: none;
        font-weight: 500;

        &:hover {
            text-decoration: underline;
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

    .platform-tabs {
        flex-direction: column;

        .tab-item {
            justify-content: center;
        }
    }

    .download-section {
        flex-direction: column;

        .el-button {
            width: 100%;
        }
    }
}
</style>
