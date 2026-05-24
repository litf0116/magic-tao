<template>
    <div class="android-download-page">
        <!-- 微信内置浏览器引导 -->
        <div v-if="isWechatBrowser" class="wechat-guide">
            <div class="wechat-guide-content">
                <div class="wechat-icon">
                    <svg viewBox="0 0 24 24" fill="currentColor" width="48" height="48">
                        <path
                            d="M8.5 11a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3zm7 0a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3zM12 2C6.477 2 2 6.477 2 12c0 1.89.525 3.66 1.438 5.168L3.146 20.59A1 1 0 0 0 4.5 22h.17a9.98 9.98 0 0 0 5.33-1.473A9.96 9.96 0 0 0 12 22c5.523 0 10-4.477 10-10S17.523 2 12 2z"
                        />
                    </svg>
                </div>
                <h2>请使用其他浏览器打开</h2>
                <p>点击右上角 <strong>···</strong> 或 <strong>更多</strong> 按钮</p>
                <p>选择 <strong>在浏览器中打开</strong> 或 <strong>Safari/Chrome</strong></p>
                <p>即可下载魔力淘 App</p>
                <div class="browser-hints">
                    <div class="hint-item">
                        <span class="hint-icon">🦊</span>
                        <span>Chrome</span>
                    </div>
                    <div class="hint-item">
                        <span class="hint-icon">🐒</span>
                        <span>Safari</span>
                    </div>
                    <div class="hint-item">
                        <span class="hint-icon">🌐</span>
                        <span>其他浏览器</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- 主内容区域 -->
        <template v-else>
            <!-- 标题区域 -->
            <div class="page-header">
                <h1 class="page-title">魔力淘 App</h1>
                <p class="page-subtitle">随时随地，畅享魔力淘</p>
            </div>

            <!-- App 信息卡片 -->
            <div class="content-wrapper">
                <div class="app-card">
                    <div class="app-icon">
                        <img :src="logoImage" alt="魔力淘" />
                    </div>
                    <div class="app-info">
                        <h2 class="app-name">魔力淘</h2>
                        <div class="version-badge">
                            <span class="label">最新版本</span>
                            <span class="value">{{ latestVersion?.latestVersionName || '加载中...' }}</span>
                        </div>
                        <div v-if="latestVersion?.fileSize" class="file-size">
                            {{ formatFileSize(latestVersion.fileSize) }}
                        </div>
                    </div>
                </div>

                <!-- 版本描述 -->
                <div v-if="latestVersion?.description" class="description-card">
                    <h3 class="card-title">更新内容</h3>
                    <div class="description-content">{{ latestVersion.description }}</div>
                </div>

                <!-- 下载按钮 -->
                <div class="download-section">
                    <el-button type="primary" size="large" :loading="downloading" @click="downloadAndroid">
                        <template #icon>
                            <el-icon><Download /></el-icon>
                        </template>
                        {{ downloading ? '下载中...' : '下载 APK' }}
                    </el-button>

                    <el-button size="large" @click="openH5">
                        <template #icon>
                            <el-icon><Link /></el-icon>
                        </template>
                        访问 H5 网页
                    </el-button>
                </div>

                <!-- 其他平台提示 -->
                <div class="other-platforms">
                    <span class="other-platforms-text">遇到下载问题？</span>
                    <a href="https://www.molitao.top/h5/" target="_blank" class="other-platforms-link">打开 H5 网页</a>
                </div>
            </div>
        </template>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Download, Link } from '@element-plus/icons-vue'
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

const latestVersion = ref<VersionInfo | null>(null)
const downloading = ref(false)

// 检测微信内置浏览器
const isWechatBrowser = computed(() => {
    const ua = navigator.userAgent.toLowerCase()
    return ua.includes('micromessenger')
})

function formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B'
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
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
</script>

<style lang="scss" scoped>
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.android-download-page {
    width: 100%;
    max-width: 600px;
    margin: 0 auto;
    padding: 30px 20px;
    min-height: 100vh;
    background: linear-gradient(135deg, #fff2e8 0%, #fff8f5 100%);
}

// 微信引导样式
.wechat-guide {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 80vh;
    padding: 40px 20px;
}

.wechat-guide-content {
    text-align: center;
    background: #fff;
    border-radius: 16px;
    padding: 40px 30px;
    box-shadow: 0 8px 32px rgba(131, 58, 0, 0.15);

    .wechat-icon {
        width: 80px;
        height: 80px;
        margin: 0 auto 24px;
        background: #07c160;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
    }

    h2 {
        font-size: 24px;
        font-weight: 600;
        color: $primary-color;
        margin: 0 0 20px 0;
    }

    p {
        font-size: 16px;
        color: #666;
        margin: 8px 0;
        line-height: 1.6;
    }

    strong {
        color: $primary-color;
    }
}

.browser-hints {
    display: flex;
    justify-content: center;
    gap: 24px;
    margin-top: 24px;
    padding-top: 24px;
    border-top: 1px solid #eee;

    .hint-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;

        .hint-icon {
            font-size: 32px;
        }

        span {
            font-size: 13px;
            color: #999;
        }
    }
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
    gap: 20px;
}

.app-card {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 24px;
    background: #fff;
    border: 2px solid $border-color;
    border-radius: 16px;

    .app-icon {
        width: 80px;
        height: 80px;
        border-radius: 16px;
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
            font-size: 22px;
            font-weight: 600;
            color: $primary-color;
            margin: 0 0 10px 0;
        }

        .version-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 4px 12px;
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

        .file-size {
            margin-top: 8px;
            font-size: 13px;
            color: $primary-light;
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

.download-section {
    display: flex;
    flex-direction: column;
    gap: 12px;

    .el-button {
        width: 100%;
        height: 52px;
        font-size: 16px;
        border-radius: 12px;
    }

    .el-button--primary {
        background: $primary-color;
        border-color: $primary-color;

        &:hover {
            background: #a34a06;
            border-color: #a34a06;
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

@media (max-width: 480px) {
    .android-download-page {
        padding: 20px 15px;
    }

    .app-card {
        flex-direction: column;
        text-align: center;
    }

    .wechat-guide-content {
        padding: 30px 20px;

        h2 {
            font-size: 20px;
        }

        p {
            font-size: 14px;
        }
    }

    .browser-hints {
        gap: 16px;

        .hint-item .hint-icon {
            font-size: 28px;
        }
    }
}
</style>
