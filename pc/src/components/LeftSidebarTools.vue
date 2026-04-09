<template>
    <div class="left-sidebar-tools">
        <!-- 主工具容器 -->
        <div class="main-wrapper" :class="{ 'expanded': showRechargePanel }">
            <!-- 悬浮功能列 -->
            <div class="tools-container">
                <!-- 魔力值充值 -->
                <div class="tool-item active" @click="toggleRechargePanel">
                    <div class="tool-icon">
                        <el-icon><Wallet /></el-icon>
                    </div>
                    <span class="tool-label">充值</span>
                    <!-- 余额提示 -->
                    <div v-if="userStore.isLogin && userStore.user.depositBalance > 0" class="balance-badge">
                        ¥{{ userStore.user.depositBalance }}
                    </div>
                </div>

                <!-- 拍卖行 -->
                <div class="tool-item" @click="goToAuction">
                    <div class="tool-icon">
                        <el-icon><Trophy /></el-icon>
                    </div>
                    <span class="tool-label">拍卖</span>
                </div>

                <!-- 交易站 -->
                <div class="tool-item" @click="goToTrading">
                    <div class="tool-icon">
                        <el-icon><Shop /></el-icon>
                    </div>
                    <span class="tool-label">交易</span>
                </div>

                <!-- 个人中心 -->
                <div class="tool-item" @click="goToProfile">
                    <div class="tool-icon">
                        <el-icon><User /></el-icon>
                    </div>
                    <span class="tool-label">我的</span>
                </div>

                <!-- 回到顶部 -->
                <div class="tool-item back-top" @click="scrollToTop" v-show="showBackTop">
                    <div class="tool-icon">
                        <el-icon><ArrowUp /></el-icon>
                    </div>
                    <span class="tool-label">顶部</span>
                </div>
            </div>

            <!-- 右侧展开的充值面板 -->
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
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Wallet, Trophy, Shop, User, ArrowUp, Close, Loading, CircleClose } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { payApi } from '@/api/pay'

const router = useRouter()
const userStore = useUserStore()

// 状态管理
const showBackTop = ref(false)
const showRechargePanel = ref(false)
const loading = ref(false)
const qrCodeUrl = ref('')
const countdown = ref(300)
const error = ref('')
const orderNo = ref('')

// 定时器
let pollTimer: number | null = null
let countdownTimer: number | null = null

// 监听滚动显示回到顶部按钮
const handleScroll = () => {
    showBackTop.value = window.scrollY > 300
}

onMounted(() => {
    window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
    clearAllTimers()
})

// 回到顶部
const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

// 打开/关闭充值面板
const toggleRechargePanel = () => {
    if (!userStore.isLogin) {
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

// 去拍卖行
const goToAuction = () => {
    closeRechargePanel()
    router.push('/chat/auction/auction')
}

// 去交易站
const goToTrading = () => {
    closeRechargePanel()
    router.push('/forum/tradingPost')
}

// 去个人中心
const goToProfile = () => {
    closeRechargePanel()
    if (!userStore.isLogin) {
        ElMessage.info('请先登录')
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

.left-sidebar-tools {
    position: fixed;
    left: 20px;
    top: 50%;
    transform: translateY(-50%);
    z-index: 100;

    @media (max-width: 1400px) {
        left: 10px;
    }

    @media (max-width: 1200px) {
        display: none;
    }
}

.main-wrapper {
    display: flex;
    transition: all 0.3s ease;
}

.tools-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 16px 12px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
    z-index: 2;
}

.tool-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 12px 8px;
    cursor: pointer;
    border-radius: 12px;
    transition: all 0.3s ease;
    position: relative;

    &:hover, &.active {
        background: rgba(131, 58, 0, 0.15);
        transform: translateY(-2px);

        .tool-icon {
            transform: scale(1.1);
        }
    }

    &:active {
        transform: scale(0.95);
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
    white-space: nowrap;
}

// 余额徽章
.balance-badge {
    position: absolute;
    top: -4px;
    right: -4px;
    background: #d02129;
    color: #fff;
    font-size: 10px;
    font-weight: bold;
    padding: 2px 6px;
    border-radius: 10px;
    border: 1px solid #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

// 回到顶部按钮
.back-top {
    margin-top: 8px;
    border-top: 2px dashed $border-color;
    padding-top: 16px;

    .tool-icon {
        background: $primary-light;
        border-color: darken($primary-light, 10%);
    }
}

// 充值面板
.recharge-panel {
    width: 320px;
    background: #fff;
    border: 3px solid $border-color;
    border-left: none;
    border-radius: 0 16px 16px 0;
    box-shadow: 4px 4px 20px rgba(131, 58, 0, 0.2);
    animation: slideIn 0.3s ease;
    overflow: hidden;
}

@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
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

@keyframes rotating {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}
</style>