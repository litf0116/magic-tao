<template>
    <div class="top-right-toolbar">
        <!-- 主工具栏 -->
        <div class="toolbar-container">
            <!-- 充值 -->
            <div class="tool-item" :class="{ 'has-badge': userStore.isLogin && userStore.user.depositBalance > 0 }" @click="handleRecharge">
                <div class="tool-icon">
                    <el-icon><Wallet /></el-icon>
                </div>
                <span class="tool-label">充值</span>
                <div v-if="userStore.isLogin && userStore.user.depositBalance > 0" class="balance-badge">
                    ¥{{ userStore.user.depositBalance }}
                </div>
            </div>

            <!-- 拍卖 -->
            <div class="tool-item" @click="goToAuction">
                <div class="tool-icon">
                    <el-icon><Trophy /></el-icon>
                </div>
                <span class="tool-label">拍卖</span>
            </div>

            <!-- 交易 -->
            <div class="tool-item" @click="goToTrading">
                <div class="tool-icon">
                    <el-icon><Shop /></el-icon>
                </div>
                <span class="tool-label">交易</span>
            </div>

            <!-- 下载App -->
            <div class="tool-item download-item" @click="toggleDownloadPanel">
                <div class="tool-icon">
                    <el-icon><Download /></el-icon>
                </div>
                <span class="tool-label">下载App</span>
            </div>

            <!-- 用户信息 / 登录 -->
            <div class="tool-item user-item" @click="handleUser">
                <div class="tool-icon">
                    <el-icon><User /></el-icon>
                </div>
                <span class="tool-label">{{ userStore.isLogin ? userStore.user.name : '登录' }}</span>
            </div>

            <!-- 回到顶部 -->
            <div class="tool-item" v-show="showBackTop" @click="scrollToTop">
                <div class="tool-icon">
                    <el-icon><ArrowUp /></el-icon>
                </div>
                <span class="tool-label">顶部</span>
            </div>
        </div>

        <!-- 充值面板（下拉） -->
        <Transition name="slide-down">
            <div v-if="showRechargePanel" class="recharge-panel">
                <div class="panel-header">
                    <h3 class="panel-title">魔力值充值</h3>
                    <button class="close-btn" @click="closeRechargePanel">
                        <el-icon><Close /></el-icon>
                    </button>
                </div>

                <div class="panel-content">
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
                            <div v-if="countdown > 0" class="countdown">⏱️ 有效期剩余 {{ formatCountdown(countdown) }}</div>
                            <div v-else class="countdown expired">二维码已过期，请刷新</div>
                        </div>

                        <div v-else-if="error" class="error-state">
                            <el-icon><CircleClose /></el-icon>
                            <span>{{ error }}</span>
                            <el-button type="primary" size="small" @click="generateQRCode">重新生成</el-button>
                        </div>

                        <div v-else class="generate-btn-wrapper">
                            <el-button type="primary" size="large" @click="generateQRCode" :loading="loading">
                                <el-icon><Wallet /></el-icon>
                                生成支付二维码
                            </el-button>
                        </div>
                    </div>

                    <!-- 充值说明 -->
                    <div class="recharge-tips">
                        <div class="tips-title">💡 充值说明</div>
                        <ul class="tips-list">
                            <li>充值金额：¥51（含手续费¥1）</li>
                            <li>到账金额：¥50 魔力值</li>
                            <li>支持微信扫码支付</li>
                            <li>支付成功后立即到账</li>
                        </ul>
                    </div>
                </div>
            </div>
        </Transition>

        <!-- 下载App面板（下拉） -->
        <Transition name="slide-down">
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
                    <!-- 二维码 -->
                    <div class="qr-wrapper">
                        <img
                            src="https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png!w300"
                            alt="扫码下载"
                            class="qr-code"
                        />
                        <span class="qr-hint">扫码下载</span>
                    </div>

                    <!-- 跳转按钮 -->
                    <el-button type="primary" class="goto-btn" @click="goToDownloadPage">
                        <el-icon><Download /></el-icon>
                        前往下载页面
                    </el-button>
                </div>
            </div>
        </Transition>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Wallet, Trophy, Shop, User, ArrowUp, Close, Loading, CircleClose, Download } from '@element-plus/icons-vue'
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
const showRechargePanel = ref(false)
const showDownloadPanel = ref(false)
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
    // 获取App版本信息
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

// 处理充值
const handleRecharge = () => {
    closeDownloadPanel()
    if (!userStore.isLogin) {
        ElMessage.info('请先登录')
        router.push('/auth/login?redirect=/deposit-payment')
        return
    }
    showRechargePanel.value = !showRechargePanel.value
    if (!showRechargePanel.value) {
        clearAllTimers()
        resetState()
    }
}

