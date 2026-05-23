<template>
    <div class="app-download-card" :class="{ expanded: isExpanded }">
        <!-- 收起状态 - 小图标 -->
        <div v-if="!isExpanded" class="collapsed-view" @click="toggleExpand">
            <img :src="logoImage" alt="魔力淘" class="mini-icon" />
            <span class="download-text">下载App</span>
        </div>

        <!-- 展开状态 - 完整卡片 -->
        <div v-else class="expanded-view">
            <div class="card-header">
                <div class="app-info">
                    <img :src="logoImage" alt="魔力淘" class="app-icon" />
                    <div class="app-meta">
                        <h3 class="app-name">魔力淘 App</h3>
                        <span class="version">{{ latestVersion?.latestVersionName || 'v1.0.0' }}</span>
                    </div>
                </div>
                <button class="close-btn" @click="toggleExpand">
                    <el-icon><Close /></el-icon>
                </button>
            </div>

            <div class="card-body">
                <!-- 二维码 -->
                <div class="qr-wrapper">
                    <img :src="downloadQrCode" alt="扫码下载" class="qr-code" />
                    <span class="qr-hint">扫码下载</span>
                </div>

                <!-- 跳转按钮 -->
                <el-button type="primary" class="goto-btn" @click="goToDownloadPage">
                    <el-icon><Download /></el-icon>
                    前往下载页面
                </el-button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { Close, Download } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import appReleaseAPI from '@/api/appRelease'
import logoImage from '@/assets/images/logo.png'

interface VersionInfo {
    latestVersionName: string
    downloadUrl: string
}

const router = useRouter()
const isExpanded = ref(false)
const latestVersion = ref<VersionInfo | null>(null)

// 下载页面二维码
const downloadQrCode = computed(() => {
    const pageUrl = 'https://www.molitao.top/#/app-download'
    return `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(pageUrl)}`
})

const toggleExpand = () => {
    isExpanded.value = !isExpanded.value
}

const goToDownloadPage = () => {
    router.push('/app-download')
}

onMounted(async () => {
    try {
        const result = await appReleaseAPI.checkUpdate(0, 'android')
        latestVersion.value = result
    } catch (error) {
        console.error('获取版本信息失败', error)
    }
})
</script>

<style lang="scss" scoped>
// 网站主色调 - 暖色系复古游戏风格
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.app-download-card {
    position: fixed;
    top: 100px;
    right: 20px;
    z-index: 100;
    transition: all 0.3s ease;

    @media (max-width: 1200px) {
        top: 80px;
        right: 15px;
    }
}

.collapsed-view {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 16px;
    background: $bg-card;
    border: 2px solid $border-color;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
    cursor: pointer;
    transition: all 0.3s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(131, 58, 0, 0.25);
        background: darken($bg-card, 5%);
    }

    .mini-icon {
        width: 24px;
        height: 24px;
        border-radius: 6px;
        border: 1px solid $border-color;
    }

    .download-text {
        color: $primary-color;
        font-size: 14px;
        font-weight: 600;
        white-space: nowrap;
    }
}

.expanded-view {
    width: 280px;
    background: #fff;
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 8px 24px rgba(131, 58, 0, 0.2);
    overflow: hidden;
    animation: slideIn 0.3s ease;
}

@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px;
    background: $bg-card;
    border-bottom: 2px solid $border-color;

    .app-info {
        display: flex;
        align-items: center;
        gap: 12px;

        .app-icon {
            width: 40px;
            height: 40px;
            border-radius: 10px;
            border: 2px solid $border-color;
        }

        .app-meta {
            .app-name {
                font-size: 16px;
                font-weight: 600;
                color: $primary-color;
                margin: 0 0 2px 0;
            }

            .version {
                font-size: 12px;
                color: $primary-light;
                background: #fff;
                padding: 2px 8px;
                border-radius: 10px;
            }
        }
    }

    .close-btn {
        width: 28px;
        height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #fff;
        border: 1px solid $border-color;
        border-radius: 50%;
        color: $primary-color;
        cursor: pointer;
        transition: all 0.2s;

        &:hover {
            background: $primary-color;
            color: #fff;
        }
    }
}

.card-body {
    padding: 16px;
    background: $bg-light;

    .qr-wrapper {
        display: flex;
        flex-direction: column;
        align-items: center;
        margin-bottom: 16px;

        .qr-code {
            width: 140px;
            height: 140px;
            border-radius: 8px;
            border: 2px solid $border-color;
            background: #fff;
        }

        .qr-hint {
            margin-top: 8px;
            font-size: 12px;
            color: $primary-light;
        }
    }

    .goto-btn {
        width: 100%;
        background: $primary-color;
        border-color: $primary-color;

        &:hover {
            background: darken($primary-color, 10%);
            border-color: darken($primary-color, 10%);
        }
    }
}

// 移动端隐藏固定卡片
@media (max-width: 768px) {
    .app-download-card {
        display: none;
    }
}
</style>
