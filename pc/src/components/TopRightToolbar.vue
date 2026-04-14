<template>
    <div class="top-right-toolbar">
        <!-- 工具栏容器 - 垂直排列 -->
        <div class="toolbar-container">
            <!-- 用户信息 -->
            <template v-if="userStore.isLogin">
                <el-tooltip content="我的" placement="left">
                    <div class="tool-item user-item" @click="toggleUserPanel">
                        <el-avatar :size="36" :src="userStore.user.headImgUrl" class="user-avatar">
                            <el-icon><User /></el-icon>
                        </el-avatar>
                        <span class="tool-label">我的</span>
                    </div>
                </el-tooltip>
            </template>

            <!-- 未登录 - 登录按钮 -->
            <template v-else>
                <el-tooltip content="登录" placement="left">
                    <div class="tool-item login-item" @click="goToLogin">
                        <div class="tool-icon">
                            <el-icon><User /></el-icon>
                        </div>
                        <span class="tool-label">登录</span>
                    </div>
                </el-tooltip>
            </template>

            <!-- 下载App -->
            <el-tooltip content="下载App" placement="left">
                <div class="tool-item" @click="toggleDownloadPanel">
                    <div class="tool-icon download-icon">
                        <el-icon><Download /></el-icon>
                    </div>
                    <span class="tool-label">下载</span>
                </div>
            </el-tooltip>

            <!-- 拍卖 -->
            <el-tooltip content="拍卖行" placement="left">
                <div class="tool-item" @click="goToAuction">
                    <div class="tool-icon">
                        <el-icon><Trophy /></el-icon>
                    </div>
                    <span class="tool-label">拍卖</span>
                </div>
            </el-tooltip>

            <!-- 交易 -->
            <el-tooltip content="交易站" placement="left">
                <div class="tool-item" @click="goToTrading">
                    <div class="tool-icon">
                        <el-icon><Shop /></el-icon>
                    </div>
                    <span class="tool-label">交易</span>
                </div>
            </el-tooltip>

            <!-- 回到顶部 -->
            <el-tooltip content="回到顶部" placement="left">
                <div v-show="showBackTop" class="tool-item" @click="scrollToTop">
                    <div class="tool-icon back-top-icon">
                        <el-icon><ArrowUp /></el-icon>
                    </div>
                    <span class="tool-label">顶部</span>
                </div>
            </el-tooltip>

            <!-- 用户信息面板（向左展开） -->
            <Transition name="slide-left">
                <div v-if="showUserPanel" class="user-panel">
                    <div class="panel-header">
                        <div class="user-info-header">
                            <el-avatar :size="48" :src="userStore.user.headImgUrl" class="user-avatar-large">
                                <el-icon><User /></el-icon>
                            </el-avatar>
                            <div class="user-meta">
                                <h3 class="user-name">{{ userStore.user.name }}</h3>
                                <span class="user-id">ID: {{ userStore.user.id }}</span>
                            </div>
                        </div>
                        <button class="close-btn" @click="closeUserPanel">
                            <el-icon><Close /></el-icon>
                        </button>
                    </div>

                    <div class="panel-content">
                        <!-- 余额信息 -->
                        <div class="balance-info">
                            <div class="balance-item">
                                <span class="balance-label">账户余额</span>
                                <span class="balance-value">¥{{ userStore.user.balance || 0 }}</span>
                            </div>
                            <div class="balance-item highlight">
                                <span class="balance-label">诚信履约金</span>
                                <span class="balance-value">¥{{ userStore.user.depositBalance || 0 }}</span>
                            </div>
                        </div>

                        <!-- 操作按钮 -->
                        <div class="action-buttons">
                            <el-button type="primary" class="action-btn" @click="handleRechargeFromPanel">
                                <el-icon><Wallet /></el-icon>
                                诚信履约金充值
                            </el-button>
                            <el-button class="action-btn" @click="goToProfile">
                                <el-icon><User /></el-icon>
                                个人中心
                            </el-button>
                            <el-button class="action-btn logout-btn" @click="handleLogout">
                                <el-icon><SwitchButton /></el-icon>
                                退出登录
                            </el-button>
                        </div>
                    </div>
                </div>
            </Transition>

            <!-- 下载App面板（向左展开） -->
            <Transition name="slide-left">
                <div v-if="showDownloadPanel" class="download-panel">
                    <div class="panel-header">
                        <div class="app-info">
                            <img :src="logoImage" alt="魔力淘" class="app-icon" />
                            <div class="app-meta">
                                <h3 class="panel-title">魔力淘 App</h3>
                                <span class="version">{{ latestVersion?.latestVersionName || 'v1.0.0' }}</span>
                            </div>
                        </div>
                        <button class="close-btn" @click="closeDownloadPanel">
                            <el-icon><Close /></el-icon>
                        </button>
                    </div>

                    <div class="panel-content">
                        <div class="qr-wrapper">
                            <img
                                src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300"
                                alt="扫码下载"
                                class="qr-code"
                            />
                            <span class="qr-hint">扫码下载</span>
                        </div>
                        <el-button type="primary" class="goto-btn" @click="goToDownloadPage">
                            <el-icon><Download /></el-icon>
                            前往下载页面
                        </el-button>
                    </div>
                </div>
            </Transition>
        </div>

        <!-- 充值弹窗（居中） -->
        <el-dialog
            v-model="showRechargeDialog"
            title="魔力值充值"
            width="420px"
            center
            :close-on-click-modal="false"
            @closed="onRechargeDialogClosed"
        >
            <div class="recharge-dialog-content">
                <!-- 当前余额 -->
                <div class="balance-section">
                    <div class="balance-label">当前魔力值</div>
                    <div class="balance-amount">¥{{ userStore.user?.depositBalance || 0 }}</div>
                </div>

                <!-- 充值金额 -->
                <div class="amount-section">
                    <div class="amount-label">充值金额</div>
                    <div class="amount-value">¥51.00</div>
                    <div class="amount-hint">含手续费¥1，实际到账¥50</div>
                </div>

                <!-- 二维码区域 -->
                <div class="qrcode-section">
                    <div v-if="loading" class="loading-state">
                        <el-icon class="is-loading"><Loading /></el-icon>
                        <span>正在生成支付二维码...</span>
                    </div>

                    <div v-else-if="qrCodeUrl" class="qrcode-display">
                        <img :src="qrCodeUrl" alt="支付二维码" class="qrcode-img" />
                        <div class="qrcode-hint">请使用微信扫一扫完成支付</div>
                        <div v-if="countdown > 0" class="countdown">有效期剩余 {{ formatCountdown(countdown) }}</div>
                        <div v-else class="countdown expired">二维码已过期，请刷新</div>
                    </div>

                    <div v-else-if="error" class="error-state">
                        <el-icon><CircleClose /></el-icon>
                        <span>{{ error }}</span>
                        <el-button type="primary" size="small" @click="generateQRCode">重新生成</el-button>
                    </div>

                    <div v-else class="generate-btn-wrapper">
                        <el-button type="primary" size="large" :loading="loading" @click="generateQRCode">
                            <el-icon><Wallet /></el-icon>
                            生成支付二维码
                        </el-button>
                    </div>
                </div>

                <!-- 充值说明 -->
                <div class="recharge-tips">
                    <div class="tips-title">充值说明</div>
                    <ul class="tips-list">
                        <li>充值金额：¥51（含手续费¥1）</li>
                        <li>到账金额：¥50 魔力值</li>
                        <li>支持微信扫码支付</li>
                        <li>支付成功后立即到账</li>
                    </ul>
                </div>
            </div>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
    Wallet,
    Trophy,
    Shop,
    User,
    ArrowUp,
    Close,
    Loading,
    CircleClose,
    Download,
    SwitchButton,
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { payApi } from '@/api/pay'
import appReleaseAPI from '@/api/appRelease'
import logoImage from '@/assets/images/logo.png'