const closeRechargePanel = () => {
    showRechargePanel.value = false
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

        // 启动倒计时和轮询
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
                // 刷新用户信息
                await userStore.getUserInfo()
                closeRechargePanel()
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
    closeRechargePanel()
    showDownloadPanel.value = !showDownloadPanel.value
}

const closeDownloadPanel = () => {
    showDownloadPanel.value = false
}

const goToDownloadPage = () => {
    closeDownloadPanel()
    router.push('/app-download')
}

// 去拍卖行
const goToAuction = () => {
    closeRechargePanel()
    closeDownloadPanel()
    router.push('/chat/auction/auction')
}

// 去交易站
const goToTrading = () => {
    closeRechargePanel()
    closeDownloadPanel()
    router.push('/forum/tradingPost')
}

// 处理用户点击
const handleUser = () => {
    closeRechargePanel()
    closeDownloadPanel()
    if (!userStore.isLogin) {
        router.push('/auth/login')
        return
    }
    ElMessage.info('个人中心功能开发中')
}
</script>

<style lang="scss" scoped>
// 网站主色调 - 暖色系复古游戏风格
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
    flex-direction: column;
    align-items: flex-end;
    gap: 12px;

    @media (max-width: 1200px) {
        top: 10px;
        right: 10px;
    }
}

.toolbar-container {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 12px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
}

.tool-item {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 12px;
    cursor: pointer;
    border-radius: 12px;
    transition: all 0.3s ease;
    position: relative;
    white-space: nowrap;

    &:hover {
        background: rgba(131, 58, 0, 0.15);
        transform: translateY(-2px);
    }

    &:active {
        transform: scale(0.95);
    }

    &.has-badge {
        padding-right: 20px;
    }
}

.tool-icon {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: $primary-color;
    border-radius: 50%;
    border: 2px solid darken($primary-color, 10%);
    transition: all 0.3s ease;

    .el-icon {
        font-size: 18px;
        color: #fff;
    }
}

.tool-label {
    font-size: 14px;
    font-weight: 600;
    color: $primary-color;
}

// 余额徽章
.balance-badge {
    position: absolute;
    top: -4px;
    right: 0;
    background: #d02129;
    color: #fff;
    font-size: 10px;
    font-weight: bold;
    padding: 2px 6px;
    border-radius: 10px;
    border: 1px solid #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

// 用户项特殊样式
.user-item {
    .tool-icon {
        background: $primary-light;
        border-color: darken($primary-light, 10%);
    }
}

// 下载项特殊样式
.download-item {
    .tool-icon {
        background: #52c41a;
        border-color: darken(#52c41a, 10%);
    }
}

// 通用面板样式
.recharge-panel,
.download-panel {
    width: 320px;
    background: #fff;
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 20px rgba(131, 58, 0, 0.2);
    overflow: hidden;
}

.slide-down-enter-active,
.slide-down-leave-active {
    transition: all 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
    opacity: 0;
    transform: translateY(-10px);
}

.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border-bottom: 2px solid $border-color;

    .panel-title {
        font-size: 18px;
        font-weight: 600;
        color: $primary-color;
        margin: 0;
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
    padding: 20px;
    max-height: 500px;
    overflow-y: auto;
}

// 余额区域
.balance-section {
    text-align: center;
    padding: 16px;
    background: $bg-light;
    border-radius: 12px;
    margin-bottom: 16px;

    .balance-label {
        font-size: 13px;
        color: $primary-light;
        margin-bottom: 4px;
    }

    .balance-amount {
        font-size: 28px;
        font-weight: bold;
        color: $primary-color;
    }
}

// 金额区域
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

// 二维码区域
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

// 充值说明
.recharge-tips {
    .tips-title {
        font-size: 13px;
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

// 下载面板特殊样式
.download-panel {
    .panel-header {
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
                    margin-bottom: 2px;
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
    }

    .panel-content {
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
}

@keyframes rotating {
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
}

// 响应式：小屏幕隐藏部分元素
@media (max-width: 768px) {
    .toolbar-container {
        padding: 6px 8px;
        gap: 4px;
    }

    .tool-item {
        padding: 6px 8px;
    }

    .tool-label {
        font-size: 12px;
    }

    .tool-icon {
        width: 30px;
        height: 30px;

        .el-icon {
            font-size: 15px;
        }
    }

    .recharge-panel,
    .download-panel {
        width: 280px;
    }
}
</style>