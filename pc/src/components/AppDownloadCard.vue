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
                    <img
                        src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300"
                        alt="扫码下载"
                        class="qr-code"
                    />
                    <span class="qr-hint">扫码下载</span>
                </div>

                <!-- 下载按钮 -->
                <div class="download-buttons">
                    <el-button type="primary" size="small" @click="downloadApp('android')">
                        <el-icon><Download /></el-icon>
                        Android
                    </el-button>
                    <el-button size="small" @click="downloadApp('ios')">
                        <el-icon><Iphone /></el-icon>
                        iOS
                    </el-button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Close, Download, Iphone } from '@element-plus/icons-vue'
import appReleaseAPI from '@/api/appRelease'
import logoImage from '@/assets/images/logo.png'

interface VersionInfo {
    latestVersionName: string
    downloadUrl: string
}

const isExpanded = ref(false)
const latestVersion = ref<VersionInfo | null>(null)

const toggleExpand = () => {
    isExpanded.value = !isExpanded.value
}

const downloadApp = (platform: string) => {
    if (latestVersion.value?.downloadUrl) {
        window.open(latestVersion.value.downloadUrl, '_blank')
    } else {
        // 如果没有版本信息，跳转到下载页面
        window.location.href = '/app-download'
    }
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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 24px;
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    cursor: pointer;
    transition: all 0.3s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(102, 126, 234, 0.5);
    }

    .mini-icon {
        width: 24px;
        height: 24px;
        border-radius: 6px;
    }

    .download-text {
        color: #fff;
        font-size: 14px;
        font-weight: 500;
        white-space: nowrap;
    }
}

.expanded-view {
    width: 280px;
    background: #fff;
    border-radius: 16px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: #fff;

    .app-info {
        display: flex;
        align-items: center;
        gap: 12px;

        .app-icon {
            width: 40px;
            height: 40px;
            border-radius: 10px;
        }

        .app-meta {
            .app-name {
                font-size: 16px;
                font-weight: 600;
                margin: 0 0 2px 0;
            }

            .version {
                font-size: 12px;
                opacity: 0.8;
            }
        }
    }

    .close-btn {
        width: 28px;
        height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(255, 255, 255, 0.2);
        border: none;
        border-radius: 50%;
        color: #fff;
        cursor: pointer;
        transition: background 0.2s;

        &:hover {
            background: rgba(255, 255, 255, 0.3);
        }
    }
}

.card-body {
    padding: 16px;

    .qr-wrapper {
        display: flex;
        flex-direction: column;
        align-items: center;
        margin-bottom: 16px;

        .qr-code {
            width: 140px;
            height: 140px;
            border-radius: 8px;
            border: 1px solid #eee;
        }

        .qr-hint {
            margin-top: 8px;
            font-size: 12px;
            color: #999;
        }
    }

    .download-buttons {
        display: flex;
        gap: 10px;
        justify-content: center;

        .el-button {
            flex: 1;
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