interface VersionInfo {
    latestVersionName: string
    downloadUrl: string
}

const router = useRouter()
const userStore = useUserStore()

// 状态管理
const showBackTop = ref(false)
const showDownloadPanel = ref(false)
const showRechargeDialog = ref(false)
const loading = ref(false)
const qrCodeUrl = ref('')
const countdown = ref(300)
const error = ref('')
const orderNo = ref('')
const latestVersion = ref<VersionInfo | null>(null)

// 定时器
let pollTimer: number | null = null
let countdownTimer: number | null = null

// 监听滚动显示回到顶部按钮
const handleScroll = () => {
    showBackTop.value = window.scrollY > 300
}

onMounted(() => {
    window.addEventListener('scroll', handleScroll)
    fetchLatestVersion()
})

onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
    clearAllTimers()
})

// 获取最新版本信息
const fetchLatestVersion = async () => {
    try {
        const result = await appReleaseAPI.checkUpdate(0, 'android')
        latestVersion.value = result
    } catch (error) {
        console.error('获取版本信息失败', error)
    }
}

// 回到顶部
const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

// 用户面板
const showUserPanel = ref(false)

const toggleUserPanel = () => {
    closeDownloadPanel()
    showUserPanel.value = !showUserPanel.value
}

const closeUserPanel = () => {
    showUserPanel.value = false
}

const handleRechargeFromPanel = () => {
    closeUserPanel()
    router.push({
        path: '/payment',
        query: { type: 'deposit' },
    })
}

const goToProfile = () => {
    closeUserPanel()
    ElMessage.info('个人中心功能开发中')
}

// 打开充值弹窗
const openRechargeDialog = () => {
    showRechargeDialog.value = true
}

// 关闭充值弹窗
const closeRechargeDialog = () => {
    showRechargeDialog.value = false
}

// 充值弹窗关闭回调
const onRechargeDialogClosed = () => {
    clearAllTimers()
    resetState()
}

const resetState = () => {
    qrCodeUrl.value = ''
    error.value = ''
    countdown.value = 300
}

// 格式化倒计时
const formatCountdown = (seconds: number) => {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

// 生成支付二维码
const generateQRCode = async () => {
    try {
        loading.value = true
        error.value = ''
        countdown.value = 300

        const response = await payApi.payDepositNative(51)
        qrCodeUrl.value = response.code_url
        orderNo.value = response.outTradeNo || Date.now().toString()

        startCountdown()
        startPolling()
    } catch (err: any) {
        console.error('生成二维码失败:', err)
        error.value = err.message || '生成二维码失败，请重试'
    } finally {
        loading.value = false
    }
}

// 启动倒计时
const startCountdown = () => {
    clearCountdownTimer()
    countdownTimer = window.setInterval(() => {
        countdown.value--
        if (countdown.value <= 0) {
            clearAllTimers()
        }
    }, 1000)
}

// 启动轮询检查支付状态
const startPolling = () => {
    clearPollTimer()
    pollTimer = window.setInterval(async () => {
        await checkPaymentStatus()
    }, 3000)
}

// 检查支付状态
const checkPaymentStatus = async () => {
    try {
        if (orderNo.value) {
            const status = await payApi.getOrderStatus(orderNo.value)
            if (status.status === '已支付') {
                ElMessage.success('支付成功！魔力值已到账')
                clearAllTimers()
                await userStore.getUserInfo()
                closeRechargeDialog()
            }
        }
    } catch (err) {
        console.error('检查支付状态失败:', err)
    }
}

// 清除所有定时器
const clearAllTimers = () => {
    clearPollTimer()
    clearCountdownTimer()
}

const clearPollTimer = () => {
    if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
    }
}

const clearCountdownTimer = () => {
    if (countdownTimer) {
        clearInterval(countdownTimer)
        countdownTimer = null
    }
}

// 下载面板
const toggleDownloadPanel = () => {
    showDownloadPanel.value = !showDownloadPanel.value
}

const closeDownloadPanel = () => {
    showDownloadPanel.value = false
}

const goToDownloadPage = () => {
    closeDownloadPanel()
    router.push('/app-download')
}

// 导航
const goToLogin = () => {
    router.push('/auth/login')
}

const goToAuction = () => {
    closeDownloadPanel()
    router.push('/chat/auction/auction')
}

const goToTrading = () => {
    closeDownloadPanel()
    router.push('/forum/tradingPost')
}

const handleLogout = async () => {
    closeUserPanel()
    await userStore.logout()
    router.push('/auth/login')
}
</script>

<style lang="scss" scoped>
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.top-right-toolbar {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 100;
    display: flex;
    flex-direction: row;
    align-items: flex-start;
    gap: 10px;

    @media (max-width: 1200px) {
        top: 10px;
        right: 10px;
    }
}

.toolbar-container {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
    padding: 16px 12px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 20px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
}

.tool-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 60px;
    height: 70px;
    cursor: pointer;
    border-radius: 12px;
    transition: all 0.3s ease;
    position: relative;
    gap: 6px;

    &:hover {
        background: rgba(131, 58, 0, 0.15);
        transform: scale(1.05);
    }

    &:active {
        transform: scale(0.98);
    }
}

.tool-icon {
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: $primary-color;
    border-radius: 50%;
    border: 2px solid darken($primary-color, 10%);
    transition: all 0.3s ease;

    .el-icon {
        font-size: 22px;
        color: #fff;
    }
}

.tool-label {
    font-size: 12px;
    font-weight: 600;
    color: $primary-color;
}

.download-icon {
    background: #52c41a;
    border-color: darken(#52c41a, 10%);
}

.back-top-icon {
    background: $primary-light;
    border-color: darken($primary-light, 10%);
}

.user-avatar {
    border: 2px solid $border-color;
}

// 用户信息面板
.user-panel {
    position: absolute;
    right: calc(100% + 10px);
    top: 0;
    width: 280px;
    background: #fff;
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 20px rgba(131, 58, 0, 0.2);
    overflow: hidden;

    .panel-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 16px;
        background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
        border-bottom: 2px solid $border-color;

        .user-info-header {
            display: flex;
            align-items: center;
            gap: 12px;

            .user-avatar-large {
                border: 2px solid $border-color;
            }

            .user-meta {
                .user-name {
                    font-size: 16px;
                    font-weight: 600;
                    color: $primary-color;
                    margin: 0 0 4px 0;
                }

                .user-id {
                    font-size: 12px;
                    color: $primary-light;
                }
            }
        }
    }

    .panel-content {
        padding: 16px;
        background: $bg-light;

        .balance-info {
            display: flex;
            flex-direction: column;
            gap: 12px;
            margin-bottom: 20px;
            padding: 16px;
            background: #fff;
            border-radius: 12px;
            border: 1px solid $border-color;

            .balance-item {
                display: flex;
                justify-content: space-between;
                align-items: center;

                .balance-label {
                    font-size: 14px;
                    color: #666;
                }

                .balance-value {
                    font-size: 18px;
                    font-weight: 600;
                    color: $primary-color;
                }

                &.highlight {
                    padding-top: 12px;
                    border-top: 1px dashed #ddd;

                    .balance-value {
                        color: #d02129;
                    }
                }
            }
        }

        .action-buttons {
            display: flex;
            flex-direction: column;
            gap: 10px;

            .action-btn {
                width: 100%;
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 6px;
                padding: 12px 0;
                margin-left: 0 !important;
                background: $primary-color;
                border-color: $primary-color;
                color: #fff;

                &:hover {
                    background: darken($primary-color, 10%);
                    border-color: darken($primary-color, 10%);
                }

                &.logout-btn {
                    background: $primary-light;
                    border-color: $primary-light;

                    &:hover {
                        background: darken($primary-light, 10%);
                        border-color: darken($primary-light, 10%);
                    }
                }

                .el-icon {
                    font-size: 14px;
                }
            }
        }
    }
}

// 下载面板
.download-panel {
    position: absolute;
    right: calc(100% + 10px);
    top: 0;
    width: 280px;
    background: #fff;
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 20px rgba(131, 58, 0, 0.2);
    overflow: hidden;
}

.slide-left-enter-active,
.slide-left-leave-active {
    transition: all 0.3s ease;
}

.slide-left-enter-from,
.slide-left-leave-to {
    opacity: 0;
    transform: translateX(20px);
}

.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
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
            .panel-title {
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

.panel-content {
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

// 充值弹窗样式
.recharge-dialog-content {
    .balance-section {
        text-align: center;
        padding: 20px;
        background: $bg-light;
        border-radius: 12px;
        margin-bottom: 16px;

        .balance-label {
            font-size: 14px;
            color: $primary-light;
            margin-bottom: 8px;
        }

        .balance-amount {
            font-size: 32px;
            font-weight: bold;
            color: $primary-color;
        }
    }

    .amount-section {
        text-align: center;
        padding: 16px;
        background: #f5f5f5;
        border-radius: 12px;
        margin-bottom: 16px;

        .amount-label {
            font-size: 13px;
            color: #666;
            margin-bottom: 4px;
        }

        .amount-value {
            font-size: 24px;
            font-weight: bold;
            color: #52c41a;
        }

        .amount-hint {
            font-size: 12px;
            color: #999;
            margin-top: 4px;
        }
    }

    .qrcode-section {
        text-align: center;
        padding: 16px;
        background: #fff;
        border: 2px dashed $border-color;
        border-radius: 12px;
        margin-bottom: 16px;
        min-height: 200px;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;

        .loading-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 12px;
            color: $primary-light;

            .el-icon {
                font-size: 32px;
                animation: rotating 2s linear infinite;
            }
        }

        .qrcode-display {
            .qrcode-img {
                width: 180px;
                height: 180px;
                border-radius: 8px;
                border: 2px solid $border-color;
            }

            .qrcode-hint {
                margin-top: 12px;
                font-size: 13px;
                color: $primary-light;
            }

            .countdown {
                margin-top: 8px;
                font-size: 12px;
                color: #666;

                &.expired {
                    color: #d02129;
                    font-weight: bold;
                }
            }
        }

        .error-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 12px;
            color: #d02129;

            .el-icon {
                font-size: 32px;
            }
        }

        .generate-btn-wrapper {
            .el-button {
                background: $primary-color;
                border-color: $primary-color;

                &:hover {
                    background: darken($primary-color, 10%);
                    border-color: darken($primary-color, 10%);
                }
            }
        }
    }

    .recharge-tips {
        .tips-title {
            font-size: 14px;
            font-weight: 600;
            color: $primary-color;
            margin-bottom: 8px;
        }

        .tips-list {
            list-style: none;
            padding: 0;
            margin: 0;

            li {
                font-size: 12px;
                color: $primary-light;
                padding: 4px 0;
                padding-left: 16px;
                position: relative;

                &::before {
                    content: '•';
                    position: absolute;
                    left: 6px;
                    color: $primary-color;
                }
            }
        }
    }
}

@keyframes rotating {
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
}

// 响应式
@media (max-width: 768px) {
    .toolbar-container {
        padding: 8px 6px;
        gap: 6px;
    }

    .tool-item {
        width: 36px;
        height: 36px;
    }

    .tool-icon {
        width: 32px;
        height: 32px;

        .el-icon {
            font-size: 16px;
        }
    }

    .download-panel {
        width: 260px;
    }
}
</style>
